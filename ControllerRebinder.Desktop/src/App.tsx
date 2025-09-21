import { useEffect, useMemo, useRef, useState } from 'react';

import SectionCard from './components/SectionCard';

import ToggleField from './components/ToggleField';

import NumberField from './components/NumberField';

import KeySelect from './components/KeySelect';

import LogConsole from './components/LogConsole';
import JoystickVisualizer from './components/JoystickVisualizer';

import type {

  ButtonsOptions,

  ControllerRemapperOptions,

  JoystickKeyBindings,

  JoystickOptions,

  LogEntry,

  RemapperState

} from './desktop-api';

import './App.css';



type ConfigEnvelope = { ControllerRemapper: ControllerRemapperOptions };

type Banner = { kind: 'info' | 'error' | 'success' | 'warning'; message: string };



type JoystickSide = 'LeftJoystick' | 'RightJoystick';



const MAX_LOGS = 200;

const DEFAULT_STATE: RemapperState = { status: 'stopped', detail: 'Service not running.' };



const deepClone = <T,>(value: T): T => JSON.parse(JSON.stringify(value));



const configsEqual = (

  a?: ControllerRemapperOptions | null,

  b?: ControllerRemapperOptions | null

): boolean => {

  if (!a && !b) {

    return true;

  }

  if (!a || !b) {

    return false;

  }



  return JSON.stringify(a) === JSON.stringify(b);

};



const App = () => {

  const [configEnvelope, setConfigEnvelope] = useState<ConfigEnvelope | null>(null);

  const [draft, setDraft] = useState<ControllerRemapperOptions | null>(null);

  const [remapperState, setRemapperState] = useState<RemapperState>(DEFAULT_STATE);

  const [logs, setLogs] = useState<LogEntry[]>([]);

  const [loading, setLoading] = useState(true);

  const [isSaving, setIsSaving] = useState(false);

  const [starting, setStarting] = useState(false);

  const [stopping, setStopping] = useState(false);

  const [banner, setBanner] = useState<Banner | null>(null);

  const [diskOutOfSync, setDiskOutOfSync] = useState(false);

  const [lastError, setLastError] = useState<string | null>(null);



  const draftRef = useRef<ControllerRemapperOptions | null>(draft);

  const envelopeRef = useRef<ConfigEnvelope | null>(configEnvelope);



  useEffect(() => {

    draftRef.current = draft;

  }, [draft]);



  useEffect(() => {

    envelopeRef.current = configEnvelope;

  }, [configEnvelope]);



  useEffect(() => {

    if (!banner) {

      return;

    }



    const timeout = window.setTimeout(() => setBanner(null), 5000);

    return () => window.clearTimeout(timeout);

  }, [banner]);



  const handleIncomingConfiguration = (incoming: ConfigEnvelope) => {

    const nextEnvelope = deepClone(incoming);

    const currentDraft = draftRef.current;



    setConfigEnvelope(nextEnvelope);



    if (!currentDraft) {

      setDraft(deepClone(nextEnvelope.ControllerRemapper));

      setDiskOutOfSync(false);

      setLastError(null);

      return;

    }



    const identical = configsEqual(currentDraft, nextEnvelope.ControllerRemapper);

    if (identical) {

      setDraft(deepClone(nextEnvelope.ControllerRemapper));

      setDiskOutOfSync(false);

    } else {

      setDiskOutOfSync(true);

      setBanner({

        kind: 'warning',

        message: 'Configuration file changed on disk. Reload to synchronise your edits.'

      });

    }

  };



  useEffect(() => {

    let isMounted = true;



    const bootstrap = async () => {

      try {

        const [configResult, statusResult, logsResult] = await Promise.all([

          window.desktopApi.loadConfiguration(),

          window.desktopApi.getStatus(),

          window.desktopApi.getLogs()

        ]);



        if (!isMounted) {

          return;

        }



        if (configResult.ok && configResult.payload) {

          handleIncomingConfiguration(configResult.payload);

        } else {

          const message = configResult.error ?? 'Failed to load configuration file.';

          setLastError(message);

          setBanner({ kind: 'error', message });

        }



        if (statusResult.ok && statusResult.state) {

          setRemapperState(statusResult.state);

        }



        if (logsResult.ok && logsResult.logs) {

          setLogs(logsResult.logs.slice(-MAX_LOGS));

        }

      } catch (error) {

        const message = error instanceof Error ? error.message : 'Unexpected initialisation error.';

        setLastError(message);

        setBanner({ kind: 'error', message });

      } finally {

        if (isMounted) {

          setLoading(false);

        }

      }

    };



    bootstrap();



    const unsubscribeStatus = window.desktopApi.onStatusChanged((state) => {

      if (isMounted) {

        setRemapperState(state);

      }

    });



    const unsubscribeConfig = window.desktopApi.onConfigUpdated((payload) => {

      if (isMounted && payload) {

        handleIncomingConfiguration(payload);

      }

    });



    const unsubscribeLog = window.desktopApi.onLogEntry((entry) => {

      if (!isMounted) {

        return;

      }



      setLogs((previous) => {

        const next = [...previous, entry];

        if (next.length > MAX_LOGS) {

          next.splice(0, next.length - MAX_LOGS);

        }

        return next;

      });

    });



    return () => {

      isMounted = false;

      unsubscribeStatus();

      unsubscribeConfig();

      unsubscribeLog();

    };

  }, []);



  const hasUnsavedChanges = useMemo(() => {

    if (!configEnvelope || !draft) {

      return false;

    }



    return !configsEqual(configEnvelope.ControllerRemapper, draft);

  }, [configEnvelope, draft]);



  const handleReloadFromDisk = async () => {

    setLoading(true);

    let success = false;

    try {

      const result = await window.desktopApi.loadConfiguration();

      if (result.ok && result.payload) {

        handleIncomingConfiguration(result.payload);

        setBanner({ kind: 'info', message: 'Configuration reloaded from disk.' });

        success = true;

      } else {

        const message = result.error ?? 'Unable to reload configuration file.';

        setBanner({ kind: 'error', message });

      }

    } finally {

      setLoading(false);

      if (success) {

        setDiskOutOfSync(false);

      }

    }

  };



  const handleSave = async () => {

    if (!draft) {

      return;

    }



    setIsSaving(true);

    try {

      const payload: ConfigEnvelope = { ControllerRemapper: deepClone(draft) };

      const result = await window.desktopApi.saveConfiguration(payload);

      if (!result.ok) {

        const message = result.error ?? 'Failed to save configuration.';

        setBanner({ kind: 'error', message });

        return;

      }



      setConfigEnvelope(deepClone(payload));

      setDraft(deepClone(payload.ControllerRemapper));

      setDiskOutOfSync(false);

      setLastError(null);

      setBanner({ kind: 'success', message: 'Configuration saved successfully.' });

    } finally {

      setIsSaving(false);

    }

  };



  const handleStart = async () => {

    setStarting(true);

    try {

      const result = await window.desktopApi.startRemapper();

      if (!result.ok) {

        const message = result.error ?? 'Failed to start controller remapper.';

        setBanner({ kind: 'error', message });

        return;

      }

      if (result.state) {

        setRemapperState(result.state);

      }

    } finally {

      setStarting(false);

    }

  };



  const handleStop = async () => {

    setStopping(true);

    try {

      const result = await window.desktopApi.stopRemapper();

      if (!result.ok) {

        const message = result.error ?? 'Failed to stop controller remapper.';

        setBanner({ kind: 'error', message });

        return;

      }

      if (result.state) {

        setRemapperState(result.state);

      }

    } finally {

      setStopping(false);

    }

  };



  const handleRevealFolder = async () => {

    const result = await window.desktopApi.revealConfigFolder();

    if (!result.ok) {

      const message = result.error ?? 'Unable to open configuration folder.';

      setBanner({ kind: 'error', message });

    }

  };



  const updateDraft = (updater: (previous: ControllerRemapperOptions) => ControllerRemapperOptions) => {

    setDraft((previous) => {

      if (!previous) {

        return previous;

      }



      return updater(previous);

    });

  };



  const updateJoystick = (side: JoystickSide, updater: (joystick: JoystickOptions) => JoystickOptions) => {

    updateDraft((previous) => ({

      ...previous,

      [side]: updater(previous[side])

    }));

  };



  const updateJoystickKeys = (

    side: JoystickSide,

    updater: (keys: JoystickKeyBindings) => JoystickKeyBindings

  ) => {

    updateJoystick(side, (joystick) => ({

      ...joystick,

      Keys: updater(joystick.Keys)

    }));

  };



  const updateButtons = (updater: (buttons: ButtonsOptions) => ButtonsOptions) => {

    updateDraft((previous) => ({

      ...previous,

      Buttons: updater(previous.Buttons)

    }));

  };



  const updateButtonKey = (key: keyof ButtonsOptions['Keys'], value: string | null) => {

    updateButtons((buttons) => ({

      ...buttons,

      Keys: {

        ...buttons.Keys,

        [key]: value

      }

    }));

  };



  const renderJoystickPanel = (side: JoystickSide, title: string, subtitle: string) => {

    if (!draft) {

      return null;

    }



    const joystick = draft[side];



    return (

      <SectionCard

        key={side}

        title={title}

        subtitle={subtitle}

        actions={

          <span className={`status-icon status-icon--${joystick.Enabled ? 'active' : 'inactive'}`}>

            {joystick.Enabled ? 'Enabled' : 'Disabled'}

          </span>

        }

      >

        <div className="joystick-panel">

          <div className="joystick-panel__visual">

            <JoystickVisualizer

              title={title}

              deadZone={joystick.DeadZone}

              threshold={joystick.Threshold}

              maxValue={joystick.MaxValue}

              forwardDown={joystick.ForwardDown}

              leftRight={joystick.LeftRight}

              keys={joystick.Keys}

            />

          </div>

          <div className="joystick-panel__form">

            <div className="panel__grid panel__grid--two">

              <ToggleField

                label="Enable remapping"

                checked={joystick.Enabled}

                onChange={(value) => updateJoystick(side, (current) => ({ ...current, Enabled: value }))}

                description="Toggle joystick event to keyboard translation for this stick."

              />

              <NumberField

                label="Dead zone"

                value={joystick.DeadZone}

                onChange={(value) =>

                  updateJoystick(side, (current) => ({

                    ...current,

                    DeadZone: Math.max(0, Math.floor(value))

                  }))

                }

                description="Ignore minor thumbstick noise within this radius."

                step={250}

                min={0}

              />

              <NumberField

                label="Forward threshold"

                value={joystick.ForwardDown}

                onChange={(value) =>

                  updateJoystick(side, (current) => ({

                    ...current,

                    ForwardDown: Math.max(0, value)

                  }))

                }

                description="Percentage of motion favouring vertical press."

                step={0.5}

                min={0}

              />

              <NumberField

                label="Horizontal threshold"

                value={joystick.LeftRight}

                onChange={(value) =>

                  updateJoystick(side, (current) => ({

                    ...current,

                    LeftRight: Math.max(0, value)

                  }))

                }

                description="Percentage of motion favouring horizontal press."

                step={0.5}

                min={0}

              />

              <NumberField

                label="Actuation threshold"

                value={joystick.Threshold}

                onChange={(value) =>

                  updateJoystick(side, (current) => ({

                    ...current,

                    Threshold: Math.max(0, Math.floor(value))

                  }))

                }

                description="Joystick area required before any key is triggered."

                step={250}

                min={0}

              />

              <NumberField

                label="Maximum value"

                value={joystick.MaxValue}

                onChange={(value) =>

                  updateJoystick(side, (current) => ({

                    ...current,

                    MaxValue: Math.max(0, Math.floor(value))

                  }))

                }

                description="Hardware-reported maximum value. Adjust only for unusual controllers."

                step={1000}

                min={0}

              />

            </div>

            <div className="panel__grid panel__grid--four">

              <KeySelect

                label="Up"

                value={joystick.Keys.Up}

                allowEmpty={false}

                onChange={(value) =>

                  updateJoystickKeys(side, (keys) => ({ ...keys, Up: value ?? keys.Up }))

                }

              />

              <KeySelect

                label="Down"

                value={joystick.Keys.Down}

                allowEmpty={false}

                onChange={(value) =>

                  updateJoystickKeys(side, (keys) => ({ ...keys, Down: value ?? keys.Down }))

                }

              />

              <KeySelect

                label="Left"

                value={joystick.Keys.Left}

                allowEmpty={false}

                onChange={(value) =>

                  updateJoystickKeys(side, (keys) => ({ ...keys, Left: value ?? keys.Left }))

                }

              />

              <KeySelect

                label="Right"

                value={joystick.Keys.Right}

                allowEmpty={false}

                onChange={(value) =>

                  updateJoystickKeys(side, (keys) => ({ ...keys, Right: value ?? keys.Right }))

                }

              />

            </div>

          </div>

        </div>

      </SectionCard>

    );

  };



  const statusClass = `status-pill status-pill--${remapperState.status}`;

  const statusText =

    remapperState.status === 'running'

      ? 'Service running'

      : remapperState.status === 'starting'

      ? 'Starting…'

      : remapperState.status === 'error'

      ? 'Error'

      : 'Stopped';



  return (

    <div className="app-shell">

      <header className="topbar">

        <div className="topbar__status">

          <span className={statusClass}>{statusText}</span>

          <span className="topbar__detail">{remapperState.detail}</span>

          {hasUnsavedChanges ? <span className="topbar__badge">Unsaved changes</span> : null}

          {diskOutOfSync ? <span className="topbar__badge topbar__badge--warn">Disk updated</span> : null}

        </div>

        <div className="topbar__actions">

          <button

            type="button"

            className="ghost-button"

            onClick={handleRevealFolder}

            disabled={loading}

          >

            Open config folder

          </button>

          <button

            type="button"

            className="ghost-button"

            onClick={handleReloadFromDisk}

            disabled={loading}

          >

            Reload from disk

          </button>

          <button

            type="button"

            className="primary-button"

            onClick={handleSave}

            disabled={!hasUnsavedChanges || isSaving || !draft}

          >

            {isSaving ? 'Saving…' : 'Save configuration'}

          </button>

          {remapperState.status === 'running' ? (

            <button

              type="button"

              className="stop-button"

              onClick={handleStop}

              disabled={stopping}

            >

              {stopping ? 'Stopping…' : 'Stop remapper'}

            </button>

          ) : (

            <button

              type="button"

              className="start-button"

              onClick={handleStart}

              disabled={starting || !!lastError}

            >

              {starting ? 'Launching…' : 'Start remapper'}

            </button>

          )}

        </div>

      </header>



      {banner ? (

        <div className={`banner banner--${banner.kind}`}>

          <span>{banner.message}</span>

        </div>

      ) : null}



      {loading ? (

        <main className="loading-state">

          <div className="spinner" />

          <p>Loading controller remapper workspace…</p>

        </main>

      ) : draft ? (

        <main className="layout">

          <section className="main-column">

            <SectionCard

              title="Controller"

              subtitle="Adjust global behaviour for the remapper service."

              actions={

                <span className="status-icon status-icon--secondary">

                  Controller #{draft.ControllerIndex + 1}

                </span>

              }

            >

              <div className="panel__grid panel__grid--three">

                <NumberField

                  label="Controller index"

                  value={draft.ControllerIndex}

                  onChange={(value) =>

                    updateDraft((previous) => ({

                      ...previous,

                      ControllerIndex: Math.max(0, Math.min(3, Math.floor(value)))

                    }))

                  }

                  description="Select which controller (0-3) the remapper should observe."

                  min={0}

                  max={3}

                />

                <NumberField

                  label="Refresh rate (ms)"

                  value={draft.RefreshRate}

                  onChange={(value) =>

                    updateDraft((previous) => ({

                      ...previous,

                      RefreshRate: Math.max(1, Math.floor(value))

                    }))

                  }

                  description="Polling interval for reading controller state."

                  min={1}

                />

                <ToggleField

                  label="Verbose logging"

                  checked={draft.Log}

                  onChange={(value) =>

                    updateDraft((previous) => ({

                      ...previous,

                      Log: value

                    }))

                  }

                  description="Write joystick telemetry to the service logs."

                />

              </div>

            </SectionCard>



            <div className="panel-stack">

              {renderJoystickPanel('LeftJoystick', 'Left joystick', 'Map WASD-like movement controls.')}

              {renderJoystickPanel('RightJoystick', 'Right joystick', 'Configure camera / look bindings.')}

            </div>



            <SectionCard

              title="Face buttons"

              subtitle="Assign keyboard inputs to the controller buttons and D-pad."

              actions={

                <span className={`status-icon status-icon--${draft.Buttons.Enabled ? 'active' : 'inactive'}`}>

                  {draft.Buttons.Enabled ? 'Enabled' : 'Disabled'}

                </span>

              }

            >

              <ToggleField

                label="Enable button remapping"

                checked={draft.Buttons.Enabled}

                onChange={(value) =>

                  updateButtons((buttons) => ({

                    ...buttons,

                    Enabled: value

                  }))

                }

                description="Activate keyboard bindings for the controller buttons."

              />

              <div className="panel__grid panel__grid--four">

                <KeySelect

                  label="A"

                  value={draft.Buttons.Keys.A ?? null}

                  onChange={(value) => updateButtonKey('A', value)}

                />

                <KeySelect

                  label="B"

                  value={draft.Buttons.Keys.B ?? null}

                  onChange={(value) => updateButtonKey('B', value)}

                />

                <KeySelect

                  label="X"

                  value={draft.Buttons.Keys.X ?? null}

                  onChange={(value) => updateButtonKey('X', value)}

                />

                <KeySelect

                  label="Y"

                  value={draft.Buttons.Keys.Y ?? null}

                  onChange={(value) => updateButtonKey('Y', value)}

                />

                <KeySelect

                  label="D-Pad Up"

                  value={draft.Buttons.Keys.DPadUp ?? null}

                  onChange={(value) => updateButtonKey('DPadUp', value)}

                />

                <KeySelect

                  label="D-Pad Down"

                  value={draft.Buttons.Keys.DPadDown ?? null}

                  onChange={(value) => updateButtonKey('DPadDown', value)}

                />

                <KeySelect

                  label="D-Pad Left"

                  value={draft.Buttons.Keys.DPadLeft ?? null}

                  onChange={(value) => updateButtonKey('DPadLeft', value)}

                />

                <KeySelect

                  label="D-Pad Right"

                  value={draft.Buttons.Keys.DPadRight ?? null}

                  onChange={(value) => updateButtonKey('DPadRight', value)}

                />

                <KeySelect

                  label="Left bumper"

                  value={draft.Buttons.Keys.LeftShoulder ?? null}

                  onChange={(value) => updateButtonKey('LeftShoulder', value)}

                />

                <KeySelect

                  label="Right bumper"

                  value={draft.Buttons.Keys.RightShoulder ?? null}

                  onChange={(value) => updateButtonKey('RightShoulder', value)}

                />

                <KeySelect

                  label="Start"

                  value={draft.Buttons.Keys.Start ?? null}

                  onChange={(value) => updateButtonKey('Start', value)}

                />

                <KeySelect

                  label="Back"

                  value={draft.Buttons.Keys.Back ?? null}

                  onChange={(value) => updateButtonKey('Back', value)}

                />

              </div>

            </SectionCard>

          </section>



          <aside className="sidebar">

            <LogConsole entries={logs} onClear={() => setLogs([])} />

          </aside>

        </main>

      ) : (

        <main className="loading-state">

          <div className="placeholder">

            <h2>Configuration not available</h2>

            <p>{lastError ?? 'The configuration could not be loaded.'}</p>

            <div className="topbar__actions">

              <button type="button" className="primary-button" onClick={handleReloadFromDisk}>

                Retry

              </button>

            </div>

          </div>

        </main>

      )}

    </div>

  );

};



export default App;


export interface JoystickKeyBindings {
  Up: string;
  Down: string;
  Left: string;
  Right: string;
}

export interface JoystickOptions {
  Enabled: boolean;
  ForwardDown: number;
  LeftRight: number;
  DeadZone: number;
  MaxValue: number;
  Threshold: number;
  Keys: JoystickKeyBindings;
}

export interface ButtonKeyBindings {
  A?: string | null;
  B?: string | null;
  X?: string | null;
  Y?: string | null;
  DPadUp?: string | null;
  DPadDown?: string | null;
  DPadLeft?: string | null;
  DPadRight?: string | null;
  LeftShoulder?: string | null;
  RightShoulder?: string | null;
  Start?: string | null;
  Back?: string | null;
}

export interface ButtonsOptions {
  Enabled: boolean;
  Keys: ButtonKeyBindings;
}

export interface ControllerRemapperOptions {
  RefreshRate: number;
  Log: boolean;
  ControllerIndex: number;
  LeftJoystick: JoystickOptions;
  RightJoystick: JoystickOptions;
  Buttons: ButtonsOptions;
}

export type RemapperState =
  | { status: 'stopped'; detail: string }
  | { status: 'starting'; detail: string }
  | { status: 'running'; detail: string }
  | { status: 'error'; detail: string };

export interface LogEntry {
  timestamp: string;
  level: 'info' | 'error' | 'warn' | string;
  message: string;
}

export interface DesktopApi {
  loadConfiguration(): Promise<{ ok: boolean; payload?: { ControllerRemapper: ControllerRemapperOptions }; error?: string }>;
  saveConfiguration(config: { ControllerRemapper: ControllerRemapperOptions }): Promise<{ ok: boolean; error?: string }>;
  startRemapper(): Promise<{ ok: boolean; state?: RemapperState; error?: string }>;
  stopRemapper(): Promise<{ ok: boolean; state?: RemapperState; error?: string }>;
  getStatus(): Promise<{ ok: boolean; state: RemapperState }>;
  getLogs(): Promise<{ ok: boolean; logs: LogEntry[] }>;
  revealConfigFolder(): Promise<{ ok: boolean; error?: string }>;
  onStatusChanged(listener: (state: RemapperState) => void): () => void;
  onConfigUpdated(listener: (config: { ControllerRemapper: ControllerRemapperOptions }) => void): () => void;
  onLogEntry(listener: (entry: LogEntry) => void): () => void;
}

declare global {
  interface Window {
    desktopApi: DesktopApi;
  }
}

export {};

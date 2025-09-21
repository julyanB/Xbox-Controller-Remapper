const { app, BrowserWindow, ipcMain, dialog, shell } = require('electron');
const path = require('path');
const fs = require('fs');
const fsPromises = fs.promises;
const { spawn } = require('child_process');

let mainWindow;
let remapperProcess = null;
let remapperState = {
  status: 'stopped',
  detail: 'Service not running.'
};

const logBuffer = [];
const MAX_LOG_LINES = 500;

const repoRoot = path.resolve(__dirname, '..', '..');
const projectPath = path.resolve(repoRoot, 'ControllerRebinder', 'ControllerRebinder.TesterConsole.csproj');
const projectWorkingDirectory = path.dirname(projectPath);
const configPath = path.resolve(projectWorkingDirectory, 'Configurations.json');

let configWatcher;

function pushLog(level, message) {
  const timestamp = new Date().toISOString();
  const entry = { timestamp, level, message };
  logBuffer.push(entry);
  if (logBuffer.length > MAX_LOG_LINES) {
    logBuffer.splice(0, logBuffer.length - MAX_LOG_LINES);
  }

  if (mainWindow && !mainWindow.isDestroyed()) {
    mainWindow.webContents.send('remapper:log', entry);
  }
}

async function ensureConfigFile() {
  try {
    await fsPromises.access(configPath, fs.constants.F_OK);
  } catch {
    pushLog('error', `Configuration file not found at ${configPath}.`);
    throw new Error('Configuration file not found.');
  }
}

async function loadConfiguration() {
  await ensureConfigFile();
  const raw = await fsPromises.readFile(configPath, 'utf-8');
  const sanitized = raw.replace(/^\uFEFF/, '');
  return JSON.parse(sanitized);
}

async function saveConfiguration(payload) {
  await ensureConfigFile();
  const serialized = JSON.stringify(payload, null, 2);
  await fsPromises.writeFile(configPath, serialized, 'utf-8');
  pushLog('info', 'Configuration saved successfully.');
  if (mainWindow && !mainWindow.isDestroyed()) {
    mainWindow.webContents.send('remapper:config-updated', payload);
  }
}

function setRemapperState(next) {
  remapperState = next;
  if (mainWindow && !mainWindow.isDestroyed()) {
    mainWindow.webContents.send('remapper:status', remapperState);
  }
}

function attachProcessListeners(child) {
  child.once('spawn', () => {
    setRemapperState({ status: 'running', detail: 'Controller remapper is running.' });
  });
  child.stdout.setEncoding('utf-8');
  child.stdout.on('data', (data) => {
    data
      .split(/\r?\n/)
      .filter(Boolean)
      .forEach((line) => pushLog('info', line));
  });

  child.stderr.setEncoding('utf-8');
  child.stderr.on('data', (data) => {
    data
      .split(/\r?\n/)
      .filter(Boolean)
      .forEach((line) => pushLog('error', line));
  });

  child.on('close', (code, signal) => {
    const detail = code === 0
      ? 'Service stopped.'
      : `Service exited with code ${code ?? 'unknown'} (signal: ${signal ?? 'none'}).`;
    setRemapperState({ status: 'stopped', detail });
    remapperProcess = null;
  });

  child.on('error', (error) => {
    pushLog('error', `Failed to start remapper: ${error.message}`);
    setRemapperState({ status: 'error', detail: error.message });
    remapperProcess = null;
  });
}

function startRemapperProcess() {
  if (remapperProcess) {
    setRemapperState({ status: 'running', detail: 'Controller remapper already running.' });
    return remapperState;
  }

  pushLog('info', 'Starting controller remapper process...');

  const child = spawn('dotnet', ['run', '--project', projectPath], {
    cwd: projectWorkingDirectory,
    env: {
      ...process.env,
      DOTNET_ENVIRONMENT: process.env.DOTNET_ENVIRONMENT ?? 'Development'
    },
    stdio: ['ignore', 'pipe', 'pipe']
  });

  remapperProcess = child;
  setRemapperState({ status: 'starting', detail: 'Launching controller remapper...' });
  attachProcessListeners(child);
  return remapperState;
}

async function stopRemapperProcess() {
  if (!remapperProcess) {
    setRemapperState({ status: 'stopped', detail: 'Controller remapper is not running.' });
    return remapperState;
  }

  pushLog('info', 'Stopping controller remapper process...');

  return new Promise((resolve) => {
    const timeout = setTimeout(() => {
      if (remapperProcess) {
        remapperProcess.kill('SIGKILL');
      }
      resolve({ status: 'stopped', detail: 'Process terminated forcefully.' });
    }, 5000);

    remapperProcess.once('close', () => {
      clearTimeout(timeout);
      resolve({ status: 'stopped', detail: 'Process stopped.' });
    });

    remapperProcess.kill('SIGINT');
  });
}

function watchConfiguration() {
  if (configWatcher) {
    return;
  }

  try {
    configWatcher = fs.watch(configPath, { persistent: false }, async (eventType) => {
      if (eventType !== 'change') {
        return;
      }

      try {
        const payload = await loadConfiguration();
        if (mainWindow && !mainWindow.isDestroyed()) {
          mainWindow.webContents.send('remapper:config-updated', payload);
        }
      } catch (error) {
        pushLog('error', `Failed to reload configuration: ${error.message}`);
      }
    });
  } catch (error) {
    pushLog('error', `Unable to watch configuration file: ${error.message}`);
  }
}

async function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1280,
    height: 800,
    minWidth: 1100,
    minHeight: 720,
    backgroundColor: '#0f172a',
    title: 'Controller Rebinder',
    webPreferences: {
      preload: path.resolve(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false,
      sandbox: false
    }
  });

  const devServerUrl = process.env.VITE_DEV_SERVER_URL;
  if (devServerUrl) {
    await mainWindow.loadURL(devServerUrl);
    mainWindow.webContents.openDevTools({ mode: 'detach' });
  } else {
    const indexHtml = path.resolve(__dirname, '..', 'dist', 'index.html');
    await mainWindow.loadFile(indexHtml);
  }

  watchConfiguration();
  setRemapperState(remapperState);

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

app.on('ready', () => {
  createWindow().catch((error) => {
    dialog.showErrorBox('Controller Rebinder', `Failed to create window: ${error.message}`);
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow().catch((error) => {
      dialog.showErrorBox('Controller Rebinder', `Failed to recreate window: ${error.message}`);
    });
  }
});

app.on('before-quit', () => {
  if (configWatcher) {
    configWatcher.close();
  }
  if (remapperProcess) {
    remapperProcess.kill('SIGINT');
  }
});

ipcMain.handle('remapper:load-config', async () => {
  try {
    const payload = await loadConfiguration();
    return { ok: true, payload };
  } catch (error) {
    pushLog('error', error.message);
    return { ok: false, error: error.message };
  }
});

ipcMain.handle('remapper:save-config', async (_event, payload) => {
  try {
    await saveConfiguration(payload);
    return { ok: true };
  } catch (error) {
    pushLog('error', error.message);
    return { ok: false, error: error.message };
  }
});

ipcMain.handle('remapper:start', () => {
  try {
    const state = startRemapperProcess();
    return { ok: true, state };
  } catch (error) {
    pushLog('error', error.message);
    setRemapperState({ status: 'error', detail: error.message });
    return { ok: false, error: error.message };
  }
});

ipcMain.handle('remapper:stop', async () => {
  try {
    const state = await stopRemapperProcess();
    setRemapperState(state);
    return { ok: true, state };
  } catch (error) {
    pushLog('error', error.message);
    return { ok: false, error: error.message };
  }
});

ipcMain.handle('remapper:get-status', () => ({ ok: true, state: remapperState }));

ipcMain.handle('remapper:get-logs', () => ({ ok: true, logs: logBuffer.slice(-200) }));

ipcMain.handle('system:reveal-config', async () => {
  try {
    await ensureConfigFile();
    const folder = path.dirname(configPath);
    await shell.openPath(folder);
    return { ok: true };
  } catch (error) {
    pushLog('error', error.message);
    return { ok: false, error: error.message };
  }
});





const { contextBridge, ipcRenderer } = require('electron');

function subscribe(channel, listener) {
  const handler = (_event, payload) => listener(payload);
  ipcRenderer.on(channel, handler);
  return () => {
    ipcRenderer.removeListener(channel, handler);
  };
}

contextBridge.exposeInMainWorld('desktopApi', {
  loadConfiguration: () => ipcRenderer.invoke('remapper:load-config'),
  saveConfiguration: (config) => ipcRenderer.invoke('remapper:save-config', config),
  startRemapper: () => ipcRenderer.invoke('remapper:start'),
  stopRemapper: () => ipcRenderer.invoke('remapper:stop'),
  getStatus: () => ipcRenderer.invoke('remapper:get-status'),
  getLogs: () => ipcRenderer.invoke('remapper:get-logs'),
  revealConfigFolder: () => ipcRenderer.invoke('system:reveal-config'),
  onStatusChanged: (listener) => subscribe('remapper:status', listener),
  onConfigUpdated: (listener) => subscribe('remapper:config-updated', listener),
  onLogEntry: (listener) => subscribe('remapper:log', listener)
});

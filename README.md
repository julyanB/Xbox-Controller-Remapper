# Xbox Controller Remapper

The application is a generic host that continuously polls an Xbox controller and translates stick positions and button presses into keyboard events.

## Prerequisites

- Windows with XInput drivers
- .NET 9 SDK
- Xbox controller (wired or wireless)

## Quick Start

1. Restore dependencies and build once:
   `sh
dotnet restore
dotnet build
`
2. Plug in / pair your controller.
3. Run the remapper:
   `sh
dotnet run --project ControllerRebinder
`
4. Adjust bindings in ControllerRebinder/Configurations.json. Changes are picked up without restarting (except for ControllerIndex).

Press Ctrl+C to stop the host.

## Configuration

All settings live under the ControllerRemapper section.

`json
{
  "ControllerRemapper": {
    "RefreshRate": 8,
    "Log": true,
    "ControllerIndex": 0,
    "LeftJoystick": {
      "Enabled": true,
      "ForwardDown": 23.9,
      "LeftRight": 14.7,
      "DeadZone": 21815,
      "MaxValue": 32767,
      "Threshold": 21815,
      "Keys": {
        "Up": "VK_W",
        "Down": "VK_S",
        "Left": "VK_A",
        "Right": "VK_D"
      }
    },
    "RightJoystick": {
      "Enabled": false,
      "ForwardDown": 23.9,
      "LeftRight": 14.7,
      "DeadZone": 21815,
      "MaxValue": 32767,
      "Threshold": 21815,
      "Keys": {
        "Up": "VK_I",
        "Down": "VK_K",
        "Left": "VK_J",
        "Right": "VK_L"
      }
    },
    "Buttons": {
      "Enabled": false,
      "Keys": {
        "A": "SPACE",
        "B": "VK_Q"
      }
    }
  }
}
`

### Field Reference

- RefreshRate: Polling interval in milliseconds (>=1).
- Log: When rue, dumps joystick telemetry to the console.
- ControllerIndex: 0‑3. Update requires an app restart.
- \*Joystick.ForwardDown / LeftRight: Area thresholds that control when diagonals become vertical or horizontal. Larger numbers tighten the zone.
- DeadZone: Magnitude threshold before any key is pressed.
- Threshold: Radius used for the area calculation (defaults to the Xbox thumbstick maximum of 21815).
- Keys: Use VirtualKeyCode names (VK_W, SPACE, etc.).
- Buttons.Keys: Map any subset of controller buttons; unmapped buttons are ignored.

Validation runs at startup. Invalid bindings or malformed JSON stop the host with a helpful error.

## Testing

Execute the unit suite anytime:
`sh
dotnet test
`
The tests cover joystick quadrant handling, button translation, and keyboard state transitions.

## Troubleshooting

- **No key events** → Verify the controller is reported as connected in logs and check the configured dead zone.
- **Configuration reload errors** → Confirm the JSON is valid and that key names map to VirtualKeyCode values.
- **Controller not found** → Adjust ControllerIndex (0 = first connected pad) and restart the app.

## Desktop Control Center

The repository now ships with a desktop front end located in `ControllerRebinder.Desktop`. The Electron + React UI mirrors the JSON configuration, exposes live telemetry, and lets you launch or stop the remapper without touching the console.

### Setup

1. Install Node.js 18+ and npm.
2. From `ControllerRebinder.Desktop`, install dependencies:
   ```
   npm install
   ```
3. (Optional) Build the .NET host once so runtime assets exist:
   ```
   dotnet build
   ```
4. Run the desktop workspace in development mode (Vite dev server + Electron shell):
   ```
   npm run dev
   ```
5. For a packaged build, emit static assets and launch Electron against them:
   ```
   npm run build
   npm run start
   ```

### Highlights

- Rich forms for controller index, refresh cadence, joystick thresholds, and button bindings with validation helpers.
- Start/stop controls for the remapper host plus live status badges and telemetry streaming.
- Automatic detection of external edits to `Configurations.json` with a prompt to reload the draft.
- Quick links to open the configuration folder for manual inspection.

The UI writes directly to `ControllerRebinder/Configurations.json`, so any changes remain compatible with the existing headless workflow.

## Project Structure

- ControllerRebinder.Core – remapping engine, options, services, and adapters.
- ControllerRebinder – lightweight host wiring configuration and DI.
- ControllerRebinder.Core.Tests – xUnit regression suite for the processing pipeline.

# Xbox Controller Remapper - Usage Instructions

## Installation
### Prerequisites
- .NET 9
- Node.js and npm
- ElectronNET.CLI (`dotnet tool install -g ElectronNET.CLI`)
- Xbox controller (wired or wireless)

### Setup Instructions
1. Clone the repository:
   ```sh
   git clone https://github.com/julyan97/Xbox-Controller-Remapper.git
   ```
2. Navigate to the project directory:
   ```sh
   cd Xbox-Controller-Remapper
   ```
3. Restore dependencies:
   ```sh
   dotnet restore
   ```
4. Build the project:
   ```sh
   dotnet build
   ```
5. Run the application:
   ```sh
   dotnet run --project ControllerRebinder
   ```

### Electron UI
1. Restore dependencies:
   ```sh
   dotnet restore
   ```
2. Build the Electron front end:
   ```sh
   dotnet build ControllerRebinder.Ui/ControllerRebinder.Ui.csproj
   ```
3. Run the Electron app:
   ```sh
   electronize start -p ControllerRebinder.Ui/ControllerRebinder.Ui.csproj
   ```

## Getting Started
1. Connect your Xbox controller via USB or Bluetooth.
2. Run the project with:
   ```sh
   dotnet run --project ControllerRebinder
   ```
3. The app reads `ControllerRebinder/Configurations.json` on startup.

## Configuration Reference
### Editing `Configurations.json`
Modify the `Configurations.json` file to adjust controller mappings and settings.

Example `Configurations.json`:
```json
{
  "RefreshRate": 10,
  "Log": true,
  "LeftJoyStick": {
    "On": true,
    "StaticArea": 0.5,
    "ForwardDown": 1.0,
    "LeftRight": 0.8,
    "DeadZone": 1000,
    "MaxValController": 32768,
    "ThreshHoldAreaCal": 5000,
    "Controlls": {
      "Up": "VK_W",
      "Down": "VK_S",
      "Left": "VK_A",
      "Right": "VK_D"
    }
  }
}
```

Key fields in this file include:
- **RefreshRate**: number of milliseconds between controller polls.
- **Log**: enables verbose logging of joystick values.
- **LeftJoyStick** and **RightJoyStick**: specify thresholds and key mappings for each stick.
- **Buttons**: maps controller buttons to keyboard keys.

---

## Example Workflow
Start the application with:
```sh
   dotnet run --project ControllerRebinder
```
1. Edit `ControllerRebinder/Configurations.json` to map buttons or adjust joystick thresholds.
2. Launch the program and move the sticks or press buttons on your controller.
3. Confirm that the expected keyboard keys are triggered.

---

## Troubleshooting
### 1. No Key Presses Detected
- Ensure the Xbox controller is connected.
- Check logs for detected joystick movements.
- Verify `Configurations.json` formatting and validity.

### 2. Application Crashes on Startup
- Confirm `Configurations.json` is correctly formatted.
- Ensure all dependencies are installed.
- Run in debug mode for detailed logs:
  ```sh
  dotnet run --project ControllerRebinder --configuration Debug
  ```


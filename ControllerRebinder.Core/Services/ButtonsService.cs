using ControllerRebinder.Common.Models.Configurations.SubModelsOfConfigurations;
using ControllerRebinder.Core.Caches;
using ControllerRebinder.Core.Helpers;
using DXNET.XInput;
using System;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;

namespace ControllerRebinder.Core.Services.Imp
{
    public class ButtonsService : IButtonsService
    {
        private readonly Controller _controller;
        private readonly InputSimulator _inputSimulator;
        private readonly Buttons _configurations;
        private readonly bool _log;

        public ButtonsService(Controller controller, InputSimulator inputSimulator, Buttons configurations, bool log)
        {
            _controller = controller;
            _inputSimulator = inputSimulator;
            _configurations = configurations;
            _log = log;
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var state = _controller.GetState();
                var button = state.Gamepad.Buttons;

                if (_log)
                {
                    Log(button);
                }

                await ExecuteButtonPress(button).ConfigureAwait(false);
                await Task.Delay(ConfigCache.Configurations.RefreshRate, cancellationToken);
            }
        }


        private void Log(GamepadButtonFlags button)
        {
            ConsoleHelper.ClearConsole();
            Console.WriteLine(button);
        }

        private async Task ExecuteButtonPress(GamepadButtonFlags button)
        {
            var keyboard = _inputSimulator.Keyboard;

            if (button == GamepadButtonFlags.X) keyboard.KeyPress(_configurations.X);
            else if (button == GamepadButtonFlags.Y) keyboard.KeyPress(_configurations.Y);
            else if (button == GamepadButtonFlags.B) keyboard.KeyPress(_configurations.B);
            else if (button == GamepadButtonFlags.A) keyboard.KeyPress(_configurations.A);
            else if (button == GamepadButtonFlags.DPadUp) keyboard.KeyPress(_configurations.DPadUp);
            else if (button == GamepadButtonFlags.DPadDown) keyboard.KeyPress(_configurations.DPadDown);
            else if (button == GamepadButtonFlags.DPadLeft) keyboard.KeyPress(_configurations.DPadLeft);
            else if (button == GamepadButtonFlags.DPadRight) keyboard.KeyPress(_configurations.DPadRight);
        }
    }
}

using ControllerRebinder.Common.Enumerations;
using ControllerRebinder.Core.Caches;
using ControllerRebinder.Core.Events.Versions.v04;
using ControllerRebinder.Core.Helpers;
using ControllerRebinder.Core.Services.Imp;
using DXNET.XInput;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;

namespace ControllerRebinder.Core.Services
{
    public class JoyStickService_v04 : IJoyStickService
    {
        private readonly Controller _controller;
        private readonly InputSimulator _inputSimulator;
        private readonly JoyStick _joyStick;
        private readonly ILogger _logger;
        private double _staticYArea;
        private double _currentXArea;

        private Timer _timer;
        private CancellationToken _cancellationToken;

        private const int AreaMultiplier = 10_000_000;

        public event JoyStickEventHandler_v04 JoyStickMoved;

        public JoyStickService_v04(
            Controller controller,
            InputSimulator inputSimulator,
            JoyStick joyStick,
            ILogger logger)
        {
            _controller = controller;
            _inputSimulator = inputSimulator;
            _joyStick = joyStick;
            _logger = logger;
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            var config = ConfigCache.Configurations.LeftJoyStick;

            CircleHelper.FindArea(
                config.ThreshHoldAreaCal,
                config.MaxValController,
                config.MaxValController,
                out double staticYAngle,
                out _staticYArea);

            _timer = new Timer(CheckJoystickState, null, 0, ConfigCache.Configurations.RefreshRate);
            if (_cancellationToken.CanBeCanceled)
            {
                _cancellationToken.Register(() => _timer?.Dispose());
            }
        }

        private void CheckJoystickState(object state)
        {
            try
            {
                var controllerState = _controller.GetState();
                short stickX = 0;
                short stickY = 0;

                ChooseJoyStick(controllerState, ref stickX, ref stickY);
                OnJoyStickMoved(stickX, stickY).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckJoystickState");
            }
        }

        private void ChooseJoyStick(State state, ref short stickX, ref short stickY)
        {
            if (_joyStick == JoyStick.Left)
            {
                stickX = state.Gamepad.LeftThumbX;
                stickY = state.Gamepad.LeftThumbY;
            }
            else
            {
                stickX = state.Gamepad.RightThumbX;
                stickY = state.Gamepad.RightThumbY;
            }
        }

        protected virtual async Task OnJoyStickMoved(int stickX, int stickY)
        {
            var handler = JoyStickMoved;
            if (handler != null)
            {
                await handler(this, new JoyStickEventArgs_v04(stickX, stickY)).ConfigureAwait(false);
            }
        }

        public void Stop()
        {
            _timer?.Dispose();
        }
    }
}

using ControllerRebinder.Common.Enumerations;
using ControllerRebinder.Common.Models.Configurations.SubModelsOfConfigurations;
using ControllerRebinder.Core.Caches;
using ControllerRebinder.Core.Helpers;
using ControllerRebinder.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WindowsInput;

namespace ControllerRebinder.Core.Events.Versions.v04
{
    public class JoyStickEventArgs_v04 : EventArgs
    {
        public int StickX { get; }
        public int StickY { get; }

        public JoyStickEventArgs_v04(int stickX, int stickY)
        {
            StickX = stickX;
            StickY = stickY;
        }
    }

    public delegate Task JoyStickEventHandler_v04(object sender, JoyStickEventArgs_v04 e);

    public class JoyStickHandler_v04
    {
        private const string Version = "4.0";
        private readonly InputSimulator _inputSimulator;
        private readonly ILogger _logger;
        private double _staticYArea;
        private double _currentXArea;
        private Quadrant _currentQuadrant = Quadrant.TopLeft;

        private const int AreaMultiplier = 10_000_000;

        public JoyStickHandler_v04(InputSimulator inputSimulator, ILogger logger)
        {
            _inputSimulator = inputSimulator;
            _logger = logger;
        }

        public void Subscribe(JoyStickService_v04 joyStickService)
        {
            joyStickService.JoyStickMoved += OnJoyStickMoved;
        }

        private async Task OnJoyStickMoved(object sender, JoyStickEventArgs_v04 e)
        {
            var config = ConfigCache.Configurations.LeftJoyStick;
            double upDown = config.ForwardDown * AreaMultiplier;
            double leftRight = config.LeftRight * AreaMultiplier;
            var controls = config.Controlls;
            var deadZone = config.DeadZone;
            var keyboard = _inputSimulator.Keyboard;

            Log(e.StickX, e.StickY);

            if (CircleHelper.IsInDeadZone(e.StickX, e.StickY, deadZone, _logger))
            {
                controls.ReleaseAll_v04(_inputSimulator);
            }
            else
            {
                CircleHelper.FindArea(
                    config.ThreshHoldAreaCal,
                    Math.Abs(e.StickX), Math.Abs(e.StickY),
                    out double currentAngle,
                    out _currentXArea);

                HandleStickMovement(upDown, leftRight, controls, keyboard);
            }

            _currentQuadrant = QuadrantHelper.WhereAmI(e.StickX, e.StickY);
        }

        private void Log(int stickX, int stickY)
        {
            if (ConfigCache.Configurations.Log)
            {
                ConsoleHelper.ClearConsole();
                _logger.LogInformation($"Version {Version}\n");
                _logger.LogInformation($"X (left-right): {stickX} : Y (up-down): {stickY}\n");
                _logger.LogInformation($"Static: {_staticYArea} : X: {_currentXArea}\n");
            }
        }

        private void HandleStickMovement(double upDown, double leftRight, Controlls controls, IKeyboardSimulator keyboard)
        {
            bool upPressed = false, downPressed = false, leftPressed = false, rightPressed = false;

            if (_currentXArea > leftRight && _currentXArea < upDown)
            {
                switch (_currentQuadrant)
                {
                    case Quadrant.TopLeft:
                        keyboard.KeyDown(controls.Up);
                        keyboard.KeyDown(controls.Left);
                        upPressed = leftPressed = true;
                        break;
                    case Quadrant.TopRight:
                        keyboard.KeyDown(controls.Up);
                        keyboard.KeyDown(controls.Right);
                        upPressed = rightPressed = true;
                        break;
                    case Quadrant.BottomLeft:
                        keyboard.KeyDown(controls.Down);
                        keyboard.KeyDown(controls.Left);
                        downPressed = leftPressed = true;
                        break;
                    case Quadrant.BottomRight:
                        keyboard.KeyDown(controls.Down);
                        keyboard.KeyDown(controls.Right);
                        downPressed = rightPressed = true;
                        break;
                }
            }

            if ((_currentQuadrant == Quadrant.TopLeft || _currentQuadrant == Quadrant.TopRight) && _currentXArea > upDown)
            {
                keyboard.KeyDown(controls.Up);
                upPressed = true;
            }
            if ((_currentQuadrant == Quadrant.BottomLeft || _currentQuadrant == Quadrant.BottomRight) && _currentXArea > upDown)
            {
                keyboard.KeyDown(controls.Down);
                downPressed = true;
            }
            if ((_currentQuadrant == Quadrant.TopLeft || _currentQuadrant == Quadrant.BottomLeft) && _currentXArea < leftRight)
            {
                keyboard.KeyDown(controls.Left);
                leftPressed = true;
            }
            if ((_currentQuadrant == Quadrant.TopRight || _currentQuadrant == Quadrant.BottomRight) && _currentXArea < leftRight)
            {
                keyboard.KeyDown(controls.Right);
                rightPressed = true;
            }

            // Release keys that are not pressed
            if (!upPressed) keyboard.KeyUp(controls.Up);
            if (!downPressed) keyboard.KeyUp(controls.Down);
            if (!leftPressed) keyboard.KeyUp(controls.Left);
            if (!rightPressed) keyboard.KeyUp(controls.Right);
        }
    }
}

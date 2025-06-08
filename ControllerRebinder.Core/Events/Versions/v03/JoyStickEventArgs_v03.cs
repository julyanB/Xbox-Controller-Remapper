using ControllerRebinder.Common.Enumerations;
using ControllerRebinder.Common.Models.Configurations.SubModelsOfConfigurations;
using ControllerRebinder.Core.Caches;
using ControllerRebinder.Core.Helpers;
using ControllerRebinder.Core.Services.v03;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WindowsInput;

namespace ControllerRebinder.Core.Events.Versions.v03;
public class JoyStickEventArgs_v03 : EventArgs
{
    public int StickX { get; }
    public int StickY { get; }

    public JoyStickEventArgs_v03(int stickX, int stickY)
    {
        StickX = stickX;
        StickY = stickY;
    }
}

public delegate Task JoyStickEventHandler_v03(object sender, JoyStickEventArgs_v03 e);


public class JoyStickHandler_v03
{
    private const string Version = "3.0";
    private readonly InputSimulator _inputSimulator;
    private readonly ILogger _logger;
    private double _staticYArea;
    private double _currentXArea;
    private Quadrant _currentQuadrant = Quadrant.TopLeft;

    private const int AreaMultiplier = 10_000_000;

    public JoyStickHandler_v03(InputSimulator inputSimulator, ILogger logger)
    {
        _inputSimulator = inputSimulator;
        _logger = logger;
    }

    public void Subscribe(JoyStickService_v03 joyStickService)
    {
        joyStickService.JoyStickMoved += OnJoyStickMoved;
    }

    private async Task OnJoyStickMoved(object sender, JoyStickEventArgs_v03 e)
    {
        var config = ConfigCache.Configurations.LeftJoyStick;
        double upDown = config.ForwardDown * AreaMultiplier;
        double leftRight = config.LeftRight * AreaMultiplier;
        var controlls = config.Controlls;
        var deadZone = config.DeadZone;
        var keyboard = _inputSimulator.Keyboard;

        Log(e.StickX, e.StickY);

        if (CircleHelper.IsInDeadZone(e.StickX, e.StickY, deadZone, _logger))
        {
            await controlls.ReleaseAll_v03(_inputSimulator).ConfigureAwait(false);
        }
        else
        {
            CircleHelper.FindArea(
                config.ThreshHoldAreaCal,
                Math.Abs(e.StickX), Math.Abs(e.StickY),
                out double currentAngle,
                out _currentXArea);

            HandleStickMovement(upDown, leftRight, controlls, keyboard);
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

    private void HandleStickMovement(double upDown, double leftRight, Controlls controlls, IKeyboardSimulator keyboard)
    {
        bool isMoving = false;

        if (_currentXArea > leftRight && _currentXArea < upDown)
        {
            switch (_currentQuadrant)
            {
                case Quadrant.TopLeft:
                    keyboard.KeyDown(controlls.Up);
                    keyboard.KeyDown(controlls.Left);
                    isMoving = true;
                    break;
                case Quadrant.TopRight:
                    keyboard.KeyDown(controlls.Up);
                    keyboard.KeyDown(controlls.Right);
                    isMoving = true;
                    break;
                case Quadrant.BottomLeft:
                    keyboard.KeyDown(controlls.Down);
                    keyboard.KeyDown(controlls.Left);
                    isMoving = true;
                    break;
                case Quadrant.BottomRight:
                    keyboard.KeyDown(controlls.Down);
                    keyboard.KeyDown(controlls.Right);
                    isMoving = true;
                    break;
            }
        }

        if (!isMoving)
        {
            keyboard.KeyUp(controlls.Up);
            keyboard.KeyUp(controlls.Left);
            keyboard.KeyUp(controlls.Down);
            keyboard.KeyUp(controlls.Right);
        }

        if ((_currentQuadrant == Quadrant.TopLeft || _currentQuadrant == Quadrant.TopRight) && _currentXArea > upDown)
        {
            keyboard.KeyDown(controlls.Up);
        }
        if ((_currentQuadrant == Quadrant.TopLeft || _currentQuadrant == Quadrant.BottomLeft) && _currentXArea < leftRight)
        {
            keyboard.KeyDown(controlls.Left);
        }
        if ((_currentQuadrant == Quadrant.BottomLeft || _currentQuadrant == Quadrant.BottomRight) && _currentXArea > upDown)
        {
            keyboard.KeyDown(controlls.Down);
        }
        if ((_currentQuadrant == Quadrant.BottomRight || _currentQuadrant == Quadrant.TopRight) && _currentXArea < leftRight)
        {
            keyboard.KeyDown(controlls.Right);
        }
    }
}



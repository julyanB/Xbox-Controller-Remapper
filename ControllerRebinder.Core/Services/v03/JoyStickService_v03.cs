using ControllerRebinder.Common.Enumerations;
using ControllerRebinder.Common.Models.Configurations.SubModelsOfConfigurations;
using ControllerRebinder.Core.Caches;
using ControllerRebinder.Core.Events.Versions.v03;
using ControllerRebinder.Core.Helpers;
using ControllerRebinder.Core.Services.Imp;
using DXNET.XInput;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;

namespace ControllerRebinder.Core.Services.v03;

public class JoyStickService_v03 : IJoyStickService
{
    private readonly Controller _controller;
    private readonly InputSimulator _inputSimulator;
    private readonly JoyStick _joyStick;
    private readonly ILogger _logger;
    private Quadrant _currentQuadrant = Quadrant.TopLeft;
    private double _staticYArea;
    private double _currentXArea;

    private Timer _timer;

    private const int ThresholdMultiplier = 100;
    private const int AreaMultiplier = 10_000_000;

    public event JoyStickEventHandler_v03 JoyStickMoved;

    public JoyStickService_v03(
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
        var config = ConfigCache.Configurations.LeftJoyStick;
        CircleHelper.FindArea(
            config.ThreshHoldAreaCal,
            config.MaxValController,
            config.MaxValController,
            out double staticYAngle,
            out _staticYArea);

        // Use an async loop instead of a Timer
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var controllerState = _controller.GetState();
                short stickX = 0, stickY = 0;
                ChooseJoyStick(controllerState, ref stickX, ref stickY);
                await OnJoyStickMoved(stickX, stickY).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing joystick state.");
            }
        
            await Task.Delay(ConfigCache.Configurations.RefreshRate, cancellationToken);
        }
    }
    
    private void CheckJoystickState(object state)
    {
        HandleException(() =>
        {
            var controllerState = _controller.GetState();
            short stickX = 0;
            short stickY = 0;

            ChooseJoyStick(controllerState, ref stickX, ref stickY);
            OnJoyStickMoved(stickX, stickY).GetAwaiter().GetResult();
        });
    }

    private void ChooseJoyStick(State state, ref short stickX, ref short stickY)
    {
        switch (_joyStick)
        {
            case JoyStick.Left:
                stickX = state.Gamepad.LeftThumbX;
                stickY = state.Gamepad.LeftThumbY;
                break;
            case JoyStick.Right:
                stickX = state.Gamepad.RightThumbX;
                stickY = state.Gamepad.RightThumbY;
                break;
        }
    }

    protected virtual async Task OnJoyStickMoved(int stickX, int stickY)
    {
        var handler = JoyStickMoved;
        if (handler != null)
        {
            await handler(this, new JoyStickEventArgs_v03(stickX, stickY)).ConfigureAwait(false);
        }
    }

    public void HandleException(Action action)
    {
        try
        {
            action();
        }
        catch { }
    }


}
using System.Threading;
using System.Threading.Tasks;
using ControllerRebinder.Core.Abstractions;
using ControllerRebinder.Core.Configuration;
using ControllerRebinder.Core.Models;
using ControllerRebinder.Core.Processing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ControllerRebinder.Core.Services;

public sealed class ControllerRemappingService : BackgroundService
{
    private readonly IXboxController _controller;
    private readonly ILogger<ControllerRemappingService> _logger;
    private readonly JoystickBindingProcessor _leftJoystick;
    private readonly JoystickBindingProcessor _rightJoystick;
    private readonly ButtonsBindingProcessor _buttons;
    private IDisposable? _optionsChangeToken;
    private readonly object _sync = new();

    private int _refreshRateMs = 8;
    private bool _logEnabled;
    private bool _isDisconnected = true;

    public ControllerRemappingService(
        IXboxController controller,
        IKeyboardEmulator keyboard,
        IOptionsMonitor<ControllerRemapperOptions> optionsMonitor,
        ILogger<ControllerRemappingService> logger)
    {
        _controller = controller;
        _logger = logger;
        _leftJoystick = new JoystickBindingProcessor(JoystickSide.Left, keyboard);
        _rightJoystick = new JoystickBindingProcessor(JoystickSide.Right, keyboard);
        _buttons = new ButtonsBindingProcessor(keyboard);

        ApplyOptions(optionsMonitor.CurrentValue);
        _optionsChangeToken = optionsMonitor.OnChange(ApplyOptions);
    }

    private void ApplyOptions(ControllerRemapperOptions options)
    {
        lock (_sync)
        {
            var left = options.LeftJoystick ?? new JoystickOptions();
            var right = options.RightJoystick ?? new JoystickOptions();
            var buttons = options.Buttons ?? new ButtonsOptions();

            _leftJoystick.UpdateConfiguration(left);
            _rightJoystick.UpdateConfiguration(right);
            _buttons.UpdateConfiguration(buttons);
            _logEnabled = options.Log;
            Volatile.Write(ref _refreshRateMs, Math.Max(1, options.RefreshRate));
        }

        _logger.LogInformation("Controller remapper options applied. RefreshRate={RefreshRate}ms", options.RefreshRate);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Controller remapping service starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ProcessFrame();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing controller input.");
            }

            var delay = TimeSpan.FromMilliseconds(Math.Max(1, Volatile.Read(ref _refreshRateMs)));
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        lock (_sync)
        {
            ReleaseAllInternal();
        }

        _logger.LogInformation("Controller remapping service stopped.");
    }

    private void ProcessFrame()
    {
        lock (_sync)
        {
            if (!_controller.IsConnected)
            {
                if (!_isDisconnected)
                {
                    _logger.LogWarning("Xbox controller disconnected. Waiting for reconnection...");
                    ReleaseAllInternal();
                    _isDisconnected = true;
                }

                return;
            }

            if (_isDisconnected)
            {
                _logger.LogInformation("Xbox controller connected.");
                _isDisconnected = false;
            }

            GamepadSnapshot snapshot;
            try
            {
                snapshot = _controller.GetState();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read controller state. Releasing keys.");
                ReleaseAllInternal();
                _isDisconnected = true;
                return;
            }

            var shouldLog = _logEnabled;
            _leftJoystick.Process(snapshot.LeftThumbX, snapshot.LeftThumbY, shouldLog, _logger);
            _rightJoystick.Process(snapshot.RightThumbX, snapshot.RightThumbY, shouldLog, _logger);
            _buttons.Process(snapshot.Buttons);
        }
    }

    private void ReleaseAllInternal()
    {
        _leftJoystick.ReleaseAll();
        _rightJoystick.ReleaseAll();
        _buttons.ReleaseAll();
    }

    public override void Dispose()
    {
        base.Dispose();
        _optionsChangeToken?.Dispose();
    }
}

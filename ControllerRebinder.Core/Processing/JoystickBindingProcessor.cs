using ControllerRebinder.Core.Abstractions;
using ControllerRebinder.Core.Configuration;
using ControllerRebinder.Core.Models;
using ControllerRebinder.Core.Utilities;
using Microsoft.Extensions.Logging;
using WindowsInput.Native;

namespace ControllerRebinder.Core.Processing;

internal sealed class JoystickBindingProcessor
{
    private readonly JoystickSide _side;
    private readonly KeyboardStateTracker _keyboardState;
    private readonly List<VirtualKeyCode> _workingKeys = new(2);

    private JoystickOptions _options = new();
    private JoystickKeyMap _keys;
    private bool _enabled;
    private double _leftRightThresholdArea;
    private double _forwardDownThresholdArea;

    public JoystickBindingProcessor(JoystickSide side, IKeyboardEmulator keyboard)
    {
        _side = side;
        _keyboardState = new KeyboardStateTracker(keyboard);
    }

    public void UpdateConfiguration(JoystickOptions options)
    {
        _options = options ?? new JoystickOptions();
        _enabled = _options.Enabled;
        _leftRightThresholdArea = _options.LeftRight * JoystickMath.AreaMultiplier;
        _forwardDownThresholdArea = _options.ForwardDown * JoystickMath.AreaMultiplier;
        _keys = CreateKeyMap(_options.Keys);

        if (!_enabled)
        {
            _keyboardState.ReleaseAll();
        }
    }

    public void Process(short rawX, short rawY, bool logEnabled, ILogger logger)
    {
        if (!_enabled)
        {
            _keyboardState.ReleaseAll();
            return;
        }

        if (JoystickMath.Magnitude(rawX, rawY) <= _options.DeadZone)
        {
            _keyboardState.ReleaseAll();
            return;
        }

        var quadrant = QuadrantResolver.FromAxes(rawX, rawY);
        if (quadrant == Quadrant.Center)
        {
            _keyboardState.ReleaseAll();
            return;
        }

        var absX = Math.Abs((int)rawX);
        var absY = Math.Abs((int)rawY);
        var area = JoystickMath.ComputeMovementArea(_options.Threshold, absX, absY);

        if (logEnabled)
        {
            logger.LogInformation("{Side} joystick x:{X} y:{Y} area:{Area:F0}", _side, rawX, rawY, area);
        }

        _workingKeys.Clear();
        var verticalKey = quadrant is Quadrant.TopLeft or Quadrant.TopRight ? _keys.Up : _keys.Down;
        var horizontalKey = quadrant is Quadrant.TopLeft or Quadrant.BottomLeft ? _keys.Left : _keys.Right;

        if (area >= _forwardDownThresholdArea)
        {
            _workingKeys.Add(verticalKey);
        }
        else if (area <= _leftRightThresholdArea)
        {
            _workingKeys.Add(horizontalKey);
        }
        else
        {
            _workingKeys.Add(verticalKey);
            _workingKeys.Add(horizontalKey);
        }

        _keyboardState.Apply(_workingKeys);
        _workingKeys.Clear();
    }

    public void ReleaseAll()
    {
        _keyboardState.ReleaseAll();
    }

    private static JoystickKeyMap CreateKeyMap(JoystickKeyBindings bindings)
    {
        if (!VirtualKeyParser.TryParse(bindings.Up, out var up) ||
            !VirtualKeyParser.TryParse(bindings.Down, out var down) ||
            !VirtualKeyParser.TryParse(bindings.Left, out var left) ||
            !VirtualKeyParser.TryParse(bindings.Right, out var right))
        {
            throw new InvalidOperationException("Joystick key bindings contain invalid values.");
        }

        return new JoystickKeyMap(up, down, left, right);
    }
}

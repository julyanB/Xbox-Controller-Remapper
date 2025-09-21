using System.Collections.Generic;
using ControllerRebinder.Core.Abstractions;
using ControllerRebinder.Core.Configuration;
using ControllerRebinder.Core.Processing;
using ControllerRebinder.Core.Utilities;
using DXNET.XInput;
using WindowsInput.Native;

namespace ControllerRebinder.Core.Processing;

internal sealed class ButtonsBindingProcessor
{
    private readonly KeyboardStateTracker _keyboardState;
    private readonly Dictionary<GamepadButtonFlags, VirtualKeyCode> _bindings = new();
    private readonly List<VirtualKeyCode> _workingKeys = new();
    private bool _enabled;

    public ButtonsBindingProcessor(IKeyboardEmulator keyboard)
    {
        _keyboardState = new KeyboardStateTracker(keyboard);
    }

    public void UpdateConfiguration(ButtonsOptions options)
    {
        _bindings.Clear();
        _enabled = options?.Enabled ?? false;

        if (!_enabled || options is null)
        {
            _keyboardState.ReleaseAll();
            return;
        }

        TryAddBinding(options.Keys.A, GamepadButtonFlags.A);
        TryAddBinding(options.Keys.B, GamepadButtonFlags.B);
        TryAddBinding(options.Keys.X, GamepadButtonFlags.X);
        TryAddBinding(options.Keys.Y, GamepadButtonFlags.Y);
        TryAddBinding(options.Keys.DPadUp, GamepadButtonFlags.DPadUp);
        TryAddBinding(options.Keys.DPadDown, GamepadButtonFlags.DPadDown);
        TryAddBinding(options.Keys.DPadLeft, GamepadButtonFlags.DPadLeft);
        TryAddBinding(options.Keys.DPadRight, GamepadButtonFlags.DPadRight);
        TryAddBinding(options.Keys.LeftShoulder, GamepadButtonFlags.LeftShoulder);
        TryAddBinding(options.Keys.RightShoulder, GamepadButtonFlags.RightShoulder);
        TryAddBinding(options.Keys.Start, GamepadButtonFlags.Start);
        TryAddBinding(options.Keys.Back, GamepadButtonFlags.Back);

        if (_bindings.Count == 0)
        {
            _enabled = false;
        }
    }

    public void Process(GamepadButtonFlags buttons)
    {
        if (!_enabled)
        {
            _keyboardState.ReleaseAll();
            return;
        }

        _workingKeys.Clear();
        foreach (var binding in _bindings)
        {
            if ((buttons & binding.Key) == binding.Key)
            {
                _workingKeys.Add(binding.Value);
            }
        }

        _keyboardState.Apply(_workingKeys);
        _workingKeys.Clear();
    }

    public void ReleaseAll()
    {
        _keyboardState.ReleaseAll();
    }

    private void TryAddBinding(string? value, GamepadButtonFlags flag)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!VirtualKeyParser.TryParse(value, out var key))
        {
            throw new InvalidOperationException($"Invalid virtual key '{value}' configured for {flag}.");
        }

        _bindings[flag] = key;
    }
}

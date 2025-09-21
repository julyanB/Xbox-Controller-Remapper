using System.Collections.Generic;
using ControllerRebinder.Core.Abstractions;
using WindowsInput.Native;

namespace ControllerRebinder.Core.Processing;

internal sealed class KeyboardStateTracker
{
    private readonly IKeyboardEmulator _keyboard;
    private readonly HashSet<VirtualKeyCode> _pressed = new();

    public KeyboardStateTracker(IKeyboardEmulator keyboard)
    {
        _keyboard = keyboard;
    }

    public void Apply(IReadOnlyCollection<VirtualKeyCode> desired)
    {
        if (desired.Count == 0)
        {
            ReleaseAll();
            return;
        }

        foreach (var key in desired)
        {
            if (_pressed.Add(key))
            {
                _keyboard.KeyDown(key);
            }
        }

        var toRelease = new List<VirtualKeyCode>();
        foreach (var key in _pressed)
        {
            if (!desired.Contains(key))
            {
                toRelease.Add(key);
            }
        }

        foreach (var key in toRelease)
        {
            _keyboard.KeyUp(key);
            _pressed.Remove(key);
        }
    }

    public void ReleaseAll()
    {
        if (_pressed.Count == 0)
        {
            return;
        }

        foreach (var key in _pressed)
        {
            _keyboard.KeyUp(key);
        }

        _pressed.Clear();
    }
}

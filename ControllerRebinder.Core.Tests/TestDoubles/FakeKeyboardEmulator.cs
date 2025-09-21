using System.Collections.Generic;
using ControllerRebinder.Core.Abstractions;
using WindowsInput.Native;

namespace ControllerRebinder.Core.Tests.TestDoubles;

internal sealed class FakeKeyboardEmulator : IKeyboardEmulator
{
    private readonly HashSet<VirtualKeyCode> _pressed = new();

    public IReadOnlyCollection<VirtualKeyCode> PressedKeys => _pressed;

    public List<(string Action, VirtualKeyCode Key)> Events { get; } = new();

    public void KeyDown(VirtualKeyCode key)
    {
        _pressed.Add(key);
        Events.Add(("Down", key));
    }

    public void KeyUp(VirtualKeyCode key)
    {
        _pressed.Remove(key);
        Events.Add(("Up", key));
    }
}

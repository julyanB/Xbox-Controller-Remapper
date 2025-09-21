using ControllerRebinder.Core.Abstractions;
using WindowsInput;
using WindowsInput.Native;

namespace ControllerRebinder.Core.Adapters;

public sealed class KeyboardSimulatorAdapter : IKeyboardEmulator
{
    private readonly InputSimulator _simulator;

    public KeyboardSimulatorAdapter(InputSimulator simulator)
    {
        _simulator = simulator;
    }

    public void KeyDown(VirtualKeyCode key)
    {
        _simulator.Keyboard.KeyDown(key);
    }

    public void KeyUp(VirtualKeyCode key)
    {
        _simulator.Keyboard.KeyUp(key);
    }
}

using WindowsInput.Native;

namespace ControllerRebinder.Core.Abstractions;

public interface IKeyboardEmulator
{
    void KeyDown(VirtualKeyCode key);

    void KeyUp(VirtualKeyCode key);
}

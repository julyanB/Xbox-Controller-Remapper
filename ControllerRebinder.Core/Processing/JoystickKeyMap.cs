using WindowsInput.Native;

namespace ControllerRebinder.Core.Processing;

internal readonly record struct JoystickKeyMap(
    VirtualKeyCode Up,
    VirtualKeyCode Down,
    VirtualKeyCode Left,
    VirtualKeyCode Right);

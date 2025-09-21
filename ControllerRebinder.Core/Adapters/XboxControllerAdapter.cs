using ControllerRebinder.Core.Abstractions;
using ControllerRebinder.Core.Models;
using DXNET.XInput;

namespace ControllerRebinder.Core.Adapters;

public sealed class XboxControllerAdapter : IXboxController
{
    private readonly Controller _controller;

    public XboxControllerAdapter(UserIndex userIndex = UserIndex.One)
    {
        _controller = new Controller(userIndex);
    }

    public bool IsConnected => _controller.IsConnected;

    public GamepadSnapshot GetState()
    {
        var state = _controller.GetState();
        var gamepad = state.Gamepad;
        return new GamepadSnapshot(
            gamepad.LeftThumbX,
            gamepad.LeftThumbY,
            gamepad.RightThumbX,
            gamepad.RightThumbY,
            gamepad.LeftTrigger,
            gamepad.RightTrigger,
            gamepad.Buttons);
    }
}

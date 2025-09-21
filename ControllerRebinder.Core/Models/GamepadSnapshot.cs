using DXNET.XInput;

namespace ControllerRebinder.Core.Models;

/// <summary>
/// Immutable snapshot of the gamepad state used by the remapping pipeline.
/// </summary>
public readonly struct GamepadSnapshot
{
    public GamepadSnapshot(
        short leftThumbX,
        short leftThumbY,
        short rightThumbX,
        short rightThumbY,
        byte leftTrigger,
        byte rightTrigger,
        GamepadButtonFlags buttons)
    {
        LeftThumbX = leftThumbX;
        LeftThumbY = leftThumbY;
        RightThumbX = rightThumbX;
        RightThumbY = rightThumbY;
        LeftTrigger = leftTrigger;
        RightTrigger = rightTrigger;
        Buttons = buttons;
    }

    public short LeftThumbX { get; }
    public short LeftThumbY { get; }
    public short RightThumbX { get; }
    public short RightThumbY { get; }
    public byte LeftTrigger { get; }
    public byte RightTrigger { get; }
    public GamepadButtonFlags Buttons { get; }
}

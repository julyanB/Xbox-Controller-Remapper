namespace ControllerRebinder.Core.Configuration;

public sealed class JoystickOptions
{
    public bool Enabled { get; set; }

    public double ForwardDown { get; set; } = 24;

    public double LeftRight { get; set; } = 15;

    public int DeadZone { get; set; } = 8_000;

    public int MaxValue { get; set; } = 32_767;

    public int Threshold { get; set; } = 21_815;

    public JoystickKeyBindings Keys { get; set; } = new();
}

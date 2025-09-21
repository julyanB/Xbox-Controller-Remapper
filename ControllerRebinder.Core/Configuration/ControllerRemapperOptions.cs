namespace ControllerRebinder.Core.Configuration;

public sealed class ControllerRemapperOptions
{
    public const string SectionName = "ControllerRemapper";

    public int RefreshRate { get; set; } = 8;

    public bool Log { get; set; }

    /// <summary>
    /// Zero-based index of the controller to bind. Defaults to the first controller.
    /// </summary>
    public int ControllerIndex { get; set; } = 0;

    public JoystickOptions LeftJoystick { get; set; } = new();

    public JoystickOptions RightJoystick { get; set; } = new();

    public ButtonsOptions Buttons { get; set; } = new();
}

namespace ControllerRebinder.Core.Configuration;

public sealed class ButtonsOptions
{
    public bool Enabled { get; set; }

    public ButtonKeyBindings Keys { get; set; } = new();
}

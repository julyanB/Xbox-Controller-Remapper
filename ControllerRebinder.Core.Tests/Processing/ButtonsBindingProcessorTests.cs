using ControllerRebinder.Core.Configuration;
using ControllerRebinder.Core.Processing;
using ControllerRebinder.Core.Tests.TestDoubles;
using DXNET.XInput;
using WindowsInput.Native;

namespace ControllerRebinder.Core.Tests.Processing;

public class ButtonsBindingProcessorTests
{
    private static ButtonsOptions CreateOptions()
    {
        return new ButtonsOptions
        {
            Enabled = true,
            Keys = new ButtonKeyBindings
            {
                A = "SPACE",
                B = "VK_Q"
            }
        };
    }

    [Fact]
    public void Process_ButtonPress_PressesConfiguredKey()
    {
        var keyboard = new FakeKeyboardEmulator();
        var processor = new ButtonsBindingProcessor(keyboard);
        processor.UpdateConfiguration(CreateOptions());

        processor.Process(GamepadButtonFlags.A);

        Assert.Contains(VirtualKeyCode.SPACE, keyboard.PressedKeys);
    }

    [Fact]
    public void Process_ReleaseButton_ReleasesKey()
    {
        var keyboard = new FakeKeyboardEmulator();
        var processor = new ButtonsBindingProcessor(keyboard);
        processor.UpdateConfiguration(CreateOptions());

        processor.Process(GamepadButtonFlags.A);
        Assert.Contains(VirtualKeyCode.SPACE, keyboard.PressedKeys);

        processor.Process(GamepadButtonFlags.None);
        Assert.Empty(keyboard.PressedKeys);
    }

    [Fact]
    public void Process_MultipleButtons_HoldsAllConfiguredKeys()
    {
        var keyboard = new FakeKeyboardEmulator();
        var processor = new ButtonsBindingProcessor(keyboard);
        processor.UpdateConfiguration(CreateOptions());

        processor.Process(GamepadButtonFlags.A | GamepadButtonFlags.B);

        Assert.Contains(VirtualKeyCode.SPACE, keyboard.PressedKeys);
        Assert.Contains(VirtualKeyCode.VK_Q, keyboard.PressedKeys);
    }
}

using ControllerRebinder.Core.Configuration;
using ControllerRebinder.Core.Models;
using ControllerRebinder.Core.Processing;
using ControllerRebinder.Core.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using WindowsInput.Native;

namespace ControllerRebinder.Core.Tests.Processing;

public class JoystickBindingProcessorTests
{
    private static JoystickOptions CreateOptions()
    {
        return new JoystickOptions
        {
            Enabled = true,
            ForwardDown = 0.7,
            LeftRight = 0.3,
            DeadZone = 500,
            Threshold = 4_000,
            Keys = new JoystickKeyBindings
            {
                Up = "VK_W",
                Down = "VK_S",
                Left = "VK_A",
                Right = "VK_D"
            }
        };
    }

    [Fact]
    public void Process_HorizontalMovement_PressesHorizontalKeyOnly()
    {
        var keyboard = new FakeKeyboardEmulator();
        var processor = new JoystickBindingProcessor(JoystickSide.Left, keyboard);
        processor.UpdateConfiguration(CreateOptions());

        processor.Process(6_000, 0, logEnabled: false, NullLogger.Instance);

        Assert.Contains(VirtualKeyCode.VK_D, keyboard.PressedKeys);
        Assert.DoesNotContain(VirtualKeyCode.VK_W, keyboard.PressedKeys);
    }

    [Fact]
    public void Process_VerticalMovement_PressesVerticalKeyOnly()
    {
        var keyboard = new FakeKeyboardEmulator();
        var processor = new JoystickBindingProcessor(JoystickSide.Left, keyboard);
        processor.UpdateConfiguration(CreateOptions());

        processor.Process(0, 6_000, logEnabled: false, NullLogger.Instance);

        Assert.Contains(VirtualKeyCode.VK_W, keyboard.PressedKeys);
        Assert.DoesNotContain(VirtualKeyCode.VK_D, keyboard.PressedKeys);
    }

    [Fact]
    public void Process_DiagonalMovement_HoldsBothKeys()
    {
        var keyboard = new FakeKeyboardEmulator();
        var processor = new JoystickBindingProcessor(JoystickSide.Left, keyboard);
        processor.UpdateConfiguration(CreateOptions());

        processor.Process(6_000, 6_000, logEnabled: false, NullLogger.Instance);

        Assert.Contains(VirtualKeyCode.VK_W, keyboard.PressedKeys);
        Assert.Contains(VirtualKeyCode.VK_D, keyboard.PressedKeys);
    }

    [Fact]
    public void Process_EnterDeadZone_ReleasesKeys()
    {
        var keyboard = new FakeKeyboardEmulator();
        var processor = new JoystickBindingProcessor(JoystickSide.Left, keyboard);
        processor.UpdateConfiguration(CreateOptions());

        processor.Process(6_000, 0, logEnabled: false, NullLogger.Instance);
        Assert.Contains(VirtualKeyCode.VK_D, keyboard.PressedKeys);

        processor.Process(0, 0, logEnabled: false, NullLogger.Instance);
        Assert.Empty(keyboard.PressedKeys);
    }
}

using ControllerRebinder.Core.Models;
using ControllerRebinder.Core.Utilities;
using Microsoft.Extensions.Options;

namespace ControllerRebinder.Core.Configuration;

public sealed class ControllerRemapperOptionsValidator : IValidateOptions<ControllerRemapperOptions>
{
    public ValidateOptionsResult Validate(string? name, ControllerRemapperOptions options)
    {
        if (options is null)
        {
            return ValidateOptionsResult.Fail("Options instance is null.");
        }

        var errors = new List<string>();

        if (options.RefreshRate < 1)
        {
            errors.Add("RefreshRate must be at least 1 millisecond.");
        }

        if (options.ControllerIndex < 0 || options.ControllerIndex > 3)
        {
            errors.Add("ControllerIndex must be between 0 and 3 (inclusive).");
        }

        ValidateJoystick(JoystickSide.Left, options.LeftJoystick, errors);
        ValidateJoystick(JoystickSide.Right, options.RightJoystick, errors);
        ValidateButtons(options.Buttons, errors);

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }

    private static void ValidateJoystick(JoystickSide side, JoystickOptions options, List<string> errors)
    {
        if (options is null)
        {
            errors.Add($"{side} joystick options must be provided.");
            return;
        }

        if (!options.Enabled)
        {
            return;
        }

        if (options.LeftRight < 0)
        {
            errors.Add($"{side} joystick LeftRight threshold must be non-negative.");
        }

        if (options.ForwardDown < options.LeftRight)
        {
            errors.Add($"{side} joystick ForwardDown must be greater than or equal to LeftRight.");
        }

        if (options.DeadZone < 0)
        {
            errors.Add($"{side} joystick dead zone must be non-negative.");
        }

        if (options.MaxValue <= 0)
        {
            errors.Add($"{side} joystick MaxValue must be greater than zero.");
        }

        if (options.DeadZone >= options.MaxValue)
        {
            errors.Add($"{side} joystick DeadZone must be less than MaxValue.");
        }

        if (options.Threshold <= 0)
        {
            errors.Add($"{side} joystick Threshold must be greater than zero.");
        }
        else if (options.Threshold > options.MaxValue)
        {
            errors.Add($"{side} joystick Threshold must be less than or equal to MaxValue.");
        }

        if (string.IsNullOrWhiteSpace(options.Keys.Up) ||
            string.IsNullOrWhiteSpace(options.Keys.Down) ||
            string.IsNullOrWhiteSpace(options.Keys.Left) ||
            string.IsNullOrWhiteSpace(options.Keys.Right))
        {
            errors.Add($"{side} joystick key bindings must include Up/Down/Left/Right.");
        }
        else
        {
            ValidateVirtualKey(options.Keys.Up, $"{side} joystick Up", errors);
            ValidateVirtualKey(options.Keys.Down, $"{side} joystick Down", errors);
            ValidateVirtualKey(options.Keys.Left, $"{side} joystick Left", errors);
            ValidateVirtualKey(options.Keys.Right, $"{side} joystick Right", errors);
        }
    }

    private static void ValidateButtons(ButtonsOptions options, List<string> errors)
    {
        if (options is null)
        {
            errors.Add("Button mappings must be provided.");
            return;
        }

        if (!options.Enabled)
        {
            return;
        }

        ValidateOptionalKey(options.Keys.A, "Button A", errors);
        ValidateOptionalKey(options.Keys.B, "Button B", errors);
        ValidateOptionalKey(options.Keys.X, "Button X", errors);
        ValidateOptionalKey(options.Keys.Y, "Button Y", errors);
        ValidateOptionalKey(options.Keys.DPadUp, "DPad Up", errors);
        ValidateOptionalKey(options.Keys.DPadDown, "DPad Down", errors);
        ValidateOptionalKey(options.Keys.DPadLeft, "DPad Left", errors);
        ValidateOptionalKey(options.Keys.DPadRight, "DPad Right", errors);
        ValidateOptionalKey(options.Keys.LeftShoulder, "Left Shoulder", errors);
        ValidateOptionalKey(options.Keys.RightShoulder, "Right Shoulder", errors);
        ValidateOptionalKey(options.Keys.Start, "Start", errors);
        ValidateOptionalKey(options.Keys.Back, "Back", errors);
    }

    private static void ValidateOptionalKey(string? value, string name, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        ValidateVirtualKey(value, name, errors);
    }

    private static void ValidateVirtualKey(string value, string name, List<string> errors)
    {
        if (!VirtualKeyParser.TryParse(value, out _))
        {
            errors.Add($"{name} value '{value}' is not a valid VirtualKeyCode enum value.");
        }
    }
}

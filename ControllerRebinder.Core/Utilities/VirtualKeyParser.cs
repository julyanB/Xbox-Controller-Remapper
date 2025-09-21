using System;
using WindowsInput.Native;

namespace ControllerRebinder.Core.Utilities;

public static class VirtualKeyParser
{
    public static bool TryParse(string? value, out VirtualKeyCode key)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            key = default;
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out key);
    }
}

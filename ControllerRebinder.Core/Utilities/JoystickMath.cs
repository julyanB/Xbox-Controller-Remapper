using System;

namespace ControllerRebinder.Core.Utilities;

public static class JoystickMath
{
    public const double AreaMultiplier = 10_000_000d;

    public static double ComputeMovementArea(int threshold, int absX, int absY)
    {
        if (threshold <= 0 || (absX == 0 && absY == 0))
        {
            return 0d;
        }

        var angle = Math.Atan2(absY, absX);
        return Math.Abs(angle) * threshold * threshold * 0.5d;
    }

    public static double Magnitude(short x, short y)
    {
        return Math.Sqrt((double)x * x + (double)y * y);
    }
}

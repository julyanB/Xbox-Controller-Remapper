using ControllerRebinder.Core.Models;

namespace ControllerRebinder.Core.Utilities;

public static class QuadrantResolver
{
    public static Quadrant FromAxes(int x, int y)
    {
        if (x == 0 && y == 0)
        {
            return Quadrant.Center;
        }

        if (y >= 0)
        {
            return x > 0 ? Quadrant.TopRight : Quadrant.TopLeft;
        }

        return x > 0 ? Quadrant.BottomRight : Quadrant.BottomLeft;
    }
}

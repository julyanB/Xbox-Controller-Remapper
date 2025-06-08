using ControllerRebinder.Common.Enumerations;
using ControllerRebinder.Common.Models;
using System.Collections.Generic;

namespace ControllerRebinder.Core.Helpers
{
    public static class QuadrantHelper
    {
        /// <summary>
        /// Tells us in what Qudrant are we at the specific moment by x and y values
        /// </summary>
        public static Quadrant WhereAmI(int LeftStickX, int LeftStickY)
        {
            if(LeftStickX <= 0 && LeftStickY > 0)
            {
                return Quadrant.TopLeft;
            }
            else if(LeftStickX > 0 && LeftStickY > 0)
            {
                return Quadrant.TopRight;
            }
            else if(LeftStickX <= 0 && LeftStickY < 0)
            {
                return Quadrant.BottomLeft;
            }
            else if(LeftStickX > 0 && LeftStickY <= 0)
            {
                return Quadrant.BottomRight;
            }

            return Quadrant.DefaultQuadrant;
        }

        /// <summary>
        /// Tells us in what Zone are we at the specific by giving it specific ranges that we migth be in
        /// and our current area the we are in right now
        /// </summary>
        public static ZoneRange WhereAmI(List<ZoneRange> ranges, double _currentXArea)
        {
            foreach(var range in ranges)
            {
                if(range.ZoneCalculationType == ZoneCalctulationType.Positive)
                {
                    if(_currentXArea > range.Left)
                    {
                        return range;
                    }
                }
                else if(range.ZoneCalculationType == ZoneCalctulationType.Between)
                {
                    if(_currentXArea > range.SmallerNumber() && _currentXArea < range.BiggerNumber())
                    {
                        return range;
                    }
                }
                else if(range.ZoneCalculationType == ZoneCalctulationType.Negative)
                {
                    if(_currentXArea < range.Left)
                    {
                        return range;
                    }
                }
            }

            return null;
        }
    }
}

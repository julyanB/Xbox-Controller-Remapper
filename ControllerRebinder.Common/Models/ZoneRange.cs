using ControllerRebinder.Common.Enumerations;
using System;
using System.Collections.Generic;
using WindowsInput.Native;

namespace ControllerRebinder.Common.Models
{
    /// <summary>
    /// ZoneRanges Reprezents a specific zone in an Quadrant Upper, Middle, Lower
    /// Each Zone Holds its ZoneCalctulationType wich tells us how we need to calculate where we are and its own Buttons 
    /// we need to click to recreate the movement for the zone
    /// </summary>
    public class ZoneRange : IEquatable<ZoneRange>
    {

        public double Left { get; set; }
        public double Right { get; set; }

        public List<VirtualKeyCode> Buttons { get; set; }
        public Zone Zone { get; set; }
        public ZoneCalctulationType ZoneCalculationType { get; set; }


        public ZoneRange(double left, double right, ZoneCalctulationType positive = ZoneCalctulationType.DefaultZoneCalctulationType, Zone zone = Zone.DefaultZone, List<VirtualKeyCode> buttons = null)
        {
            Left = left;
            Right = right;
            ZoneCalculationType = positive;
            Zone = zone;
            Buttons = buttons;
        }

        public ZoneRange()
        {

        }

        public bool Equals(ZoneRange other)
        {
            return this.Left == other.Left && this.Right == other.Right;
        }

        public double BiggerNumber()
        {
            return this.Left > this.Right ? Left : Right;
        }

        public double SmallerNumber()
        {
            return this.Left < this.Right ? Left : Right;
        }
    }

}

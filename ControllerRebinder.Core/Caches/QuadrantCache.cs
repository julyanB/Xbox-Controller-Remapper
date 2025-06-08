using ControllerRebinder.Common.Enumerations;
using ControllerRebinder.Common.Models;
using ControllerRebinder.Common.Models.Configurations;
using System.Collections.Generic;
using WindowsInput.Native;

namespace ControllerRebinder.Core.Caches
{

    public static class QuadrantCache
    {
        private static Configurations Configurations { get; set; }
        public static Dictionary<Quadrant, List<ZoneRange>> Quadrants { get; set; }


        /// <summary>
        /// The Init method caches hte information about eache Quadrant and its zones att the start of the application
        /// so we can move between them. Each Quadrant has the Zones and eaches zone needs to be calculated diffrently 
        /// thats why we use the ZoneCalctulationType enum to indicate the calculation type.
        /// </summary>
        public static void Init()
        {
            Configurations = ConfigCache.Configurations;
            var controlls = Configurations.LeftJoyStick.Controlls;

            Quadrants = new Dictionary<Quadrant, List<ZoneRange>>();

            Quadrants.Add(Quadrant.TopLeft, new List<ZoneRange>());
            Quadrants.Add(Quadrant.TopRight, new List<ZoneRange>());
            Quadrants.Add(Quadrant.BottomLeft, new List<ZoneRange>());
            Quadrants.Add(Quadrant.BottomRight, new List<ZoneRange>());

            //++
            Quadrants[Quadrant.TopRight].Add(new ZoneRange(Configurations.LeftJoyStick.ForwardDown, Configurations.LeftJoyStick.ForwardDown, ZoneCalctulationType.Positive, Zone.Upper, new List<VirtualKeyCode> { controlls.Up }));
            Quadrants[Quadrant.TopRight].Add(new ZoneRange(Configurations.LeftJoyStick.ForwardDown, Configurations.LeftJoyStick.LeftRight, ZoneCalctulationType.Between, Zone.Middle, new List<VirtualKeyCode> { controlls.Up, controlls.Right }));
            Quadrants[Quadrant.TopRight].Add(new ZoneRange(Configurations.LeftJoyStick.LeftRight, Configurations.LeftJoyStick.LeftRight, ZoneCalctulationType.Negative, Zone.Lower, new List<VirtualKeyCode> { controlls.Right }));

            //-+
            Quadrants[Quadrant.TopLeft].Add(new ZoneRange(Configurations.LeftJoyStick.ForwardDown, Configurations.LeftJoyStick.ForwardDown, ZoneCalctulationType.Positive, Zone.Upper, new List<VirtualKeyCode> { controlls.Up }));
            Quadrants[Quadrant.TopLeft].Add(new ZoneRange(Configurations.LeftJoyStick.ForwardDown, Configurations.LeftJoyStick.LeftRight, ZoneCalctulationType.Between, Zone.Middle, new List<VirtualKeyCode> { controlls.Up, controlls.Left }));
            Quadrants[Quadrant.TopLeft].Add(new ZoneRange(Configurations.LeftJoyStick.LeftRight, Configurations.LeftJoyStick.LeftRight, ZoneCalctulationType.Negative, Zone.Lower, new List<VirtualKeyCode> { controlls.Left }));

            //--
            Quadrants[Quadrant.BottomLeft].Add(new ZoneRange(Configurations.LeftJoyStick.LeftRight, Configurations.LeftJoyStick.LeftRight, ZoneCalctulationType.Negative, Zone.Upper, new List<VirtualKeyCode> { controlls.Left }));
            Quadrants[Quadrant.BottomLeft].Add(new ZoneRange(Configurations.LeftJoyStick.ForwardDown, Configurations.LeftJoyStick.LeftRight, ZoneCalctulationType.Between, Zone.Middle, new List<VirtualKeyCode> { controlls.Down, controlls.Left }));
            Quadrants[Quadrant.BottomLeft].Add(new ZoneRange(Configurations.LeftJoyStick.ForwardDown, Configurations.LeftJoyStick.ForwardDown, ZoneCalctulationType.Positive, Zone.Lower, new List<VirtualKeyCode> { controlls.Down }));

            //+-
            Quadrants[Quadrant.BottomRight].Add(new ZoneRange(Configurations.LeftJoyStick.ForwardDown, Configurations.LeftJoyStick.ForwardDown, ZoneCalctulationType.Positive, Zone.Lower, new List<VirtualKeyCode> { controlls.Down }));
            Quadrants[Quadrant.BottomRight].Add(new ZoneRange(Configurations.LeftJoyStick.ForwardDown, Configurations.LeftJoyStick.LeftRight, ZoneCalctulationType.Between, Zone.Middle, new List<VirtualKeyCode> { controlls.Down, controlls.Right }));
            Quadrants[Quadrant.BottomRight].Add(new ZoneRange(Configurations.LeftJoyStick.LeftRight, Configurations.LeftJoyStick.LeftRight, ZoneCalctulationType.Negative, Zone.Upper, new List<VirtualKeyCode> { controlls.Right }));

        }



    }
}

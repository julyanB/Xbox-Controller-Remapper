using ControllerRebinder.Common.Enumerations;
using ControllerRebinder.Common.Models;
using ControllerRebinder.Common.Models.Configurations;
using ControllerRebinder.Core.Caches;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ControllerRebinder.Core.Helpers
{
    public static class CircleHelper
    {
        /// <summary>
        /// Use Distance formula for a circle to measure if you are in the dead zone
        /// </summary>
        public static bool IsInDeadZone(int stickX, int stickY, int deadZone, ILogger logger = null)
        {
            var currentPositionInTheCircle = Math.Sqrt(stickX * stickX + stickY * stickY);
            logger?.LogInformation($"Current Position: {currentPositionInTheCircle:F2}");
            return currentPositionInTheCircle <= deadZone;
        }

        public static List<ZoneRange> InitCurrentZonesForQuadrant(double currentXArea, ref Quadrant currentQuadrant, ref ZoneRange currentZone, ref ZoneRange prevZone, ref bool initZones)
        {
            List<ZoneRange> zones = QuadrantCache.Quadrants[currentQuadrant];
            if (initZones)
            {
                var temp = QuadrantHelper.WhereAmI(zones, currentXArea);
                prevZone = temp;
                currentZone = temp;
                initZones = false;
            }

            return zones;
        }

        /// <summary>
        /// Keep track of the previous and current zone and if it changes didZoneChange will become true 
        /// </summary>
        public static void DetectZoneChange(double currentXArea, List<ZoneRange> zones, ref ZoneRange currentZone, ref ZoneRange prevZone, ref bool didZoneChange)
        {
            var tempZone = QuadrantHelper.WhereAmI(zones, currentXArea);
            if (currentZone != null && tempZone != currentZone)
            {
                prevZone = currentZone;
                currentZone = tempZone;
                didZoneChange = true;
            }
        }

        /// <summary>
        /// Keep track of the previous and current zone and if it changes didQuadrantChange will become true 
        /// </summary>
        public static void DetectQuadrantChange(int leftStickX, int leftStickY, ref Quadrant currentQuadrant, ref Quadrant prevQuadrant, ref bool didQuadrantChange)
        {
            var tempQuadrant = QuadrantHelper.WhereAmI(leftStickX, leftStickY);
            if (tempQuadrant != currentQuadrant)
            {
                prevQuadrant = currentQuadrant;
                currentQuadrant = tempQuadrant;
                didQuadrantChange = true;
            }
        }

        public static void FindArea(int threshold, int leftStickX, int leftStickY, out double angle, out double area)
        {
            angle = Math.Atan2(leftStickY, leftStickX);
            area = (angle / (2 * Math.PI)) * Math.PI * Math.Pow(threshold, 2);
        }

        public static void ExtractCurrentArea(int leftStickX, int leftStickY, out double currentXArea, Configurations configuration)
        {
            FindArea(configuration.LeftJoyStick.ThreshHoldAreaCal, Math.Abs(leftStickX), Math.Abs(leftStickY), out _, out currentXArea);
        }
    }
}

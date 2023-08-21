using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    public static class HallwayUtils
    {
        public static LineOrientation GetLineType(HallwayLineBase inputLine)
        {
            double epsilon = 0.016;
            if (Math.Abs(inputLine.startpoint.X - inputLine.endpoint.X) < epsilon)
                return LineOrientation.VERTICAL;
            else if (Math.Abs(inputLine.startpoint.Y - inputLine.endpoint.Y) < epsilon)
                return LineOrientation.HORIZONTAL;
            else
                return LineOrientation.INVALID;
        }

        /// <summary>
        ///  Check if the given two points are equal
        /// </summary>
        /// <param name="first">first point XYZ </param>
        /// <param name="second">second point XYZ</param>
        /// <param name="epsilon">precison required, default = 0.16</param>
        /// <returns>true or false</returns>
        public static bool AreAlmostEqual(XYZ first, XYZ second, double epsilon = 0.16f)
        {
            return Math.Abs(first.X - second.X) < epsilon &&
                   Math.Abs(first.Y - second.Y) < epsilon;
        }

        /// <summary>
        /// Check if the given two doble values are equal
        /// </summary>
        /// <param name="first">first double value</param>
        /// <param name="second">second double value</param>
        /// <param name="epsilon">precison required, default = 0.16</param>
        /// <returns>true or false</returns>
        public static bool AreAlmostEqual(double first, double second, double epsilon = 0.16f)
        {
            return Math.Abs(first - second) < epsilon;
        }

        /// <summary>
        /// check if the given line is horizontal
        /// </summary>
        /// <param name="line"></param>
        /// <returns>true if horizontal, else false</returns>
        public static bool IsLineHorizontal(Line line)
        {
            XYZ direction = line.Direction; // Get the direction of the line

            // Define a threshold value for the Y-component to account for slight deviations from a perfectly horizontal line
            double thresholdY = 0.01; // You can adjust this threshold based on your precision requirements

            // Check if the Y-component of the direction vector is close to 0
            if (Math.Abs(direction.Y) < thresholdY)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// check if the given line is vertical
        /// </summary>
        /// <param name="line"></param>
        /// <returns>true if vertical, else false</returns>
        public static bool IsLineVertical(Line line)
        {
            XYZ direction = line.Direction; // Get the direction of the line

            // Define a threshold value for the X-component to account for slight deviations from a perfectly vertical line
            double thresholdX = 0.01; // You can adjust this threshold based on your precision requirements

            // Check if the X-component of the direction vector is close to 0
            if (Math.Abs(direction.X) < thresholdX)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// checks if the lines are equal
        /// </summary>
        /// <param name="first"> first line </param>
        /// <param name="second"> second line</param>
        /// <param name="epsilon">precision ( fixed to 0.16 for now ) </param>
        /// <returns></returns>
        public static bool AreLinesEqual(HallwayLineBase first, HallwayLineBase second, double epsilon = 0.16f)
        {

            return Math.Abs(first.startpoint.X - second.startpoint.X) < epsilon &&
          Math.Abs(first.startpoint.Y - second.startpoint.Y) < epsilon &&
          Math.Abs(first.endpoint.X - second.endpoint.X) < epsilon &&
          Math.Abs(first.endpoint.Y - second.endpoint.Y) < epsilon;
        }
    }
}

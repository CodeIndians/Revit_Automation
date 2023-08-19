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
    }
}

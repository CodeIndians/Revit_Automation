/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source
{
    /// <summary>
    /// Commonly used Math routines
    /// </summary>
    internal class MathUtils
    {
        /// <summary>
        /// Given two lines, this method returns the intersection points. 
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        public static bool GetIntersectionPoint(PointF a1, PointF a2, PointF b1, PointF b2, out PointF intersection)
        {
            intersection = PointF.Empty;

            float delta = ((b2.Y - b1.Y) * (a2.X - a1.X)) - ((b2.X - b1.X) * (a2.Y - a1.Y));

            if (delta == 0)
                return false;

            float na = ((b2.X - b1.X) * (a1.Y - b1.Y)) - ((b2.Y - b1.Y) * (a1.X - b1.X));
            float nb = ((a2.X - a1.X) * (a1.Y - b1.Y)) - ((a2.Y - a1.Y) * (a1.X - b1.X));

            float ua = na / delta;
            float ub = nb / delta;

            if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
            {
                intersection = new PointF(a1.X + ua * (a2.X - a1.X), a1.Y + ua * (a2.Y - a1.Y));
                return true;
            }

            return false;
        }

        public static bool ApproximatelyEqual(double d1, double d2)
        {
            double precision = 0.0001;

            return (Math.Abs(d1 - d2) <= precision);
                
            
        }

        public static bool IsWithInRange (double reference, double high, double Low)
        {

            return (reference >= Low && reference <= high);
        }

        public static string CompareVectors(XYZ vector1, XYZ vector2)
        {
            // Calculate the dot product of the two vectors
            double dotProduct = vector1.DotProduct(vector2);

            // Compare the dot product to determine if vectors are parallel or anti-parallel
            if (Math.Abs(dotProduct - 1) < 1e-6)
            {
                return "Parallel";
            }
            else if (Math.Abs(dotProduct + 1) < 1e-6)
            {
                return "Anti-parallel";
            }
            else
            {
                return "Not parallel or anti-parallel";
            }
        }
    }
}

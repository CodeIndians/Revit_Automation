using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Revit_Automation.Source.Utils.WarningSwallowers;

namespace Revit_Automation.Source.Utils
{
    public class GenericUtils
    {

        public static double FlangeWidth(string strColumnName)
        {
            double width = 0;
            string token = "x";
            string[] result = strColumnName.Split(new string[] { token }, StringSplitOptions.None);

            if (result[1].Contains(" 1\""))
            {
                return 0.083333;
            }
            else if (result[1].Contains("1 1/2\""))
            {
                return 0.125;
            }
            else if (result[1].Contains(" 2\""))
            {
                return 0.166666;
            }
            else if (result[1].Contains("2 1/2\""))
            {
                return 0.208333;
            }
            else if (result[1].Contains(" 3\""))
            {
                return 0.25;
            }
            else if (result[1].Contains("3 1/2\""))
            {
                return 0.291666;
            }

            return width;
        }

        public static double WebWidth(string strColumnName)
        {
            double width = 0;
            string token = "x";
            string[] result = strColumnName.Split(new string[] { token }, StringSplitOptions.None);

            if (result[0].Contains("4\""))
            {
                return 0.333333;
            }
            else if (result[0].Contains("6\""))
            {
                return 0.5;
            }
            else if (result[0].Contains("8\""))
            {
                return 0.666666;
            }
            else if (result[0].Contains("2 1/2\""))
            {
                return 0.208333;
            }
            else if (result[0].Contains("3 5/8\""))
            {
                return 0.302083;
            }

            return width;

        }

        public static XYZ GetLineOrientation(Element continuousLine)
        {
            XYZ LineOrientation = null;

            LocationCurve locationCurve = (LocationCurve)continuousLine.Location;
            XYZ pt1 = locationCurve.Curve.GetEndPoint(0);
            XYZ pt2 = locationCurve.Curve.GetEndPoint(1);

            LineType lineType = LineType.vertical;

            if (MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y))
            {
                lineType = LineType.Horizontal;
            }

            if (lineType == LineType.Horizontal)
            {
                XYZ start = pt1.X > pt2.X ? pt2 : pt1;
                XYZ end = pt1.X > pt2.X ? pt1 : pt2;
                LineOrientation = end - start;
            }

            if (lineType == LineType.vertical)
            {
                XYZ start = pt1.Y > pt2.Y ? pt2 : pt1;
                XYZ end = pt1.Y > pt2.Y ? pt1 : pt2;
                LineOrientation = end - start;
            }

            return LineOrientation;
        }

        public static XYZ GetLineOrientation(InputLine inputLine)
        {
            XYZ LineOrientation = null;

            LocationCurve locationCurve = inputLine.locationCurve;
            XYZ pt1 = locationCurve.Curve.GetEndPoint(0);
            XYZ pt2 = locationCurve.Curve.GetEndPoint(1);

            LineType lineType = LineType.vertical;

            if (MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y))
            {
                lineType = LineType.Horizontal;
            }

            if (lineType == LineType.Horizontal)
            {
                XYZ start = pt1.X > pt2.X ? pt2 : pt1;
                XYZ end = pt1.X > pt2.X ? pt1 : pt2;
                LineOrientation = end - start;
            }

            if (lineType == LineType.vertical)
            {
                XYZ start = pt1.Y > pt2.Y ? pt2 : pt1;
                XYZ end = pt1.Y > pt2.Y ? pt1 : pt2;
                LineOrientation = end - start;
            }

            return LineOrientation;
        }

        public static void SupressWarningsInTransaction(Transaction tx)
        {
            FailureHandlingOptions failureHandlingOptions =
                                tx.GetFailureHandlingOptions();

            DuplicateColumnWarningSwallower duplicateColumnWarningSwallower =
              new DuplicateColumnWarningSwallower();

            _ = failureHandlingOptions.SetFailuresPreprocessor(
              duplicateColumnWarningSwallower);

            _ = failureHandlingOptions.SetClearAfterRollback(
              true);

            tx.SetFailureHandlingOptions(
              failureHandlingOptions);
        }

        public static void GetlineStartAndEndPoints(Element Line, out XYZ start, out XYZ end)
        {
            LocationCurve locationCurve = (LocationCurve)Line.Location;
            XYZ pt1 = locationCurve.Curve.GetEndPoint(0);
            XYZ pt2 = locationCurve.Curve.GetEndPoint(1);

            start = null; end = null;
            
            LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

            if (lineType == LineType.Horizontal)
            {
                start = pt1.X > pt2.X ? pt2 : pt1;
                end = pt1.X > pt2.X ? pt1 : pt2;
            }

            if (lineType == LineType.vertical)
            {
                start = pt1.Y > pt2.Y ? pt2 : pt1;
                end = pt1.Y > pt2.Y ? pt1 : pt2;
            }
        }
    }
}

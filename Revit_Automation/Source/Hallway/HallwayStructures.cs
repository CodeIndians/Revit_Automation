using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    /// <summary>
    /// Line Typen Enum
    /// This is horizontal, vertical or invalid 
    /// </summary>
    public enum LineType
    {
        HORIZONTAL,
        VERTICAL,
        INVALID
    }


    /// <summary>
    /// Class to collect the External Lines
    /// Two main members
    ///        1. Main External Input lines
    ///        2. List of perpendicular input lines that are intersecting with the main line
    /// Implements Comparable interface ( Used for sorting ) 
    /// </summary>
    public struct ExternalLine : IComparable<ExternalLine>
    {
        public InputLine mainExternalLine;
        public List<InputLine> intersectingInternalInputLines;

        public ExternalLine(InputLine line)
        {
            mainExternalLine = line;
            intersectingInternalInputLines = new List<InputLine>();
        }

        int IComparable<ExternalLine>.CompareTo(ExternalLine other)
        {
            double epsilon = 0.016; // precision

            double xDiff = mainExternalLine.start.X - other.mainExternalLine.start.X;
            if (Math.Abs(xDiff) > epsilon)
            {
                return (xDiff < 0) ? -1 : 1;
            }

            double yDiff = mainExternalLine.start.Y - other.mainExternalLine.start.Y;
            if (Math.Abs(yDiff) > epsilon)
            {
                return (yDiff < 0) ? -1 : 1;
            }

            return 0;
        }
    }

    /// <summary>
    /// Red input lines
    /// This will have start and end XYZ positions
    /// Implements Comparable(used for sorting) and Equatable(check if lines are equal)
    /// Has AreLinesIntersecting(...) function which will check if the line is interesecting
    ///     with the other provided line
    /// </summary>
    public struct InputLine : IComparable<InputLine>, IEquatable<InputLine>
    {
        public XYZ start;
        public XYZ end;

        public InputLine(XYZ startPoint, XYZ endPoint)
        {
            start = startPoint;
            end = endPoint;
            SortPoints();
        }


        /// <summary>
        /// Sort based on X first and then Y
        /// </summary>
        private void SortPoints()
        {
            double epsilon = 0.016; // precision

            if (start.X > end.X + epsilon || (Math.Abs(start.X - end.X) < epsilon && start.Y > end.Y + epsilon))
            {
                // Swap start and end points
                XYZ temp = start;
                start = end;
                end = temp;
            }
        }
        /// <summary>
        /// Swap start and end points, required for circular sorting 
        /// </summary>
        public void Swap()
        {
            // Swap start and end points
            XYZ temp = start;
            start = end;
            end = temp;
        }

        int IComparable<InputLine>.CompareTo(InputLine other)
        {
            double epsilon = 0.016; // precision

            double xDiff = start.X - other.start.X;
            if (Math.Abs(xDiff) > epsilon)
            {
                return (xDiff < 0) ? -1 : 1;
            }

            double yDiff = start.Y - other.start.Y;
            if (Math.Abs(yDiff) > epsilon)
            {
                return (yDiff < 0) ? -1 : 1;
            }

            return 0;
        }

        bool IEquatable<InputLine>.Equals(InputLine other)
        {
            double epsilon = 0.016; // precision

            return Math.Abs(start.X - other.start.X) < epsilon &&
          Math.Abs(start.Y - other.start.Y) < epsilon &&
          Math.Abs(end.X - other.end.X) < epsilon &&
          Math.Abs(end.Y - other.end.Y) < epsilon;
        }

        public bool AreLinesIntersecting(InputLine other)
        {

            if (PointUtils.AreEqual(this.start, other.start) || PointUtils.AreEqual(this.start, other.end) || PointUtils.AreEqual(this.end, other.start) || PointUtils.AreEqual(this.end, other.end))
                return true;

            // perpendicular lines
            if (GetLineType(this) == LineType.HORIZONTAL && GetLineType(other) == LineType.VERTICAL)
            {
                return LineUtils.AreIntersecting(this, other);
            }
            else if (GetLineType(this) == LineType.VERTICAL && GetLineType(other) == LineType.HORIZONTAL)
            {
                return LineUtils.AreIntersecting(other, this);
            }

            return false;
        }

        /// <summary>
        /// Get the current line type
        /// </summary>
        /// <param name="inputLine"></param>
        /// <returns>HORIZONTAL VERTICAL or INVALID</returns>
        public static LineType GetLineType(InputLine inputLine)
        {
            double epsilon = 0.016;
            if (Math.Abs(inputLine.start.X - inputLine.end.X) < epsilon)
                return LineType.VERTICAL;
            else if (Math.Abs(inputLine.start.Y - inputLine.end.Y) < epsilon)
                return LineType.HORIZONTAL;
            else
                return LineType.INVALID;
        }

    }
}

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class ExternalPointsGenerator
    {
        // generated mPoints
        private List<XYZ> mPoints;

        //external hallway lines
        public List<InputLine> mExternalHallwayLines;

        // external input lines
        private List<InputLine> mExternalLines;

        //external hatch rects ( contains four points) 
        private List<List<XYZ>> mExternalHatchRects;

        // external hatch rects ( contains four lines ) 
        private List<List<InputLine>> mExternalHatchLines;

        // These are the bounds for the current floor ( top,bottom,left,right)
        private Boundaries mBounds;

        public struct Boundaries
        {
            public double top;
            public double bottom;
            public double left;
            public double right;
        }

        /// <summary>
        /// Takes in external hatch rects and external input lines as input
        /// Validates if the rects are proper
        /// Computes 
        /// </summary>
        /// <param name="externalRectLines"></param>
        /// <param name="extLines"></param>
        public ExternalPointsGenerator(List<List<InputLine>> externalRectLines, List<ExternalLine> extLines)
        {
            //initialize points 
            mPoints = new List<XYZ>();

            // initialize lines
            mExternalHallwayLines = new List<InputLine>();

            mExternalHatchRects = new List<List<XYZ>>();
            foreach (var line in externalRectLines)
            {
                mExternalHatchRects.Add(GetRectPoints(line));
            }

            mExternalHatchLines = externalRectLines;

            mExternalLines = new List<InputLine>();
            foreach (var line in extLines)
            {
                // capture only the main external lines, intersections are ignored
                mExternalLines.Add(line.mainExternalLine);
            }

            if (!Validate())
            {
                TaskDialog.Show("Error", "External RECT validation failed");
            }

            CreateBoundaries();

            Generate();


            FileWriter.WritePointListtoFile(mExternalHatchRects, @"C:\temp\external_rects");
            FileWriter.WritePointListtoFile(mPoints, @"C:\temp\final_points");
            FileWriter.WriteInputListToFile(mExternalLines, @"C:\temp\external_lines");
        }

        private bool Validate()
        {
            //check if all the external rects are of size 4 
            foreach (var rect in mExternalHatchRects)
            {
                if (rect.Count != 4)
                    return false;
            }
            return true;
        }

        private void CreateBoundaries()
        {
            // initialize the bounds
            mBounds.top = Double.NegativeInfinity;
            mBounds.bottom = Double.PositiveInfinity;
            mBounds.left = Double.PositiveInfinity;
            mBounds.right = Double.NegativeInfinity;


            foreach (var extLine in mExternalLines)
            {
                var lineType = InputLine.GetLineType(extLine);
                if (lineType == LineType.HORIZONTAL)
                {
                    mBounds.left = Math.Min(mBounds.left, extLine.start.X);
                    mBounds.right = Math.Max(mBounds.right, extLine.end.X);
                }
                else if (lineType == LineType.VERTICAL)
                {
                    mBounds.bottom = Math.Min(mBounds.bottom, extLine.start.Y);
                    mBounds.top = Math.Max(mBounds.top, extLine.end.Y);
                }
            }
        }

        private void Generate()
        {
            List<List<InputLine>> intersectingRects = new List<List<InputLine>>();

            // iterate through all the external lines
            foreach (var extLine in mExternalLines)
            {
                intersectingRects.Clear();
                intersectingRects = GetIntersectingRect(extLine);
            }

            // Filter the external hatch points and collect them into mPoints
            FilterExternalPoints();

            //Generate External Hallway lines from Points
            GenerateExternalHallwayLines();

        }

        /// <summary>
        /// Returns the intersecting rect lines with the given external input line
        /// TODO: might not be used. This can be removed it this is not required.
        /// </summary>
        /// <param name="extLine"> takes external line as input</param>
        /// <returns>List of intersecting rects</returns>
        private List<List<InputLine>> GetIntersectingRect(InputLine extLine)
        {
            List<List<InputLine>> intersectingRects = new List<List<InputLine>>();

            foreach (var rect in mExternalHatchLines)
            {
                foreach (var line in rect)
                {
                    if (extLine.AreLinesIntersecting(line))
                    {
                        intersectingRects.Add(rect);
                        break;
                    }
                }
            }

            return intersectingRects;
        }

        private List<XYZ> GetRectPoints(List<InputLine> rectLines)
        {
            var points = new List<XYZ>();
            foreach (var line in rectLines)
            {
                if(PointUtils.IndexOf(points,line.start) == -1)
                    points.Add(line.start);
                if (PointUtils.IndexOf(points, line.end) == -1)
                    points.Add(line.end);
            }

            return points.ToList<XYZ>();
        }

        /// <summary>
        /// Valid hatch points are filtered based on the below conditions
        ///     1. External line condition ( point should not be at the end and start of the line) 
        ///     2. Hatch intersecting points are removed
        /// </summary>
        private void FilterExternalPoints()
        {
            // iterate through all of the external points
            foreach(var pointList in mExternalHatchRects)
            {
                foreach(var point  in pointList)
                {
                    if(!CheckExternalLineIntersectCondition(point) && CheckExternaHatchIntersetConditon(point) == 1)
                    {
                        mPoints.Add(point);
                    }
                }
            }
        }

        /// <summary>
        /// Check if the given point is at the end or the start of any of the external input lines 
        /// </summary>
        /// <param name="point"></param>
        /// <returns>true if the point falls on start or end of any of the external input line other wise false</returns>
        private bool CheckExternalLineIntersectCondition(XYZ point)
        {
            foreach (var extLine in mExternalLines)
            {
                if (PointUtils.AreAlmostEqual(point, extLine.start) || PointUtils.AreAlmostEqual(point,extLine.end))
                {
                    return true;
                }
            }
            return false;
        }

        private int CheckExternaHatchIntersetConditon(XYZ point)
        {
            int count = 0;

            foreach (var extHatchPointList in mExternalHatchRects)
            {
                foreach( var pt in extHatchPointList)
                {
                    if(PointUtils.AreAlmostEqual(pt,point,1.0f))
                        count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Creates horizontal and vertical lines from the generated external points
        /// </summary>
        private void GenerateExternalHallwayLines()
        {
            var horLines = GenerateHorizontalLines();
            var verLines = GenerateVerticalLines();

            mExternalHallwayLines.AddRange(horLines);
            mExternalHallwayLines.AddRange(verLines);

            mExternalHallwayLines.Sort();

            //FileWriter.WriteInputListToFile(mExternalHallwayLines, @"C:\temp\external_hallway_lines");
        }

        /// <summary>
        /// Creates horizontal lines from the generated external points
        /// </summary>
        /// <returns>Horizontal lines from the generated external points</returns>
        private List<InputLine> GenerateHorizontalLines()
        {
            var horizontalPointsGroup = new List<List<XYZ>>();

            var horizontalLines = new List<InputLine>();

            horizontalPointsGroup = mPoints.GroupBy(p => Math.Round(p.Y * 2) / 2.0)
                                           .Select(g => g.ToList())
                                           .ToList();

            //FileWriter.WritePointListtoFile(horizontalPointsGroup, @"C:\temp\horizontal_point_group");

            foreach (var pointList in horizontalPointsGroup)
            {
                if(pointList.Count <= 1)
                    continue;
                double startX = Double.PositiveInfinity;
                double endX = Double.NegativeInfinity;
                double Y = pointList[0].Y;
                double Z = pointList[0].Z;
                foreach (var point in pointList)
                {
                    startX = Math.Min(point.X, startX);
                    endX = Math.Max(point.X, endX);
                }

                horizontalLines.Add(new InputLine(new XYZ(startX,Y,Z), new XYZ(endX,Y,Z)));
            }

            //FileWriter.WriteInputListToFile(horizontalLines, @"C:\temp\horizontal_hallway_lines");

            return horizontalLines;
        }

        /// <summary>
        ///  Creates vertical lines from the generated external points
        /// </summary>
        /// <returns> Vertical lines from the generated external points</returns>
        private List<InputLine> GenerateVerticalLines()
        {

            var verticalPointsGroup = new List<List<XYZ>>();

            var verticalLines = new List<InputLine>();

            verticalPointsGroup = mPoints.GroupBy(p => Math.Round(p.X * 2) / 2.0)
                                           .Select(g => g.ToList())
                                           .ToList();

            //FileWriter.WritePointListtoFile(verticalPointsGroup, @"C:\temp\vertical_point_group");

            foreach (var pointList in verticalPointsGroup)
            {
                if (pointList.Count <= 1)
                    continue;
                double startY = Double.PositiveInfinity;
                double endY = Double.NegativeInfinity;
                double X = pointList[0].X;
                double Z = pointList[0].Z;
                foreach (var point in pointList)
                {
                    startY = Math.Min(point.Y, startY);
                    endY = Math.Max(point.Y, endY);
                }

                verticalLines.Add(new InputLine(new XYZ(X, startY, Z), new XYZ(X, endY, Z)));
            }

            //FileWriter.WriteInputListToFile(verticalLines, @"C:\temp\vertical_hallway_lines");

            return verticalLines;
        }

    }
}

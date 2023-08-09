using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

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


            //FileWriter.WritePointListtoFile(mExternalHatchRects, @"C:\temp\external_rects");
            //FileWriter.WritePointListtoFile(mPoints, @"C:\temp\final_points");
            //FileWriter.WriteInputListToFile(mExternalLines, @"C:\temp\external_lines");
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

        /// <summary>
        /// Create the boundaries of the current floor plan
        /// Not used anywhere currently and can be removed
        /// </summary>
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
            // iterate through all of the external hatch rect points
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

            // collect non-hatch intersecting points on the external lines
            foreach(var extLine in mExternalLines)
            {
                // add start point if it satisfies the following conditions
                if(PointUtils.IndexOf(mPoints,extLine.start) == -1) // the point is not already present
                {
                    if(GetHatchIntersectIndex(extLine.start) == -1) // the point does not fall on any of the external hatch rects
                    {
                        mPoints.Add(extLine.start);
                    }
                }

                // add end point if it satisfies the following conditions
                if (PointUtils.IndexOf(mPoints, extLine.end) == -1) // the point is not already present
                {
                    if (GetHatchIntersectIndex(extLine.end) == -1) // the point does not fall on any of the external hatch rects
                    {
                        mPoints.Add(extLine.end);
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

            // group points based on the same Y co-ordinate
            horizontalPointsGroup = mPoints.GroupBy(p => Math.Round(p.Y * 2) / 2.0)
                                           .Select(g => g.ToList())
                                           .ToList();

            // sort the inner lists based on the X cordinate
            LineUtils.SortByXCoordinate(horizontalPointsGroup);

            //FileWriter.WritePointListtoFile(horizontalPointsGroup, @"C:\temp\horizontal_point_group");

            foreach (var pointList in horizontalPointsGroup)
            {
                // dont add any lines if there are no enough points
                if(pointList.Count <= 1)
                    continue;
                
                double Y = pointList[0].Y;
                double Z = pointList[0].Z;


                for (int i = 0; i < pointList.Count -1; i++)
                {
                    double startX = pointList[i].X;
                    double endX = pointList[i+1].X;

                    var startHatchIntersectIndex = GetHatchIntersectIndex(new XYZ(startX, Y, Z));
                    var endHatchIntersectIndex = GetHatchIntersectIndex(new XYZ(endX, Y, Z));

                    // points are not falling on the same hatch
                    if (startHatchIntersectIndex != endHatchIntersectIndex)
                    {
                        // we need to keep the line which falls on the external line
                        if (!IsFallingOnHorizontalExternalLine(new XYZ(startX, Y, Z)))
                        {
                            // corner hatche open condition . skip the lines that do not fall on the external lines.
                            if (startHatchIntersectIndex == -1 || endHatchIntersectIndex == -1)
                                continue;

                            // if the hatches are not intersecting, ignore the line
                            if (!AreHatchRectsIntersecting(mExternalHatchRects[startHatchIntersectIndex], mExternalHatchRects[endHatchIntersectIndex]))
                                continue;
                        }
                    }

                    // points are falling on the same hatch
                    if(startHatchIntersectIndex == endHatchIntersectIndex && startHatchIntersectIndex != -1)
                    {
                        //remove the line if it is falling on the external line
                        if (IsFallingOnHorizontalExternalLine(new XYZ(startX, Y, Z)))
                            continue;
                    }

                    horizontalLines.Add(new InputLine(new XYZ(startX, Y, Z), new XYZ(endX, Y, Z)));
                }
            }

            //TODO: Can be removed if the breakpoints are not hitting
            // filter the lines and join the lines after filtering them
            FilterAndJoinHorizontalLines(ref horizontalLines);

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

            // group points based on the same X coordinate
            verticalPointsGroup = mPoints.GroupBy(p => Math.Round(p.X * 2) / 2.0)
                                           .Select(g => g.ToList())
                                           .ToList();

            // sort the inner lists based on Y cordinate
            LineUtils.SortByYCoordinate(verticalPointsGroup);

            //FileWriter.WritePointListtoFile(verticalPointsGroup, @"C:\temp\vertical_point_group");

            foreach (var pointList in verticalPointsGroup)
            {
                // dont add any lines if there are no enough points
                if (pointList.Count <= 1)
                    continue;
               
                double X = pointList[0].X;
                double Z = pointList[0].Z;

                for (int i = 0; i < pointList.Count - 1; i++)
                {
                    double startY = pointList[i].Y;
                    double endY = pointList[i+1].Y;

                    var startHatchIntersectIndex = GetHatchIntersectIndex(new XYZ(X, startY, Z));
                    var endHatchIntersectIndex = GetHatchIntersectIndex(new XYZ(X, endY, Z));

                    // points are not falling on the same hatch
                    if (startHatchIntersectIndex != endHatchIntersectIndex)
                    {
                        // we need to keep the line which falls on the external line
                        if (!IsFallingOnVerticalExternalLine(new XYZ(X, startY, Z)))
                        {
                            // corner hatche open condition . skip the lines that do not fall on the external lines.
                            if (startHatchIntersectIndex == -1 || endHatchIntersectIndex == -1)
                                continue;

                            // if the hatches are not intersecting, ignore the line
                            if (!AreHatchRectsIntersecting(mExternalHatchRects[startHatchIntersectIndex], mExternalHatchRects[endHatchIntersectIndex]))
                                continue;
                        }
                    }

                    // points are falling on the sam hatch
                    if (startHatchIntersectIndex == endHatchIntersectIndex && startHatchIntersectIndex != -1)
                    {
                        //remove the line if it is falling on the external line
                        if (IsFallingOnVerticalExternalLine(new XYZ(X, startY, Z)))
                            continue;
                    }

                    verticalLines.Add(new InputLine(new XYZ(X, startY, Z), new XYZ(X, endY, Z)));
                }

                //TODO: Can be removed if the breakpoints are not hitting
                // filter the lines and join the lines after filtering them
                FilterAndJoinVerticalLines(ref verticalLines);
            }

            //FileWriter.WriteInputListToFile(verticalLines, @"C:\temp\vertical_hallway_lines");

            return verticalLines;
        }

        /// <summary>
        /// Step1: Identify the parallel lines with same start.X and end.X
        /// Step2: Check if they are intersecting with the same different hatches
        /// Step3: If yes, delete the line that does not fall on the external line
        /// </summary>
        /// <param name="horLines"></param>
        private void FilterAndJoinHorizontalLines(ref List<InputLine> horLines)
        {
            int currentIndex = 0;


            while (currentIndex < horLines.Count)
            {
                // assign the first line as the current index
                var firstLine = horLines[currentIndex];

                int indexToDelete = -1;

                // start from after the current index 
                for (int i = currentIndex + 1; i < horLines.Count; i++)
                {
                    var secondLine = horLines[i];
                    // check the parallel lines with same start.X and end.X condition
                    if(PointUtils.AreAlmostEqual(firstLine.start.X, secondLine.start.X) 
                        && PointUtils.AreAlmostEqual(firstLine.end.X,secondLine.end.X))
                    {
                        //step one is satisfied. we now have lines which start and end at same X positions 

                        var firstStartHatchIndex = GetHatchIntersectIndex(firstLine.start);
                        var firstEndHatchIndex = GetHatchIntersectIndex(firstLine.end);

                        var secondStartHatchIndex = GetHatchIntersectIndex(secondLine.start);
                        var secondEndHatchIndex = GetHatchIntersectIndex(secondLine.end);

                        if((firstStartHatchIndex == secondStartHatchIndex)
                            && (firstEndHatchIndex == secondEndHatchIndex)
                            && (firstStartHatchIndex != firstEndHatchIndex))
                        {
                            // We will have to delete the line that is not falling on the external line in this case
                            if(IsFallingOnHorizontalExternalLine(firstLine.start) && IsFallingOnHorizontalExternalLine(firstLine.end))
                            {
                                // delete the second line
                                indexToDelete = i;

                                //assigning the first line to the current index is not needed
                                break;
                            }
                            else if (IsFallingOnHorizontalExternalLine(secondLine.start) && IsFallingOnHorizontalExternalLine(secondLine.end))
                            {
                                // delete the second line 
                                indexToDelete = i;

                                // assign the second line to the first line 
                                horLines[currentIndex] = secondLine;

                                break;
                            }
                            else
                            {
                                TaskDialog.Show("Error", "Found a line deletion case which was not anticipated");
                            }

                        }
                    }
                }

                if(indexToDelete != -1)
                {
                    horLines.RemoveAt(indexToDelete);
                }
                // doing this for every iteration ( assuming that we only have two such lines 
                currentIndex++;
            }

        }

        /// <summary>
        /// Step1: Identify the parallel lines with same start.Y and end.Y
        /// Step2: Check if they are intersecting with the same different hatches
        /// Step3: If yes, delete the line that does not fall on the external line
        /// </summary>
        /// <param name="horLines"></param>
        private void FilterAndJoinVerticalLines(ref List<InputLine> horLines)
        {
            int currentIndex = 0;


            while (currentIndex < horLines.Count)
            {
                // assign the first line as the current index
                var firstLine = horLines[currentIndex];

                int indexToDelete = -1;

                // start from after the current index 
                for (int i = currentIndex + 1; i < horLines.Count; i++)
                {
                    var secondLine = horLines[i];
                    // check the parallel lines with same start.X and end.X condition
                    if (PointUtils.AreAlmostEqual(firstLine.start.Y, secondLine.start.Y)
                        && PointUtils.AreAlmostEqual(firstLine.end.Y, secondLine.end.Y))
                    {
                        //step one is satisfied. we now have lines which start and end at same X positions 

                        var firstStartHatchIndex = GetHatchIntersectIndex(firstLine.start);
                        var firstEndHatchIndex = GetHatchIntersectIndex(firstLine.end);

                        var secondStartHatchIndex = GetHatchIntersectIndex(secondLine.start);
                        var secondEndHatchIndex = GetHatchIntersectIndex(secondLine.end);


                        // TODO: This condition will never be hit and can be removed
                        if ((firstStartHatchIndex == secondStartHatchIndex)
                            && (firstEndHatchIndex == secondEndHatchIndex)
                            && (firstStartHatchIndex != firstEndHatchIndex))
                        {
                            // We will have to delete the line that is not falling on the external line in this case
                            if (IsFallingOnVerticalExternalLine(firstLine.start) && IsFallingOnVerticalExternalLine(firstLine.end))
                            {
                                // delete the second line
                                indexToDelete = i;

                                //assigning the first line to the current index is not needed
                                break;
                            }
                            else if (IsFallingOnVerticalExternalLine(secondLine.start) && IsFallingOnVerticalExternalLine(secondLine.end))
                            {
                                // delete the second line 
                                indexToDelete = i;

                                // assign the second line to the first line 
                                horLines[currentIndex] = secondLine;

                                break;
                            }
                            else
                            {
                                TaskDialog.Show("Error", "Found a line deletion case which was not anticipated");
                            }

                        }
                    }
                }

                if (indexToDelete != -1)
                {
                    horLines.RemoveAt(indexToDelete);
                }
                // doing this for every iteration ( assuming that we only have two such lines 
                currentIndex++;
            }

        }

        private int GetHatchIntersectIndex(XYZ point)
        {
            int index = -1;

            for(int i = 0; i < mExternalHatchRects.Count; i++)
            {
                foreach (var rectPoint in mExternalHatchRects[i])
                {
                    if (PointUtils.AreAlmostEqual(point, rectPoint))
                    {
                        return i;
                    }
                }
            }
            return index;
        }

        private bool IsFallingOnHorizontalExternalLine(XYZ point)
        {
            foreach(var line in mExternalLines)
            {
                if(InputLine.GetLineType(line) == LineType.HORIZONTAL)
                {
                    if(Math.Abs(line.start.Y - point.Y) < 0.5f)
                    {
                        if((line.start.X < point.X || Math.Abs(line.start.X - point.X) < 0.5) && (line.end.X >point.X || Math.Abs(line.end.X - point.X) < 0.5))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool IsFallingOnVerticalExternalLine(XYZ point)
        {
            foreach (var line in mExternalLines)
            {
                if (InputLine.GetLineType(line) == LineType.VERTICAL)
                {
                    if (Math.Abs(line.start.X - point.X) < 0.5f)
                    {
                        if ((line.start.Y < point.Y || Math.Abs(line.start.Y - point.Y) < 0.5) && (line.end.Y > point.Y || Math.Abs(line.end.Y - point.Y) < 0.5))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool AreHatchRectsIntersecting(List<XYZ> firstRect, List<XYZ> secondRect)
        {
            bool found = false;

            foreach (var rectFirst in firstRect)
            {
                foreach (var rectSecond in secondRect)
                {
                    if (PointUtils.AreAlmostEqual(rectFirst, rectSecond))
                    {
                        return true;
                    }
                }
            }

            return found;
        }
    }
}

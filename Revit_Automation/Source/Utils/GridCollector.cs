/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

using Autodesk.Revit.DB;
using Revit_Automation.Source;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Document = Autodesk.Revit.DB.Document;

namespace Revit_Automation
{
    internal class GridCollector
    {
        private readonly Document mDocument;
        public static List<Tuple<XYZ, XYZ>> mHorizontalMainLines;
        public static List<Tuple<XYZ, XYZ>> mVerticalMainLines;

        public ICollection<Element> GridCollection { get; private set; }
        public List<Tuple<XYZ, XYZ>> HorizontalLines { get; private set; }
        public List<Tuple<XYZ, XYZ>> VerticalLines { get; private set; }


        public GridCollector(Document doc)
        {
            mDocument = doc;

            if (HorizontalLines == null && VerticalLines == null)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Collects all the grids in the given document and segregates them into horizontal and vertical lines
        /// Also segregates Main and non-main grid lines
        /// </summary>
        private void Initialize()
        {
            FilteredElementCollector gridCollector = new FilteredElementCollector(mDocument);
            GridCollection = gridCollector.OfCategory(BuiltInCategory.OST_Grids).ToElements();

            List<Tuple<XYZ, XYZ>> gridLines = new List<Tuple<XYZ, XYZ>>();
            List<Tuple<XYZ, XYZ>> mainGridLines = new List<Tuple<XYZ, XYZ>>();
            // collect each line into a gridline tuple 
            foreach (Element element in GridCollection)
            {
                Parameter archParameter = element.LookupParameter("Arch Grid");

                if (archParameter != null)
                {
                    int value = archParameter.AsInteger();
                    // Skip if Arch Grid is checkmarked
                    if (value == 1)
                    {
                        continue;
                    }
                }


                // add the tuple grid lines
                if (element is Grid grid)
                {

                    Tuple<XYZ, XYZ> pair = Tuple.Create(grid.Curve.GetEndPoint(0), grid.Curve.GetEndPoint(1));
                    gridLines.Add(pair);

                    Parameter mainGridParameter = element.LookupParameter("Main Grid");
                    if (mainGridParameter != null)
                    {
                        int value = mainGridParameter.AsInteger();
                        // Skip if Main Grid is not checkmarked
                        if (value == 1)
                        {
                            mainGridLines.Add(pair);
                        }
                    }
                }
            }

            double precision = 0.0001;

            // Collect Horizontal and Vertical Lines
            HorizontalLines = gridLines.Where(pair => Math.Abs(pair.Item1.Y - pair.Item2.Y) < precision).ToList().OrderBy(pair => pair.Item1.Y).ToList();
            VerticalLines = gridLines.Where(pair => Math.Abs(pair.Item1.X - pair.Item2.X) < precision).ToList().OrderBy(pair => pair.Item1.X).ToList();

            // Lines that are marked as main grids;
            mHorizontalMainLines = mainGridLines.Where(pair => Math.Abs(pair.Item1.Y - pair.Item2.Y) < precision).ToList().OrderBy(pair => pair.Item1.Y).ToList();
            mVerticalMainLines = mainGridLines.Where(pair => Math.Abs(pair.Item1.X - pair.Item2.X) < precision).ToList().OrderBy(pair => pair.Item1.X).ToList();
        }

        /// <summary>
        /// Validates if the grid lines are equidistant
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {

            double precision = 0.0001;
            bool isEquidistant = true;

            // Check consecutive horizontal lines
            for (int i = 0; i < HorizontalLines.Count - 1; i++)
            {
                double distance = HorizontalLines[i + 1].Item1.X - HorizontalLines[i].Item1.X;
                if (Math.Abs(distance - (HorizontalLines[i].Item2 - HorizontalLines[i].Item1).GetLength()) > precision)
                {
                    isEquidistant = false;
                    break;
                }
            }

            if (isEquidistant)
            {
                // Check consecutive vertical lines
                for (int i = 0; i < VerticalLines.Count - 1; i++)
                {
                    double distance = VerticalLines[i + 1].Item1.Y - VerticalLines[i].Item1.Y;
                    if (Math.Abs(distance - (VerticalLines[i].Item2 - VerticalLines[i].Item1).GetLength()) > precision)
                    {
                        isEquidistant = false;
                        break;
                    }
                }
            }

            return isEquidistant;
        }

        /// <summary>
        /// This method is used to compute the intersection points between the grids and given Input line
        /// </summary>
        /// <param name="linecoords">[in] the coordinates of line to find intesection with grids</param>
        /// <returns></returns>
        internal List<XYZ> computeIntersectionPoints(Tuple<XYZ, XYZ> linecoords, bool bMain = false)
        {

            List<XYZ> colintesectPoints = new List<XYZ>();

            XYZ lineStart = linecoords.Item1;
            XYZ lineEnd = linecoords.Item2;

            LineType lineType = MathUtils.ApproximatelyEqual(lineStart.Y, lineEnd.Y) ? LineType.Horizontal : LineType.vertical;

            List<Tuple<XYZ, XYZ>> mGridLinesToIntersect = bMain ? mHorizontalMainLines : HorizontalLines;

            if (MathUtils.ApproximatelyEqual(lineStart.Y, lineEnd.Y))
            {
                mGridLinesToIntersect = bMain ? mVerticalMainLines : VerticalLines;
            }


            foreach (Tuple<XYZ, XYZ> GridlinetoIntersect in mGridLinesToIntersect)
            {
                bool bInsersects = MathUtils.GetIntersectionPoint(new PointF((float)lineStart.X, (float)lineStart.Y),
                                                                    new PointF((float)lineEnd.X, (float)lineEnd.Y),
                                                                    new PointF((float)GridlinetoIntersect.Item1.X, (float)GridlinetoIntersect.Item1.Y),
                                                                    new PointF((float)GridlinetoIntersect.Item2.X, (float)GridlinetoIntersect.Item2.Y),
                                                                    out PointF ptIntesectionPoint);

                if (bInsersects)
                {
                    XYZ intesectPoint = new XYZ(ptIntesectionPoint.X, ptIntesectionPoint.Y, linecoords.Item1.Z);
                    colintesectPoints.Add(intesectPoint);
                }
            }

            List<XYZ> sortedPoints = new List<XYZ>();

            sortedPoints = lineType == LineType.Horizontal ? colintesectPoints.OrderBy(p => p.X).ToList() : colintesectPoints.OrderBy(p => p.Y).ToList();

            return sortedPoints;
        }
    }
}

using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Revit_Automation.Source.Hallway
{
    internal class HallwayTrim
    {
        private static Document mDocument;

        private List<HallwayLabelLine> mHorLabelLines;

        private List<HallwayLabelLine> mVertLabelLines;

        public static bool bOnButtonClick = false;

        private List<TrimLineInfo> mTrimLineInfos;

        public enum TrimType
        {
            Top,
            Bottom,
            Left,
            Right,
            None
        }

        public class TrimLineInfo
        {
            public HallwayLabelLine mLabelLine;

            public List<Element> mIntersectingLines;

            public TrimType mTrimType;

            public int mTrimValue;

            public TrimLineInfo(HallwayLabelLine labelLine)
            {
                mLabelLine = labelLine;
                mIntersectingLines = GetHallwayIntersectingInternalLines(labelLine);

                var lineType = HallwayUtils.GetLineType(labelLine.mLines[0]);

                // set trim type based on the line orientation
                if (lineType == LineOrientation.HORIZONTAL)
                {
                    foreach (DataRow row in HallwayTrimData.TrimDataHorizontal.Rows)
                    {
                        if (row[0].ToString() == labelLine.mLabel)
                        {
                            int top = int.Parse((row[1]).ToString());
                            int bottom = int.Parse((row[2]).ToString());

                            if (top == 0 && bottom == 0)
                            {
                                mTrimType = TrimType.None;
                                mTrimValue = 0;
                            }
                            else if (top == 0)
                            {
                                mTrimType = TrimType.Bottom;
                                mTrimValue = bottom;
                            }
                            else if (bottom == 0)
                            {
                                mTrimType = TrimType.Top;
                                mTrimValue = top;
                            }
                            //other invalid cases are handled in the data validation part
                        }
                    }
                }
                else if (lineType == LineOrientation.VERTICAL)
                {
                    foreach (DataRow row in HallwayTrimData.TrimDataVertical.Rows)
                    {
                        if (row[0].ToString() == labelLine.mLabel)
                        {
                            int left = int.Parse((row[1]).ToString());
                            int right = int.Parse((row[2]).ToString());

                            if (left == 0 && right == 0)
                            {
                                mTrimType = TrimType.None;
                                mTrimValue = 0;
                            }
                            else if (left == 0)
                            {
                                mTrimType = TrimType.Right;
                                mTrimValue = right;
                            }
                            else if (right == 0)
                            {
                                mTrimType = TrimType.Left;
                                mTrimValue = left;
                            }
                            //other invalid cases are handled in the data validation part
                        }
                    }
                }
            }

        }

        // TODO: hard coding this for now. This has to be taken from project settings.
        private double HPT = 1.0f;


        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="doc"> DB document reference</param>
        /// <param name="horLabelLines"> Horizontal label lines </param>
        /// <param name="verLabelLines">Vertical lable lines</param>
        public HallwayTrim(ref Document doc, List<HallwayLabelLine> horLabelLines, List<HallwayLabelLine> verLabelLines)
        {
            mDocument = doc;

            // these are the hallway label lines
            mHorLabelLines = horLabelLines;
            mVertLabelLines = verLabelLines;

            // trim related data is stored here
            mTrimLineInfos = new List<TrimLineInfo>();


            CollectTrimData();

            TrimHallwayIntersectingLines();

        }

        private void CollectTrimData()
        {
            foreach (HallwayLabelLine line in mHorLabelLines)
            {
                mTrimLineInfos.Add(new TrimLineInfo(line));
            }

            foreach (HallwayLabelLine line in mVertLabelLines)
            {
                mTrimLineInfos.Add(new TrimLineInfo(line));
            }

            //FileWriter.WriteTrimLineInfoToFile(mTrimLineInfos, @"C:\temp\trim_info");
        }

        /// <summary>
        ///  Get lines that are intersecting with the given lable line set ( Hx set or Vx set)
        /// </summary>
        /// <param name="labelLine"></param>
        /// <returns>List of elements which are falling on the current line</returns>
        static private List<Element> GetHallwayIntersectingInternalLines(HallwayLabelLine labelLine)
        {
            var internalIntersectingLines = new List<Element>();

            foreach (var inputLine in labelLine.mLines)
            {
                if (HallwayUtils.GetLineType(inputLine) == LineOrientation.HORIZONTAL
                    || HallwayUtils.GetLineType(inputLine) == LineOrientation.VERTICAL)
                {
                    // PRECISION: change this if required
                    double intersectOffset = 0.05;

                    // Create a Outline, uses a minimum and maximum XYZ point to initialize the Bounding Box. 
                    Outline myOutLn = new Outline(
                        new XYZ(inputLine.startpoint.X - intersectOffset,
                        inputLine.startpoint.Y - intersectOffset,
                        inputLine.startpoint.Z - intersectOffset),
                        new XYZ(inputLine.endpoint.X + intersectOffset,
                        inputLine.endpoint.Y + intersectOffset,
                        inputLine.endpoint.Z + intersectOffset));

                    // Create a BoundingBoxIntersects filter with this Outline
                    BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);

                    // Apply the filter to the elements in the active document to all the input lines
                    FilteredElementCollector collector = new FilteredElementCollector(mDocument);
                    IList<Element> lineElements = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();

                    foreach (var lineElement in lineElements)
                    {
                        // exclude external lines 
                        if (!lineElement.LookupParameter("Wall Type").AsString().Contains("Ex")) 
                            internalIntersectingLines.Add(lineElement);
                    }
                }
            }

            return internalIntersectingLines;
        }

        /// <summary>
        /// Trim the intersecting lines based on the trim data
        /// </summary>
        public void TrimHallwayIntersectingLines()
        {
            // PRECISION: change this if required
            double precision = 0.05f;

            using (Transaction trans = new Transaction(mDocument, "Trimming Hallway lines"))
            {
                trans.Start("Trimming Hallway Intersections");
                foreach (var trimLineInfo in mTrimLineInfos)
                {
                    // skip trimming if there trimming is not opted in the form
                    // or if the number of intersecting lines is zero
                    if (trimLineInfo.mTrimType == TrimType.None
                            || trimLineInfo.mLabelLine.mLines.Count == 0)
                        continue;

                    // trim value from the form ( 1 or 2 ) munltiplied by HPT(project settings) / 12 ( for feet to inch conversion) 
                    double offset = (trimLineInfo.mTrimValue * HPT) / 12.0;  

                    // compute the move vector for all the four possible scenarios ( left, right, top, bottom ) 
                    XYZ moveYvector = new XYZ(0.0, (trimLineInfo.mTrimType == TrimType.Top) ? (offset) : -(offset), 0.0);
                    XYZ moveXvector = new XYZ((trimLineInfo.mTrimType == TrimType.Right) ? (offset) : -(offset), 0.0, 0.0);

                    // trim each of the intersecting line
                    // parallel and perpendicular lines are handled separately
                    foreach (var lineElement in trimLineInfo.mIntersectingLines)
                    {
                        // extract the location curve
                        LocationCurve locationCurve = lineElement.Location as LocationCurve;

                        // extract the line from the curve
                        Line curLine = locationCurve.Curve as Line;

                        // extract start and end points from the line
                        XYZ startPoint = curLine.GetEndPoint(0);
                        XYZ endPoint = curLine.GetEndPoint(1);

                        // trim top and bottom conditions ( for horizontal label lines ) 
                        if (trimLineInfo.mTrimType == TrimType.Top || trimLineInfo.mTrimType == TrimType.Bottom)
                        {
                            // trim only vertical lines
                            if (HallwayUtils.IsLineVertical(curLine))
                            {
                                double mainY = trimLineInfo.mLabelLine.mLines[0].startpoint.Y;

                                if (HallwayUtils.AreAlmostEqual(mainY, startPoint.Y, precision))
                                    startPoint += moveYvector;
                                else if (HallwayUtils.AreAlmostEqual(mainY, endPoint.Y, precision))
                                    endPoint += moveYvector;

                            }
                            // move the horizontal lines
                            else
                            {
                                startPoint += moveYvector;
                                endPoint += moveYvector;
                            }
                        }
                        // trim left and right conditions ( for vertical label lines )
                        else if (trimLineInfo.mTrimType == TrimType.Left || trimLineInfo.mTrimType == TrimType.Right)
                        {
                            // trim only horizontal lines
                            if (HallwayUtils.IsLineHorizontal(curLine))
                            {
                                double mainX = trimLineInfo.mLabelLine.mLines[0].startpoint.X;

                                if (HallwayUtils.AreAlmostEqual(mainX, startPoint.X, precision))
                                    startPoint += moveXvector;
                                else if (HallwayUtils.AreAlmostEqual(mainX, endPoint.X, precision))
                                    endPoint += moveXvector;
                            }
                            // move the vertical lines
                            else
                            {
                                startPoint += moveXvector;
                                endPoint += moveXvector;
                            }
                        }

                        // update the location curve with the new start and the end point
                        locationCurve.Curve = Line.CreateBound(startPoint, endPoint);
                    }
                }
                trans.Commit();
            }
        }
    
        public void AdjustHallwayLines()
        {
            var hallwayAdjust = new HallwayAdjustment(ref mDocument);

            hallwayAdjust.AdjustHallwayLine(HallwayTrimData.HorizontalLabelLines[0], 2 * HPT );
        }
    }
}

using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Revit_Automation.Source.Hallway.HallwayGenerator;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data;

namespace Revit_Automation.Source.Hallway
{
    internal class HallwayTrim
    {
        private static Document mDocument;

        private List<LabelLine> mHorLabelLines;

        private List<LabelLine> mVertLabelLines;

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
            public LabelLine mLabelLine;

            public List<Element> mIntersectingLines;

            public TrimType mTrimType;

            public int  mTrimValue;

            public TrimLineInfo(LabelLine labelLine)
            {
                mLabelLine = labelLine;
                mIntersectingLines = GetHallwayIntersectingInternalLines(labelLine);

                var lineType = InputLine.GetLineType(labelLine.mLines[0]);

                // set trim type
                if (lineType == LineType.HORIZONTAL)
                {
                    foreach(DataRow row in HallwayTrimData.TrimDataHorizontal.Rows)
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
                else if (lineType == LineType.VERTICAL)
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

        public HallwayTrim(ref Document doc, List<LabelLine> horLabelLines, List<LabelLine> verLabelLines) 
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
            foreach(LabelLine line in mHorLabelLines) 
            { 
                mTrimLineInfos.Add(new TrimLineInfo(line));
            }
            
            foreach(LabelLine line in mVertLabelLines) 
            { 
                mTrimLineInfos.Add(new TrimLineInfo(line));
            }

            FileWriter.WriteTrimLineInfoToFile(mTrimLineInfos, @"C:\temp\trim_info");
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="labelLine"></param>
        /// <returns>List of elements which are falling on the current line</returns>
        static private List<Element> GetHallwayIntersectingInternalLines(LabelLine labelLine)
        {
            var internalIntersectingLines = new List<Element>();

            foreach( var inputLine in labelLine.mLines)
            {
                if(InputLine.GetLineType(inputLine) == LineType.HORIZONTAL 
                    || InputLine.GetLineType(inputLine) == LineType.VERTICAL)
                {

                    // Create a Outline, uses a minimum and maximum XYZ point to initialize the Bounding Box. 
                    Outline myOutLn = new Outline(
                        new XYZ(inputLine.start.X - 0.5,
                        inputLine.start.Y - 0.5,
                        inputLine.start.Z - 0.5),
                        new XYZ(inputLine.end.X + 0.5,
                        inputLine.end.Y + 0.5,
                        inputLine.end.Z + 0.5));

                    // Create a BoundingBoxIntersects filter with this Outline
                    BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);

                    // Apply the filter to the elements in the active document to retrieve posts at a point
                    FilteredElementCollector collector = new FilteredElementCollector(mDocument);
                    IList<Element> lineElements = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();

                    foreach( var lineElement in lineElements)
                    {
                        // do not add external lines
                        if(!lineElement.LookupParameter("Wall Type").AsString().Contains("Ex"));
                            internalIntersectingLines.Add(lineElement);
                    }
                }
            }

            return internalIntersectingLines;
        }

        public void TrimHallwayIntersectingLines()
        {
            using (Transaction trans = new Transaction(mDocument, "Trimming Hallway lines"))
            {
                trans.Start("Trimming Hallway Intersections");
                foreach (var trimLineInfo in mTrimLineInfos)
                {
                    // skip trimming if there trimming is not opted in the form
                    // and 
                    if (trimLineInfo.mTrimType == TrimType.None
                            || trimLineInfo.mLabelLine.mLines.Count == 0)
                        continue;

                    double offset = (trimLineInfo.mTrimValue * HPT) / 12.0;

                    XYZ moveYvector = new XYZ(0.0, (trimLineInfo.mTrimType == TrimType.Top) ? (offset) : -(offset), 0.0);

                    XYZ moveXvector = new XYZ((trimLineInfo.mTrimType == TrimType.Right) ? (offset) : -(offset), 0.0, 0.0);

                    foreach (var lineElement in trimLineInfo.mIntersectingLines)
                    {
                        LocationCurve locationCurve = lineElement.Location as LocationCurve;
                        Line curLine = locationCurve.Curve as Line;

                        XYZ startPoint = curLine.GetEndPoint(0);
                        XYZ endPoint = curLine.GetEndPoint(1);

                        if (trimLineInfo.mTrimType == TrimType.Top || trimLineInfo.mTrimType == TrimType.Bottom)
                        {
                            // trim only vertical lines
                            if (LineUtils.IsLineVertical(curLine))
                            {
                                double mainY = trimLineInfo.mLabelLine.mLines[0].start.Y;

                                if (PointUtils.AreAlmostEqual(mainY, startPoint.Y, 0.5))
                                    startPoint += moveYvector;
                                else if (PointUtils.AreAlmostEqual(mainY, endPoint.Y, 0.5))
                                    endPoint += moveYvector;

                            }
                            // move the horizontal lines
                            else
                            {
                                startPoint += moveYvector;
                                endPoint += moveYvector;
                            }
                        }
                        else if (trimLineInfo.mTrimType == TrimType.Left || trimLineInfo.mTrimType == TrimType.Right)
                        {
                            // trim only horizontal lines
                            if (LineUtils.IsLineHorizontal(curLine))
                            {
                                double mainX = trimLineInfo.mLabelLine.mLines[0].start.X;

                                if (PointUtils.AreAlmostEqual(mainX, startPoint.X, 0.5))
                                    startPoint += moveXvector;
                                else if (PointUtils.AreAlmostEqual(mainX, endPoint.X, 0.5))
                                    endPoint += moveXvector;
                            }
                            // move the vertical lines
                            else
                            {
                                startPoint += moveXvector;
                                endPoint += moveXvector;
                            } 
                        }


                        locationCurve.Curve = Line.CreateBound(startPoint, endPoint);
                    }
                }
                trans.Commit();
            }
        }
    }
}

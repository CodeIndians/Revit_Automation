using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure.StructuralSections;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.CollisionDetectors;
using Revit_Automation.Source.ModelCreators.Walls;
using Revit_Automation.Source.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Xml.Schema;
using static Autodesk.Revit.DB.SpecTypeId;

namespace Revit_Automation.Source.ModelCreators
{
    internal class WallCreator : IModelCreator
    {
        private Autodesk.Revit.DB.Document m_Document { get; set; }

        private Form1 m_Form { get; set; }
        
        public WallCreator(Autodesk.Revit.DB.Document doc, Form1 form)
        {

            m_Document = doc;
            m_Form = form;
        }

        public void CreateModel(List<CustomTypes.InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            using (Transaction stx = new Transaction(m_Document))
            {
                PanelUtils panelUtils = new PanelUtils(m_Document);
                panelUtils.ComputePanelDirectionForExteriorPanels();
            }

            using (Transaction tx = new Transaction(m_Document))
            {
                GenericUtils.SupressWarningsInTransaction(tx);
                tx.Start("Generating Model");
                ProcessWallLines(colInputLines, levels);
                tx.Commit();
            }
        }

        private void ProcessWallLines(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            m_Form.PostMessage("");
            m_Form.PostMessage("\n Starting Placement of Panels");
            Logger.logMessage("Method - ProcessWallLines");

            int iLineProcessing = 0;

            DateTime StartTime = DateTime.Now;

            double dCounter = 0;
            int iCounter = 1;
            double dIncrementFactor = 100 / colInputLines.Count;

            foreach (InputLine inputLine in colInputLines)
            {
                try
                {
                    iLineProcessing++;
                    m_Form.PostMessage(string.Format("\n Placing Wall at Line {0} / {1}", iLineProcessing, colInputLines.Count));
                    Logger.logMessage(string.Format("Placing Wall at Line {0} / {1} : ID : {2}", iLineProcessing, colInputLines.Count, inputLine.id));

                    if (iCounter < 100 && (iCounter < dCounter))
                    {
                        iCounter = (int)Math.Ceiling(dCounter);
                        m_Form.UpdateProgress(iCounter);
                    }

                    PlaceWall(inputLine, levels);

                }
                catch (Exception e)
                { 
                }
            }

            DateTime EndTime = DateTime.Now;

            TimeSpan timeDifference = EndTime - StartTime;
            double seconds = timeDifference.TotalSeconds;

            m_Form.PostMessage(string.Format("\n Completed Placement of walls in {0} seconds", seconds));
        }

        private double GetPanelMaxLength(InputLine line)
        {
            PanelTypeGlobalParams pg = string.IsNullOrEmpty(line.strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == line.strPanelType);

            double dPanelPreferredLength = pg.iPanelMaxLength;

            return dPanelPreferredLength;
        }

        private void  PlaceWall(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
            double dPanelHeighOffset = GetPanelHeightOffsetValue(inputLine, levels);

            double dMaxLength = GetPanelMaxLength(inputLine);

            Logger.logMessage("Method - PlaceWall");

            // Get Line End points.
            XYZ pt1 = null, pt2 = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);


            // Get the orientation of the line
            LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y)? LineType.Horizontal:  LineType.vertical;

            // Identify the base and top levels,
            Level toplevel = null, baseLevel = null;

            // Filter levels based on buldings to use
            List<Level> filteredLevels = new List<Level>();
            foreach (Level filteredlevel in levels)
            {
                if (filteredlevel.Name.Contains(inputLine.strBuildingName))
                {
                    filteredLevels.Add(filteredlevel);
                }
            }

            for (int i = 0; i < filteredLevels.Count() - 1; i++)
            {
                Level tempLevel = filteredLevels.ElementAt(i);

                if ((pt2.Z < (tempLevel.Elevation + 1)) && (pt2.Z > (tempLevel.Elevation - 1)))
                {

                    baseLevel = tempLevel;
                    toplevel = filteredLevels.ElementAt(i + 1);

                    break;
                }
            }

            
            string strPanelType =  GetPanelType(inputLine);

            // Wall options
            WallType wallType = SymbolCollector.GetWall(strPanelType, "Basic Wall");

            // Compute the wall end points based on various conditions.
            List<XYZ> wallEndPtsCollection = new List<XYZ>();
            bool bFlip = false;
            
            ComputeWallEndPoints(pt1, pt2, inputLine, lineType, out wallEndPtsCollection, out bFlip);
            PanelDirection panelDirection = ComputePanelDirection(inputLine);

            double dLineLength = 0.0;

            if (lineType == LineType.Horizontal)
                dLineLength = (Math.Abs(pt2.X - pt1.X));
            else if (lineType == LineType.vertical)
                dLineLength = (Math.Abs(pt2.Y - pt1.Y));

            for (int i = 0; i < wallEndPtsCollection.Count; i++)
            {
                XYZ wp1 = wallEndPtsCollection[i];
                XYZ wp2 = wallEndPtsCollection[i + 1];

                bool bStartingPoint = false;
                bool bEndingPoint = false;
                if (i == 0)
                    bStartingPoint = true;

                if (panelDirection == PanelDirection.B)
                {
                    if ((i == wallEndPtsCollection.Count - 2) || (i == wallEndPtsCollection.Count / 2  - 2))
                        bEndingPoint = true;
                }
                else if (i == wallEndPtsCollection.Count - 2)
                    bEndingPoint = true;

                XYZ awp1 = null, awp2 = null;
                double dPanelClearance = GetPanelClearance(inputLine);

                if (dPanelClearance != 0)
                    RoundoffToNearestInch(lineType, wp1, wp2, out awp1, out awp2, bStartingPoint, bEndingPoint);
                
                if (dPanelClearance == 0 && dLineLength < dMaxLength)
                    RoundDownToNearestInch(lineType, wp1, wp2, out awp1, out awp2, bStartingPoint, bEndingPoint);
                
                // Panel Clearance - only for single panels
                if (dLineLength < dMaxLength)
                    AddPanelClearance(inputLine, ref awp1, ref awp2);

                // Create Wall Curve
                Line wallLine = Line.CreateBound(awp1, awp2);
                List<Curve> wallCurves = new List<Curve> { wallLine };

                // Place Wall
                Wall wall = Wall.Create(m_Document, wallLine, wallType.Id, baseLevel.Id, (toplevel.Elevation - baseLevel.Elevation - dPanelHeighOffset), 0, false, false);

                // Disallow joins at start and End
                WallUtils.DisallowWallJoinAtEnd(wall, 0);
                WallUtils.DisallowWallJoinAtEnd(wall, 1);

                // Set the Panel gauge property
                Parameter guageParam = wall.LookupParameter("Gauge");
                if (guageParam != null)
                {
                    string strGauge = GenericUtils.GetPartitionPanelGauge(inputLine);
                    guageParam.Set(strGauge);
                }

                i++;
            }  
        }

        private void AddPanelClearance(InputLine inputLine, ref XYZ awp1, ref XYZ awp2)
        {
            if (inputLine.strWallType == "Fire" || inputLine.strPanelType == "Flat Panel")
                return;

            double dPanelClearance = GetPanelClearance(inputLine);

            if (GenericUtils.GetLineType(inputLine) == LineType.Horizontal)
            {
                awp1 = awp1 + new XYZ(dPanelClearance / 2.0, 0, 0);
                awp2 = awp2 + new XYZ(-dPanelClearance / 2.0, 0, 0);
            }
            else
            {
                awp1 = awp1 + new XYZ(0, dPanelClearance / 2.0, 0);
                awp2 = awp2 + new XYZ(0, -dPanelClearance / 2.0, 0);
            }
        }

        private double GetPanelClearance(InputLine inputLine)
        {
            PanelTypeGlobalParams pg = string.IsNullOrEmpty(inputLine.strPanelType) ?
                 GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                 GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == inputLine.strPanelType);

            double dPanelClearance = pg.iPanelClearance;

            if (dPanelClearance == 0.08)
                return 0.0833333;
            if (dPanelClearance == 0.17)
                return 0.1666666;
            if (dPanelClearance == 0.25)
                return 0.25;
            if (dPanelClearance == 0.33)
                return 0.333333;

            return 0.0;
        }

        private void RoundoffToNearestInch(LineType lineType, XYZ wp1, XYZ wp2, out XYZ awp1, out XYZ awp2, bool bStartingPoint, bool bEndingPoint)
        {
            awp1 = wp1; awp2 = wp2;

            //return;
            if (lineType == LineType.Horizontal)
            {
                double fraction = Math.Abs(wp2.X - wp1.X) % 1;
                double roundedFraction = RoundInches(fraction);
                if (!bEndingPoint)
                {
                    awp1 = wp1;
                    awp2 = wp2 + new XYZ(roundedFraction - fraction, 0, 0);
                }
                else
                {
                    awp1 = wp1 - new XYZ(roundedFraction - fraction, 0, 0);
                    awp2 = wp2;
                }
            }
            else
            {
                double fraction = Math.Abs(wp2.Y - wp1.Y) % 1;

                if (!MathUtils.ApproximatelyEqual(fraction, 0))
                {
                    double roundedFraction = RoundInches(fraction);

                    if (!bEndingPoint)
                    {
                        awp1 = wp1;
                        awp2 = wp2 + new XYZ(0, roundedFraction - fraction, 0);
                    }
                    else
                    {
                        awp1 = wp1 - new XYZ(0, roundedFraction - fraction, 0);
                        awp2 = wp2;
                    }
                }
            }
        }

        private void RoundDownToNearestInch(LineType lineType, XYZ wp1, XYZ wp2, out XYZ awp1, out XYZ awp2, bool bStartingPoint, bool bEndingPoint)
        {
            awp1 = wp1; awp2 = wp2;

            //return;
            if (lineType == LineType.Horizontal)
            {
                double fraction = Math.Abs(wp2.X - wp1.X) % 1;
                double roundedFraction = RoundInches(fraction);

                //RoundInches will give zero or the next inch. so substract 1-Inch when round incges is non-zero
                if (!MathUtils.ApproximatelyEqual(fraction, roundedFraction))
                    roundedFraction -= 1.0 / 12.0;

                double roundDownFactor = fraction - roundedFraction;
                if (!bEndingPoint)
                {
                    awp1 = wp1;
                    awp2 = wp2 - new XYZ(roundDownFactor, 0, 0);
                }
                else
                {
                    awp1 = wp1 + new XYZ(roundDownFactor, 0, 0);
                    awp2 = wp2;
                }
            }
            else
            {
                double fraction = Math.Abs(wp2.Y - wp1.Y) % 1;
                double roundedFraction = RoundInches(fraction) - 1.0/12.0;//RoundInches will give the next inch. so substract 1-Inch
                double roundDownFactor = fraction - roundedFraction;
                if (!MathUtils.ApproximatelyEqual(fraction, 0))
                {
                   

                    if (!bEndingPoint)
                    {
                        awp1 = wp1;
                        awp2 = wp2 - new XYZ(0, roundDownFactor, 0);
                    }
                    else
                    {
                        awp1 = wp1 + new XYZ(0, roundDownFactor, 0);
                        awp2 = wp2;
                    }
                }
            }
        }

        private double RoundInches(double fraction)
        {
            if (0.00 < fraction && (fraction < 0.0833333 || MathUtils.ApproximatelyEqual(0.0833333, fraction)))
                return 0.0833333;
            else if (0.0833333 < fraction && (fraction <= 0.166666 || MathUtils.ApproximatelyEqual(0.166666, fraction)))
                return 0.166666;
            else if (0.166666 < fraction && (fraction <= 0.25 || MathUtils.ApproximatelyEqual(0.25, fraction)))
                return 0.25;
            else if ( 0.25 < fraction && (fraction <= 0.333333 || MathUtils.ApproximatelyEqual(0.333333, fraction)))
                return 0.333333;
            else if (0.333333 < fraction && (fraction <= 0.416666 || MathUtils.ApproximatelyEqual(0.416666, fraction)))
                return 0.416666;
            else if (0.416666 < fraction && (fraction <= 0.5 || MathUtils.ApproximatelyEqual(0.5, fraction)))
                return 0.5;
            else if (0.5 < fraction && (fraction <= 0.583333 || MathUtils.ApproximatelyEqual(0.583333, fraction)))
                return 0.583333;
            else if (0.583333 < fraction && (fraction <= 0.666666 || MathUtils.ApproximatelyEqual(0.666666, fraction)))
                return 0.666666;
            else if (0.666666 < fraction && (fraction <= 0.75 || MathUtils.ApproximatelyEqual(0.75, fraction)))
                return 0.75;
            else if (0.75 < fraction && (fraction <= 0.833333 || MathUtils.ApproximatelyEqual(0.833333, fraction)))
                return 0.833333;
            else if (0.833333 < fraction && (fraction <= 0.916666 || MathUtils.ApproximatelyEqual(0.916666, fraction)))
                return 0.916666;
            else if (0.916666 < fraction && (fraction < 1.0 || MathUtils.ApproximatelyEqual(1.0, fraction)))
                return 1.0;

            return 0.0;
        }

        private string GetPanelType(InputLine inputLine)
        {
            PanelTypeGlobalParams pg = string.IsNullOrEmpty(inputLine.strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == inputLine.strPanelType);

            return pg.strWallName;
        }

        /// <summary>
        /// Computes the wall end points based on the Input line and grid reference
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="inputLine"></param>
        /// <param name="lineType"></param>
        /// <param name="wp1"></param>
        /// <param name="wp2"></param>
        /// <param name="bFlip"></param>
        private void ComputeWallEndPoints(XYZ pt1, XYZ pt2, InputLine inputLine, LineType lineType, out List<XYZ> wallEndPointsCollection, out bool bFlip)
        {
            // Initialize an empty collection
            wallEndPointsCollection = new List<XYZ>();

            // Get panel direction as per the 
            PanelDirection direction = ComputePanelDirection(inputLine);

            // For Panel directions of Top and left, Flip will be true. Bottom and Right Flip will be false
            bFlip = (direction == PanelDirection.U || direction == PanelDirection.L);

            SortedDictionary<XYZ, string> rightPanelIntersection, leftPanelIntersections, endPanelIntersections;

            //Get Panel Intersections - Right/Left and Top/Bottom
            if (lineType == LineType.Horizontal)
            {
                rightPanelIntersection = new SortedDictionary<XYZ, string>(new XComparer());
                leftPanelIntersections = new SortedDictionary<XYZ, string>(new XComparer());
                endPanelIntersections = new SortedDictionary<XYZ, string>(new XComparer());
            }
            else
            {
                rightPanelIntersection = new SortedDictionary<XYZ, string>(new YComparer());
                leftPanelIntersections = new SortedDictionary<XYZ, string>(new YComparer());
                endPanelIntersections = new SortedDictionary<XYZ, string>(new YComparer());
            }

            GetPanelIntersections(inputLine, ref rightPanelIntersection, ref leftPanelIntersections, ref endPanelIntersections);

            switch (inputLine.strWallType)
            {
                case "Fire":
                    FireWallPoints fireWallPoints = new FireWallPoints();
                    fireWallPoints.ComputeEndPoints(m_Document, inputLine, rightPanelIntersection, leftPanelIntersections, endPanelIntersections, ref wallEndPointsCollection);
                    break;
                case "Insulation":
                    InsulationWallPoints insulationWallPoints = new InsulationWallPoints();
                    insulationWallPoints.ComputeEndPoints(m_Document, inputLine, rightPanelIntersection, leftPanelIntersections, endPanelIntersections, ref wallEndPointsCollection);
                    break;
                case "Ex w/ Insulation":
                    ExteriorInsulationWallPoints exteriorInsulationWallPoints = new ExteriorInsulationWallPoints();
                    exteriorInsulationWallPoints.ComputeEndPoints(m_Document, inputLine, rightPanelIntersection, leftPanelIntersections, endPanelIntersections, ref wallEndPointsCollection);
                    break;
                case "LB":
                case "LBS":
                    LoadBearingWallPoints loadBearingWallPoints = new LoadBearingWallPoints();
                    loadBearingWallPoints.ComputeEndPoints(m_Document, inputLine, rightPanelIntersection, leftPanelIntersections, endPanelIntersections, ref wallEndPointsCollection);
                    break;
                case "NLB":
                case "NLBS":
                    NonLoadBearingWallPoints nonloadBearingWallPoints = new NonLoadBearingWallPoints();
                    nonloadBearingWallPoints.ComputeEndPoints(m_Document, inputLine, rightPanelIntersection, leftPanelIntersections, endPanelIntersections, ref wallEndPointsCollection);
                    break;
                default: break;

            }
        }

        public class XComparer : IComparer<XYZ>
        {
            public int Compare(XYZ x, XYZ y)
            {
                if (x.X < y.X)
                    return -1;
                if (x.X > y.X)
                    return 1;
                return 0;
            }
        }

        public class YComparer : IComparer<XYZ>
        {
            public int Compare(XYZ x, XYZ y)
            {
                if (x.Y < y.Y)
                    return -1;
                if (x.Y > y.Y)
                    return 1;
                return 0;
            }
        }

        private void GetPanelIntersections(InputLine inputLine, ref SortedDictionary<XYZ, string> rightPanelIntersection, ref SortedDictionary<XYZ, string> leftPanelIntersections, ref SortedDictionary<XYZ, string> endPanelIntersections)
        {
            // Get Left and right or top and bottom outlines for a given line
            List<Outline> outlines = GetOutlinesForInputLine(inputLine);

            XYZ ilp1 = null, ilp2 = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out ilp1, out ilp2);

            LineType linetype = MathUtils.ApproximatelyEqual(ilp1.Y, ilp2.Y) ? LineType.Horizontal : LineType.vertical;

            for (int i = 0 ; i < 2; i++)
            {
                Outline myOutLn = outlines[i];
                
                // Create a BoundingBoxIntersects filter with this Outline
                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);

                // Apply the filter to the elements in the active document to retrieve posts at a point
                FilteredElementCollector collector = new FilteredElementCollector(m_Document);
                IList<Element> GenericModelElems = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();

                foreach (Element modelLine in GenericModelElems)
                {
                    XYZ pt1 = null, pt2 = null;
                    GenericUtils.GetlineStartAndEndPoints(modelLine, out pt1, out pt2);

                    LineType iLineOrientation = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

                    if (iLineOrientation != linetype)
                    {
                        string strWallType = string.Empty;
                        string strLineInfo = string.Empty;
                        Parameter WallTypeParam = modelLine.LookupParameter("Wall Type");
                        if (WallTypeParam != null)
                        {
                            strWallType = WallTypeParam.AsString();
                            strLineInfo += WallTypeParam.AsString();
                        }
                        
                        Parameter stud = modelLine.LookupParameter("Stud Size");
                        if (stud != null)
                        {
                            strLineInfo += "|";
                            strLineInfo += stud.AsString();
                        }


                        PanelDirection panelDir =  (strWallType == "Insulation" || strWallType == "Fire") ? PanelDirection.B : ComputePanelDirection(modelLine);
                        strLineInfo += "|";
                        strLineInfo += panelDir.ToString();

                        strLineInfo += "|";
                        strLineInfo += modelLine.Id.ToString();

                        XYZ intersectionPT = GetNearestPointToLine(inputLine, pt1, pt2);
                        XYZ endPt = null;

                        // To-Do - Add code for End panel intersections.
                        if (CheckIfLineAtEnds(inputLine, intersectionPT, out endPt))
                        {
                            if (!endPanelIntersections.ContainsKey(intersectionPT))
                                endPanelIntersections.Add(intersectionPT, strLineInfo);
                        }
                        else
                        {
                            if (i == 0)
                            {
                                if (!rightPanelIntersection.ContainsKey(intersectionPT))
                                    rightPanelIntersection.Add(intersectionPT, strLineInfo);
                            }
                            else
                            {
                                if (!leftPanelIntersections.ContainsKey(intersectionPT))
                                    leftPanelIntersections.Add(intersectionPT, strLineInfo);
                            }
                        }
                    }
                }
            }
        }

        private bool CheckIfLineAtEnds(InputLine inputLine, XYZ intersectionPT, out XYZ endPt)
        {
            // Get Line End points.
            XYZ ilpt1 = null, ilpt2 = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out ilpt1, out ilpt2);

            // Get the orientation of the line
            LineType lineType = MathUtils.ApproximatelyEqual(ilpt1.Y, ilpt2.Y) ? LineType.Horizontal : LineType.vertical;

            if (lineType == LineType.Horizontal)
            {
                if (Math.Abs(intersectionPT.X - ilpt1.X) <= 0.5 || Math.Abs(intersectionPT.X - ilpt2.X) <= 0.5)
                {
                    endPt = intersectionPT;
                    return true;
                }
            }
            else if (Math.Abs(intersectionPT.Y - ilpt1.Y) <= 0.5 || Math.Abs(intersectionPT.Y - ilpt2.Y) <= 0.5)
            {
                endPt = intersectionPT;
                return true;
            }

            endPt = null;
            return false;
        }

        private XYZ GetNearestPointToLine(InputLine inputLine, XYZ pt1, XYZ pt2)
        {
            XYZ nearestPT = null;
            
            // Get Line End points.
            XYZ ilpt1 = null, ilpt2 = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out ilpt1, out ilpt2);

            // Get the orientation of the line
            LineType lineType = MathUtils.ApproximatelyEqual(ilpt1.Y, ilpt2.Y) ? LineType.Horizontal : LineType.vertical;

            if (lineType == LineType.Horizontal)
            {
                nearestPT = (Math.Abs(ilpt1.Y - pt1.Y) < Math.Abs(ilpt1.Y - pt2.Y)) ? pt1 : pt2;
            }
            else
            {
                nearestPT = (Math.Abs(ilpt1.X - pt1.X) < Math.Abs(ilpt1.X - pt2.X)) ? pt1 : pt2;
            }

            return nearestPT;
        }

        private List<Outline> GetOutlinesForInputLine(InputLine inputLine)
        {
            List<Outline> outlines = new List<Outline>();

            // Get Line End points.
            XYZ pt1 = null, pt2 = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);

            // Get the orientation of the line
            LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

            Outline rtOutline = null, LftOutline = null; 
            if (lineType == LineType.Horizontal)
            {
                // Top is right
                rtOutline = new Outline(
                    new XYZ(pt1.X- 0.5,
                    pt1.Y,
                    pt1.Z- 0.5),
                    new XYZ(pt2.X + 0.5,
                    pt2.Y + 0.5,
                    pt2.Z + 0.5));

                // Bottom is left
                LftOutline = new Outline(
                    new XYZ(pt1.X - 0.5,
                    pt1.Y - 0.5,
                    pt1.Z - 0.5),
                    new XYZ(pt2.X + 0.5,
                    pt2.Y,
                    pt2.Z+ 0.5));
            }
            else
            {
                // Right
                rtOutline = new Outline(
                    new XYZ(pt1.X,
                    pt1.Y - 0.5 ,
                    pt1.Z - 0.5),
                    new XYZ(pt2.X + 0.5,
                    pt2.Y + 0.5 ,
                    pt2.Z + 0.5));

                // Bottom is left
                LftOutline = new Outline(
                    new XYZ(pt1.X - 0.5,
                    pt1.Y - 0.5,
                    pt1.Z - 0.5),
                    new XYZ(pt2.X,
                    pt2.Y + 0.5,
                    pt2.Z + 0.5)
                    );
            }


            outlines.Add(rtOutline);
            outlines.Add(LftOutline);

            return outlines;
        }

        private PanelDirection ComputePanelDirection(InputLine inputLine)
        {
            // Panel Direction computation has the following priority.
            // Line > Settings > Automatic Computation

            PanelDirection panelDirection = PanelDirection.B ;

            if (inputLine.strWallType == "Fire" || inputLine.strWallType == "Insulation")
                return PanelDirection.B ;

            // Get Line End points.
            XYZ pt1 = null, pt2 = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);

            // Get the orientation of the line
            LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

            string  strPanelDirection = (lineType == LineType.Horizontal) ?
                                            inputLine.strHorizontalPanelDirection : 
                                            inputLine.strVerticalPanelDirection;

            if (strPanelDirection != string.Empty)
            {
                panelDirection = (PanelDirection)Enum.Parse(typeof(PanelDirection), strPanelDirection);
                return panelDirection;
            }
            else
            {
                XYZ lineOrientation = pt2 - pt1;
                XYZ SlopeDirection = RoofUtility.GetRoofSlopeDirection(pt1);

                //if Panel Direction Computation is automatic and Line is perpendicular to slope determine direction
                if ((GlobalSettings.s_PanelDirectionComputation == 0) && !(MathUtils.IsParallel(SlopeDirection, lineOrientation)) && SlopeDirection != null )
                {
                    if (lineType == LineType.Horizontal && SlopeDirection.Y < 0)
                        panelDirection = PanelDirection.D;
                    else if (lineType == LineType.Horizontal && SlopeDirection.Y > 0)
                        panelDirection = PanelDirection.U;
                    else if (lineType == LineType.vertical && SlopeDirection.X > 0)
                        panelDirection = PanelDirection.R;
                    else if (lineType == LineType.vertical && SlopeDirection.X < 0)
                        panelDirection = PanelDirection.L;
                }
                else
                {
                    PanelTypeGlobalParams pg = string.IsNullOrEmpty(inputLine.strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == inputLine.strPanelType);

                    string strPanelDir = (lineType == LineType.Horizontal) ? pg.strPanelHorizontalDirection : pg.strPanelVerticalDirection;
                    panelDirection = (PanelDirection)Enum.Parse(typeof(PanelDirection), strPanelDir);
                }
                return panelDirection; 
            }
        }

        private PanelDirection ComputePanelDirection(Element InputLineElement)
        {
            // Panel Direction computation has the following priority.
            // Line > Settings > Automatic Computation


            PanelDirection panelDirection = PanelDirection.B;

            // Get Line End points.
            XYZ pt1 = null, pt2 = null;
            GenericUtils.GetlineStartAndEndPoints(InputLineElement, out pt1, out pt2);

            string strHorizontalPanelDirection = string.Empty,
                   strVerticalPanelDirection = string.Empty,
                   strPanelType = string.Empty;


            Parameter param = InputLineElement.LookupParameter("Horizontal Panel Direction");
            if (param != null)
            {
                strHorizontalPanelDirection = param.AsString();
            }
            
            param = InputLineElement.LookupParameter("Vertical Panel Direction");
            if (param != null)
            {
                strVerticalPanelDirection = param.AsString();
            }
            
            param = InputLineElement.LookupParameter("Panel Type");
            if (param != null)
            {
                strPanelType = param.AsString();

            }
            // Get the orientation of the line
            LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

            string strPanelDirection = (lineType == LineType.Horizontal) ?
                                            strHorizontalPanelDirection :
                                            strVerticalPanelDirection;

            if (strPanelDirection != string.Empty)
            {
                panelDirection = (PanelDirection)Enum.Parse(typeof(PanelDirection), strPanelDirection);
                return panelDirection;
            }
            else
            {
                XYZ lineOrientation = pt2 - pt1;
                XYZ SlopeDirection = RoofUtility.GetRoofSlopeDirection(pt1);

                //if Panel Direction Computation is automatic and Line is perpendicular to slope determine direction
                if ((GlobalSettings.s_PanelDirectionComputation == 0) && !(MathUtils.IsParallel(SlopeDirection, lineOrientation)) && SlopeDirection != null)
                {
                    if (lineType == LineType.Horizontal && SlopeDirection.Y < 0)
                        panelDirection = PanelDirection.D;
                    else if (lineType == LineType.Horizontal && SlopeDirection.Y > 0)
                        panelDirection = PanelDirection.U;
                    else if (lineType == LineType.vertical && SlopeDirection.X > 0)
                        panelDirection = PanelDirection.R;
                    else if (lineType == LineType.vertical && SlopeDirection.X < 0)
                        panelDirection = PanelDirection.L;
                }
                else
                {
                    PanelTypeGlobalParams pg = string.IsNullOrEmpty(strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == strPanelType);

                    string strPanelDir = (lineType == LineType.Horizontal) ? pg.strPanelHorizontalDirection : pg.strPanelVerticalDirection;
                    panelDirection = (PanelDirection)Enum.Parse(typeof(PanelDirection), strPanelDir);
                }
                return panelDirection;
            }
        }

        private double GetPanelHeightOffsetValue(InputLine line, IOrderedEnumerable<Level> levels)
        {
            double dOffset = 0.0;

            Level level = GetLevelForInputLine(line, levels);

            if (level != null)
            {
                Parameter thicknessParam = null;

                Element SlabElement = GenericUtils.GetNearestFloorOrRoof(level, line.startpoint, m_Document);
                if (SlabElement != null)
                    thicknessParam = SlabElement.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);

                if (SlabElement == null)
                {
                    Element roofElement = GenericUtils.GetRoofAtPoint(line.startpoint, m_Document);
                    thicknessParam = roofElement.get_Parameter(BuiltInParameter.ROOF_ATTR_THICKNESS_PARAM);
                }
                if (thicknessParam != null)
                {
                    dOffset = thicknessParam.AsDouble();
                }
            }

            // First Check for Height Offset in the Inputline, if not availble then go to UNO
            if (line.dPanelOffsetHeight == 0.0)
            {
                PanelTypeGlobalParams pg = string.IsNullOrEmpty(line.strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == line.strPanelType);

                dOffset += pg.iPanelHeightOffset;
            }
            else
                dOffset += (line.dPanelOffsetHeight * -1); // Usually the offset is entered in negative values in the line 

            return dOffset;
        }

        private Level GetLevelForInputLine(InputLine temp, IOrderedEnumerable<Level> levels)
        {
            Level level = null;

            // Filter levels based on buldings to use
            List<Level> filteredLevels = new List<Level>();
            foreach (Level filteredlevel in levels)
            {
                if (filteredlevel.Name.Contains(temp.strBuildingName))
                {
                    filteredLevels.Add(filteredlevel);
                }
            }

            for (int i = 0; i < filteredLevels.Count() - 1; i++)
            {
                Level tempLevel = filteredLevels.ElementAt(i);

                if ((temp.startpoint.Z < (tempLevel.Elevation + 1)) && (temp.startpoint.Z > (tempLevel.Elevation - 1)))
                {
                    Level toplevel = filteredLevels.ElementAt(i + 1);
                    level = toplevel;
                }
            }

            return level;
        }
    }
}

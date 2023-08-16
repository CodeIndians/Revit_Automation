﻿using Autodesk.Revit.DB;
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
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;
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

        private void PlaceWall(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
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

            for (int i = 0; i < wallEndPtsCollection.Count; i++)
            {
                XYZ wp1 = wallEndPtsCollection[i];
                XYZ wp2 = wallEndPtsCollection[i + 1];
                // Create Wall Curve
                Line wallLine = Line.CreateBound(wp1, wp2);
                List<Curve> wallCurves = new List<Curve> { wallLine };

                // Place Wall
                Wall wall = Wall.Create(m_Document, wallLine, wallType.Id, baseLevel.Id, (toplevel.Elevation - baseLevel.Elevation), 0, false, false);

                // Disallow joins at start and End
                WallUtils.DisallowWallJoinAtEnd(wall, 0);
                WallUtils.DisallowWallJoinAtEnd(wall, 1);

                i++;
            }  
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
                    break;
                case "NLB":
                case "NLBS":
                    break;
                default: break;

            }
        }

        public class XComparer : IComparer<XYZ>
        {
            public int Compare(XYZ x, XYZ y)
            {
                if (x.X < y.X)
                    return 1;
                if (x.X > y.X)
                    return -1;
                return 0;
            }
        }

        public class YComparer : IComparer<XYZ>
        {
            public int Compare(XYZ x, XYZ y)
            {
                if (x.Y < y.Y)
                    return 1;
                if (x.Y > y.Y)
                    return -1;
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
                        string strLineInfo = string.Empty;
                        Parameter WallTypeParam = modelLine.LookupParameter("Wall Type");
                        if (WallTypeParam != null)
                        {
                            strLineInfo += WallTypeParam.AsString();
                        }
                        
                        Parameter stud = modelLine.LookupParameter("Stud Size");
                        if (stud != null)
                        {
                            strLineInfo += "|";
                            strLineInfo += stud.AsString();
                        }

                        PanelDirection panelDir = ComputePanelDirection(modelLine);
                        strLineInfo += "|";
                        strLineInfo += panelDir.ToString(); 

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
    }
}
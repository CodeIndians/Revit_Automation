﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Interfaces;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.ModelCreators.Walls
{
    public class ExteriorInsulationWallPoints : IWallPointsGenerator
    {
        private List<XYZ> studpoints = new List<XYZ>();
        private Document m_Document = null;
        private InputLine m_inputLine;
        public void AddStudsIfNeeded()
        {
            foreach(XYZ studpoint in studpoints)
            {
                PostCreationUtils.PlaceStudAtPoint(m_Document, studpoint, m_inputLine, true);
            }
        }

        public void ComputeEndPoints(Document doc, InputLine inputLine, SortedDictionary<XYZ, string> rightPanelIntersection, SortedDictionary<XYZ, string> leftPanelIntersections, SortedDictionary<XYZ, string> endPanelIntersections, ref List<XYZ> wallEndPointsCollection)
        {
            // Initialize the class variables;
            m_Document = doc;
            m_inputLine = inputLine;

            PanelDirection panelDirection = ComputePanelDirectionForExteriorPanels(inputLine);

            LineRelations startReleation = LineRelations.NoStartIntersection;
            LineRelations endRelation = LineRelations.NoEndIntersection;

            LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X) ? LineType.vertical : LineType.Horizontal;
            double dWebwidth = GenericUtils.WebWidth(inputLine.strStudType);

            if (endPanelIntersections.Count > 0)
            {
                CheckStartAndEndIntersectRelationsForFireWall(inputLine, endPanelIntersections, ref startReleation, ref endRelation);
            }


            XYZ startpt = null, endPt = null;
            XYZ startpoint = null, endpoint = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out startpoint, out endpoint);

            if (startReleation != LineRelations.NoStartIntersection)
            {
                startpt = ComputeStartPoint(startReleation, inputLine, endPanelIntersections, panelDirection) ;
            }
            else
            {
                if (linetype == LineType.Horizontal)
                {

                    startpt = (panelDirection == PanelDirection.R) ? new XYZ(startpoint.X, startpoint.Y + dWebwidth / 2, startpoint.Z)
                                        : new XYZ(startpoint.X, startpoint.Y - dWebwidth / 2, startpoint.Z);
                }
                else
                {
                    startpt = (panelDirection == PanelDirection.R) ? new XYZ(startpoint.X + dWebwidth / 2, startpoint.Y, startpoint.Z)
                                            : new XYZ(startpoint.X - dWebwidth / 2, startpoint.Y, startpoint.Z);

                }

            }

            if (endRelation != LineRelations.NoEndIntersection)
            {
                endPt = ComputerEndPoint(endRelation, inputLine, endPanelIntersections, panelDirection);
            }
            else
            {
                if (linetype == LineType.Horizontal)
                {

                    endPt = (panelDirection == PanelDirection.R) ? new XYZ(endpoint.X, endpoint.Y + dWebwidth / 2, endpoint.Z)
                                        : new XYZ(endpoint.X, endpoint.Y - dWebwidth / 2, endpoint.Z);
                }
                else
                {
                    endPt = (panelDirection == PanelDirection.R) ? new XYZ(endpoint.X + dWebwidth / 2, endpoint.Y, endpoint.Z)
                                            : new XYZ(endpoint.X - dWebwidth / 2, endpoint.Y, endpoint.Z);

                }
            }
 
            XYZ AdditionVector = null;
            double dPanelPreferredLength = GetPanelPreferredLength(inputLine);

            // For firewall T Intersections, Continue the boards. Do not stop at the intersection. 
            PanelUtils panelUtils = new PanelUtils(doc);
            List<XYZ> middleIntersections = panelUtils.ComputeMiddleIntersectionPts(inputLine, rightPanelIntersection.Count > 0 ? rightPanelIntersection : leftPanelIntersections, startpt, endPt);

            GenericUtils.AdjustWallEndPoints(inputLine, ref startpt, ref middleIntersections, ref endPt, linetype, panelDirection);

            wallEndPointsCollection.Add(startpt);
            wallEndPointsCollection.AddRange(middleIntersections);
            wallEndPointsCollection.Add(endPt);

            double dHourRate = GetHourRate(inputLine);

            AddStudsIfNeeded();
        }

        private PanelDirection ComputePanelDirectionForExteriorPanels(InputLine inputLine)
        {
            IOrderedEnumerable<Level> levels = ModelCreator.FindAndSortLevels(m_Document);
            Level toplevel = null, baselevel = null;

            LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X) ? LineType.vertical : LineType.Horizontal;

            if (linetype == LineType.Horizontal)
            {
                if (inputLine.strHorizontalPanelDirection == "D")
                    return PanelDirection.L;
                else
                    return PanelDirection.R;
            }
            else
            {
                if (inputLine.strVerticalPanelDirection == "R")
                    return PanelDirection.R;
                else
                    return PanelDirection.L;
            }
        }

        
        private XYZ ComputeStartPoint(LineRelations startReleation, InputLine inputLine, SortedDictionary<XYZ, string> endPanelIntersections, PanelDirection panelDirection)
        {
            XYZ StartPoint = null;

            KeyValuePair<XYZ, string> kvp = endPanelIntersections.ElementAt(0);
            XYZ intersectionPt = kvp.Key;
            string combinedString = kvp.Value.ToString();
            string[] tokens = combinedString.Split('|');

            string strWebWidth = tokens[1];
            string strWallType = tokens[0];

            LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X) ? LineType.vertical : LineType.Horizontal;
            double iInputLineWebWidth = GenericUtils.WebWidth(inputLine.strStudType);
            double iIntersectingLineWebWidth = GenericUtils.WebWidth(strWebWidth);

            double dHourrate = ComputeFireWallHourRate(inputLine);

            switch (startReleation)
            {
                case LineRelations.StartExtendRight:
                    if (linetype == LineType.Horizontal)
                    {
                        if (panelDirection == PanelDirection.R)
                        {
                            StartPoint = new XYZ(inputLine.startpoint.X + iIntersectingLineWebWidth + dHourrate,
                                                  inputLine.startpoint.Y + iInputLineWebWidth / 2,
                                                  inputLine.startpoint.Z);
                            // For 3-C condition the stud should coincide with the intersecting line web. So no need of hour rate correction
                            studpoints.Add( new XYZ(StartPoint.X - dHourrate, inputLine.startpoint.Y, StartPoint.Z));
                        }
                        else
                            StartPoint = new XYZ(inputLine.startpoint.X - dHourrate,
                                                    inputLine.startpoint.Y - iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Z);
                    }
                    else
                    {
                        if (panelDirection == PanelDirection.R)
                        {
                            StartPoint = new XYZ(inputLine.startpoint.X + iInputLineWebWidth / 2,
                                                  inputLine.startpoint.Y + iIntersectingLineWebWidth + dHourrate,
                                                  inputLine.startpoint.Z);
                            // For 3-C condition the stud should coincide with the intersecting line web. So no need of hour rate correction
                            studpoints.Add( new XYZ(inputLine.startpoint.X, StartPoint.Y - dHourrate, StartPoint.Z));
                        }
                        else
                            StartPoint = new XYZ(inputLine.startpoint.X - iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Y - dHourrate,
                                                    inputLine.startpoint.Z);
                    }
                    break;
                case LineRelations.StartExtendLeft:

                    if (linetype == LineType.Horizontal)
                    {
                        if (panelDirection == PanelDirection.L)
                        {
                            StartPoint = new XYZ(inputLine.startpoint.X + iIntersectingLineWebWidth + dHourrate,
                                                    inputLine.startpoint.Y - iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Z);
                            // For 3-C condition the stud should coincide with the intersecting line web. So no need of hour rate correction
                            studpoints.Add( new XYZ(StartPoint.X - dHourrate, inputLine.startpoint.Y, StartPoint.Z));
                        }
                        else
                            StartPoint = new XYZ(inputLine.startpoint.X - dHourrate,
                                                    inputLine.startpoint.Y + iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Z);
                    }
                    else
                    {
                        if (panelDirection == PanelDirection.L)
                        {
                            StartPoint = new XYZ(inputLine.startpoint.X - iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Y + iIntersectingLineWebWidth + dHourrate,
                                                    inputLine.startpoint.Z);
                            // For 3-C condition the stud should coincide with the intersecting line web. So no need of hour rate correction
                            studpoints.Add( new XYZ(inputLine.startpoint.X, StartPoint.Y - dHourrate, StartPoint.Z));
                        }
                        else
                            StartPoint = new XYZ(inputLine.startpoint.X + iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Y - dHourrate,
                                                    inputLine.startpoint.Z);
                    }
                    break;
                case LineRelations.StartTrimRight:
                    if (linetype == LineType.Horizontal)
                    {
                        if (panelDirection == PanelDirection.R)
                            StartPoint = new XYZ(inputLine.startpoint.X,
                                                    inputLine.startpoint.Y + iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Z);
                        else
                            StartPoint = new XYZ(inputLine.startpoint.X - iIntersectingLineWebWidth,
                                                    inputLine.startpoint.Y - iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Z);
                    }
                    else
                    {
                        if (panelDirection == PanelDirection.R)
                            StartPoint = new XYZ(inputLine.startpoint.X + iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Y,
                                                    inputLine.startpoint.Z);
                        else
                            StartPoint = new XYZ(inputLine.startpoint.X - iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Y - iIntersectingLineWebWidth,
                                                    inputLine.startpoint.Z);
                    }
                    break;
                case LineRelations.StartTrimLeft:
                    if (linetype == LineType.Horizontal)
                    {
                        if (panelDirection == PanelDirection.L)
                            StartPoint = new XYZ(inputLine.startpoint.X,
                                                    inputLine.startpoint.Y - iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Z);
                        else
                            StartPoint = new XYZ(inputLine.startpoint.X - iIntersectingLineWebWidth,
                                                    inputLine.startpoint.Y + iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Z);
                    }
                    else
                    {
                        if (panelDirection == PanelDirection.L)
                            StartPoint = new XYZ(inputLine.startpoint.X - iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Y,
                                                    inputLine.startpoint.Z);
                        else
                            StartPoint = new XYZ(inputLine.startpoint.X + iInputLineWebWidth / 2,
                                                    inputLine.startpoint.Y - iIntersectingLineWebWidth,
                                                    inputLine.startpoint.Z);
                    }
                    break;
                case LineRelations.StartTrimT:
                    if (linetype == LineType.Horizontal)
                    {

                        StartPoint = (panelDirection == PanelDirection.R) ? new XYZ(inputLine.startpoint.X, inputLine.startpoint.Y + iInputLineWebWidth / 2, inputLine.startpoint.Z)
                                         : new XYZ(inputLine.startpoint.X, inputLine.startpoint.Y - iInputLineWebWidth / 2, inputLine.startpoint.Z);
                    }
                    else
                    {
                        StartPoint = (panelDirection == PanelDirection.R) ? new XYZ(inputLine.startpoint.X + iInputLineWebWidth / 2, inputLine.startpoint.Y, inputLine.startpoint.Z)
                                             : new XYZ(inputLine.startpoint.X - iInputLineWebWidth / 2, inputLine.startpoint.Y, inputLine.startpoint.Z);

                    }
                    break;
            }
            return StartPoint;
        }

        private List<XYZ> ComputeMiddleIntersectionPts(InputLine inputLine, SortedDictionary<XYZ, string> sortedDictionary)
        {
            List<XYZ> middleIntersections = new List<XYZ>();
            foreach (KeyValuePair<XYZ, string> kvp in sortedDictionary)
            {
                XYZ intersectPt = kvp.Key;
                string combinedString = kvp.Value;

                string[] tokens = combinedString.Split('|');

                string strWebWidth = tokens[1];
                string strWallType = tokens[0];

                if (!(strWallType == "Fire" || strWallType == "Insulation" || strWallType == "Ex w/ Insulation"))
                    continue;

                LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X) ? LineType.vertical : LineType.Horizontal;
                double iInputLineWebWidth = GenericUtils.WebWidth(inputLine.strStudType);
                double iIntersectingLineWebWidth = GenericUtils.WebWidth(strWebWidth);

                double dHourrate = ComputeFireWallHourRate(inputLine);

                if (linetype == LineType.Horizontal)
                {
                    middleIntersections.Add(new XYZ(intersectPt.X - iIntersectingLineWebWidth / 2 - dHourrate, intersectPt.Y, intersectPt.Z));
                    middleIntersections.Add(new XYZ(intersectPt.X + iIntersectingLineWebWidth / 2 + dHourrate, intersectPt.Y, intersectPt.Z));
                }
                else
                {
                    middleIntersections.Add(new XYZ(intersectPt.X, intersectPt.Y - iIntersectingLineWebWidth / 2 - dHourrate, intersectPt.Z));
                    middleIntersections.Add(new XYZ(intersectPt.X, intersectPt.Y + iIntersectingLineWebWidth / 2 + dHourrate, intersectPt.Z));
                }
            }

            return middleIntersections;
        }

        private XYZ ComputerEndPoint(LineRelations endRelation, InputLine inputLine, SortedDictionary<XYZ, string> endPanelIntersections, PanelDirection panelDirection)
        {
            XYZ EndPoint = null;

            KeyValuePair<XYZ, string> kvp;
            if (endPanelIntersections.Count == 1)
                kvp = endPanelIntersections.ElementAt(0);
            else
                kvp = endPanelIntersections.ElementAt(1);

            XYZ intersectionPt = kvp.Key;
            string combinedString = kvp.Value.ToString();
            string[] tokens = combinedString.Split('|');

            string strWebWidth = tokens[1];
            string strWallType = tokens[0];

            LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X) ? LineType.vertical : LineType.Horizontal;
            double iInputLineWebWidth = GenericUtils.WebWidth(inputLine.strStudType);
            double iIntersectingLineWebWidth = GenericUtils.WebWidth(strWebWidth);

            double dHourrate = ComputeFireWallHourRate(inputLine);

            switch (endRelation)
            {
                case LineRelations.EndExtendRight:
                    if (linetype == LineType.Horizontal)
                    {
                        if (panelDirection == PanelDirection.R)
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X - iIntersectingLineWebWidth - dHourrate,
                                                inputLine.endpoint.Y + iInputLineWebWidth / 2,
                                                inputLine.endpoint.Z);
                            // For 3-C condition the stud should coincide with the intersecting line web. So no need of hour rate correction
                            studpoints.Add( new XYZ(EndPoint.X + dHourrate, inputLine.endpoint.Y, EndPoint.Z));
                        }
                        else
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X + dHourrate,
                                                inputLine.endpoint.Y - iInputLineWebWidth / 2,
                                                inputLine.endpoint.Z);
                        }
                    }
                    else
                    {
                        if (panelDirection == PanelDirection.R)
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X + iInputLineWebWidth / 2,
                                                inputLine.endpoint.Y - iIntersectingLineWebWidth - dHourrate,
                                                inputLine.endpoint.Z);
                            // For 3-C condition the stud should coincide with the intersecting line web. So no need of hour rate correction
                            studpoints.Add( new XYZ(inputLine.startpoint.X, EndPoint.Y + dHourrate, EndPoint.Z));
                        }
                        else
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X - iInputLineWebWidth / 2,
                                                inputLine.endpoint.Y + dHourrate,
                                                inputLine.endpoint.Z);
                        }
                    }
                    break;
                case LineRelations.EndExtendLeft:
                    if (linetype == LineType.Horizontal)
                    {
                        if (panelDirection == PanelDirection.L)
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X - iIntersectingLineWebWidth - dHourrate,
                                                inputLine.endpoint.Y - iInputLineWebWidth / 2,
                                                inputLine.endpoint.Z);
                            // For 3-C condition the stud should coincide with the intersecting line web. So no need of hour rate correction
                            studpoints.Add( new XYZ(EndPoint.X + dHourrate, inputLine.startpoint.Y, EndPoint.Z));
                        }
                        else
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X + dHourrate,
                                                inputLine.endpoint.Y + iInputLineWebWidth / 2,
                                                inputLine.endpoint.Z);
                        }
                    }
                    else
                    {
                        if (panelDirection == PanelDirection.L)
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X - iInputLineWebWidth / 2,
                                                inputLine.endpoint.Y - iIntersectingLineWebWidth - dHourrate,
                                                inputLine.endpoint.Z);
                            // For 3-C condition the stud should coincide with the intersecting line web. So no need of hour rate correction
                            studpoints.Add( new XYZ(inputLine.startpoint.X, EndPoint.Y + dHourrate, EndPoint.Z));
                        }
                        else
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X + iInputLineWebWidth / 2,
                                                inputLine.endpoint.Y + dHourrate,
                                                inputLine.endpoint.Z);
                        }
                    }
                    break;
                case LineRelations.EndTrimRight:
                    if (linetype == LineType.Horizontal)
                    {
                        if (panelDirection == PanelDirection.R)
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X,
                                                inputLine.endpoint.Y + iInputLineWebWidth / 2,
                                                inputLine.endpoint.Z);
                        }
                        else
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X + iIntersectingLineWebWidth,
                                                inputLine.endpoint.Y - iInputLineWebWidth / 2,
                                                inputLine.endpoint.Z);
                        }
                    }
                    else
                    {
                        if (panelDirection == PanelDirection.R)
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X + iInputLineWebWidth / 2,
                                                inputLine.endpoint.Y,
                                                inputLine.endpoint.Z);
                        }
                        else
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X - iInputLineWebWidth / 2,
                                                inputLine.endpoint.Y + iIntersectingLineWebWidth,
                                                inputLine.endpoint.Z);
                        }
                    }
                    break;

                case LineRelations.EndTrimLeft:
                    if (linetype == LineType.Horizontal)
                    {
                        if (panelDirection == PanelDirection.L)
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X,
                                                inputLine.endpoint.Y - iInputLineWebWidth / 2,
                                                inputLine.endpoint.Z);
                        }
                        else
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X + iIntersectingLineWebWidth,
                                                inputLine.endpoint.Y + iInputLineWebWidth / 2,
                                                inputLine.endpoint.Z);
                        }
                    }
                    else
                    {
                        if (panelDirection == PanelDirection.L)
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X - iInputLineWebWidth / 2,
                                                inputLine.endpoint.Y,
                                                inputLine.endpoint.Z);
                        }
                        else
                        {
                            EndPoint = new XYZ(inputLine.endpoint.X + iInputLineWebWidth / 2,
                                                inputLine.endpoint.Y + iIntersectingLineWebWidth,
                                                inputLine.endpoint.Z);
                        }
                    }
                    break;
                case LineRelations.EndTrimT:
                    if (linetype == LineType.Horizontal)
                    {

                        EndPoint = (panelDirection == PanelDirection.R) ? new XYZ(inputLine.endpoint.X, inputLine.endpoint.Y + iInputLineWebWidth / 2, inputLine.endpoint.Z)
                                         : new XYZ(inputLine.endpoint.X, inputLine.endpoint.Y - iInputLineWebWidth / 2, inputLine.endpoint.Z);
                    }
                    else
                    {
                        EndPoint = (panelDirection == PanelDirection.R) ? new XYZ(inputLine.endpoint.X + iInputLineWebWidth / 2, inputLine.endpoint.Y, inputLine.endpoint.Z)
                                             : new XYZ(inputLine.endpoint.X - iInputLineWebWidth / 2, inputLine.endpoint.Y, inputLine.endpoint.Z);

                    }
                    break;

            }
            return EndPoint;
        }

        public void ComputePanelLength()
        {
            throw new NotImplementedException();
        }

        public void ModifyHeight()
        {
            throw new NotImplementedException();
        }

        public void RemoveStudsIfNeeded()
        {
            throw new NotImplementedException();
        }

        private void CheckStartAndEndIntersectRelationsForFireWall(InputLine inputLine, SortedDictionary<XYZ, string> endPanelIntersections, ref LineRelations startReleation, ref LineRelations endRelation)
        {
            LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.Y, inputLine.endpoint.Y) ? LineType.Horizontal : LineType.vertical;

            XYZ startPt = null, endPoint = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out startPt, out endPoint);

            bool bAtStart = false, bExtending = false, bRight = false;

            foreach (KeyValuePair<XYZ, string> kvp in endPanelIntersections)
            {
                XYZ intersectPt = kvp.Key;

                string combinedString = kvp.Value;

                string[] tokens = combinedString.Split('|');

                string strWebWidth = tokens[1];
                string strWallType = tokens[0];

                // Check if the given line is at start or end
                if (linetype == LineType.Horizontal)
                {
                    bAtStart = (Math.Abs(intersectPt.X - startPt.X) < Math.Abs(intersectPt.X - endPoint.X));

                    if (!(strWallType == "Ex w/ Insulation"))
                    {
                        if (bAtStart)
                            startReleation = LineRelations.NoStartIntersection;
                        else
                            endRelation = LineRelations.NoEndIntersection;

                        continue;
                    }
                    // Check if the given point is trim / intersection point
                    if (bAtStart)
                    {
                        bExtending = startPt.X < intersectPt.X;
                        if (bExtending)
                        {
                            bRight = intersectPt.Y > startPt.Y;

                            startReleation = bRight ? LineRelations.StartExtendRight : LineRelations.StartExtendLeft;
                        }
                        else
                        {
                            if ((intersectPt.Y > startPt.Y) && (intersectPt.Y - startPt.Y < 1.0))
                                startReleation = LineRelations.StartTrimLeft;
                            if ((intersectPt.Y < startPt.Y) && (startPt.Y - intersectPt.Y < 1.0))
                                startReleation = LineRelations.StartTrimRight;
                            if (((intersectPt.Y > startPt.Y) && (intersectPt.Y - startPt.Y > 1.0)) || ((intersectPt.Y < startPt.Y) && (startPt.Y - intersectPt.Y > 1.0)))
                                startReleation = LineRelations.StartTrimT;
                        }
                    }
                    else
                    {
                        bExtending = intersectPt.X < endPoint.X;
                        if (bExtending)
                        {
                            bRight = intersectPt.Y > endPoint.Y;

                            endRelation = bRight ? LineRelations.EndExtendRight : LineRelations.EndExtendLeft;
                        }
                        else
                        {
                            if ((intersectPt.Y > endPoint.Y) && (intersectPt.Y - endPoint.Y < 1.0))
                                endRelation = LineRelations.EndTrimLeft;
                            if ((intersectPt.Y < endPoint.Y) && (endPoint.Y - intersectPt.Y < 1.0))
                                endRelation = LineRelations.EndTrimRight;
                            if (((intersectPt.Y > endPoint.Y) && (intersectPt.Y - endPoint.Y > 1.0)) || ((intersectPt.Y < endPoint.Y) && (endPoint.Y - intersectPt.Y > 1.0)))
                                endRelation = LineRelations.EndTrimT;
                        }
                    }
                }
                else
                {
                    bAtStart = (Math.Abs(intersectPt.Y - startPt.Y) < Math.Abs(intersectPt.Y - endPoint.Y));

                    if (!(strWallType == "Ex w/ Insulation"))
                    {
                        if (bAtStart)
                            startReleation = LineRelations.NoStartIntersection;
                        else
                            endRelation = LineRelations.NoEndIntersection;

                        continue;
                    }

                    // Check if the given point is trim / intersection point
                    if (bAtStart)
                    {
                        bExtending = intersectPt.Y > startPt.Y;
                        if (bExtending)
                        {
                            bRight = intersectPt.X > startPt.X;

                            startReleation = bRight ? LineRelations.StartExtendRight : LineRelations.StartExtendLeft;
                        }
                        else
                        {
                            if ((intersectPt.X > startPt.X) && (intersectPt.X - startPt.X < 1.0))
                                startReleation = LineRelations.StartTrimLeft;
                            if ((intersectPt.X < startPt.X) && (startPt.X - intersectPt.X < 1.0))
                                startReleation = LineRelations.StartTrimRight;
                            if (((startPt.X > intersectPt.X) && (startPt.X - intersectPt.X > 1.0)) || ((startPt.X < intersectPt.X) && (intersectPt.X - startPt.X > 1.0)))
                                startReleation = LineRelations.StartTrimT;
                        }
                    }
                    else
                    {
                        bExtending = intersectPt.Y < endPoint.Y;
                        if (bExtending)
                        {
                            bRight = intersectPt.X > endPoint.X;

                            endRelation = bRight ? LineRelations.EndExtendRight : LineRelations.EndExtendLeft;
                        }
                        else
                        {
                            if ((intersectPt.X > endPoint.X) && (intersectPt.X - endPoint.X < 1.0))
                                endRelation = LineRelations.EndTrimLeft;
                            if ((intersectPt.X < endPoint.X) && (endPoint.X - intersectPt.X < 1.0))
                                endRelation = LineRelations.EndTrimRight;
                            if (((endPoint.X > intersectPt.X) && (endPoint.X - intersectPt.X > 1.0)) || ((endPoint.X < intersectPt.X) && (intersectPt.X - endPoint.X > 1.0)))
                                endRelation = LineRelations.EndTrimT;
                        }
                    }
                }
            }
        }

        // For normal panels we take as per the X-Y table
        private double GetHourRate(InputLine inputLine)
        {
            double dHourrate = 0.0;

            // Check if the hour rate is present on the line. If not check the project settings
            if (inputLine.dHourrate == 0)
            {
                PanelTypeGlobalParams pg = string.IsNullOrEmpty(inputLine.strPanelType) ?
                                GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                                GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == inputLine.strPanelType);

                dHourrate = pg.iPanelHourRate;
            }
            else
                dHourrate += inputLine.dHourrate;

            return  dHourrate == 0 ? 1 : dHourrate;
        }

        // For firewall we take it as HourRate * (5/8)"
        private double ComputeFireWallHourRate(InputLine line)
        {  
            // Get Hour Rate
            double dHourrate = GetHourRate(line);

            // Get Panel Thickness
            double dPanelthickness = GenericUtils.GetPanelWidth(line);
            
            double hourrate = dHourrate * dPanelthickness;

            return hourrate;
        }

        private double GetPanelPreferredLength(InputLine line)
        {
            PanelTypeGlobalParams pg = string.IsNullOrEmpty(line.strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == line.strPanelType);

            double dPanelPreferredLength = pg.iPanelPreferredLength;

            return dPanelPreferredLength;
        }
    }
}

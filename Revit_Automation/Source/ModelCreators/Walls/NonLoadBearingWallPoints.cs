using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Interfaces;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.ModelCreators.Walls
{
    public class NonLoadBearingWallPoints : IWallPointsGenerator
    {
        private bool bExteriorAtStart = false;
        private bool bExteriorAtEnd = false;
        private ElementId iDStartIntersectingLine = null;
        private ElementId iDEndIntersectingLine = null;
        private XYZ startStudPt;
        private XYZ endStudPt;
        private Document m_Document = null;
        private InputLine m_inputLine;

        public void AddStudsIfNeeded()
        {
            InputLine startInputLine = InputLineUtility.GetInputLineFromID(iDStartIntersectingLine);
            InputLine EndInputLine = InputLineUtility.GetInputLineFromID(iDEndIntersectingLine);

            if (bExteriorAtStart)
                PostCreationUtils.PlaceStudAtPoint(m_Document, startStudPt, startInputLine);

            if (bExteriorAtEnd)
                PostCreationUtils.PlaceStudAtPoint(m_Document, endStudPt, EndInputLine);
        }

        public void ComputeEndPoints(Document doc, InputLine inputLine, SortedDictionary<XYZ, string> rightPanelIntersection, SortedDictionary<XYZ, string> leftPanelIntersections, SortedDictionary<XYZ, string> endPanelIntersections, ref List<XYZ> wallEndPointsCollection)
        {
            m_Document = doc;
            m_inputLine = inputLine;

            LineRelations startReleation = LineRelations.NoStartIntersection;
            LineRelations endRelation = LineRelations.NoEndIntersection;

            if (endPanelIntersections.Count > 0)
            {
                ComputeStartAndEndRelations(inputLine, endPanelIntersections, ref startReleation, ref endRelation);
            }

            XYZ startpt = null, endPt = null;
            PanelDirection panelDirection = PanelDirection.B;
            List<XYZ> intermediatePts = new List<XYZ>();

            XYZ startpoint = null, endpoint = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out startpoint, out endpoint);

            panelDirection = ComputePanelDirection(inputLine);
            
            // Up is right and Down is left
            if (panelDirection == PanelDirection.U)
                panelDirection = PanelDirection.R;
            if (panelDirection == PanelDirection.D)
                panelDirection = PanelDirection.L;

            LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X) ? LineType.vertical : LineType.Horizontal;
            double dWebwidth = GenericUtils.WebWidth(inputLine.strStudType);

            startpt = ComputeStartPoint(startReleation, inputLine, endPanelIntersections);

            if (linetype == LineType.Horizontal)
            {
                startpt = (panelDirection == PanelDirection.R) ? new XYZ(startpt.X, startpt.Y + dWebwidth / 2, startpt.Z)
                                 : new XYZ(startpt.X, startpt.Y - dWebwidth / 2, startpt.Z);
                
                if (startStudPt != null)
                    startStudPt = (panelDirection == PanelDirection.R) ? new XYZ(startStudPt.X, startStudPt.Y + dWebwidth / 2, startStudPt.Z)
                                 : new XYZ(startStudPt.X, startStudPt.Y - dWebwidth / 2, startStudPt.Z);
            }
            else
            {
                startpt = (panelDirection == PanelDirection.R) ? new XYZ(startpt.X + dWebwidth / 2, startpt.Y, startpt.Z)
                                     : new XYZ(startpt.X - dWebwidth / 2, startpt.Y, startpt.Z);

                if (startStudPt != null)
                    startStudPt = (panelDirection == PanelDirection.R) ? new XYZ(startStudPt.X + dWebwidth / 2, startStudPt.Y, startStudPt.Z)
                                     : new XYZ(startStudPt.X - dWebwidth / 2, startStudPt.Y, startStudPt.Z);
            }

            endPt = ComputeEndPoint(endRelation, inputLine, endPanelIntersections);

            if (linetype == LineType.Horizontal)
            {

                endPt = (panelDirection == PanelDirection.R) ? new XYZ(endPt.X, endPt.Y + dWebwidth / 2, endPt.Z)
                                 : new XYZ(endPt.X, endPt.Y - dWebwidth / 2, endPt.Z);

                if (endStudPt != null)
                    endStudPt = (panelDirection == PanelDirection.R) ? new XYZ(endStudPt.X, endStudPt.Y + dWebwidth / 2, endStudPt.Z)
                                 : new XYZ(endStudPt.X, endStudPt.Y - dWebwidth / 2, endStudPt.Z);

            }
            else
            {
                endPt = (panelDirection == PanelDirection.R) ? new XYZ(endPt.X + dWebwidth / 2, endPt.Y, endPt.Z)
                                     : new XYZ(endPt.X - dWebwidth / 2, endPt.Y, endPt.Z);

                if (endStudPt != null)
                    endStudPt = (panelDirection == PanelDirection.R) ? new XYZ(endStudPt.X + dWebwidth / 2, endStudPt.Y, endStudPt.Z)
                                     : new XYZ(endStudPt.X - dWebwidth / 2, endStudPt.Y, endStudPt.Z);

            }


            XYZ AdditionVector = null;
            double dPanelPreferredLength = GetPanelPreferredLength(inputLine);

            if (linetype == LineType.Horizontal)
                AdditionVector = new XYZ(dPanelPreferredLength, 0, 0);
            else
                AdditionVector = new XYZ(0, dPanelPreferredLength, 0);

            XYZ middlePoint = startpt;

            while (true)
            {
                middlePoint = middlePoint + AdditionVector;

                if ((linetype == LineType.Horizontal && middlePoint.X < endPt.X) ||
                    (linetype == LineType.vertical && middlePoint.Y < endPt.Y))
                {
                    // Add middle point 2 times, the processing order is
                    // 1-2, 2-3, 3-4, 4-5, and so on
                    intermediatePts.Add(middlePoint);
                    intermediatePts.Add(middlePoint);
                }
                else
                    break;
            }


            GenericUtils.AdjustWallEndPoints(inputLine, ref startpt, ref intermediatePts, ref endPt, linetype, panelDirection);

            wallEndPointsCollection.Add(startpt);
            wallEndPointsCollection.AddRange(intermediatePts);
            wallEndPointsCollection.Add(endPt);

            AddStudsIfNeeded();
        }

        private XYZ ComputeEndPoint(LineRelations endRelation, InputLine inputLine, SortedDictionary<XYZ, string> endPanelIntersections)
        {
            XYZ EndPoint = inputLine.endpoint;

            if (endPanelIntersections.Count > 0)
            {
                KeyValuePair<XYZ, string> kvp = endPanelIntersections.ElementAt(endPanelIntersections.Count > 1 ? 1 : 0);
                XYZ intersectionPt = kvp.Key;

                string combinedString = kvp.Value.ToString();
                string[] tokens = combinedString.Split('|');
                string strWallType = tokens[0];
                double iIntersectingLineWebWidth = GenericUtils.WebWidth(tokens[1]);
                PanelDirection intersectLinePanelDirection = (PanelDirection)Enum.Parse(typeof(PanelDirection), tokens[2]);

                // Up is right and Down is left
                if (intersectLinePanelDirection == PanelDirection.U)
                    intersectLinePanelDirection = PanelDirection.R;
                if (intersectLinePanelDirection == PanelDirection.D)
                    intersectLinePanelDirection = PanelDirection.L;

                LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X) ? LineType.vertical : LineType.Horizontal;

                if (linetype == LineType.Horizontal)
                    intersectionPt = new XYZ(intersectionPt.X, inputLine.startpoint.Y, inputLine.startpoint.Z);
                else
                    intersectionPt = new XYZ(inputLine.startpoint.X, intersectionPt.Y, inputLine.startpoint.Z);

                if (endRelation == LineRelations.EndTrimT && intersectLinePanelDirection == PanelDirection.R)
                {
                    XYZ lineDirection = inputLine.endpoint - inputLine.startpoint;
                    RemoveStudAtPoint(inputLine.endpoint, lineDirection);

                    endStudPt = intersectionPt;
                    
                    if (linetype == LineType.Horizontal)
                        EndPoint = intersectionPt + new XYZ(iIntersectingLineWebWidth / 2.0, 0, 0);
                    else
                        EndPoint = intersectionPt + new XYZ(0, iIntersectingLineWebWidth / 2.0, 0);

                    
                    bExteriorAtEnd = true;

                    int elementID = int.Parse(tokens[3]);
                    iDEndIntersectingLine = new ElementId(elementID);
                }
            }
            return EndPoint;
        }

        private XYZ ComputeStartPoint(LineRelations startRelation, InputLine inputLine, SortedDictionary<XYZ, string> endPanelIntersections)
        {
            XYZ StartPoint = inputLine.startpoint;

            if (endPanelIntersections.Count > 0)
            {
                KeyValuePair<XYZ, string> kvp = endPanelIntersections.ElementAt(0);
                XYZ intersectionPt = kvp.Key;

                string combinedString = kvp.Value.ToString();
                string[] tokens = combinedString.Split('|');
                string strWallType = tokens[0];
                double iIntersectingLineWebWidth = GenericUtils.WebWidth(tokens[1]);
                PanelDirection intersectLinePanelDirection = (PanelDirection)Enum.Parse(typeof(PanelDirection),  tokens[2]);

                // Up is right and Down is left
                if (intersectLinePanelDirection == PanelDirection.U)
                    intersectLinePanelDirection = PanelDirection.R;
                if (intersectLinePanelDirection == PanelDirection.D)
                    intersectLinePanelDirection = PanelDirection.L;

                LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X) ? LineType.vertical : LineType.Horizontal;

                if (linetype == LineType.Horizontal)
                    intersectionPt = new XYZ(intersectionPt.X, inputLine.startpoint.Y, inputLine.startpoint.Z);
                else
                    intersectionPt = new XYZ(inputLine.startpoint.X, intersectionPt.Y, inputLine.startpoint.Z);

                if (startRelation == LineRelations.StartTrimT && intersectLinePanelDirection == PanelDirection.L)
                {
                    XYZ lineDirection = inputLine.endpoint - inputLine.startpoint;
                    RemoveStudAtPoint(inputLine.startpoint, lineDirection);

                    startStudPt = intersectionPt;

                    if (linetype == LineType.Horizontal)
                        StartPoint = intersectionPt + new XYZ(-iIntersectingLineWebWidth / 2.0, 0, 0);
                    else
                        StartPoint = intersectionPt + new XYZ(0, -iIntersectingLineWebWidth / 2.0, 0);


                    bExteriorAtStart = true;

                    int elementID = int.Parse(tokens[3]);
                    iDStartIntersectingLine = new ElementId(elementID);
                }
            }
            return StartPoint;
        }
        private PanelDirection ComputePanelDirection(InputLine inputLine)
        {
            // Panel Direction computation has the following priority.
            // Line > Settings > Automatic Computation

            PanelDirection panelDirection = PanelDirection.B;

            // Get Line End points.
            XYZ pt1 = null, pt2 = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);

            // Get the orientation of the line
            LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

            string strPanelDirection = (lineType == LineType.Horizontal) ?
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
                    PanelTypeGlobalParams pg = string.IsNullOrEmpty(inputLine.strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == inputLine.strPanelType);

                    string strPanelDir = (lineType == LineType.Horizontal) ? pg.strPanelHorizontalDirection : pg.strPanelVerticalDirection;
                    panelDirection = (PanelDirection)Enum.Parse(typeof(PanelDirection), strPanelDir);
                }
                return panelDirection;
            }
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

        private void RemoveStudAtPoint(XYZ endpoint, XYZ webOrientation)
        {
            PostCreationUtils.RemoveStudAtPoint(endpoint, webOrientation, m_Document);
        }

        private double GetPanelPreferredLength(InputLine line)
        {
            PanelTypeGlobalParams pg = string.IsNullOrEmpty(line.strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == line.strPanelType);

            double dPanelPreferredLength = pg.iPanelPreferredLength;

            return dPanelPreferredLength;
        }

        private void ComputeStartAndEndRelations(InputLine inputLine, SortedDictionary<XYZ, string> endPanelIntersections, ref LineRelations startReleation, ref LineRelations endRelation)
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
    }
}

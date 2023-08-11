// This file is part of the  R A N O R E X  Project. | http://www.ranorex.com

using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Revit_Automation.CustomTypes;
using Revit_Automation.Dialogs;
using Revit_Automation.Source.Interfaces;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Policy;

namespace Revit_Automation.Source.Preprocessors
{
    public class LineTrimmer : IPreprocessInterface
    {
        public Document m_Document;

        private LineProcessing m_LineProcessing;
        private Selection m_Selection;
        private bool m_bProcessSelected;
        public LineTrimmer(Document doc, LineProcessing form, bool bProcessSelected, Selection selection = null) { m_Document = doc; m_LineProcessing = form; m_bProcessSelected = bProcessSelected; m_Selection = selection; }


        public enum IntersectionType
        {
            TIntersection = 0,
            LIntersection,
            NonIntesecting
        }

        /// <summary>
        /// This method will look for all L joints in the model and extend them as per the below priority
        ///Fire, Insulation, ExWithInsulation, Ex, LB, LBS, NLB, NLBS
        /// </summary>
        public void Preprocess()
        {
            using (Transaction tx = new Transaction(m_Document))
            {

                // Gather all Input Lines
                InputLineUtility.GatherInputLines(m_Document, m_bProcessSelected, m_Selection, CommandCode.Posts, false);

                if (InputLineUtility.colInputLines.Count == 0)
                    return;

                tx.Start("Trimming Lines ");

                m_LineProcessing.Show();
                m_LineProcessing.Visible = true;
                Thread.Sleep(2000);

                m_LineProcessing.LineTrimmingMessage(string.Format("Executing Line Trimming Algorithm \n "), 0);

                // Iterate through the collection and Identify the Intersecting Lines
                for (int i = 0; i < InputLineUtility.colInputLines.Count; i++)
                {
                    InputLine inputLine1 = InputLineUtility.colInputLines[i];
                    m_LineProcessing.Refresh();
                    m_LineProcessing.LineTrimmingMessage(string.Format("\n Now Processing Line : {0} \n", inputLine1.id), 1);

                    bool bHasLIntersections = false;
                    for (int j = i + 1; j < InputLineUtility.colInputLines.Count; j++)
                    {
                        InputLine inputLine2 = InputLineUtility.colInputLines[j];
                        LineType lt1 = MathUtils.ApproximatelyEqual(inputLine1.startpoint.X, inputLine1.endpoint.X) ? LineType.vertical : LineType.Horizontal;
                        LineType lt2 = MathUtils.ApproximatelyEqual(inputLine2.startpoint.X, inputLine2.endpoint.X) ? LineType.vertical : LineType.Horizontal;

                        // We check only for perpendicular lines
                        if ((lt1 == LineType.vertical && lt2 == LineType.Horizontal) || (lt2 == LineType.vertical && lt1 == LineType.Horizontal))
                        {
                            XYZ intersectionPT = null;
                            ElementId iContinousLine = null;
                            IntersectionType intesection = IdentifyRelationShip(inputLine1, inputLine2, out intersectionPT, out iContinousLine);

                            // Only in case of L-Intersection we extend lines
                            if (intesection == IntersectionType.TIntersection)
                            {
                                m_LineProcessing.LineTrimmingMessage(string.Format("There is an T-Intersection Between {0} ({2}) and {1} ({3}) lines \n", inputLine1.id, inputLine2.id, inputLine1.strWallType, inputLine2.strWallType), 2);
                                TrimNonContinuousLine (ref inputLine1, ref inputLine2, intersectionPT, iContinousLine);
                                bHasLIntersections = true;

                                InputLineUtility.colInputLines[i] = inputLine1;
                                InputLineUtility.colInputLines[j] = inputLine2;
                            }
                        }
                    }

                    if (!bHasLIntersections)
                        m_LineProcessing.LineTrimmingMessage(string.Format("No T-Intersections. Nothing to do \n", inputLine1.id), 4);
                }

                // Clone Line
                UpdateParametersForNewLines();

                m_LineProcessing.LineTrimmingMessage(" \n 😊 😀 😉 😍 Finished Trimming  😂 😄 😎  \n", 3);

                tx.Commit();
            }
        }

        private void TrimNonContinuousLine(ref InputLine inputLine1, ref InputLine inputLine2, XYZ intersectionPT, ElementId elemID)
        {
            ElementId elemID1 = inputLine1.id;
            ElementId elemID2 = inputLine2.id;

            ref InputLine lineToTrim = ref inputLine1;
            ref InputLine lineToRemain = ref inputLine2;
            
            if (elemID == elemID1)
            {
                lineToTrim = ref inputLine2;
                lineToRemain = ref inputLine1;
            }

            TrimLine(ref lineToTrim, ref lineToRemain, intersectionPT);
        }

        private void TrimLine(ref InputLine lineToTrim, ref InputLine lineToRemain, XYZ intersectionPT)
        {
            m_LineProcessing.LineTrimmingMessage(string.Format("Trimming Line {0} as it is non-continuous among {1} , {2} \n", lineToTrim.id, lineToTrim.id, lineToRemain.id), 4);

            // Total length of trimming
            double dTrimDistance = 0.0;

            // Get web width of non-Trimming line 
            double dWebWidth = GenericUtils.WebWidth(lineToRemain.strStudType) / 2;

            // Get Panel Trim Distance
            double dPanelTrim = GetPanelTrimDistance(lineToTrim,  lineToRemain, intersectionPT);

            // Final trim accounts for both stud width and Panel width
            dTrimDistance = dWebWidth + dPanelTrim;
            
            // Get the Element
            FamilyInstance lineElement = m_Document.GetElement(lineToTrim.id) as FamilyInstance;

            // Get the End points
            LocationCurve locCurve = lineElement.Location as LocationCurve;

            XYZ startpoint = null, endpoint = null;
            GenericUtils.GetlineStartAndEndPoints(lineElement, out startpoint, out endpoint);

            LineType lineType = MathUtils.ApproximatelyEqual(startpoint.X, endpoint.X) ? LineType.vertical : LineType.Horizontal;

            XYZ updatedStartPt = null, updatedEndPt = null;

            // See which end needs to be trimmed.
            if (MathUtils.ApproximatelyEqual(startpoint.X, intersectionPT.X) && MathUtils.ApproximatelyEqual(startpoint.Y, intersectionPT.Y))
            {
                if (lineType == LineType.Horizontal)
                    updatedStartPt = new XYZ(startpoint.X + dTrimDistance, startpoint.Y, startpoint.Z);
                else
                    updatedStartPt = new XYZ(startpoint.X, startpoint.Y + dTrimDistance, startpoint.Z);

                updatedEndPt = endpoint;
            }
            else
            {
                updatedStartPt = startpoint;

                if (lineType == LineType.Horizontal)
                    updatedEndPt = new XYZ(endpoint.X - dTrimDistance, endpoint.Y, endpoint.Z);
                else
                    updatedEndPt = new XYZ(endpoint.X, endpoint.Y - dTrimDistance, endpoint.Z);
            }

            //Create a new line with given end points
            Line newInputLine = Line.CreateBound(updatedStartPt, updatedEndPt);

            FamilySymbol inputLineSym = SymbolCollector.GetInputLineSymbol();

            if (inputLineSym != null && !inputLineSym.IsActive)
                inputLineSym.Activate();

            Level level = lineElement.Host as Level;

            FamilyInstance extendedLine = m_Document.Create.NewFamilyInstance(newInputLine, inputLineSym, level, StructuralType.NonStructural);

            // Save it as location curve of the element
            locCurve.Curve = newInputLine;

            // Delete the line
            m_Document.Delete(lineToTrim.id);

            // update the Input Line
            lineToTrim.locationCurve = locCurve;
            lineToTrim.id = extendedLine.Id;
            lineToTrim.bLineExtendedOrTrimmed = true;
            lineToTrim.startpoint = updatedStartPt;
            lineToTrim.endpoint = updatedEndPt;

        }

        private double GetPanelTrimDistance(InputLine lineToTrim, InputLine lineToRemain, XYZ intersectionPT)
        {
            double dTrimDistance = 0.0;
            double dPanelThickness = 0.0;
            string strPanelDirection = "";
            string strLineDirection = "";

            bool bTintersection = false;
            //Two wall lines are of same type and are either Fire, Insulation or Ex w/ Insulation
            if (lineToTrim.strWallType == lineToRemain.strWallType
                && (lineToTrim.strWallType == "Fire" ||
                    lineToTrim.strWallType == "Insulation" ||
                    lineToTrim.strWallType == "Ex w/ Insulation")
                    )
            {
                bTintersection = CheckPanelsForTIntersection(lineToTrim, lineToRemain);

                if (!bTintersection)
                    return 0.0;
            }

            
            //Panel Thickness and direction based on the Panel type parameter
            // if empty - Take the values from UNO row
            // else take the values from the corresponding row in the global settings

            PanelTypeGlobalParams pg = string.IsNullOrEmpty(lineToRemain.strPanelType) ?
                                        GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                                        GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == lineToRemain.strPanelType);
            
            // Panel Thickness
            dPanelThickness = GetThickness(pg.iPanelHourRate);

            // Panel Direction 
            if (MathUtils.ApproximatelyEqual(lineToRemain.startpoint.X, lineToRemain.endpoint.X))
                strPanelDirection = pg.strPanelVerticalDirection.ToString();
            else
                strPanelDirection = pg.strPanelHorizontalDirection.ToString();

            // For Exteriror or Fire wall or Insulating wall, panels will be placed on both sides
            if (lineToRemain.strWallType == "Fire" ||
                lineToRemain.strWallType == "Insulation" ||
                lineToRemain.strWallType == "Ex w/ Insulation")
            {
                strPanelDirection = "B";
            }


            // Trimming line orientation with respect to continuous line
            strLineDirection = GetTrimLineDirectionWrtContinuousLine(lineToRemain, lineToTrim);

            // if Trimming line is on the same side as continuous line panel direction add panel thickness to trim distance
            if (strPanelDirection == "B" || (strPanelDirection == strLineDirection))
            {
                dTrimDistance = dPanelThickness;
            }

            return dTrimDistance;
        }

        private bool CheckPanelsForTIntersection(InputLine lineToTrim, InputLine lineToRemain)
        {

            XYZ trimlineStart = null, trimlineEnd = null, RemainLineStart = null, RemainLineEnd = null;
            GenericUtils.GetlineStartAndEndPoints(lineToTrim, out trimlineStart, out trimlineEnd);
            GenericUtils.GetlineStartAndEndPoints(lineToRemain, out RemainLineStart, out RemainLineEnd);

            // Get the line type
            LineType TrimLineType = MathUtils.ApproximatelyEqual(trimlineStart.X , trimlineEnd.X) ? LineType.vertical : LineType.Horizontal;

            if (TrimLineType == LineType.vertical)
            {
                if (Math.Abs(trimlineStart.X - RemainLineStart.X) > 1.0 && Math.Abs(trimlineStart.X - RemainLineEnd.X) > 1.0)
                    return true;
            }
            else
            {
                if (Math.Abs(trimlineStart.Y - RemainLineStart.Y) > 1.0 && Math.Abs(trimlineStart.Y - RemainLineEnd.Y) > 1.0)
                    return true;
            }
            return false;
        }

        private string GetTrimLineDirectionWrtContinuousLine(InputLine lineToRemain, InputLine lineToTrim)
        {
            string strRelation = "";
            if (MathUtils.ApproximatelyEqual(lineToRemain.startpoint.X, lineToRemain.endpoint.X))
            {
                if (((lineToTrim.startpoint.X < lineToRemain.startpoint.X) || (MathUtils.ApproximatelyEqual (lineToTrim.startpoint.X, lineToRemain.startpoint.X))) && 
                    ((lineToTrim.endpoint.X < lineToRemain.endpoint.X) || (MathUtils.ApproximatelyEqual(lineToTrim.endpoint.X, lineToRemain.endpoint.X))))
                {
                    strRelation = "L";
                }
                else
                {
                    strRelation = "R";
                }
            }
            else
            {
                if (((lineToTrim.startpoint.Y < lineToRemain.startpoint.Y) || (MathUtils.ApproximatelyEqual(lineToTrim.startpoint.Y, lineToRemain.startpoint.Y))) &&
                    ((lineToTrim.endpoint.Y < lineToRemain.endpoint.Y) || (MathUtils.ApproximatelyEqual(lineToTrim.endpoint.Y, lineToRemain.endpoint.Y))))
                {
                    strRelation = "D";
                }
                else
                {
                    strRelation = "U";
                }
            }
            return strRelation;
        }

        private double GetThickness(double iPanelHourRate)
        {
            if (iPanelHourRate == 0 || iPanelHourRate == 1)
                return 1.0 / 12;
            if (iPanelHourRate == 2 || iPanelHourRate == 3)
                return 2.0 / 12;
            if (iPanelHourRate == 4)
                return 3.0 / 12;

            return 1.0 / 12;
        }

        private void UpdateParametersForNewLines()
        {
            foreach (InputLine iLine in InputLineUtility.colInputLines)
            {
                if (iLine.bLineExtendedOrTrimmed)
                {
                    Element InputLineElem = m_Document.GetElement(iLine.id);
                    InputLineElem.LookupParameter("Additional Panel")?.Set(iLine.strAdditionalPanel);
                    InputLineElem.LookupParameter("Additional Panel Gauge")?.Set(iLine.strAdditionalPanelGuage);
                    InputLineElem.LookupParameter("Beam Size")?.Set(iLine.strBeamSize);
                    InputLineElem.LookupParameter("Bracing")?.Set(iLine.strBracing);
                    InputLineElem.LookupParameter("Cee Header Gauge")?.Set(iLine.strCHeaderGuage);
                    InputLineElem.LookupParameter("Cee Header Quantity")?.Set(iLine.strCHeaderQuantity);
                    InputLineElem.LookupParameter("Cee Header Size")?.Set(iLine.strCHeaderSize);
                    InputLineElem.LookupParameter("Color")?.Set(iLine.strColor);
                    InputLineElem.LookupParameter("HSS Type")?.Set(iLine.strHSSType);
                    InputLineElem.LookupParameter("Horizontal Panel Direction")?.Set(iLine.strHorizontalPanelDirection);
                    InputLineElem.LookupParameter("Vertical Panelwdwdf Direction")?.Set(iLine.strVerticalPanelDirection);
                    InputLineElem.LookupParameter("Material")?.Set(iLine.strMaterial);
                    InputLineElem.LookupParameter("Panel Type")?.Set(iLine.strPanelType);
                    InputLineElem.LookupParameter("Partition Panel Gauge")?.Set(iLine.strPartitionPanelGuage);
                    InputLineElem.LookupParameter("Roof System")?.Set(iLine.strRoofSystem);
                    InputLineElem.LookupParameter("Row Name")?.Set(iLine.strRowName);
                    InputLineElem.LookupParameter("Color (Door Header)")?.Set(iLine.strColorDoorHeader);
                    InputLineElem.LookupParameter("HSS Height")?.Set(iLine.dHSSHeight);
                    InputLineElem.LookupParameter("Material Height")?.Set(iLine.dMaterialHeight);
                    InputLineElem.LookupParameter("Material Thickness")?.Set(iLine.dMaterialThickness);
                    InputLineElem.LookupParameter("Panel Offset Height")?.Set(iLine.dPanelOffsetHeight);
                    InputLineElem.LookupParameter("Partition Panel Each Side (Y/N)")?.Set(iLine.dPartitionPanelEachSide);
                    InputLineElem.LookupParameter("Stud Gauge")?.Set(iLine.strStudGuage);
                    InputLineElem.LookupParameter("Stud Size")?.Set(iLine.strStudType);
                    InputLineElem.LookupParameter("T62 Gauge")?.Set(iLine.strT62Guage);
                    InputLineElem.LookupParameter("T62 Type")?.Set(iLine.strT62Type);
                    InputLineElem.LookupParameter("Wall Type")?.Set(iLine.strWallType);
                    InputLineElem.LookupParameter("Top Track Gauge")?.Set(iLine.strTopTrackGuage);
                    InputLineElem.LookupParameter("Top Track Size")?.Set(iLine.strTopTrackSize);
                    InputLineElem.LookupParameter("Building Name")?.Set(iLine.strBuildingName); // str Building Name.Set(iLine.
                    InputLineElem.LookupParameter("Bottom Track Gauge")?.Set(iLine.strBottomTrackGuage);
                    InputLineElem.LookupParameter("Bottom Track Size")?.Set(iLine.strBottomTrackSize);
                    InputLineElem.LookupParameter("Bottom Track Punch")?.Set(iLine.strBottomTrackPunch);
                    InputLineElem.LookupParameter("Flange Offset")?.Set(iLine.dFlangeOfset);
                    InputLineElem.LookupParameter("Stud O.C.")?.Set(iLine.dOnCenter);
                    InputLineElem.LookupParameter("Parapet Height")?.Set(iLine.dParapetHeight);
                    InputLineElem.LookupParameter("Double Stud")?.Set(iLine.strDoubleStudType);
                }
            }
        }

        private IntersectionType IdentifyRelationShip(InputLine inputLine1, InputLine inputLine2, out XYZ IntersectionPt, out ElementId iContinuousLine)
        {
            // Initialize the out parameters
            IntersectionType intersection = IntersectionType.NonIntesecting;
            iContinuousLine = null;

            IntersectionPt = null;

            // Define two lines using their end points
            Line line1 = Line.CreateBound(inputLine1.startpoint, inputLine1.endpoint);
            Line line2 = Line.CreateBound(inputLine2.startpoint, inputLine2.endpoint);

            // Compute the intersection point of the two lines
            SetComparisonResult result = line1.Intersect(line2, out IntersectionResultArray intersectionResult);
            if (result == SetComparisonResult.Overlap)
            {
                // Access the first intersection point
                XYZ intersectionPoint = intersectionResult.get_Item(0).XYZPoint;

                // Compute the continuity of both the lines at a given point.
                iContinuousLine = IdentifyContinousLineAtPoint(intersectionPoint, inputLine1, inputLine2);

                if (iContinuousLine == null)
                    intersection = IntersectionType.LIntersection;
                else
                    intersection = IntersectionType.TIntersection;

                // Return the Intersection point
                IntersectionPt = intersectionPoint;
            }

            return intersection;
        }

        private ElementId IdentifyContinousLineAtPoint(XYZ collisionPoint, InputLine line1, InputLine line2)
        {

            Logger.logMessage("Method : IdentifyContinousLineAtPoint");
            ElementId iContinuousLine = null;

            XYZ TracePoint1;
            XYZ TracePoint2;

            for (int i = 0; i < 2; i++)
            {
                InputLine GenericLine = i == 0 ? line1 : line2;
                // Get the location curve
                XYZ pt1 = GenericLine.startpoint;
                XYZ pt2 = GenericLine.endpoint;

                LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

                if (lineType == LineType.Horizontal)
                {
                    TracePoint1 = collisionPoint + new XYZ(0.1, 0, 0);
                    TracePoint2 = collisionPoint + new XYZ(-0.1, 0, 0);

                    XYZ start = pt1.X > pt2.X ? pt2 : pt1;
                    XYZ end = pt1.X > pt2.X ? pt1 : pt2;

                    if (TracePoint1.X > start.X && TracePoint1.X < end.X &&
                        TracePoint2.X > start.X && TracePoint2.X < end.X)
                    {
                        iContinuousLine = i == 0 ? line1.id : line2.id;
                        break;
                    }
                }

                if (lineType == LineType.vertical)
                {
                    TracePoint1 = collisionPoint + new XYZ(0, 0.1, 0);
                    TracePoint2 = collisionPoint + new XYZ(0, -0.1, 0);

                    XYZ start = pt1.Y > pt2.Y ? pt2 : pt1;
                    XYZ end = pt1.Y > pt2.Y ? pt1 : pt2;

                    if (TracePoint1.Y > start.Y && TracePoint1.Y < end.Y &&
                        TracePoint2.Y > start.Y && TracePoint2.Y < end.Y)
                    {
                        iContinuousLine = i == 0 ? line1.id : line2.id;
                        break;
                    }
                }
            }
            return iContinuousLine;
        }
    }
}



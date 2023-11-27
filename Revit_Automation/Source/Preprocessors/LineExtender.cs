    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using Newtonsoft.Json.Serialization;
using Revit_Automation.CustomTypes;
using Revit_Automation.Dialogs;
using Revit_Automation.Source.Interfaces;
using Revit_Automation.Source.Utils;

namespace Revit_Automation.Source.Preprocessors
{
    public  class LineExtender : IPreprocessInterface 
    {
        public Document m_Document;

        private LineProcessing m_LineProcessing;
        private Selection m_Selection;
        private bool m_bProcessSelected;
        public LineExtender(Document doc, LineProcessing form, bool bProcessSelected, Selection selection = null) { m_Document = doc; m_LineProcessing = form; m_bProcessSelected = bProcessSelected; m_Selection = selection; }


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
            using (Transaction tx = new Transaction (m_Document))
            { 
                
                // Gather all Input Lines
                InputLineUtility.GatherInputLines(m_Document, m_Selection, CommandCode.Posts, false);

                if (InputLineUtility.colInputLines.Count == 0)
                    return;
                tx.Start("Extending Lines");

                m_LineProcessing.Show();
                m_LineProcessing.Visible = true;
                Thread.Sleep(2000);

                m_LineProcessing.LineExtendingMessage(string.Format("Executing Line Extension Algorithm \n "), 0);

                // Iterate through the collection and Identify the Intersecting Lines
                for (int i = 0; i < InputLineUtility.colInputLines.Count; i ++)
                {
                    InputLine inputLine1 = InputLineUtility.colInputLines[i];
                    m_LineProcessing.Refresh();
                    m_LineProcessing.LineExtendingMessage(string.Format("\n Now Processing Line : {0} \n", inputLine1.id), 1);

                    bool bHasLIntersections = false;
                    for ( int j = i+1; j < InputLineUtility.colInputLines.Count; j++)
                    {
                        InputLine inputLine2 = InputLineUtility.colInputLines[j];
                        LineType lt1 = MathUtils.ApproximatelyEqual(inputLine1.startpoint.X, inputLine1.endpoint.X) ? LineType.vertical : LineType.Horizontal;
                        LineType lt2 = MathUtils.ApproximatelyEqual(inputLine2.startpoint.X, inputLine2.endpoint.X) ? LineType.vertical : LineType.Horizontal;

                        // We check only for perpendicular lines
                        if ((lt1 == LineType.vertical && lt2 == LineType.Horizontal) || (lt2 == LineType.vertical && lt1 == LineType.Horizontal))
                        {
                            XYZ intersectionPT = null;
                            IntersectionType intesection = IdentifyRelationShip(inputLine1, inputLine2, out intersectionPT);

                            // Only in case of L-Intersection we extend lines
                            if (intesection == IntersectionType.LIntersection)
                            {
                                m_LineProcessing.LineExtendingMessage(string.Format("There is an L-Intersection Between {0} ({2}) and {1} ({3}) lines \n", inputLine1.id, inputLine2.id, inputLine1.strWallType, inputLine2.strWallType), 2);
                                ExtendPriorityLine(ref inputLine1, ref inputLine2, intersectionPT);
                                bHasLIntersections = true;

                                InputLineUtility.colInputLines[i] = inputLine1;
                                InputLineUtility.colInputLines[j] = inputLine2;
                            }
                        }
                    }

                    if (!bHasLIntersections)
                        m_LineProcessing.LineExtendingMessage(string.Format("No L-Intersections. Nothing to do \n", inputLine1.id), 4);
                }

                // Clone Line
                UpdateParametersForNewLines();

                m_LineProcessing.LineExtendingMessage(" \n 😊 😀 😉 😍 Finished Extending  😂 😄 😎  \n", 3);

                tx.Commit();
            }
        }

        private void ExtendPriorityLine(ref InputLine inputLine1, ref InputLine inputLine2, XYZ intersectionPT)
        {
            WallPriority wall1 = GetWallType(inputLine1);
            WallPriority wall2 = GetWallType(inputLine2);

            ref InputLine lineToExtend = ref inputLine1;
            ref InputLine lineToRemain = ref inputLine2;
            if (wall1 < wall2)
            { 
                lineToExtend = ref inputLine2;
                lineToRemain = ref inputLine1;
            }

            ExtendLine(ref lineToExtend, ref lineToRemain, intersectionPT);
        }

        private void ExtendLine(ref InputLine lineToExtend, ref InputLine lineToRemain, XYZ intersectionPT)
        {   
            m_LineProcessing.LineExtendingMessage(string.Format("Extending Line {0} as it has higher priority among {1} , {2} \n", lineToExtend.id, lineToExtend.strWallType, lineToRemain.strWallType), 4);
                
            // Get web width of non-Extending line 
            double dWebWidth =  GenericUtils.WebWidthForTrimExtend(lineToRemain.strStudType) / 2;

            // Get the Element
            FamilyInstance lineElement = m_Document.GetElement(lineToExtend.id) as FamilyInstance;

            // Get the End points
            LocationCurve locCurve = lineElement.Location as LocationCurve;

            XYZ startpoint = null, endpoint = null;
            GenericUtils.GetlineStartAndEndPoints(lineElement, out startpoint, out endpoint);

            LineType lineType = MathUtils.ApproximatelyEqual(startpoint.X, endpoint.X) ? LineType.vertical : LineType.Horizontal;

            XYZ updatedStartPt = null, updatedEndPt = null;

            // See which end needs to be extended.
            if (MathUtils.ApproximatelyEqual(startpoint.X, intersectionPT.X) && MathUtils.ApproximatelyEqual(startpoint.Y, intersectionPT.Y))
            {
                if (lineType == LineType.Horizontal)
                    updatedStartPt = new XYZ(startpoint.X - dWebWidth, startpoint.Y, startpoint.Z);
                else
                    updatedStartPt = new XYZ(startpoint.X, startpoint.Y - dWebWidth, startpoint.Z);

                updatedEndPt = endpoint;
            }
            else
            {
                updatedStartPt = startpoint;

                if (lineType == LineType.Horizontal)
                    updatedEndPt = new XYZ(endpoint.X + dWebWidth, endpoint.Y, endpoint.Z);
                else
                    updatedEndPt = new XYZ(endpoint.X, endpoint.Y + dWebWidth, endpoint.Z);
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
            m_Document.Delete(lineToExtend.id);

            // update the Input Line
            lineToExtend.locationCurve = locCurve;
            lineToExtend.id = extendedLine.Id;
            lineToExtend.bLineExtendedOrTrimmed = true;
            lineToExtend.startpoint = updatedStartPt;
            lineToExtend.endpoint = updatedEndPt;            

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

        private WallPriority GetWallType(InputLine inputLine)
        {
            if (inputLine.strWallType == "Fire")
                return WallPriority.Fire;
            else if (inputLine.strWallType == "Ex w/ Insulation")
                return WallPriority.ExWithoutInsulation;
            else if (inputLine.strWallType == "Insulation")
                return WallPriority.Insulation;
            else if (inputLine.strWallType == "Ex")
                return WallPriority.Ex;
            else if (inputLine.strWallType == "LBS")
                return WallPriority.LBS;
            else if (inputLine.strWallType == "LB")
                return WallPriority.LB;
            else if (inputLine.strWallType == "NLBS")
                return WallPriority.NLBS;
            else
                return WallPriority.NLB;
        }

        private IntersectionType IdentifyRelationShip(InputLine inputLine1, InputLine inputLine2, out XYZ IntersectionPt)
        {
            IntersectionType intersection = IntersectionType.NonIntesecting;

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
                int iContinuousLine = IdentifyContinousLineAtPoint(intersectionPoint, inputLine1, inputLine2);

                if (iContinuousLine == 0)
                    intersection = IntersectionType.LIntersection;
                else
                    intersection = IntersectionType.TIntersection;
                
                // Return the Intersection point
                IntersectionPt = intersectionPoint;
            }
            else
            {
                
            }

            return intersection;
        }

        private int IdentifyContinousLineAtPoint(XYZ collisionPoint, InputLine line1, InputLine line2)
        {

            Logger.logMessage("Method : IdentifyContinousLineAtPoint");
            int iContinuousLine = 0;

            XYZ TracePoint1;
            XYZ TracePoint2;

            for (int i = 0; i < 2; i++ )
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
                        iContinuousLine = i+1;
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
                        iContinuousLine = i + 1;
                        break;
                    }
                }
            }
            return iContinuousLine;
        }
    }
}



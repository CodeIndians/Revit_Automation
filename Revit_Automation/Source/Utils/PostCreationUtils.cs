using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Revit_Automation.Source.Utils
{
    public class PostCreationUtils
    {
        // For Firewalls, Insulation and Exterior insulation, we have 3C condition and the stud will be at end
        // But for LB/NLB the stud additions will happen at the middle, so the code should be different.
        public static void PlaceStudAtPoint(Document doc, XYZ studPoint, InputLine inputLine, bool bEndingColumns = false, PanelDirection panelDirection = PanelDirection.B, double dPanelThickness = 0.0)
        {
            try
            {
                // For Panel directions of Top and left, Flip will be true. Bottom and Right Flip will be false
                bool bFlip = (panelDirection == PanelDirection.U || panelDirection == PanelDirection.L);

                // Remove any existing studs
                RemoveStudAtPoint(studPoint, inputLine.endpoint - inputLine.startpoint, doc);

                // Input line start and end points
                XYZ pt1 = null, pt2 = null;
                GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);

                // Line orientation
                LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

                // Get Top and base levels
                         IOrderedEnumerable<Level> levels = ModelCreator.FindAndSortLevels(doc);
                Level toplevel = null, baselevel = null;
                ComputeTopAndBaseLevels(inputLine, levels, out toplevel, out baselevel);

                // Get Top and Bottom Attachment Elements
                Element topAttachElement = null, bottomAttachElement = null;
                topAttachElement = GenericUtils.GetNearestFloorOrRoof(toplevel, pt1, doc);
                bottomAttachElement = GenericUtils.GetNearestFloorOrRoof(baselevel, pt1, doc);
                if (topAttachElement == null)
                {
                    topAttachElement = GetRoofAtPoint(doc, pt1);
                }

                // Get the  family Symbol for the Stud
                string strFamilySymbol = inputLine.strStudType.ToString() + string.Format(" x {0}ga", inputLine.strStudGuage);
                FamilySymbol columnType = SymbolCollector.GetSymbol(strFamilySymbol, "Post", SymbolCollector.FamilySymbolType.posts);

                if (!columnType.IsActive)
                    columnType.Activate();

                // Compute whether the point is at start or end
                bool bAtStart = lineType == LineType.vertical ?
                                    (Math.Abs(studPoint.Y - pt1.Y) > Math.Abs(studPoint.Y - pt2.Y) ? false : true) :
                                    (Math.Abs(studPoint.X - pt1.X) > Math.Abs(studPoint.X - pt2.X) ? false : true);

                // Create the column instance at the point
                FamilyInstance column = doc.Create.NewFamilyInstance(studPoint, columnType, baselevel, StructuralType.Column);

                // Set top level based on parapet conditions
                if (inputLine.dParapetHeight == 0)
                {
                    _ = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                    _ = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
                }
                else
                {
                    _ = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(baselevel.Id);
                    _ = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(inputLine.dParapetHeight);
                }

                // Add Top and Bottom attachments
                if (topAttachElement != null)
                {
                    ColumnAttachment.AddColumnAttachment(doc, column, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                }

                if (bottomAttachElement != null)
                {
                    ColumnAttachment.AddColumnAttachment(doc, column, bottomAttachElement, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                }

                ElementId columnID = column.Id;
                XYZ ColumnOrientation = column.FacingOrientation;

                // Flange width
                double dFlangeWidth = GenericUtils.FlangeWidth(inputLine.strStudType);

                // update the orientation - based on start or end and the roof slope
                if (bEndingColumns)
                {
                    UpdateOrientation(doc, columnID, ColumnOrientation, studPoint, bAtStart ? pt2 : pt1, true);

                    // Adjust the stud location
                    XYZ Adjustedpt1 = AdjustLinePoint(studPoint, bAtStart ? pt2 : pt1, lineType, dFlangeWidth / 2);
                    MoveColumn(doc, columnID, Adjustedpt1);
                    Logger.logMessage("ProcessStudInputLine - Move Column at end");
                }

                else
                {
                    UpdateOrientation(doc, columnID, ColumnOrientation, studPoint, pt2);
                    
                    // Adjust the stud location
                    XYZ Adjustedpt1 = AdjustStudAccordingToPanel(studPoint, lineType, dFlangeWidth / 2, bFlip, column.FacingOrientation, dPanelThickness);
                    MoveColumn(doc, columnID, Adjustedpt1);
                    Logger.logMessage("ProcessStudInputLine - Move Column at end");
                }
            }
            catch (Exception)
            { 
            }
        }

        private static XYZ AdjustStudAccordingToPanel(XYZ studPoint, LineType lineType, double v, bool bFlip, XYZ facingOrientation, double dPanelThickness)
        {
            XYZ AdjustedStudPoint = studPoint;
            
            if (lineType == LineType.Horizontal)
            {
                if (bFlip == false && facingOrientation.X < 0)
                {
                    AdjustedStudPoint = new XYZ(studPoint.X + v + dPanelThickness, studPoint.Y, studPoint.Z);
                }
                else if (bFlip == true && facingOrientation.X > 0)
                {
                    AdjustedStudPoint = new XYZ(studPoint.X - v - dPanelThickness, studPoint.Y, studPoint.Z);
                }
                else if (bFlip == false && facingOrientation.X > 0)
                {
                    AdjustedStudPoint = new XYZ(studPoint.X - v, studPoint.Y, studPoint.Z);
                }
                else if (bFlip == true && facingOrientation.X < 0)
                {
                    AdjustedStudPoint = new XYZ(studPoint.X + v, studPoint.Y, studPoint.Z);
                }
            }
            else
            {
                if (bFlip == false && facingOrientation.Y < 0)
                {
                    AdjustedStudPoint = new XYZ(studPoint.X , studPoint.Y + v , studPoint.Z);
                }
                else if (bFlip == true && facingOrientation.Y > 0)
                {
                    AdjustedStudPoint = new XYZ(studPoint.X , studPoint.Y - v, studPoint.Z);
                }
                else if (bFlip == false && facingOrientation.Y > 0)
                {
                    AdjustedStudPoint = new XYZ(studPoint.X, studPoint.Y - v - dPanelThickness, studPoint.Z);
                }
                else if (bFlip == true && facingOrientation.Y < 0)
                {
                    AdjustedStudPoint = new XYZ(studPoint.X, studPoint.Y + v + dPanelThickness, studPoint.Z);
                }

            }
            return AdjustedStudPoint;
        }

        public static void ComputeTopAndBaseLevels(InputLine inputLine, IOrderedEnumerable<Level> levels, out Level toplevel, out Level baselevel)
        {
            toplevel = null;
            baselevel = null;

            XYZ pt1 = null, pt2 = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);

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

                    baselevel = tempLevel;
                    toplevel = filteredLevels.ElementAt(i + 1);

                    break;
                }
            }
        }


        private static Element GetRoofAtPoint(Document doc, XYZ pt1)
        {
            Logger.logMessage("Method : GetRoofAtPoint");

            RoofObject targetRoof;
            targetRoof.slopeLine = null;
            targetRoof.roofElementID = null;

            foreach (RoofObject roof in RoofUtility.colRoofs)
            {
                double Xmin, Xmax, Ymin, Ymax = 0.0;
                Xmin = Math.Min(roof.max.X, roof.min.X);
                Xmax = Math.Max(roof.max.X, roof.min.X);
                Ymin = Math.Min(roof.max.Y, roof.min.Y);
                Ymax = Math.Max(roof.max.Y, roof.min.Y);

                if (pt1.X >= Xmin && pt1.X <= Xmax && pt1.Y >= Ymin && pt1.Y <= Ymax)
                {
                    targetRoof = roof;
                    break;
                }
            }
            return targetRoof.roofElementID != null ? doc.GetElement(targetRoof.roofElementID) : null;
        }

        private static void UpdateOrientation(Document doc, ElementId columnID, XYZ ColumnOrientation, XYZ pt1, XYZ pt2, bool bEndingColumns = false)
        {
            XYZ UnitVectorAlongLine
                = null;

            XYZ point1 = new XYZ(pt1.X, pt1.Y, 0);
            XYZ point2 = new XYZ(pt1.X, pt1.Y, 1);
            Line axis = Line.CreateBound(point1, point2);
            ``
            // This logic is to rotate the column such that it is perpendicular to Input line

            double dAngle = 0;

            XYZ LineOrientation = pt2 - pt1;
            UnitVectorAlongLine = LineOrientation.Normalize();

            if ((ColumnOrientation.X == 0 && !MathUtils.ApproximatelyEqual(LineOrientation.X, 0)) || (ColumnOrientation.Y == 0 && !MathUtils.ApproximatelyEqual(LineOrientation.Y, 0)))
            {
                dAngle = Math.PI * 90 / 180;
            }

            Logger.logMessage("UpdateOrientation - Making web Perpendicular to the line");
            ElementTransformUtils.RotateElement(doc, columnID, axis, dAngle);

            // Compute the orientation after rotation. 
            FamilyInstance column = doc.GetElement(columnID) as FamilyInstance;
            XYZ newOrientation = column.FacingOrientation;

            // End columns should face towards the line and also each other
            if (bEndingColumns)
            {
                // The web outward normal should be in a direction opposite to that of Input Line For Start and End Lines
                if (MathUtils.CompareVectors(UnitVectorAlongLine, newOrientation) == "Parallel")
                {
                    Logger.logMessage("UpdateOrientation - Making End Columns face each other");
                    ElementTransformUtils.RotateElement(doc, columnID, axis, Math.PI);
                }
            }

            // Columns should point to low eve
            else
            {
                XYZ SlopeDirection = RoofUtility.GetRoofSlopeDirection(pt1);

                // The web outward normal should be in a direction of slope
                if (MathUtils.IsParallel(SlopeDirection, newOrientation))
                {
                    if (MathUtils.CompareVectors(SlopeDirection, newOrientation) == "Anti-Parallel")
                    {
                        Logger.logMessage("UpdateOrientation - Open C should point to high eve");
                        ElementTransformUtils.RotateElement(doc, columnID, axis, Math.PI);
                    }
                }
            }
        }

        private static XYZ AdjustLinePoint(XYZ pt1, XYZ pt2, LineType lineType, double dOffset)
        {
            Logger.logMessage("Method : GetRoofAtPoint");

            if (pt1 == null || pt2 == null)
                return null;

            XYZ AdjustedPoint = null;

            XYZ tempXVector = new XYZ(dOffset, 0, 0);
            XYZ tempYVector = new XYZ(0, dOffset, 0);

            XYZ tempXMinusVector = new XYZ(-dOffset, 0, 0);
            XYZ tempYMinusVector = new XYZ(0, -dOffset, 0);

            if (lineType == LineType.vertical)
            {
                AdjustedPoint = pt1.Y < pt2.Y ? pt1 + tempYVector : pt1 + tempYMinusVector;
            }

            if (lineType == LineType.Horizontal)
            {
                AdjustedPoint = pt1.X < pt2.X ? pt1 + tempXVector : pt1 + tempXMinusVector;
            }
            return AdjustedPoint;
        }

        private static void MoveColumn(Document doc, ElementId columnId, XYZ newLocation)
        {

            Logger.logMessage("Method : MoveColumn");
            if (columnId == null)
                return;

            FamilyInstance column = doc.GetElement(columnId) as FamilyInstance;

            // Get the column's Location property
            Location location = column.Location;

            // Check if the column's location is a LocationPoint
            if (location is LocationPoint locationPoint)
            {
                // Set the new location for the column
                locationPoint.Point = newLocation;
            }
        }

        internal static void RemoveStudAtPoint(XYZ endpoint, XYZ webOrientation, Document doc)
        {
            try
            {
                // build a bounding box intersects filter to get the studs at a give point
                // Create a Outline, uses a minimum and maximum XYZ point to initialize the Bounding Box. 
                Outline myOutLn = new Outline(
                    new XYZ(endpoint.X - 0.05,
                    endpoint.Y - 0.05,
                    endpoint.Z - 0.05),
                    new XYZ(endpoint.X + 0.05,
                    endpoint.Y + 0.05,
                    endpoint.Z + 0.05));
                Logger.logMessage("Outline for Collection Objects Created");

                // Create a BoundingBoxIntersects filter with this Outline
                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);

                // Apply the filter to the elements in the active document to retrieve posts at a point
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IList<Element> postElements = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_StructuralColumns).ToElements();

                // Delete only that stud which is parallel to the web orientation
                foreach (Element column in postElements)
                {
                    FamilyInstance post = doc.GetElement(column.Id) as FamilyInstance;
                    XYZ postorientation = post.FacingOrientation;

                    if (MathUtils.IsParallel(postorientation, webOrientation))
                    {
                        doc.Delete(post.Id);
                    }
                }
            }
            catch (Exception ex) 
            { 
            }
        }

        internal static void PlaceStudForCeeHeader(Document doc, XYZ ceeHeaderStartPt, string ceeHeaderType, string postType, string postGuage, int postCount, Level topLevel, Level baseLevel)
        {
            try
            {



                // Get Top and Bottom Attachment Elements
                Element topAttachElement = null, bottomAttachElement = null;
                topAttachElement = GenericUtils.GetNearestFloorOrRoof(topLevel, ceeHeaderStartPt, doc);
                bottomAttachElement = GenericUtils.GetNearestFloorOrRoof(baseLevel, ceeHeaderStartPt, doc);
                
                if (topAttachElement == null)
                {
                    topAttachElement = GetRoofAtPoint(doc, ceeHeaderStartPt);
                }

                // Get the  family Symbol for the Stud
                string strFamilySymbol = postType + string.Format(" x {0}ga", postGuage);
                FamilySymbol columnType = SymbolCollector.GetSymbol(strFamilySymbol, "Post", SymbolCollector.FamilySymbolType.posts);

                if (!columnType.IsActive)
                    columnType.Activate();

                // Create the column instance at the point
                FamilyInstance column = doc.Create.NewFamilyInstance( ceeHeaderStartPt, columnType, baseLevel, StructuralType.Column);

                // Add Top and Bottom attachments
                if (topAttachElement != null)
                {
                    ColumnAttachment.AddColumnAttachment(doc, column, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                }

                if (bottomAttachElement != null)
                {
                    ColumnAttachment.AddColumnAttachment(doc, column, bottomAttachElement, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                }

                // Adjust the position and orientation based on the type of the cee Header
                if (ceeHeaderType == "StartCont")
                {
                    column.orien
                }
                else if (ceeHeaderType == "StartNonCont") 
                { 
                }
                else if (ceeHeaderType == "EndCont") 
                { 
                }
                else if (ceeHeaderType == "EndNonCont") 
                { 
                }

                // Remove any existing studs
                RemoveIntersectingStuds(column);



                //for (int i = 0; i < 2; i++)
                //{


                //    //ElementId columnID = column.Id;
                //    //XYZ ColumnOrientation = column.FacingOrientation;

                //    //// Flange width
                //    //double dFlangeWidth = GenericUtils.FlangeWidth(inputLine.strStudType);

                //    //// update the orientation - based on start or end and the roof slope
                //    //if (bEndingColumns)
                //    //{
                //    //    UpdateOrientation(doc, columnID, ColumnOrientation, studPoint, bAtStart ? pt2 : pt1, true);

                //    //    // Adjust the stud location
                //    //    XYZ Adjustedpt1 = AdjustLinePoint(studPoint, bAtStart ? pt2 : pt1, lineType, dFlangeWidth / 2);
                //    //    MoveColumn(doc, columnID, Adjustedpt1);
                //    //    Logger.logMessage("ProcessStudInputLine - Move Column at end");
                //    //}

                //    //else
                //    //{
                //    //    UpdateOrientation(doc, columnID, ColumnOrientation, studPoint, pt2);

                //    //    // Adjust the stud location
                //    //    XYZ Adjustedpt1 = AdjustStudAccordingToPanel(studPoint, lineType, dFlangeWidth / 2, bFlip, column.FacingOrientation, dPanelThickness);
                //    //    MoveColumn(doc, columnID, Adjustedpt1);
                //    //    Logger.logMessage("ProcessStudInputLine - Move Column at end");
                //    //}
                //}
            }
            catch (Exception)
            {
            }

        }

        private static void RemoveIntersectingStuds(FamilyInstance column)
        {
            throw new NotImplementedException();
        }
    }
}

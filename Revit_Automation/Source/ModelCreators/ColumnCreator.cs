﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.CollisionDetectors;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Animation;
using static Revit_Automation.Source.Utils.WarningSwallowers;

namespace Revit_Automation.Source.ModelCreators
{
    public class ColumnCreator : IModelCreator
    {
        private enum LineType
        {
            vertical = 0, horizontal = 1
        }
        private Autodesk.Revit.DB.Document m_Document { get; set; }

        private Form1 m_Form { get; set; }
        public ColumnCreator(Autodesk.Revit.DB.Document doc, Form1 form)
        {

            m_Document = doc;
            m_Form = form;
        }
        public void CreateModel(List<CustomTypes.InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            ProcessInputLines(colInputLines, levels);
        }
        private void ProcessInputLines(List<InputLine> inputLinesCollection, IOrderedEnumerable<Level> levels)
        {

            int iLineProcessing = 1;

            DateTime StartTime = DateTime.Now;

            double dCounter = 0;
            int iCounter = 1;
            double dIncrementFactor = 100 / inputLinesCollection.Count;

            foreach (InputLine inputLine in inputLinesCollection)
            {
                try
                {
                    m_Form.PostMessage(string.Format("Processing Line {0} / {1}", iLineProcessing, inputLinesCollection.Count));


                    if (iCounter < 100 && (iCounter < dCounter))
                    {
                        iCounter = (int)Math.Ceiling(dCounter);
                        m_Form.UpdateProgress(iCounter);
                    }


                    ErrorHandler.elemIDbeingProcessed = inputLine.id;

                    if (!string.IsNullOrEmpty(inputLine.strDoubleStudType))
                    {
                        ProcessDoubleStud(inputLine, levels);
                    }
                    else if (!string.IsNullOrEmpty(inputLine.strT62Guage) && !string.IsNullOrEmpty(inputLine.strStudGuage))
                    {
                        ProcessT62AndStudLine(inputLine, levels);
                    }
                    else if (!string.IsNullOrEmpty(inputLine.strT62Guage))
                    {
                        ProcessT62InputLine(inputLine, levels);
                    }
                    else if (!string.IsNullOrEmpty(inputLine.strStudGuage))
                    {
                        ProcessStudInputLine(inputLine, levels);
                    }

                    dCounter += dIncrementFactor;
                    iLineProcessing++;
                    m_Form.PostMessage(string.Format("\n SuccessFully Procesed InputLine {0} at {1}", inputLine.id, DateTime.Now));
                }

                catch (Exception)
                {
                    m_Form.PostMessage(string.Format("\n !!! Failed  To Process InputLine {0}, at {1}. Please Review", inputLine.id, DateTime.Now), true);
                }

            }

            DateTime EndTime = DateTime.Now;

            TimeSpan timeDifference = EndTime - StartTime;
            double seconds = timeDifference.TotalSeconds;

            m_Form.PostMessage(string.Format("\n Completed Generation of Model in {0} seconds", seconds));
        }

        private void ProcessDoubleStud(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {

            ErrorHandler.elemIDbeingProcessed = inputLine.id;

            try
            {
                // Create the collision object.
                PostCollisionResolver collider = new PostCollisionResolver(m_Document);

                XYZ pt1 = null, pt2 = null;
                GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);

                LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? lineType = LineType.horizontal  : LineType.vertical;
                
                // Line Orientation
                XYZ lineOrientation = GenericUtils.GetLineOrientation(inputLine);

                //compute levels
                Level toplevel = null, baseLevel = null;

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
                // Compute the top Attachment Object
                Element topAttachElement = null;

                // compute the bottom attach element
                Element bottomAttachElement = null;

                // Get the Family symbol of the respective post
                string strFamilySymbol = inputLine.strStudType.ToString() + string.Format(" x {0}ga", inputLine.strStudGuage);
                FamilySymbol columnType = SymbolCollector.GetSymbol(strFamilySymbol, "Post");


                // We have 3 types of Double Stud Conditions. 
                // 1 - At Ends - Place Double Stud at Start and End
                // 2 - At Grid - Intersection of the Input Line and Grids
                // 3 - Continuous - At given On Center
                // In a Double stud the Main Stud should Exend below floor by 3 feet, the secondary studs should flush at the floor

                if (inputLine.strDoubleStudType.Contains("At Ends"))
                {
                    ProcessDoubleStudAtEnds(inputLine, pt1, pt2, columnType, topAttachElement, bottomAttachElement, toplevel, baseLevel);
                    if (inputLine.dOnCenter != 0)
                        ProcessStudInputLine(inputLine, levels, true);
                }

                else if (inputLine.strDoubleStudType == "At Grids")
                {
                    ProcessDoubleStudAtGrids(inputLine, levels, columnType, topAttachElement, bottomAttachElement, toplevel, baseLevel);
                    if (inputLine.dOnCenter != 0)
                        ProcessStudInputLine(inputLine, levels, false);
                }

                else if (inputLine.strDoubleStudType == "Cont")
                { 
                    ProcessContinuousDoubleStuds(inputLine, pt1, pt2, columnType, topAttachElement, bottomAttachElement, toplevel, baseLevel, lineType, lineOrientation);
                }
            }

            catch (Exception)
            {
                //ErrorHandler.reportError();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputLine"></param>
        /// <param name="levels"></param>
        /// <param name="bDoubleStudOnCenter"> If this parameter is true we do not place start and end columns</param>
        private void ProcessStudInputLine(InputLine inputLine, IOrderedEnumerable<Level> levels, bool bDoubleStudOnCenter = false)
        {
            ErrorHandler.elemIDbeingProcessed = inputLine.id;
            double dFlangeWidth = GenericUtils.FlangeWidth(inputLine.strStudType);

            if (inputLine.dOnCenter == 0)
            {
                return;
            }

            try
            {
                // Create the collision object.
                PostCollisionResolver collider = new PostCollisionResolver(m_Document);

                XYZ pt1 = null, pt2 = null;
                GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);

                ElementId startColumnID, EndColumnID;
                XYZ startColumnOrientation, endColumnOrientation;

                LineType lineType = LineType.vertical;

                if (MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y))
                {
                    lineType = LineType.horizontal;
                }

                // Line Orientation
                XYZ lineOrientation =  GenericUtils.GetLineOrientation(inputLine);

                //compute levels
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

                // Compute the top Attachment Object
                Element topAttachElement = null;

                // Compute Bottom Attachment Object
                Element bottomAttachElement = null;


               string strFamilySymbol = inputLine.strStudType.ToString() + string.Format(" x {0}ga", inputLine.strStudGuage);
                FamilySymbol columnType = SymbolCollector.GetSymbol(strFamilySymbol, "Post");

                if (columnType == null)
                {
                    _ = TaskDialog.Show(string.Format("The Family {0} couldn't be loaded or found"), inputLine.strStudGuage);
                }

                if (!bDoubleStudOnCenter)
                {
                    using (Transaction tx = new Transaction(m_Document))
                    {
                        GenericUtils.SupressWarningsInTransaction(tx);

                        _ = tx.Start("Place Column");

                        _ = CheckForExistingColumns(pt1);

                        //Place Column at start
                        FamilyInstance startcolumn = m_Document.Create.NewFamilyInstance(pt1, columnType, baseLevel, StructuralType.Column);

                        if (inputLine.dParapetHeight == 0)
                        {
                            _ = startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                            _ = startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
                        }
                        else
                        {
                            _ = startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(baseLevel.Id);
                            _ = startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(inputLine.dParapetHeight);
                        }
                        // If we coulfdn't find a floor, the input line is on top floor
                        // Need to attach it to roof

                        topAttachElement = GetNearestFloorOrRoof(toplevel, pt1);
                        bottomAttachElement = GetNearestFloorOrRoof(baseLevel, pt1);

                        if (topAttachElement == null)
                        {
                            topAttachElement = GetRoofAtPoint(pt1);
                        }

                        if (topAttachElement != null)
                        {
                            ColumnAttachment.AddColumnAttachment(m_Document, startcolumn, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                        }

                        if (bottomAttachElement != null)
                        {
                            ColumnAttachment.AddColumnAttachment(m_Document, startcolumn, bottomAttachElement, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                        }

                        //m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", pt1.X, pt1.Y, pt1.Z, inputLine.strStudType));
                        startColumnID = startcolumn.Id;
                        startColumnOrientation = startcolumn.FacingOrientation;

                        // Place column at end
                        FamilyInstance endColumn = m_Document.Create.NewFamilyInstance(pt2, columnType, baseLevel, StructuralType.Column);

                        if (inputLine.dParapetHeight == 0)
                        {
                            _ = endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                            _ = endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
                        }
                        else
                        {
                            _ = endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(baseLevel.Id);
                            _ = endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(inputLine.dParapetHeight);
                        }

                        // If we coulfdn't find a floor, the input line is on top floor
                        // Need to attach it to roof
                        topAttachElement = GetNearestFloorOrRoof(toplevel, pt2);
                        bottomAttachElement = GetNearestFloorOrRoof(baseLevel, pt2);

                        if (topAttachElement == null)
                        {
                            topAttachElement = GetRoofAtPoint(pt2);
                        }

                        if (topAttachElement != null)
                        {
                            ColumnAttachment.AddColumnAttachment(m_Document, endColumn, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                        }

                        if (bottomAttachElement != null)
                        {
                            ColumnAttachment.AddColumnAttachment(m_Document, endColumn, bottomAttachElement, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                        }

                        //m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", pt2.X, pt2.Y, pt2.Z, inputLine.strStudType));
                        EndColumnID = endColumn.Id;
                        endColumnOrientation = endColumn.FacingOrientation;


                        _ = tx.Commit();
                    }

                    // Reference - Dev Guide - Page 1
                    // At ends the column web should match with grid point. so we compute the adjusted point and move the colum to the desired location
                    UpdateOrientation(startColumnID, startColumnOrientation, pt1, pt2, true);
                    XYZ Adjustedpt1 = AdjustLinePoint(pt1, pt2, lineType, dFlangeWidth / 2);
                    MoveColumn(startColumnID, Adjustedpt1);

                    // Reference - Dev Guide - Page 1
                    // At ends the column web should match with grid point. so we compute the adjusted point and move the colum to the desired location
                    UpdateOrientation(EndColumnID, endColumnOrientation, pt2, pt1, true);
                    XYZ Adjustedpt2 = AdjustLinePoint(pt2, pt1, lineType, dFlangeWidth / 2);
                    MoveColumn(EndColumnID, Adjustedpt2);

                    // Collision Handling

                    CollisionObject collisionObject = new CollisionObject
                    {
                        CollisionPoint = pt1,
                        inputLineID = inputLine.id,
                        collisionElementID = startColumnID
                    };
                    collider.HandleCollision(collisionObject);

                    CollisionObject collisionObject2 = new CollisionObject
                    {
                        CollisionPoint = pt2,
                        inputLineID = inputLine.id,
                        collisionElementID = EndColumnID
                    };
                    collider.HandleCollision(collisionObject2);
                }




                XYZ studPoint = null, studEndPoint = null, studStartPoint = null;

                // For placing On Center's This is the Logic
                // Compute the Start point for On center placement in a given line
                // Check for Flange Offset Parameter and adjust accordingly

                XYZ FlangeOffsetXVector = new XYZ(dFlangeWidth / 2, 0, 0);
                XYZ FlangeOffsetYVector = new XYZ(0, dFlangeWidth / 2, 0);


                if (lineType == LineType.vertical)
                {
                    studPoint = ComputeOnCenterStartingPoint(pt1, pt2, inputLine, inputLine.dOnCenter, lineType);
                    if (inputLine.dFlangeOfset == 1)
                    {
                        studPoint += FlangeOffsetYVector;
                    }
                    studStartPoint = pt1.Y > pt2.Y ? pt2 : pt1;
                    studEndPoint = pt1.Y > pt2.Y ? pt1 : pt2;
                }
                else
                {
                    studPoint = ComputeOnCenterStartingPoint(pt1, pt2, inputLine, inputLine.dOnCenter, lineType);
                    if (inputLine.dFlangeOfset == 1)
                    {
                        studPoint += FlangeOffsetXVector;
                    }
                    studStartPoint = pt1.X > pt2.X ? pt2 : pt1;
                    studEndPoint = pt1.X > pt2.X ? pt1 : pt2;
                }

                XYZ tempXVector = new XYZ(inputLine.dOnCenter, 0, 0);
                XYZ tempYVector = new XYZ(0, inputLine.dOnCenter, 0);

                bool bCanCreateColumn = true;
                while (bCanCreateColumn)
                {
                    ElementId StudColumnID;
                    XYZ StudColumnOrientation;

                    if ((lineType == LineType.vertical && studPoint.Y < (studEndPoint.Y - 1.0)) || (lineType == LineType.horizontal && studPoint.X < (studEndPoint.X - 1.0)))
                    {

                        if ((lineType == LineType.vertical && studPoint.Y > (studStartPoint.Y + 0.32)) || (lineType == LineType.horizontal && studPoint.X >  (studStartPoint.X + 0.32))) // This condition ensures there are no collisions at the start
                        { 
                            using (Transaction tx = new Transaction(m_Document))
                            {
                                GenericUtils.SupressWarningsInTransaction(tx);

                                _ = tx.Start("Placing posts");
                                FamilyInstance studColumn = m_Document.Create.NewFamilyInstance(studPoint, columnType, baseLevel, StructuralType.Column);

                                if (inputLine.dParapetHeight == 0)
                                {
                                    _ = studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                                    _ = studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
                                }
                                else
                                {
                                    _ = studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(baseLevel.Id);
                                    _ = studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(inputLine.dParapetHeight);
                                }


                                topAttachElement = GetNearestFloorOrRoof(toplevel, studPoint);
                                bottomAttachElement = GetNearestFloorOrRoof(baseLevel, studPoint);

                                // If we coulfdn't find a floor, the input line is on top floor
                                // Need to attach it to roof
                                if (topAttachElement == null)
                                {
                                    topAttachElement = GetRoofAtPoint(studPoint);
                                }

                                if (topAttachElement != null)
                                {
                                    ColumnAttachment.AddColumnAttachment(m_Document, studColumn, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                                }

                                if (bottomAttachElement != null)
                                {
                                    ColumnAttachment.AddColumnAttachment(m_Document, studColumn, bottomAttachElement, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                                }


                                //m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", studPoint.X, studPoint.Y, studPoint.Z, inputLine.strStudType));
                                StudColumnID = studColumn.Id;
                                StudColumnOrientation = studColumn.FacingOrientation;
                                _ = tx.Commit();
                            }

                            UpdateOrientation(StudColumnID, StudColumnOrientation, studPoint, pt2);


                            // Compute the orientation after rotation. 
                            FamilyInstance column = m_Document.GetElement(StudColumnID) as FamilyInstance;
                            XYZ newOrientation = column.FacingOrientation;
                            XYZ AdjustedLinePoint = null;
                            
                            //If line orientation and Web orientaion are is same direction, we need to move the column back by Flange Width
                            // This is because, we have moved the point away from origin while computing the stud on-center point.
                            // Figure 2 Dev Guide
                            if (inputLine.dFlangeOfset != 0 && MathUtils.CompareVectors(lineOrientation, newOrientation) == "Parallel")
                            {

                                AdjustedLinePoint = AdjustLinePoint(studPoint, pt2, lineType, -dFlangeWidth);
                                MoveColumn(StudColumnID, AdjustedLinePoint);
                            }

                            CollisionObject collisionObject3 = new CollisionObject
                            {
                                CollisionPoint = studPoint,
                                inputLineID = inputLine.id,
                                collisionElementID = StudColumnID
                            };
                            collider.HandleCollision(collisionObject3);
                        }

                        // Move to next point
                        if (lineType == LineType.vertical)
                        {
                            studPoint += tempYVector;
                        }
                        else
                        {
                            studPoint += tempXVector;
                        }
                    }
                    else
                    {
                        bCanCreateColumn = false;
                    }
                }
            }

            catch (Exception)
            {
                //ErrorHandler.reportError();
            }
        }

        /// <summary>
        /// BE VERY VERY Careful when changing any of the below logic
        /// The current Logic is based on 2 conditions.
        /// The start and End column should be facing towards the line and facing each other
        /// Other studs should be oriented along the Low Eave.
        /// Any modification should evaluate each of the condition very carefully.
        /// </summary>
        /// <param name="columnID"></param>
        /// <param name="ColumnOrientation"></param>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="bRotate"></param>
        private void UpdateOrientation(ElementId columnID, XYZ ColumnOrientation, XYZ pt1, XYZ pt2, bool bEndingColumns = false)
        {
            XYZ UnitVectorAlongLine
                = null;

            XYZ point1 = new XYZ(pt1.X, pt1.Y, 0);
            XYZ point2 = new XYZ(pt1.X, pt1.Y, 1);
            Line axis = Line.CreateBound(point1, point2);

            // This logic is to rotate the column such that it is perpendicular to Input line
            using (Transaction tx = new Transaction(m_Document))
            {
                GenericUtils.SupressWarningsInTransaction(tx);

                _ = tx.Start("Change Orientation");

                double dAngle = 0;

                XYZ LineOrientation = pt2 - pt1;
                UnitVectorAlongLine = LineOrientation.Normalize();

                if ((ColumnOrientation.X == 0 && !MathUtils.ApproximatelyEqual(LineOrientation.X, 0)) || (ColumnOrientation.Y == 0 && !MathUtils.ApproximatelyEqual(LineOrientation.Y, 0)))
                {
                    dAngle = Math.PI * 90 / 180;
                }

                ElementTransformUtils.RotateElement(m_Document, columnID, axis, dAngle);

                _ = tx.Commit();
            }


            using (Transaction tx = new Transaction(m_Document))
            {
                GenericUtils.SupressWarningsInTransaction(tx);

                _ = tx.Start("Change Orientation2");

                // Compute the orientation after rotation. 
                FamilyInstance column = m_Document.GetElement(columnID) as FamilyInstance;
                XYZ newOrientation = column.FacingOrientation;

                // End columns should face towards the line and also each other
                if (bEndingColumns)
                {
                    // The web outward normal should be in a direction opposite to that of Input Line For Start and End Lines
                    if (MathUtils.CompareVectors(UnitVectorAlongLine, newOrientation) == "Parallel")
                    {
                        ElementTransformUtils.RotateElement(m_Document, columnID, axis, Math.PI);
                    }
                }

                // Columns should point to low eve
                else
                {
                    XYZ SlopeDirection = GetRoofSlopeDirection(pt1);

                    // The web outward normal should be in a direction of slope
                    if (MathUtils.IsParallel(SlopeDirection, newOrientation))
                    {
                        if (MathUtils.CompareVectors(SlopeDirection, newOrientation) == "Anti-Parallel")
                        {
                            ElementTransformUtils.RotateElement(m_Document, columnID, axis, Math.PI);
                        }
                    }
                }
                _ = tx.Commit();
            }

        }

        private void RotateColumn(ElementId columnID, XYZ pt1, XYZ pt2, double dAngle)
        {
            XYZ point1 = new XYZ(pt1.X, pt1.Y, 0);
            XYZ point2 = new XYZ(pt1.X, pt1.Y, 1);
            Line axis = Line.CreateBound(point1, point2);

            using (Transaction tx = new Transaction(m_Document))
            {
                GenericUtils.SupressWarningsInTransaction(tx);

                _ = tx.Start("Change Orientation");

                ElementTransformUtils.RotateElement(m_Document, columnID, axis, dAngle);

                _ = tx.Commit();
            }
        }

        private XYZ GetRoofSlopeDirection(XYZ pt1)
        {
            XYZ SlopeDirect = null;

            RoofObject targetRoof;
            targetRoof.slopeLine = null;

            foreach (RoofObject roof in RoofUtility.colRoofs)
            {
                double Xmin, Xmax, Ymin, Ymax = 0.0;
                Xmin = Math.Min(roof.max.X, roof.min.X);
                Xmax = Math.Max(roof.max.X, roof.min.X);
                Ymin = Math.Min(roof.max.Y, roof.min.Y);
                Ymax = Math.Max(roof.max.Y, roof.min.Y);

                if (pt1.X > Xmin && pt1.X < Xmax && pt1.Y > Ymin && pt1.Y < Ymax)
                {
                    targetRoof = roof;
                    break;
                }
            }

            //we are trying to intersect the point with extended roof
            foreach (RoofObject roof in RoofUtility.colExtendedRoofs)
            {
                double Xmin, Xmax, Ymin, Ymax = 0.0;
                Xmin = Math.Min(roof.max.X, roof.min.X);
                Xmax = Math.Max(roof.max.X, roof.min.X);
                Ymin = Math.Min(roof.max.Y, roof.min.Y);
                Ymax = Math.Max(roof.max.Y, roof.min.Y);

                if (pt1.X > Xmin && pt1.X < Xmax && pt1.Y > Ymin && pt1.Y < Ymax)
                {
                    targetRoof = roof;
                    break;
                }
            }

            if (targetRoof.slopeLine != null)
            {
                Curve SlopeCurve = targetRoof.slopeLine;
                XYZ start = SlopeCurve.GetEndPoint(0);
                XYZ end = SlopeCurve.GetEndPoint(1);

                XYZ slope = start.Z > end.Z ? (end - start) : (start - end);

                SlopeDirect = new XYZ(slope.X, slope.Y, pt1.Z);
            }

            return SlopeDirect;
        }

        private Element GetRoofAtPoint(XYZ pt1)
        {
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


            return targetRoof.roofElementID != null ? m_Document.GetElement(targetRoof.roofElementID) : null;
        }

        /// <summary>
        /// Moves the point P1 in the direction of the line represented by P1, P2
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="lineType"></param>
        private XYZ AdjustLinePoint(XYZ pt1, XYZ pt2, LineType lineType, double dOffset)
        {
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

            if (lineType == LineType.horizontal)
            {
                AdjustedPoint = pt1.X < pt2.X ? pt1 + tempXVector : pt1 + tempXMinusVector;
            }
            return AdjustedPoint;
        }

        private void ProcessT62InputLine(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
            ErrorHandler.elemIDbeingProcessed = inputLine.id;

            try
            {
                XYZ pt1 = inputLine.locationCurve.Curve.GetEndPoint(0);
                XYZ pt2 = inputLine.locationCurve.Curve.GetEndPoint(1);


                Level toplevel = null, baseLevel = null;

                string strBuildingName = "Building " + inputLine.strBuildingName;
                // Filter levels based on buldings to use
                List<Level> filteredLevels = new List<Level>();
                foreach (Level filteredlevel in levels)
                {
                    if (filteredlevel.Name.Contains(strBuildingName))
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

                        inputLine.strT62Type = i == 0
                            ? inputLine.strT62Type.ToString() + " (Flush Bottom / Female Top)"
                            : i > 0 && i < levels.Count() - 2
                                ? inputLine.strT62Type.ToString() + " (Male Bottom / Female Top)"
                                : inputLine.strT62Type.ToString() + " (Male Bottom / Flush Top)";

                        break;
                    }
                }

                FamilySymbol columnType = SymbolCollector.GetSymbol(inputLine.strT62Type, "T62");
                XYZ T62Orientation = null;
                ElementId t62ElementId = null;

                foreach (XYZ studPoint in inputLine.gridIntersectionPoints)
                {
                    using (Transaction tx = new Transaction(m_Document))
                    {
                        GenericUtils.SupressWarningsInTransaction(tx);

                        _ = tx.Start("Place Column");


                        FamilyInstance column = m_Document.Create.NewFamilyInstance(studPoint, columnType, baseLevel, StructuralType.Column);

                        //m_Form.PostMessage(string.Format("Placing T62  {3} at {0} , {1} , {2} \n \n ", studPoint.X, studPoint.Y, studPoint.Z, inputLine.strT62Type));

                        if (inputLine.dParapetHeight == 0)
                        {
                            _ = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                            _ = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
                        }
                        else
                        {
                            _ = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(baseLevel.Id);
                            _ = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(inputLine.dParapetHeight);
                        }

                        t62ElementId = column.Id;
                        T62Orientation = column.FacingOrientation;

                        _ = tx.Commit();

                    }

                    UpdateOrientation(t62ElementId, T62Orientation, studPoint, pt2, false);
                }
            }
            catch (Exception) { }
        }

        private void ProcessT62AndStudLine(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
            ProcessT62InputLine(inputLine, levels);
            ProcessStudInputLine(inputLine, levels);
        }

        private bool CheckForExistingColumns(XYZ pt)
        {
            BoundingBoxContainsPointFilter filter = new BoundingBoxContainsPointFilter(pt);

            // Apply the filter to the elements in the active document
            // This filter will excludes all objects derived from View and objects derived from ElementType
            FilteredElementCollector collector = new FilteredElementCollector(m_Document);
            IList<Element> elements = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_StructuralColumns).ToElements();

            return elements.Count > 0;
        }

        private void ProcessDoubleStudAtGrids(InputLine inputLine, IOrderedEnumerable<Level> levels, FamilySymbol columnType, Element topAttachElement, Element bottomAttachElement, Level toplevel, Level baseLevel)
        {

            double dFlangeWidth = GenericUtils.FlangeWidth(inputLine.strStudType);
            int iMovementFactor = 1; // This will be used to determine the movement direction of the Second stud before rotation
            try
            {
                for (int J = 0; J < 2; J++)
                {
                    XYZ pt1 = null, pt2 = null;
                    GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);

                    XYZ AdjustedLinePoint = null;
                    LineType lineType = LineType.vertical;
                    XYZ lineOrientation = GenericUtils.GetLineOrientation(inputLine);

                    if (MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y))
                    {
                        lineType = LineType.horizontal;
                    }

                    ElementId columnID = null;


                    foreach (XYZ studPoint in inputLine.gridIntersectionPoints)
                    {
                        using (Transaction tx = new Transaction(m_Document))
                        {
                            GenericUtils.SupressWarningsInTransaction(tx);

                            _ = tx.Start("Place Column");


                            FamilyInstance column = m_Document.Create.NewFamilyInstance(studPoint, columnType, baseLevel, StructuralType.Column);

                            //m_Form.PostMessage(string.Format("Placing T62  {3} at {0} , {1} , {2} \n \n ", studPoint.X, studPoint.Y, studPoint.Z, inputLine.strT62Type));

                            if (inputLine.dParapetHeight == 0)
                            {
                                _ = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                                _ = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
                            }
                            else
                            {
                                _ = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(baseLevel.Id);
                                _ = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(inputLine.dParapetHeight);
                            }

                            // If we coulfdn't find a floor, the input line is on top floor
                            // Need to attach it to roof
                            topAttachElement = GetNearestFloorOrRoof(toplevel, studPoint);
                            bottomAttachElement = GetNearestFloorOrRoof(baseLevel, studPoint);


                            if (topAttachElement == null)
                            {
                                topAttachElement = GetRoofAtPoint(studPoint);
                            }

                            if (topAttachElement != null)
                            {
                                ColumnAttachment.AddColumnAttachment(m_Document, column, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                            }

                            // main Stud Should go Under the floor
                            if (J != 0 && bottomAttachElement != null)
                            {
                                ColumnAttachment.AddColumnAttachment(m_Document, column, bottomAttachElement, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                            }

                            columnID = column.Id;

                            _ = tx.Commit();

                            UpdateOrientation(columnID, column.FacingOrientation, studPoint, pt2, false);

                            XYZ newOrientation = column.FacingOrientation;

                            // Figure 3 - Dev Guide
                            if (MathUtils.CompareVectors(lineOrientation, newOrientation) == "Parallel")
                                    iMovementFactor = -1;

                            //If line orientation and Web orientaion are parallel, we need to move point towards origin
                            //if Line Orientation and Web orientation are Anti-parallel, we move the point away from origin
                            if (inputLine.dFlangeOfset != 0)
                            {
                                if (MathUtils.CompareVectors(lineOrientation, newOrientation) == "Parallel")
                                    AdjustedLinePoint = AdjustLinePoint(studPoint, pt2, lineType, -dFlangeWidth/ 2);
                                else
                                    AdjustedLinePoint = AdjustLinePoint(studPoint, pt2, lineType, dFlangeWidth / 2);
                                MoveColumn(columnID, AdjustedLinePoint);
                            }


                            if (J != 0)
                            {
                                XYZ newlocation = AdjustedLinePoint == null ? studPoint : AdjustedLinePoint;

                                XYZ Adjustedpt1 = AdjustLinePoint(newlocation, pt2, lineType, dFlangeWidth * iMovementFactor);
                                MoveColumn(columnID, Adjustedpt1);
                                RotateColumn(columnID, newlocation, pt2, Math.PI);
                            }
                        }
                    }


                }
            }
            catch (Exception)
            {
                m_Form.PostMessage(string.Format("\n !!! Failed  To Process InputLine {0}, at {1}. Please Review", inputLine.id, DateTime.Now), true);
            }
        }

        private void ProcessContinuousDoubleStuds(InputLine inputLine, 
                                                    XYZ pt1, XYZ pt2, 
                                                    FamilySymbol columnType, 
                                                    Element topAttachElement, 
                                                    Element bottomAttachElement, 
                                                    Level toplevel, 
                                                    Level baseLevel,
                                                    LineType lineType,
                                                    XYZ lineOrientation)
        {
            // Place Two Double Studs at end
            ProcessDoubleStudAtEnds(inputLine, pt1, pt2, columnType, topAttachElement, bottomAttachElement, toplevel, baseLevel);

            double dFlangeWidth = GenericUtils.FlangeWidth(inputLine.strStudType);
            int iMovementFactor = 1; // This will be used to determine the movement direction of the Second stud before rotation

            if (inputLine.dOnCenter == 0)
            {
                return;
            }

            for (int i = 0; i < 2; i++)
            {
                XYZ studPoint = null, studEndPoint = null, studStartPoint = null;

                // For placing On Center's This is the Logic
                // Compute the Start point for On center placement in a given line
                // Check for Flange Offset Parameter and adjust accordingly

                XYZ FlangeOffsetXVector = new XYZ(dFlangeWidth / 2, 0, 0);
                XYZ FlangeOffsetYVector = new XYZ(0, dFlangeWidth / 2, 0);


                if (lineType == LineType.vertical)
                {
                    studPoint = ComputeOnCenterStartingPoint(pt1, pt2, inputLine, inputLine.dOnCenter, lineType);
                    if (inputLine.dFlangeOfset == 1)
                    {
                        studPoint += FlangeOffsetYVector;
                    }
                    studStartPoint = pt1.Y > pt2.Y ? pt2 : pt1;
                    studEndPoint = pt1.Y > pt2.Y ? pt1 : pt2;
                }
                else
                {
                    studPoint = ComputeOnCenterStartingPoint(pt1, pt2, inputLine, inputLine.dOnCenter, lineType);
                    if (inputLine.dFlangeOfset == 1)
                    {
                        studPoint += FlangeOffsetXVector;
                    }
                    studStartPoint = pt1.X > pt2.X ? pt2 : pt1;
                    studEndPoint = pt1.X > pt2.X ? pt1 : pt2;
                }

                XYZ tempXVector = new XYZ(inputLine.dOnCenter, 0, 0);
                XYZ tempYVector = new XYZ(0, inputLine.dOnCenter, 0);

                bool bCanCreateColumn = true;
                while (bCanCreateColumn)
                {
                    ElementId StudColumnID = null;
                    XYZ StudColumnOrientation;
                    FamilyInstance studColumn = null;
                    if ((lineType == LineType.vertical && studPoint.Y < (studEndPoint.Y - 1.0)) || (lineType == LineType.horizontal && studPoint.X < (studEndPoint.X - 1.0)))
                    {

                        XYZ AdjustedLinePoint = null;

                        if ((lineType == LineType.vertical && studPoint.Y > (studStartPoint.Y + 0.64)) || (lineType == LineType.horizontal && studPoint.X > (studStartPoint.X + 0.64))) // This condition ensures there are no collisions at the start
                        {
                            using (Transaction tx = new Transaction(m_Document))
                            {
                                GenericUtils.SupressWarningsInTransaction(tx);

                                _ = tx.Start("Placing posts");
                                studColumn = m_Document.Create.NewFamilyInstance(studPoint, columnType, baseLevel, StructuralType.Column);

                                if (inputLine.dParapetHeight == 0)
                                {
                                    _ = studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                                    _ = studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
                                }
                                else
                                {
                                    _ = studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(baseLevel.Id);
                                    _ = studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(inputLine.dParapetHeight);
                                }


                                topAttachElement = GetNearestFloorOrRoof(toplevel, studPoint);
                                bottomAttachElement = GetNearestFloorOrRoof(baseLevel, studPoint);

                                // If we coulfdn't find a floor, the input line is on top floor
                                // Need to attach it to roof
                                if (topAttachElement == null)
                                {
                                    topAttachElement = GetRoofAtPoint(studPoint);
                                }

                                if (topAttachElement != null)
                                {
                                    ColumnAttachment.AddColumnAttachment(m_Document, studColumn, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                                }

                                if (bottomAttachElement != null)
                                {
                                    ColumnAttachment.AddColumnAttachment(m_Document, studColumn, bottomAttachElement, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                                }


                                //m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", studPoint.X, studPoint.Y, studPoint.Z, inputLine.strStudType));
                                StudColumnID = studColumn.Id;
                                StudColumnOrientation = studColumn.FacingOrientation;
                                _ = tx.Commit();
                            }

                            UpdateOrientation(StudColumnID, StudColumnOrientation, studPoint, pt2);


                            // Compute the orientation after rotation. 
                            FamilyInstance column = m_Document.GetElement(StudColumnID) as FamilyInstance;
                            XYZ newOrientation = column.FacingOrientation;

                            if (MathUtils.CompareVectors(lineOrientation, newOrientation) == "Parallel")
                                iMovementFactor = -1;

                            //If line orientation and Web orientaion are is same direction, we need to move the column back by Flange Width
                            // This is because, we have moved the point away from origin while computing the stud on-center point.
                            // Figure 2 Dev Guide
                            if (inputLine.dFlangeOfset != 0 && MathUtils.CompareVectors(lineOrientation, newOrientation) == "Parallel")
                            {
                                XYZ lineEndPoint = null;

                                if (lineType == LineType.vertical)
                                {
                                    lineEndPoint = pt1.Y > pt2.Y ? pt1 : pt2;
                                }

                                if (lineType == LineType.horizontal)
                                {
                                    lineEndPoint = pt1.X > pt2.X ? pt1 : pt2;
                                }

                                AdjustedLinePoint = AdjustLinePoint(studPoint, lineEndPoint, lineType, -dFlangeWidth);
                                MoveColumn(StudColumnID, AdjustedLinePoint);
                            }
                        }


                        if (i != 0 && StudColumnID != null)
                        {
                            XYZ newLocation = AdjustedLinePoint == null ? studPoint : AdjustedLinePoint;

                            XYZ Adjustedpt1 = AdjustLinePoint(newLocation, pt2, lineType, dFlangeWidth * iMovementFactor);
                            MoveColumn(StudColumnID, Adjustedpt1);
                            RotateColumn(StudColumnID, newLocation, pt2, Math.PI);
                        }

                        // Move to next point
                        if (lineType == LineType.vertical)
                        {
                            studPoint += tempYVector;
                        }
                        else
                        {
                            studPoint += tempXVector;
                        }
                    }
                    else
                    {
                        bCanCreateColumn = false;
                    }
                }
            }
        }

        private void ProcessDoubleStudAtEnds(InputLine inputLine, XYZ pt1, XYZ pt2, FamilySymbol columnType, Element topAttachElement, Element bottomAttachElement, Level toplevel, Level baseLevel)
        {
            ElementId startColumnID = null, EndColumnID = null;
            XYZ startColumnOrientation = null, endColumnOrientation = null;

            bool bAtStart = true, bAtEnd = true;

            if (inputLine.strDoubleStudType.Contains("- R"))
            {
                bAtStart = false;
            }
            if (inputLine.strDoubleStudType.Contains("- L"))
            {
                bAtEnd = false;
            }

            LineType lineType = LineType.vertical;

            if (MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y))
            {
                lineType = LineType.horizontal;
            }

            for (int i = 0; i < 2; i++)
            {

                // Start a new Tranasaction
                using (Transaction tx = new Transaction(m_Document))
                {

                    GenericUtils.SupressWarningsInTransaction(tx);

                    _ = tx.Start("Place Column");

                    _ = CheckForExistingColumns(pt1);

                    if (bAtStart)
                    {
                        //Place Column at start
                        FamilyInstance startcolumn = m_Document.Create.NewFamilyInstance(pt1, columnType, baseLevel, StructuralType.Column);

                        if (inputLine.dParapetHeight == 0)
                        {
                            _ = startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                            _ = startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
                        }
                        else
                        {
                            _ = startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(baseLevel.Id);
                            _ = startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(inputLine.dParapetHeight);
                        }

                        // If we coulfdn't find a floor, the input line is on top floor
                        // Need to attach it to roof

                        topAttachElement = GetNearestFloorOrRoof(toplevel, pt1);
                        bottomAttachElement = GetNearestFloorOrRoof(baseLevel, pt1);

                        if (topAttachElement == null)
                        {
                            topAttachElement = GetRoofAtPoint(pt1);
                        }

                        if (topAttachElement != null)
                        {
                            ColumnAttachment.AddColumnAttachment(m_Document, startcolumn, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                        }

                        if (i != 0 && bottomAttachElement != null)
                        {
                            ColumnAttachment.AddColumnAttachment(m_Document, startcolumn, bottomAttachElement, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                        }

                        // m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", pt1.X, pt1.Y, pt1.Z, inputLine.strStudType));
                        startColumnID = startcolumn.Id;
                        startColumnOrientation = startcolumn.FacingOrientation;
                    }

                    if (bAtEnd)
                    {
                        // Place column at end
                        FamilyInstance endColumn = m_Document.Create.NewFamilyInstance(pt2, columnType, baseLevel, StructuralType.Column);

                        if (inputLine.dParapetHeight == 0)
                        {
                            _ = endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                            _ = endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
                        }
                        else
                        {
                            _ = endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(baseLevel.Id);
                            _ = endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(inputLine.dParapetHeight);
                        }

                        // If we coulfdn't find a floor, the input line is on top floor
                        // Need to attach it to roof

                        topAttachElement = GetNearestFloorOrRoof(toplevel, pt2);
                        bottomAttachElement = GetNearestFloorOrRoof(baseLevel, pt2);

                        if (topAttachElement == null)
                        {
                            topAttachElement = GetRoofAtPoint(pt2);
                        }

                        if (topAttachElement != null)
                        {
                            ColumnAttachment.AddColumnAttachment(m_Document, endColumn, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                        }

                        if (i != 0 && bottomAttachElement != null)
                        {
                            ColumnAttachment.AddColumnAttachment(m_Document, endColumn, bottomAttachElement, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                        }

                        //m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", pt2.X, pt2.Y, pt2.Z, inputLine.strStudType));
                        EndColumnID = endColumn.Id;
                        endColumnOrientation = endColumn.FacingOrientation;
                    }

                    _ = tx.Commit();
                }

                double dFlangeWidth = GenericUtils.FlangeWidth(inputLine.strStudType);

                if (bAtStart)
                {
                    UpdateOrientation(startColumnID, startColumnOrientation, pt1, pt2, true);
                    XYZ AdjustedStart = AdjustLinePoint(pt1, pt2, lineType, 1.5 * dFlangeWidth);
                    MoveColumn(startColumnID, AdjustedStart);

                    if (i != 0)
                    {
                        XYZ Adjustedpt1 = AdjustLinePoint(AdjustedStart, pt2, lineType, dFlangeWidth);
                        MoveColumn(startColumnID, Adjustedpt1);
                        RotateColumn(startColumnID, AdjustedStart, pt2, Math.PI);
                    }
                }

                if (bAtEnd)
                {
                    UpdateOrientation(EndColumnID, endColumnOrientation, pt2, pt1, true);
                    XYZ AdjustedEnd = AdjustLinePoint(pt2, pt1, lineType, 1.5 * dFlangeWidth);
                    MoveColumn(EndColumnID, AdjustedEnd);

                    if (i != 0)
                    {
                        XYZ Adjustedpt2 = AdjustLinePoint(AdjustedEnd, pt1, lineType, dFlangeWidth);
                        MoveColumn(EndColumnID, Adjustedpt2);
                        RotateColumn(EndColumnID, AdjustedEnd, pt1, Math.PI);
                    }
                }
            }
        }

        private Element GetNearestFloorOrRoof(Level level, XYZ pt1)
        {
            List<FloorObject> floorObjects = FloorHelper.colFloors;

            Element elemID = null;

            // match the building name as the level
            List<FloorObject> filteredFloors = new List<FloorObject>();

            foreach (FloorObject floorObject in floorObjects)
            {
                if (level.Name.Contains(floorObject.strBuildingName))
                {
                    filteredFloors.Add(floorObject);
                }
            }


            foreach (FloorObject floor in filteredFloors)
            {
                if (floor.min == null || floor.max == null)
                {
                    continue;
                }

                // match the bounding box of the point with the Floor Range
                double Xmin, Xmax, Ymin, Ymax = 0.0;
                Xmin = Math.Min(floor.max.X, floor.min.X);
                Xmax = Math.Max(floor.max.X, floor.min.X);
                Ymin = Math.Min(floor.max.Y, floor.min.Y);
                Ymax = Math.Max(floor.max.Y, floor.min.Y);

                if (pt1.X >= Xmin && pt1.X <= Xmax && pt1.Y >= Ymin && pt1.Y <= Ymax)
                {
                    Element levelElement = m_Document.GetElement(floor.levelID);
                    Parameter elevationParam = levelElement.get_Parameter(BuiltInParameter.LEVEL_ELEV);
                    if (elevationParam != null)
                    {
                        if (MathUtils.IsWithInRange(elevationParam.AsDouble(), level.Elevation + 3, level.Elevation - 3))
                        {
                            elemID = m_Document.GetElement(floor.elemID);
                            break;
                        }
                    }
                }
            }
            return elemID;
        }

        private void MoveColumn(ElementId columnId, XYZ newLocation)
        {
            if (columnId == null)
                return;

            FamilyInstance column = m_Document.GetElement(columnId) as FamilyInstance;

            using (Transaction tx = new Transaction(m_Document))
            {
                GenericUtils.SupressWarningsInTransaction(tx);

                _ = tx.Start("Change Orientation");

                // Get the column's Location property
                Location location = column.Location;

                // Check if the column's location is a LocationPoint
                if (location is LocationPoint locationPoint)
                {
                    // Set the new location for the column
                    locationPoint.Point = newLocation;
                }

                _ = tx.Commit();
            }
        }

        

        // This method returns the location of the nearest main grid to the given line.
        // The nearest main grid could be intersecting or not intersectin the line
        private XYZ GetNearestMainGridLocation(XYZ pt1, XYZ pt2, InputLine inputLine, LineType lineType)
        {
            XYZ nearestMainGridLocation;
            if (inputLine.mainGridIntersectionPoints.Count > 0)
            {
                nearestMainGridLocation = inputLine.mainGridIntersectionPoints[0];
            }
            else
            {
                XYZ referencePt;
                if (lineType == LineType.horizontal)
                {
                    referencePt = pt1.X < pt2.X ? pt1 : pt2;
                    nearestMainGridLocation = GetNearestPoint(GridCollector.mVerticalMainLines, referencePt, lineType);
                }
                else
                {
                    referencePt = pt1.Y < pt2.Y ? pt1 : pt2;
                    nearestMainGridLocation = GetNearestPoint(GridCollector.mHorizontalMainLines, referencePt, lineType);
                }

            }

            return nearestMainGridLocation;
        }

        private XYZ GetNearestPoint(List<Tuple<XYZ, XYZ>> gridLinesCollection, XYZ referencePoint, LineType lineType)
        {
            XYZ nearestPoint = null;
            double minDistance = double.MaxValue;
            foreach (Tuple<XYZ, XYZ> gridline in gridLinesCollection)
            {
                XYZ point = lineType == LineType.horizontal
        ? new XYZ(gridline.Item1.X, referencePoint.Y, referencePoint.Z)
        : new XYZ(referencePoint.X, gridline.Item1.Y, referencePoint.Z);

                double distance = point.DistanceTo(referencePoint);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPoint = point;
                }
            }

            return nearestPoint;
        }

        private XYZ ComputeOnCenterStartingPoint(XYZ lineStart, XYZ lineEnd, InputLine inputLine, double dOnCenter, LineType lineType)
        {
            XYZ OnCenterStartPoint = null;

            // Temp Vectors
            XYZ positiveXVector = new XYZ(dOnCenter, 0, 0);
            XYZ positiveYVector = new XYZ(0, dOnCenter, 0);
            XYZ negativeXVector = new XYZ(-dOnCenter, 0, 0);
            XYZ NegativeYVector = new XYZ(0, -dOnCenter, 0);
            XYZ refpoint;
            XYZ refpoint2;
            // Refpoint is computed such that it is always the smalllest among the given
            // Input points
            if (lineType == LineType.horizontal)
            {
                refpoint = lineStart.X < lineEnd.X ? lineStart : lineEnd;
                refpoint2 = lineStart.X > lineEnd.X ? lineStart : lineEnd;
            }
            else
            {
                refpoint = lineStart.Y < lineEnd.Y ? lineStart : lineEnd;
                refpoint2 = lineStart.Y > lineEnd.Y ? lineStart : lineEnd;
            }
            // Get the Nearest GridPoint to the start of the line
            XYZ nearestGridPoint = GetNearestMainGridLocation(lineStart, lineEnd, inputLine, lineType);


            // We have 4 cases

            // 1. nearestGridPoint is with in the input line and line is horizontal
            if (lineType == LineType.horizontal &&
                nearestGridPoint.X > refpoint.X &&
                nearestGridPoint.X < refpoint2.X)
            {
                OnCenterStartPoint = nearestGridPoint;
                while (OnCenterStartPoint.X > refpoint.X)
                {
                    OnCenterStartPoint += negativeXVector;
                }
                OnCenterStartPoint += positiveXVector;
            }

            // 2. nearestGridPoint is outside the input line and line is horizontal
            if (lineType == LineType.horizontal &&
                nearestGridPoint.X < refpoint.X &&
                nearestGridPoint.X < refpoint2.X)
            {
                OnCenterStartPoint = nearestGridPoint;
                while (OnCenterStartPoint.X < refpoint.X)
                {
                    OnCenterStartPoint += positiveXVector;
                }
            }

            // 2.2 nearestGridPoint is outside the input line and line is horizontal
            if (lineType == LineType.horizontal &&
                nearestGridPoint.X > refpoint.X &&
                nearestGridPoint.X > refpoint2.X)
            {
                OnCenterStartPoint = nearestGridPoint;
                while (OnCenterStartPoint.X > refpoint.X)
                {
                    OnCenterStartPoint += negativeXVector;
                }
                OnCenterStartPoint += positiveXVector;
            }

            // 3. nearestGridPoint is with in the input line and line is vertical
            if (lineType == LineType.vertical &&
               nearestGridPoint.Y > refpoint.Y &&
               nearestGridPoint.Y < refpoint2.Y)
            {
                OnCenterStartPoint = nearestGridPoint;
                while (OnCenterStartPoint.Y > refpoint.Y)
                {
                    OnCenterStartPoint += NegativeYVector;
                }
                OnCenterStartPoint += positiveYVector;
            }

            // 4. nearestGridPoint is outside the input line and line is vertical - nearest grid at bottom of line
            if (lineType == LineType.vertical &&
               nearestGridPoint.Y < refpoint.Y &&
               nearestGridPoint.Y < refpoint2.Y)
            {
                OnCenterStartPoint = nearestGridPoint;
                while (OnCenterStartPoint.Y < refpoint.Y)
                {
                    OnCenterStartPoint += positiveYVector;
                }
            }

            // 4.2 nearestGridPoint is outside the input line and line is vertical - Nearest Grid at top of line
            if (lineType == LineType.vertical &&
               nearestGridPoint.Y > refpoint.Y &&
               nearestGridPoint.Y > refpoint2.Y)
            {
                OnCenterStartPoint = nearestGridPoint;
                while (OnCenterStartPoint.Y > refpoint.Y)
                {
                    OnCenterStartPoint += NegativeYVector;
                }
                OnCenterStartPoint += positiveYVector;
            }

            return OnCenterStartPoint;
        }
    }
}

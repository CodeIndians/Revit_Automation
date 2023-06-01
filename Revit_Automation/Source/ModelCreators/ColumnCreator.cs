using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Autodesk.Revit.DB.SpecTypeId;
using static Revit_Automation.Source.Utils.WarningSwallowers;

namespace Revit_Automation.Source.ModelCreators
{
    public class ColumnCreator : IModelCreator
    {
        enum LineType
        { 
            vertical =0, horizontal = 1
        }
        private Autodesk.Revit.DB.Document m_Document { get; set; }

        private Form1 m_Form {  get; set; }
        public ColumnCreator(Autodesk.Revit.DB.Document doc, Form1 form) {

            m_Document = doc;
            m_Form = form;
        }
        public void CreateModel(List<CustomTypes.InputLine> colInputLines, IOrderedEnumerable<Level> levels) 
        {
            ProcessInputLines(colInputLines, levels);
        }
        private  void ProcessInputLines(List<InputLine> inputLinesCollection, IOrderedEnumerable<Level> levels)
        {
            try
            {
                foreach (InputLine inputLine in inputLinesCollection)
                {
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
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.reportError();
            }
        }

        private void ProcessDoubleStud(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
            ErrorHandler.elemIDbeingProcessed = inputLine.id;


            try
            {
                // To-Do : This is repeated Code - Chances of moving to separate Method
                XYZ pt1 = inputLine.locationCurve.Curve.GetEndPoint(0);
                XYZ pt2 = inputLine.locationCurve.Curve.GetEndPoint(1);
                
                LineType lineType = LineType.vertical;

                if (MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y))
                    lineType = LineType.horizontal;
                //compute levels
                Level toplevel = null, baseLevel = null;

                string strBuildingName = "Building " + inputLine.strBuildingName;
                // Filter levels based on buldings to use
                List<Level> filteredLevels = new List<Level>();
                foreach (Level filteredlevel in levels)
                {
                    if (filteredlevel.Name.Contains(strBuildingName))
                        filteredLevels.Add(filteredlevel);
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
                Element topAttachElement = GetNearestFloor(toplevel);

                inputLine.strStudType = inputLine.strStudType.ToString() + string.Format(" x {0}ga", inputLine.strStudGuage);

                FamilySymbol columnType = SymbolCollector.GetSymbol(inputLine.strStudType, "Post");


                // We have 3 types of Double Stud Conditions. 
                // 1 - At Ends - Place Double Stud at Start and End
                // 2 - At Grid - Intersection of the Input Line and Grids
                // 3 - Continuous - At given On Center
                // In a Double stud the Main Stud should Exend below floor by 3 feet, the secondary studs should flush at the floor

                if (inputLine.strDoubleStudType == "At Ends")
                {
                    ProcessDoubleStudAtEnds(inputLine, pt1, pt2, columnType, topAttachElement, toplevel, baseLevel);
                }
                else if (inputLine.strDoubleStudType == "Grid")
                {
                    ProcessDoubleStudAtGrids(inputLine, levels, columnType, topAttachElement, toplevel, baseLevel);
                }

                else if (inputLine.strDoubleStudType == "Continuous")
                {


                    if (inputLine.dOnCenter == 0)
                        return;

                    XYZ studPoint = null, studEndPoint = null;

                    // For placing On Center's This is the Logic
                    // First Check if the line is vertical or Horizontal,
                    // Get the Horizontal Grids[0] / Vertical Grids [0] 
                    // Assumption is from which ever grid we take, the On-Center points will be same

                    if (lineType == LineType.vertical)
                    {
                        XYZ referencePoint = pt1.Y < pt2.Y ? pt1 : pt2;
                        studPoint = new XYZ(referencePoint.X, GridCollector.mHorizontalMainLines[0].Item1.Y, GridCollector.mHorizontalMainLines[0].Item1.Z);
                        while ((studPoint.Y + inputLine.dOnCenter) < referencePoint.Y)
                        {
                            studPoint = new XYZ(studPoint.X, studPoint.Y + inputLine.dOnCenter, referencePoint.Z);
                        }
                        studEndPoint = pt1.Y > pt2.Y ? pt1 : pt2;
                    }
                    else
                    {
                        XYZ referencePoint = pt1.X < pt2.X ? pt1 : pt2;
                        studPoint = new XYZ(GridCollector.mVerticalMainLines[0].Item1.X, referencePoint.Y, GridCollector.mHorizontalMainLines[0].Item1.Z);
                        while ((studPoint.X + inputLine.dOnCenter) < referencePoint.X)
                        {
                            studPoint = new XYZ(referencePoint.X + inputLine.dOnCenter, studPoint.Y, referencePoint.Z);
                        }
                        studEndPoint = pt1.X > pt2.X ? pt1 : pt2;
                    }

                    XYZ tempXVector = new XYZ(inputLine.dFlangeOfset, 0, 0);
                    XYZ tempYVector = new XYZ(0, inputLine.dFlangeOfset, 0);

                    bool bCanCreateColumn = true;
                    while (bCanCreateColumn)
                    {
                        ElementId StudColumnID;
                        XYZ StudColumnOrientation;

                        if (lineType == LineType.vertical)
                            studPoint = studPoint + tempYVector;
                        else
                            studPoint = studPoint + tempXVector;

                        if ((lineType == LineType.vertical && studPoint.Y < (studEndPoint.Y - 1.0)) || (lineType == LineType.horizontal && studPoint.X < (studEndPoint.X - 1.0)))
                        {
                            using (Transaction tx = new Transaction(m_Document))
                            {
                                SupressWarningsInTransaction(tx);

                                tx.Start("Placing posts");
                                FamilyInstance studColumn = m_Document.Create.NewFamilyInstance(studPoint, columnType, baseLevel, StructuralType.Column);
                                studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                                studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);

                                if (topAttachElement != null)
                                    ColumnAttachment.AddColumnAttachment(m_Document, studColumn, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Minimum, 0);

                                m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", studPoint.X, studPoint.Y, studPoint.Z, inputLine.strStudType));
                                StudColumnID = studColumn.Id;
                                StudColumnOrientation = studColumn.FacingOrientation;
                                tx.Commit();
                            }

                            UpdateOrientation(StudColumnID, StudColumnOrientation, studPoint, pt2);
                        }
                        else
                            bCanCreateColumn = false;

                    }
                }
            }

            catch (Exception)
            {
                ErrorHandler.reportError();
            }
        }

        private void SupressWarningsInTransaction(Transaction tx)
        {
            FailureHandlingOptions failureHandlingOptions =
                                tx.GetFailureHandlingOptions();

            DuplicateColumnWarningSwallower duplicateColumnWarningSwallower =
              new DuplicateColumnWarningSwallower();

            failureHandlingOptions.SetFailuresPreprocessor(
              duplicateColumnWarningSwallower);

            failureHandlingOptions.SetClearAfterRollback(
              true);

            tx.SetFailureHandlingOptions(
              failureHandlingOptions);
        }

        private void ProcessStudInputLine(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
            ErrorHandler.elemIDbeingProcessed = inputLine.id;

            if (inputLine.dOnCenter == 0)
                return;

            try
            {
                XYZ pt1 = inputLine.locationCurve.Curve.GetEndPoint(0);
                XYZ pt2 = inputLine.locationCurve.Curve.GetEndPoint(1);

                ElementId startColumnID, EndColumnID;
                XYZ startColumnOrientation, endColumnOrientation;

                LineType lineType = LineType.vertical;

                if (MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y))
                    lineType = LineType.horizontal;

                //compute levels
                Level toplevel = null, baseLevel = null;

                string strBuildingName = "Building " + inputLine.strBuildingName;
                // Filter levels based on buldings to use
                List<Level> filteredLevels = new List<Level>();
                foreach (Level filteredlevel in levels)
                {
                    if (filteredlevel.Name.Contains(strBuildingName))
                        filteredLevels.Add(filteredlevel);
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
                Element topAttachElement = GetNearestFloor(toplevel);


                inputLine.strStudType = inputLine.strStudType.ToString() + string.Format(" x {0}ga", inputLine.strStudGuage);

                FamilySymbol columnType = SymbolCollector.GetSymbol(inputLine.strStudType, "Post");

                using (Transaction tx = new Transaction(m_Document))
                {
                    SupressWarningsInTransaction(tx);

                    tx.Start("Place Column");

                    CheckForExistingColumns(pt1);

                    //Place Column at start
                    FamilyInstance startcolumn = m_Document.Create.NewFamilyInstance(pt1, columnType, baseLevel, StructuralType.Column);
                    startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                    startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);

                    if (topAttachElement != null)
                        ColumnAttachment.AddColumnAttachment(m_Document, startcolumn, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Minimum, 0);

                    m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", pt1.X, pt1.Y, pt1.Z, inputLine.strStudType));
                    startColumnID = startcolumn.Id;
                    startColumnOrientation = startcolumn.FacingOrientation;

                    // Place column at end
                    FamilyInstance endColumn = m_Document.Create.NewFamilyInstance(pt2, columnType, baseLevel, StructuralType.Column);
                    endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                    endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);

                    if (topAttachElement != null)
                        ColumnAttachment.AddColumnAttachment(m_Document, endColumn, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Minimum, 0);

                    m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", pt2.X, pt2.Y, pt2.Z, inputLine.strStudType));
                    EndColumnID = endColumn.Id;
                    endColumnOrientation = endColumn.FacingOrientation;


                    XYZ referencePoint = null;
                    if (inputLine.mainGridIntersectionPoints?.Count > 0)
                        referencePoint = inputLine.mainGridIntersectionPoints[0];

                    tx.Commit();
                }

                double dFlangeWidth = FlangeWidth(inputLine.strStudType);

                UpdateOrientation(startColumnID, startColumnOrientation, pt1, pt2, true);
                XYZ Adjustedpt1 = AdjustLinePoint(pt1, pt2, lineType, dFlangeWidth/2);
                MoveColumn(startColumnID, Adjustedpt1);

                UpdateOrientation(EndColumnID, endColumnOrientation, pt2, pt1, true);
                XYZ Adjustedpt2 = AdjustLinePoint(pt2, pt1, lineType, dFlangeWidth / 2);
                MoveColumn(EndColumnID, Adjustedpt2);

                XYZ studPoint = null, studEndPoint = null;

                // For placing On Center's This is the Logic
                // First Check if the line is vertical or Horizontal,
                // Get the Horizontal Grids[0] / Vertical Grids [0] 
                // Assumption is from which ever grid we take, the On-Center points will be same

                if (lineType == LineType.vertical)
                {   
                    XYZ referencePoint = pt1.Y < pt2.Y ? pt1 : pt2;
                    studPoint = new XYZ( referencePoint.X, GridCollector.mHorizontalMainLines[0].Item1.Y, GridCollector.mHorizontalMainLines[0].Item1.Z) ;
                    while ((studPoint.Y  + inputLine.dOnCenter ) < referencePoint.Y )
                    {
                        studPoint = new XYZ(studPoint.X , studPoint.Y + inputLine.dOnCenter, referencePoint.Z); 
                    }
                    studEndPoint = pt1.Y > pt2.Y ? pt1 : pt2;
                }
                else
                {
                    XYZ referencePoint = pt1.X < pt2.X ? pt1 : pt2;
                    studPoint =  new XYZ( GridCollector.mVerticalMainLines[0].Item1.X, referencePoint.Y, GridCollector.mHorizontalMainLines[0].Item1.Z);
                    while ((studPoint.X + inputLine.dOnCenter) < referencePoint.X)
                    {
                        studPoint = new XYZ(referencePoint.X + inputLine.dOnCenter, studPoint.Y , referencePoint.Z);
                    }
                    studEndPoint = pt1.X > pt2.X ? pt1 : pt2;
                }

                XYZ tempXVector = new XYZ(inputLine.dFlangeOfset, 0, 0);
                XYZ tempYVector = new XYZ(0, inputLine.dFlangeOfset, 0);

                bool bCanCreateColumn = true;
                while (bCanCreateColumn)
                {
                    ElementId StudColumnID;
                    XYZ StudColumnOrientation;

                    if (lineType == LineType.vertical)
                        studPoint = studPoint + tempYVector;
                    else
                        studPoint = studPoint + tempXVector;

                    if ((lineType == LineType.vertical && studPoint.Y < (studEndPoint.Y - 1.0)) || (lineType == LineType.horizontal && studPoint.X < (studEndPoint.X - 1.0)))
                    {
                        using (Transaction tx = new Transaction(m_Document))
                        {
                            SupressWarningsInTransaction(tx);

                            tx.Start("Placing posts");
                            FamilyInstance studColumn = m_Document.Create.NewFamilyInstance(studPoint, columnType, baseLevel, StructuralType.Column);
                            studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                            studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);

                            if (topAttachElement != null)
                                ColumnAttachment.AddColumnAttachment(m_Document, studColumn, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Minimum, 0);

                            m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", studPoint.X, studPoint.Y, studPoint.Z, inputLine.strStudType));
                            StudColumnID = studColumn.Id;
                            StudColumnOrientation = studColumn.FacingOrientation;
                            tx.Commit();
                        }

                        UpdateOrientation(StudColumnID, StudColumnOrientation, studPoint, pt2);
                    }
                    else
                        bCanCreateColumn = false;

                }
            }

            catch (Exception)
            {
                ErrorHandler.reportError();
            }
        }

        private XYZ computeInstersectionPoint(XYZ pt1, XYZ pt2, LineType lineType)
        {
        
            XYZ intersectPoint = null;

            if (lineType == LineType.horizontal)
                intersectPoint = new XYZ (GridCollector.mVerticalMainLines[0].Item1.X, pt1.Y, pt1.Z);
            else
                intersectPoint = new XYZ(pt1.X, GridCollector.mVerticalMainLines[0].Item1.Y, pt1.Z);


            return intersectPoint;

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
        private void UpdateOrientation(ElementId columnID, XYZ ColumnOrientation,  XYZ pt1, XYZ pt2 , bool bEndingColumns = false)
        {
            XYZ UnitVectorAlongLine
                = null;

            XYZ point1 = new XYZ(pt1.X, pt1.Y, 0);
            XYZ point2 = new XYZ(pt1.X, pt1.Y, 1);
            Line axis = Line.CreateBound(point1, point2);

            using (Transaction tx = new Transaction(m_Document))
            {
                SupressWarningsInTransaction(tx);

                tx.Start("Change Orientation");

                double dAngle = 0;

                XYZ LineOrientation = pt2 - pt1;
                UnitVectorAlongLine = LineOrientation.Normalize();

                if ((ColumnOrientation.X == 0 && !MathUtils.ApproximatelyEqual(LineOrientation.X, 0)) || (ColumnOrientation.Y == 0 && !MathUtils.ApproximatelyEqual(LineOrientation.Y, 0)))
                    dAngle = (Math.PI * 90) / 180;
                
                ElementTransformUtils.RotateElement(m_Document, columnID, axis, dAngle);

                tx.Commit();
            }

            using (Transaction tx = new Transaction(m_Document))
            {
                SupressWarningsInTransaction(tx);

                tx.Start("Change Orientation2");

                // Compute the orientation after rotation. 
                FamilyInstance column = m_Document.GetElement(columnID) as FamilyInstance;
                XYZ newOrientation = column.FacingOrientation;

                if (bEndingColumns)
                {
                    // The web outward normal should be in a direction opposite to that of Input Line For Start and End Lines
                    if (MathUtils.CompareVectors(UnitVectorAlongLine, newOrientation) == "Parallel")
                    {
                        ElementTransformUtils.RotateElement(m_Document, columnID, axis, Math.PI);
                    }
                }
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
                tx.Commit();
            }

        }

        private void RotateColumn(ElementId columnID, XYZ pt1, XYZ pt2, double dAngle)
        {
            XYZ point1 = new XYZ(pt1.X, pt1.Y, 0);
            XYZ point2 = new XYZ(pt1.X, pt1.Y, 1);
            Line axis = Line.CreateBound(point1, point2);

            using (Transaction tx = new Transaction(m_Document))
            {
                SupressWarningsInTransaction(tx);

                tx.Start("Change Orientation");

                ElementTransformUtils.RotateElement(m_Document, columnID, axis, dAngle);

                tx.Commit();
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

            Curve SlopeCurve = targetRoof.slopeLine;
            XYZ start = SlopeCurve.GetEndPoint(0);
            XYZ end = SlopeCurve.GetEndPoint(1);

            XYZ slope = start.Z > end.Z ? (end - start) : (start - end);

            SlopeDirect = new XYZ(slope.X, slope.Y, pt1.Z);

            return SlopeDirect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="lineType"></param>
        private XYZ AdjustLinePoint(XYZ pt1, XYZ pt2, LineType lineType, double dOffset)
        {
            XYZ AdjustedPoint = null;

            XYZ tempXVector = new XYZ(dOffset, 0, 0);
            XYZ tempYVector = new XYZ(0, dOffset, 0);

            XYZ tempXMinusVector = new XYZ(-dOffset, 0, 0);
            XYZ tempYMinusVector = new XYZ(0, -dOffset, 0);

            if (lineType == LineType.vertical)
            {
                if (pt1.Y < pt2.Y)
                    AdjustedPoint = pt1 + tempYVector;
                else
                    AdjustedPoint = pt1 + tempYMinusVector;
            }

            if (lineType == LineType.horizontal)
            {
                if (pt1.X < pt2.X)
                    AdjustedPoint = pt1 + tempXVector;
                else
                    AdjustedPoint = pt1 + tempXMinusVector;
            }
            return AdjustedPoint;
        }

        private  void ProcessT62InputLine(InputLine inputLine, IOrderedEnumerable<Level> levels)
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
                        filteredLevels.Add(filteredlevel);
                }


                for (int i = 0; i < filteredLevels.Count() - 1; i++)
                {
                    Level tempLevel = filteredLevels.ElementAt(i);

                    if ((pt2.Z < (tempLevel.Elevation + 1)) && (pt2.Z > (tempLevel.Elevation - 1)))
                    {

                        baseLevel = tempLevel;
                        toplevel = filteredLevels.ElementAt(i + 1);

                        if (i == 0)
                        {
                            inputLine.strT62Type = inputLine.strT62Type.ToString() + " (Flush Bottom / Female Top)";
                        }
                        else if (i > 0 && i < levels.Count() - 2)
                        {
                            inputLine.strT62Type = inputLine.strT62Type.ToString() + " (Male Bottom / Female Top)";
                        }
                        else
                        {
                            inputLine.strT62Type = inputLine.strT62Type.ToString() + " (Male Bottom / Flush Top)";
                        }

                        break;
                    }
                }

                FamilySymbol columnType = SymbolCollector.GetSymbol(inputLine.strT62Type, "T62");

                using (Transaction tx = new Transaction(m_Document))
                {
                    SupressWarningsInTransaction(tx);

                    tx.Start("Place Column");

                    foreach (XYZ studPoint in inputLine.gridIntersectionPoints)
                    {
                        FamilyInstance column = m_Document.Create.NewFamilyInstance(studPoint, columnType, baseLevel, StructuralType.Column);

                        m_Form.PostMessage(string.Format("Placing T62  {3} at {0} , {1} , {2} \n \n ", studPoint.X, studPoint.Y, studPoint.Z, inputLine.strT62Type));

                        column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                        column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);

                    }
                    tx.Commit();
                }
            }
            catch (Exception) { }
        }

        private  void ProcessT62AndStudLine(InputLine inputLine, IOrderedEnumerable<Level> levels)
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

            return (elements.Count > 0); 
        }

        private void ProcessDoubleStudAtGrids(InputLine inputLine, IOrderedEnumerable<Level> levels, FamilySymbol columnType, Element topAttachElement, Level toplevel, Level baseLevel)
        {

            double dFlangeWidth = FlangeWidth(inputLine.strStudType);

            try
            {
                for (int J = 0; J < 2; J++)
                {
                    XYZ pt1 = inputLine.locationCurve.Curve.GetEndPoint(0);
                    XYZ pt2 = inputLine.locationCurve.Curve.GetEndPoint(1);

                    LineType lineType = LineType.vertical;

                    if (MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y))
                        lineType = LineType.horizontal;

                    ElementId columnID = null;
                    

                    foreach (XYZ studPoint in inputLine.gridIntersectionPoints)
                    {
                        using (Transaction tx = new Transaction(m_Document))
                        {
                            SupressWarningsInTransaction(tx);

                            tx.Start("Place Column");


                            FamilyInstance column = m_Document.Create.NewFamilyInstance(studPoint, columnType, baseLevel, StructuralType.Column);

                            m_Form.PostMessage(string.Format("Placing T62  {3} at {0} , {1} , {2} \n \n ", studPoint.X, studPoint.Y, studPoint.Z, inputLine.strT62Type));

                            column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                            column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);

                            columnID = column.Id;

                            tx.Commit();

                            UpdateOrientation(columnID, column.FacingOrientation, studPoint, pt2, true);
                            XYZ Adjustedpt1 = AdjustLinePoint(studPoint, pt2, lineType, dFlangeWidth / 2);
                            MoveColumn(columnID, Adjustedpt1);

                            if (J != 0)
                                RotateColumn(columnID, studPoint, pt2, Math.PI); 
                        }
                    }


                }
            }
            catch(Exception ex) {
                TaskDialog.Show("Automation Error", string.Format("Cannot Place Double Studs at element {0}", inputLine.id));
            }
        }

        private void ProcessDoubleStudAtEnds(InputLine inputLine, XYZ pt1, XYZ pt2, FamilySymbol columnType, Element topAttachElement, Level toplevel, Level baseLevel)
        {
            ElementId startColumnID, EndColumnID;
            XYZ startColumnOrientation, endColumnOrientation;

            LineType lineType = LineType.vertical;

            if (MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y))
                lineType = LineType.horizontal;

            for (int i = 0; i < 2; i++)
            {

                // Start a new Tranasaction
                using (Transaction tx = new Transaction(m_Document))
                {

                    SupressWarningsInTransaction(tx);

                    tx.Start("Place Column");

                    CheckForExistingColumns(pt1);

                    //Place Column at start
                    FamilyInstance startcolumn = m_Document.Create.NewFamilyInstance(pt1, columnType, baseLevel, StructuralType.Column);
                    startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                    startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);

                    if (topAttachElement != null)
                        ColumnAttachment.AddColumnAttachment(m_Document, startcolumn, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Minimum, 0);

                    m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", pt1.X, pt1.Y, pt1.Z, inputLine.strStudType));
                    startColumnID = startcolumn.Id;
                    startColumnOrientation = startcolumn.FacingOrientation;

                    // Place column at end
                    FamilyInstance endColumn = m_Document.Create.NewFamilyInstance(pt2, columnType, baseLevel, StructuralType.Column);
                    endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                    endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);

                    if (topAttachElement != null)
                        ColumnAttachment.AddColumnAttachment(m_Document, endColumn, topAttachElement, 1, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Minimum, 0);

                    m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", pt2.X, pt2.Y, pt2.Z, inputLine.strStudType));
                    EndColumnID = endColumn.Id;
                    endColumnOrientation = endColumn.FacingOrientation;


                    XYZ referencePoint = null;
                    if (inputLine.mainGridIntersectionPoints?.Count > 0)
                        referencePoint = inputLine.mainGridIntersectionPoints[0];

                    tx.Commit();
                }

                double dFlangeWidth = FlangeWidth(inputLine.strStudType);

                UpdateOrientation(startColumnID, startColumnOrientation, pt1, pt2, true);
                XYZ Adjustedpt1 = AdjustLinePoint(pt1, pt2, lineType, dFlangeWidth / 2);
                MoveColumn(startColumnID, Adjustedpt1);

                UpdateOrientation(EndColumnID, endColumnOrientation, pt2, pt1, true);
                XYZ Adjustedpt2 = AdjustLinePoint(pt2, pt1, lineType, dFlangeWidth / 2);
                MoveColumn(EndColumnID, Adjustedpt2);


                if (i != 0)
                {
                    RotateColumn(startColumnID, pt1, pt2, Math.PI);
                    RotateColumn(EndColumnID, pt2, pt1, Math.PI);
                }
            }
        }
        private Element GetNearestFloor(Level level)
        {
            List<FloorObject> floorObjects = FloorHelper.colFloors;

            Element elemID = null;

            foreach (FloorObject floor in floorObjects)
            {
                
                Element levelElement = m_Document.GetElement(floor.levelID);
                Parameter elevationParam = levelElement.get_Parameter(BuiltInParameter.LEVEL_ELEV);
                if (elevationParam != null)
                {
                    if (MathUtils.IsWithInRange(elevationParam.AsDouble(), level.Elevation + 1 , level.Elevation - 1))
                    {
                        elemID = m_Document.GetElement(floor.elemID);
                        
                    } 
                }
            }

            return elemID;
        }

        private void MoveColumn(ElementId columnId, XYZ newLocation)
        {
            FamilyInstance column = m_Document.GetElement(columnId) as FamilyInstance;

            using (Transaction tx = new Transaction(m_Document))
            {
                SupressWarningsInTransaction(tx);

                tx.Start("Change Orientation");
                
                // Get the column's Location property
                Location location = column.Location;

                // Check if the column's location is a LocationPoint
                if (location is LocationPoint locationPoint)
                {
                    // Set the new location for the column
                    locationPoint.Point = newLocation;
                }

                tx.Commit();
            }
        }

        private double FlangeWidth(string strColumnName)
        {
            double width = 0;
            string token = "x";
            string[] result = strColumnName.Split(new string[] { token }, StringSplitOptions.None);

            if (result[1].Contains(" 1\""))
                return 0.083333;

            else if (result[1].Contains("1 1/2\""))
                return 0.125;

            else if (result[1].Contains(" 2\""))
                return 0.166666;
            
            else if (result[1].Contains("2 1/2\""))
                return 0.208333;
            
            else if (result[1].Contains(" 3\""))
                return 0.25;
            
            else if (result[1].Contains("3 1/2\""))
                return 0.291666;

            return width;
               
        }
    }
}

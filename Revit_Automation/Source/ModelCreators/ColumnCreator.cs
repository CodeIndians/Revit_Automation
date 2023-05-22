using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Revit_Automation.CustomTypes;
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
            foreach (InputLine inputLine in inputLinesCollection)
            {
                if (!string.IsNullOrEmpty(inputLine.strT62Guage) && !string.IsNullOrEmpty(inputLine.strStudGuage))
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

        private void ProcessStudInputLine(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
            try
            {
                XYZ pt1 = inputLine.locationCurve.Curve.GetEndPoint(0);
                XYZ pt2 = inputLine.locationCurve.Curve.GetEndPoint(1);

                ElementId startColumnID, EndColumnID, columnID;
                XYZ startColumnOrientation, endColumnOrientation, columnOrientation;

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

                inputLine.strStudType = inputLine.strStudType.ToString() + string.Format(" x {0}ga", inputLine.strStudGuage);

                FamilySymbol columnType = SymbolCollector.GetSymbol(inputLine.strStudType, "Post");

                using (Transaction tx = new Transaction(m_Document))
                {
                    tx.Start("Place Column");

                    // Place column at the start
                    if (CheckForExistingColumns(pt1))
                    {
                        AdjustLinePoint(pt1, pt2, lineType);
                    }

                    FamilyInstance startcolumn = m_Document.Create.NewFamilyInstance(pt1, columnType, baseLevel, StructuralType.Column);
                    startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                    startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
                    m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", pt1.X, pt1.Y, pt1.Z, inputLine.strStudType));
                    startColumnID = startcolumn.Id;
                    startColumnOrientation = startcolumn.FacingOrientation;

                    if (CheckForExistingColumns(pt1))
                    {
                        AdjustLinePoint(pt1, pt2, lineType);
                    }

                    // Place column at end
                    FamilyInstance endColumn = m_Document.Create.NewFamilyInstance(pt2, columnType, baseLevel, StructuralType.Column);
                    endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                    endColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
                    m_Form.PostMessage(string.Format("Placing Post  {3} at {0} , {1} , {2} \n \n ", pt2.X, pt2.Y, pt2.Z, inputLine.strStudType));
                    EndColumnID = endColumn.Id;
                    endColumnOrientation = endColumn.FacingOrientation;


                    XYZ referencePoint = null;
                    if (inputLine.mainGridIntersectionPoints != null)
                        referencePoint = inputLine.mainGridIntersectionPoints[0];

                    tx.Commit();
                }

                UpdateOrientation(startColumnID, startColumnOrientation, pt1, pt2);
                UpdateOrientation(EndColumnID, endColumnOrientation, pt2, pt1, true);

                /* XYZ IntersectionPoint = null;

                 if(inputLine.mainGridIntersectionPoints != null && inputLine.mainGridIntersectionPoints.Count > 0)
                     IntersectionPoint = inputLine.mainGridIntersectionPoints[0];
                 else
                     IntersectionPoint = computeInstersectionPoint(pt1, pt2, lineType);

                 XYZ tempXVector = new XYZ(2.5, 0, 0);
                 XYZ tempYVector = new XYZ(0, 2.5, 0);
                 XYZ tempXNegativeVector = new XYZ(-2.5, 0, 0);
                 XYZ tempYNegativeVector = new XYZ(0, -2.5, 0);

                 XYZ onCenterVector = null;

                 if (lineType == LineType.horizontal && IntersectionPoint.X < pt1.X && IntersectionPoint.X < pt2.X)
                     onCenterVector = tempXVector;
                 else if (lineType == LineType.horizontal && IntersectionPoint.X > pt1.X && IntersectionPoint.X > pt2.X)
                     onCenterVector = tempXNegativeVector;

                 if (lineType == LineType.vertical && IntersectionPoint.Y < pt1.Y && IntersectionPoint.Y < pt2.Y)
                     onCenterVector = tempYVector;
                 else if (lineType == LineType.vertical && IntersectionPoint.Y > pt1.Y && IntersectionPoint.Y > pt2.Y)
                     onCenterVector = tempYNegativeVector;*/

                XYZ studPoint = null, studEndPoint = null;

                // 4. Place columns in the Model
                if (lineType == LineType.vertical)
                {
                    studPoint = pt1.Y < pt2.Y ? pt1 : pt2;
                    studEndPoint = pt1.Y > pt2.Y ? pt1 : pt2;
                }
                else
                {
                    studPoint = pt1.X < pt2.X ? pt1 : pt2;
                    studEndPoint = pt1.X > pt2.X ? pt1 : pt2;
                }

                XYZ tempXVector = new XYZ(2.5, 0, 0);
                XYZ tempYVector = new XYZ(0, 2.5, 0);

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
                            tx.Start("Placing posts");
                            FamilyInstance studColumn = m_Document.Create.NewFamilyInstance(studPoint, columnType, baseLevel, StructuralType.Column);
                            studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                            studColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
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

        private void UpdateOrientation(ElementId columnID, XYZ ColumnOrientation,  XYZ pt1, XYZ pt2 , bool bRotate = false)
        {
            using (Transaction tx = new Transaction(m_Document))
            {
                tx.Start("Change Orientation");

                double dAngle = 0;

                XYZ point1 = new XYZ(pt1.X, pt1.Y, 0);
                XYZ point2 = new XYZ(pt1.X, pt1.Y, 1);
                Line axis = Line.CreateBound(point1, point2);

                XYZ LineOrientation = pt1.X > pt2.X ? (pt1 - pt2) : (pt2 - pt1);

                if ((ColumnOrientation.X == 0 && !MathUtils.ApproximatelyEqual(LineOrientation.X, 0)) || (ColumnOrientation.Y == 0 && LineOrientation.Y != 0))
                    dAngle = (Math.PI * 90) / 180;

                if (bRotate && dAngle != 0)
                    dAngle = -dAngle;
                
                if (dAngle == 0 && bRotate)
                    dAngle = Math.PI;
                
                ElementTransformUtils.RotateElement(m_Document, columnID, axis, dAngle);

                tx.Commit();
            }
        }

        private void AdjustLinePoint(XYZ pt1, XYZ pt2, LineType lineType)
        {
            XYZ tempXVector = new XYZ(0.4, 0, 0);
            XYZ tempYVector = new XYZ(0, 0.4, 0);
            XYZ tempXMinusVector = new XYZ(-0.4, 0, 0);
            XYZ tempYMinusVector = new XYZ(0, -0.4, 0);

            if (lineType == LineType.vertical && pt1.Y < pt2.Y)
                pt1 = pt1 + tempYVector;
            else
                pt1 = pt1 + tempYMinusVector;

            if (lineType == LineType.horizontal && pt1.X < pt2.X)
                pt1 = pt1 + tempXVector;
            else
                pt1 = pt1 + tempXMinusVector;
        }

        private  void ProcessT62InputLine(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
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
    }
}

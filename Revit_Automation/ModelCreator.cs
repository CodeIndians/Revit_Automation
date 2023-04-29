using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Revit_Automation.Command;
using System.Windows.Forms;

namespace Revit_Automation
{
    public class ModelCreator
    {
        static public void CreateModel(UIApplication uiapp, Form1 form)
        {
            //form.TopMost = true;
            form.Show();
            form.Refresh();

            Thread.Sleep(2000);

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // 0. Identify the main Grids in the model
            GridCollector gridCollection = new GridCollector(doc);

            // 1. Validate if the grids are equidistant
            if(gridCollection.Validate())
            {
                MessageBox.Show("Grid Validation Failed");
                return;
            }

            // 1. Find the levels in the project
            IOrderedEnumerable<Level> levels = FindAndSortLevels(doc);


            SymbolCollector.CollectColumns(doc);

            // 2. Get the symbols for T62 and Post 
            FamilySymbol T62columnType = null;
            FamilySymbol StudColumnType = null;
            FilteredElementCollector coll = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_StructuralColumns);

            foreach (FamilySymbol symbol in coll)
            {
                if (symbol.FamilyName == "T62")
                {
                    T62columnType = symbol;
                }

                if (symbol.FamilyName == "Post")
                {
                    StudColumnType = symbol;
                }
            }


            // 3. Retrieve elements from database
            FilteredElementCollector col
              = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_GenericModel);

            int i = 100 / col.Count();
            // Filtered element collector is iterable
            foreach (Element e in col)
            {
                form.UpdateProgress(i);

                LocationCurve locationCurve = (LocationCurve)e.Location;
                if (locationCurve != null)
                {
                    ParameterMap parameterMap = e.ParametersMap;
                    string strStudGauge = string.Empty, T62Gauge = string.Empty;

                    FamilySymbol columnType = StudColumnType;

                    if (parameterMap.Contains("Stud Gauge"))
                    {
                        Parameter StudGuageParameter = parameterMap.get_Item("Stud Gauge");
                        strStudGauge = StudGuageParameter.AsString();


                    }
                    if (parameterMap.Contains("T62 Gauge"))
                    {
                        Parameter T62TypeParameter = parameterMap.get_Item("T62 Gauge");
                        T62Gauge = T62TypeParameter.AsString();

                    }

                    XYZ pt1 = locationCurve.Curve.GetEndPoint(0);
                    XYZ pt2 = locationCurve.Curve.GetEndPoint(1);

                    // Check if we have a horizontal or vertical Input line
                    LineType lineType = LineType.Horizontal;

                    if (pt1.X == pt2.X)
                        lineType = LineType.vertical;
                    else
                        lineType = LineType.Horizontal;

                    // Determine the Column type;
                    if (!string.IsNullOrEmpty(strStudGauge))
                        columnType = StudColumnType;

                    else if (!string.IsNullOrEmpty(T62Gauge))
                        columnType = T62columnType;

                    else
                        continue;

                    // compute levels
                    Level toplevel = null, baseLevel = null;

                    if (pt2.Z > 10.0 && pt2.Z < 12)
                    {
                        toplevel = levels.ElementAt(levels.Count() - 3);
                        baseLevel = levels.ElementAt(levels.Count() - 4);
                        if (columnType == T62columnType)
                            columnType = SymbolCollector.T62FlushBottomFemaletop;
                    }
                    else if (pt2.Z > 20.0 && pt2.Z < 24)
                    {
                        toplevel = levels.ElementAt(levels.Count() - 3);
                        baseLevel = levels.ElementAt(levels.Count() - 2);
                        if (columnType == T62columnType)
                            columnType = SymbolCollector.T62FemaleTopMaleBottom;
                    }
                    else if (pt2.Z > 30.0 && pt2.Z < 34)
                    {
                        toplevel = levels.ElementAt(levels.Count() - 2);
                        baseLevel = levels.ElementAt(levels.Count() - 1);
                        if (columnType == T62columnType)
                            columnType = SymbolCollector.T62FlushTopMaleBottom;
                    }

                    // Place the column
                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Place Column");

                        if (columnType == StudColumnType)
                        {
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

                            bool bCanCreateColumn = true;
                            while (bCanCreateColumn)
                            {
                                FamilyInstance column = doc.Create.NewFamilyInstance(studPoint, columnType, baseLevel, StructuralType.Column);

                                form.PostMessage(string.Format("Placing POST 4\"x4\" 2 1/2\" at {0} , {1} , {2} \n \n ", pt1.X, pt1.Y, pt1.Z));

                                column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                                column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);

                                XYZ tempXVector = new XYZ(2.5, 0, 0);
                                XYZ tempYVector = new XYZ(0, 2.5, 0);

                                if (lineType == LineType.vertical)
                                    studPoint = studPoint + tempYVector;
                                else
                                    studPoint = studPoint + tempXVector;

                                if (lineType == LineType.vertical && studPoint.Y < (studEndPoint.Y - 1.0))
                                    bCanCreateColumn = true;
                                else if (lineType == LineType.Horizontal && studPoint.X < (studEndPoint.X - 1.0))
                                    bCanCreateColumn = true;
                                else
                                    bCanCreateColumn = false;

                            }
                        }

                        else
                        {
                            FamilyInstance Startcolumn = doc.Create.NewFamilyInstance(pt1, columnType, baseLevel, StructuralType.Column);

                            form.PostMessage(string.Format("Placing T62 at {0} , {1} , {2} \n \n", pt1.X, pt1.Y, pt1.Z));
                            Startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                            Startcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);

                            FamilyInstance Endcolumn = doc.Create.NewFamilyInstance(pt2, columnType, baseLevel, StructuralType.Column);

                            Endcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(toplevel.Id);
                            Endcolumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(0);
                        }

                        tx.Commit();
                    }

                }

                Debug.Print(e.Name);
            }

            // Collect all curve elements in the document
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .OfClass(typeof(CurveElement));

            // Filter the collector to only include the specified curve type
            FilteredElementCollector filteredElements = collector.WherePasses(new ElementClassFilter(typeof(Family
                )));

            // Get all lines in the document
            List<Line> allLines = filteredElements.Cast<Line>().ToList();

            foreach (Line line in allLines)
            {
                // Process the line as needed
                string lineType = line.GetType().Name;
                string lineId = line.Id.ToString();
                Debug.Print(lineType + " - " + lineId);
            }


            // Modify document within a transaction
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Name");
                tx.Commit();
            }

            FilteredElementCollector collector1 = new FilteredElementCollector(doc);
            collector1.OfClass(typeof(FamilyInstance));
            collector1.OfCategory(BuiltInCategory.OST_GenericModel);

            IList<Element> filteredElements1 = new List<Element>();
            foreach (Element e in collector1)
            {
                FamilyInstance fi = e as FamilyInstance;
                if (fi != null && fi.Symbol.Family.Name == "Line")
                {
                    filteredElements1.Add(fi);
                }
            }

            form.Visible = false;
            form.UpdateCompleted();
            form.ShowDialog();
        }
    }
}

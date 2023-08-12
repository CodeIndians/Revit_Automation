using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class PlaceCustomHatch
    {
        private Document mDocument;

        private Selection mSelection;

        private ElementId hatchId;

        public PlaceCustomHatch(ref Document doc, ref Selection selection, bool isExt)
        {
            mDocument = doc;
            mSelection = selection;
            hatchId = null;


            var filledRegion = new FilteredElementCollector(mDocument).OfClass(typeof(FilledRegionType));
            foreach (var region in filledRegion)
            {
                if (isExt)
                {
                    if (region.Name == "Obstinate Orange")
                    {
                        hatchId = region.Id;
                        break;
                    }
                }
                else
                {
                    if (region.Name == "Nuture Green")
                    {
                        hatchId = region.Id;
                        break;
                    }
                }
            }

            // cannot find the hatch type 
            // return while throwing a task dialog
            if(hatchId == null)
            {
                TaskDialog.Show("Error", "Hatch family is not detected");
                return;
            }

            // Place hatch
            PlaceHatch();
        }



        private void PlaceHatch()
        {
            // if the selection is present, it has to be a line and the count should be 2
            if (mSelection != null && mSelection.GetElementIds().Count == 2)
            {
                // Get the selected elements
                ICollection<ElementId> selectedIds = mSelection.GetElementIds();

                // Create a line filter
                ElementCategoryFilter modelCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_GenericModel);

                // Collect the selected elements
                FilteredElementCollector locationCurvedCol = new FilteredElementCollector(mDocument, selectedIds);

                // Apply the category filter to the collector
                _ = locationCurvedCol.WherePasses(modelCategoryFilter);

                if (locationCurvedCol.Count() != 2)
                {
                    TaskDialog.Show("Error", "Lines are not selected");
                    return;
                }

                InputLine firstLine = GetInputLineFromElement(locationCurvedCol.ElementAt(0));

                InputLine secondLine = GetInputLineFromElement(locationCurvedCol.ElementAt(1));

                PlaceHatchUsingLines(firstLine,secondLine);

            }
            else if(mSelection == null)
            {
                
            }
            else
            {
                TaskDialog.Show("Error", "Not a valid input");
            }
        }

        private void PlaceHatchUsingLines(InputLine firstLine, InputLine secondLine)
        {
            var firstLineType = InputLine.GetLineType(firstLine);
            var secondLineType = InputLine.GetLineType(secondLine);

            if (firstLineType != secondLineType)
            {
                TaskDialog.Show("Error", "The lines are not parallel");
                return;
            }
            //
            else
            {
                if( firstLineType == LineType.HORIZONTAL)
                {
                    if(PointUtils.AreAlmostEqual(firstLine.start.Y, secondLine.start.Y))
                    {
                        TaskDialog.Show("Error", "The lines are co linear");
                        return;
                            
                    }
                }
                else if ( firstLineType == LineType.VERTICAL)
                {
                    if (PointUtils.AreAlmostEqual(firstLine.start.X, secondLine.start.X))
                    {
                        TaskDialog.Show("Error", "The lines are co linear");
                        return;
                    }
                }
            }

            using (Transaction transaction = new Transaction(mDocument))
            {
                transaction.Start("Creating Custom Hatch");

                CurveLoop loop = new CurveLoop();

                InputLine newFirstLine = new InputLine();
                InputLine newSecondLine= new InputLine();

                if (firstLineType == LineType.HORIZONTAL)
                {
                    // use the shorter line 
                    if (LineUtils.GetLineLength(firstLine) < LineUtils.GetLineLength(secondLine))
                    {
                        newFirstLine = firstLine;
                        newSecondLine = new InputLine(new XYZ(firstLine.start.X, secondLine.start.Y, firstLine.start.Z),
                                                                new XYZ(firstLine.end.X, secondLine.end.Y, firstLine.end.Z));
                    }
                    else
                    {
                        newFirstLine = new InputLine(new XYZ(secondLine.start.X, firstLine.start.Y, secondLine.start.Z),
                                                                new XYZ(secondLine.end.X, firstLine.end.Y, secondLine.end.Z));
                        newSecondLine = secondLine;
                    }
                }

                else if (firstLineType == LineType.VERTICAL)
                {
                    // use the shorter line 
                    if (LineUtils.GetLineLength(firstLine) < LineUtils.GetLineLength(secondLine))
                    {
                        newFirstLine = firstLine;
                        newSecondLine = new InputLine(new XYZ(secondLine.start.X, firstLine.start.Y, firstLine.start.Z),
                                                                new XYZ(secondLine.end.X, firstLine.end.Y, firstLine.end.Z));
                    }
                    else
                    {
                        newFirstLine = new InputLine(new XYZ(firstLine.start.X, secondLine.start.Y, secondLine.start.Z),
                                                                new XYZ(firstLine.end.X, secondLine.end.Y, secondLine.end.Z));
                        newSecondLine = secondLine;
                    }
                }

                // Create the lines for the bounding loop
                Line line1 = Line.CreateBound(newFirstLine.start, newFirstLine.end);
                Line line2 = Line.CreateBound(newFirstLine.end, newSecondLine.end);

                Line line3 = Line.CreateBound(newSecondLine.end, newSecondLine.start);
                Line line4 = Line.CreateBound(newSecondLine.start, newFirstLine.start);

                // Add the lines to the bounding loop
                loop.Append(line1);
                loop.Append(line2);
                loop.Append(line3);
                loop.Append(line4);

                IList<CurveLoop> curveLoop = new List<CurveLoop>();
                curveLoop.Add(loop);

                FilledRegion hatchData = FilledRegion.Create(mDocument, hatchId, mDocument.ActiveView.Id, curveLoop);

                transaction.Commit();
            }
        }

        private InputLine GetInputLineFromElement(Element elem)
        {
            InputLine inputLine = new InputLine();

            // get the location curve from the element
            LocationCurve firstLocationCurve = elem.Location as LocationCurve;

            if (firstLocationCurve != null)
            {
                // capture the line from the location curve
                Line line = firstLocationCurve.Curve as Line;

                // construct an input line from the Line
                inputLine = new InputLine(line.GetEndPoint(0), line.GetEndPoint(1));
            }

            return inputLine;
        }


    }
}

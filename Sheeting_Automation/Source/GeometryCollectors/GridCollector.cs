using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Sheeting_Automation.Source.GeometryCollectors
{
    internal class GridCollector
    {
        private readonly Document mDocument;

        // Grid structure to collect start, end, name and gridref
        public struct GridLine
        {
            public XYZ start;
            public XYZ end;
            public string name;
            public Reference gridReference;

            public GridLine(XYZ startPoint, XYZ endPoint, string Name, Reference startReference)
            {
                start = startPoint;
                end = endPoint;
                name = Name;
                gridReference = startReference;
            }

            public void SortPoints()
            {
                double epsilon = 0.0001; // precision

                if (start.X > end.X + epsilon || (Math.Abs(start.X - end.X) < epsilon && start.Y > end.Y + epsilon))
                {
                    // Swap start and end points
                    XYZ temp = start;
                    start = end;
                    end = temp;
                }
            }
        }

        // collects all the hoprizontal grids
        public List<GridLine> HorizontalLines;

        // collects all tehe vertical grids
        public List<GridLine> VerticalLines;

        public GridCollector(ref  Document document)
        {
            mDocument = document;
            HorizontalLines = new List<GridLine>();
            VerticalLines = new List<GridLine>();
            Collect();
        }

        private void Collect()
        {
            FilteredElementCollector collector = new FilteredElementCollector(mDocument);
            IList<Element> gridElements = collector.OfCategory(BuiltInCategory.OST_Grids).ToElements();
            List<GridLine> allGrids = new List<GridLine>();

            // collect all the sorted grid points
            foreach (Element element in gridElements)
            {
                if (element is Grid grid)
                {

                    if (!grid.IsHidden(mDocument.ActiveView))
                    {
                        var gridLine = new GridLine(grid.Curve.GetEndPoint(0), grid.Curve.GetEndPoint(1), grid.Name, new Reference(grid));

                        gridLine.SortPoints();

                        allGrids.Add(gridLine);
                    }
                }
            }

            // Separate horizontal and vertical grid lines
            foreach (var gridLine in allGrids)
            {
                double epsilon = 0.0001;

                if (Math.Abs(gridLine.start.Y - gridLine.end.Y) < epsilon)
                {
                    HorizontalLines.Add(gridLine);
                }
                else if (Math.Abs(gridLine.start.X - gridLine.end.X) < epsilon)
                {
                    VerticalLines.Add(gridLine);
                }
            }

            // sort horizontal lines
            HorizontalLines.Sort((line1, line2) => line2.start.Y.CompareTo(line1.start.Y));

            // sort vertical lines
            VerticalLines.Sort((line1, line2) => line1.start.X.CompareTo(line2.start.X));

            // Write the horizontal and vertical grid lines to separate files
            //WriteGridListToFile(HorizontalLines, @"C:\temp\horizontal_grids.txt");
            //WriteGridListToFile(VerticalLines, @"C:\temp\vertical_grids.txt");
        }

        /// <summary>
        /// Writes the list to a file
        /// Only used for development purpose
        /// </summary>
        /// <param name="gridList"></param>
        /// <param name="filePath"></param>
        private void WriteGridListToFile(List<GridLine> gridList, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            // Iterate through each grid line
            foreach (var gridLine in gridList)
            {
                // Append the XYZ coordinates to the StringBuilder
                sb.AppendLine($" start = {gridLine.start} end= {gridLine.end} name= {gridLine.name} ");
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}

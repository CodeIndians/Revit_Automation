using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Sheeting_Automation.Source.GeometryCollectors.FloorGeometryCollector;

namespace Sheeting_Automation.Source.GeometryCollectors
{
    internal class GridCollector
    {
        private Document mDocument;

        public struct GridLine
        {
            public XYZ start;
            public XYZ end;
            public String name;

            public GridLine(XYZ startPoint, XYZ endPoint, String Name)
            {
                start = startPoint;
                end = endPoint;
                name = Name;
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
        public List<FloorExternalLine> HorizontalLines;

        // collects all tehe vertical grids
        public List<FloorExternalLine> VerticalLines;

        public GridCollector(ref  Document document)
        {
            mDocument = document;
            HorizontalLines = new List<FloorExternalLine>();
            VerticalLines = new List<FloorExternalLine>();
            Collect();
        }

        private void Collect()
        {
            FilteredElementCollector collector = new FilteredElementCollector(mDocument);
            IList<Element> gridElements = collector.OfCategory(BuiltInCategory.OST_Grids).ToElements();
            List<GridLine> allGrids = new List<GridLine>();

            foreach (Element element in gridElements)
            {
                if (element is Grid grid)
                {
                    var gridLine = new GridLine(grid.Curve.GetEndPoint(0), grid.Curve.GetEndPoint(1), grid.Name);

                    gridLine.SortPoints();

                    allGrids.Add(gridLine);
                }
            }

            //MessageBox.Show(allGrids.Count.ToString());
            WriteGridListToFile(allGrids, @"C:\temp\grids.txt");

        }

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

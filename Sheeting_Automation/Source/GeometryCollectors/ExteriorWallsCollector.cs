using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sheeting_Automation.Source.GeometryCollectors.GridCollector;
using static System.Windows.Forms.LinkLabel;

namespace Sheeting_Automation.Source.GeometryCollectors
{
    internal class ExteriorWallsCollector
    {
        private Document mDocument;

        public ExteriorWallsCollector(ref Document document)
        {
            mDocument = document;
            Collect();
        }

        private void Collect()
        {
            //List<ElementId> intersectingWallIds = new List<ElementId>();

            var floorCollector = new FloorGeometryCollector(ref mDocument);

            var externaFloorLines = floorCollector.FloorExternalLines;

            List<Element> walls = new List<Element>();

            // Get the active view
            View activeView = mDocument.ActiveView;

            // Get all the walls in the document
            FilteredElementCollector wallCollector = new FilteredElementCollector(mDocument,activeView.Id);
            wallCollector.OfClass(typeof(Wall));
            IList<Element> allwallS = wallCollector.ToElements();

            foreach (Element wall in allwallS)
            {
                walls.Add(wall);
            }

            WriteWallListToFile(walls, @"C:\temp\walls.txt");
        }

        private void WriteWallListToFile(List<Element> wallElements, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            // Iterate through each grid line
            foreach (var wallname in wallElements)
            {
                // Append the XYZ coordinates to the StringBuilder
                sb.AppendLine($" Name = {wallname.Name}, Id = {wallname.Id}");
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}

using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using static Sheeting_Automation.Source.GeometryCollectors.FloorGeometryCollector;
using System.Drawing;

namespace Sheeting_Automation.Source.GeometryCollectors
{
    internal class FloorGeometryCollector
    {
        private Document mDocument;

        public struct FloorExternalLine
        {
            public XYZ start;
            public XYZ end;

            public FloorExternalLine(XYZ startPoint, XYZ endPoint)
            {
                start = startPoint;
                end = endPoint;
            }
        }

        // collects all the external lines in the diagram
        private List<FloorExternalLine> mAllFloorExternalLines;

        // collected external lines are separated floor wise 
        public List<List<FloorExternalLine>> mFloorExternalLines;

        public FloorGeometryCollector(ref Document document)
        {
            mDocument = document;
            mAllFloorExternalLines = new List<FloorExternalLine>();
            mFloorExternalLines = new List<List<FloorExternalLine>>();
        }

        public void Collect()
        {
            FilteredElementCollector collector = new FilteredElementCollector(mDocument);
            IList<Element> floorElements = collector.OfClass(typeof(Floor)).ToElements();

            //uint count = 0;

            // Iterate through each floor element
            foreach (Element floorElement in floorElements)
            {
                Floor floor = floorElement as Floor;

                // get the floor sketch geometry
                Sketch sketch = mDocument.GetElement(floor.SketchId) as Sketch;

                // get all the curves from the sketch
                foreach (CurveArray curveArray in sketch.Profile)
                {
                    // add all the external lines
                    foreach (Curve curve in curveArray)
                    {
                        mAllFloorExternalLines.Add(new FloorExternalLine(curve.GetEndPoint(0),curve.GetEndPoint(1)));
                    }
                }
            }
            var groupedPoints = mAllFloorExternalLines.GroupBy(p => p.start.Z);

            // Iterate through each group and create a separate list for each Z coordinate
            foreach (var group in groupedPoints)
            {
                List<FloorExternalLine> separatedList = group.ToList();
                mFloorExternalLines.Add(separatedList);
            }

            // sort the floor lists by x and y 
            SortFloorListsByXY();

            //MessageBox.Show(mFloorLists.ToString());

            WriteFloorListsToFile(mFloorExternalLines, @"C:\temp\floor.txt");

        }

        public void SortFloorListsByXY()
        {

            foreach (var floorList in mFloorExternalLines)
            {
                // Sort the XYZ points within each floor list based on X position
                List<FloorExternalLine> sortedList = floorList.OrderBy(p => p.start.X).ThenBy(p => p.start.Y).ToList();

                // Replace the original floor list with the sorted list
                floorList.Clear();
                floorList.AddRange(sortedList);
            }
        }

        /// <summary>
        /// Used for Debugging the data
        /// Takes floorLists and file path
        /// And prints the data line by line into a file
        /// </summary>
        /// <param name="floorLists"></param>
        /// <param name="filePath"></param>
        private void WriteFloorListsToFile(List<List<FloorExternalLine>> floorLists, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            // Iterate through each floor list
            foreach (var floorList in floorLists)
            {
                // Iterate through each XYZ point in the floor list
                foreach (var points in floorList)
                {
                    // Append the XYZ coordinates to the StringBuilder
                    sb.AppendLine($" start = {points.start.X},{points.start.Y},{points.start.Z} , end ={points.end.X},{points.end.Y},{points.end.Z}  ");
                }
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());
        }

    }
}

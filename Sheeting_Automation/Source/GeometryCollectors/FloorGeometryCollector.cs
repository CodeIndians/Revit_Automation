using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using static Sheeting_Automation.Source.GeometryCollectors.FloorGeometryCollector;
using System.Drawing;
using static System.Windows.Forms.LinkLabel;
using System;

namespace Sheeting_Automation.Source.GeometryCollectors
{
    internal class FloorGeometryCollector
    {
        private readonly Document mDocument;

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
        public List<List<FloorExternalLine>> FloorExternalLines;

        public FloorGeometryCollector(ref Document document)
        {
            mDocument = document;
            mAllFloorExternalLines = new List<FloorExternalLine>();
            FloorExternalLines = new List<List<FloorExternalLine>>();
            Collect();
        }

        private void Collect()
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
                FloorExternalLines.Add(separatedList);
            }

            // sort the floor lines in a circular order
            SortFloorListsCircular();

            //MessageBox.Show(mFloorLists.ToString());

            // TODO: Should be required in the production version 
            WriteFloorListsToFile(FloorExternalLines, @"C:\temp\floor.txt");

        }

        /// <summary>
        /// sort the given list of lines based on the condition that the,
        /// end of each line is the start of the next line
        /// </summary>
        public void SortFloorListsCircular()
        {
            foreach (var floorList in FloorExternalLines)
            {
                // Sort the XYZ points within each floor list based on X position and then on Y position
                List<FloorExternalLine> sortedList = SortLineListCircular(floorList);

                // Replace the original floor list with the sorted list
                floorList.Clear();
                floorList.AddRange(sortedList);
            }
        }

        /// <summary>
        /// sort the given list of lines based on the condition that the,
        /// end of each line is the start of the next line
        /// </summary>
        /// <param name="lines"></param>
        /// <returns> circular sorted lines</returns>
        private List<FloorExternalLine> SortLineListCircular(List<FloorExternalLine> lines)
        {
            List<FloorExternalLine> sortedLines  = new List<FloorExternalLine>();

            FloorExternalLine startLine = lines[0];
            sortedLines.Add(startLine);
            lines.Remove(startLine);

            while (lines.Count > 0)
            {
                XYZ lastPoint = sortedLines[sortedLines.Count - 1].end;
                bool foundNextLine = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    FloorExternalLine line = lines[i];
                    if (line.start.IsAlmostEqualTo(lastPoint))
                    {
                        sortedLines.Add(line);
                        lines.RemoveAt(i);
                        foundNextLine = true;
                        break;
                    }
                }

                if (!foundNextLine)
                {
                    // If no next line is found, the list is not a closed loop
                    Console.WriteLine("Error: List is not a closed loop.");
                    break;
                }
            }

            return sortedLines;
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

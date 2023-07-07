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
        public  List<FloorExternalLine> FloorExternalLines;

        public FloorGeometryCollector(ref Document document)
        {
            mDocument = document;
            FloorExternalLines = new List<FloorExternalLine>();
            Collect();
        }

        private void Collect()
        {
            FilteredElementCollector collector = new FilteredElementCollector(mDocument,mDocument.ActiveView.Id);
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
                        FloorExternalLines.Add(new FloorExternalLine(curve.GetEndPoint(0),curve.GetEndPoint(1)));
                    }
                }
            }

            // sort the floor lines in a circular order
            SortFloorListsCircular();

            // TODO: Should be removed in the production version 
            WriteFloorListsToFile(FloorExternalLines, @"C:\temp\floor.txt");

        }

        /// <summary>
        /// sort the given list of lines based on the condition that the,
        /// end of each line is the start of the next line
        /// </summary>
        public void SortFloorListsCircular()
        {
            // Sort the XYZ points within each floor list based on X position and then on Y position
            List<FloorExternalLine> sortedList = SortLineListCircular(FloorExternalLines);

            // clear the unsorted list 
            FloorExternalLines.Clear();

            // assin the sorted list to the floor lines collection
            FloorExternalLines = sortedList;
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
        private void WriteFloorListsToFile(List<FloorExternalLine> floorLists, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            // Iterate through each floor list
            foreach (var line in floorLists)
            {
                    // Append the XYZ coordinates to the StringBuilder
                    sb.AppendLine($" start = {line.start.X},{line.start.Y},{line.start.Z} , end ={line.end.X},{line.end.Y},{line.end.Z}  ");
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());
        }

    }
}

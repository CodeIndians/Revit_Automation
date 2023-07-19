using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static Revit_Automation.Source.Hallway.ExternalLine;

namespace Revit_Automation.Source.Hallway
{
    internal class LineCollector
    {
        private readonly Document mDocument;

        // all the input lines
        private List<InputLine> InputLines;

        // External lines, which has a list of intersecting internal lines 
        private List<ExternalLine>  ExternalLines;
       
        public LineCollector(ref Document doc, ref List<InputLine> inputLines, ref List<ExternalLine> externalLines)
        {
            mDocument = doc;
            InputLines = inputLines;
            ExternalLines = externalLines;
            Collect();
        }

        private void Collect()
        {
            var lineCollector = new FilteredElementCollector(mDocument, mDocument.ActiveView.Id)
                                        .WhereElementIsNotElementType()
                                        .OfCategory(BuiltInCategory.OST_GenericModel);

            // collect lines into internal and external lines 
            foreach (Element elem in lineCollector)
            {
                LocationCurve locationCurve = elem.Location as LocationCurve;
                if (elem.IsHidden(mDocument.ActiveView))
                {
                    continue;
                }
                if (locationCurve != null)
                {
                    Line line = locationCurve.Curve as Line;
                    elem.LookupParameter("Wall Type").ToString().Contains("Ex");
                    if (line != null)
                    {
                        var wallType = elem.LookupParameter("Wall Type").AsString().Contains("Ex");
                        if (wallType)
                            ExternalLines.Add(new ExternalLine(new InputLine(line.GetEndPoint(0), line.GetEndPoint(1))));
                        else
                            InputLines.Add(new InputLine(line.GetEndPoint(0), line.GetEndPoint(1)));
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            // sort the internal and external lines 
            InputLines.Sort();
            ExternalLines.Sort();

            //Joins the external lines that are falling on the same line
            JoinExternalLines();

            //collect the intersecting input lines on each of the external line
            CollectExternalIntersections();

            // Write the data to files. Used for debugging
            // Comment these in the production version
            //FileWriter.WriteInputListToFile(InputLines, @"C:\temp\input_lines");
            
        }

        private void CollectExternalIntersections()
        {
            double epsilon = 0.016;
            for (int i = 0; i < ExternalLines.Count; i++)
            {
                if (InputLine.GetLineType(ExternalLines[i].mainExternalLine) == LineType.VERTICAL)
                {
                    // collect intersecting vertical lines to the external line array
                    foreach (var inputLine in InputLines)
                    {                                                                                                   
                        if (InputLine.GetLineType(inputLine) == LineType.HORIZONTAL)
                        {
                            if (Math.Abs(inputLine.start.X - ExternalLines[i].mainExternalLine.start.X) < epsilon || Math.Abs(inputLine.end.X - ExternalLines[i].mainExternalLine.start.X) < epsilon)
                            {
                                if((ExternalLines[i].mainExternalLine.start.Y < inputLine.start.Y) && (ExternalLines[i].mainExternalLine.end.Y > inputLine.start.Y) )
                                    ExternalLines[i].intersectingInternalInputLines.Add(inputLine);
                            }
                        }
                    }

                    // Add extra lines at the start and end if they are not present
                    if (ExternalLines[i].intersectingInternalInputLines.Count > 0)
                    {
                        var firstLine = ExternalLines[i].intersectingInternalInputLines[0];
                        var lastLine = ExternalLines[i].intersectingInternalInputLines[ExternalLines[i].intersectingInternalInputLines.Count - 1];

                        // insert a new line at the beginnning of the intersecting lines list
                        if ((firstLine.start.Y - ExternalLines[i].mainExternalLine.start.Y) > 1)
                        {
                            XYZ newStart = new XYZ(firstLine.start.X, ExternalLines[i].mainExternalLine.start.Y, firstLine.start.Z);
                            XYZ newEnd = new XYZ(firstLine.end.X, ExternalLines[i].mainExternalLine.start.Y, firstLine.end.Z);
                            InputLine newFirstLine = new InputLine(newStart, newEnd);
                            ExternalLines[i].intersectingInternalInputLines.Insert(0, newFirstLine);
                        }

                        //insert a new line at the end of the intersecting lines list
                        if ((ExternalLines[i].mainExternalLine.end.Y - lastLine.start.Y) > 1)
                        {
                            XYZ newStart = new XYZ(lastLine.start.X, ExternalLines[i].mainExternalLine.end.Y, lastLine.start.Z);
                            XYZ newEnd = new XYZ(lastLine.end.X, ExternalLines[i].mainExternalLine.end.Y, lastLine.end.Z);
                            InputLine newLastLine = new InputLine(newStart, newEnd);
                            ExternalLines[i].intersectingInternalInputLines.Add(newLastLine);
                        }
                    }

                }
                else if (InputLine.GetLineType(ExternalLines[i].mainExternalLine) == LineType.HORIZONTAL)
                {
                    // collect intersecting horizontal lines to the external line array
                    foreach (var inputLine in InputLines)
                    {
                        if (InputLine.GetLineType(inputLine) == LineType.VERTICAL)
                        {
                            if (Math.Abs(inputLine.start.Y - ExternalLines[i].mainExternalLine.start.Y) < epsilon || Math.Abs(inputLine.end.Y - ExternalLines[i].mainExternalLine.start.Y) < epsilon)
                            {
                                if ((ExternalLines[i].mainExternalLine.start.X < inputLine.start.X) && (ExternalLines[i].mainExternalLine.end.X > inputLine.start.X))
                                    ExternalLines[i].intersectingInternalInputLines.Add(inputLine);
                            }
                        }
                    }
                    // Add extra lines at the start and end if they are not present
                    if (ExternalLines[i].intersectingInternalInputLines.Count > 0)
                    {
                        var firstLine = ExternalLines[i].intersectingInternalInputLines[0];
                        var lastLine = ExternalLines[i].intersectingInternalInputLines[ExternalLines[i].intersectingInternalInputLines.Count - 1];

                        // insert a new line at the beginnning of the intersecting lines list
                        if ((firstLine.start.X - ExternalLines[i].mainExternalLine.start.X) > 1)
                        {
                            XYZ newStart = new XYZ(ExternalLines[i].mainExternalLine.start.X, firstLine.start.Y, firstLine.start.Z);
                            XYZ newEnd = new XYZ(ExternalLines[i].mainExternalLine.start.X, firstLine.end.Y, firstLine.end.Z);
                            InputLine newFirstLine = new InputLine(newStart, newEnd);
                            ExternalLines[i].intersectingInternalInputLines.Insert(0, newFirstLine);
                        }

                        //insert a new line at the end of the intersecting lines list
                        if ((ExternalLines[i].mainExternalLine.end.X - lastLine.start.X) > 1)
                        {
                            XYZ newStart = new XYZ(ExternalLines[i].mainExternalLine.end.X, lastLine.start.Y, lastLine.start.Z);
                            XYZ newEnd = new XYZ(ExternalLines[i].mainExternalLine.end.X, lastLine.end.Y, lastLine.end.Z);
                            InputLine newLastLine = new InputLine(newStart, newEnd);
                            ExternalLines[i].intersectingInternalInputLines.Add(newLastLine);
                        }
                    }
                }
            }
        }
        
        private void JoinExternalLines()
        {
            List<InputLine> horExtLines = new List<InputLine>();
            List<InputLine> verExtLines = new List<InputLine>();

            // separate main external lines into Horizontal and Vertical lines
            if (ExternalLines.Count > 0)
            {
                foreach(var extLine in ExternalLines)
                {
                    // get the line type
                    var lineType = InputLine.GetLineType(extLine.mainExternalLine);

                    if (lineType == LineType.HORIZONTAL)
                        horExtLines.Add(extLine.mainExternalLine);
                    else if (lineType == LineType.VERTICAL)
                        verExtLines.Add(extLine.mainExternalLine);
                }

                //FileWriter.WriteInputListToFile(horExtLines, @"C:\temp\hor_ext_lines");
                //FileWriter.WriteInputListToFile(verExtLines, @"C:\temp\ver_ext_lines");

                var joinedHorExtLines = JoinHorizontalLines(horExtLines);
                var joinedVerExtLines = JoinVerticalLines(verExtLines);

                ExternalLines.Clear();
                foreach(var extHorLine in joinedHorExtLines) 
                {
                    ExternalLines.Add(new ExternalLine(extHorLine));
                }

                foreach(var extVerLine in joinedVerExtLines)
                {
                    ExternalLines.Add(new ExternalLine(extVerLine));
                }

                ExternalLines.Sort();

                //FileWriter.WriteInputListToFile(ExternalLines, @"C:\temp\joined_ext_lines");
            }
            else
            {
                TaskDialog.Show("Error","No External Lines detected");
            }
        }

        private List<InputLine> JoinHorizontalLines(List<InputLine> horLines)
        {
            var joinedList = new List<InputLine>();

            while(horLines.Count > 0)
            {
                SortedSet<int> indexesToDelete = new SortedSet<int>();
                indexesToDelete.Add(0);

                var firstLine = horLines[0];

                var tempList = new List<InputLine>();
                tempList.Add(firstLine);

                for (int i = 1; i < horLines.Count; i++)
                {
                    var secondLine = horLines[i];
                    if (Math.Abs(firstLine.start.Y - secondLine.start.Y) < 0.016)
                    {
                        tempList.Add(secondLine);
                        indexesToDelete.Add(i);
                    }
                }

                foreach(var index in indexesToDelete.Reverse())
                    horLines.RemoveAt(index);

                joinedList.Add(JoinHorizontalParallelLine(tempList));
            }

            return joinedList;
        }

        private InputLine JoinHorizontalParallelLine(List<InputLine> horParLines)
        {
            var line = horParLines[0];
            if (horParLines.Count == 1)
                return line;
            else if(horParLines.Count > 1)
            {
                var minX = horParLines[0].start.X;
                var maxX = horParLines[0].end.X;
                // calculate minX and maxX
                foreach(var horParLine in horParLines)
                {
                    minX = Math.Min(minX, horParLine.start.X);
                    maxX = Math.Max(maxX, horParLine.end.X);
                }

                var Y = horParLines[0].start.Y;
                var Z = horParLines[0].start.Z;

                line = new InputLine(new XYZ(minX, Y, Z), new XYZ(maxX,Y,Z));
            }
            return line;
        }

        private List<InputLine> JoinVerticalLines(List<InputLine> verLines)
        {
            var joinedList = new List<InputLine>();

            while (verLines.Count > 0)
            {
                SortedSet<int> indexesToDelete = new SortedSet<int>();

                indexesToDelete.Add(0);

                var firstLine = verLines[0];

                var tempList = new List<InputLine>();
                tempList.Add(firstLine);

                for (int i = 1; i < verLines.Count; i++)
                {
                    var secondLine = verLines[i];
                    if (Math.Abs(firstLine.start.X - secondLine.start.X) < 0.016)
                    {
                        tempList.Add(secondLine);
                        indexesToDelete.Add(i);
                    }
                }

                foreach (var index in indexesToDelete.Reverse())
                    verLines.RemoveAt(index);

                joinedList.Add(JoinVerticallParallelLine(tempList));
            }

            return joinedList;
        }

        private InputLine JoinVerticallParallelLine(List<InputLine> verParLines)
        {
            var line = verParLines[0];
            if (verParLines.Count == 1)
                return line;
            else if (verParLines.Count > 1)
            {
                var minY = verParLines[0].start.Y;
                var maxY = verParLines[0].end.Y;
                // calculate minX and maxX
                foreach (var verParLine in verParLines)
                {
                    minY = Math.Min(minY, verParLine.start.Y);
                    maxY = Math.Max(maxY, verParLine.end.Y);
                }

                var X = verParLines[0].start.X;
                var Z = verParLines[0].start.Z;

                line = new InputLine(new XYZ(X, minY, Z), new XYZ(X, maxY, Z));
            }
            return line;
        }
    }
}

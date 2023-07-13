using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            //collect the intersecting input lines on each of the external line
            CollectExternalIntersections();

            // Write the data to files. Used for debugging
            // Comment these in the production version
            FileWriter.WriteInputListToFile(InputLines, @"C:\temp\input_lines");
            FileWriter.WriteInputListToFile(ExternalLines, @"C:\temp\extinput_lines");
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
    }
}

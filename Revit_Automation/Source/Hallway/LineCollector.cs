using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace Revit_Automation.Source.Hallway
{
    internal class LineCollector
    {
        enum LineType
        {
            HORIZONTAL,
            VERTICAL,
            INVALID
        }
        private readonly Document mDocument;

        public List<InputLine> InputLines;

        public List<ExternalLine>  ExternalLines;
        
        public struct ExternalLine : IComparable<ExternalLine>
        {
            public InputLine mainExternalLine;
            public List<InputLine> intersectingInternalInputLines;

            public ExternalLine(InputLine line)
            {
                mainExternalLine = line;
                intersectingInternalInputLines = new List<InputLine>();
            }

            int IComparable<ExternalLine>.CompareTo(ExternalLine other)
            {
                double epsilon = 0.016; // precision

                double xDiff = mainExternalLine.start.X - other.mainExternalLine.start.X;
                if (Math.Abs(xDiff) > epsilon)
                {
                    return (xDiff < 0) ? -1 : 1;
                }

                double yDiff = mainExternalLine.start.Y - other.mainExternalLine.start.Y;
                if (Math.Abs(yDiff) > epsilon)
                {
                    return (yDiff < 0) ? -1 : 1;
                }

                return 0;
            }
        }

        public struct InputLine : IComparable<InputLine>
        {
            public XYZ start;
            public XYZ end;

            public InputLine(XYZ startPoint, XYZ endPoint)
            {
                start = startPoint;
                end = endPoint;
                SortPoints();
            }
        

            /// <summary>
            /// Sort based on X first and then Y
            /// </summary>
            public void SortPoints()
            {
                double epsilon = 0.016; // precision

                if (start.X > end.X + epsilon || (Math.Abs(start.X - end.X) < epsilon && start.Y > end.Y + epsilon))
                {
                    // Swap start and end points
                    XYZ temp = start;
                    start = end;
                    end = temp;
                }
            }

            int IComparable<InputLine>.CompareTo(InputLine other)
            {
                double epsilon = 0.016; // precision

                double xDiff = start.X - other.start.X;
                if (Math.Abs(xDiff) > epsilon)
                {
                    return (xDiff < 0) ? -1 : 1;
                }

                double yDiff = start.Y - other.start.Y;
                if (Math.Abs(yDiff) > epsilon)
                {
                    return (yDiff < 0) ? -1 : 1;
                }

                return 0;
            }
        }

        public LineCollector(ref Document doc)
        {
            mDocument = doc;
            InputLines = new List<InputLine>();
            ExternalLines = new List<ExternalLine>();
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

            // collect the intersecting input lines on each of the external line
            CollectExternalIntersections();

            // Place the External Hatches
            PlaceExternalHatches();

            //Console.WriteLine(InputLines.Count);
            WriteInputListToFile(InputLines, @"C:\temp\input_lines");
            WriteInputListToFile(ExternalLines, @"C:\temp\extinput_lines");
        }

        private void CollectExternalIntersections()
        {
            double epsilon = 0.016;
            for (int i = 0; i < ExternalLines.Count; i++)
            {
                if (GetLineType(ExternalLines[i].mainExternalLine) == LineType.VERTICAL)
                {
                    // collect intersecting vertical lines to the external line array
                    foreach (var inputLine in InputLines)
                    {                                                                                                   
                        if (GetLineType(inputLine) == LineType.HORIZONTAL)
                        {
                            if (Math.Abs(inputLine.start.X - ExternalLines[i].mainExternalLine.start.X) < epsilon || Math.Abs(inputLine.end.X - ExternalLines[i].mainExternalLine.start.X) < epsilon)
                            {
                                if((ExternalLines[i].mainExternalLine.start.Y < inputLine.start.Y) && (ExternalLines[i].mainExternalLine.end.Y > inputLine.start.Y) )
                                    ExternalLines[i].intersectingInternalInputLines.Add(inputLine);
                            }
                        }
                    }

                    // Add extra lines at the start and end if they are not present
                    if(ExternalLines[i].intersectingInternalInputLines.Count > 0)
                    {
                        var firstLine = ExternalLines[i].intersectingInternalInputLines[0];
                        var lastLine = ExternalLines[i].intersectingInternalInputLines[ExternalLines[i].intersectingInternalInputLines.Count - 1];

                        // insert a new line at the beginnning of the intersecting lines list
                        if((firstLine.start.Y - ExternalLines[i].mainExternalLine.start.Y) > 1)
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
                else if (GetLineType(ExternalLines[i].mainExternalLine) == LineType.HORIZONTAL)
                {
                    // collect intersecting horizontal lines to the external line array
                    foreach (var inputLine in InputLines)
                    {
                        if (GetLineType(inputLine) == LineType.VERTICAL)
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

        private LineType GetLineType(InputLine inputLine)
        {
            double epsilon = 0.016;
            if (Math.Abs(inputLine.start.X - inputLine.end.X) < epsilon)
                return LineType.VERTICAL;
            else if (Math.Abs(inputLine.start.Y - inputLine.end.Y) < epsilon)
                return LineType.HORIZONTAL;
            else
                return LineType.INVALID;
        }

        private void PlaceExternalHatches()
        {
            var filledRegion = new FilteredElementCollector(mDocument).OfClass(typeof(FilledRegionType));
            var typeId = filledRegion.First().Id;

            using (Transaction transaction = new Transaction(mDocument))
            {
                transaction.Start("Creating Hatch");
                foreach(var externalLine in ExternalLines)
                {
                    for( int i = 0; i < externalLine.intersectingInternalInputLines.Count - 1; i++)
                    {
                        CurveLoop loop = new CurveLoop();

                        var firstLine = externalLine.intersectingInternalInputLines[i];
                        var secondLine = externalLine.intersectingInternalInputLines[i + 1];

                        

                        if (GetLineType(firstLine) == LineType.HORIZONTAL && GetLineType(secondLine) == LineType.HORIZONTAL)
                        {
                            var firstLineLength = Math.Abs(firstLine.start.X - firstLine.end.X);
                            var secondLineLength = Math.Abs(secondLine.start.X - secondLine.end.X);

                            // first line is longer
                            if (firstLineLength - secondLineLength> 0.016)
                            {
                                // form a new first line
                                XYZ newStart = new XYZ(secondLine.start.X, firstLine.start.Y, firstLine.start.Z);
                                XYZ newEnd = new XYZ(secondLine.end.X, firstLine.end.Y, firstLine.end.Z);
                                firstLine = new InputLine(newStart, newEnd);
                            }
                            // second line is longer
                            else if (secondLineLength - firstLineLength > 0.016)
                            {
                                //for a new second line
                                XYZ newStart = new XYZ(firstLine.start.X, secondLine.start.Y, secondLine.start.Z);
                                XYZ newEnd = new XYZ(firstLine.start.X, secondLine.end.Y, secondLine.end.Z);
                                secondLine = new InputLine(newStart, newEnd);
                            }
                        }
                        else if (GetLineType(firstLine) == LineType.VERTICAL && GetLineType(secondLine) == LineType.VERTICAL)
                        {
                            var firstLineLength = Math.Abs(firstLine.start.Y - firstLine.end.Y);
                            var secondLineLength = Math.Abs(secondLine.start.Y - secondLine.end.Y);

                            if (firstLineLength - secondLineLength > 0.016)
                            {
                                // form a new first line
                                XYZ newStart = new XYZ(firstLine.start.X, secondLine.start.Y, firstLine.start.Z);
                                XYZ newEnd = new XYZ(firstLine.end.X, secondLine.end.Y, firstLine.end.Z);
                                firstLine = new InputLine(newStart, newEnd);
                            }
                            // second line is longer
                            else if (secondLineLength - firstLineLength > 0.016)
                            {
                                //for a new second line
                                XYZ newStart = new XYZ(secondLine.start.X, firstLine.start.Y, secondLine.start.Z);
                                XYZ newEnd = new XYZ(secondLine.start.X, firstLine.end.Y, secondLine.end.Z);
                                secondLine = new InputLine(newStart, newEnd);
                            }
                        }

                            // Create the lines for the bounding loop
                        Line line1 = Line.CreateBound(firstLine.start, firstLine.end);
                        Line line2 = Line.CreateBound(firstLine.end, secondLine.end);
                        Line line3 = Line.CreateBound(secondLine.end, secondLine.start);
                        Line line4 = Line.CreateBound(secondLine.start, firstLine.start);

                        

                        // Add the lines to the bounding loop
                        loop.Append(line1);
                        loop.Append(line2);
                        loop.Append(line3);
                        loop.Append(line4);

                        IList<CurveLoop> curveLoop = new List<CurveLoop>();
                        curveLoop.Add(loop);

                        FilledRegion hatchData = FilledRegion.Create(mDocument,typeId, mDocument.ActiveView.Id, curveLoop);
                    }
                }
                transaction.Commit();
            }
        }
        private void WriteInputListToFile(List<InputLine> inputList, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            // Iterate through each grid line
            foreach (var inputLine in inputList)
            {
                // Append the XYZ coordinates to the StringBuilder
                sb.AppendLine($" start = {inputLine.start} end= {inputLine.end}");
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());
        }

        private void WriteInputListToFile(List<ExternalLine> externalInputLines, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            // Iterate through each grid line
            foreach (var externalLine in externalInputLines)
            {
                // Append the XYZ coordinates to the StringBuilder
                sb.AppendLine($" start = {externalLine.mainExternalLine.start} end= {externalLine.mainExternalLine.end}");

                foreach (var inputLine in externalLine.intersectingInternalInputLines)
                {
                    sb.AppendLine($"\t start= {inputLine.start} end = {inputLine.end}");
                }
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());
        }

        
    }
}

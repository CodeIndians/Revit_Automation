using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Revit_Automation.Source.Hallway
{
    internal class LineCollector
    {
        //enum for idenfiying the line direction
        enum LineType
        {
            HORIZONTAL,
            VERTICAL,
            INVALID
        }

        private readonly Document mDocument;

        // all the input lines
        public List<InputLine> InputLines;

        // External lines, which has a list of intersecting internal lines 
        public List<ExternalLine>  ExternalLines;

        // lines which are not touching the external lines
        public List<InputLine> InternalInputLines;

        public List<List<InputLine>> IntersectingInternalLines;
        
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

        public struct InputLine : IComparable<InputLine>,IEquatable<InputLine>
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

            bool IEquatable<InputLine>.Equals(InputLine other)
            {
                double epsilon = 0.016; // precision

                return Math.Abs(start.X - other.start.X) < epsilon &&
              Math.Abs(start.Y - other.start.Y) < epsilon &&
              Math.Abs(end.X - other.end.X) < epsilon &&
              Math.Abs(end.Y - other.end.Y) < epsilon;
            }

            public bool AreLinesIntersecting(InputLine other)
            {

                if (AreEqual(this.start, other.start) || AreEqual(this.start, other.end) || AreEqual(this.end, other.start) || AreEqual(this.end, other.end))
                    return true;

                // perpendicular lines
                if (GetLineType(this) == LineType.HORIZONTAL && GetLineType(other) == LineType.VERTICAL)
                {
                    return AreIntersecting(this, other);
                }
                else if (GetLineType(this) == LineType.VERTICAL && GetLineType(other) == LineType.HORIZONTAL)
                {
                    return AreIntersecting(other, this);
                }

                return false;
            }

            private bool AreIntersecting(InputLine horizontalLine, InputLine verticalLine)
            {
                double precision = 0.016;

                // horizontal line is falling between the vertical Y positions 
                var horizontaY = horizontalLine.start.Y;
                if ((horizontaY > verticalLine.start.Y && horizontaY < verticalLine.end.Y) || Math.Abs(horizontaY - verticalLine.start.Y) < precision || Math.Abs(horizontaY - verticalLine.end.Y) < precision)
                {
                    // check if the vertical line is falling between or touching the line 
                    var verticalX = verticalLine.start.X;
                    if ((verticalX > horizontalLine.start.X && verticalX < horizontalLine.end.X) || Math.Abs(verticalX - horizontalLine.start.X) < precision || Math.Abs(verticalX - horizontalLine.end.X) < precision)
                    {
                        return true;
                    }

                }
                return false;
            }

            private bool AreEqual(XYZ first, XYZ second)
            {
                double epsilon = 0.016; // precision

                return Math.Abs(first.X - second.X) < epsilon &&
              Math.Abs(first.Y - second.Y) < epsilon;
            }
        }

        public LineCollector(ref Document doc)
        {
            mDocument = doc;
            InputLines = new List<InputLine>();
            ExternalLines = new List<ExternalLine>();
            InternalInputLines = new List<InputLine>();
            IntersectingInternalLines = new List<List<InputLine>>();
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

            SeparateInternalInputLines();

            GroupIntersectingInternalLines();

            // Place the External Hatches
            PlaceExternalHatches();

            //Console.WriteLine(InputLines.Count);
            WriteInputListToFile(InputLines, @"C:\temp\input_lines");
            WriteInputListToFile(ExternalLines, @"C:\temp\extinput_lines");
            WriteInputListToFile(InternalInputLines, @"C:\temp\internal_input_lines");
            WriteInputListToFile(IntersectingInternalLines, @"C:\temp\intersect_group");
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

        /// <summary>
        /// Separate all the internal input lines
        /// This will exclude the external lines and also the lines intersecting with them
        /// </summary>
        private void SeparateInternalInputLines()
        {
            // iterate all the input lines 
            foreach(var inputLine in InputLines)
            {
                bool found = false;
                // iterate through all the collected external lines
                foreach(var externalLine in ExternalLines)
                {
                    // do not add if the input line is equal to the main external line 
                    if (inputLine.Equals(externalLine.mainExternalLine))
                    {
                        found = true;
                        break;
                    }
                    // do not add if the input line is any of the external intersecting lines
                    foreach (var intersectingInputLine in externalLine.intersectingInternalInputLines)
                    {
                        if (inputLine.Equals(intersectingInputLine))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
                // add only if the line not an external line or any of the internal lines that are intersecting with the external lines
                if (!found)
                    InternalInputLines.Add(inputLine);
            }
        }

        private void GroupIntersectingInternalLines()
        {
            //copy the internal input lines to a separate list
            List<InputLine> inputLines = new List<InputLine>(InternalInputLines);

            // return if these are no internal input lines 
            if (inputLines.Count <= 0)
                return;


            while ( inputLines.Count > 0)
            {
                // int firstIndex = -1; // we will always be comparing the first element
                int interSectingIndex = -1; // this will remain -1 is there is not intersection

                var firstLine = inputLines[0]; //capture the first input line

                // check if it already intersects with any line of the list 
                for (var j = 0; j < IntersectingInternalLines.Count; j++)
                {
                    for( var k = 0; k < IntersectingInternalLines[j].Count; k++)
                    {
                        if (firstLine.AreLinesIntersecting(IntersectingInternalLines[j][k]))
                        {
                            interSectingIndex = j;
                            break;
                        }
                    }
                }

                // this means that the line is intersecting with one of the already collected list of lists
                if(interSectingIndex != -1)
                {
                    IntersectingInternalLines[interSectingIndex].Add(firstLine); // add this to the list input lines on which this was intersecting

                    inputLines.RemoveAt(0); // remove the first line and
                    continue;               //skip to the next iteration
                }

                // iterate through all the available input lines skipping the first one 
                for (var i = 1; i < inputLines.Count; i++)
                {
                    if (firstLine.AreLinesIntersecting(inputLines[i]))
                    {
                        interSectingIndex = i; // set the intersecting index  
                        break;
                    }
                }

                // If any intersection pair is found, move it to a new list
                if(interSectingIndex !=  -1)
                {
                    int secondIntersectingIndex = -1;
                    var secondLine = inputLines[interSectingIndex];

                    // check if it already intersects with any line of the list 
                    for (var j = 0; j < IntersectingInternalLines.Count; j++)
                    {
                        for (var k = 0; k < IntersectingInternalLines[j].Count; k++)
                        {
                            if (secondLine.AreLinesIntersecting(IntersectingInternalLines[j][k]))
                            {
                                secondIntersectingIndex = j;
                                break;
                            }
                        }
                    }

                    if (secondIntersectingIndex != -1)
                    {
                        IntersectingInternalLines[secondIntersectingIndex].Add(firstLine);
                        IntersectingInternalLines[secondIntersectingIndex].Add(secondLine);
                    }
                    else
                    {
                        // add the pair of first and the intersecting line to the Intersecting list of lists 
                        IntersectingInternalLines.Add(new List<InputLine> { firstLine, secondLine });
                    }

                    inputLines.RemoveAt(interSectingIndex);     // the intersecting line from the internal lines list
                    inputLines.RemoveAt(0);                     // Remove the first line and 
                    continue;
                }

                // This means that the line is not intersecting with anything
                IntersectingInternalLines.Add(new List<InputLine> { firstLine });    //Add this to a separate list
                inputLines.RemoveAt(0);                                              //and remove this
            }

        }

        private static LineType GetLineType(InputLine inputLine)
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

        private void WriteInputListToFile(List<List<InputLine>> intersectingLineList, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            // Iterate through each grid line
            foreach (var list in intersectingLineList)
            {
                foreach (var line in list)
                {
                    // Append the XYZ coordinates to the StringBuilder
                    sb.AppendLine($" start = {line.start} end= {line.end}");
                }

                sb.AppendLine("\n\n");
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());
        }


    }
}

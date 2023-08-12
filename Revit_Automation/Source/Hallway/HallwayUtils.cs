using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using static Revit_Automation.Source.Hallway.HallwayGenerator;
using static Revit_Automation.Source.Hallway.HallwayTrim;

namespace Revit_Automation.Source.Hallway
{
    internal static class PointUtils
    {
        /// <summary>
        /// check if the points are equal
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>true or false</returns>
        public static bool AreEqual(XYZ first, XYZ second)
        {
            double epsilon = 0.016; // precision

            return Math.Abs(first.X - second.X) < epsilon &&
          Math.Abs(first.Y - second.Y) < epsilon;
        }

        public static bool AreAlmostEqual(XYZ first, XYZ second, double epsilon = 1.0f)
        {
            return Math.Abs(first.X - second.X) < epsilon &&
                   Math.Abs(first.Y - second.Y) < epsilon;
        }

        public static bool AreAlmostEqual(double first, double second , double epsilon = 1.0f)
        {
            return Math.Abs(first - second) < epsilon;
        }

        public static bool IsPointWithinBoundingBox(XYZ point, BoundingBoxXYZ boundingBox)
        {
            double minX = boundingBox.Min.X;
            double minY = boundingBox.Min.Y;
            double minZ = boundingBox.Min.Z;
            double maxX = boundingBox.Max.X;
            double maxY = boundingBox.Max.Y;
            double maxZ = boundingBox.Max.Z;

            if (point.X >= minX && point.X <= maxX &&
                point.Y >= minY && point.Y <= maxY &&
                point.Z >= minZ && point.Z <= maxZ)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the point is present in the list
        /// </summary>
        /// <param name="points"></param>
        /// <param name="point"></param>
        /// <returns>returns - 1 if the point is not present, otherwise returns the index</returns>
        public static int IndexOf(List<XYZ> points, XYZ point)
        {
            for( int i =0; i < points.Count; i++)
            {
                if(AreAlmostEqual(point, points[i]))
                    return i;
            }

            return -1;
        }
    }

    internal static class LineUtils
    {
        /// <summary>
        ///  Checks if the horizontal and vertical line are intersecting
        /// </summary>
        /// <param name="horizontalLine"></param>
        /// <param name="verticalLine"></param>
        /// <returns></returns>
        public static bool AreIntersecting(InputLine horizontalLine, InputLine verticalLine)
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

        public static double GetLineLength(InputLine inputLine)
        {
            double length = 0;

            if(InputLine.GetLineType(inputLine) == LineType.HORIZONTAL)
                length =  Math.Abs(inputLine.start.X - inputLine.end.X);

            if (InputLine.GetLineType(inputLine) == LineType.VERTICAL)
                length = Math.Abs (inputLine.start.Y - inputLine.end.Y);

            return length;
        }

        public static bool AreLinesEqual(InputLine first, InputLine second)
        {
            double epsilon = 0.016;

            return Math.Abs(first.start.X - second.start.X) < epsilon &&
          Math.Abs(first.start.Y - second.start.Y) < epsilon &&
          Math.Abs(first.end.X - second.end.X) < epsilon &&
          Math.Abs(first.end.Y - second.end.Y) < epsilon;
        }

        public static bool AreLinesAlmostEqual(InputLine first, InputLine second)
        {
            double epsilon = 1.0f;

            return Math.Abs(first.start.X - second.start.X) < epsilon &&
          Math.Abs(first.start.Y - second.start.Y) < epsilon &&
          Math.Abs(first.end.X - second.end.X) < epsilon &&
          Math.Abs(first.end.Y - second.end.Y) < epsilon;
        }

        /// <summary>
        /// sort the given list of lines based on the condition that the,
        /// end of each line is the start of the next line
        /// </summary>
        /// <param name="lines"></param>
        /// <returns> circular sorted lines</returns>
        public static List<InputLine> SortLineListCircular(List<InputLine> lines)
        {
            lines.Sort();
            List<InputLine> sortedLines = new List<InputLine>();

            InputLine startLine = lines[0];
            sortedLines.Add(startLine);
            lines.Remove(startLine);

            while (lines.Count > 0)
            {
                XYZ lastPoint = sortedLines[sortedLines.Count - 1].end;
                bool foundNextLine = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    InputLine line = lines[i];
                    
                    //if (line.start.IsAlmostEqualTo(lastPoint))
                    if(PointUtils.AreAlmostEqual(lastPoint, line.start))
                    {
                        line.start = lastPoint; // normalize the value 
                        sortedLines.Add(line);
                        lines.RemoveAt(i);
                        foundNextLine = true;
                        break;
                    }

                    if (PointUtils.AreAlmostEqual(lastPoint, line.end))
                    {
                        line.Swap();
                        line.start = lastPoint;  // normalize the value 
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

            //normalize first.start and last.end
            var lastIndex = sortedLines.Count - 1;
            var last = sortedLines[lastIndex];
            last.end = sortedLines[0].start;
            sortedLines[lastIndex] = last;

            return sortedLines;
        }
    
        /// <summary>
        /// sort the line list based on the Y Co-ordinate
        /// </summary>
        /// <param name="lines"></param>
        public static void SortByYCoordinate(List<List<XYZ>> listOfLists)
        {
            foreach (var innerList in listOfLists)
            {
                innerList.Sort((p1, p2) => p1.Y.CompareTo(p2.Y));
            }
        }
        /// <summary>
        /// sort the line list based on the X Co-ordinate
        /// </summary>
        /// <param name="listOfLists"></param>
        public static void SortByXCoordinate(List<List<XYZ>> listOfLists)
        {
            foreach (var innerList in listOfLists)
            {
                innerList.Sort((p1, p2) => p1.X.CompareTo(p2.X));
            }
        }

        public static int GetIntersectIndex(XYZ point, List<InputLine> lineList)
        {
            int index = -1;
            for (int i = 0; i < lineList.Count; i++)
            {
                var line = lineList[i];

                if (PointUtils.AreAlmostEqual(point, line.start, 0.5) || PointUtils.AreAlmostEqual(point, line.end, 0.5))
                    return i;
            }
            return index;
        }

        /// <summary>
        /// check if the given line is horizontal
        /// </summary>
        /// <param name="line"></param>
        /// <returns>true if horizontal, else false</returns>
        public static bool IsLineHorizontal(Line line)
        {
            XYZ direction = line.Direction; // Get the direction of the line

            // Define a threshold value for the Y-component to account for slight deviations from a perfectly horizontal line
            double thresholdY = 0.01; // You can adjust this threshold based on your precision requirements

            // Check if the Y-component of the direction vector is close to 0
            if (Math.Abs(direction.Y) < thresholdY)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// check if the given line is vertical
        /// </summary>
        /// <param name="line"></param>
        /// <returns>true if vertical, else false</returns>
        public static bool IsLineVertical(Line line)
        {
            XYZ direction = line.Direction; // Get the direction of the line

            // Define a threshold value for the X-component to account for slight deviations from a perfectly vertical line
            double thresholdX = 0.01; // You can adjust this threshold based on your precision requirements

            // Check if the X-component of the direction vector is close to 0
            if (Math.Abs(direction.X) < thresholdX)
            {
                return true;
            }

            return false;
        }
    }

    internal static class FileWriter
    {
        public static void WriteInputListToFile(List<InputLine> inputList, string filePath)
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

        public static void WriteInputListToFile(List<ExternalLine> externalInputLines, string filePath)
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

        public static void WriteInputListToFile(List<List<InputLine>> intersectingLineList, string filePath)
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

        public static void WritePointListtoFile(List<List<XYZ>> points, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            // iterate through all the face rects and print the faces
            foreach (var pointList in points)
            {
                foreach (var point in pointList)
                {
                    sb.AppendLine($"{point}");
                }
                sb.AppendLine("\n\n");
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());
        }

        public static void WritePointListtoFile(List<XYZ> points, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            // iterate through all the face rects and print the faces
            foreach (var point in points)
            {
                sb.AppendLine($"{point}");
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());
        }

        public static void WriteLabelListtoFile(List<LabelLine> labelList, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            foreach(var labelData in  labelList)
            {
                sb.AppendLine(labelData.mLabel);
                foreach(var line in labelData.mLines)
                {
                    // Append the XYZ coordinates to the StringBuilder
                    sb.AppendLine($"     start = {line.start} end= {line.end}");
                }
                sb.AppendLine("\n");
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());
        }

        public static void WriteDataTableToFile(DataTable dataTable, string filePath)
        {
            if (dataTable.Rows.Count == 0)
                return;

            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            // Write the column headers to the StringBuilder
            foreach (DataColumn column in dataTable.Columns)
            {
                sb.Append($"{column.ColumnName},");
            }
            sb.Length--; // Remove the trailing comma
            sb.AppendLine(); // Move to the next line

            // Write the data rows to the StringBuilder
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    sb.Append($"{item},");
                }
                sb.Length--; // Remove the trailing comma
                sb.AppendLine(); // Move to the next line
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());

        }

        public static void WriteTrimLineInfoToFile(List<TrimLineInfo> trimInfoList, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            foreach(TrimLineInfo line in trimInfoList)
            {
                sb.AppendLine(line.mLabelLine.mLabel);
                sb.AppendLine("\t" + "Intersecting Count: " + line.mIntersectingLines.Count.ToString());
                sb.AppendLine("\t" + "Trim Type: " + line.mTrimType.ToString()) ;
                sb.AppendLine("\t" + "Trim Value: " + line.mTrimValue.ToString()) ;
                sb.AppendLine("\n");
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());

        }
    }

}


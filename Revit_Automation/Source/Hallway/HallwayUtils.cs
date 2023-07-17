using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        public static bool AreLinesEqual(InputLine first, InputLine second)
        {
            double epsilon = 0.016;

            return Math.Abs(first.start.X - second.start.X) < epsilon &&
          Math.Abs(first.start.Y - second.start.Y) < epsilon &&
          Math.Abs(first.end.X - second.end.X) < epsilon &&
          Math.Abs(first.end.Y - second.end.Y) < epsilon;
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
    }
}


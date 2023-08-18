using Revit_Automation.CustomTypes;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Revit_Automation.Source.Utils
{
    internal static class FileLogger
    {
        /// <summary>
        /// Writes the hallway line list into a file
        /// </summary>
        /// <param name="hallwayLines"> List of hallway lines </param>
        /// <param name="filePath"> full file path name</param>
        public static void WriteHallwayLineToFile(List<HallwayLine> hallwayLines, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            // Iterate through each grid line
            foreach (var inputLine in hallwayLines)
            {
                // Append the XYZ coordinates to the StringBuilder
                sb.AppendLine($" start = {inputLine.startpoint} end= {inputLine.endpoint}");
            }

            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// Writes the hallway curveloop list into a file
        /// </summary>
        /// <param name="hallwayLineLoops">List of hallway lists ( direct mapping of hallway curve loop</param>
        /// <param name="filePath">full file path name</param>
        public static void WriteHallwayLineToFile(List<List<HallwayLine>> hallwayLineLoops, string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            // Iterate through each grid line
            foreach (var list in hallwayLineLoops)
            {
                foreach (var line in list)
                {
                    // Append the XYZ coordinates to the StringBuilder
                    sb.AppendLine($" start = {line.startpoint} end= {line.endpoint}");
                }

                sb.AppendLine("\n\n");
            }
            // Write the StringBuilder data to the file
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}

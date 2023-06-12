using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Utils
{
    // ToDo - Configure Level of Logging
    public class Logger
    {
        public static int LoggerLevel;

        private static string filePath;
        public static void CreateLogFile()
        {

            if (!Directory.Exists("C:\\Temp")) 
            {
                Directory.CreateDirectory("C:\\Temp");
            }

            string fileName = "C:\\Temp\\Revit_Automation"; // Specify the desired file name
            string timestamp = DateTime.Now.ToString("ddMMyyyy_HHmmss"); // Generate timestamp in the desired format

            // Combine the file name and timestamp to create the full file name
            filePath = $"{fileName}_{timestamp}.txt";

            // Create the file
            File.CreateText(filePath);
        }

        public static void logMessage(string strMessage)
        {
            if (LoggerLevel == 1)
            {
                
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    // Write text to the file
                    writer.WriteLine("INFO : " + strMessage);
                    writer.WriteLine("\n");
                    writer.Close();
                }
            }
        }

        public static void logError(string strMessage)
        {
            if (LoggerLevel == 1)
            {
                string filePath = "C:\\Temp\\Revit_Log.txt";
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    // Write text to the file
                    writer.WriteLine("ERROR : " + strMessage);
                    writer.WriteLine("\n");
                    writer.Close();
                }
            }
        }
    }
}

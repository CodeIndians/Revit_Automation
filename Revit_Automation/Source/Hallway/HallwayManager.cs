using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;

namespace Revit_Automation.Source.Hallway
{
    /// <summary>
    /// Every hallway operation happens through this class
    /// </summary>
    internal class HallwayManager
    {
        private Document mDocument;


        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="doc">Takes DB document reference</param>
        public HallwayManager(ref Document doc) 
        {
            mDocument = doc;

            Initialize();
        }

        private void Initialize()
        {
            var hallwayLineCollector = new HallwayLineCollector(ref mDocument);

            // COMMENT: comment this in the production release
            FileLogger.WriteHallwayLineToFile(hallwayLineCollector.HallwayLines, @"C:\temp\hallway_lines");

        }

        private void ShowForm()
        {

        }
    }
}

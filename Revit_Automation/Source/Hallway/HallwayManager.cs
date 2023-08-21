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

        private HallwayLineCollector mHallwayLineCollector;

        private HallwayLabelGenerator mHallwayLabelGenerator;


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
            // collect hallway lines
            mHallwayLineCollector = new HallwayLineCollector(ref mDocument);

            mHallwayLabelGenerator = new HallwayLabelGenerator(ref mDocument, mHallwayLineCollector.HallwayLines);

            // COMMENT: comment this in the production release
            //FileLogger.WriteHallwayLineToFile(hallwayLineCollector.HallwayLines, @"C:\temp\hallway_lines");

        }

        // generate the hallway line labels
        public void GenerateLabels()
        {
            mHallwayLabelGenerator.GenerateLabels();
        }

        // show the hallway trim form
        public void ShowForm()
        {
            HallwayTrimForm trimForm = new HallwayTrimForm(ref mDocument, mHallwayLabelGenerator.HorizontalLabelLines, mHallwayLabelGenerator.VerticalLabelLines);
            trimForm.Show();
        }
    }
}

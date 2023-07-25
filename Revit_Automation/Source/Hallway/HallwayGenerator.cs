﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class HallwayGenerator
    {
        private List<InputLine> mExternalHallwayLines;

        private Document mDocument;

        // hatch id for the hallway
        private ElementId hatchId;

        // TODO: need to collect internal lines as well
        public HallwayGenerator(ref Document doc, List<InputLine> externalHallwayLines) 
        {
            // assign the passed hallway lines 
            mExternalHallwayLines = externalHallwayLines;

            // assign the active document
            mDocument = doc;

            var filledRegion = new FilteredElementCollector(mDocument).OfClass(typeof(FilledRegionType));
            foreach (var region in filledRegion)
            {
                if (region.Name == "Hallway hatch")
                    hatchId = region.Id;
            }

            //generate the hallway hatch
            Generate();
        }

        private void Generate()
        {
            var circularSortedLines = LineUtils.SortLineListCircular(mExternalHallwayLines);

            FileWriter.WriteInputListToFile(circularSortedLines, @"C:\temp\circular_external_hallway_lines");

            using (Transaction transaction = new Transaction(mDocument))
            {
                transaction.Start("Creating External Hatches");

                CurveLoop loop = new CurveLoop();
                IList<CurveLoop> curveLoop = new List<CurveLoop>();

                foreach (var externalLine in circularSortedLines)
                {


                    // Create the lines for the bounding loop
                    Line line1 = Line.CreateBound(externalLine.start, externalLine.end);

                    // Add the lines to the bounding loop
                    loop.Append(line1);
                }

                curveLoop.Add(loop);

                FilledRegion hatchData = FilledRegion.Create(mDocument, hatchId, mDocument.ActiveView.Id, curveLoop);

                transaction.Commit();

            }
        }
    }
}

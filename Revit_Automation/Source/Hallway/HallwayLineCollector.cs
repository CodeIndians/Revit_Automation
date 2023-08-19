using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Revit_Automation.Source.Hallway
{
    public class HallwayLineCollector
    {
        private Document mDocument;

        private List<HallwayLine> mHallwayLines;

        private ElementId mHallwayHatchId;

        public List<HallwayLine> HallwayLines { get { return mHallwayLines; } }

        public HallwayLineCollector(ref Document document)
        {
            mDocument = document;

            mHallwayHatchId = GetHallwayHatchId();

            mHallwayLines = GetHallwayLines();
        }

        /// <summary>
        /// Function to get the hatch id of the hallway region type
        /// </summary>
        /// <returns>hallway hatch id</returns>
        private ElementId GetHallwayHatchId()
        {
            // hatch id for hallway hatch filled region 
            ElementId hatchId = null;

            // capture hallway hatch element ID
            var filledRegion = new FilteredElementCollector(mDocument).OfClass(typeof(FilledRegionType));
            foreach (var region in filledRegion)
            {
                if (region.Name == "Hallway hatch")
                {
                    hatchId = region.Id;
                    break;
                }
            }

            return hatchId;
        }

        private List<HallwayLine> GetHallwayLines()
        {
            // collect all the filled regions
            FilteredElementCollector collector = new FilteredElementCollector(mDocument, mDocument.ActiveView.Id);
            ICollection<Element> filledRegions = collector.OfClass(typeof(FilledRegion)).ToElements();

            FilledRegion hallwayRegion = null;

            // collect hallway region from the filled regions based on hatch id 
            foreach (var region in filledRegions)
            {
                if (region.GetTypeId() == mHallwayHatchId)
                {
                    hallwayRegion = region as FilledRegion;
                    break;
                }
            }

            // list of lists to gather curveloops
            // hallway should have a single curve loop
            List<List<HallwayLine>> hallwayCurveLoop = new List<List<HallwayLine>>();

            // gather external hatches
            if (hallwayRegion != null)
            {
                var curveLoops = hallwayRegion.GetBoundaries();
                List<HallwayLine> curveLines = new List<HallwayLine>();

                // loop through the curve loops 
                foreach (var curveLoop in curveLoops)
                {
                    IEnumerator<Curve> curveEnumerator = curveLoop.GetEnumerator();
                    while (curveEnumerator.MoveNext())
                    {
                        curveLines.Add(new HallwayLine(curveEnumerator.Current.GetEndPoint(0), curveEnumerator.Current.GetEndPoint(1)));
                    }
                }

                hallwayCurveLoop.Add(curveLines);
            }

            if(hallwayCurveLoop.Count > 1)
            {
                TaskDialog.Show("Warning", "Extra hallway hatch is detected. Things might not work as expected");
            }

            List<HallwayLine> hallwayLines = hallwayCurveLoop.ElementAtOrDefault(0);

            return hallwayLines;
        }

    }
}

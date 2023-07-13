using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class HallwayManager
    {
        private Document mDocument;

        // all the input lines
        public List<InputLine> InputLines;

        // External lines, which has a list of intersecting internal lines 
        public List<ExternalLine> ExternalLines;

        // lines which are not touching the external lines
        public List<InputLine> InternalInputLines;

        // group of intersecting internal lines
        public List<List<InputLine>> IntersectingInternalLines;

        public HallwayManager(ref Document doc) 
        {
            mDocument = doc;
            InputLines = new List<InputLine>();
            ExternalLines = new List<ExternalLine>();
            InternalInputLines = new List<InputLine>();
            IntersectingInternalLines = new List<List<InputLine>>();

            Collect();

            ProcessLines();

        }

        private void Collect()
        {
            new LineCollector(ref mDocument, ref InputLines, ref ExternalLines);
        }

        private void ProcessLines()
        {
            new HallwayGroupSeparator(ref mDocument, ref InputLines, ref ExternalLines, ref InternalInputLines, ref IntersectingInternalLines);
        }

        public void PlaceHatches()
        {
            var hatch = new ExternalHatch(ref mDocument, ref ExternalLines, ref InternalInputLines);
            hatch.CreateHatching();
        }
    }
}

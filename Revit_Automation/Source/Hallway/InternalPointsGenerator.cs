using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class InternalPointsGenerator
    {
        // internal input rect lines
        public List<List<InputLine>> mInternalLines;


        public InternalPointsGenerator(List<List<InputLine>> internalRectLines)
        {
            mInternalLines = internalRectLines;

            ProcessInternalHallwayLines();
        }

        private void ProcessInternalHallwayLines()
        {
            //TODO : Join the internal lines

        }
    }
}

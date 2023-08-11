using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.ModelCreators.Walls
{
    public class ExteriorInsulationWallPoints : IWallPointsGenerator
    {
        public void AddStudsIfNeeded()
        {
            throw new NotImplementedException();
        }

        public void ComputeEndPoints(Document doc, InputLine inputLine, SortedDictionary<XYZ, string> rightPanelIntersection, SortedDictionary<XYZ, string> leftPanelIntersections, SortedDictionary<XYZ, string> endPanelIntersections, ref List<XYZ> wallEndPointsCollection)
        {
            throw new NotImplementedException();
        }

        public void ComputePanelLength()
        {
            throw new NotImplementedException();
        }

        public void ModifyHeight()
        {
            throw new NotImplementedException();
        }

        public void RemoveStudsIfNeeded()
        {
            throw new NotImplementedException();
        }
    }
}

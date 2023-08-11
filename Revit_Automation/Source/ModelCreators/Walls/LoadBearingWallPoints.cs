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
    public class LoadBearingWallPoints : IWallPointsGenerator
    {
        public void AddStudsIfNeeded()
        {
            throw new NotImplementedException();
        }

        public void ComputeEndPoints(Document doc, InputLine inputLine, SortedDictionary<XYZ, string> rightPanelIntersection, SortedDictionary<XYZ, string> leftPanelIntersections, SortedDictionary<XYZ, string> endPanelIntersections, ref List<XYZ> wallEndPointsCollection)
        { 
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

        //private void ProcessLBWallEndpts(InputLine inputLine, SortedDictionary<XYZ, string> rightPanelIntersection, SortedDictionary<XYZ, string> leftPanelIntersections, SortedDictionary<XYZ, string> endPanelIntersections, ref List<XYZ> wallEndPointsCollection)
        //{
        //    XYZ startpt = null, endPt = null;
        //    PanelDirection panelDirection = PanelDirection.B;

        //    XYZ startpoint = null, endpoint = null;
        //    GenericUtils.GetlineStartAndEndPoints(inputLine, out startpoint, out endpoint);

        //    panelDirection = ComputePanelDirection(inputLine);

        //    LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X) ? LineType.vertical : LineType.Horizontal;
        //    double dWebwidth = GenericUtils.WebWidth(inputLine.strStudType);

        //    if (linetype == LineType.Horizontal)
        //    {

        //        startpt = (panelDirection == PanelDirection.R) ? new XYZ(startpoint.X, startpoint.Y + dWebwidth / 2, startpoint.Z)
        //                         : new XYZ(startpoint.X, startpoint.Y - dWebwidth / 2, startpoint.Z);
        //    }
        //    else
        //    {
        //        startpt = (panelDirection == PanelDirection.R) ? new XYZ(startpoint.X + dWebwidth / 2, startpoint.Y, startpoint.Z)
        //                             : new XYZ(startpoint.X - dWebwidth / 2, startpoint.Y, startpoint.Z);

        //    }

        //    if (linetype == LineType.Horizontal)
        //    {

        //        endPt = (panelDirection == PanelDirection.R) ? new XYZ(endpoint.X, endpoint.Y + dWebwidth / 2, endpoint.Z)
        //                         : new XYZ(endpoint.X, endpoint.Y - dWebwidth / 2, endpoint.Z);
        //    }
        //    else
        //    {
        //        endPt = (panelDirection == PanelDirection.R) ? new XYZ(endpoint.X + dWebwidth / 2, endpoint.Y, endpoint.Z)
        //                             : new XYZ(endpoint.X - dWebwidth / 2, endpoint.Y, endpoint.Z);

        //    }

        //    wallEndPointsCollection.Add(startpt);
        //    wallEndPointsCollection.Add(endPt);
        //}
    }
}

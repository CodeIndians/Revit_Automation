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
    public class NonLoadBearingWallPoints : IWallPointsGenerator
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

        //private void ProcessNLBWallEndpts(InputLine inputLine, SortedDictionary<XYZ, string> rightPanelIntersection, SortedDictionary<XYZ, string> leftPanelIntersections, SortedDictionary<XYZ, string> endPanelIntersections, ref List<XYZ> wallEndPointsCollection)
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

        //        startpt = (panelDirection == PanelDirection.R) ? new XYZ(startpoint.X, startpoint.Y + dWebwidth, startpoint.Z)
        //                         : new XYZ(startpoint.X, startpoint.Y - dWebwidth, startpoint.Z);
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

        //    LineRelations startReleation = LineRelations.NoStartIntersection;
        //    LineRelations endRelation = LineRelations.NoEndIntersection;

        //    if (endPanelIntersections.Count > 0)
        //    {
        //        CheckStartAndEndIntersectRelationsForFireWall(inputLine, endPanelIntersections, ref startReleation, ref endRelation);
        //    }

        //    if (startReleation != LineRelations.NoStartIntersection)
        //    {
        //        KeyValuePair<XYZ, string> kvp = endPanelIntersections.ElementAt(0);
        //        XYZ intersectionPt = kvp.Key;
        //        string combinedString = kvp.Value.ToString();
        //        string[] tokens = combinedString.Split('|');

        //        string strWebWidth = tokens[1];
        //        string strWallType = tokens[0];
        //        PanelDirection intersectingPanelDirection = (PanelDirection)Enum.Parse(typeof(PanelDirection), tokens[2]);

        //        double iInputLineWebWidth = GenericUtils.WebWidth(inputLine.strStudType);
        //        double iIntersectingLineWebWidth = GenericUtils.WebWidth(strWebWidth);

        //        if (startReleation == LineRelations.StartTrimT || startReleation == LineRelations.StartTrimLeft || startReleation == LineRelations.StartTrimRight)
        //        {
        //            if (intersectingPanelDirection == PanelDirection.L)
        //            {
        //                if (linetype == LineType.Horizontal)
        //                {
        //                    startpt = new XYZ(intersectionPt.X, startpt.Y, startpt.Z);
        //                }
        //                else
        //                {
        //                    startpt = new XYZ(startpt.X, intersectionPt.Y, startpt.Z);
        //                }
        //            }
        //        }
        //    }

        //    if (endRelation != LineRelations.NoEndIntersection)
        //    {
        //        KeyValuePair<XYZ, string> kvp = endPanelIntersections.ElementAt(0);
        //        XYZ intersectionPt = kvp.Key;
        //        string combinedString = kvp.Value.ToString();
        //        string[] tokens = combinedString.Split('|');

        //        string strWebWidth = tokens[1];
        //        string strWallType = tokens[0];
        //        PanelDirection intersectingPanelDirection = (PanelDirection)Enum.Parse(typeof(PanelDirection), tokens[2]);

        //        double iInputLineWebWidth = GenericUtils.WebWidth(inputLine.strStudType);
        //        double iIntersectingLineWebWidth = GenericUtils.WebWidth(strWebWidth);

        //        if (endRelation == LineRelations.EndTrimT || endRelation == LineRelations.EndTrimLeft || endRelation == LineRelations.EndTrimRight)
        //        {
        //            if (intersectingPanelDirection == PanelDirection.R)
        //            {
        //                if (linetype == LineType.Horizontal)
        //                {
        //                    endPt = new XYZ(intersectionPt.X, endPt.Y, endPt.Z);
        //                }
        //                else
        //                {
        //                    endPt = new XYZ(endPt.X, intersectionPt.Y, endPt.Z);
        //                }
        //            }
        //        }
        //    }
        //    List<XYZ> middleIntersections = new List<XYZ>();
        //    AdjustWallEndPoints(ref startpt, ref middleIntersections, ref endPt, linetype, panelDirection);

        //    wallEndPointsCollection.Add(startpt);
        //    wallEndPointsCollection.Add(endPt);
        //}
    }
}

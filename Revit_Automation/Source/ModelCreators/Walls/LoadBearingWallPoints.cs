using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Interfaces;
using Revit_Automation.Source.Utils;
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
            XYZ startpt = null, endPt = null;
            PanelDirection panelDirection = PanelDirection.B;
            List<XYZ> intermediatePts = null;

            XYZ startpoint = null, endpoint = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out startpoint, out endpoint);

            panelDirection = ComputePanelDirection(inputLine);

            LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X) ? LineType.vertical : LineType.Horizontal;
            double dWebwidth = GenericUtils.WebWidth(inputLine.strStudType);

            if (linetype == LineType.Horizontal)
            {

                startpt = (panelDirection == PanelDirection.R) ? new XYZ(startpoint.X, startpoint.Y + dWebwidth / 2, startpoint.Z)
                                 : new XYZ(startpoint.X, startpoint.Y - dWebwidth / 2, startpoint.Z);
            }
            else
            {
                startpt = (panelDirection == PanelDirection.R) ? new XYZ(startpoint.X + dWebwidth / 2, startpoint.Y, startpoint.Z)
                                     : new XYZ(startpoint.X - dWebwidth / 2, startpoint.Y, startpoint.Z);

            }

            if (linetype == LineType.Horizontal)
            {

                endPt = (panelDirection == PanelDirection.R) ? new XYZ(endpoint.X, endpoint.Y + dWebwidth / 2, endpoint.Z)
                                 : new XYZ(endpoint.X, endpoint.Y - dWebwidth / 2, endpoint.Z);
            }
            else
            {
                endPt = (panelDirection == PanelDirection.R) ? new XYZ(endpoint.X + dWebwidth / 2, endpoint.Y, endpoint.Z)
                                     : new XYZ(endpoint.X - dWebwidth / 2, endpoint.Y, endpoint.Z);

            }

            wallEndPointsCollection.Add(startpt);
            wallEndPointsCollection.AddRange(intermediatePts);
            wallEndPointsCollection.Add(endPt);
        }

        private PanelDirection ComputePanelDirection(InputLine inputLine)
        {
            // Panel Direction computation has the following priority.
            // Line > Settings > Automatic Computation

            PanelDirection panelDirection = PanelDirection.B;

            // Get Line End points.
            XYZ pt1 = null, pt2 = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);

            // Get the orientation of the line
            LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

            string strPanelDirection = (lineType == LineType.Horizontal) ?
                                            inputLine.strHorizontalPanelDirection :
                                            inputLine.strVerticalPanelDirection;

            if (strPanelDirection != string.Empty)
            {
                panelDirection = (PanelDirection)Enum.Parse(typeof(PanelDirection), strPanelDirection);
                return panelDirection;
            }
            else
            {
                XYZ lineOrientation = pt2 - pt1;
                XYZ SlopeDirection = RoofUtility.GetRoofSlopeDirection(pt1);

                //if Panel Direction Computation is automatic and Line is perpendicular to slope determine direction
                if ((GlobalSettings.s_PanelDirectionComputation == 0) && !(MathUtils.IsParallel(SlopeDirection, lineOrientation)) && SlopeDirection != null)
                {
                    if (lineType == LineType.Horizontal && SlopeDirection.Y < 0)
                        panelDirection = PanelDirection.D;
                    else if (lineType == LineType.Horizontal && SlopeDirection.Y > 0)
                        panelDirection = PanelDirection.U;
                    else if (lineType == LineType.vertical && SlopeDirection.X > 0)
                        panelDirection = PanelDirection.R;
                    else if (lineType == LineType.vertical && SlopeDirection.X < 0)
                        panelDirection = PanelDirection.L;
                }
                else
                {
                    PanelTypeGlobalParams pg = string.IsNullOrEmpty(inputLine.strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == inputLine.strPanelType);

                    string strPanelDir = (lineType == LineType.Horizontal) ? pg.strPanelHorizontalDirection : pg.strPanelVerticalDirection;
                    panelDirection = (PanelDirection)Enum.Parse(typeof(PanelDirection), strPanelDir);
                }
                return panelDirection;
            }
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

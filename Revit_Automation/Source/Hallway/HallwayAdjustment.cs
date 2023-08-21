using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class HallwayAdjustment
    {
        private static Document mDocument;

        private ElementId mHallwayHatchId;

        public HallwayAdjustment(ref Document doc)
        {
            mDocument = doc;

            mHallwayHatchId = GetHallwayHatchId();

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

        private FilledRegion GetHallwayRegion()
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

            return hallwayRegion;
        }

        public void AdjustHallwayLine( HallwayLabelLine labelLine, double adjustValue )
        {
            // check if the label line is horizontal or vertical
            bool isHorizontal = HallwayUtils.GetLineType(labelLine.mLines[0]) == LineOrientation.HORIZONTAL;

            XYZ moveVector = isHorizontal ? new XYZ(0, adjustValue, 0) : new XYZ(adjustValue, 0, 0);

            var hallwayRegion = GetHallwayRegion();

            // Identify the edge you want to move (for example, the first edge in the first loop)
            IList<CurveLoop> originalCurveLoops = hallwayRegion.GetBoundaries();

            IList<CurveLoop> modifiedCurveLoops = null;

            foreach (var line in labelLine.mLines)
            {

                modifiedCurveLoops = ModifyHallwayLine(line, originalCurveLoops, moveVector);

                originalCurveLoops = modifiedCurveLoops;
            }

            // Update the filled region's boundary
            using (Transaction transaction = new Transaction(mDocument, "Move Edge"))
            {
                transaction.Start();

                var typeId = hallwayRegion.GetTypeId();

                mDocument.Delete(hallwayRegion.Id);

                FilledRegion newFilledRegion = FilledRegion.Create(mDocument, typeId, mDocument.ActiveView.Id, modifiedCurveLoops);

                transaction.Commit();
            }


        }

        private List<CurveLoop> ModifyHallwayLine( HallwayLine hallwayLine, IList<CurveLoop> originalCurveLoops , XYZ moveVector)
        {
            List<CurveLoop> modifiedCurveLoops = new List<CurveLoop>();

            foreach (CurveLoop originalLoop in originalCurveLoops)
            {
                List<Curve> curveArray = new List<Curve>();

                foreach (Curve originalCurve in originalLoop)
                {
                   
                    XYZ startPoint = originalCurve.GetEndPoint(0);
                    XYZ endPoint = originalCurve.GetEndPoint(1);

                    HallwayLine tempLine = new HallwayLine(startPoint,endPoint);

                    //move the entire line if the lines are equal
                    if (HallwayUtils.AreLinesEqual(hallwayLine, tempLine))
                    {
                        startPoint += moveVector;
                        endPoint += moveVector;
                    }
                    else if (HallwayUtils.AreAlmostEqual(startPoint, hallwayLine.startpoint) 
                        || HallwayUtils.AreAlmostEqual(startPoint, hallwayLine.endpoint))
                    {
                        startPoint += moveVector;
                    }
                    else if (HallwayUtils.AreAlmostEqual(endPoint, hallwayLine.startpoint)
                        || HallwayUtils.AreAlmostEqual(endPoint, hallwayLine.endpoint))
                    {
                        endPoint += moveVector;
                    }


                    Line modifiedLine = Line.CreateBound(startPoint, endPoint);
                    curveArray.Add(modifiedLine as Curve);
                }

                CurveLoop modifiedLoop = CurveLoop.Create(curveArray);

                modifiedCurveLoops.Add(modifiedLoop);
            }


            return modifiedCurveLoops;
        }
    }
}

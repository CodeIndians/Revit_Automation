using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class ExternalHatch : HatchBase
    {
        private readonly Document mDocument;

        // External lines, which has a list of intersecting internal lines 
        private readonly List<ExternalLine> ExternalLines;

        // lines which are not touching the external lines
        private readonly List<InputLine> InternalInputLines;

        private ElementId hatchId;

        public ExternalHatch(ref Document doc, ref List<ExternalLine> externalLines, ref List<InputLine> internalInputLines) 
        {
            mDocument = doc;
            ExternalLines = externalLines;
            InternalInputLines = internalInputLines;

            // collect the hatch id of the hatch element obstinate orange
            var filledRegion = new FilteredElementCollector(mDocument).OfClass(typeof(FilledRegionType));
            foreach (var region in filledRegion)
            {
                if (region.Name == "Obstinate Orange")
                {
                    hatchId = region.Id;
                    break;
                }
            }
        }

        protected override void PlaceHatches()
        {
            using (Transaction transaction = new Transaction(mDocument))
            {
                transaction.Start("Creating External Hatches");

                // iterate through all the collected external lines 
                foreach (var externalLine in ExternalLines)
                {
                    // for each external line, again iterate through all the intersecting internal input lines
                    for (int i = 0; i < externalLine.intersectingInternalInputLines.Count - 1; i++)
                    {
                        CurveLoop loop = new CurveLoop();

                        var firstLine = externalLine.intersectingInternalInputLines[i];
                        var secondLine = externalLine.intersectingInternalInputLines[i + 1];

                        // for each intersecting input lines set join the consecutive parallel lines 
                        // this will not cause any problems since the lines are already in the sorted order 
                        if (InputLine.GetLineType(firstLine) == LineType.HORIZONTAL && InputLine.GetLineType(secondLine) == LineType.HORIZONTAL)
                        {
                            var firstLineLength = Math.Abs(firstLine.start.X - firstLine.end.X);
                            var secondLineLength = Math.Abs(secondLine.start.X - secondLine.end.X);

                            // first line is longer
                            if (firstLineLength - secondLineLength > 0.016)
                            {
                                // form a new first line
                                XYZ newStart = new XYZ(secondLine.start.X, firstLine.start.Y, firstLine.start.Z);
                                XYZ newEnd = new XYZ(secondLine.end.X, firstLine.end.Y, firstLine.end.Z);
                                firstLine = new InputLine(newStart, newEnd);
                            }
                            // second line is longer
                            else if (secondLineLength - firstLineLength > 0.016)
                            {
                                //for a new second line
                                XYZ newStart = new XYZ(firstLine.start.X, secondLine.start.Y, secondLine.start.Z);
                                XYZ newEnd = new XYZ(firstLine.start.X, secondLine.end.Y, secondLine.end.Z);
                                secondLine = new InputLine(newStart, newEnd);
                            }
                        }
                        else if (InputLine.GetLineType(firstLine) == LineType.VERTICAL && InputLine.GetLineType(secondLine) == LineType.VERTICAL)
                        {
                            var firstLineLength = Math.Abs(firstLine.start.Y - firstLine.end.Y);
                            var secondLineLength = Math.Abs(secondLine.start.Y - secondLine.end.Y);

                            // first line is longer
                            if (firstLineLength - secondLineLength > 0.016)
                            {
                                // form a new first line
                                XYZ newStart = new XYZ(firstLine.start.X, secondLine.start.Y, firstLine.start.Z);
                                XYZ newEnd = new XYZ(firstLine.end.X, secondLine.end.Y, firstLine.end.Z);
                                firstLine = new InputLine(newStart, newEnd);
                            }
                            // second line is longer
                            else if (secondLineLength - firstLineLength > 0.016)
                            {
                                //for a new second line
                                XYZ newStart = new XYZ(secondLine.start.X, firstLine.start.Y, secondLine.start.Z);
                                XYZ newEnd = new XYZ(secondLine.start.X, firstLine.end.Y, secondLine.end.Z);
                                secondLine = new InputLine(newStart, newEnd);
                            }
                        }

                        // safe checks, the line is a point
                        if (PointUtils.AreAlmostEqual(firstLine.start, firstLine.end) || PointUtils.AreAlmostEqual(secondLine.start, secondLine.end))
                            continue;

                        // both lines are same 
                        if (PointUtils.AreAlmostEqual(firstLine.start, secondLine.start) || PointUtils.AreAlmostEqual(firstLine.end, secondLine.end))
                            continue;

                        // Create the lines for the bounding loop
                        Line line1 = Line.CreateBound(firstLine.start, firstLine.end);
                        Line line2 = Line.CreateBound(firstLine.end, secondLine.end);
                        Line line3 = Line.CreateBound(secondLine.end, secondLine.start);
                        Line line4 = Line.CreateBound(secondLine.start, firstLine.start);



                        // Add the lines to the bounding loop
                        loop.Append(line1);
                        loop.Append(line2);
                        loop.Append(line3);
                        loop.Append(line4);

                        IList<CurveLoop> curveLoop = new List<CurveLoop>();
                        curveLoop.Add(loop);

                        FilledRegion hatchData = FilledRegion.Create(mDocument, hatchId, mDocument.ActiveView.Id, curveLoop);
                    }
                }
                transaction.Commit();
            }
        }

        // <summary>
        // Delete the filled region boxes that overlap with hatches
        // </summary>
        protected override void DeleteHatches()
        {
            //FilteredElementCollector collector = new FilteredElementCollector(mDocument, mDocument.ActiveView.Id);
            //ICollection<Element> filledRegionElements = collector.OfClass(typeof(FilledRegion)).ToElements();

            //List<ElementId> elementIds = new List<ElementId>();
            //// Process the collected filled region elements
            //foreach (Element filledRegionElement in filledRegionElements)
            //{
            //    var elementId = filledRegionElement.Id;
            //    FilledRegion filledRegion = filledRegionElement as FilledRegion;
            //    if (filledRegion != null)
            //    {
            //        bool isFound = false;
            //        var boundingBox = filledRegion.get_BoundingBox(null);
            //        foreach (var inputLine in InternalInputLines)
            //        {
            //            if (PointUtils.IsPointWithinBoundingBox(inputLine.start, boundingBox) || PointUtils.IsPointWithinBoundingBox(inputLine.end, boundingBox))
            //            {
            //                isFound = true;
            //                break;
            //            }
            //        }

            //        if (isFound)
            //            elementIds.Add(elementId);
            //    }
            //}

            //// Delete all the element Ids
            //using (Transaction transaction = new Transaction(mDocument, "Delete intersecting external hatches"))
            //{
            //    transaction.Start();
            //    foreach (var element in elementIds)
            //        mDocument.Delete(element);

            //    transaction.Commit();
            //}
        }
    }
}

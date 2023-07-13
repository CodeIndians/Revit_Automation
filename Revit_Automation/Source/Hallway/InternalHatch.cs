using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class InternalHatch : HatchBase
    {
        private readonly Document mDocument;

        private const float precision = 1.0f;

        // group of intersecting internal lines
        public List<List<InputLine>> IntersectingInternalLines;
        public InternalHatch(ref Document document,
                                ref List<List<InputLine>> intersectingInternalLines) 
        {
            mDocument = document;
            IntersectingInternalLines = intersectingInternalLines;
            hatchPairs = new List<Tuple<InputLine, InputLine>>();
        }

        private List<Tuple<InputLine, InputLine>> hatchPairs;

        protected override void DeleteHatches()
        {
            // Not deleting anything for now currently
        }

        protected override void PlaceHatches()
        {
            foreach (var lineList in IntersectingInternalLines)
            {
                List<InputLine> horizontalLines = new List<InputLine> ();
                List<InputLine> verticaLines = new List<InputLine> ();

                foreach (var line in lineList)
                {
                    // collect horizontal and vertical lines
                    if(InputLine.GetLineType(line) == LineType.HORIZONTAL)
                        horizontalLines.Add(line);

                    if (InputLine.GetLineType(line) == LineType.VERTICAL)
                        verticaLines.Add(line);
                }

                while (horizontalLines.Count > 0)
                {
                    var firstLine = horizontalLines[0];
                    for(int i = 1; i < horizontalLines.Count; i++)
                    {
                        if(Math.Abs(firstLine.start.X - horizontalLines[i].start.X) < precision && Math.Abs(firstLine.end.X - horizontalLines[i].end.X) < precision)
                        {
                            hatchPairs.Add(new Tuple<InputLine, InputLine>(firstLine, horizontalLines[i]));
                            break;
                        }
                    }
                    horizontalLines.RemoveAt(0);
                }

                while (verticaLines.Count > 0)
                {
                    var firstLine = verticaLines[0];
                    for (int i = 1; i < verticaLines.Count; i++)
                    {
                        if (Math.Abs(firstLine.start.Y - verticaLines[i].start.Y) < precision && Math.Abs(firstLine.end.Y - verticaLines[i].end.Y) < precision)
                        {
                            hatchPairs.Add(new Tuple<InputLine, InputLine>(firstLine, verticaLines[i]));
                            break;
                        }
                    }
                    verticaLines.RemoveAt(0);
                }
            }

            // Add the hacthes 
            AddHatches();
        }

        private void AddHatches()
        {
            var filledRegion = new FilteredElementCollector(mDocument).OfClass(typeof(FilledRegionType));
            var typeId = filledRegion.First().Id;

            using (Transaction transaction = new Transaction(mDocument))
            {
                transaction.Start("Creating Internal Hatches");
                foreach (var hatchPair in hatchPairs)
                {
                    var firstLine = hatchPair.Item1;
                    var secondLine = hatchPair.Item2;

                    // Create the lines for the bounding loop
                    Line line1 = Line.CreateBound(firstLine.start, firstLine.end);
                    Line line2 = Line.CreateBound(firstLine.end, secondLine.end);
                    Line line3 = Line.CreateBound(secondLine.end, secondLine.start);
                    Line line4 = Line.CreateBound(secondLine.start, firstLine.start);

                    CurveLoop loop = new CurveLoop();

                    // Add the lines to the bounding loop
                    loop.Append(line1);
                    loop.Append(line2);
                    loop.Append(line3);
                    loop.Append(line4);

                    IList<CurveLoop> curveLoop = new List<CurveLoop>();
                    curveLoop.Add(loop);

                    FilledRegion hatchData = FilledRegion.Create(mDocument, typeId, mDocument.ActiveView.Id, curveLoop);
                }
                transaction.Commit();
            }
        }
    }
}

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class HallwayGenerator
    {
        private List<InputLine> mExternalHallwayLines;

        private List<List<InputLine>> mInternalHallwayLineLoops;

        public List<LabelLine> mHorizontalLabelLines;

        public List<LabelLine> mVerticalLabelLines;

        private Document mDocument;

        // hatch id for the hallway
        private ElementId hatchId;

        public class LabelLine
        {
            public string mLabel;

            public List<InputLine> mLines;
            public LabelLine(string label, InputLine line)
            {
                mLabel = label;
                mLines = new List<InputLine>
                {
                    line
                };
            }
        }


        // TODO: need to collect internal lines as well
        public HallwayGenerator(ref Document doc, List<InputLine> externalHallwayLines, List<List<InputLine>> internalHallwayLineLoops) 
        {
            // assign the passed hallway lines 
            mExternalHallwayLines = externalHallwayLines;

            mInternalHallwayLineLoops = internalHallwayLineLoops;

            // assign the active document
            mDocument = doc;

            mHorizontalLabelLines = new List<LabelLine>();

            mVerticalLabelLines = new List<LabelLine>();

            var filledRegion = new FilteredElementCollector(mDocument).OfClass(typeof(FilledRegionType));
            foreach (var region in filledRegion)
            {
                if (region.Name == "Hallway hatch")
                    hatchId = region.Id;
            }

            //Generate labels on the hallway lines
            GenerateLabels();

            //generate the hallway hatch
            Generate();

            
        }

        private void Generate()
        {
            var circularSortedLines = LineUtils.SortLineListCircular(mExternalHallwayLines);

            FileWriter.WriteInputListToFile(circularSortedLines, @"C:\temp\circular_external_hallway_lines");

            using (Transaction transaction = new Transaction(mDocument))
            {
                transaction.Start("Creating External Hatches");

                CurveLoop loop = new CurveLoop();
                IList<CurveLoop> curveLoop = new List<CurveLoop>();

                foreach (var externalLine in circularSortedLines)
                {
                    // Create the lines for the bounding loop
                    Line line1 = Line.CreateBound(externalLine.start, externalLine.end);

                    // Add the lines to the bounding loop
                    loop.Append(line1);
                }

                curveLoop.Add(loop);

                foreach(var internalLineList in mInternalHallwayLineLoops)
                {
                    CurveLoop internalLoop = new CurveLoop();
                    var internalCircularSortedlines = LineUtils.SortLineListCircular(internalLineList);

                    //FileWriter.WriteInputListToFile(internalCircularSortedlines, @"C:\temp\circular_internal_hallway_lines");

                    foreach (var internalLine in internalCircularSortedlines)
                    {
                        // Create the lines for the bounding loop
                        Line line1 = Line.CreateBound(internalLine.start, internalLine.end);

                        // Add the lines to the bounding loop
                        internalLoop.Append(line1);
                    }

                    curveLoop.Add(internalLoop);
                }

                FilledRegion hatchData = FilledRegion.Create(mDocument, hatchId, mDocument.ActiveView.Id, curveLoop);

                transaction.Commit();

            }
        }
    
        private void GenerateLabels()
        {
            // join internal and external hallway loop lines and separate them into horizontal and vertical lines

            List<InputLine> horLines = new List<InputLine>();

            List<InputLine> verLines = new List<InputLine>();

            // separate hor and ver lines from external hallway lines 
            foreach(var line in mExternalHallwayLines)
            {
                if(InputLine.GetLineType(line) == LineType.HORIZONTAL)
                    horLines.Add(line);
                else if (InputLine.GetLineType(line) ==LineType.VERTICAL)
                    verLines.Add(line);
            }

            //separate hor and verlines from internal hallway lines 
            foreach (var lineList in mInternalHallwayLineLoops)
            {
                foreach(var line in lineList)
                {
                    if (InputLine.GetLineType(line) == LineType.HORIZONTAL)
                        horLines.Add(line);
                    else if (InputLine.GetLineType(line) == LineType.VERTICAL)
                        verLines.Add(line);
                }
            }

            // sort horizontal lines by Y Co-ordinate
            horLines.Sort((p1, p2) => p1.start.Y.CompareTo(p2.start.Y));

            //sort vertical lines by X Co-ordinate
            verLines.Sort((p1, p2) => p1.start.X.CompareTo(p2.start.X));

            //FileWriter.WriteInputListToFile(horLines, @"C:\temp\hor_label_lines");
            //FileWriter.WriteInputListToFile(verLines, @"C:\temp\ver_label_lines");

            var textId = GetExistingTextNoteType(mDocument, "5/32\" Arial").Id;

            using (Transaction tx = new Transaction(mDocument, "Place Text Box"))
            {
                tx.Start();

                int horNum = 1;

                int verNum = 1;

                for (int i = 0; i < horLines.Count; i++)
                {
                    XYZ midpoint = (horLines[i].start + horLines[i].end) * 0.5;

                    if(i > 0)
                    {
                        if (!PointUtils.AreAlmostEqual(horLines[i].start.Y, horLines[i - 1].start.Y))
                            horNum++;
                    }

                    var index = mHorizontalLabelLines.FindIndex(x => x.mLabel == $"H{horNum}");
                    // add the line to the label list
                    if ( index == -1)
                        mHorizontalLabelLines.Add(new LabelLine($"H{horNum}", horLines[i]));
                    else
                        mHorizontalLabelLines[index].mLines.Add(horLines[i]);

                    // Create a text note at the midpoint
                    TextNote textNote = TextNote.Create(mDocument, mDocument.ActiveView.Id, midpoint, String.Format($"H{horNum}"), new TextNoteOptions()
                    {
                        HorizontalAlignment = HorizontalTextAlignment.Center,
                        VerticalAlignment = VerticalTextAlignment.Middle,
                        Rotation = 0,
                        TypeId = textId
                    });
                }

                for (int i = 0; i < verLines.Count; i++)
                {
                    XYZ midpoint = (verLines[i].start + verLines[i].end) * 0.5;

                    if (i > 0)
                    {
                        if (!PointUtils.AreAlmostEqual(verLines[i].start.X, verLines[i - 1].start.X))
                            verNum++;
                    }

                    var index = mVerticalLabelLines.FindIndex(x => x.mLabel == $"V{verNum}");
                    // add the line to the label list
                    if (index == -1)
                        mVerticalLabelLines.Add(new LabelLine($"V{verNum}", verLines[i]));
                    else
                        mVerticalLabelLines[index].mLines.Add(verLines[i]);

                    // Create a text note at the midpoint
                    TextNote textNote = TextNote.Create(mDocument, mDocument.ActiveView.Id, midpoint, String.Format($"V{verNum}"), new TextNoteOptions()
                    {
                        HorizontalAlignment = HorizontalTextAlignment.Center,
                        VerticalAlignment = VerticalTextAlignment.Middle,
                        Rotation = 1.5708f,
                        TypeId = textId
                    });
                }

                // Set properties of the text note (optional)
                // Example: textNote.TextSize = 0.1; (You can modify other properties like text size, font, etc.)

                tx.Commit();

                FileWriter.WriteLabelListtoFile(mHorizontalLabelLines, @"C:\temp\hor_labels");
                FileWriter.WriteLabelListtoFile(mVerticalLabelLines, @"C:\temp\ver_labels");
            }

        }

        private TextNoteType GetExistingTextNoteType(Document doc, string typeName)
        {
            // Check if the specified text note type already exists in the document
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            TextNoteType existingTextNoteType = collector
                .OfClass(typeof(TextNoteType))
                .FirstOrDefault(x => x.Name == typeName) as TextNoteType;

            return existingTextNoteType;
        }
    }
}

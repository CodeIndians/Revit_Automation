using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Revit_Automation.Source.Hallway
{
    /// <summary>
    /// Generate hallway labels
    /// </summary>
    internal class HallwayLabelGenerator
    {
        private Document mDocument;

        // all the hallway lines
        private List<HallwayLine> mHallwayLines;

        // horizontal hallway label lines
        private List<HallwayLabelLine> mHorizontalLabelLines;

        // vertical hallway label lines
        private List<HallwayLabelLine> mVerticalLabelLines;

        // public accessor for horizontal label Lines
        public List<HallwayLabelLine> HorizontalLabelLines { get { return mHorizontalLabelLines; } }

        // public accessor vertical label lines
        public List<HallwayLabelLine> VerticalLabelLines { get { return mVerticalLabelLines; } }

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="document">document</param>
        /// <param name="hallwayLines">list of hallway lines</param>
        public HallwayLabelGenerator(ref Document document, List<HallwayLine> hallwayLines)
        {
            mDocument = document;

            mHallwayLines = hallwayLines;

            mHorizontalLabelLines = new List<HallwayLabelLine>();

            mVerticalLabelLines = new List<HallwayLabelLine>();
        }

        /// <summary>
        /// Generate the horizontal and vertical labels
        /// </summary>
        public void GenerateLabels()
        {
            List<HallwayLine> horLines = new List<HallwayLine>();

            List<HallwayLine> verLines = new List<HallwayLine>();

            // separate hor and ver lines from hallway lines 
            foreach (var line in mHallwayLines)
            {
                if (HallwayUtils.GetLineType(line) == LineOrientation.HORIZONTAL)
                    horLines.Add(line);

                else if (HallwayUtils.GetLineType(line) == LineOrientation.VERTICAL)
                    verLines.Add(line);
            }

            // sort horizontal lines by Y Co-ordinate
            horLines.Sort((p1, p2) => p1.startpoint.Y.CompareTo(p2.startpoint.Y));

            // sort vertical lines by X Co-ordinate
            verLines.Sort((p1, p2) => p1.endpoint.X.CompareTo(p2.endpoint.X));

            var textId = GetExistingTextNoteType(ref mDocument, "3/32\" Arial").Id;

            using (Transaction tx = new Transaction(mDocument, "Place Text Box"))
            {
                //start transaction
                tx.Start();

                mHorizontalLabelLines = GetParallelLabelLines(horLines,LineOrientation.HORIZONTAL);
                mVerticalLabelLines = GetParallelLabelLines(verLines,LineOrientation.VERTICAL);

                tx.Commit();
            }
        }

        /// <summary>
        /// Returns the type identifier for the specified font string type
        /// </summary>
        /// <param name="doc"> DB document </param>
        /// <param name="typeName"> Type Name ( which is the type of the text font) </param>
        /// <returns></returns>
        private TextNoteType GetExistingTextNoteType(ref Document doc, string typeName)
        {
            // Check if the specified text note type already exists in the document
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            TextNoteType existingTextNoteType = collector
                .OfClass(typeof(TextNoteType))
                .FirstOrDefault(x => x.Name == typeName) as TextNoteType;

            return existingTextNoteType;
        }

        /// <summary>
        /// Form the label lines from the parallel lines ( Horizontal or Vertical ) 
        /// colinear lines are added to the internal list with the same label
        /// </summary>
        /// <param name="parallelLines"> List of parallel hallway lines ( horizontal or vertical ) </param>
        /// <param name="orientationType"> HORIZONTAL or VERTICAL </param>
        /// <returns></returns>
        private List<HallwayLabelLine> GetParallelLabelLines(List<HallwayLine> parallelLines, LineOrientation orientationType)
        {
            if (orientationType == LineOrientation.INVALID)
                return null;

            List<HallwayLabelLine> hallwayLabelLines = new List<HallwayLabelLine>();

            int lineNum = 1;

            // H for horizontal, V for vertical
            string prefix = orientationType == LineOrientation.HORIZONTAL ? "H" : "V";

            // collect horizontal label lines
            for (int i = 0; i < parallelLines.Count; i++)
            {
                //compute the midpoint 
                XYZ midpoint = (parallelLines[i].startpoint + parallelLines[i].endpoint) * 0.5;

                if (i > 0)
                {
                    // increase the linenum only if the lines are not colinear
                    if (orientationType == LineOrientation.HORIZONTAL
                        && !HallwayUtils.AreAlmostEqual(parallelLines[i].startpoint.Y, parallelLines[i - 1].startpoint.Y))
                        lineNum++;
                    // increase the linenum only if the lines are not colinear
                    else if (orientationType == LineOrientation.VERTICAL
                        && !HallwayUtils.AreAlmostEqual(parallelLines[i].startpoint.X, parallelLines[i - 1].startpoint.X))
                        lineNum++;
                }

                // all the co linear lines are added to the same label
                var index = hallwayLabelLines.FindIndex(x => x.mLabel == $"{prefix}{lineNum}");

                // add the line to the label list
                if (index == -1)
                    hallwayLabelLines.Add(new HallwayLabelLine($"{prefix}{lineNum}", parallelLines[i]));
                else
                    hallwayLabelLines[index].mLines.Add(parallelLines[i]);

                // Create a text note at the midpoint
                TextNote textNote = TextNote.Create(mDocument, mDocument.ActiveView.Id, midpoint, String.Format($"{prefix}{lineNum}"), new TextNoteOptions()
                {
                    HorizontalAlignment = HorizontalTextAlignment.Center,
                    VerticalAlignment = VerticalTextAlignment.Middle,
                    Rotation = (orientationType == LineOrientation.HORIZONTAL) ? 0.0f : 1.5708f,
                    TypeId = GetExistingTextNoteType(ref mDocument, "3/32\" Arial").Id
                });

            }

            return hallwayLabelLines;
        }

    }
}

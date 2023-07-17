using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class HallwayGroupSeparator
    {
        private readonly Document mDocument;
        // all the input lines
        private readonly List<InputLine> InputLines;

        // External lines, which has a list of intersecting internal lines 
        private readonly List<ExternalLine> ExternalLines;

        // lines which are not touching the external lines
        private List<InputLine> InternalInputLines;

        // group of intersecting internal lines
        private List<List<InputLine>> IntersectingInternalLines;

        public HallwayGroupSeparator(ref Document document,
                                     ref List<InputLine> inputLines, 
                                     ref List<ExternalLine> externalLines,
                                     ref List<InputLine> internalInputLines,
                                     ref List<List<InputLine>> intersectingInternalLines)
        {
            mDocument = document;
            InputLines = inputLines;
            ExternalLines = externalLines;
            InternalInputLines = internalInputLines;
            IntersectingInternalLines = intersectingInternalLines;

            Execute();

            //FileWriter.WriteInputListToFile(InternalInputLines, @"C:\temp\internal_input_lines");
            //FileWriter.WriteInputListToFile(IntersectingInternalLines, @"C:\temp\intersect_group");
        }

        private void Execute()
        {
            SeparateInternalInputLines();
            GroupIntersectingInternalLines();
        }

        private void SeparateInternalInputLines()
        {
            // iterate all the input lines 
            foreach (var inputLine in InputLines)
            {
                bool found = false;
                // iterate through all the collected external lines
                foreach (var externalLine in ExternalLines)
                {
                    // do not add if the input line is equal to the main external line 
                    if (inputLine.Equals(externalLine.mainExternalLine))
                    {
                        found = true;
                        break;
                    }
                    // do not add if the input line is any of the external intersecting lines
                    foreach (var intersectingInputLine in externalLine.intersectingInternalInputLines)
                    {
                        if (inputLine.Equals(intersectingInputLine))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
                // add only if the line not an external line or any of the internal lines that are intersecting with the external lines
                if (!found)
                    InternalInputLines.Add(inputLine);
            }
        }

        private void GroupIntersectingInternalLines()
        {
            //copy the internal input lines to a separate list
            List<InputLine> inputLines = new List<InputLine>(InternalInputLines);

            // return if these are no internal input lines 
            if (inputLines.Count <= 0)
                return;


            while (inputLines.Count > 0)
            {
                // int firstIndex = -1; // we will always be comparing the first element
                int interSectingIndex = -1; // this will remain -1 is there is not intersection

                var firstLine = inputLines[0]; //capture the first input line

                // check if it already intersects with any line of the list 
                for (var j = 0; j < IntersectingInternalLines.Count; j++)
                {
                    for (var k = 0; k < IntersectingInternalLines[j].Count; k++)
                    {
                        if (firstLine.AreLinesIntersecting(IntersectingInternalLines[j][k]))
                        {
                            interSectingIndex = j;
                            break;
                        }
                    }
                }

                // this means that the line is intersecting with one of the already collected list of lists
                if (interSectingIndex != -1)
                {
                    IntersectingInternalLines[interSectingIndex].Add(firstLine); // add this to the list input lines on which this was intersecting

                    inputLines.RemoveAt(0); // remove the first line and
                    continue;               //skip to the next iteration
                }

                // iterate through all the available input lines skipping the first one 
                for (var i = 1; i < inputLines.Count; i++)
                {
                    if (firstLine.AreLinesIntersecting(inputLines[i]))
                    {
                        interSectingIndex = i; // set the intersecting index  
                        break;
                    }
                }

                // If any intersection pair is found, move it to a new list
                if (interSectingIndex != -1)
                {
                    int secondIntersectingIndex = -1;
                    var secondLine = inputLines[interSectingIndex];

                    // check if it already intersects with any line of the list 
                    for (var j = 0; j < IntersectingInternalLines.Count; j++)
                    {
                        for (var k = 0; k < IntersectingInternalLines[j].Count; k++)
                        {
                            if (secondLine.AreLinesIntersecting(IntersectingInternalLines[j][k]))
                            {
                                secondIntersectingIndex = j;
                                break;
                            }
                        }
                    }

                    if (secondIntersectingIndex != -1)
                    {
                        IntersectingInternalLines[secondIntersectingIndex].Add(firstLine);
                        IntersectingInternalLines[secondIntersectingIndex].Add(secondLine);
                    }
                    else
                    {
                        // add the pair of first and the intersecting line to the Intersecting list of lists 
                        IntersectingInternalLines.Add(new List<InputLine> { firstLine, secondLine });
                    }

                    inputLines.RemoveAt(interSectingIndex);     // the intersecting line from the internal lines list
                    inputLines.RemoveAt(0);                     // Remove the first line and 
                    continue;
                }

                // This means that the line is not intersecting with anything
                IntersectingInternalLines.Add(new List<InputLine> { firstLine });    //Add this to a separate list
                inputLines.RemoveAt(0);                                              //and remove this
            }

        }
    }
}

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class InternalPointsGenerator
    {
        // internal input rect lines
        public List<List<InputLine>> mInternalLines;


        public InternalPointsGenerator(List<List<InputLine>> internalRectLines)
        {
            mInternalLines = internalRectLines;

            ProcessInternalHallwayLines();

            FileWriter.WriteInputListToFile(mInternalLines, @"C:\temp\internal_hallway_lines");
        }

        private void ProcessInternalHallwayLines()
        {
            
            int currentIndex = 0;

            while (currentIndex < mInternalLines.Count)
            {
                int indexToDelete = -1;

                for (int i = currentIndex + 1; i < mInternalLines.Count; i++)
                {
                    var first = mInternalLines[currentIndex];
                    var second = mInternalLines[i];
                    if (JoinOverlappingRects(ref first,ref second))
                    {
                        mInternalLines[currentIndex] = first;
                        indexToDelete = i;
                        currentIndex--;
                        break;
                    }
                }

                currentIndex++;

                if(indexToDelete != -1)
                {
                    mInternalLines.RemoveAt(indexToDelete);
                }
            }

        }

        private bool JoinOverlappingRects(ref List<InputLine> firstRect, ref List<InputLine> secondRect) 
        {
            bool isOverlap = false;

            for (int i = 0; i < firstRect.Count; i++) 
            {
                if (isOverlap)
                    break;

                var firstLineType = InputLine.GetLineType(firstRect[i]);

                for(int j = 0; j < secondRect.Count; j++)
                {
                    var secondLineType = InputLine.GetLineType(secondRect[j]);

                    if(firstLineType == secondLineType)
                    {
                        // check for horizontal overlap
                        if(firstLineType == LineType.HORIZONTAL)
                        {
                            // falling on the same Y position ( possiblity of an overlap)
                            if (PointUtils.AreAlmostEqual(firstRect[i].start.Y, secondRect[j].start.Y))
                            {
                                List<XYZ> pointsList = new List<XYZ>();

                                // add all the 4 points from the two lines into a list 
                                pointsList.Add(firstRect[i].start);
                                pointsList.Add(secondRect[j].start);
                                pointsList.Add(firstRect[i].end);
                                pointsList.Add(secondRect[j].end);

                                // sort the points in the X direction 
                                pointsList.Sort((p1, p2) => p1.X.CompareTo(p2.X));

                                // first condition. Check if the starting points are overlapping
                                if (PointUtils.AreAlmostEqual(pointsList[0], pointsList[1]))
                                {
                                    isOverlap = true;

                                    firstRect[i] = new InputLine(pointsList[2], pointsList[3]);

                                    // remove the intersecting line from the second rect 
                                    secondRect.RemoveAt(j);

                                    // consume the second rect 
                                    firstRect.AddRange(secondRect);

                                    firstRect.Sort();

                                    break;

                                }
                                // second condition. Check if the endinf points are overlapping
                                else if (PointUtils.AreAlmostEqual(pointsList[2], pointsList[3]))
                                {
                                    isOverlap = true;

                                    firstRect[i] = new InputLine(pointsList[0], pointsList[1]);

                                    // remove the intersecting line from the second rect 
                                    secondRect.RemoveAt(j);

                                    // consume the second rect 
                                    firstRect.AddRange(secondRect);

                                    firstRect.Sort();

                                    break;
                                }
                                // third condition. This will remove the overlap part and give the non over lap lines
                                else
                                {
                                    // add the lines to a list 
                                    List<InputLine> lines = new List<InputLine>() { firstRect[i], secondRect[j] };

                                    isOverlap = true;

                                    // This means that the lines are overlapping
                                    if (LineUtils.GetIntersectIndex(pointsList[0],lines) != LineUtils.GetIntersectIndex(pointsList[1], lines))
                                    {
                                        // form new lines with the sorted points

                                        firstRect[i] = new InputLine(pointsList[0], pointsList[1]);

                                        secondRect[j] = new InputLine(pointsList[2], pointsList[3]);

                                        // consume the second rect 
                                        firstRect.AddRange(secondRect);

                                        break;
                                    }
                                }
                            }
                        }
                        // check for vertical overlap 
                        else if (firstLineType == LineType.VERTICAL)
                        {
                            // falling on the same X position ( possibility of an overlap)
                            if(PointUtils.AreAlmostEqual(firstRect[i].start.X,secondRect[j].start.X))
                            {
                                List<XYZ> pointsList = new List<XYZ>();

                                // add all the 4 points from the two lines into a list 
                                pointsList.Add(firstRect[i].start);
                                pointsList.Add(secondRect[j].start);
                                pointsList.Add(firstRect[i].end);
                                pointsList.Add(secondRect[j].end);

                                // sort the points in the Y direction 
                                pointsList.Sort((p1, p2) => p1.Y.CompareTo(p2.Y));

                                // first condition. Check if the starting points are overlapping
                                if (PointUtils.AreAlmostEqual(pointsList[0], pointsList[1]))
                                {
                                    isOverlap = true;

                                    firstRect[i] = new InputLine(pointsList[2], pointsList[3]);

                                    // remove the intersecting line from the second rect 
                                    secondRect.RemoveAt(j);

                                    // consume the second rect 
                                    firstRect.AddRange(secondRect);

                                    firstRect.Sort();

                                    break;

                                }
                                // second condition. Check if the endinf points are overlapping
                                else if (PointUtils.AreAlmostEqual(pointsList[2], pointsList[3]))
                                {
                                    isOverlap = true;

                                    firstRect[i] = new InputLine(pointsList[0], pointsList[1]);

                                    // remove the intersecting line from the second rect 
                                    secondRect.RemoveAt(j);

                                    // consume the second rect 
                                    firstRect.AddRange(secondRect);

                                    firstRect.Sort();

                                    break;
                                }
                                // third condition. This will remove the overlap part and give the non over lap lines
                                else
                                {
                                    // add the lines to a list 
                                    List<InputLine> lines = new List<InputLine>() { firstRect[i], secondRect[j] };


                                    // This means that the lines are overlapping
                                    if (LineUtils.GetIntersectIndex(pointsList[0], lines) != LineUtils.GetIntersectIndex(pointsList[1], lines))
                                    {
                                    isOverlap = true;

                                        firstRect[i] = new InputLine(pointsList[0], pointsList[1]);

                                        secondRect[j] = new InputLine(pointsList[2], pointsList[3]);

                                        // consume the second rect 
                                        firstRect.AddRange(secondRect);

                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            TaskDialog.Show("Error","Unhandled internal hatch shape detected");
                        }
                    }

                }
            }
            return isOverlap;
        }
    }
}

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class HallwayGenerator
    {
        private readonly Document mDocument;

        private List<List<InputLine>> CurveLinesExternal;

        private List<List<InputLine>> CurveLinesInternal;

        public HallwayGenerator(ref Document doc)
        {
            mDocument = doc;

            // list to gather external boundary lines of the final hatches 
            CurveLinesExternal = new List<List<InputLine>>();

            // list to gather internal boudary lines of the final hatches 
            CurveLinesInternal = new List<List<InputLine>>();

            var filledRegion = new FilteredElementCollector(mDocument).OfClass(typeof(FilledRegionType));
            foreach (var region in filledRegion)
            {
                if (region.Name == "Obstinate Orange")
                    externalHatchId = region.Id;
                if(region.Name == "Nuture Green")
                    internalHatchId = region.Id;
            }

            CollectHatchLines();
        }

        private ElementId externalHatchId;

        private ElementId internalHatchId;

        public void CollectHatchLines()
        {
            // collect all the filled regions
            FilteredElementCollector collector = new FilteredElementCollector(mDocument, mDocument.ActiveView.Id);
            ICollection<Element> filledRegionCollector = collector
                .OfClass(typeof(FilledRegion))
                .ToElements();

            // Collect the filled region types of the Detail Filled Region type
            foreach (Element filledRegionElement in filledRegionCollector)
            {
                if (filledRegionElement.IsHidden(mDocument.ActiveView))
                    continue;

                FilledRegion filledRegion = filledRegionElement as FilledRegion;

                // gather external hatches
                if (filledRegion != null && filledRegion.GetTypeId() == externalHatchId)
                {
                    var curveLoops = filledRegion.GetBoundaries();
                    List<InputLine> curveLines = new List<InputLine>();

                    foreach (var curveLoop in curveLoops)
                    {
                        IEnumerator<Curve> curveEnumerator = curveLoop.GetEnumerator();
                        while (curveEnumerator.MoveNext())
                        {
                            curveLines.Add(new InputLine(curveEnumerator.Current.GetEndPoint(0),curveEnumerator.Current.GetEndPoint(1)));
                        }
                    }

                    CurveLinesExternal.Add(curveLines);
                }

                // gather internal hatches
                if (filledRegion != null && filledRegion.GetTypeId() == internalHatchId)
                {
                    var curveLoops = filledRegion.GetBoundaries();
                    List<InputLine> curveLines = new List<InputLine>();

                    foreach (var curveLoop in curveLoops)
                    {
                        IEnumerator<Curve> curveEnumerator = curveLoop.GetEnumerator();
                        while (curveEnumerator.MoveNext())
                        {
                            curveLines.Add(new InputLine(curveEnumerator.Current.GetEndPoint(0), curveEnumerator.Current.GetEndPoint(1)));
                        }
                    }

                    CurveLinesInternal.Add(curveLines);
                }
            }

            //FileWriter.WriteInputListToFile(CurveLinesExternal, @"C:\temp\all_curve_lines");

            JoinHatchLines(ref CurveLinesExternal);

            JoinHatchLines(ref CurveLinesInternal);

            //FileWriter.WriteInputListToFile(CurveLinesExternal, @"C:\temp\joined_curve_lines");

            PostProcessCurves(ref CurveLinesExternal);

            PostProcessCurves(ref CurveLinesInternal);


            FileWriter.WriteInputListToFile(CurveLinesExternal, @"C:\temp\external_curve_lines");

            FileWriter.WriteInputListToFile(CurveLinesInternal, @"C:\temp\internal_curve_lines");

        }

        public void JoinHatchLines(ref List<List<InputLine>> curveLines)
        {
            int firstIndex = 0;
            while( firstIndex < curveLines.Count)
            {
                var firstList = curveLines[firstIndex];
                int secondIndex = -1;
                for( int i = firstIndex + 1; i < curveLines.Count; i++)
                {
                    var secondList = curveLines[i];
                    if(AreCurveLinesSame(firstList, secondList))
                    {
                        secondIndex = i;
                        var tempList = JoinCurves(firstList, secondList);
                        curveLines[firstIndex] = tempList;
                        break;
                    }
                }
                if(secondIndex != -1)
                {
                    curveLines.RemoveAt(secondIndex);
                }
                else
                {
                    firstIndex++;
                }
            }
        }

        private List<InputLine> JoinCurves ( List<InputLine> first, List<InputLine> second)
        {
            var resultList = new List<InputLine>();
            resultList.AddRange(first);
            resultList.AddRange(second);
            resultList.Sort();

            // detect the complete intersections
            int deleteIndex1 = -1;
            int deleteIndex2 = -1;
            LineType overlapType = LineType.INVALID; // this is either horizontal or vertical 
                                    
            LineType intersectLineType = LineType.INVALID;

            for ( int i = 0; i < resultList.Count - 1; i++)
            {
                for (int j = i+1; j < resultList.Count - 1; j++)
                {
                    if (LineUtils.AreLinesEqual(resultList[i], resultList[j]))
                    {
                        deleteIndex1 = i;
                        deleteIndex2 = j;
                        intersectLineType = InputLine.GetLineType(resultList[i]);
                        break;
                    }
                }
            }

            if (deleteIndex1 != -1)
            {
                // remove the intersecting lines
                resultList.RemoveAt(deleteIndex2); // first remove the last line then 
                resultList.RemoveAt(deleteIndex1); // remove the first line, this ensures that the deleting is done correctly
            }

            // join vertical lines if the intersecting lines are horizontal
            if (intersectLineType == LineType.HORIZONTAL)
            {
                int firstIndex = 0;
                while(firstIndex < resultList.Count)
                {
                    var firstVer = resultList[firstIndex];
                    if(InputLine.GetLineType(firstVer) != LineType.VERTICAL)
                    {
                        firstIndex++;
                        continue;
                    }
                    else
                    {
                        int removeIndex = -1;
                        for(int i = firstIndex + 1; i < resultList.Count; i++)
                        {
                            var secondVer = resultList[i];
                            if (InputLine.GetLineType(resultList[i]) == LineType.VERTICAL)
                            {
                                // a line's ending is equal to the starting point of another line, join them
                                if(PointUtils.AreEqual(firstVer.end,secondVer.start))
                                {
                                    resultList[firstIndex] = new InputLine(firstVer.start, secondVer.end);
                                    removeIndex = i;
                                    break;
                                }

                                //lines are overlapping , we just have to consider the bigger rectangle here
                                if(PointUtils.AreEqual(firstVer.start,secondVer.start) || PointUtils.AreEqual(firstVer.end, secondVer.end))
                                {
                                    overlapType = LineType.VERTICAL;
                                    break;
                                }
                            }
                        }
                        if(removeIndex != -1)
                            resultList.RemoveAt(removeIndex);
                        else
                            firstIndex++;
                    }
                }
            }
            // join vertical lines if the intersecting lines are vertical
            else if (intersectLineType == LineType.VERTICAL)
            {
                int firstIndex = 0;
                while (firstIndex < resultList.Count)
                {
                    var firstHor = resultList[firstIndex];
                    if (InputLine.GetLineType(firstHor) != LineType.HORIZONTAL)
                    {
                        firstIndex++;
                        continue;
                    }
                    else
                    {
                        int removeIndex = -1;
                        for (int i = firstIndex + 1; i < resultList.Count; i++)
                        {
                            var secondHor = resultList[i];
                            if (InputLine.GetLineType(resultList[i]) == LineType.HORIZONTAL)
                            {
                                if (PointUtils.AreEqual(firstHor.end,secondHor.start))
                                {
                                    resultList[firstIndex] = new InputLine(firstHor.start, secondHor.end);
                                    removeIndex = i;
                                    break;
                                }

                                //lines are overlapping , we just have to consider the bigger rectangle here
                                if (PointUtils.AreEqual(firstHor.start, secondHor.start) || PointUtils.AreEqual(firstHor.end, secondHor.end))
                                {
                                    overlapType = LineType.HORIZONTAL;
                                    break;
                                }
                            }
                        }
                        if (removeIndex != -1)
                            resultList.RemoveAt(removeIndex);
                        else
                            firstIndex++;
                    }
                }
            }

            if(overlapType != LineType.INVALID) 
            {
                resultList .Clear();
                resultList = GetLargestRect(first, second, overlapType);
            }

            return resultList;
        }

        private List<InputLine> JoinAlmostSameCurves(List<InputLine> first, List<InputLine> second)
        {
            var resultList = new List<InputLine>();
            resultList.AddRange(first);
            resultList.AddRange(second);
            resultList.Sort();

            // detect the complete intersections
            int deleteIndex1 = -1;
            int deleteIndex2 = -1;

            LineType intersectLineType = LineType.INVALID;

            for (int i = 0; i < resultList.Count - 1; i++)
            {
                for (int j = i + 1; j < resultList.Count - 1; j++)
                {
                    if (LineUtils.AreLinesAlmostEqual(resultList[i], resultList[j]))
                    {
                        deleteIndex1 = i;
                        deleteIndex2 = j;
                        intersectLineType = InputLine.GetLineType(resultList[i]);
                        break;
                    }
                }
            }

            if (deleteIndex1 != -1)
            {
                // remove the intersecting lines
                resultList.RemoveAt(deleteIndex2); // first remove the last line then 
                resultList.RemoveAt(deleteIndex1); // remove the first line, this ensures that the deleting is done correctly
            }

            // join vertical lines if the intersecting lines are horizontal
            if (intersectLineType == LineType.HORIZONTAL)
            {
                int firstIndex = 0;
                double minX = GetMinX(resultList);
                double maxX = GetMaxX(resultList);

                while (firstIndex < resultList.Count)
                {
                    var firstVer = resultList[firstIndex];
                    if (InputLine.GetLineType(firstVer) != LineType.VERTICAL)
                    {
                        firstIndex++;
                        continue;
                    }
                    else
                    {
                        int removeIndex = -1;
                        for (int i = firstIndex + 1; i < resultList.Count; i++)
                        {
                            var secondVer = resultList[i];
                            if (InputLine.GetLineType(resultList[i]) == LineType.VERTICAL)
                            {
                                // a line's ending is equal to the starting point of another line, join them
                                if (PointUtils.AreAlmostEqual(firstVer.end, secondVer.start))
                                {
                                    var xCordinate = Math.Abs(firstVer.start.X - minX) < Math.Abs(firstVer.start.X - maxX) ? minX : maxX;
                                    var newStart = new XYZ(xCordinate, firstVer.start.Y, firstVer.start.Z);
                                    var newEnd = new XYZ(xCordinate, secondVer.end.Y, secondVer.end.Z);
                                    resultList[firstIndex] = new InputLine(newStart, newEnd);
                                    removeIndex = i;
                                    break;
                                }
                            }
                        }
                        if (removeIndex != -1)
                            resultList.RemoveAt(removeIndex);
                        else
                            firstIndex++;
                    }
                }
            }
            // join vertical lines if the intersecting lines are vertical
            else if (intersectLineType == LineType.VERTICAL)
            {
                int firstIndex = 0;

                double minY = GetMinY(resultList);
                double maxY = GetMaxY(resultList);

                while (firstIndex < resultList.Count)
                {
                    var firstHor = resultList[firstIndex];
                    if (InputLine.GetLineType(firstHor) != LineType.HORIZONTAL)
                    {
                        firstIndex++;
                        continue;
                    }
                    else
                    {
                        int removeIndex = -1;
                        for (int i = firstIndex + 1; i < resultList.Count; i++)
                        {
                            var secondHor = resultList[i];
                            if (InputLine.GetLineType(resultList[i]) == LineType.HORIZONTAL)
                            {
                                if (PointUtils.AreAlmostEqual(firstHor.end, secondHor.start))
                                {
                                    var yCordinate = Math.Abs(firstHor.start.Y - minY) < Math.Abs(firstHor.start.Y - maxY) ? minY : maxY;
                                    var newStart = new XYZ(firstHor.start.X, yCordinate, firstHor.start.Z);
                                    var newEnd = new XYZ(secondHor.end.X, yCordinate, secondHor.end.Z);
                                    resultList[firstIndex] = new InputLine(newStart, newEnd);
                                    removeIndex = i;
                                    break;
                                }
                            }
                        }
                        if (removeIndex != -1)
                            resultList.RemoveAt(removeIndex);
                        else
                            firstIndex++;
                    }
                }
            }

            return resultList;
        }

        private void PostProcessCurves(ref List<List<InputLine>> curveLines)
        {
            int firstIndex = 0;
            while (firstIndex < curveLines.Count)
            {
                var firstList = curveLines[firstIndex];
                int secondIndex = -1;
                for (int i = firstIndex + 1; i < curveLines.Count; i++)
                {
                    var secondList = curveLines[i];
                    if (AreCurveLinesAlmostSame(firstList, secondList))
                    {
                        secondIndex = i;
                        var tempList = JoinAlmostSameCurves(firstList, secondList);
                        curveLines[firstIndex] = tempList;
                        break;
                    }
                }
                if (secondIndex != -1)
                {
                    curveLines.RemoveAt(secondIndex);
                }
                else
                {
                    firstIndex++;
                }
            }
        }

        private bool AreCurveLinesSame(List<InputLine> firstList, List<InputLine> secondList )
        {
            for (int i = 0; i < firstList.Count; i++)
            {
                for (int j = 0; j < secondList.Count; j++)
                {
                    if (LineUtils.AreLinesEqual(firstList[i],secondList[j]))
                        return true;
                }
            }
            return false;
        }

        // has a precision of 1
        private bool AreCurveLinesAlmostSame(List<InputLine> firstList, List<InputLine> secondList)
        {
            for (int i = 0; i < firstList.Count; i++)
            {
                for (int j = 0; j < secondList.Count; j++)
                {
                    if (LineUtils.AreLinesAlmostEqual(firstList[i], secondList[j]))
                        return true;
                }
            }
            return false;
        }


        private List<InputLine> GetLargestRect(List<InputLine> firstList, List<InputLine> secondList, LineType overlapType)
        {
            double maxFirstLength = 0.0f;
            double maxSecondLength = 0.0f;

            foreach (var line in firstList)
            {
                if (InputLine.GetLineType(line) == overlapType)
                {
                    var length = GetLineLength(line,overlapType);
                    if( length > maxFirstLength)
                    {
                        maxFirstLength = length;
                    }
                }
            }

            foreach (var line in secondList)
            {
                if (InputLine.GetLineType(line) == overlapType)
                {
                    var length = GetLineLength(line, overlapType);
                    if (length > maxSecondLength)
                    {
                        maxSecondLength = length;
                    }
                }
            }
            if (maxFirstLength > maxSecondLength)
                return firstList;
            else
                return secondList;
        }

        private double GetLineLength(InputLine line, LineType lineType) 
        {
            if (lineType == LineType.HORIZONTAL)
            {
                return Math.Abs(line.start.X - line.end.X);
            }
            else if (lineType == LineType.VERTICAL)
            {
                return Math.Abs(line.start.Y - line.end.Y);
            }
            return 0.0f;
        }

        private double GetMinY(List<InputLine> lines)
        {
            double min = double.PositiveInfinity;
            foreach(var line in lines)
            {
                min = Math.Min(min, line.start.Y);
            }
            return min;
        }

        private double GetMaxY(List<InputLine> lines)
        {
            double max = double.NegativeInfinity;
            foreach (var line in lines)
            {
                max = Math.Max(max, line.end.Y);
            }
            return max;
        }

        private double GetMinX(List<InputLine> lines)
        {
            double min = double.PositiveInfinity;
            foreach (var line in lines)
            {
                min = Math.Min(min, line.start.X);
            }
            return min;
        }

        private double GetMaxX(List<InputLine> lines)
        {
            double max = double.NegativeInfinity;
            foreach (var line in lines)
            {
                max = Math.Max(max, line.end.X);
            }
            return max;
        }
    }
}

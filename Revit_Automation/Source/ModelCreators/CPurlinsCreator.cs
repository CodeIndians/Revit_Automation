using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Windows.Documents;

namespace Revit_Automation
{
    internal class CPurlinsCreator
    {
        private Document doc;
        private Form1 form;
        private Transaction m_Transaction;

        public CPurlinsCreator(Document doc, Form1 form)
        {
            this.doc = doc;
            this.form = form;
        }

        internal void CreateModel(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            // Purlins will run perpendicular to the LB lines in a given layout
            using (Transaction tx = new Transaction(doc))
            {
                form.PostMessage("");
                form.PostMessage("\n Placing Purlins");
                m_Transaction = tx;
                tx.Start("Placing Purlins");

                //// Get purlin settings specific to the roofs present in the given view.
                //List<PurlinTypeSettings> purlinSettings = GlobalSettings.framingSettings.purlinSettings;
                //List<(RoofObject, string, PurlinTypeSettings)> lst = new System.Collections.Generic.List<(RoofObject, string, PurlinTypeSettings)>();

                //// This method assumes that the level Low eve is the only one that has building name in it.
                //// depending on the levels having the building name parameter
                //Level deckElevation = GetPurlinElevation(colInputLines[0], levels);

                //// Intervals at which purlins should be placed. with the given overlap value                
                //List<double> spanIntervals = ComputeSpanIntervals(colInputLines, dMaximumSpans);

                //// Purlin running direction is perpendicular to slope direction
                //LineType deckingDirection = ComputePurlinDirection(colInputLines);

                //// Place purlins according to decking direction
                //PlacePurlins(colInputLines, deckingDirection, spanIntervals, deckElevation);

                form.PostMessage("\n Finished Placing Purlins");
                tx.Commit();
            }
        }

        private Level GetPurlinElevation(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
            Level level = null;

            // Filter levels based on buldings to use
            List<Level> filteredLevels = new List<Level>();
            foreach (Level filteredlevel in levels)
            {
                if (filteredlevel.Name.Contains(inputLine.strBuildingName))
                {
                    filteredLevels.Add(filteredlevel);
                }
            }

            for (int i = 0; i < filteredLevels.Count() - 1; i++)
            {
                Level tempLevel = filteredLevels.ElementAt(i);

                if ((inputLine.startpoint.Z < (tempLevel.Elevation + 1)) && (inputLine.startpoint.Z > (tempLevel.Elevation - 1)))
                {
                    Level toplevel = filteredLevels.ElementAt(i + 1);
                    level = toplevel;
                }
            }

            return level;
        }

        private void PlacePurlins(List<InputLine> colInputLines, LineType deckingDirection, List<double> spanIntervals, Level deckElevation)
        {
            
        }
        
        private List<double> ComputeSpanIntervals(List<InputLine> colInputLines)
        {
            List<double> spanIntervals = new List<double>();

            //double dElevation = GetInputLineElevation(colInputLines[0]);

            //LineType slopeLineType = LineType.vertical;

            //foreach (InputLine line in colInputLines)
            //{
            //    if (line.strWallType == "LB" || line.strWallType == "LBS")
            //    {
            //        slopeLineType = GenericUtils.GetLineType(line);
            //        break;
            //    }
            //}

            //// if the span grid is horizontal we have a horizontal LBs and if the span grid is vertical we have vertical LBs
            //Grid northBoundary = lstBoundaries[0];
            //Grid southBoundary = lstBoundaries[1];
            //Grid eastBoundary = lstBoundaries[2];
            //Grid westBoudary = lstBoundaries[3];
            //Grid SpanStartingGrid = lstBoundaries[4];

            //XYZ spanStart1 = null, spanStart2 = null;
            //GenericUtils.GetlineStartAndEndPoints(SpanStartingGrid.Curve, out spanStart1, out spanStart2);

            //if (slopeLineType == LineType.vertical)
            //{
            //    XYZ spanEndPoint1 = null, spanEndPoint2 = null;
            //    GenericUtils.GetlineStartAndEndPoints(eastBoundary.Curve, out spanEndPoint1, out spanEndPoint2);

            //    XYZ minBoundary1 = null, minBoundary2 = null;
            //    GenericUtils.GetlineStartAndEndPoints(southBoundary.Curve, out minBoundary1, out minBoundary2);

            //    XYZ maxBoundary1 = null, maxBoundary2 = null;
            //    GenericUtils.GetlineStartAndEndPoints(northBoundary.Curve, out maxBoundary1, out maxBoundary2);

            //    XYZ currentPoint = spanStart1;

            //    while (currentPoint.X < spanEndPoint1.X)
            //    {
            //        bool bFoundLBLines = false;

            //        currentPoint = new XYZ(currentPoint.X + 10 * m_CompositeDeckSpans, currentPoint.Y, currentPoint.Z);

            //        while (!bFoundLBLines)
            //        {
            //            XYZ min = new XYZ(currentPoint.X - 0.5, minBoundary1.Y, dElevation - 0.5);
            //            XYZ max = new XYZ(currentPoint.X + 0.5, maxBoundary1.Y, dElevation + 0.5);

            //            Outline outline = new Outline(min, max);

            //            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

            //            FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel);

            //            ICollection<Element> inputLineElems = collector.ToElements();

            //            foreach (Element elem in inputLineElems)
            //            {
            //                string tempString = string.Empty;
            //                Parameter param = elem.LookupParameter("Wall Type");
            //                if (param != null)
            //                {
            //                    tempString = param.AsString();
            //                }
            //                if (!string.IsNullOrEmpty(tempString) && (tempString == "LB" || tempString == "LBS"))
            //                {
            //                    bFoundLBLines = true;
            //                    XYZ startPt = null, endPt = null;
            //                    GenericUtils.GetlineStartAndEndPoints(elem, out startPt, out endPt);
            //                    spanIntervals.Add(startPt.X);
            //                    break;
            //                }
            //            }

            //            if (!bFoundLBLines && currentPoint.X < spanEndPoint1.X) // we shouldn't count back if we reached the end of the building
            //                currentPoint = new XYZ(currentPoint.X - 1, currentPoint.Y, currentPoint.Z);

            //            if (currentPoint.X > spanEndPoint1.X)
            //                break;
            //        }

            //    }

            //}
            //else
            //{
            //    XYZ spanEndPoint1 = null, spanEndPoint2 = null;
            //    GenericUtils.GetlineStartAndEndPoints(northBoundary.Curve, out spanEndPoint1, out spanEndPoint2);

            //    XYZ minBoundary1 = null, minBoundary2 = null;
            //    GenericUtils.GetlineStartAndEndPoints(westBoudary.Curve, out minBoundary1, out minBoundary2);

            //    XYZ maxBoundary1 = null, maxBoundary2 = null;
            //    GenericUtils.GetlineStartAndEndPoints(eastBoundary.Curve, out maxBoundary1, out maxBoundary2);

            //    XYZ currentPoint = spanStart1;

            //    while (currentPoint.Y < spanEndPoint1.Y)
            //    {
            //        bool bFoundLBLines = false;

            //        currentPoint = new XYZ(currentPoint.X, currentPoint.Y + 10 * m_CompositeDeckSpans, currentPoint.Z);

            //        while (!bFoundLBLines)
            //        {
            //            XYZ min = new XYZ(minBoundary1.X, currentPoint.Y - 0.5, dElevation - 0.5);
            //            XYZ max = new XYZ(maxBoundary1.X, currentPoint.Y + 0.5, dElevation + 0.5);

            //            Outline outline = new Outline(min, max);

            //            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

            //            FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel);

            //            ICollection<Element> inputLineElems = collector.ToElements();

            //            foreach (Element elem in inputLineElems)
            //            {
            //                string tempString = string.Empty;
            //                Parameter param = elem.LookupParameter("Wall Type");
            //                if (param != null)
            //                {
            //                    tempString = param.AsString();
            //                }
            //                if (!string.IsNullOrEmpty(tempString) && (tempString == "LB" || tempString == "LBS"))
            //                {
            //                    bFoundLBLines = true;
            //                    XYZ startPt = null, endPt = null;
            //                    GenericUtils.GetlineStartAndEndPoints(elem, out startPt, out endPt);
            //                    spanIntervals.Add(startPt.Y);
            //                    break;
            //                }
            //            }

            //            if (!bFoundLBLines && currentPoint.Y < spanEndPoint1.Y) // we shouldn't count back if we reached the end of the building
            //                currentPoint = new XYZ(currentPoint.X, currentPoint.Y - 1, currentPoint.Z);

            //            if (currentPoint.Y > spanEndPoint1.Y)
            //                break;
            //        }
            //    }
            //}

            return spanIntervals;
        }
        
        private LineType ComputePurlinDirection(List<InputLine> colInputLines)
        {
            LineType deckingDirection = LineType.vertical;
            LineType slopeLineType = LineType.vertical;

            foreach (InputLine line in colInputLines)
            {
                if (line.strWallType == "LB" || line.strWallType == "LBS")
                {
                    slopeLineType = GenericUtils.GetLineType(line);
                    break;
                }
            }

            // Decking direction is always perpendicular to slope direction
            deckingDirection = slopeLineType == LineType.vertical ? LineType.Horizontal : LineType.vertical;

            return deckingDirection;
        }
    }
}
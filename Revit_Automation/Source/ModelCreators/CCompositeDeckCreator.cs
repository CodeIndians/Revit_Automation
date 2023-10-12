using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Structure;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Revit_Automation
{
    internal class CCompositeDeckCreator
    {
        public static List<Grid> lstBoundaries = new List<Grid>();
        private Document doc;
        private Form1 form;
        private string m_CompositeDeckType;
        private double m_CompositeDeckSpans;
        private double m_CompositeDeckMaxLength;
        private double m_CompositeDeckOverlap;
        public CCompositeDeckCreator(Document doc, Form1 form)
        {
            this.doc = doc;
            this.form = form;
            m_CompositeDeckType = GlobalSettings.framingSettings.strFloorDeckType;
            m_CompositeDeckSpans = GlobalSettings.framingSettings.dFloorDeckMaxSpan;
            m_CompositeDeckMaxLength = GlobalSettings.framingSettings.dFloorDeckMaxLength;
            m_CompositeDeckOverlap = GlobalSettings.framingSettings.dFloorDeckOverlap;
        }

        internal void CreateModel(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Placing Composite Decks");

                Level deckElevation = GetDeckElevation(colInputLines[0], levels);
                // Intervals at which decks should be placed. with the given overlap value
                List<double> spanIntervals = ComputeSpanIntervals(colInputLines);

                // Decking direction is perpendicular to slope direction
                LineType deckingDirection = ComputeDeckingDirection(colInputLines);

                // Place decks according to decking direction
                PlaceFloorDecks(colInputLines, deckingDirection, spanIntervals, deckElevation);

                // Trim the deck as per the slab boundary
                //TrimDeckPanels();

                // Compute the span start and end points.
                tx.Commit();
            }
        }

        private void PlaceFloorDecks(List<InputLine> colInputLines, LineType deckingDirection, List<double> spanIntervals, Level deckElevation)
        {
            
            // if the span grid is horizontal we have a horizontal LBs and if the span grid is vertical we have vertical LBs
            Grid northBoundary = lstBoundaries[0];
            Grid southBoundary = lstBoundaries[1];
            Grid eastBoundary = lstBoundaries[2];
            Grid westBoudary = lstBoundaries[3];
            Grid SpanStartingGrid = lstBoundaries[4];

            XYZ northGridStartPt = null, northGridEndPt = null;
            GenericUtils.GetlineStartAndEndPoints(northBoundary.Curve, out northGridStartPt, out northGridEndPt);

            XYZ SouthGridStartPt = null, SouthGridEndPt = null;
            GenericUtils.GetlineStartAndEndPoints(southBoundary.Curve, out SouthGridStartPt, out SouthGridEndPt);

            XYZ EastGridStartPt = null, EastGridEndPt = null;
            GenericUtils.GetlineStartAndEndPoints(eastBoundary.Curve, out EastGridStartPt, out EastGridEndPt);

            XYZ WestGridStartPt = null, WestGridEndPt = null;
            GenericUtils.GetlineStartAndEndPoints(westBoudary.Curve, out WestGridStartPt, out WestGridEndPt);

            if (deckingDirection == LineType.Horizontal)
            {
                // TODO : Get the family type parameter of given typestart point is south grid and end point is north grid
                double dWidth = GetDeckWidth(m_CompositeDeckType);

                // Couldn't retrieve the deck width. For now hardcoding it to 3.0 feet
                if (dWidth == 0)
                    dWidth = 3.0;


                XYZ currentPoint = SouthGridStartPt;
                while (currentPoint.Y < northGridStartPt.Y)
                {
                    double minX = 10000, maxX = -100000;
                    XYZ min = new XYZ(WestGridStartPt.X, currentPoint.Y, WestGridStartPt.Z - 0.5);
                    XYZ max = new XYZ(EastGridStartPt.X, currentPoint.Y + 3, EastGridStartPt.Z + 0.5);

                    Outline outline = new Outline(min, max);

                    BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

                    FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel);

                    ICollection<Element> inputLineElems = collector.ToElements();

                    foreach (Element elem in inputLineElems)
                    {
                        Parameter param = elem.LookupParameter("Wall Type");
                        if (param != null)
                        {
                            string strWalltype = param.AsString();
                            if (strWalltype == "Ex" || strWalltype == "Ex Opening" || strWalltype == "Ex w/ Insulation")
                            {
                                XYZ startPt = null, endPt = null;
                                GenericUtils.GetlineStartAndEndPoints(elem, out startPt, out endPt);

                                // if the line is inclined then intersect the line with the range bottom line and top line and take them as start and end

                                if (!(MathUtils.ApproximatelyEqual(startPt.X, endPt.X) || MathUtils.ApproximatelyEqual(startPt.Y, endPt.Y)))
                                {
                                    Line exLine = Line.CreateBound (startPt, endPt);

                                    // We have inclined lines intersect with range lines
                                    XYZ min1 = new XYZ (min.X, min.Y, startPt.Z);
                                    XYZ min2 = new  XYZ(max.X, min.Y, startPt.Z); 
                                    Line minLine = Line.CreateBound(min1, min2);

                                    XYZ max1 = new XYZ(min.X, max.Y, startPt.Z);
                                    XYZ max2 = new XYZ (max.X, max.Y, startPt.Z);
                                    Line maxLine = Line.CreateBound(max1, max2);

                                    for (int i = 0; i < 2; i++)
                                    {
                                        Line temp = i == 0 ? minLine : maxLine;
                                        IntersectionResultArray intersectionResults = new IntersectionResultArray();
                                        SetComparisonResult result = exLine.Intersect(temp, out intersectionResults);
                                        if (result == SetComparisonResult.Overlap)
                                        {
                                            foreach (IntersectionResult intersectionResult in intersectionResults)
                                            { 
                                                if (i == 0)
                                                    startPt = intersectionResult.XYZPoint;
                                                else
                                                    endPt = intersectionResult.XYZPoint;
                                            }
                                        }
                                    }
                                }

                                if (minX > startPt.X)
                                    minX = startPt.X;
                                if (maxX < endPt.X)
                                    maxX = endPt.X;
                            }
                        }
                    }

                    double startlocation = minX < WestGridStartPt.X ? WestGridStartPt.X : minX;
                    maxX = Math.Min (maxX, EastGridEndPt.X);

                    while (startlocation < maxX)
                    {
                        // Get nearest span point for overlap
                        double endLocation = Math.Min(spanIntervals.Find(value => value > startlocation), maxX);
                        if (endLocation == 0)
                            endLocation = maxX;

                        // Deck get placed from top to bottom, so we need to give the top 2 points
                        XYZ startPoint = new XYZ(startlocation - m_CompositeDeckOverlap / 2.0, currentPoint.Y + 3, deckElevation.Elevation);
                        XYZ endPoint = new XYZ(endLocation + m_CompositeDeckOverlap / 2.0, currentPoint.Y + 3, deckElevation.Elevation);

                        bool bCanPlaceDeck = CheckIfDeckCanBePlaced(startPoint, endPoint, deckingDirection);
                        
                        if (bCanPlaceDeck)
                        {
                            if (Math.Abs(startPoint.X - endPoint.X) < 0.5)
                                continue;

                            Line newInputLine = Line.CreateBound(startPoint, endPoint);

                            FamilySymbol symbol = SymbolCollector.GetCompositeDeckSymbol(m_CompositeDeckType, "Composite Deck");

                            if (symbol != null && !symbol.IsActive)
                                symbol.Activate();


                            FamilyInstance compositeDeckInstance = doc.Create.NewFamilyInstance(newInputLine, symbol, deckElevation, StructuralType.Beam);

                            StructuralFramingUtils.DisallowJoinAtEnd(compositeDeckInstance, 0);

                            StructuralFramingUtils.DisallowJoinAtEnd(compositeDeckInstance, 1);

                            form.PostMessage(string.Format(" Placing Composite Deck ID : {0} \n", compositeDeckInstance.Id));

                        }
                        
                        startlocation = endLocation;

                    }

                    // We have the composite deck starting and ending points - move to the next location
                    currentPoint = new XYZ(currentPoint.X, currentPoint.Y + 3, currentPoint.Z);
                }
            }
            else
            { 
                //TO-DO the same code for vertical deck placement
            }
        }


        //private void ModifyDeckForOpenings(FamilyInstance compositeDeckInstance, XYZ startPoint, XYZ endPoint)
        //{
        //    Element elem = compositeDeckInstance as Element;

        //    startPoint = new XYZ(startPoint.X, startPoint.Y - 3.0 /*Deck Thickness - this might change*/, startPoint.Z - 0.5);
        //    endPoint = new XYZ(endPoint.X, endPoint.Y, endPoint.Z + 0.5);

        //    Outline outline = new Outline(startPoint,endPoint);

        //    BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);
        //    FilteredElementCollector exLinesCollector = new FilteredElementCollector(doc).WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel);

        //    ICollection<Element> exLines = exLinesCollector.ToElements();

        //    IList<Element> targetLines = new List<Element>();
        //    foreach (Element inputLineElem in exLines)
        //    {
        //        Parameter param = inputLineElem.LookupParameter("Wall Type");
        //        if (param != null)
        //        {
        //            string strWalltype = param.AsString();
        //            if (strWalltype == "Ex" || strWalltype == "Ex w/ Insulation" || strWalltype == "Ex Opening")
        //            {
        //                targetLines.Add(inputLineElem);
        //            }
        //        }
        //    }

        //    if (targetLines.Count > 0)
        //    {
        //        TrimDeckPanel(elem, targetLines);
        //    }
        //}

        //private void TrimDeckPanel(Element elem, IList<Element> targetLines)
        //{
        //    if (targetLines.Count == 1)
        //    {
        //    }
        //    else if (targetLines.Count == 2)
        //    {
        //    }
        //    else if (targetLines.Count == 3)
        //    { 
        //    }
        //}

        private bool CheckIfDeckCanBePlaced(XYZ startPoint, XYZ endPoint, LineType deckingDirection)
        {

            return true;

            // TO-DO: The condition whether deck can be placed is that, there should be LB lines or Cee Headers at both start and end

        }

        private double GetDeckWidth(string m_CompositeDeckType)
        {
            // Get Family symbol based on type
            Element deckElement = SymbolCollector.GetCompositeDeckSymbol (m_CompositeDeckType, "Composite Deck") as Element;
            
            if (deckElement != null)
            {
                IList<Parameter> paramsss = deckElement.GetOrderedParameters();
                
                Parameter param = deckElement.LookupParameter("Width");
                if (param != null)
                {
                    return param.AsDouble();
                }
            }

            return 0.0;
        }

        private LineType ComputeDeckingDirection(List<InputLine> colInputLines)
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

        private List<double> ComputeSpanIntervals(List<InputLine> colInputLines)
        {
            List<double> spanIntervals = new List<double>();

            double dElevation = GetInputLineElevation(colInputLines[0]);
            
            LineType slopeLineType = LineType.vertical;

            foreach (InputLine line in colInputLines)
            {
                if (line.strWallType == "LB" || line.strWallType == "LBS")
                {
                    slopeLineType = GenericUtils.GetLineType(line);
                    break;
                }
            }

            // if the span grid is horizontal we have a horizontal LBs and if the span grid is vertical we have vertical LBs
            Grid northBoundary = lstBoundaries[0];
            Grid southBoundary = lstBoundaries[1];
            Grid eastBoundary = lstBoundaries[2];
            Grid westBoudary = lstBoundaries[3];
            Grid SpanStartingGrid = lstBoundaries[4];

            XYZ spanStart1 = null, spanStart2 = null;
            GenericUtils.GetlineStartAndEndPoints(SpanStartingGrid.Curve, out spanStart1, out spanStart2);

            if (slopeLineType == LineType.vertical)
            {
                XYZ spanEndPoint1 = null, spanEndPoint2 = null;
                GenericUtils.GetlineStartAndEndPoints(eastBoundary.Curve, out spanEndPoint1, out spanEndPoint2);

                XYZ minBoundary1 = null, minBoundary2 = null;
                GenericUtils.GetlineStartAndEndPoints(southBoundary.Curve, out minBoundary1, out minBoundary2);

                XYZ maxBoundary1 = null, maxBoundary2 = null;
                GenericUtils.GetlineStartAndEndPoints(northBoundary.Curve, out maxBoundary1, out maxBoundary2);

                XYZ currentPoint = spanStart1;

                while (currentPoint.X < spanEndPoint1.X)
                {
                    bool bFoundLBLines = false;

                    currentPoint  = new XYZ(currentPoint.X + 10 * m_CompositeDeckSpans, currentPoint.Y, currentPoint.Z);
                    
                    while (!bFoundLBLines)
                    {
                        XYZ min = new XYZ(currentPoint.X - 0.5, minBoundary1.Y, currentPoint.Z - 0.5);
                        XYZ max = new XYZ(currentPoint.X + 0.5, maxBoundary1.Y, currentPoint.Z + 0.5);

                        Outline outline = new Outline(min, max);

                        BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

                        FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel);

                        ICollection<Element> inputLineElems = collector.ToElements();
                        
                        foreach (Element elem in inputLineElems)
                        {
                            string tempString = string.Empty;
                            Parameter param = elem.LookupParameter("Wall Type");
                            if (param != null)
                            {
                                tempString = param.AsString();
                            }
                            if (!string.IsNullOrEmpty(tempString) && (tempString == "LB" || tempString == "LBS"))
                            {
                                bFoundLBLines = true;
                                XYZ startPt = null, endPt = null;
                                GenericUtils.GetlineStartAndEndPoints(elem, out startPt, out endPt);
                                spanIntervals.Add(startPt.X);
                                break;
                            }
                        }

                        if (!bFoundLBLines && currentPoint.X < spanEndPoint1.X) // we shouldn't count back if we reached the end of the building
                            currentPoint = new XYZ(currentPoint.X - 1, currentPoint.Y, currentPoint.Z);

                        if (currentPoint.X > spanEndPoint1.X)
                            break;
                    }

                }

            }
            else
            {
                XYZ spanEndPoint1 = null, spanEndPoint2 = null;
                GenericUtils.GetlineStartAndEndPoints(northBoundary.Curve, out spanEndPoint1, out spanEndPoint2);

                XYZ minBoundary1 = null, minBoundary2 = null;
                GenericUtils.GetlineStartAndEndPoints(westBoudary.Curve, out minBoundary1, out minBoundary2);

                XYZ maxBoundary1 = null, maxBoundary2 = null;
                GenericUtils.GetlineStartAndEndPoints(eastBoundary.Curve, out maxBoundary1, out maxBoundary2);

                XYZ currentPoint = spanStart1;

                while (currentPoint.Y < spanEndPoint1.Y)
                {
                    bool bFoundLBLines = false;

                    currentPoint = new XYZ(currentPoint.X , currentPoint.Y + 10 * m_CompositeDeckSpans, currentPoint.Z);

                    while (!bFoundLBLines)
                    {
                        XYZ min = new XYZ(minBoundary1.X , currentPoint.Y - 0.5, currentPoint.Z - 0.5);
                        XYZ max = new XYZ(maxBoundary1.X , currentPoint.Y + 0.5, currentPoint.Z + 0.5);

                        Outline outline = new Outline(min, max);

                        BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

                        FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel);

                        ICollection<Element> inputLineElems = collector.ToElements();

                        foreach (Element elem in inputLineElems)
                        {
                            string tempString = string.Empty;
                            Parameter param = elem.LookupParameter("Wall Type");
                            if (param != null)
                            {
                                tempString = param.AsString();
                            }
                            if (!string.IsNullOrEmpty(tempString) && (tempString == "LB" || tempString == "LBS"))
                            {
                                bFoundLBLines = true;
                                XYZ startPt = null, endPt = null;
                                GenericUtils.GetlineStartAndEndPoints(elem, out startPt, out endPt);
                                spanIntervals.Add(startPt.Y);
                                break;
                            }
                        }

                        if (!bFoundLBLines && currentPoint.Y < spanEndPoint1.Y) // we shouldn't count back if we reached the end of the building
                            currentPoint = new XYZ(currentPoint.X , currentPoint.Y - 1, currentPoint.Z);

                        if (currentPoint.Y > spanEndPoint1.Y)
                            break;
                    }
                }
            }

            return spanIntervals;
        }

        private double GetInputLineElevation(InputLine inputLine)
        {
            XYZ startPt = null, endPt = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out startPt, out endPt);

            return startPt.Z;
        }

        private Level GetDeckElevation(InputLine inputLine, IOrderedEnumerable<Level> levels)
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
    }
}
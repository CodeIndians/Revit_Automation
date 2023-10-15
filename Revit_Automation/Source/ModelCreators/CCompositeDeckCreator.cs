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
        private Transaction m_Transaction;
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
                form.PostMessage("");
                form.PostMessage("\n Placing composite Decks");
                m_Transaction = tx; 
                tx.Start("Placing Composite Decks");

                Level deckElevation = GetDeckElevation(colInputLines[0], levels);
                
                // Intervals at which decks should be placed. with the given overlap value
                List<double> spanIntervals = ComputeSpanIntervals(colInputLines);

                // Decking direction is perpendicular to slope direction
                LineType deckingDirection = ComputeDeckingDirection(colInputLines);

                // Create void families
                CreateVoidFamilies(colInputLines[0], deckElevation);

                // Place decks according to decking direction
                PlaceFloorDecks(colInputLines, deckingDirection, spanIntervals, deckElevation);

                form.PostMessage("\n Finished Placing composite Decks");
                // Compute the span start and end points.
                tx.Commit();
            }
        }

        private void CreateVoidFamilies(InputLine inputLine, Level deckElevation)
        {
            // Get Floor Boundaries - we will get a collection of loops.
            Element floorElement =  GenericUtils.GetNearestFloorOrRoof(deckElevation, inputLine.startpoint, doc);

            Options opt = doc.Application.Create.NewGeometryOptions();
            List<List<XYZ>> floorCurves = GenericUtils.GetFloorCurves(floorElement, opt);

            foreach (List<XYZ> cureveLoop in floorCurves)
            {
                // Treat it like an internal opening
                if (cureveLoop.Count == 4)
                {
                    double dMinX = 100000, dMaxX = -100000, dMinY = 100000, dMaxY = -100000;
                    foreach (XYZ pt in cureveLoop)
                    {
                        if (dMaxX < pt.X)
                            dMaxX = pt.X;
                        if (dMinX > pt.X)
                            dMinX = pt.X;
                        if (dMaxY < pt.Y)
                            dMaxY = pt.Y;
                        if (dMinY > pt.Y)
                            dMinY = pt.Y;
                    }

                    // create a void family along the line
                    FamilySymbol voidSymbol = SymbolCollector.GetVoidFamilySymbol();

                    double dElevation = cureveLoop[0].Z;

                    if (voidSymbol != null && !voidSymbol.IsActive)
                        voidSymbol.Activate();

                    XYZ startPt = new XYZ(dMinX, dMaxY, dElevation);
                    XYZ endPt = new XYZ(dMinX, dMinY, dElevation);

                    while (startPt.X <  dMaxX - 3.0)
                    {
                        Curve line = Line.CreateBound(startPt, endPt) as Curve;

                        if (voidSymbol != null)
                        {
                            FamilyInstance voidInstance = doc.Create.NewFamilyInstance(line, voidSymbol, deckElevation, StructuralType.NonStructural);
                        }

                        startPt = startPt + new XYZ(3.0, 0, 0); // This is the width of the void family
                        endPt = endPt + new XYZ(3.0, 0, 0); // This is the width of the void family
                    }

                    startPt = new XYZ(dMaxX - 3.0, startPt.Y, startPt.Z);
                    endPt = new XYZ(dMaxX - 3.0, endPt.Y, endPt.Z);
                    Curve curve = Line.CreateBound(startPt, endPt) as Curve;

                    if (voidSymbol != null)
                    {
                        FamilyInstance voidInstance = doc.Create.NewFamilyInstance(curve, voidSymbol, deckElevation, StructuralType.NonStructural);
                    }

                    startPt = startPt + new XYZ(3.0, 0, 0); // This is the width of the void family
                }

                else
                {
                    for (int i = 0; i < cureveLoop.Count; i++)
                    {
                        // create a void family along the line
                        FamilySymbol voidSymbol = SymbolCollector.GetVoidFamilySymbol();


                        if (voidSymbol != null && !voidSymbol.IsActive)
                            voidSymbol.Activate();

                        XYZ startPt = null, endPt = null;

                        Curve line = null;
                        if (i == cureveLoop.Count - 1)
                        {
                            startPt = cureveLoop[i];
                            endPt = cureveLoop[0];
                        }
                        else
                        { 
                            startPt = cureveLoop[i];
                            endPt = cureveLoop[i + 1];
                        }

                        AdjustPoints(ref startPt, ref endPt, deckElevation);
                        line = Line.CreateBound(startPt, endPt) as Curve;

                        if (voidSymbol != null)
                        {
                            FamilyInstance voidInstance = doc.Create.NewFamilyInstance(line, voidSymbol, deckElevation, StructuralType.NonStructural);
                        }
                    }
                }
            }
            // Smaller ones are internal openings, so create voids along them
            // bigger loops are external, for each line compute the exterior side and place the void accordingly
        }

        private void AdjustPoints(ref XYZ startPt, ref XYZ endPt, Level deckElevation)
        {
            PanelDirection interiorSide = PanelDirection.L;

            interiorSide = GenericUtils.ComputeInteriorSide(startPt, endPt, deckElevation, doc, m_Transaction);

            if (interiorSide == PanelDirection.L)
            {
                //swap the points
                XYZ temp = startPt;
                startPt = endPt; endPt = temp;
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

                                LineType linetype = GenericUtils.GetLineType(elem);

                                if (linetype == deckingDirection)
                                    continue; 
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

                            // Trim the deck as per the slab boundary
                            //TrimDeckPanel(startPoint, endPoint, compositeDeckInstance);

                            //form.PostMessage(string.Format(" Placing Composite Deck ID : {0} \n", compositeDeckInstance.Id));

                        }
                        
                        startlocation = endLocation;

                    }

                    // We have the composite deck starting and ending points - move to the next location
                    currentPoint = new XYZ(currentPoint.X, currentPoint.Y + 3, currentPoint.Z);
                }
            }
            else
            {
                // TODO : Get the family type parameter of given typestart point is south grid and end point is north grid
                double dWidth = GetDeckWidth(m_CompositeDeckType);

                // Couldn't retrieve the deck width. For now hardcoding it to 3.0 feet
                if (dWidth == 0)
                    dWidth = 3.0;


                XYZ currentPoint = WestGridStartPt;
                while (currentPoint.X < EastGridStartPt.X)
                {
                    double minY = 10000, maxY = -100000;
                    XYZ min = new XYZ(currentPoint.X, SouthGridStartPt.Y, SouthGridStartPt.Z - 0.5);
                    XYZ max = new XYZ(currentPoint.X + 3, northGridStartPt.Y, northGridStartPt.Z + 0.5);

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

                                LineType linetype = GenericUtils.GetLineType(elem);

                                if (linetype == deckingDirection)
                                    continue;
                                // if the line is inclined then intersect the line with the range bottom line and top line and take them as start and end

                                if (!(MathUtils.ApproximatelyEqual(startPt.X, endPt.X) || MathUtils.ApproximatelyEqual(startPt.Y, endPt.Y)))
                                {
                                    Line exLine = Line.CreateBound(startPt, endPt);

                                    // We have inclined lines intersect with range lines
                                    XYZ min1 = new XYZ(min.X, min.Y, startPt.Z);
                                    XYZ min2 = new XYZ(max.X, min.Y, startPt.Z);
                                    Line minLine = Line.CreateBound(min1, min2);

                                    XYZ max1 = new XYZ(min.X, max.Y, startPt.Z);
                                    XYZ max2 = new XYZ(max.X, max.Y, startPt.Z);
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

                                if (minY > startPt.Y)
                                    minY = startPt.Y;
                                if (maxY < endPt.Y)
                                    maxY = endPt.Y;
                            }
                        }
                    }

                    double startlocation = minY < SouthGridStartPt.Y ? SouthGridStartPt.Y : minY;
                    maxY = Math.Min(maxY, northGridEndPt.Y);

                    while (startlocation < maxY)
                    {
                        // Get nearest span point for overlap
                        double endLocation = Math.Min(spanIntervals.Find(value => value > startlocation), maxY);
                        if (endLocation == 0)
                            endLocation = maxY;

                        // Deck get placed from top to bottom, so we need to give the top 2 points
                        XYZ startPoint = new XYZ(currentPoint.X , startlocation - m_CompositeDeckOverlap / 2.0,  deckElevation.Elevation);
                        XYZ endPoint = new XYZ(currentPoint.X, endLocation + m_CompositeDeckOverlap / 2.0, deckElevation.Elevation);

                        bool bCanPlaceDeck = CheckIfDeckCanBePlaced(startPoint, endPoint, deckingDirection);

                        if (bCanPlaceDeck)
                        {
                            if (Math.Abs(startPoint.Y - endPoint.Y) < 0.5)
                                continue;

                            Line newInputLine = Line.CreateBound(startPoint, endPoint);

                            FamilySymbol symbol = SymbolCollector.GetCompositeDeckSymbol(m_CompositeDeckType, "Composite Deck");

                            if (symbol != null && !symbol.IsActive)
                                symbol.Activate();


                            FamilyInstance compositeDeckInstance = doc.Create.NewFamilyInstance(newInputLine, symbol, deckElevation, StructuralType.Beam);

                            StructuralFramingUtils.DisallowJoinAtEnd(compositeDeckInstance, 0);

                            StructuralFramingUtils.DisallowJoinAtEnd(compositeDeckInstance, 1);

                        }

                        startlocation = endLocation;

                    }

                    // We have the composite deck starting and ending points - move to the next location
                    currentPoint = new XYZ(currentPoint.X + 3, currentPoint.Y, currentPoint.Z);
                }
            }
        }

        private void TrimDeckPanel(XYZ startPoint, XYZ endPoint, FamilyInstance compositeDeckInstance)
        {
            throw new NotImplementedException();
        }

        private bool CheckIfDeckCanBePlaced(XYZ startPoint, XYZ endPoint, LineType deckingDirection)
        {
            // TO-DO: The condition whether deck can be placed is that, there should be LB lines or Cee Headers at both start and end
            return true;
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
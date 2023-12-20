using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Odbc;

namespace Revit_Automation.Source.Utils
{
    public class PanelUtils
    {
        private Document _document;
        public PanelUtils(Document document)
        {
            _document = document;
        }

        public void ComputePanelDirectionForExteriorPanels()
        {
            List<InputLine> lines = InputLineUtility.colInputLines;
            ElementId rightcolumnID = null, leftColumnID = null;

            for (int i = 0; i < lines.Count; i++)
            {
                InputLine inputLine = lines[i];

                if (inputLine.strWallType == "Ex w/ Insulation")
                {
                    
                    IOrderedEnumerable<Level> levels = ModelCreator.FindAndSortLevels(_document);
                    Level toplevel = null, baselevel = null;

                    LineType linetype = MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X) ? LineType.vertical : LineType.Horizontal;

                    PostCreationUtils.ComputeTopAndBaseLevels(inputLine, levels, out toplevel, out baselevel);

                    Element baseAttach = GenericUtils.GetNearestFloorOrRoof(baselevel, inputLine.startpoint, _document);

                    // Create columns on either side of the line and try to attach the base.
                    // Whichever side base attach is true, Panel direction is that
                    XYZ midpoint = new XYZ((inputLine.startpoint.X + inputLine.endpoint.X) / 2.0,
                                            (inputLine.startpoint.Y + inputLine.endpoint.Y) / 2.0,
                                            (inputLine.startpoint.Z + inputLine.endpoint.Z) / 2.0);

                    XYZ rightpoint = null, leftpoint = null;

                    if (linetype == LineType.Horizontal)
                    {
                        rightpoint = new XYZ(midpoint.X, midpoint.Y + 1.1, midpoint.Z);
                        leftpoint = new XYZ(midpoint.X, midpoint.Y - 1.1, midpoint.Z);
                    }
                    else
                    {
                        rightpoint = new XYZ(midpoint.X + 1.1, midpoint.Y, midpoint.Z);
                        leftpoint = new XYZ(midpoint.X - 1.1, midpoint.Y, midpoint.Z);
                    }

                    bool bright = false;

                    string strFamilySymbol = inputLine.strStudType.ToString() + string.Format(" x {0}ga", inputLine.strStudGuage);
                    FamilySymbol columnType = SymbolCollector.GetSymbol(strFamilySymbol, "Post", SymbolCollector.FamilySymbolType.posts);

                    using (Transaction stx = new Transaction(_document))
                    {
                        GenericUtils.SupressWarningsInTransaction(stx);

                        stx.Start("Determining the Panel Direction");

                        if (rightcolumnID != null)
                            _document.Delete(rightcolumnID);
                        if (leftColumnID != null)
                            _document.Delete(leftColumnID);

                        // Create the column instance at the point
                        FamilyInstance rightcolumn = _document.Create.NewFamilyInstance(rightpoint, columnType, baselevel, StructuralType.Column);
                        FamilyInstance leftcolumn = _document.Create.NewFamilyInstance(leftpoint, columnType, baselevel, StructuralType.Column);

                        rightcolumn.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).Set(2);
                        leftcolumn.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).Set(2);

                        rightcolumnID = rightcolumn.Id;
                        leftColumnID = leftcolumn.Id;

                        if (baseAttach != null)
                        {
                            ColumnAttachment.AddColumnAttachment(_document, rightcolumn, baseAttach, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);

                            ColumnAttachment.AddColumnAttachment(_document, leftcolumn, baseAttach, 0, ColumnAttachmentCutStyle.CutColumn, ColumnAttachmentJustification.Midpoint, 0);
                        }

                        stx.Commit();
                    }

                    Element right = _document.GetElement(rightcolumnID);
                    Element left = _document.GetElement(leftColumnID);

                    bool bRightSet = false, bLeftSet = false;
                    double dRightHeight = 0.0, dLeftHeight = 0.0;

                    Parameter rightHeightParam = right.LookupParameter("Height");
                    if (rightHeightParam != null)
                    {
                        dRightHeight = rightHeightParam.AsDouble();
                    }
                    Parameter LeftHeightParam = left.LookupParameter("Height");
                    if (LeftHeightParam != null)
                    {
                        dLeftHeight = LeftHeightParam.AsDouble();
                    }

                    using (Transaction stx2 = new Transaction(_document))
                    {
                        stx2.Start("Adding Panel Direction to Input lines");
                        // Set the value on the Input line revit element
                        Element inputLineElement = _document.GetElement(inputLine.id);

                        if (dLeftHeight > dRightHeight)
                        {

                            if (linetype == LineType.Horizontal)
                            {
                                inputLine.strHorizontalPanelDirection = "D";
                                inputLineElement.LookupParameter("Horizontal Panel Direction")?.Set("D");
                            }
                            else
                            {
                                inputLine.strVerticalPanelDirection = "L";
                                inputLineElement.LookupParameter("Vertical Panel Direction")?.Set("L");
                            }
                        }
                        else
                        {
                            if (linetype == LineType.Horizontal)
                            {
                                inputLine.strHorizontalPanelDirection = "U";
                                inputLineElement.LookupParameter("Horizontal Panel Direction")?.Set("U");
                            }
                            else
                            {
                                inputLine.strVerticalPanelDirection = "R";
                                inputLineElement.LookupParameter("Vertical Panel Direction")?.Set("R");
                            }
                        }
                        stx2.Commit();
                    }
                    lines[i] = inputLine;
                }
            }
            InputLineUtility.colInputLines = lines;

            using (Transaction tx = new Transaction(_document))
            {
                tx.Start("Deleting Columns");
                if (rightcolumnID != null)
                    _document.Delete(rightcolumnID);
                if (leftColumnID != null)
                    _document.Delete(leftColumnID);
                tx.Commit();
            }
        }

        private double GetPanelPreferredLength(InputLine line)
        {
            PanelTypeGlobalParams pg = string.IsNullOrEmpty(line.strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == line.strPanelType);

            double dPanelPreferredLength = pg.iPanelPreferredLength;

            return dPanelPreferredLength;
        }

        public List<XYZ> ComputeMiddleIntersectionPts(InputLine inputLine, SortedDictionary<XYZ, string> sortedDictionary, XYZ StartPoint, XYZ EndPoint)
        {
            XYZ startPt = StartPoint, endPt = EndPoint ;
            double dPreferedLength = GetPanelPreferredLength(inputLine);
            double dMaxLength = GetPanelMaxLength(inputLine);
            List<XYZ> middleIntersections = new List<XYZ>();

            LineType lineType = GenericUtils.GetLineType(inputLine);

            // For line lengths less than 25.0 single panel is sufficient
            if ((lineType == LineType.Horizontal && Math.Abs(StartPoint.X - EndPoint.X) < 25.0) ||
                    (lineType == LineType.vertical && Math.Abs(StartPoint.Y - EndPoint.Y) < 25.0))
                return middleIntersections;

            double dPanelStrategy = GetPanelStrategy(inputLine);
            if (dPanelStrategy == 2)
            {
                List<XYZ> retPts = ComputePointsAccordingToSelectedStrategy(inputLine, sortedDictionary, startPt, endPt);
                middleIntersections.AddRange(retPts);

                // Adjust the intersection points such that Intersections are at studs. 
                List<XYZ> adjustedMiddleIntersections1 = AdjustIntersectionsAtStud(inputLine, middleIntersections);

                return adjustedMiddleIntersections1;

            }
            
            // if there are no intersections - Place panels as per 3rd strategy
            if (sortedDictionary.Count == 0)
            {
                List<XYZ> retPts = ComputePointsAccordingToSelectedStrategy(inputLine, sortedDictionary, startPt, endPt);
                middleIntersections.AddRange(retPts);
            }

            else
            {
                bool bPreferredAtStart = false;
                bool bPreferredAtEnd = false;
                bool bMaxAtStart = false;
                bool bMaxAtEnd = false;
                bool bFallbackStrategy = true;

                XYZ dPreferredX = new XYZ(dPreferedLength, 0, 0);
                XYZ dPreferredY = new XYZ(0, dPreferedLength, 0);
                XYZ dMaximumX = new XYZ(dMaxLength, 0, 0);
                XYZ dMaximumY = new XYZ(0, dMaxLength, 0);

                Direction direction = Direction.NoDirection;

                // Check if we can place a panel at preferred or max length from start or end
                // That will help us determine the startegy for next panel placements
                foreach (KeyValuePair<XYZ, string> kvp in sortedDictionary)
                {
                    XYZ intersectPt = kvp.Key;

                    if (lineType == LineType.Horizontal)
                    {

                        if (Math.Abs((startPt.X + dPreferedLength) - intersectPt.X) < 1.0)
                        {
                            bPreferredAtStart = true;
                            direction = Direction.StartToEnd;

                        }
                        else if (Math.Abs((endPt.X - dPreferedLength) - intersectPt.X) < 1.0)
                        {
                            bPreferredAtEnd = true;
                            direction = Direction.EndToStart;
                        }
                        else if (Math.Abs((startPt.X + dMaxLength) - intersectPt.X) < 1.0)
                        {
                            bMaxAtStart = true;
                            direction = Direction.StartToEnd;
                        }
                        else if (Math.Abs((endPt.X - dMaxLength) - intersectPt.X) < 1.0)
                        {
                            bMaxAtEnd = true;
                            direction = Direction.EndToStart;
                        }
                    }
                    else
                    {
                        if (Math.Abs((startPt.Y + dPreferedLength) - intersectPt.Y) < 1.0)
                        {
                            bPreferredAtStart = true;
                            direction = Direction.StartToEnd;

                        }
                        else if (Math.Abs((endPt.Y - dPreferedLength) - intersectPt.Y) < 1.0)
                        {
                            bPreferredAtEnd = true;
                            direction = Direction.EndToStart;
                        }
                        else if (Math.Abs((startPt.Y + dMaxLength) - intersectPt.Y) < 1.0)
                        {
                            bMaxAtStart = true;
                            direction = Direction.StartToEnd;
                        }
                        else if (Math.Abs((endPt.Y - dMaxLength) - intersectPt.Y) < 1.0)
                        {
                            bMaxAtEnd = true;
                            direction = Direction.EndToStart;
                        }
                    }

                    if (bPreferredAtStart == true || bPreferredAtEnd == true || bMaxAtStart == true || bMaxAtEnd == true)
                    {
                        bFallbackStrategy = false;
                        break;
                    }
                }

                if (!bFallbackStrategy)
                {
                    if (direction == Direction.StartToEnd)
                    {
                        if (lineType == LineType.Horizontal)
                        {
                            if (bPreferredAtStart)
                            {
                                middleIntersections.Add(startPt + dPreferredX);
                                startPt = startPt + dPreferredX;
                            }
                            if (bMaxAtStart)
                            {
                                middleIntersections.Add(startPt + dMaximumX);
                                startPt = startPt + dMaximumX;
                            }

                            while (startPt.X < (endPt.X - 25.0) && !bFallbackStrategy)
                            {
                                bool bStPref = false;
                                bool bStMax = false;
                                foreach (KeyValuePair<XYZ, string> kvp in sortedDictionary)
                                {
                                    XYZ intersectPt = kvp.Key;
                                    if (Math.Abs((startPt.X + dPreferedLength) - intersectPt.X) < 1.0)
                                    {
                                        middleIntersections.Add(startPt + dPreferredX);
                                        startPt = startPt + dPreferredX;
                                        bStPref = true;
                                    }
                                    else if (Math.Abs((startPt.X + dMaxLength) - intersectPt.X) < 1.0)
                                    {
                                        middleIntersections.Add(startPt + dMaximumX);
                                        startPt = startPt + dMaximumX;
                                        bStMax = true;
                                    }

                                    if (bStMax == true || bStPref == true)
                                    {
                                        break;
                                    }
                                }
                                if (bStMax == false && bStPref == false)
                                    bFallbackStrategy = true;
                            }

                            if (bFallbackStrategy)
                            {
                                List<XYZ> retpts = new List<XYZ>();
                                retpts = ComputePointsAccordingToSelectedStrategy(inputLine, sortedDictionary, startPt, endPt);
                                middleIntersections.AddRange(retpts);
                            }
                        }
                        else
                        {
                            if (bPreferredAtStart)
                            {
                                middleIntersections.Add(startPt + dPreferredY);
                                startPt = startPt + dPreferredY;
                            }
                            if (bMaxAtStart)
                            {
                                middleIntersections.Add(startPt + dMaximumY);
                                startPt = startPt + dMaximumY;
                            }
                            while (startPt.Y < (endPt.Y - 25.0) && !bFallbackStrategy)
                            {
                                bool bStPref = false;
                                bool bStMax = false;
                                foreach (KeyValuePair<XYZ, string> kvp in sortedDictionary)
                                {
                                    XYZ intersectPt = kvp.Key;
                                    if (Math.Abs((startPt.Y + dPreferedLength) - intersectPt.Y) < 1.0)
                                    {
                                        middleIntersections.Add(startPt + dPreferredY);
                                        startPt = startPt + dPreferredY;
                                        bStPref = true;
                                    }
                                    else if (Math.Abs((startPt.Y + dMaxLength) - intersectPt.Y) < 1.0)
                                    {
                                        middleIntersections.Add(startPt + dMaximumY);
                                        startPt = startPt + dMaximumY;
                                        bStMax = true;
                                    }

                                    if (bStMax == true || bStPref == true)
                                    {
                                        break;
                                    }
                                }
                                if (bStMax == false && bStPref == false)
                                    bFallbackStrategy = true;
                            }

                            if (bFallbackStrategy)
                            {
                                List<XYZ> retpts = new List<XYZ>();
                                retpts = ComputePointsAccordingToSelectedStrategy(inputLine, sortedDictionary, startPt, endPt);
                                middleIntersections.AddRange(retpts);
                            }
                        }
                    }
                    else if (direction == Direction.EndToStart)
                    {
                        if (lineType == LineType.Horizontal)
                        {
                            if (bPreferredAtEnd)
                            {
                                middleIntersections.Add(endPt - dPreferredX);
                                endPt = endPt - dPreferredX;
                            }
                            if (bMaxAtEnd)
                            {
                                middleIntersections.Add(endPt - dMaximumX);
                                endPt = endPt - dMaximumX;
                            }

                            while (startPt.X < (endPt.X - 25.0) && !bFallbackStrategy)
                            {
                                bool bStPref = false;
                                bool bStMax = false;
                                foreach (KeyValuePair<XYZ, string> kvp in sortedDictionary)
                                {
                                    XYZ intersectPt = kvp.Key;
                                    if (Math.Abs((endPt.X - dPreferedLength) - intersectPt.X) < 1.0)
                                    {
                                        middleIntersections.Add(endPt - dPreferredX);
                                        endPt = endPt - dPreferredX;
                                        bStPref = true;
                                    }
                                    else if (Math.Abs((endPt.X - dMaxLength) - intersectPt.X) < 1.0)
                                    {
                                        middleIntersections.Add(endPt - dMaximumX);
                                        endPt = endPt - dMaximumX;
                                        bStMax = true;
                                    }

                                    if (bStMax == true || bStPref == true)
                                    {
                                        break;
                                    }
                                }
                                if (bStMax == false && bStPref == false)
                                    bFallbackStrategy = true;
                            }

                            if (bFallbackStrategy)
                            {
                                List<XYZ> retpts = new List<XYZ>();
                                retpts = ComputePointsAccordingToSelectedStrategy(inputLine, sortedDictionary, startPt, endPt);
                                middleIntersections.AddRange(retpts);
                            }
                        }
                        else
                        {
                            if (bPreferredAtEnd)
                            {
                                middleIntersections.Add(endPt - dPreferredY);
                                endPt = endPt - dPreferredY;
                            }
                            if (bMaxAtEnd)
                            {
                                middleIntersections.Add(endPt - dMaximumY);
                                endPt = endPt - dMaximumY;
                            }
                            while (startPt.Y < (endPt.Y - 25.0) && !bFallbackStrategy)
                            {
                                bool bStPref = false;
                                bool bStMax = false;
                                foreach (KeyValuePair<XYZ, string> kvp in sortedDictionary)
                                {
                                    XYZ intersectPt = kvp.Key;
                                    if (Math.Abs((endPt.Y - dPreferedLength) - intersectPt.Y) < 1.0)
                                    {
                                        middleIntersections.Add(endPt - dPreferredY);
                                        endPt = endPt - dPreferredY;
                                        bStPref = true;
                                    }
                                    else if (Math.Abs((endPt.Y - dMaxLength) - intersectPt.Y) < 1.0)
                                    {
                                        middleIntersections.Add(endPt - dMaximumY);
                                        endPt = endPt - dMaximumY;
                                        bStMax = true;
                                    }

                                    if (bStMax == true || bStPref == true)
                                    {
                                        break;
                                    }
                                }
                                if (bStMax == false && bStPref == false)
                                    bFallbackStrategy = true;
                            }

                            if (bFallbackStrategy)
                            {
                                List<XYZ> retpts = new List<XYZ>();
                                retpts = ComputePointsAccordingToSelectedStrategy(inputLine, sortedDictionary, startPt, endPt);
                                middleIntersections.AddRange(retpts);
                            }
                        }
                    }
                }
                else
                {
                    List<XYZ> retPts = ComputePointsAccordingToSelectedStrategy(inputLine, sortedDictionary, startPt, endPt);
                    middleIntersections.AddRange(retPts);
                }
            }

            // Adjust the intersection points such that Intersections are at studs. 
            List<XYZ> adjustedMiddleIntersections = AdjustIntersectionsAtStud(inputLine, middleIntersections);

            return adjustedMiddleIntersections;
        }

        private List<XYZ> AdjustIntersectionsAtStud(InputLine inputLine, List<XYZ> middleIntersections)
        {

            List<XYZ> returnPoints = new List<XYZ>();
            LineType lineType = GenericUtils.GetLineType(inputLine);
            List<XYZ> studLocations = GetStudlocations(inputLine);

            middleIntersections = lineType == LineType.Horizontal ? middleIntersections.OrderBy(il => il.X).ToList() : middleIntersections.OrderBy(il => il.Y).ToList();
            
            // TO-Do : Get Stud Lap Parameter
            double dPanelLap = GetMinimumPaneLapValue(inputLine);

            foreach (XYZ intesectPt in middleIntersections)
            {
                XYZ studPt = GetNearestStudPoint(studLocations, intesectPt);

                if (lineType == LineType.Horizontal)
                {
                    returnPoints.Add(new XYZ(studPt.X + dPanelLap/2, intesectPt.Y, intesectPt.Z));
                    returnPoints.Add(new XYZ(studPt.X - dPanelLap/2, intesectPt.Y, intesectPt.Z));
                }
                else
                {
                    returnPoints.Add(new XYZ(intesectPt.X, studPt.Y + dPanelLap / 2, intesectPt.Z));
                    returnPoints.Add(new XYZ(intesectPt.X, studPt.Y - dPanelLap / 2, intesectPt.Z));
                }
            }

            return returnPoints;
        }

        private XYZ GetNearestStudPoint(List<XYZ> pointCollection, XYZ givenPoint)
        {
            double minDistance = double.MaxValue;
            XYZ closestPoint = null;

            foreach (XYZ point in pointCollection)
            {
                double distance = point.DistanceTo(givenPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = point;
                }
            }

            return closestPoint;
        }

        private List<XYZ> GetStudlocations(InputLine inputLine)
        {
            List<XYZ> lstStudLocations = new List<XYZ>();

            XYZ startPt = null, endPt = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out startPt, out endPt);
            LineType lineType = GenericUtils.GetLineType(inputLine);
            Outline outline = null;

            if (lineType == LineType.Horizontal)
            {
                outline = new Outline(new XYZ(startPt.X, startPt.Y - 0.25, startPt.Z),
                                        new XYZ(endPt.X, endPt.Y + 0.25, endPt.Z));
            }
            else
            {
                outline = new Outline(new XYZ(startPt.X - 0.25, startPt.Y, startPt.Z),
                                        new XYZ(endPt.X + 0.25, endPt.Y, endPt.Z));
            }

            // Create a BoundingBoxIntersects filter with this Outline
            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

            // Apply the filter to the elements in the active document to retrieve posts at a point
            FilteredElementCollector collector = new FilteredElementCollector(_document);
            IList<Element> postElements = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_StructuralColumns).ToElements();

            foreach (Element colElement in postElements)
            {
                FamilyInstance post = _document.GetElement(colElement.Id) as FamilyInstance;

                Location location = post.Location;
                if (MathUtils.IsParallel(post.FacingOrientation, GenericUtils.GetLineOrientation(inputLine)))
                {
                    // Check if the column's location is a LocationPoint
                    if (location is LocationPoint locationPoint)
                    {
                        lstStudLocations.Add(locationPoint.Point);
                    }
                }
            }

            return lstStudLocations;
        }

        private double GetPanelMaxLength(InputLine line)
        {
            PanelTypeGlobalParams pg = string.IsNullOrEmpty(line.strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == line.strPanelType);

            double dPanelPreferredLength = pg.iPanelMaxLength;

            return dPanelPreferredLength;
        }

        private double GetMinimumPaneLapValue(InputLine line)
        {
            PanelTypeGlobalParams pg = string.IsNullOrEmpty(line.strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == line.strPanelType);

            return pg.iPanelMinLap;
        }

        private double GetPanelHeightOffsetValue (InputLine line)
        {
            PanelTypeGlobalParams pg = string.IsNullOrEmpty(line.strPanelType) ?
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                            GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == line.strPanelType);

            return pg.iPanelHeightOffset;
        }
        private double GetPanelStrategy(InputLine line)
        {
            return double.Parse(GlobalSettings.s_iPanelStrategy);
        }
        private List<XYZ> ComputePointsAccordingToSelectedStrategy(InputLine inputLine, SortedDictionary<XYZ, string> sortedDictionary, XYZ startPt, XYZ endPt)
        {

            double dPanelStrategy = GetPanelStrategy(inputLine);
            List<XYZ> middleIntersections = new List<XYZ>();
            XYZ middlePoint = startPt;
            LineType linetype = GenericUtils.GetLineType(inputLine);

            // Preferred Length or Max Length
            if (dPanelStrategy == 0 || dPanelStrategy == 1)
            {
                XYZ AdditionVector = null;

                double dPanelPreferredLength = GetPanelPreferredLength(inputLine);
                double dMaxLength = GetPanelMaxLength(inputLine);

                if (linetype == LineType.Horizontal)
                    AdditionVector = new XYZ(dPanelStrategy == 0 ? dPanelPreferredLength : dMaxLength, 0, 0);
                else
                    AdditionVector = new XYZ(0, dPanelStrategy == 0 ? dPanelPreferredLength : dMaxLength, 0);


                while (true)
                {
                    middlePoint = middlePoint + AdditionVector;

                    if ((linetype == LineType.Horizontal && middlePoint.X < endPt.X) ||
                        (linetype == LineType.vertical && middlePoint.Y < endPt.Y))
                    {
                        middleIntersections.Add(middlePoint);
                    }
                    else
                        break;
                }
            }

            // At Rooms
            if (dPanelStrategy == 2)
            {
                foreach (KeyValuePair<XYZ, string> kvp in sortedDictionary)
                {
                    XYZ intersectPt = kvp.Key;

                    if ((linetype == LineType.Horizontal && intersectPt.X < middlePoint.X) ||
                            (linetype == LineType.vertical && intersectPt.Y < middlePoint.Y))
                        continue;

                    else
                    {
                        if (linetype == LineType.Horizontal)
                        {
                            middlePoint = new XYZ(intersectPt.X, middlePoint.Y, middlePoint.Z);
                            //middlePoint = middlePoint + tempPoint;
                            middleIntersections.Add(middlePoint);
                        }
                        else
                        {
                            middlePoint = new XYZ(middlePoint.X, intersectPt.Y, middlePoint.Z);
                            middleIntersections.Add(middlePoint);
                        }
                    }
                }
            }
            return middleIntersections;
        }
    }
}

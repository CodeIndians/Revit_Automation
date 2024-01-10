using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source;
using Revit_Automation.Source.ModelCreators.Walls;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml;
using static Autodesk.Revit.DB.SpecTypeId;

namespace Revit_Automation
{
    // Assumptions : if Span is vertical - And we have vertical CMU walls in the span, consider Cee Header points of the horizontal walls as a vertical wall is always bounded by pair of horizontal walls
    // Vice  -versa if the span is horizontal
    internal class CeeHeaderCreator
    {
        private Document doc;
        private Form1 form;
        private double m_SlabThickness = 0.0;
        enum SlopeDirection
        {
            Horizontal,
            Vertical
        }
        private bool m_bStartingFromExterior = true;
        private XYZ m_startPoint;
        private XYZ m_endPoint;
        private SlopeDirection m_direction;
        private double m_DeckSpan;
        private Level m_inputLineElevationLevel;
        public CeeHeaderCreator(Document doc, Form1 form)
        {
            this.doc = doc;
            this.form = form;
        }

        internal void CreateModel(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            using (Transaction tx = new Transaction(doc))
            {
                form.PostMessage("");
                form.PostMessage("\n Starting creation of cee headers \n");
                tx.Start("Placing Cee Headers");
                Dictionary<double, List<InputLine>> sortedInputLineCollection = new Dictionary<double, List<InputLine>>();
                sortedInputLineCollection = SortInputLinesByElevation(colInputLines);

                // Get the Span
                m_DeckSpan = GlobalSettings.framingSettings.dCeeHeaderDeckSpan;

                if (m_DeckSpan == 0.0)
                {
                    MessageBox.Show("Cee Header Deck Span is not set, Please set it and rerun the command");
                    tx.Commit();
                    return;
                }
                // Compute of the slope is horizontal or vertical
                ComputeSlopeDirection(colInputLines);

                // Compute the start point for CeeHeader computation
                ComputeStartPoint(colInputLines);

                // Compute the level for the given Input line collection and store it in the class variable
                ComputeFloorLevel(colInputLines[0], levels);

                foreach (KeyValuePair<double, List<InputLine>> kvp in sortedInputLineCollection)
                {
                    List<InputLine> list = kvp.Value;
                    double elevation = kvp.Key;

                    InputLine temp = list[0];
                    Level level = GetLevelForInputLine(temp, levels);

                    // Get the settings for this level
                    if (level != null)
                    {
                        Parameter thicknessParam = null;

                        Element SlabElement = GenericUtils.GetNearestFloorOrRoof(level, temp.startpoint, doc);
                        if (SlabElement != null)
                            thicknessParam = SlabElement.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);

                        if (SlabElement == null)
                        {
                            // keep checking for the lines till we find a roof
                            foreach (InputLine il in colInputLines)
                            {
                                Element roofElement = GenericUtils.GetRoofAtPoint(il.startpoint, doc);
                                if (roofElement != null)
                                {
                                    thicknessParam = roofElement.get_Parameter(BuiltInParameter.ROOF_ATTR_THICKNESS_PARAM);
                                    break;
                                }
                            }
                                
                        }
                        if (thicknessParam != null)
                        {
                            m_SlabThickness = thicknessParam.AsDouble();
                        }
                        
                        CeeHeaderSettings ceeHeaderSettings = GetCeeHeaderSettingsForGivenLevel(level.Name);

                        // Place Cee Headers at given lines.
                        PlaceCeeHeaders(ceeHeaderSettings, list, level);
                    }
                }
                form.PostMessage(" \n Finished creation of cee headers");
                tx.Commit();
            }
        }

        private void ComputeFloorLevel(InputLine inputLine, IOrderedEnumerable<Level> levels)
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
                    level = filteredLevels.ElementAt(i);
                }
            }

            m_inputLineElevationLevel =  level;

            return;
        }
    

        private void ComputeStartPoint(List<InputLine> colInputLines)
        {
            List<InputLine> exteriorLines = colInputLines.Where(ex => (ex.strWallType == "Ex" || ex.strWallType == "Ex w/ Insulation")).ToList();

            List<InputLine> targetLines = exteriorLines.Count == 0 ? colInputLines : exteriorLines;

            if (exteriorLines.Count == 0)
                m_bStartingFromExterior = false;

            List<XYZ> points = new List<XYZ>();
            foreach (InputLine exLine in targetLines)
            {
                points.Add(exLine.startpoint);
                points.Add(exLine.endpoint);
            }

            m_startPoint = points.OrderBy(point => point.X).ThenBy(point => point.Y).FirstOrDefault();
            m_endPoint = points.OrderBy(point => point.X).ThenBy(point => point.Y).Last();
        }

        private void ComputeSlopeDirection(List<InputLine> colInputLines)
        {
            foreach (InputLine line in colInputLines)
            {
                if (line.strWallType == "LB" || line.strWallType == "LBS")
                {
                    m_direction = (GenericUtils.GetLineType(line) == LineType.Horizontal) ? SlopeDirection.Horizontal : SlopeDirection.Vertical;
                    break;
                }
            }
        }

        private Level GetLevelForInputLine(InputLine temp, IOrderedEnumerable<Level> levels)
        {
            Level level = null;
            
            // Filter levels based on buldings to use
            List<Level> filteredLevels = new List<Level>();
            foreach (Level filteredlevel in levels)
            {
                if (filteredlevel.Name.Contains(temp.strBuildingName))
                {
                    filteredLevels.Add(filteredlevel);
                }
            }

            for (int i = 0; i < filteredLevels.Count() - 1; i++)
            {
                Level tempLevel = filteredLevels.ElementAt(i);

                if ((temp.startpoint.Z < (tempLevel.Elevation + 1)) && (temp.startpoint.Z > (tempLevel.Elevation - 1)))
                {
                    Level toplevel = filteredLevels.ElementAt(i + 1);
                    level = toplevel;
                }
            }

            return level;
        }

        private void PlaceCeeHeaders(CeeHeaderSettings ceeHeaderSettings, List<InputLine> InputlineList, Level level)
        {
            int iSpan = 1;
            
            // Points where Cee-Headers are to be placed
            Dictionary<XYZ, double> ceeHeaderPts = new Dictionary<XYZ, double>();

            // Get the starting grid
            List<XYZ> spanStartPts = CeeHeaderBoundaries.GetSpanStartingGrid();
            XYZ startPoint = spanStartPts.First();

            // compute the orientation of the starting grid
            LineType spanLineType = MathUtils.ApproximatelyEqual(spanStartPts[0].X, spanStartPts[1].X) ? LineType.vertical : LineType.Horizontal;
            XYZ additionVector = spanLineType == LineType.vertical ? new XYZ(m_DeckSpan, 0, 0) : new XYZ(0, m_DeckSpan, 0);

            // 1st Span from the edge of the building
            startPoint += additionVector;

            List<string> lstceeHeaderPoints = new List<string>();

            // Get the CeeHeaderpoints
            while (IdentifyCeeHederPoints(ref startPoint, InputlineList, out lstceeHeaderPoints))
            {                
                ceeHeaderPts = ProcessCeeHeaderPoints(lstceeHeaderPoints, spanLineType);

                if (ceeHeaderPts.Count > 0)
                {
                    for (int i = 0; i < ceeHeaderPts.Count - 1; i++)
                    {
                        KeyValuePair<XYZ, double> kvp1 = ceeHeaderPts.ElementAt(i++);
                        KeyValuePair<XYZ, double> kvp2 = ceeHeaderPts.ElementAt(i);

                        XYZ ceeHeaderStartPoint = kvp1.Key;
                        XYZ ceeHeaderEndPoint = kvp2.Key;

                        

                        //form.PostMessage($" \nPlacing Cee-Headers at location {ceeHeaderStartPoint.X} , {ceeHeaderStartPoint.Y}, {ceeHeaderEndPoint.X} , {ceeHeaderEndPoint.Y}");

                        double dWebWitdth = Math.Max(kvp1.Value, kvp2.Value);

                        bool bHeaderAtHallway = GenericUtils.LineIntersectsHallway(doc, ceeHeaderStartPoint, ceeHeaderEndPoint);

                        double dElevation = Math.Abs(ceeHeaderStartPoint.Z - level.Elevation);
                        ceeHeaderStartPoint += new XYZ(0, 0, dElevation - m_SlabThickness);
                        ceeHeaderEndPoint += new XYZ(0, 0, dElevation - m_SlabThickness);

                        // if the points are apart only by 1, 1 1/2 feet do not place ceeHeaders
                        if ((spanLineType == LineType.vertical && Math.Abs(ceeHeaderStartPoint.Y - ceeHeaderEndPoint.Y) < 1.5) || (spanLineType == LineType.Horizontal
                                && Math.Abs(ceeHeaderStartPoint.X - ceeHeaderEndPoint.X) < 1.5))
                            continue;

                        double ceeHeaderMaxLength = GlobalSettings.framingSettings.dCeeHeaderMaxLength == 0 ? 20.0 : GlobalSettings.framingSettings.dCeeHeaderMaxLength;
                        
                        // if lines greater than max cee header length do not place headers
                        if ((spanLineType == LineType.vertical && Math.Abs(ceeHeaderStartPoint.Y - ceeHeaderEndPoint.Y) > ceeHeaderMaxLength) || (spanLineType == LineType.Horizontal && Math.Abs(ceeHeaderStartPoint.X - ceeHeaderEndPoint.X) > ceeHeaderMaxLength))
                            continue;

                        FamilySymbol ceeHeaderFamily = SymbolCollector.GetCeeHeadersFamily(bHeaderAtHallway ? ceeHeaderSettings.HallwayCeeHeaderName : ceeHeaderSettings.ceeHeaderName
                            , "Cee Header");

                        Line bounds = Line.CreateBound(ceeHeaderStartPoint, ceeHeaderEndPoint);
                        FamilyInstance ceeHeaderInstance = doc.Create.NewFamilyInstance(bounds, ceeHeaderFamily, level, StructuralType.Beam);
                        StructuralFramingUtils.DisallowJoinAtEnd(ceeHeaderInstance, 0);
                        StructuralFramingUtils.DisallowJoinAtEnd(ceeHeaderInstance, 1);

                        form.PostMessage($" \nPlacing Cee-Header {ceeHeaderInstance.Id} at location {ceeHeaderStartPoint.X} , {ceeHeaderStartPoint.Y}, {ceeHeaderEndPoint.X} , {ceeHeaderEndPoint.Y}");

                        // set the Post CL Offset parameter
                        if (dWebWitdth != 0.0)
                            ceeHeaderInstance.LookupParameter("Post CL Face Offset").Set(dWebWitdth);

                        if (bHeaderAtHallway == true && ceeHeaderSettings.HallwayCeeHeaderCount == "Double" || bHeaderAtHallway == false && ceeHeaderSettings.ceeHeaderCount == "Double")
                        {
                            Line bounds2 = Line.CreateBound(ceeHeaderEndPoint, ceeHeaderStartPoint);
                            FamilyInstance ceeHeaderInstance2 = doc.Create.NewFamilyInstance(bounds2, ceeHeaderFamily, level, StructuralType.Beam);
                            
                            // set the Post CL Offset parameter
                            if (dWebWitdth != 0.0)
                                ceeHeaderInstance2.LookupParameter("Post CL Face Offset").Set(dWebWitdth);

                            StructuralFramingUtils.DisallowJoinAtEnd(ceeHeaderInstance2, 0);
                            StructuralFramingUtils.DisallowJoinAtEnd(ceeHeaderInstance2, 1);
                        }
                    }
                }
                // Move to the next span
                startPoint = startPoint + additionVector;
                ceeHeaderPts.Clear();
            }
        }

        private Dictionary<XYZ, double> ProcessCeeHeaderPoints(List<string> lstceeHeaderPoints, LineType spanDirection)
        {
            // Cee headers points are added in such a way that we place Cee Headers at 1-2, 3-4, 5-6 etc
            // we may have odd number of points - In below cases (1, 2) we have 5 points. so, 1,2 3,4 logic will not work 
            // |--------------------------------| |--------------------------------| |--------------------------------|
            // |       |          |             | |                                | |       |          |             |
            // |       |__________|             | |                                | |       |__________|             |
            // |                                | |            |                   | |                                |
            // |            |                   | |            |                   | |            |                   | 
            // |            |                   | |            |                   | |            |                   | 
            // |                                | |       |----------|             | |       |----------|             | 
            // |________________________________| |_______|__________|____________ | |_______|__________|____________ | 
            // So we have to check if the two penultimate points are CMU points
            // if 2nd point is CMU series will be 2-3 4-5, Leave out 1
            // if 4th point is CMU like in adjacent figure 1-2, 3-4 leave out 5
            // We may have 6 points like in figure 3 tyhen we need cee headers at 2-3, 4-5 leave out 1, 6
            // So the logic is, if number of points are odd, make it even by stripping the relevant end point (Case 1 & 2)
            // if even and penultimare points are CMU points strip both end points (Case 3)
            
            // In this method we also need to do adjustment for flange width,
            // if we are having double studs, we have to make sure the Cee headers extend upto flange width
            // If we are having CMU walls, the cee headers should terminate at the wall end

            // We also need to get the floor thickness and subtract it from the elevation of the cee header

            Dictionary<XYZ, double> ceeHeaderPointsDict = new Dictionary<XYZ, double>();

            if (lstceeHeaderPoints.Count % 2 == 1)
            {
                if (lstceeHeaderPoints[lstceeHeaderPoints.Count - 2].Contains("CMU") && lstceeHeaderPoints[lstceeHeaderPoints.Count - 1].Contains("CMU"))
                    lstceeHeaderPoints.RemoveAt(lstceeHeaderPoints.Count - 1);

                if (lstceeHeaderPoints[1].Contains("CMU") && lstceeHeaderPoints[0].Contains("CMU"))
                    lstceeHeaderPoints.RemoveAt(0);
            }
            else
            {
                if (lstceeHeaderPoints[lstceeHeaderPoints.Count - 2].Contains("CMU") &&
                    lstceeHeaderPoints[1].Contains("CMU"))
                {
                    lstceeHeaderPoints.RemoveAt(lstceeHeaderPoints.Count - 1);
                    lstceeHeaderPoints.RemoveAt(0);
                }
            }

            // Preprocessing
            // One more condition, when we have the last end points either stud or CMU  try to align the point with the adjacent point for straight placement
            // else it will be inclined. Refer- RA-19 in the bugs and improvements Doc.
            for (int k = 0; k < 2; k++)
            {
                string str = k == 0 ? lstceeHeaderPoints[0]: lstceeHeaderPoints[lstceeHeaderPoints.Count - 1];
                string[] tokens = str.Split('|');
                string dFlangeWidth = tokens[3];
                XYZ startCMUpoint = new XYZ(double.Parse(tokens[0]), double.Parse(tokens[1]), double.Parse(tokens[2]));

                string str2 = k == 0 ? lstceeHeaderPoints[1] : lstceeHeaderPoints[lstceeHeaderPoints.Count - 2]; ;
                string[] tokens2 = str2.Split('|');
                XYZ startSecondPoint = new XYZ(double.Parse(tokens2[0]), double.Parse(tokens2[1]), double.Parse(tokens2[2]));

                if ((spanDirection == LineType.vertical && startCMUpoint.X != startSecondPoint.X) ||
                    (spanDirection == LineType.Horizontal && startCMUpoint.Y != startSecondPoint.Y))
                {
                    XYZ newStartCMUPoint = new XYZ(startSecondPoint.X, startCMUpoint.Y, startCMUpoint.Z);
                    {
                        string updatedString = newStartCMUPoint.X.ToString() + "|"
                                                    + newStartCMUPoint.Y.ToString() + "|"
                                                    + newStartCMUPoint.Z.ToString() + "|"
                                                    + dFlangeWidth;

                        if (k==0)
                            lstceeHeaderPoints[0] = updatedString;
                        else
                            lstceeHeaderPoints[lstceeHeaderPoints.Count - 1] = updatedString;
                    }

                    }
            }
            Dictionary<XYZ, string> ceeHeaderValues = new Dictionary<XYZ, string>();
            
            foreach (string str in lstceeHeaderPoints)
            {
                string[] tokens = str.Split('|');
                string[] tokens2 = tokens[3].Split(';');
                XYZ point = new XYZ(double.Parse(tokens[0]), double.Parse(tokens[1]), double.Parse(tokens[2]));
                ceeHeaderValues.Add(point, tokens[3]);
            }

            int i = 1;
            foreach (KeyValuePair<XYZ, string> kvp in ceeHeaderValues)
            {
                string strWallTypeAndGuage = kvp.Value.ToString();
                XYZ ceeHeaderPoint = kvp.Key;

                string[] tokens = strWallTypeAndGuage.Split(';');
                double dAdjustmentFactor  = double.Parse(tokens[1]);
                string strPointType = tokens[0].ToString();

                double dhalfWebWidth = 0.0;
                if (tokens.Length == 4 && !string.IsNullOrEmpty(tokens[2]))
                    dhalfWebWidth  = double.Parse(tokens[2]) / 2.0;    

                if (strPointType == "StartCMU" || strPointType == "EndStud")
                {
                    if (spanDirection == LineType.vertical)
                        ceeHeaderPoint += new XYZ(0, dAdjustmentFactor, 0);
                    else
                        ceeHeaderPoint += new XYZ(dAdjustmentFactor, 0, 0);
                }
                else if (strPointType == "StartStud" || strPointType == "EndCMU")
                {
                    if (spanDirection == LineType.vertical)
                        ceeHeaderPoint += new XYZ(0, -dAdjustmentFactor, 0);
                    else
                        ceeHeaderPoint += new XYZ(-dAdjustmentFactor, 0, 0);
                }
                else
                {
                    // Flange Width Adjustments
                    if (i % 2 == 1)
                    {
                        if (spanDirection == LineType.vertical)
                            ceeHeaderPoint += new XYZ(0, strPointType == "CMU" ? dAdjustmentFactor : -dAdjustmentFactor, 0);
                        else
                            ceeHeaderPoint += new XYZ(strPointType == "CMU" ? dAdjustmentFactor : -dAdjustmentFactor, 0, 0);
                    }
                    else
                    {
                        if (spanDirection == LineType.vertical)
                            ceeHeaderPoint += new XYZ(0, strPointType == "CMU" ? -dAdjustmentFactor : dAdjustmentFactor, 0);
                        else
                            ceeHeaderPoint += new XYZ(strPointType == "CMU" ? -dAdjustmentFactor : dAdjustmentFactor, 0, 0);
                    }
                }
                ceeHeaderPointsDict.Add(ceeHeaderPoint, dhalfWebWidth);
                i++;
            }
            
            return ceeHeaderPointsDict;
        }

        private bool IdentifyCeeHederPoints(ref XYZ startPt, List<InputLine> inputlineList, out List<string> ceeHeaderPts )
        {
            bool bCanPlaceCeeHeaders = false;
            ceeHeaderPts  = new List<string >();


            double elevation = inputlineList[0].startpoint.Z;

            // Compute the span Line type
            List<XYZ> spanStartPts = CeeHeaderBoundaries.GetSpanStartingGrid();
            LineType spanLineType = MathUtils.ApproximatelyEqual(spanStartPts[0].X, spanStartPts[1].X) ? LineType.vertical : LineType.Horizontal;
 
            // Get the boundaries for Cee Header placements
            List<XYZ> boundingBoxExtens = CeeHeaderBoundaries.GetFirstBoundingBoxCoordinates();

            // Check if we reached end of the span.
            XYZ endPoint = CeeHeaderBoundaries.GetExtentsEndPoint();
            if ((spanLineType == LineType.vertical && startPt.X < endPoint.X && !MathUtils.ApproximatelyEqual(startPt.X,endPoint.X)) || 
                (spanLineType == LineType.Horizontal && startPt.Y < endPoint.Y & !MathUtils.ApproximatelyEqual(startPt.Y, endPoint.Y)))
                bCanPlaceCeeHeaders = true;

            // if we reached the end just return without further computations
            if (!bCanPlaceCeeHeaders)
                return false;

            // Get the boundingbox outline based on the Span starting grid orientation
            Outline outline = GetBoundingBoxOutline (spanLineType, startPt, boundingBoxExtens, elevation);

            List<string> points = new List<string> ();

            // Get Input lines at given span
            points.AddRange(GetInputLinePoints(ref outline, inputlineList, spanLineType, ref startPt));

            double dCeeHeaderCoordinate = 0.0;
            if (points.Count > 0)
            {
                string[] tokens = points[0].Split('|');
                double XCoord = double.Parse(tokens[0]);
                double YCoord = double.Parse(tokens[1]);

                dCeeHeaderCoordinate = spanLineType == LineType.vertical ? XCoord : YCoord;
            }

            // Get the building endpoints
            points.AddRange(GetEndPoints(outline, inputlineList, spanLineType, startPt, elevation, dCeeHeaderCoordinate));

            bool bCMUAtStart = points.Find(e => e.Contains("StartCMU")) != null;
            bool bCMUAtEnd = points.Find(e => e.Contains("EndCMU")) != null; 

            // Get the CMU wall endpoints in a given bounding box
            points.AddRange(GetCMUWallPoints(outline, spanLineType, dCeeHeaderCoordinate, elevation, bCMUAtStart, bCMUAtEnd));

            // Sort the points according to Span Grid type
            ceeHeaderPts = (spanLineType == LineType.vertical) ? points.OrderBy(elem => double.Parse(elem.Split('|')[1])).ToList() : points.OrderBy(elem => double.Parse(elem.Split('|')[0])).ToList();
            
            return bCanPlaceCeeHeaders;

        }

        private List<string> GetEndPoints(Outline outline, List<InputLine> inputlineList, LineType spanLineType, XYZ startPoint, double elevation, double dCeeHeaderCoordinate)
        {

            // If span line is vertical, we want to get the Y coordinates of the two extremities of the exterior
            // if Horizontal, we need X coordinates of extremities of the exterior

            double min = 100000.0, max = 0.0;

            string startWallType = "";
            string endWallType = "";
            double dStartAdjustment = 0.0;
            double dEndAdjustment = 0.0;

            // Construct a bounding box filter with the outline
            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

            // Apply the filter to the elements in the active document to retrieve input lines in a range
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> GenericModelElems = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();

            // Collect LB lines which are parallel to the slope direction and sort them accordingly
            List<InputLine> targetInputLines = new List<InputLine>();
            foreach (Element genericModelElem in GenericModelElems)
            {
                InputLine input = inputlineList.FirstOrDefault(il => il.id == genericModelElem.Id && (il.strWallType == "Ex" || il.strWallType == "Ex w/ Insulation"));
                if (input.id != null)
                {
                    LineType inputlineType = GenericUtils.GetLineType(input);
                    if (inputlineType != spanLineType) // Lines perpendicular to span type
                    {
                        double temp = (spanLineType == LineType.vertical ? input.startpoint.Y : input.startpoint.X);
                        if (temp < min)
                        {
                            min = temp;
                            startWallType = "StartStud";
                            dStartAdjustment = GenericUtils.WebWidth(input.strStudType) / 2.0;
                        }
                        if (temp > max)
                        {
                            max = temp;
                            endWallType = "EndStud";
                            dEndAdjustment = GenericUtils.WebWidth(input.strStudType) / 2.0;
                        }
                    }
                }
            }

            // Apply the filter to the elements in the active document to retrieve input lines in a range
            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
            IList<Wall> wallElements = collector2.WherePasses(filter).OfCategory(BuiltInCategory.OST_Walls).Cast<Wall>().ToList();

            // Collect CMU walls in the range that are parallel to span grid
            foreach (Wall wall in wallElements)
            {
                if (!wall.Name.Contains("Masonry"))
                    continue;

                XYZ startPt = null, endPt = null;
                GenericUtils.GetlineStartAndEndPoints(wall, out startPt, out endPt);

                LineType lineType = MathUtils.ApproximatelyEqual(startPt.X, endPt.X) ? LineType.vertical : LineType.Horizontal;

                if (lineType != spanLineType)
                {
                    double temp = (spanLineType == LineType.vertical ? startPt.Y : startPt.X);
                    if (temp < min)
                    {
                        dStartAdjustment = wall.WallType.Width / 2.0;
                        startWallType = "StartCMU";
                        min = temp;
                    }
                    if (temp > max)
                    {
                        dEndAdjustment = wall.WallType.Width / 2.0;
                        endWallType = "EndCMU";
                        max = temp;
                    }
                }
            }


            // Even after looking for CMU and Ex Lines if we did not find them, use the floor boundary to compute the Min, Max
            XYZ probePoint = new XYZ();
            foreach (Element genModelElems in GenericModelElems)
            {
                InputLine input = inputlineList.FirstOrDefault(il => il.id == genModelElems.Id && (il.strWallType == "LB" || il.strWallType == "LBS"));
                if (input.id != null)
                {
                    probePoint = input.startpoint;
                    break;
                }
            }

            if (min == 100000.0)
                min = ComputeExtremitiesFromFloorBoundaries(probePoint,elevation, dCeeHeaderCoordinate, spanLineType, false);

            if (max == 0)
                max = ComputeExtremitiesFromFloorBoundaries(probePoint, elevation, dCeeHeaderCoordinate, spanLineType, true);

            if (min == max)
            {
                min = ComputeExtremitiesFromFloorBoundaries(probePoint, elevation, dCeeHeaderCoordinate, spanLineType, false);
                max = ComputeExtremitiesFromFloorBoundaries(probePoint, elevation, dCeeHeaderCoordinate, spanLineType, true);
            }

            List<string> points = new List<string>();

            List<XYZ> endPts = new List<XYZ>();

            XYZ start = null, end = null;

            if (spanLineType == LineType.vertical)
            {
                start = new XYZ(dCeeHeaderCoordinate == 0.0 ? startPoint.X : dCeeHeaderCoordinate, min, elevation);
                end = new XYZ(dCeeHeaderCoordinate == 0.0 ? startPoint.X : dCeeHeaderCoordinate, max, elevation);
            }
            else
            {
               start = new XYZ(min, dCeeHeaderCoordinate == 0.0 ? startPoint.Y : dCeeHeaderCoordinate, elevation);
               end = new XYZ(max, dCeeHeaderCoordinate == 0.0 ? startPoint.Y : dCeeHeaderCoordinate, elevation);
            }

            points.Add(start.X
                        + "|" + start.Y.ToString()
                        + "|" + start.Z.ToString()
                        + "|" + startWallType + ";" + dStartAdjustment.ToString() + ";");

            points.Add(end.X
                        + "|" + end.Y.ToString()
                        + "|" + end.Z.ToString()
                         + "|" + endWallType + ";" + dEndAdjustment.ToString() + ";");
            return points;
        }

        private double ComputeExtremitiesFromFloorBoundaries(XYZ probePoint, double elevation, double dCeeHeaderCoordinate, LineType spanLineType, bool bMax)
        {
            // Get Floor Boundaries - we will get a collection of loops.

            Element floorElement = GenericUtils.GetNearestFloorOrRoof(m_inputLineElevationLevel, probePoint, doc);

            if (floorElement != null)
            {
                Options opt = doc.Application.Create.NewGeometryOptions();
                List<List<XYZ>> floorCurves = GenericUtils.GetFloorCurves(floorElement, opt);

                List<double> floorCoordinates = new List<double>();
                foreach (List<XYZ> cureveLoop in floorCurves)
                {
                    // collect all those curves which are horizontal and within the range of the ceeheader coordinate
                    for (int i = 0; i < cureveLoop.Count; i++)
                    {

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

                        LineType curevetype = MathUtils.ApproximatelyEqual(startPt.X, endPt.X) ? LineType.vertical : LineType.Horizontal;

                        if (spanLineType != curevetype)
                        {
                            // We come here when we have horizontal curves. Check if ceeHeader Coordinate is in between Start and end.
                            // if yes collect its Y coordinate into a list;
                            if (curevetype == LineType.Horizontal)
                            {
                                //Coordinates may be inverted
                                if ((startPt.X < dCeeHeaderCoordinate && endPt.X > dCeeHeaderCoordinate) ||
                                    (startPt.X > dCeeHeaderCoordinate && endPt.X < dCeeHeaderCoordinate))
                                    floorCoordinates.Add(startPt.Y);
                            }
                            // We come here when we have vertical curves. Check if ceeHeader Coordinate is in between Start and end.
                            // if yes collect its Y coordinate into a list;
                            else
                            {
                                if ((startPt.Y < dCeeHeaderCoordinate && endPt.Y > dCeeHeaderCoordinate) ||
                                    (startPt.Y > dCeeHeaderCoordinate && endPt.Y < dCeeHeaderCoordinate))
                                    floorCoordinates.Add(startPt.X);
                            }
                        }
                    }
                }
                floorCoordinates.Sort();

                if (floorCoordinates.Count > 0)
                {
                    if (bMax)
                        return floorCoordinates.ElementAt(floorCoordinates.Count - 1);
                    else
                        return floorCoordinates.ElementAt(0);
                }

                if (bMax)
                    return 0.0;
                else
                    return 100000.0;
            }

            return 0.0;
        }

        private List<string> GetCMUWallPoints(Outline outline, LineType spanLineType, double dCeeHeaderCoordinate, double dElevation, bool bCMUAtStart, bool bCMUAtEnd)
        {
            List<string> CMUPoints = new List<string>();

            //Typically when we add CMU points, we are following below strategy
            // If cee header placement direction is vetical and there is a vertical CMU wall, we are taking CMU points from the corresponding perpendicular walls so that the headers
            // are properly terminated at CMU. In the process we are reducing the BB by 2 feet on each side to avoid double counting of CMU once in end points and once in CMU points

            // Ine RA-35 is a unique case where we have room exactly matching the grid where CMU is placed + Exterior which is resulting in a combination of Ex Line and CMU wall exactly coinciding at the Cee header placement location

            // In this case the Endpoints would be marked as - Start Stud / End Stud, and as a result of shrinking BB, only one among the 2 CMU perpendicular walls will be added resulting in incorrect placement.

            // so we introduce 2 flags bCMUAtStart and bCMUAtEnd, the shrinking will happen at each end only of the boolean is set to true else no Shrinking

            // we donot want to double count exterior CMU Walls once in end points and once in this method
            // So, reduce the outline by 2 feet on either sides
            if (spanLineType == LineType.vertical)
            {
                double YCoordinateAtStart = bCMUAtStart ? outline.MinimumPoint.Y + 2.0 : outline.MinimumPoint.Y;
                double YCoordinateAtEnd = bCMUAtEnd ? outline.MaximumPoint.Y - 2.0 : outline.MaximumPoint.Y;

                outline = new Outline(new XYZ(outline.MinimumPoint.X, YCoordinateAtStart, outline.MinimumPoint.Z),
                                        new XYZ(outline.MaximumPoint.X, YCoordinateAtEnd, outline.MaximumPoint.Z));
            }
            else
            {
                double XCoordinateAtStart = bCMUAtStart ? outline.MinimumPoint.X + 2.0 : outline.MinimumPoint.X;
                double XCoordinateAtEnd = bCMUAtEnd ? outline.MaximumPoint.X - 2.0 : outline.MaximumPoint.X;

                outline = new Outline(new XYZ(outline.MinimumPoint.X + 2.0, XCoordinateAtStart, outline.MinimumPoint.Z),
                                       new XYZ(outline.MaximumPoint.X - 2.0, XCoordinateAtEnd, outline.MaximumPoint.Z));
            }

            // Construct a bounding box filter with the outline
            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

            // Apply the filter to the elements in the active document to retrieve input lines in a range
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Wall> wallElements = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_Walls).Cast<Wall>().ToList();


            // Collect CMU walls in the range that are parallel to span grid
            foreach (Wall wall in wallElements)
            {
                if (!wall.Name.Contains("Masonry"))
                    continue;
                double halfWidth = wall.WallType.Width / 2.0;

                XYZ startPt = null, endPt = null;
                    GenericUtils.GetlineStartAndEndPoints(wall, out startPt, out endPt);

                LineType lineType = MathUtils.ApproximatelyEqual(startPt.X, endPt.X) ? LineType.vertical : LineType.Horizontal;

                string startString = "";

                if (lineType != spanLineType && lineType == LineType.Horizontal)
                {
                    startString = dCeeHeaderCoordinate.ToString()
                                           + "|" + startPt.Y.ToString()
                                           + "|" + dElevation.ToString()
                                            + "|" + "CMU" + ";" + halfWidth.ToString() + ";";

                }
                else if (lineType != spanLineType && lineType == LineType.vertical)
                {
                    startString = dCeeHeaderCoordinate.ToString()
                       + "|" + startPt.Y.ToString()
                       + "|" + dElevation.ToString()
                        + "|" + "CMU" + ";" + halfWidth.ToString() + ";";
                }

                
                if (!string.IsNullOrEmpty(startString))
                    CMUPoints.Add(startString); 
            }

            // Sort lines vertically       
            return CMUPoints;
        }

        private List<string> GetInputLinePoints(ref Outline outline, List<InputLine> inputlineList, LineType spanLineType, ref XYZ startPt)
        {
            // This method tries to find LB walls at given span
            // if not it updates the startpoints and outline by 2 feet each time - for 4 times
            // Get discontinuties of LB Lines
            // 
            XYZ initialStart = startPt;
            Outline initialOutline = outline;
            
            List<string> linePoints = new List<string>();

            for (int i = 0; i < 4; i++)
            {

                // Construct a bounding box filter with the outline
                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

                // Apply the filter to the elements in the active document to retrieve input lines in a range
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IList<Element> GenericModelElems = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();


                // Collect LB lines which are parallel to the slope direction and sort them accordingly
                List<InputLine> targetInputLines = new List<InputLine>();
                foreach (Element genericModelElem in GenericModelElems)
                {
                    InputLine input = inputlineList.FirstOrDefault(il => il.id == genericModelElem.Id && (il.strWallType == "LB" || il.strWallType == "LBS"));


                    if (input.id != null)
                    {
                        LineType inputlineType = GenericUtils.GetLineType(input);
                        if (inputlineType == spanLineType)
                            targetInputLines.Add(input);
                    }
                }

                foreach (var targetline in targetInputLines)
                {
                    double dFlangeWidth = GenericUtils.FlangeWidth(targetline.strStudType);
                    double dWebWidth = GenericUtils.WebWidth(targetline.strStudType);

                    bool bDoubleStudAtStart = targetline.strDoubleStudType == "At Ends" || targetline.strDoubleStudType == "At Ends - L";
                    bool bDoubleStudAtEnd = targetline.strDoubleStudType == "At Ends" || targetline.strDoubleStudType == "At Ends - R";

                    string startString = string.Format("{0}|{1}|{2}|Stud;{3};{4};", targetline.startpoint.X.ToString(),
                                                                                targetline.startpoint.Y.ToString(),
                                                                                targetline.startpoint.Z.ToString(),
                                                                                (bDoubleStudAtStart ? dFlangeWidth * 2 : dFlangeWidth).ToString(),
                                                                                dWebWidth.ToString());

                    string endString = string.Format("{0}|{1}|{2}|Stud;{3};{4};", targetline.endpoint.X.ToString(),
                                                                targetline.endpoint.Y.ToString(),
                                                                targetline.endpoint.Z.ToString(),
                                                                (bDoubleStudAtEnd ? dFlangeWidth * 2 : dFlangeWidth).ToString(),
                                                                dWebWidth.ToString());

                    linePoints.Add(startString);
                    linePoints.Add(endString);

                }

                // Special Processing - look for LBs before span
                if (linePoints.Count == 0)
                {
                    if (spanLineType == LineType.vertical)
                    {
                        startPt = startPt + new XYZ(-2.0, 0.0, 0.0);
                        outline = new Outline(new XYZ(outline.MinimumPoint.X - 2.0, outline.MinimumPoint.Y, outline.MinimumPoint.Z),
                                                new XYZ(outline.MaximumPoint.X - 2.0, outline.MaximumPoint.Y, outline.MaximumPoint.Z)) ;
                    }
                    else
                    {
                        startPt = startPt + new XYZ(0.0, -2.0, 0.0);
                        outline = new Outline(new XYZ(outline.MinimumPoint.X , outline.MinimumPoint.Y - 2.0, outline.MinimumPoint.Z),
                        new XYZ(outline.MaximumPoint.X , outline.MaximumPoint.Y - 2.0, outline.MaximumPoint.Z));
                    }
                }
                else
                    break;
            }

            // if at this stage we have zero points we need to add Cee Header points as per NLB Studs
            if (linePoints.Count == 0)
            {
                startPt = initialStart;
                outline = initialOutline;

                for (int j = 0; j < 5; j++)
                {
                    // Construct a bounding box filter with the outline
                    BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

                    // Apply the filter to the elements in the active document to retrieve input lines in a range
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    IList<Element> postElements = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_StructuralColumns).ToElements();
                    List <XYZ> targetPoints = new List<XYZ>();
                    foreach (Element postElement in postElements)
                    {
                        FamilyInstance post = doc.GetElement(postElement.Id) as FamilyInstance;
                        Location location = post.Location;
                        if (location is LocationPoint locationPoint)
                        {
                            XYZ point  = locationPoint.Point;
                            targetPoints.Add(new XYZ(point.X, point.Y, startPt.Z));
                        }

                    }

                    foreach (XYZ targetpoint in targetPoints)
                    {
                        string startString = string.Format("{0}|{1}|{2}|Stud;0.0;", targetpoint.X.ToString(),
                                                                               targetpoint.Y.ToString(),
                                                                                targetpoint.Z.ToString()
                                                                                );

                        // each point 2 times. only then when we process points like 1-2, 2-3, 3-4 etc, we will get continuous headers
                        linePoints.Add(startString);
                        linePoints.Add(startString);
                    }

                    if (linePoints.Count == 0)
                    {
                        if (spanLineType == LineType.vertical)
                        {
                            startPt = startPt + new XYZ(-2.0, 0.0, 0.0);
                            outline = new Outline(new XYZ(outline.MinimumPoint.X - 2.0, outline.MinimumPoint.Y, outline.MinimumPoint.Z),
                                                    new XYZ(outline.MaximumPoint.X - 2.0, outline.MaximumPoint.Y, outline.MaximumPoint.Z));
                        }
                        else
                        {
                            startPt = startPt + new XYZ(0.0, -2.0, 0.0);
                            outline = new Outline(new XYZ(outline.MinimumPoint.X, outline.MinimumPoint.Y - 2.0, outline.MinimumPoint.Z),
                            new XYZ(outline.MaximumPoint.X, outline.MaximumPoint.Y - 2.0, outline.MaximumPoint.Z));
                        }
                    }
                    else
                        break;
                }
                
            }

            return linePoints;
        }


        private Outline GetBoundingBoxOutline(LineType spanLineType, XYZ startPt, List<XYZ> boundingBoxExtens, double elevation)
        {
            Outline outline = null;
            
            if (spanLineType == LineType.vertical)
                outline = new Outline(
                   new XYZ(startPt.X - 1.0,
                   boundingBoxExtens[0].Y - 1.0,
                   elevation - 0.5),
                   new XYZ(startPt.X + 1.0,
                  boundingBoxExtens[1].Y + 1.0,
                  elevation + 0.5));
            else
                outline = new Outline(
                   new XYZ(boundingBoxExtens[0].X - 1.0,
                   startPt.Y - 1.0,
                   elevation - 0.5),
                   new XYZ(boundingBoxExtens[1].X + 1.0,
                  startPt.Y + 1.0,
                  elevation + 0.5));

            return outline;
        }


        private CeeHeaderSettings GetCeeHeaderSettingsForGivenLevel(string levelName)
        {
            CeeHeaderSettings ceeHeaderSettings = GlobalSettings.lstCeeHeaderSettings.Find(temp => temp.strGridName == levelName); 
            return ceeHeaderSettings;
        }

        private Dictionary<double, List<InputLine>> SortInputLinesByElevation(List<InputLine> colInputLines)
        {
            Dictionary<double, List<InputLine>> sortedInputLineCollection = new Dictionary<double, List<InputLine>>();
            foreach (InputLine inputLine in colInputLines) 
            {
                double zCoord = inputLine.startpoint.Z;
                if(!sortedInputLineCollection.ContainsKey(zCoord))
                {
                    sortedInputLineCollection[zCoord] = new List<InputLine>();
                }
                sortedInputLineCollection[zCoord].Add(inputLine);
            }

            return sortedInputLineCollection;
        }
    }
}
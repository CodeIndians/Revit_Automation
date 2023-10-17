using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Utils;
using Revit_Automation.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections;

namespace Revit_Automation
{
    // Top tracks
    // Need to check perpendicular and parallel intersections and use the table to know the length
    // First process Ex lines. Take into account top track at rake side parameter. Eave side - perpendicular to slope, rake side parallel to slope
    // At exterior, top track should also be placed at openings
    // Then process lines that are perpendicular to slope
    // Then Process lines parallel to slope similar to the cee header processing and hallway.
    // At roof level top track will have to be placed at a slope
    // Splice at web or stud center determines the splice parameter for top tracks greater than preferred / Max length
    // Top track round off parameter. we need to consider while placing
    // Hallway 5 feet. 
    // Lapping always at stud
    // This has mixture of 3 logics, -> Walls, Bottom tracks, and Cee Headers.

    // At each elevation, construct thin bounding boxes and collect lines that fall in it, if BB is horizontal take only Horizontal lines and vice versa
    // Sort them from left to right and bottom to top
    // Initialize a top track structure with startpoint, endpoint and type of the TT, List <elementID>
    // Starting from the first line check if the next line is within 5'6" from the end. if no, we have a disconnected wall line
    // between 0 - 1' continuous and perperdicular, 5'0 - 5'6" separated by hallway.
    // Process Continuities. if greater than 20', we need to place in preferred lengths.
    // 
    internal class TopTrackCreator
    {
        private Document m_Document;
        private Form1 m_Form;
        private double dTopTrackMaxLength;
        private double dTopTrackPreferredLength;
        internal bool bSelectedModelling;

        internal struct topTrackPlacements
        {
            public XYZ startPoint;
            public XYZ endPoint;
            public string strTopTrackSymbol;
        }

        internal enum inputLineRelations 
        {
            SingleSelfNoPerpendicular,
            SingleSelfAndPerpendicular,
            MultipleSelfAndPerpendicular,
            MultipleSelfParallell,
            MultipleSelfParallelSeparatedyByHallway
        }

        public TopTrackCreator(Document doc, Form1 form)
        {
            this.m_Document = doc;
            this.m_Form = form;
            dTopTrackMaxLength = GlobalSettings.s_dTopTrackMaxLength;
            dTopTrackPreferredLength = GlobalSettings.s_dTopTrackPrefLength;
        }

        internal void CreateModel(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            using (Transaction tx = new Transaction(m_Document))
            {
                m_Form.PostMessage("");
                m_Form.PostMessage("\n Starting placement of Top tracks");
                GenericUtils.SupressWarningsInTransaction(tx);
                tx.Start("Generating Model");
                PlaceTopTracks(colInputLines, levels);
                m_Form.PostMessage("\n Finished placement of Top tracks");
                tx.Commit();
            }
        }

        // To Debug a particular area, override the line collection logic -Instead of building BB at each
        // Case 1 :- Self and Perpendicular; case 2 :- Multiple self with perpendicular in between
        // Case 3:- Parallel ; case 4 - Parallel and divided by hallway
        private void PlaceTopTracks(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            Logger.logMessage("Method - PlaceTopTracks");
            List<topTrackPlacements> topTrackPlacements = new List<topTrackPlacements>();
            
            // Step - 1 Sort the input lines by levels
            // Step - 2 For each elevation compute the extremities
            // Step - 3 build thin volumes at each feet and compute the lines
            // Step 3.1 Process lines in each thin volumd
            // Step 3.1.1. - Sort lines in left - right and bottom - top directions
            // step 3.1.2. - Each line and next line distance > 6' Case 1, 0' - case 2, , 0'-1' Case 3, 5'-6' Case 4
            // Step 3.1.3. - A toptrack placement structure -> Type, startPt, EndPt, included Lines needs to be filled
            // Step 4 - After computing all toptrack placement elements start placing them
            // Step 5 - For top tracks under the roof, and parallel to slope,  they need to be inclined along the roof slope.

            Dictionary<double, List<InputLine>> sortedInputLineCollection = new Dictionary<double, List<InputLine>>();
            sortedInputLineCollection = SortInputLinesByElevation(colInputLines);

            foreach (KeyValuePair<double, List<InputLine>> kvp in sortedInputLineCollection)
            {
                List<InputLine> inputLinelist = kvp.Value;
                double elevation = kvp.Key;

                double HMin = 100000, HMax = 0 , VMin = 100000, VMax = 0 ;
                foreach (InputLine il in inputLinelist)
                {
                    if (il.strWallType == "Ex" || il.strWallType == "Ex w/ Insulation" || il.strWallType == "Ex Opening")
                    {
                        LineType lineType = GenericUtils.GetLineType(il);

                        if (lineType == LineType.vertical)
                        {
                            if (il.startpoint.X < HMin)
                                HMin = il.startpoint.X;
                            if (il.startpoint.X > HMax)
                                HMax = il.startpoint.X;
                        }
                        else
                        {
                            if (il.startpoint.Y < VMin)
                                VMin = il.startpoint.Y;
                            if (il.startpoint.Y > VMax)
                                VMax = il.startpoint.Y;
                        }
                    }
                }

                // Horizontal Thin volumes
                double dHeight = VMin;
                while (dHeight < VMax + 1)
                {
                    Outline outline = new Outline(new XYZ(HMin - 0.5, dHeight - 0.5, elevation - 0.5),
                                                    new XYZ(HMax + 0.5, dHeight + 0.5, elevation + 0.5));

                    BoundingBoxIntersectsFilter bbIntersectFilter = new BoundingBoxIntersectsFilter(outline);

                    ICollection<Element> elemIDs = new FilteredElementCollector(m_Document, m_Document.ActiveView.Id).WherePasses(bbIntersectFilter).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();
                    
                    // Filter only those lines that are horizontal
                    List <ElementId> horizontalInputLinesIDs = new List<ElementId>();
                    foreach (Element elem in elemIDs)
                    {
                        XYZ startPt = null, endPt = null;
                        GenericUtils.GetlineStartAndEndPoints(elem, out startPt, out endPt);

                        if (MathUtils.ApproximatelyEqual( startPt.Y , endPt.Y))
                            horizontalInputLinesIDs.Add(elem.Id);

                    }

                    // Get the Input lines
                    List<InputLine> horizontalines = new List<InputLine>();
                    foreach (ElementId horizElemID in horizontalInputLinesIDs)
                    {
                        InputLine temp = colInputLines.Where(il => il.id == horizElemID).FirstOrDefault();
                        horizontalines.Add(temp);
                    }

                    // sort them
                    if (horizontalines.Count > 0)
                    {
                        horizontalines = horizontalines.OrderBy(il => il.startpoint.X).ToList();

#if DEBUG
                        string strLineCollection = "";
                        for (int i = 0; i < horizontalines.Count; i++)
                            strLineCollection += (horizontalines[i].id.ToString()) + ",";
                        Debug.WriteLine($"{strLineCollection} \n");
#endif
                        while (horizontalines.Count > 0)
                        {
                            for (int i = 0; i < horizontalines.Count; i++)
                            {
                                // Check the relation Between this line and the next line
                                // 
                            }
                        }
                    }
                    // increment by 1 feet
                    dHeight += 1; 
                }

                // verical Thin volumes
            }
        }

        private void PlaceTopTrack(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
            Logger.logMessage("Method -PlaceBottomTrack");

            if (dTopTrackMaxLength == 0 || dTopTrackPreferredLength == 0)
            {
                TaskDialog.Show("Automation Error", "Bottom Track Preferred/Max lengths are not set");
                return;
            }

            double dLineLength = 0.0;
            List<double> BTPlacementLengths = new List<double>();
            // Get Line End points.
            XYZ pt1 = null, pt2 = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);


            // Get the orientation of the line
            LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

            // Check for wall lines shorter than max length and process them as a single track
            // Else 
            if (lineType == LineType.Horizontal)
                dLineLength = (Math.Abs(pt2.X - pt1.X));
            else if (lineType == LineType.vertical)
                dLineLength = (Math.Abs(pt2.Y - pt1.Y));

            if (dLineLength > dTopTrackMaxLength)
            {
                while (dLineLength > dTopTrackMaxLength)
                {
                    BTPlacementLengths.Add(dTopTrackPreferredLength);
                    dLineLength -= dTopTrackPreferredLength;
                }
                BTPlacementLengths.Add(dLineLength);
            }
            else
            {
                BTPlacementLengths.Add(dLineLength);
            }

            XYZ refPoint = pt1;

            // Place Bottom tracks. 
            foreach (double length in BTPlacementLengths)
            {
                XYZ endPoint = null;

                if (lineType == LineType.Horizontal)
                    endPoint = new XYZ(refPoint.X + length, refPoint.Y, refPoint.Z);
                else
                    endPoint = new XYZ(refPoint.X, refPoint.Y + length, refPoint.Z);

                Line newInputLine = Line.CreateBound(refPoint, endPoint);

                FamilySymbol symbol = GetTopTrackSymbol(inputLine);

                if (symbol != null && !symbol.IsActive)
                    symbol.Activate();

                FamilyInstance lineElement = m_Document.GetElement(inputLine.id) as FamilyInstance;
                Level level = lineElement.Host as Level;

                FamilyInstance topTrackInstance = m_Document.Create.NewFamilyInstance(newInputLine, symbol, level, StructuralType.Beam);

                Parameter zJustification = topTrackInstance.get_Parameter(BuiltInParameter.Z_JUSTIFICATION);
                if (zJustification != null)
                {
                    zJustification.Set(((double)ZJustification.Origin));
                }
                StructuralFramingUtils.DisallowJoinAtEnd(topTrackInstance, 0);

                StructuralFramingUtils.DisallowJoinAtEnd(topTrackInstance, 1);

                m_Form.PostMessage(string.Format("Placing Top Track with ID : {0} between {1}, {2} and {3}, {4}", topTrackInstance.Id, refPoint.X, refPoint.Y, endPoint.X, endPoint.Y));

                refPoint = endPoint;
            }
        }

        private Dictionary<double, List<InputLine>> SortInputLinesByElevation(List<InputLine> colInputLines)
        {
            Dictionary<double, List<InputLine>> sortedInputLineCollection = new Dictionary<double, List<InputLine>>();
            foreach (InputLine inputLine in colInputLines)
            {
                double zCoord = inputLine.startpoint.Z;
                if (!sortedInputLineCollection.ContainsKey(zCoord))
                {
                    sortedInputLineCollection[zCoord] = new List<InputLine>();
                }
                sortedInputLineCollection[zCoord].Add(inputLine);
            }

            return sortedInputLineCollection;
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

        private FamilySymbol GetTopTrackSymbol(InputLine inputLine)
        {
            string topTrackFamilyName = "Top Track";
            string topTrackSymbolName = string.Format("{0} x {1}ga", inputLine.strTopTrackSize, inputLine.strTopTrackGuage);

            FamilySymbol sym = SymbolCollector.GetBottomOrTopTrackSymbol(topTrackFamilyName, topTrackSymbolName);

            return sym;
        }
    }
}
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;

namespace Revit_Automation
{
    internal class CeeHeaderCreator
    {
        private Document doc;
        private Form1 form;

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
        public CeeHeaderCreator(Document doc, Form1 form)
        {
            this.doc = doc;
            this.form = form;
        }

        internal void CreateModel(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Placing Cee Headers");
                Dictionary<double, List<InputLine>> sortedInputLineCollection = new Dictionary<double, List<InputLine>>();
                sortedInputLineCollection = SortInputLinesByElevation(colInputLines);

                // Get the Span
                m_DeckSpan = double.Parse(GlobalSettings.s_strDeckSpan);

                // Compute of the slope is horizontal or vertical
                ComputeSlopeDirection(colInputLines);

                // Compute the start point for CeeHeader computation
                ComputeStartPoint(colInputLines);

                foreach (KeyValuePair<double, List<InputLine>> kvp in sortedInputLineCollection)
                {
                    List<InputLine> list = kvp.Value;
                    double elevation = kvp.Key;

                    InputLine temp = list[0];
                    Level level = GetLevelForInputLine(temp, levels);

                    // Get the settings for this level
                    CeeHeaderSettings ceeHeaderSettings = GetCeeHeaderSettingsForGivenLevel(level.Name);

                    // Place Cee Headers at given lines.
                    PlaceCeeHeaders(ceeHeaderSettings, list, level);

                }

                tx.Commit();
            }
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
            double dSpan = double.Parse(GlobalSettings.s_strDeckSpan);

            List<XYZ> ceeHeaderPts = IdentifyCeeHeaderPoints(InputlineList);


            for (int i = 0; i < ceeHeaderPts.Count - 1; i++)
            {
                XYZ ceeHeaderStartPoint = ceeHeaderPts[i++];
                XYZ ceeHeaderEndPoint = ceeHeaderPts[i];

                bool bHeaderAtHallway = true; // GenericUtils.LineIntersectsHallway(doc, ceeHeaderStartPoint, ceeHeaderEndPoint);
                
                double dElevation = Math.Abs ( ceeHeaderStartPoint.Z - level.Elevation );
                ceeHeaderStartPoint += new XYZ(0, 0, dElevation);
                ceeHeaderEndPoint += new XYZ(0, 0, dElevation);
                FamilySymbol ceeHeaderFamily = SymbolCollector.GetCeeHeadersFamily(bHeaderAtHallway ? ceeHeaderSettings.HallwayCeeHeaderName: ceeHeaderSettings.ceeHeaderName
                    , "Cee Header");

                
                Line bounds = Line.CreateBound(ceeHeaderStartPoint, ceeHeaderEndPoint);
                FamilyInstance ceeHeaderInstance = doc.Create.NewFamilyInstance(bounds, ceeHeaderFamily, level, StructuralType.Beam);
                Parameter PostCLFaceOffsetParam = ceeHeaderInstance.LookupParameter("Post CL Face Offset");
                if (PostCLFaceOffsetParam != null)
                {
                }

                if (bHeaderAtHallway == true && ceeHeaderSettings.HallwayCeeHeaderCount == "Double" || bHeaderAtHallway == false && ceeHeaderSettings.ceeHeaderCount == "Double")
                {
                    Line bounds2 = Line.CreateBound(ceeHeaderEndPoint, ceeHeaderStartPoint);
                    FamilyInstance ceeHeaderInstance2 = doc.Create.NewFamilyInstance(bounds2, ceeHeaderFamily, level, StructuralType.Beam);
                    Parameter TopTrackSizeParam = ceeHeaderInstance.LookupParameter("Post CL Face Offset");
                    if (TopTrackSizeParam != null)
                    {
                    }
                }
            }
        }

        private List<XYZ> IdentifyCeeHeaderPoints(List<InputLine> inputlineList)
        {
            List<XYZ> ceeHeaderPoints = new List<XYZ>();

            if (m_direction == SlopeDirection.Vertical)
            {
                XYZ startPoint = m_startPoint;
                if (m_bStartingFromExterior)
                    startPoint = m_startPoint + new XYZ(m_DeckSpan, 0, 0);

                while (startPoint.X < m_endPoint.X)
                {
                    List <InputLine> targetInputLines  = new List<InputLine>();
                    Outline outline = new Outline(
                    new XYZ(startPoint.X - 0.5,
                    startPoint.Y - 0.5,
                    startPoint.Z - 0.5),
                    new XYZ(startPoint.X + 0.5,
                    m_endPoint.Y + 0.5,
                    startPoint.Z + 0.5));

                    // Create a BoundingBoxIntersects filter with this Outline
                    BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

                    // Apply the filter to the elements in the active document to retrieve posts at a point
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    IList<Element> GenericModelElems = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();

                    // Collect LB lines which are parallel to the slope direction and sort them by Y.
                    foreach (Element genericModelElem in GenericModelElems)
                    {
                        InputLine input = inputlineList.FirstOrDefault(il => il.id == genericModelElem.Id && (il.strWallType == "LB" || il.strWallType == "LBS"));
                        if (input.id != null)
                            targetInputLines.Add(input);
                    }

                    // Sort lines vertically
                    List<XYZ> linePoints = new List<XYZ>();
                    foreach (var targetline in targetInputLines)
                    {
                        linePoints.Add(targetline.startpoint);
                        linePoints.Add(targetline.endpoint);
                    }

                    List<XYZ> sortedList = linePoints.OrderBy(elem  => elem.Y).ToList();

                    for (int i = 1; i < sortedList.Count - 1; i++)
                    {
                        ceeHeaderPoints.Add(sortedList[i++]);
                        ceeHeaderPoints.Add(sortedList[i]);
                    }

                    startPoint = startPoint + new XYZ(m_DeckSpan, 0, 0);
                }
            }
            return ceeHeaderPoints;
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
/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Revit_Automation.CustomTypes;
using System.Windows.Media.Animation;
using Autodesk.Revit.UI.Selection;
using Revit_Automation.Source.Utils;

namespace Revit_Automation.Source
{
    /// <summary>
    /// Class that holds all the data required for placement of the structure - Input Lines
    /// </summary>
    public class InputLineUtility
    {
        /// <summary>
        /// The collection of input lines
        /// </summary>
        public static List<InputLine> colInputLines = new List<InputLine>();

        /// <summary>
        /// This function is used to collect all input lines in the model
        /// </summary>
        /// <param name="doc"> Pointer to the Active document</param>
        public static void GatherInputLines(Document doc, bool bSelected, Selection selection, CommandCode commandcode)
        {
            if (colInputLines != null)
                colInputLines.Clear();

            FilteredElementCollector locationCurvedCol = null;

            if (!bSelected)
            {
                locationCurvedCol
                  = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfCategory(BuiltInCategory.OST_GenericModel);
            }

            else
            {
                // Check if any elements are selected
                if (selection.GetElementIds().Count > 0)
                {
                    // Get the selected elements
                    ICollection<ElementId> selectedIds = selection.GetElementIds();

                    // Create a filter to match elements of the OST_GenericModel category
                    ElementCategoryFilter modelCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_GenericModel);

                    // Create a filtered element collector to filter the selected elements
                    locationCurvedCol = new FilteredElementCollector(doc, selectedIds);

                    // Apply the category filter to the collector
                    locationCurvedCol.WherePasses(modelCategoryFilter);


                }
            }

            foreach (Element locCurve in locationCurvedCol)
            {
                if(locCurve.IsHidden(doc.ActiveView))
                    continue;

                InputLine iLine = new InputLine(); 

                iLine.locationCurve = (LocationCurve)locCurve.Location;

                iLine.startpoint = iLine.locationCurve.Curve.GetEndPoint(0);
                iLine.endpoint = iLine.locationCurve.Curve.GetEndPoint(1);

                iLine.id = locCurve.Id;

                Parameter studGuageParam = locCurve.LookupParameter("Stud Gauge");
                if (studGuageParam != null)
                {
                    iLine.strStudGuage = studGuageParam.AsString();
                }

                Parameter studSizeParam = locCurve.LookupParameter("Stud Size");
                if (studSizeParam != null)
                {
                    iLine.strStudType = studSizeParam.AsString();
                }

                Parameter T62GaugeParam = locCurve.LookupParameter("T62 Gauge");
                if (T62GaugeParam != null)
                {
                    iLine.strT62Guage = T62GaugeParam.AsString();
                }

                Parameter T62TypeParam = locCurve.LookupParameter("T62 Type");
                if (T62TypeParam != null)
                {
                    iLine.strT62Type = T62TypeParam.AsString();
                }

                Parameter WallTypeParam = locCurve.LookupParameter("Wall Type");
                if (WallTypeParam != null)
                {
                    iLine.strWallType = WallTypeParam.AsString();
                }

                Parameter TopTrackGaugeParam = locCurve.LookupParameter("Top Track Gauge");
                if (TopTrackGaugeParam != null)
                {
                    iLine.strTopTrackGuage = TopTrackGaugeParam.AsString();
                }

                Parameter TopTrackSizeParam = locCurve.LookupParameter("Top Track Size");
                if (TopTrackSizeParam != null)
                {
                    iLine.strTopTrackSize = TopTrackSizeParam.AsString();
                }

                Parameter BuildingNameParam = locCurve.LookupParameter("Building Name");
                if (BuildingNameParam != null)
                {
                    iLine.strBuildingName = BuildingNameParam.AsString();
                }

                Parameter BottomTrackGaugeParam = locCurve.LookupParameter("Bottom Track Gauge");
                if (BottomTrackGaugeParam != null)
                {
                    iLine.strBottomTrackGuage = BottomTrackGaugeParam.AsString();
                }

                Parameter BottomTrackSizeParam = locCurve.LookupParameter("Bottom Track Size");
                if (BottomTrackSizeParam != null)
                {
                    iLine.strBottomTrackSize = BottomTrackSizeParam.AsString();
                }

                Parameter FlangeOffsetParam = locCurve.LookupParameter("Flange Offset");
                if (FlangeOffsetParam != null)
                {
                    iLine.dFlangeOfset = FlangeOffsetParam.AsInteger();
                }

                Parameter StudOnCenterParam = locCurve.LookupParameter("Stud O.C.");
                if (StudOnCenterParam != null)
                {
                    iLine.dOnCenter = StudOnCenterParam.AsDouble();
                }

                Parameter DoubleStudParam = locCurve.LookupParameter("Double Stud");
                if (DoubleStudParam != null)
                {
                    iLine.strDoubleStudType = DoubleStudParam.AsString();
                }

                Parameter MaterialTypeParameter = locCurve.LookupParameter("Material Type");
                if (MaterialTypeParameter != null)
                {
                    iLine.strMaterialType = MaterialTypeParameter.AsString();
                }

                // Compute Intersection Points with Grids. 
                GridCollector GridCollectionHelper = new GridCollector(doc);

                var locationCurve = (LocationCurve)locCurve.Location;
                var linecoords = Tuple.Create(locationCurve.Curve.GetEndPoint(0), locationCurve.Curve.GetEndPoint(1));

                // Compute if the Line is parallel, or perpendicular to roof slope.
                XYZ lineDirection = locationCurve.Curve.GetEndPoint(1) - locationCurve.Curve.GetEndPoint(0);
                XYZ roofSlope = GetRoofSlopeDirection(locationCurve.Curve.GetEndPoint(1));
                if (MathUtils.IsParallel(roofSlope, lineDirection))
                    iLine.dirWRTRoofSlope = DirectionWithRespectToRoofSlope.Parallel;
                else
                    iLine.dirWRTRoofSlope = DirectionWithRespectToRoofSlope.Perpendicular;

                    // Compute Grid Intersections for T62 Placement
                    iLine.gridIntersectionPoints = GridCollectionHelper.computeIntersectionPoints(linecoords);

                // Compute Main intesection points for Stud placement offset
                iLine.mainGridIntersectionPoints = GridCollectionHelper.computeIntersectionPoints(linecoords, true);
                
                //Add the line to the collection 
                if (CheckIfPassesCondition(iLine, commandcode))
                    AddInputLine(iLine);
            }
        }

        public static bool CheckIfPassesCondition(InputLine iLine, CommandCode commandcode)
        {
            if (commandcode == CommandCode.All)
                return true;
            
            if (iLine.strWallType == null)
                return true;

            else if (commandcode == CommandCode.ExteriorParallel)
            {
                return (iLine.strWallType.Contains("Ex") &&
                    iLine.dirWRTRoofSlope == DirectionWithRespectToRoofSlope.Parallel);
            }
            else if (commandcode == CommandCode.ExteriorPerpendicular)
            {
                return (iLine.strWallType.Contains("Ex") &&
                    iLine.dirWRTRoofSlope == DirectionWithRespectToRoofSlope.Perpendicular);
            }
            else if (commandcode == CommandCode.InteriorParallel)
            {
                return (!iLine.strWallType.Contains("Ex") &&
                    iLine.dirWRTRoofSlope == DirectionWithRespectToRoofSlope.Parallel);
            }
            else if (commandcode == CommandCode.InteriorPerpendicular)
            {
                return (!iLine.strWallType.Contains("Ex") &&
                    iLine.dirWRTRoofSlope == DirectionWithRespectToRoofSlope.Perpendicular);
            }

            return false;
        }

        /// <summary>
        /// Adds Input line to the collection
        /// </summary>
        /// <param name="inputLine"> The Input Line to be added </param>
        /// <returns>True if the line is added to the collection </returns>
        public static bool AddInputLine(InputLine inputLine)
        {
            colInputLines.Add(inputLine);
            return true;
        }

        public static XYZ GetRoofSlopeDirection(XYZ pt1)
        {
            XYZ SlopeDirect = null;

            RoofObject targetRoof;
            targetRoof.slopeLine = null;

            foreach (RoofObject roof in RoofUtility.colRoofs)
            {
                double Xmin, Xmax, Ymin, Ymax = 0.0;
                Xmin = Math.Min(roof.max.X, roof.min.X);
                Xmax = Math.Max(roof.max.X, roof.min.X);
                Ymin = Math.Min(roof.max.Y, roof.min.Y);
                Ymax = Math.Max(roof.max.Y, roof.min.Y);

                if (pt1.X > Xmin && pt1.X < Xmax && pt1.Y > Ymin && pt1.Y < Ymax)
                {
                    targetRoof = roof;
                    break;
                }
            }

            Curve SlopeCurve = targetRoof.slopeLine;

            if (SlopeCurve != null)
            {
                XYZ start = SlopeCurve.GetEndPoint(0);
                XYZ end = SlopeCurve.GetEndPoint(1);

                XYZ slope = start.Z > end.Z ? (end - start) : (start - end);

                SlopeDirect = new XYZ(slope.X, slope.Y, 0.0);
            }
            return SlopeDirect;
        }
    }
}

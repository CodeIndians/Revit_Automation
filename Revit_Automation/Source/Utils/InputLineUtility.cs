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
        public static void GatherInputLines(Document doc, bool bSelected, Selection selection)
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

                Parameter FlangeOffsetParam = locCurve.LookupParameter("Stud O.C.");
                if (FlangeOffsetParam != null)
                {
                    iLine.dFlangeOfset = FlangeOffsetParam.AsDouble();
                }

                Parameter StudOnCenterParam = locCurve.LookupParameter("Stud O.C.");
                if (StudOnCenterParam != null)
                {
                    iLine.dOnCenter = StudOnCenterParam.AsDouble();
                }

                // Compute Intersection Points with Grids. 
                GridCollector GridCollectionHelper = new GridCollector(doc);

                var locationCurve = (LocationCurve)locCurve.Location;
                var linecoords = Tuple.Create(locationCurve.Curve.GetEndPoint(0), locationCurve.Curve.GetEndPoint(1));
                
                // Compute Grid Intersections for T62 Placement
                iLine.gridIntersectionPoints = GridCollectionHelper.computeIntersectionPoints(linecoords);

                // Compute Main intesection points for Stud placement offset
                iLine.mainGridIntersectionPoints = GridCollectionHelper.computeIntersectionPoints(linecoords, true);
                
                //Add the line to the collection 
                AddInputLine(iLine);
            }
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
    }
}

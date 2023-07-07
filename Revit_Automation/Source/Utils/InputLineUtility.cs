/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Forms;

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

        public static HashSet<string> wallTypes = new HashSet<string>();

        public static Document m_Document;
        public static void GatherWallTypesFromInputLines(Document doc)
        {
            wallTypes?.Clear();

            FilteredElementCollector locationCurvedCol = null;

            locationCurvedCol
                  = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfCategory(BuiltInCategory.OST_GenericModel);

            foreach (Element locCurve in locationCurvedCol)
            {
                Parameter WallTypeParam = locCurve.LookupParameter("Panel Type");
                if (WallTypeParam != null)
                {
                   wallTypes.Add(WallTypeParam.AsString()); 
                }
            }
        }
        /// <summary>
        /// This function is used to collect all input lines in the model
        /// </summary>
        /// <param name="doc"> Pointer to the Active document</param>
        /// 
        public static void GatherInputLines(Document doc, bool bSelected, Selection selection, CommandCode commandcode, bool bComputeRoofSlope = true)
        {
            colInputLines?.Clear();

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
                if (selection == null || selection.GetElementIds().Count == 0)
                { 
                    TaskDialog.Show("Automation Error", "Please Select Atleast 1 Input line to Proceed");
                    return;
                }

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
                    _ = locationCurvedCol.WherePasses(modelCategoryFilter);


                }
            }

            foreach (Element locCurve in locationCurvedCol)
            {
                if (locCurve.IsHidden(doc.ActiveView))
                {
                    continue;
                }

                InputLine iLine = new InputLine();

                if (locCurve.Location is LocationCurve location)
                {
                    iLine.locationCurve = location;
                }
                else
                {
                    continue;
                }  

                iLine.startpoint = iLine.locationCurve.Curve.GetEndPoint(0);
                iLine.endpoint = iLine.locationCurve.Curve.GetEndPoint(1);

                iLine.id = locCurve.Id;
                
                Parameter tempParam;

                // Whether give line is extended or trimmed;
                iLine.bLineExtendedOrTrimmed = false;

                // Additional Panel
                tempParam = locCurve.LookupParameter("Additional Panel");
                if (tempParam != null)
                {
                    iLine.strAdditionalPanel = tempParam.AsString();
                }
                tempParam = null;

                // Additional Panel Gauge
                tempParam = locCurve.LookupParameter("Additional Panel Gauge");
                if (tempParam != null)
                {
                    iLine.strAdditionalPanelGuage = tempParam.AsString();
                }
                tempParam = null;

                // Beam Size
                tempParam = locCurve.LookupParameter("Beam Size");
                if (tempParam != null)
                {
                    iLine.strBeamSize = tempParam.AsString();
                }
                tempParam = null;

                // Bracing
                tempParam = locCurve.LookupParameter("Bracing");
                if (tempParam != null)
                {
                    iLine.strBracing = tempParam.AsString();
                }
                tempParam = null;

                // Cee Header Gauge
                tempParam = locCurve.LookupParameter("Cee Header Gauge");
                if (tempParam != null)
                {
                    iLine.strCHeaderGuage = tempParam.AsString();
                }
                tempParam = null;

                // Cee Header Quantity
                tempParam = locCurve.LookupParameter("Cee Header Quantity");
                if (tempParam != null)
                {
                    iLine.strCHeaderQuantity = tempParam.AsString();
                }
                tempParam = null;

                // Cee Header Size
                tempParam = locCurve.LookupParameter("Cee Header Size");
                if (tempParam != null)
                {
                    iLine.strCHeaderSize = tempParam.AsString();
                }
                tempParam = null;

                // Color
                tempParam = locCurve.LookupParameter("Color");
                if (tempParam != null)
                {
                    iLine.strColor = tempParam.AsString();
                }
                tempParam = null;

                // HSS Type
                tempParam = locCurve.LookupParameter("HSS Type");
                if (tempParam != null)
                {
                    iLine.strHSSType = tempParam.AsString();
                }
                tempParam = null;

                // Material
                tempParam = locCurve.LookupParameter("Material");
                if (tempParam != null)
                {
                    iLine.strMaterial = tempParam.AsString();
                }
                tempParam = null;

                // Panel Type
                tempParam = locCurve.LookupParameter("Panel Type");
                if (tempParam != null)
                {
                    iLine.strPanelType = tempParam.AsString();
                }
                tempParam = null;

                // Partition Panel Gauge
                tempParam = locCurve.LookupParameter("Partition Panel Gauge");
                if (tempParam != null)
                {
                    iLine.strPartitionPanelGuage = tempParam.AsString();
                }
                tempParam = null;

                // Roof System
                tempParam = locCurve.LookupParameter("Roof System");
                if (tempParam != null)
                {
                    iLine.strRoofSystem = tempParam.AsString();
                }
                tempParam = null;

                // Row Name
                tempParam = locCurve.LookupParameter("Row Name");
                if (tempParam != null)
                {
                    iLine.strRowName = tempParam.AsString();
                }
                tempParam = null;

                // Color (Door Header)
                tempParam = locCurve.LookupParameter("Color (Door Header)");
                if (tempParam != null)
                {
                    iLine.strColorDoorHeader = tempParam.AsString();
                }
                tempParam = null;

                // HSS Height
                tempParam = locCurve.LookupParameter("HSS Height");
                if (tempParam != null)
                {
                    iLine.dHSSHeight = tempParam.AsDouble();
                }
                tempParam = null;

                // Material Height
                tempParam = locCurve.LookupParameter("Material Height");
                if (tempParam != null)
                {
                    iLine.dMaterialHeight = tempParam.AsDouble();
                }
                tempParam = null;

                // Material Thickness
                tempParam = locCurve.LookupParameter("Material Thickness");
                if (tempParam != null)
                {
                    iLine.dMaterialThickness = tempParam.AsDouble();
                }
                tempParam = null;

                // Panel Offset Height
                tempParam = locCurve.LookupParameter("Panel Offset Height");
                if (tempParam != null)
                {
                    iLine.dPanelOffsetHeight = tempParam.AsDouble();
                }
                tempParam = null;

                // Panel Offset Height
                tempParam = locCurve.LookupParameter("Partition Panel Each Side (Y/N)");
                if (tempParam != null)
                {
                    iLine.dPartitionPanelEachSide = tempParam.AsInteger();
                }
                tempParam = null;

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

                Parameter phaseCreated = locCurve.get_Parameter(BuiltInParameter.PHASE_CREATED);
                if (phaseCreated != null)
                {
                    iLine.strBuildingName = phaseCreated.AsValueString();
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

                Parameter BottomTrackPunchParam = locCurve.LookupParameter("Bottom Track Punch");
                if (BottomTrackPunchParam != null)
                {
                    iLine.strBottomTrackPunch = BottomTrackPunchParam.AsString();
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

                Parameter ParapetHeightParam = locCurve.LookupParameter("Parapet Height");
                if (ParapetHeightParam != null)
                {
                    iLine.dParapetHeight = ParapetHeightParam.AsDouble();
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

                // For Trim and extend we do not need grid intersection points and Direction wrt roof slope
                if (bComputeRoofSlope)
                {
                    // Compute Intersection Points with Grids. 
                    GridCollector GridCollectionHelper = new GridCollector(doc);

                    LocationCurve locationCurve = (LocationCurve)locCurve.Location;
                    Tuple<XYZ, XYZ> linecoords = Tuple.Create(locationCurve.Curve.GetEndPoint(0), locationCurve.Curve.GetEndPoint(1));

                    // Compute if the Line is parallel, or perpendicular to roof slope.
                    XYZ lineDirection = locationCurve.Curve.GetEndPoint(1) - locationCurve.Curve.GetEndPoint(0);
                    XYZ roofSlope = GenericUtils.GetRoofSlopeDirection(locationCurve.Curve.GetEndPoint(1));
                    iLine.dirWRTRoofSlope = MathUtils.IsParallel(roofSlope, lineDirection)
                        ? DirectionWithRespectToRoofSlope.Parallel
                        : DirectionWithRespectToRoofSlope.Perpendicular;

                    // Compute Grid Intersections for T62 Placement
                    iLine.gridIntersectionPoints = GridCollectionHelper.computeIntersectionPoints(linecoords);

                    // Compute Main intesection points for Stud placement offset
                    iLine.mainGridIntersectionPoints = GridCollectionHelper.computeIntersectionPoints(linecoords, true);
                }

                //Add the line to the collection 
                _ = AddInputLine(iLine);
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

        public static string GetProjectSettings()
        {
            Element prjSettingsLine = GetProjectSettingsLine();
            string strProjectSettings = string.Empty;

            if (prjSettingsLine != null)
            {
                Parameter ProjectSettingParam = prjSettingsLine.LookupParameter("Project Settings");
                if (ProjectSettingParam != null)
                {
                    strProjectSettings = ProjectSettingParam.AsString();
                }
            }

            return strProjectSettings;
        }

        public static void SetProjectSettings(string strProjectSettings)
        {
            Element prjSettingsLine = GetProjectSettingsLine();

            if (prjSettingsLine != null)
            {
                ParameterSet parameters = prjSettingsLine.Parameters;

                // Find the parameter by its name
                Parameter parameter = parameters.OfType<Parameter>()
                                                .FirstOrDefault(p => p.Definition.Name == "Project Settings");

                if (parameter != null)
                {
                    // Set the value of the parameter based on its data type
                    if (parameter.Definition.ParameterType == ParameterType.Text)
                    {
                        using (Transaction tx = new Transaction(m_Document))
                        {
                            tx.Start("Setting Project Parameters");
                            parameter.Set(strProjectSettings);
                            tx.Commit();    
                        }
                    }
                }
            }
        }

        private static Element GetProjectSettingsLine()
        {
            FilteredElementCollector locationCurvedCol = new FilteredElementCollector(m_Document)
                                                            .WhereElementIsNotElementType()
                                                            .OfCategory(BuiltInCategory.OST_GenericModel); ;

            Element projectSettingsLine = null;
            foreach (Element elem in locationCurvedCol)
            {
                if (elem.Name == "Project Settings Line")
                {
                    projectSettingsLine = elem;
                    break;
                }
            }

            if (projectSettingsLine == null)
            {
                projectSettingsLine = CreateProjectSettingLine();
            }

            return projectSettingsLine;
        }



        public static Element CreateProjectSettingLine()
        {
            Element prjSettingsLine = null;

            using (Transaction tx = new Transaction(m_Document))
            {
                tx.Start("Creating Project Specifications Line");

                FamilySymbol prjSettingsLineSym = SymbolCollector.GetProjectSpecificationLineSymbol();

                if (prjSettingsLineSym != null && !prjSettingsLineSym.IsActive)
                prjSettingsLineSym.Activate();

                FilteredElementCollector levels = new FilteredElementCollector(m_Document);
                levels.WherePasses(new ElementClassFilter(typeof(Level), false))
                        .Cast<Level>()
                        .OrderBy(e => e.Elevation);

                XYZ position = new XYZ(0, 0, 0);
                prjSettingsLine = m_Document.Create.NewFamilyInstance(position, prjSettingsLineSym, levels.ElementAt(0), StructuralType.NonStructural);
                tx.Commit();
            }

            return prjSettingsLine;
        }
    }
}

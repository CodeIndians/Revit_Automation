
/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Revit_Automation.Source;
using Revit_Automation.Source.ModelCreators;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace Revit_Automation
{
    /// <summary>
    /// This is the core class for creating the required elements in the model
    /// </summary>
    public class ModelCreator
    {
        public static IOrderedEnumerable<Level> levels = null;
        /// <summary>
        /// Creates the model based on the input lines provided in the document
        /// </summary>
        /// <param name="uiapp"> Application pointer</param>
        /// <param name="form">Address of the overlaying dialog when model creation is in progress </param>
        public static void CreateModel(UIApplication uiapp, Form1 form, bool bSelected = false, CommandCode commandCode = CommandCode.All)
        {
            form.Show();
            form.UpdateStarted();
            form.Refresh();

            Thread.Sleep(2000);

            UIDocument uidoc = uiapp.ActiveUIDocument;
            _ = uiapp.Application;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            
            View activeView = doc.ActiveView;

            string strActivePhase = null;
            Parameter phaseCreated = activeView.get_Parameter(BuiltInParameter.VIEW_PHASE);
            if (phaseCreated != null)
            {
                strActivePhase = phaseCreated.AsValueString();
            }

            FilteredElementCollector phaseCollector = new FilteredElementCollector(doc);
            phaseCollector.OfClass(typeof(Phase));
            Phase desiredPhase = phaseCollector.Cast<Phase>().FirstOrDefault(phase => phase.Name == strActivePhase);

            // Clear out all the static vectors
            ClearStatics();

            //Create The logger Object
            Logger.CreateLogFile();

            
            try
            {
                // 1. Identify the roof slopes
                RoofUtility.computeRoofSlopes(doc);
            }
            catch (Exception)
            {
                _ = TaskDialog.Show("Automation Error", "Failed while processing the roofs");
            }

            // 2. Identify the main Grids in the model
            GridCollector gridCollection = new GridCollector(doc);

            // 3. Validate if the grids are equidistant
            if (gridCollection.Validate())
            {
                _ = TaskDialog.Show("Automation Error", "Grid Validation Failed");
                return;
            }

            try
            {
                // 4. Find the levels in the project
                levels = FindAndSortLevels(doc);
            }
            catch (Exception)
            {
                _ = TaskDialog.Show("Automation Error", "Failed while processing the Levels");
            }

            try
            {
                // 5. Find the levels in the project
                IOrderedEnumerable<Floor> floors = FindAndSortFloors(doc);
            }
            catch (Exception)
            {
                _ = TaskDialog.Show("Automation Error", "Failed while processing the Floors");
            }

            try
            {
                // 6. Collect the necessary symbols
                SymbolCollector.CollectColumnSymbols(doc);
                SymbolCollector.CollectWallSymbols(doc);
            }
            catch (Exception)
            {
                _ = TaskDialog.Show("Automation Error", "Failed while processing the Symbols");
            }

            try
            {
                // 7. Input Lines to be collected
                InputLineUtility.GatherInputLines(doc, selection, commandCode);

                if (InputLineUtility.colInputLines.Count == 0)
                    return;
            }
            catch (Exception)
            {
                _ = TaskDialog.Show("Automation Error", "Failed while processing the Input lines");
            }
            
            // 8. Input Lines to be collected
            try
            {
                FloorHelper.GatherFloors(doc);
            }
            catch (Exception)
            {
                _ = TaskDialog.Show("Automation Error", "Failed while processing the Levels");
            }

            // 9. Place Columns
            if (commandCode == CommandCode.Posts )
            {
                ColumnCreator columnCreator = new ColumnCreator(doc, form);
                columnCreator.SetPhase(desiredPhase);
                columnCreator.CreateModel(InputLineUtility.colInputLines, levels);
            }
            else if (commandCode == CommandCode.Walls)
            {
                WallCreator wallCreator = new WallCreator(doc, form);
                wallCreator.CreateModel(InputLineUtility.colInputLines, levels);
            }
            else if (commandCode == CommandCode.BottomTracks)
            {
                BottomTrackCreator bottomTrackCreator = new BottomTrackCreator(doc, form);
                bottomTrackCreator.CreateModel(InputLineUtility.colInputLines, levels);
            }
            //uidoc.ActiveView = activeView;

            form.Visible = false;
            form.UpdateCompleted();
            _ = form.ShowDialog();
        }


        public static IOrderedEnumerable<Level> FindAndSortLevels(Document doc)
        {
            return new FilteredElementCollector(doc)
                            .WherePasses(new ElementClassFilter(typeof(Level), false))
                            .Cast<Level>()
                            .OrderBy(e => e.Elevation);

        }

        public static IOrderedEnumerable<Floor> FindAndSortFloors(Document doc)
        {
            return new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Floors)
                            .Cast<Floor>()
                            .OrderBy(e => e.LevelId);
        }

        internal static void ClearStatics()
        {
            RoofUtility.colRoofs?.Clear();
            GridCollector.mVerticalMainLines?.Clear();
            GridCollector.mHorizontalMainLines?.Clear();
            GridCollector.mHorizontalMainLines?.Clear();
            GridCollector.mVerticalMainLines?.Clear();
            InputLineUtility.colInputLines?.Clear();
            FloorHelper.colFloors?.Clear();
        }
    }
}


/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Revit_Automation.Command;
using System.Windows.Forms;
using Revit_Automation.Source;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.ModelCreators;
using System.Collections.ObjectModel;
using Revit_Automation.Source.Utils;
using Autodesk.Revit.UI.Selection;

namespace Revit_Automation
{
    /// <summary>
    /// This is the core class for creating the required elements in the model
    /// </summary>
    public class ModelCreator
    {
        /// <summary>
        /// Creates the model based on the input lines provided in the document
        /// </summary>
        /// <param name="uiapp"> Application pointer</param>
        /// <param name="form">Address of the overlaying dialog when model creation is in progress </param>
        static public void CreateModel(UIApplication uiapp, Form1 form, bool bSelected = false)
        {

            form.Show();
            form.Refresh();

            Thread.Sleep(2000);

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;

            // Clear out all the static vectors
            ClearStatics();

            // 1. Identify the roof slopes
            RoofUtility.computeRoofSlopes(doc);

            // 2. Identify the main Grids in the model
            GridCollector gridCollection = new GridCollector(doc);

            // 3. Validate if the grids are equidistant
            if(gridCollection.Validate())
            {
                MessageBox.Show("Grid Validation Failed");
                return;
            }

            // 4. Find the levels in the project
            IOrderedEnumerable<Level> levels = FindAndSortLevels(doc);

            // 5. Find the levels in the project
            IOrderedEnumerable<Floor> floors = FindAndSortFloors(doc);

            // 6. Collect the necessary symbols
            SymbolCollector.CollectColumnSymbols(doc);

            // 7. Input Lines to be collected
            InputLineUtility.GatherInputLines(doc, bSelected, selection);

            // 8. Input Lines to be collected
            FloorHelper.GatherFloors(doc);

            // 9. Place Columns
            ColumnCreator columnCreator = new ColumnCreator(doc, form);
            columnCreator.CreateModel(InputLineUtility.colInputLines, levels);


            form.Visible = false;
            form.UpdateCompleted();
            form.ShowDialog();
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
                .OfCategory(BuiltInCategory.OST_GenericModel)
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

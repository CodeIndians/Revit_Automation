/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Revit_Automation.Dialogs;
using Revit_Automation.Source;
using Revit_Automation.Source.Preprocessors;
using Revit_Automation.Source.Utils;
using System.Diagnostics;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

#endregion

namespace Revit_Automation
{
    public class PrepareCommandClass
    {
        public static void PrepareCommand(ExternalCommandData commandData)
        {
            UIApplication uiapp = commandData.Application;

            // Get the active Document
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Set the document object on the utilities
            FloorHelper.m_Document = doc;
            InputLineUtility.m_Document = doc;
            SymbolCollector.m_Document = doc;
            RoofUtility.m_Document = doc;

            if (!GlobalSettings.PopulateGlobalSettings())
            {
                TaskDialog.Show("Command", "Project Settings are not Present");
            }
        }
    }
    public enum LineType
    {
        Horizontal = 0,
        vertical,
        Inclined
    }
    public enum CommandCode
    {
        All = 0,
        Posts = 1,
        Walls = 2,
        BottomTracks = 3,
        Beams,
        CeeHeaders, 
        TopTracks,
        Purlins, 
        DragStrut,
        EaveStrut,
        CompositeDeck,
        RoofDeck,
        TrimDeck
    }


    [Transaction(TransactionMode.Manual)]
    public class PostsAtAllLines : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            PrepareCommandClass.PrepareCommand(commandData);

            Form1 form = new Form1
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            //form.TopMost= true;
            _ = form.ShowDialog();

            if (form.CanCreateModel)
            {
                ModelCreator.CreateModel(uiapp, form, false, CommandCode.Posts);
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class PanelsAtAllLines : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            PrepareCommandClass.PrepareCommand(commandData);

            Form1 form = new Form1
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            //form.TopMost= true;
            _ = form.ShowDialog();

            if (form.CanCreateModel)
            {
                ModelCreator.CreateModel(uiapp, form, false, CommandCode.Walls);
            }

            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    public class BTAtAllLines : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            PrepareCommandClass.PrepareCommand(commandData);

            Form1 form = new Form1
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            //form.TopMost= true;
            _ = form.ShowDialog();

            if (form.CanCreateModel)
            {
                ModelCreator.CreateModel(uiapp, form, false, CommandCode.BottomTracks);
            }

            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    public class TopTracksCreator : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            PrepareCommandClass.PrepareCommand(commandData);

            Form1 form = new Form1
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            //form.TopMost= true;
            _ = form.ShowDialog();

            if (form.CanCreateModel)
            {
                ModelCreator.CreateModel(uiapp, form, false, CommandCode.TopTracks);
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class ProjectSettings : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            // Walls will be needed for the Properties Dialog
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Set the document object on the utilities
            FloorHelper.m_Document = doc;
            InputLineUtility.m_Document = doc;
            SymbolCollector.m_Document = doc;
            RoofUtility.m_Document = doc;

            // For Purlin Drag and Eave struts we need roof specific settings
            try
            {
                // 1. Identify the roof slopes
                RoofUtility.computeRoofSlopes(doc);
            }
            catch (Exception)
            {
                _ = TaskDialog.Show("Automation Error", "Failed while processing the roofs");
            }

            // Check if a Project Settings line is present or not
            if (SymbolCollector.GetProjectSpecificationLineSymbol() == null)
            {
                TaskDialog.Show("Automation Error", "Project Settings line Family is not Present");
                return Result.Succeeded;
            }
            
            SymbolCollector.CollectWallSymbols(doc);
            InputLineUtility.GatherWallTypesFromInputLines(doc);
            LevelCollector.FindAndSortLevels(doc);

            ProjectPropertiesDG form = new ProjectPropertiesDG()
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            
            _ = form.ShowDialog();

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class ExtendLines : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;

            PrepareCommandClass.PrepareCommand(commandData);

            LineProcessing form = new LineProcessing()
            {
                StartPosition = FormStartPosition.CenterScreen
            };

            LineExtender lineExtender = new LineExtender(doc, form, false, selection);
            lineExtender.Preprocess();

            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    public class TrimLines : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;

            PrepareCommandClass.PrepareCommand(commandData);

            LineProcessing form = new LineProcessing()
            {
                StartPosition = FormStartPosition.CenterScreen
            };

            LineTrimmer lineTrimmer = new LineTrimmer(doc, form, false, selection);
            lineTrimmer.Preprocess();

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class CreateHatchForHallway : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class DrawHallWayLines : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class EditHatchForHallway : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class PostProperties : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;


            Element post = doc.GetElement(selection.GetElementIds().First());

            Parameter rightHeightParam = post.LookupParameter("Height");
            if (rightHeightParam != null)
            {
                double dRightHeight = rightHeightParam.AsDouble();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class PurlinsCreator : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            PrepareCommandClass.PrepareCommand(commandData);

            Form1 form = new Form1
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            //form.TopMost= true;
            _ = form.ShowDialog();

            if (form.CanCreateModel)
            {
                ModelCreator.CreateModel(uiapp, form, false, CommandCode.Purlins);
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class DragStrutCreator : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            PrepareCommandClass.PrepareCommand(commandData);

            Form1 form = new Form1
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            //form.TopMost= true;
            _ = form.ShowDialog();

            if (form.CanCreateModel)
            {
                ModelCreator.CreateModel(uiapp, form, false, CommandCode.DragStrut);
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class EaveStrutCreator : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            PrepareCommandClass.PrepareCommand(commandData);

            Form1 form = new Form1
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            //form.TopMost= true;
            _ = form.ShowDialog();

            if (form.CanCreateModel)
            {
                ModelCreator.CreateModel(uiapp, form, false, CommandCode.EaveStrut);
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class CompositeDeckCreator : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;

            PrepareCommandClass.PrepareCommand(commandData);

            Reference north = selection.PickObject(
                ObjectType.Element, "Pick Grid for North Boundary ");

            Grid Northgrid = doc.GetElement(north) as Grid;

            Reference south = selection.PickObject(
                ObjectType.Element, "Pick Grid for South Boundary ");

            Grid Southgrid = doc.GetElement(south) as Grid;

            Reference east = selection.PickObject(
                ObjectType.Element, "Pick Grid for East Boundary ");

            Grid Eastgrid = doc.GetElement(east) as Grid;

            Reference west = selection.PickObject(
                ObjectType.Element, "Pick Grid for West Boundary ");

            Grid Westgrid = doc.GetElement(west) as Grid;

            Reference elemRef = selection.PickObject(
                ObjectType.Element, "Pick Grid Span Starting  reference ");

            Grid spanStartGrid = doc.GetElement(elemRef) as Grid;

            List<Grid> grids = new List<Grid>();
            grids.Add(Northgrid);
            grids.Add(Southgrid);
            grids.Add(Eastgrid);
            grids.Add(Westgrid);
            grids.Add(spanStartGrid);

            CCompositeDeckCreator.lstBoundaries = grids;

            Form1 form = new Form1
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            //form.TopMost= true;
            _ = form.ShowDialog();

            if (form.CanCreateModel)
            {
                ModelCreator.CreateModel(uiapp, form, false, CommandCode.CompositeDeck);
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class RoofDeckCreator : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            PrepareCommandClass.PrepareCommand(commandData);

            Form1 form = new Form1
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            //form.TopMost= true;
            _ = form.ShowDialog();

            if (form.CanCreateModel)
            {
                ModelCreator.CreateModel(uiapp, form, false, CommandCode.RoofDeck);
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class TrimDecks : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            PrepareCommandClass.PrepareCommand(commandData);

            Form1 form = new Form1
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            //form.TopMost= true;
            _ = form.ShowDialog();

            if (form.CanCreateModel)
            {
                ModelCreator.CreateModel(uiapp, form, false, CommandCode.TrimDeck);
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class GetRangeCommand : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            
            ICollection<ElementId> selectedIDs = selection.GetElementIds();

            foreach (var sel in selectedIDs)
            {
                Element elem = doc.GetElement(sel);
                if (elem != null)
                {
                    BoundingBoxXYZ boundingBoxXYZ = elem.get_BoundingBox(doc.ActiveView);
                    if (boundingBoxXYZ != null) 
                    {
                        string Msg = $"ID : {elem.Id} Range : {boundingBoxXYZ.Min}, {boundingBoxXYZ.Max}";
                        MessageBox.Show(Msg);
                    }
                }
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class ExteriorPanelDirectionComputation : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            InputLineUtility.GatherInputLines(doc, selection, CommandCode.Walls, false, true);

            SymbolCollector.CollectColumnSymbols(doc);
            FloorHelper.GatherFloors(doc);

            PanelUtils panelUtils = new PanelUtils(doc);
            panelUtils.ComputePanelDirectionForExteriorPanels();

           
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class CeeHeaders : IExternalCommand
    {

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;

            CeeHeaderModeSelector ceeHeaderModeSelector = new CeeHeaderModeSelector()
            { StartPosition = FormStartPosition.CenterScreen };
            ceeHeaderModeSelector.ShowDialog();

            if (ceeHeaderModeSelector.m_bCreation)
            {
                CeeHeaderBoundaries.selectedInputlines = selection.GetElementIds().ToList();
                CeeHeaderBoundaries.bSelectedModelling = selection.GetElementIds().ToList().Count > 0;

                PrepareCommandClass.PrepareCommand(commandData);

                Reference north = selection.PickObject(
                    ObjectType.Element, "Pick Grid for North Boundary ");

                Grid Northgrid = doc.GetElement(north) as Grid;

                Reference south = selection.PickObject(
                    ObjectType.Element, "Pick Grid for South Boundary ");

                Grid Southgrid = doc.GetElement(south) as Grid;

                Reference east = selection.PickObject(
                    ObjectType.Element, "Pick Grid for East Boundary ");

                Grid Eastgrid = doc.GetElement(east) as Grid;

                Reference west = selection.PickObject(
                    ObjectType.Element, "Pick Grid for West Boundary ");

                Grid Westgrid = doc.GetElement(west) as Grid;

                Reference elemRef = selection.PickObject(
                    ObjectType.Element, "Pick Grid Span Starting  reference ");

                Grid spanStartGrid = doc.GetElement(elemRef) as Grid;


                CeeHeaderBoundaries.SetBoundaries(Northgrid, Southgrid, Eastgrid, Westgrid);
                CeeHeaderBoundaries.SetSpanStartingGrid(spanStartGrid);

                Form1 form = new Form1
                {
                    StartPosition = FormStartPosition.CenterScreen
                };
                //form.TopMost= true;
                _ = form.ShowDialog();

                if (form.CanCreateModel)
                {
                    ModelCreator.CreateModel(uiapp, form, false, CommandCode.CeeHeaders);
                }
            }
            else
            {
                CeeHeaderAdjustmentsForm ceeHeaderAdjustmentsForm = new CeeHeaderAdjustmentsForm(doc)
                { StartPosition = FormStartPosition.CenterScreen};
                InitializeUtilClasses(uiapp);
                ceeHeaderAdjustmentsForm.PopulateData();
                ceeHeaderAdjustmentsForm.ShowDialog();
            }

            ceeHeaderModeSelector.Close();

            return Result.Succeeded;
        }

        private void InitializeUtilClasses(UIApplication uiapp)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            _ = uiapp.Application;
            Document doc = uidoc.Document;

            // Clear out all the static vectors
            ClearStatics();

            try
            {
                // 1. Identify the roof slopes
                RoofUtility.computeRoofSlopes(doc);

                // Collect Necessary Symbols
                SymbolCollector.CollectColumnSymbols(doc);
                SymbolCollector.CollectWallSymbols(doc);

                // Identify the floors
                FloorHelper.GatherFloors(doc);

            }
            catch (Exception)
            {
                _ = TaskDialog.Show("Automation Error", "Failed while processing the roofs");
            }
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

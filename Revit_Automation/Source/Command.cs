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
using Revit_Automation.Source.Hallway;
using Revit_Automation.Source.Preprocessors;
using Revit_Automation.Source.Utils;
using System.Windows.Forms;

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
        vertical
    }
    public enum CommandCode
    {
        All = 0,
        Posts = 1,
        Walls = 2,
        BottomTracks = 3,
        Beams
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
    public class PostsAtSelectedLines : IExternalCommand
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
                ModelCreator.CreateModel(uiapp, form, true, CommandCode.Posts);
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
    public class PanelsAtSelectedLines : IExternalCommand
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
                ModelCreator.CreateModel(uiapp, form, true, CommandCode.Walls);
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
    public class BTAtSelectedLines : IExternalCommand
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
                ModelCreator.CreateModel(uiapp, form, true, CommandCode.BottomTracks);
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

            // Check if a Project Settings line is present or not
            if (SymbolCollector.GetProjectSpecificationLineSymbol() == null)
            {
                TaskDialog.Show("Automation Error", "Project Settings line Family is not Present");
                return Result.Succeeded;
            }
            
            SymbolCollector.CollectWallSymbols(doc);
            InputLineUtility.GatherWallTypesFromInputLines(doc);

            ProjectProperties form = new ProjectProperties()
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            
            _ = form.ShowDialog();

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class PreProcessAllLines : IExternalCommand
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

            LineExtender lineExtender = new LineExtender(doc, form, false);
            lineExtender.Preprocess();

            LineTrimmer lineTrimmer = new LineTrimmer(doc, form, false);
            lineTrimmer.Preprocess();

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class PreProcessSelectedLines : IExternalCommand
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

            LineExtender lineExtender = new LineExtender(doc, form, true, selection);
            lineExtender.Preprocess();

            LineTrimmer lineTrimmer = new LineTrimmer(doc, form, true, selection);
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
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var lineCollector = new LineCollector(ref doc);

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
}

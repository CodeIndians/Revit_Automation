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
using System.Windows.Forms;

#endregion

namespace Revit_Automation
{
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
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
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
                return Result.Succeeded;
            }

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
    public class Command2 : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
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
                return Result.Succeeded;
            }

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
    public class Command3 : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
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
                return Result.Succeeded;
            }

            //TaskDialog.Show("Walls Command", "Place Walls Command is not implemented");
            //return Result.Succeeded; ;

            //UIApplication uiapp = commandData.Application;

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
    public class Command4 : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
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
                return Result.Succeeded;
            }

            TaskDialog.Show("Bottom Track Command", "Place Bottom Tracks Command is not implemented");
            return Result.Succeeded; ;

            //UIApplication uiapp = commandData.Application;

            //Form1 form = new Form1
            //{
            //    StartPosition = FormStartPosition.CenterScreen
            //};
            ////form.TopMost= true;
            //_ = form.ShowDialog();

            //if (form.CanCreateModel)
            //{
            //    ModelCreator.CreateModel(uiapp, form, true, CommandCode.BottomTracks);
            //}

            //return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class Command5 : IExternalCommand
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
    public class Command7 : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
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
                return Result.Succeeded;
            }

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
    public class Command8 : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            // Get the active Document
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            
            // Set the document object on the utilities
            FloorHelper.m_Document = doc;
            InputLineUtility.m_Document = doc;
            SymbolCollector.m_Document = doc;
            RoofUtility.m_Document = doc;

            if (!GlobalSettings.PopulateGlobalSettings())
            {
                TaskDialog.Show("Command", "Project Settings are not Present");
                return Result.Succeeded;
            }

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
}

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
using Revit_Automation.Dialogs;
using Revit_Automation.Source;
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

            Form1 form = new Form1
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            //form.TopMost= true;
            _ = form.ShowDialog();

            if (form.CanCreateModel)
            {
                ModelCreator.CreateModel(uiapp, form, false, CommandCode.All);
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

            TaskDialog.Show("Walls Command", "Place Walls Command is not implemented");
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
            //    ModelCreator.CreateModel(uiapp, form, true, CommandCode.Walls);
            //}

            //return Result.Succeeded;
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
    public class Command6 : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            //TaskDialog.Show("Sheet Creation Command", "Sheet Created Command is not implemented");
            //return Result.Succeeded; ;

            UIApplication uiapp = commandData.Application;

            // Walls will be needed for the Properties Dialog
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            SheetUtils.m_Document = doc;    

            SheetingConfiguration form = new SheetingConfiguration()
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            

            _ = form.ShowDialog();

            return Result.Succeeded;
        }
    }
}

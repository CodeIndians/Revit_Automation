

using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Windows.Forms;
using Shop_Automation.Dialogs;
using Shop_Automation.Utils;
using Shop_Automation.Source;


namespace Shop_Automation
{
    [Transaction(TransactionMode.Manual)]
    public class AssemblySettings : IExternalCommand
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
            AssemblyGenerator.doc = doc;
            AseemblySettings.lstViewTemplates = GenericUtils.GetElevationViewType(doc);
            AseemblySettings.lstScheduleTemplates = GenericUtils.GetScheduleType(doc);
            AseemblySettings.lstTitleBlocks = GenericUtils.GetTitleBlocks(doc);
            AseemblySettings aseemblySettings = new AseemblySettings();
            aseemblySettings.ShowDialog();

            AssemblyGenerator.GenerateAssemblies();
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class GenerateAssemblies : IExternalCommand
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

            return Result.Succeeded;
        }
    }
}



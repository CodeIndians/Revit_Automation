using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Sheeting_Automation.Dialogs;
using Sheeting_Automation.Source.Dimensions;
using Sheeting_Automation.Utils;
using System.Windows.Forms;

namespace Sheeting_Automation.Source.Tags
{
    [Transaction(TransactionMode.Manual)]
    public class CreateTagsCommand : IExternalCommand
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

            SheetUtils.m_Document = doc;

            TaskDialog.Show("Info", "create tags command");

            return Result.Succeeded;
        }
    }
    
    
    [Transaction(TransactionMode.Manual)]
    public class CheckTagsCommand : IExternalCommand
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

            SheetUtils.m_Document = doc;

            TaskDialog.Show("Info", "check tags command");

            return Result.Succeeded;
        }
    }
}

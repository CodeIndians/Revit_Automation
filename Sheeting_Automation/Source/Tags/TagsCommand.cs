using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Sheeting_Automation.Utils;

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

            // assign the document
            SheetUtils.m_Document = doc;

            // check if the current view is view plan 
            if(!TagUtils.IsCurrentViewPlan())
            {
                TaskDialog.Show("Error", "Current view is not a view plan");
                return Result.Failed;
            }

            // intialize the tag data 
            TagData.Initialize();


            var form = new TagCreationForm();

            form.ShowDialog();

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

            return Result.Succeeded;
        }
    }
}

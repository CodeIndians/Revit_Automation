using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit_Automation.Source.Utils
{
    public class ErrorHandler
    {
        public static ElementId elemIDbeingProcessed;
        public static void reportError()
        {
            TaskDialog taskDialog = new TaskDialog("Automation Error")
            {
                MainContent = string.Format("There is an error while processing the Element {0}. Please review", elemIDbeingProcessed)
            };

            elemIDbeingProcessed = null;

            _ = taskDialog.Show();
        }

    }
}

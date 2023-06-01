using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Utils
{
    public class ErrorHandler
    {
        public static ElementId elemIDbeingProcessed;
        public static void reportError()
        {
            TaskDialog taskDialog = new TaskDialog("Automation Error");
            taskDialog.MainContent = string.Format("There is an error while processing the Element {0}. Please review", elemIDbeingProcessed);
            
            elemIDbeingProcessed = null;

            taskDialog.Show();            
        }
    
    }
}

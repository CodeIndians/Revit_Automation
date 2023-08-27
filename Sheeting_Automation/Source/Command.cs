using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Sheeting_Automation.Dialogs;
using Sheeting_Automation.Source.Dimensions;
using Sheeting_Automation.Source.Schedules;
using Sheeting_Automation.Utils;
using System.Windows.Forms;

namespace Sheeting_Automation
{
    /// <summary>
    /// Command for placing dimensions
    /// This will open up a form for now 
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PlaceDimensionsCommand : IExternalCommand
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

            SheetingConfiguration form = new SheetingConfiguration()
            {
                StartPosition = FormStartPosition.CenterScreen
            };


            _ = form.ShowDialog();

            if (form.startCollectingData)
            {
                DimensionManager dm = new DimensionManager(ref doc);

                dm.PlaceDimensions();
                
             }
            return Result.Succeeded;
        }
    }
    /// <summary>
    /// Command to create schedules 
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class CreateSchedulesCommand : IExternalCommand
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

            // initialize schedule manager class
            var scheduleManager = new ScheduleManager(ref doc);

            //show the schedule creation form
            scheduleManager.ShowCreateForm();

            // TaskDialog.Show("Info", "Create Schedules");

            return Result.Succeeded;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class EditSchedulesCommand : IExternalCommand
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

            // initialize schedule manager class
            var scheduleManager = new ScheduleManager(ref doc);

            scheduleManager.UpdateMarkers();

            //TaskDialog.Show("Info", "Edit Schedules");

            return Result.Succeeded;
        }
    }
}

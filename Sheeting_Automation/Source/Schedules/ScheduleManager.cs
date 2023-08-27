using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Form = System.Windows.Forms.Form;

namespace Sheeting_Automation.Source.Schedules
{
    /// <summary>
    /// Manager class fro schedules
    /// All the schedule operations should happen through this instance
    /// </summary>
    internal class ScheduleManager
    {
        // DB document reference
        private Document mDoc;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="doc">DB document</param>
        public ScheduleManager(ref Document doc)
        {
            mDoc = doc;

            // initialize the static schedule data 
            Initialize();

        }

        /// <summary>
        /// Initialize the manager instance
        /// Update the schedule data
        /// </summary>
        private void Initialize()
        {
            //update all the schedule data
            ScheduleData.UpdateScheduleData(ref mDoc);

        }

        public void ShowCreateForm()
        {
            Form createForm = new ScheduleCreateForm();

            createForm.ShowDialog();
        }

        public void UpdateMarkers() 
        {
            var scheduleCreator = new ScheduleCreator();

            //scheduleCreator.FillMarkers();
        }
    }
}

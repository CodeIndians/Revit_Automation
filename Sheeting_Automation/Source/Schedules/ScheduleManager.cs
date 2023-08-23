using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Diagnostics;
using System.Linq;

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

            //REMOVE: remove this after moving this implementation to a new class
            //CreateViewScheduleWithTemplate("Walls");

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

        // MOVE: Move this to a separate class
        public void CreateViewScheduleWithTemplate(string categoryName)
        {
            Transaction trans1 = new Transaction(mDoc, "Set Schedule Properties");

            trans1.Start();
            // Create a new view schedule
            ViewSchedule viewSchedule = ViewSchedule.CreateSchedule(mDoc, ScheduleData.CategoryDictionary[categoryName]);

            // set the name of the view schedule
            viewSchedule.Name = "123";

            // get the selected phase id from the dictionary
            ElementId phaseid = ScheduleData.PhaseDictionary["Building 2"];

            //set the phase of the view schedule
            if (phaseid != null || phaseid != ElementId.InvalidElementId)
                viewSchedule.get_Parameter(BuiltInParameter.VIEW_PHASE).Set(phaseid);

            // get the selected viewtemplate id from the dictionary
            ElementId viewTemplateId = ScheduleData.ViewTemplateDictionary["Walls - Interior U-panels"];

            //set the view template of the view schedule
            if (viewTemplateId != null && viewTemplateId != ElementId.InvalidElementId)
                viewSchedule.ViewTemplateId = viewTemplateId;

            trans1.Commit();
        }

    }
}

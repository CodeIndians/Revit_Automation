using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Schedules
{
    public static class ScheduleUtils
    {
        /// <summary>
        /// Get the schedule id from the name
        /// </summary>
        /// <param name="fieldName">Returns schedule field if found other wise returns null</param>
        /// <returns></returns>
        public static ScheduleField GetScheduleFieldFromName(ViewSchedule viewSchedule,string fieldName)
        {

            ScheduleField scheduleField = null;

            IList<ScheduleFieldId> fieldOrder = viewSchedule.Definition.GetFieldOrder();


            foreach (var filedId in fieldOrder)
            {
                ScheduleField field = viewSchedule.Definition.GetField(filedId);

                if (field.GetName() == fieldName)
                {
                    scheduleField = field;
                    break;
                }
            }


            return scheduleField;
        }
    }
}

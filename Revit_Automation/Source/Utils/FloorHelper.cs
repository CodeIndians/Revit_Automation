using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Utils
{
    public  class FloorHelper
    {
        /// <summary>
        /// The collection of input lines
        /// </summary>
        public static List<FloorObject> colFloors = new List<FloorObject>();

        /// <summary>
        /// This function is used to collect all input lines in the model
        /// </summary>
        /// <param name="doc"> Pointer to the Active document</param>
        public static void GatherFloors(Document doc)
        {
            if (colFloors != null)
                colFloors.Clear();

            FilteredElementCollector floorsCollection
              = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Floors);

            foreach (Element floor in floorsCollection)
            {
                FloorObject floorObj = new FloorObject();

                floorObj.levelID = floor.LevelId;

                floorObj.elemID = floor.Id;
                // To DO - Differentiate floors based on building

                Parameter phaseCreated = floor.get_Parameter(BuiltInParameter.PHASE_CREATED);
                if (phaseCreated != null)
                {
                    floorObj.strBuildingName = phaseCreated.AsString();
                }

                //Add the line to the collection 
                AddFloor(floorObj);
            }
        }


        /// <summary>
        /// Adds Input line to the collection
        /// </summary>
        /// <param name="inputLine"> The Input Line to be added </param>
        /// <returns>True if the line is added to the collection </returns>
        public static bool AddFloor(FloorObject floor)
        {
            colFloors.Add(floor);
            return true;
        }
    }
}

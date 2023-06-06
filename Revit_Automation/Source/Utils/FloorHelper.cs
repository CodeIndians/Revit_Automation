using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System.Collections.Generic;

namespace Revit_Automation.Source.Utils
{
    public class FloorHelper
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
            colFloors?.Clear();

            FilteredElementCollector floorsCollection
              = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Floors);

            foreach (Element floor in floorsCollection)
            {
                FloorObject floorObj = new FloorObject
                {
                    levelID = floor.LevelId,

                    elemID = floor.Id
                };

                Parameter phaseCreated = floor.get_Parameter(BuiltInParameter.PHASE_CREATED);
                if (phaseCreated != null)
                {
                    floorObj.strBuildingName = phaseCreated.AsValueString();
                }

                GeometryElement geometry = floor.get_Geometry(new Options());
                bool bRangeComputed = false;
                foreach (GeometryObject obj in geometry)
                {
                    Solid solid = obj as Solid;
                    if (solid != null)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            XYZ normal = face.ComputeNormal(new UV(0, 0));
                            XYZ ZNormal = new XYZ(0, 0, 1);

                            if (MathUtils.IsParallel(normal, ZNormal))
                            {
                                BoundingBoxUV boundingBoxUV = face.GetBoundingBox();
                                UV min = boundingBoxUV.Min;
                                UV max = boundingBoxUV.Max;

                                floorObj.min = face.Evaluate(min);
                                floorObj.max = face.Evaluate(max);

                                bRangeComputed = true;

                                break;
                            }
                        }
                    }

                    if (bRangeComputed)
                    {
                        break;
                    }
                }

                //Add the line to the collection 
                _ = AddFloor(floorObj);
            }
        }


        /// <summary>
        /// Adds Input line to the 
        /// <param name="inputLine"> The Input Line to be added </param>
        /// <returns>True if the line is added to the collection </returns>
        public static bool AddFloor(FloorObject floor)
        {
            colFloors.Add(floor);
            return true;
        }
    }
}

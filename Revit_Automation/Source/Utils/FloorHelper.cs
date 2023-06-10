using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
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

                        FaceArray faceArray = solid.Faces;
                        List<Face> tempfilteredFaces = new List<Face>();
                        List<Face> filteredFaces = new List<Face>();
                        HashSet<XYZ> uniqueNormals = new HashSet<XYZ>();

                        // Take only those faces whose normals are in Z Plane
                        foreach (Face faceObj in faceArray)
                        {
                            // TO DO , Ignore Faces that have zero Z normal
                            XYZ normal = faceObj.ComputeNormal(new UV(0, 0));
                            double dotProduct = normal.DotProduct(XYZ.BasisZ);

                            // Check if the dot product is close to zero (indicating not perpendicular to Z-direction)
                            double tolerance = 1e-6;
                            if (Math.Abs(dotProduct) > tolerance)
                            {
                                tempfilteredFaces.Add(faceObj);
                            }
                        }

                        // Filter and collect unique faces with non-parallel normals
                        foreach (Face faceObj in tempfilteredFaces)
                        {
                            XYZ normal = faceObj.ComputeNormal(new UV(0, 0));
                            bool isParallel = false;

                            // Check if the normal is parallel to any previously processed normals
                            foreach (XYZ existingNormal in uniqueNormals)
                            {
                                double dotProduct = normal.DotProduct(existingNormal);

                                // Check if the dot product is close to 1 (indicating parallel)
                                double tolerance = 1e-6;
                                if (Math.Abs(Math.Abs(dotProduct) - 1.0) < tolerance)
                                {
                                    isParallel = true;
                                    break;
                                }
                            }

                            if (!isParallel)
                            {
                                _ = uniqueNormals.Add(normal);
                                filteredFaces.Add(faceObj);
                            }
                        }

                        foreach (Face face in filteredFaces)
                        {
                            XYZ normal = face.ComputeNormal(new UV(0, 0));
                            XYZ ZNormal = new XYZ(0, 0, 1);

                            BoundingBoxUV boundingBoxUV = face.GetBoundingBox();
                            UV min = boundingBoxUV.Min;
                            UV max = boundingBoxUV.Max;

                            floorObj.min = face.Evaluate(min);
                            floorObj.max = face.Evaluate(max);

                            bRangeComputed = true;

                            break;

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

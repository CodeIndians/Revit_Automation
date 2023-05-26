using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Revit_Automation.CustomTypes;
using System.Windows.Media.Animation;
using Autodesk.Revit.DB.Architecture;
using System.Diagnostics;
using Autodesk.Revit.UI;

namespace Revit_Automation.Source.Utils
{
    public class RoofUtility
    {
        public static List<RoofObject> colRoofs = new List<RoofObject>();
        public static void computeRoofSlopes(Document doc)
        {
            // Create a filter to get roof elements
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(RoofBase));

            // Get all the roof elements
            List<RoofBase> roofs = collector.Cast<RoofBase>().ToList();

            foreach (RoofBase roof in roofs)
            {
                Parameter phaseCreated = roof.get_Parameter(BuiltInParameter.PHASE_CREATED);
                string strBuldName = string.Empty;
                if (phaseCreated != null)
                {
                    strBuldName = phaseCreated.AsValueString();
                }
                // Get the geometry of the roof element
                GeometryElement geometryElement = roof.get_Geometry(new Options());

                if (geometryElement == null)   
                    continue;

                // Iterate through the geometry objects
                foreach (GeometryObject geometryObject in geometryElement)
                {
                    // Check if the geometry is a solid or a face
                    if (geometryObject is Solid solid)
                    {
                        // Get the faces Array
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
                                uniqueNormals.Add(normal);
                                filteredFaces.Add(faceObj);
                            }
                        }

                        foreach (Face faceObj in filteredFaces)
                        {
                            // Identify the slope curve
                            Curve selectedCurve = null;

                            IList<CurveLoop> curveLoops = faceObj.GetEdgesAsCurveLoops();
                            foreach (CurveLoop curveLoop in curveLoops)
                            {
                                foreach (Curve curve in curveLoop)
                                {
                                    XYZ startPoint = curve.GetEndPoint(0);
                                    XYZ endPoint = curve.GetEndPoint(1);

                                    if (Math.Abs(startPoint.Z - endPoint.Z) > double.Epsilon)
                                    {
                                        selectedCurve = curve;
                                        break;
                                    }
                                }
                            }

                            // Get the bounding box of the solid
                            BoundingBoxUV boundingBox = faceObj.GetBoundingBox();
                            UV min = boundingBox.Min;
                            UV max = boundingBox.Max;

                            XYZ pt1 = faceObj.Evaluate(min);
                            XYZ pt2 = faceObj.Evaluate(max);

                            RoofObject roofObject = new RoofObject();
                            roofObject.min = pt1;
                            roofObject.max = pt2;
                            roofObject.slopeLine = selectedCurve;
                            roofObject.strBuildingName = strBuldName;
                            RoofUtility.colRoofs.Add(roofObject);
                        }
                    }
                }
            }
        }
    }
}

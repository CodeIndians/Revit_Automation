using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Revit_Automation.Source.Utils
{
    public class RoofUtility
    {
        public static List<RoofObject> colRoofs = new List<RoofObject>();

        public static List<RoofObject> colExtendedRoofs = new List<RoofObject>();

        public static List<string> NamedRoofs = new List<string>();

        public static Document m_Document;

        public static void computeRoofSlopes(Document doc)
        {
            // Create a filter to get roof elements
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            _ = collector.OfClass(typeof(RoofBase));

            // Get all the roof elements
            List<RoofBase> roofs = collector.Cast<RoofBase>().ToList();

            // Clear the Named roofs collection
            NamedRoofs.Clear();

            foreach (RoofBase roof in roofs)
            {
                
                ElementId roofId = roof.Id;
                Parameter phaseCreated = roof.get_Parameter(BuiltInParameter.PHASE_CREATED);
                string strBuldName = string.Empty;
                if (phaseCreated != null)
                {
                    strBuldName = phaseCreated.AsValueString();
                }

                Parameter roofNameParam = roof.LookupParameter("Roof Name");
                if (roofNameParam != null) 
                {
                    NamedRoofs.Add(roofNameParam.AsString());
                }

                // Get the geometry of the roof element
                GeometryElement geometryElement = roof.get_Geometry(new Options());

                if (geometryElement == null)
                {
                    continue;
                }

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
                                _ = uniqueNormals.Add(normal);
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

                            RoofObject roofObject = new RoofObject
                            {
                                min = pt1,
                                max = pt2,
                                slopeLine = selectedCurve,
                                strBuildingName = strBuldName,
                                roofElementID = roofId
                            };


                            RoofUtility.colRoofs.Add(roofObject);

                            RoofObject roofObject1 = roofObject;
                            roofObject1.min = new XYZ(pt1.X - 1, pt1.Y - 1, pt1.Z);
                            roofObject1.max = new XYZ(pt2.X + 1, pt2.Y + 1, pt2.Z);
                            RoofUtility.colExtendedRoofs.Add(roofObject1);
                        }
                    }
                }
            }
            NamedRoofs.Sort();
        }

        public static XYZ GetRoofSlopeDirection(XYZ pt1)
        {
            Logger.logMessage("Method : GetRoofSlopeDirection");

            XYZ SlopeDirect = null;

            RoofObject targetRoof;
            targetRoof.slopeLine = null;

            foreach (RoofObject roof in RoofUtility.colRoofs)
            {
                double Xmin, Xmax, Ymin, Ymax = 0.0;
                Xmin = Math.Min(roof.max.X, roof.min.X);
                Xmax = Math.Max(roof.max.X, roof.min.X);
                Ymin = Math.Min(roof.max.Y, roof.min.Y);
                Ymax = Math.Max(roof.max.Y, roof.min.Y);

                if (pt1.X > Xmin && pt1.X < Xmax && pt1.Y > Ymin && pt1.Y < Ymax)
                {
                    targetRoof = roof;
                    break;
                }
            }

            //we are trying to intersect the point with extended roof
            if (targetRoof.slopeLine == null)
            {
                foreach (RoofObject roof in RoofUtility.colExtendedRoofs)
                {
                    double Xmin, Xmax, Ymin, Ymax = 0.0;
                    Xmin = Math.Min(roof.max.X, roof.min.X);
                    Xmax = Math.Max(roof.max.X, roof.min.X);
                    Ymin = Math.Min(roof.max.Y, roof.min.Y);
                    Ymax = Math.Max(roof.max.Y, roof.min.Y);

                    if (pt1.X > Xmin && pt1.X < Xmax && pt1.Y > Ymin && pt1.Y < Ymax)
                    {
                        targetRoof = roof;
                        break;
                    }
                }
            }

            if (targetRoof.slopeLine != null)
            {
                Curve SlopeCurve = targetRoof.slopeLine;
                XYZ start = SlopeCurve.GetEndPoint(0);
                XYZ end = SlopeCurve.GetEndPoint(1);

                XYZ slope = start.Z > end.Z ? (end - start) : (start - end);

                SlopeDirect = new XYZ(slope.X, slope.Y, 0.0);
            }

            return SlopeDirect;
        }
    }
}

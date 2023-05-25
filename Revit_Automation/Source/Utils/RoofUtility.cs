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

namespace Revit_Automation.Source.Utils
{
    public class RoofUtility
    {
        public static void computeRoofSlopes(Document doc)
        {

            // Get the filtered element collector.
            var collector = new FilteredElementCollector(doc);

            // Filter the collection to only include roof elements.
            collector.OfClass(typeof(RoofType));

            // Get the collection of roof elements.
            var roofElements = collector.ToElements();

            // Iterate over the collection and get the roof element by its name.
            foreach (var roof in roofElements)
            {

                // Get the geometry options for retrieving the slope curve
                Options options = new Options();
                options.ComputeReferences = true;
                options.IncludeNonVisibleObjects = true;

                Parameter roofSlopeParam = roof.get_Parameter(BuiltInParameter.ROOF_SLOPE);
                double slope = roofSlopeParam.AsDouble();

                // Get the geometry element of the roof at the reference level
                GeometryElement geometryElement = roof.get_Geometry(options);

                // Iterate through the geometry elements
                foreach (GeometryObject geometryObject in geometryElement)
                {
                    if (geometryObject is Solid solid)
                    {
                        foreach (Face face in solid.Faces)
                        {
                           
                        }
                        break;
                    }
                }

                Level level = doc.GetElement(roof.LevelId) as Level;

                // Get the geometry options for retrieving the slope curve
                Options options = new Options();
                options.ComputeReferences = true;
                options.IncludeNonVisibleObjects = true;

                // Get the geometry element of the roof at the reference level
                GeometryElement geometryElement = roof.get_Geometry(options);

                // Iterate through the geometry elements
                foreach (GeometryObject geometryObject in geometryElement)
                {
                    if (geometryObject is Solid solid)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            // Check if the face is a roof slope face
                            if (face is RoofFace roofFace && roofFace.Slope != 0.0)
                            {
                                // Get the curve loops representing the slope edge of the face
                                IList<CurveLoop> curveLoops = roofFace.GetEdgesAsCurveLoops();

                                // Assuming there is only one curve loop representing the slope
                                Curve slopeCurve = curveLoops.FirstOrDefault()?.FirstOrDefault();

                                // Further process or use the slope curve as needed
                                break;
                            }
                        }
                        break;
                    }
                }

            }
        }
    }
}

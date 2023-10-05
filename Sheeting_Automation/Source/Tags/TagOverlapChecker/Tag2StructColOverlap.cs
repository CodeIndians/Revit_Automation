using Autodesk.Revit.DB;
using Sheeting_Automation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    public class Tag2StructColOverlap : TagOverlapBase
    {
        /// <summary>
        /// Get all the structural column element ids in the current view
        /// </summary>
        /// <returns></returns>
        protected override List<ElementId> GetElementIds()
        {
            List<ElementId> elementIds = new List<ElementId>();

            // Create a filtered element collector 
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_ActiveViewId);

            // Filter for elements of category structural columns
            collector.OfCategory(BuiltInCategory.OST_StructuralColumns);


            foreach (Element element in collector)
            {
                elementIds.Add(element.Id);
            }

            return elementIds;
        }

        public override List<BoundingBoxXYZ> GetBoundingBoxesOfElement(ElementId elementId)
        {
            if (TagDataCache.cachedBoundingBoxDict.ContainsKey(elementId))
                return TagDataCache.cachedBoundingBoxDict[elementId];
            else
            {
                // get the detail element from the id
                Element detailElement = SheetUtils.m_Document.GetElement(elementId);

                // set the options 
                var options = new Options();
                options.ComputeReferences = true;
                options.View = SheetUtils.m_ActiveView;

                // collect the geometry instances to a list
                List<GeometryInstance> geomInstancesList = detailElement.get_Geometry(options)?
                                                               .Where(o => o is GeometryInstance)
                                                               .Cast<GeometryInstance>()
                                                               .ToList();



                var boundingBoxesList = new List<BoundingBoxXYZ> { GetBoundingBoxOfSolid(geomInstancesList) };

                TagDataCache.cachedBoundingBoxDict[elementId] = boundingBoxesList;

                // Retrieve the element using its ElementId
                return boundingBoxesList;
            }
        }

        protected BoundingBoxXYZ GetBoundingBoxOfSolid(List<GeometryInstance> geometryInstance)
        {
            BoundingBoxXYZ bBox = null;

            if(geometryInstance == null)
                return bBox;

            // iterate all the geometry instances 
            foreach (GeometryInstance geomInstance in geometryInstance)
            {
                // iterate all the shapes 
                foreach (GeometryObject geomObj in geomInstance.GetInstanceGeometry())
                {
                    var tempBox = TagUtils.GetBoundingBox(geomObj);
                    if (tempBox != null)
                    {
                        bBox = tempBox;
                        break;
                    }
                }
            }

            return bBox;
        }
    }
}

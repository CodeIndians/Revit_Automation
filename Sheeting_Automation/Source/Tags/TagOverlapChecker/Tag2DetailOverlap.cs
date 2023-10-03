using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Sheeting_Automation.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    public class Tag2DetailOverlap : TagOverlapBase
    {
        /// <summary>
        /// Get all the detail items ids in the current view
        /// </summary>
        /// <returns></returns>
        protected override List<ElementId> GetElementIds()
        {
            List<ElementId> elementIds = new List<ElementId>();

            // Create a filtered element collector
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id);

            // Filter for elements of category Wall
            collector.OfCategory(BuiltInCategory.OST_DetailComponents);


            foreach (Element element in collector)
            {
                if(element.Name.Contains("Call out")
                        || element.Name.Contains("Call Out")
                        || element.Name.Contains("call out")
                        || element.Name.Contains("call Out")
                        || element.Name.Contains("Section Detail")
                        || element.Name.Contains("Section detail")
                        || element.Name.Contains("section detail")
                        || element.Name.Contains("section Detail"))
                    elementIds.Add(element.Id);
            }

            return elementIds;
        }

        /// <summary>
        /// Retrieve the bounding box list of the element represented by its id on the active view 
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public override  List<BoundingBoxXYZ> GetBoundingBoxesOfElement(ElementId elementId)
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
                options.View = SheetUtils.m_Document.ActiveView;

                // collect the geometry instances to a list
                List<GeometryInstance> geomInstancesList = detailElement.get_Geometry(options)?
                                                               .Where(o => o is GeometryInstance)
                                                               .Cast<GeometryInstance>()
                                                               .ToList();

                // initialize the bounding boxes list
                List<BoundingBoxXYZ> boundingBoxes = new List<BoundingBoxXYZ>();

                if (geomInstancesList == null)
                    return boundingBoxes;

                // iterate all the geometry instances 
                foreach (GeometryInstance geomInstance in geomInstancesList)
                {
                    // iterate all the shapes 
                    foreach (GeometryObject geomObj in geomInstance.GetInstanceGeometry())
                    {
                        // add the bouding box of the geometry object
                        boundingBoxes.Add(TagUtils.GetBoundingBox(geomObj));
                    }
                }

                TagDataCache.cachedBoundingBoxDict[elementId] = boundingBoxes;
                // return the final bounding boxes list
                return boundingBoxes;
            }
        }

    }
}

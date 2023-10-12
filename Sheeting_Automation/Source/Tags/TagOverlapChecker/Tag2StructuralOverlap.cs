using Autodesk.Revit.DB;
using Sheeting_Automation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    public class Tag2StructuralOverlap : TagOverlapBase
    {
        /// <summary>
        /// Get all the detail items ids in the current view
        /// </summary>
        /// <returns></returns>
        protected override List<ElementId> GetElementIds()
        {
            List<ElementId> elementIds = new List<ElementId>();

            // Create a filtered element collector
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_ActiveViewId);

            // Filter for elements of category Wall
            collector.OfCategory(BuiltInCategory.OST_StructuralFraming);


            foreach (Element element in collector)
            {
                if (TagUtils.GetFamilyNameOfElement(element).Contains("Deck")
                    || TagUtils.GetFamilyNameOfElement(element).Contains("deck"))
                    continue;
                
                elementIds.Add(element.Id);
            }

            return elementIds;
        }

        /// <summary>
        /// Returns the structural conditon bounding box of the elements
        /// </summary>
        /// <returns></returns>
        public override List<ElementId> CheckOverlap()
        {
            List<ElementId> overlapElementIds = new List<ElementId>();

            // get the element ids
            var elementIds = GetElementIds();


            for (int i = 0; i < elementIds.Count; i++)
            {
                for (int j = 0; j < m_IndependentTags.Count; j++)
                {
                    Element overlapElement = SheetUtils.m_Document.GetElement(elementIds[i]);

                    Element tagElement = m_IndependentTags[j].GetTaggedLocalElements().FirstOrDefault();

                    if (TagUtils.GetFamilyNameOfElement(overlapElement) != TagUtils.GetFamilyNameOfElement(tagElement))
                        continue;

                    foreach (BoundingBoxXYZ boundingBoxXYZ in GetBoundingBoxesOfElement(elementIds[i]))
                    {
                        if (TagUtils.AreBoundingBoxesIntersecting(boundingBoxXYZ,
                                                                m_IndependentTags[j].get_BoundingBox(SheetUtils.m_Document.ActiveView)))
                        {
                            if (!overlapElementIds.Contains(m_IndependentTags[j].Id))
                            {
                                overlapElementIds.Add(m_IndependentTags[j].Id);
                                overlapElementIds.AddRange(m_IndependentTags[j].GetTaggedLocalElementIds());

                                var indexDiff = m_IndependentTags.Count - m_TagsWithLeaders.Count;
                                if (j >= indexDiff)
                                    elementIds.Add(m_TagsWithLeaders[j - indexDiff].Id);
                            }

                            if (!overlapElementIds.Contains(elementIds[i]))
                            {
                                overlapElementIds.Add(elementIds[i]);
                            }
                        }
                    }
                }
            }

            return overlapElementIds;
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
                List<GeometryInstance> geomInstancesList = detailElement.get_Geometry(options)
                                                               .Where(o => o is GeometryInstance)
                                                               .Cast<GeometryInstance>()
                                                               .ToList();

                // get the bounding box of the solid 
                var boundingBoxList = new List<BoundingBoxXYZ> { GetBoundingBoxOfSolid(geomInstancesList) };

                TagDataCache.cachedBoundingBoxDict[elementId] = boundingBoxList;

                return boundingBoxList;
            }
        }

        protected BoundingBoxXYZ GetBoundingBoxOfSolid(List<GeometryInstance> geometryInstance)
        {
            BoundingBoxXYZ bBox = null;

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

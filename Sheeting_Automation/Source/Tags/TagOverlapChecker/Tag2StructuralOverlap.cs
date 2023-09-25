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
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id);

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

                    if (TagUtils.AreBoudingBoxesIntersecting(GetBoundingBoxOfElement(elementIds[i]),
                                                                m_IndependentTags[j].get_BoundingBox(SheetUtils.m_Document.ActiveView)))
                    {
                        if (!overlapElementIds.Contains(m_IndependentTags[j].Id))
                        {
                            overlapElementIds.Add(m_IndependentTags[j].Id);
                            overlapElementIds.AddRange(m_IndependentTags[j].GetTaggedLocalElementIds());
                        }

                        if (!overlapElementIds.Contains(elementIds[i]))
                        {
                            overlapElementIds.Add(elementIds[i]);
                        }
                    }
                }
            }

            return overlapElementIds;
        }
    }
}

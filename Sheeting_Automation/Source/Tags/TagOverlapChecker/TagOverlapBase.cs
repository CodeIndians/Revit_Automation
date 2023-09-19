using Autodesk.Revit.DB;
using Sheeting_Automation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    public class TagOverlapBase
    {
        //list of all the independent tags in the current view
        protected List<IndependentTag> m_IndependentTags;

        /// <summary>
        /// Base class constrcutor
        /// Collects all the independent tags in the view
        /// </summary>
        public TagOverlapBase() 
        {
            // collect independent tags in the current view
            m_IndependentTags = TagUtils.GetAllTagsInView();
        }

        /// <summary>
        /// Returns null for now
        /// </summary>
        /// <returns></returns>
        public virtual List<ElementId> CheckOverlap()
        {
            List<ElementId> overlapElementIds = new List<ElementId>();

            // get the wall element ids
            var elementIds = GetElementIds();


            for (int i = 0; i < elementIds.Count; i++)
            {
                for (int j = 0; j < m_IndependentTags.Count; j++)
                {
                    if (TagUtils.AreBoudingBoxesIntersecting(GetBoundingBoxOfElement(elementIds[i]),
                                                                m_IndependentTags[j].get_BoundingBox(SheetUtils.m_Document.ActiveView)))
                    {
                        if (!overlapElementIds.Contains(m_IndependentTags[i].Id))
                        {
                            overlapElementIds.Add(m_IndependentTags[i].Id);
                            overlapElementIds.AddRange(m_IndependentTags[i].GetTaggedLocalElementIds());
                        }

                        if (!overlapElementIds.Contains(elementIds[j]))
                        {
                            overlapElementIds.Add(elementIds[j]);
                        }
                    }
                }
            }

            return overlapElementIds;
        }

        /// <summary>
        /// Get all the wall element ids in the current view
        /// </summary>
        /// <returns></returns>
        protected virtual List<ElementId> GetElementIds()
        {
            List<ElementId> elementIds = new List<ElementId>();

            // return empty list by default
            return elementIds;
        }

        /// <summary>
        /// Retrieve the bounding box of the element represented by its id on the active view 
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        protected BoundingBoxXYZ GetBoundingBoxOfElement(ElementId elementId)
        {
            // Retrieve the element using its ElementId
            return SheetUtils.m_Document.GetElement(elementId)?.get_BoundingBox(SheetUtils.m_Document.ActiveView);

        }

    }
}

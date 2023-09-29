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
        public static List<IndependentTag> m_IndependentTags;
        public static List<IndependentTag> m_TagsWithLeaders;
        public static List<IndependentTag> m_TagsWithOutLeaders;

        /// <summary>
        /// Base class constrcutor
        /// Collects all the independent tags in the view
        /// </summary>
        public TagOverlapBase() 
        {
            
        }

        public static void InitializeTags()
        {
            // collect independent tags in the current view
            m_IndependentTags = TagUtils.GetAllTagsInView();

            // collect tags with leaders 
            m_TagsWithLeaders = TagUtils.GetTagsWithLeaders(m_IndependentTags);

            m_IndependentTags.RemoveAll(tag => m_TagsWithLeaders.Contains(tag));

            m_TagsWithOutLeaders = new List<IndependentTag>();
            PlaceTagsWithOutLeaders();
        }

        /// <summary>
        /// Returns the generic bounding boxes of the elements
        /// </summary>
        /// <returns></returns>
        public virtual List<ElementId> CheckOverlap()
        {
            List<ElementId> overlapElementIds = new List<ElementId>();

            // get the element ids
            var elementIds = GetElementIds();


            for (int i = 0; i < elementIds.Count; i++)
            {
                for (int j = 0; j < m_IndependentTags.Count; j++)
                {
                    if (TagUtils.AreBoudingBoxesIntersecting(GetBoundingBoxOfElement(elementIds[i]),
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
        protected virtual BoundingBoxXYZ GetBoundingBoxOfElement(ElementId elementId)
        {
            // Retrieve the element using its ElementId
            return SheetUtils.m_Document.GetElement(elementId)?.get_BoundingBox(SheetUtils.m_Document.ActiveView);
        }

        private static void PlaceTagsWithOutLeaders()
        {

            using (Transaction transaction = new Transaction(SheetUtils.m_Document))
            {
                transaction.Start("Create no leader tags from leader tags");

                foreach (IndependentTag tag in m_TagsWithLeaders)
                {

                    IndependentTag noLeaderTag = IndependentTag.Create(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id, tag.GetTaggedReferences().FirstOrDefault(), false, TagMode.TM_ADDBY_CATEGORY, tag.TagOrientation, tag.TagHeadPosition);

                    noLeaderTag.ChangeTypeId(tag.GetTypeId());

                    m_TagsWithOutLeaders.Add(noLeaderTag);

                    m_IndependentTags.Add(noLeaderTag);
                }

                transaction.Commit();
            }
        }

        public static void DeleteNoLeaderTags()
        {
            using (Transaction transaction = new Transaction(SheetUtils.m_Document))
            {
                transaction.Start("Deleting No leader tags");

                foreach (IndependentTag tag in m_TagsWithOutLeaders)
                {
                   SheetUtils.m_Document.Delete(tag.Id);
                }

                transaction.Commit();
            }
        }

    }
}

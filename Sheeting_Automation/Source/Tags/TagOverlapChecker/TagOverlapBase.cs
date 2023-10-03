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
        // list of all the independent tags with leaders
        public static List<IndependentTag> m_TagsWithLeaders;
        // list of all the independent tags that will be created without leaders
        public static List<IndependentTag> m_TagsWithOutLeaders;

        public TagOverlapBase() 
        {
            
        }

        /// <summary>
        /// Collect and initialze all the tags
        /// separate leader tags
        /// create no leader tags 
        /// </summary>
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
            var overlapElementIds = new List<ElementId>();

            // get the wall element ids
            var elementIds = GetElementIds();


            for (int i = 0; i < elementIds.Count; i++)
            {
                for (int j = 0; j < m_IndependentTags.Count; j++)
                {
                    foreach (BoundingBoxXYZ boundingBoxXYZ in GetBoundingBoxesOfElement(elementIds[i]))
                    {
                        if (TagUtils.AreBoudingBoxesIntersecting(boundingBoxXYZ,
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
        public virtual List<BoundingBoxXYZ> GetBoundingBoxesOfElement(ElementId elementId)
        {
            
            if (TagDataCache.cachedBoundingBoxDict.ContainsKey(elementId))
                return TagDataCache.cachedBoundingBoxDict[elementId];
            else
            {
                var boundingBoxXYZList = new List<BoundingBoxXYZ> { SheetUtils.m_Document.GetElement(elementId)?.get_BoundingBox(SheetUtils.m_Document.ActiveView) };
                TagDataCache.cachedBoundingBoxDict[elementId] = boundingBoxXYZList;
                return boundingBoxXYZList;
            }
            
        }

        /// <summary>
        /// Place the tags with out leaders
        /// </summary>
        private static void PlaceTagsWithOutLeaders()
        {

            using (Transaction transaction = new Transaction(SheetUtils.m_Document))
            {
                transaction.Start("Create no leader tags from leader tags");

                foreach (IndependentTag tag in m_TagsWithLeaders)
                {
                    // create an idnependent tag with out leader from the leader tag
                    IndependentTag noLeaderTag = IndependentTag.Create(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id, tag.GetTaggedReferences().FirstOrDefault(), false, TagMode.TM_ADDBY_CATEGORY, tag.TagOrientation, tag.TagHeadPosition);
                    noLeaderTag.ChangeTypeId(tag.GetTypeId());

                    // track the tags without leaders into a list
                    m_TagsWithOutLeaders.Add(noLeaderTag);

                    // add the no leader tags to the main tags list
                    m_IndependentTags.Add(noLeaderTag);
                }

                transaction.Commit();
            }
        }

        // delete tags with no leaders
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

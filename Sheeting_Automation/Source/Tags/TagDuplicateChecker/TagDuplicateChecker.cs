using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Sheeting_Automation.Source.Tags.TagOverlapChecker;
using System.Collections.Generic;
using System.Linq;

namespace Sheeting_Automation.Source.Tags
{
    public static class TagDuplicateChecker
    {

        /// <summary>
        /// Checks if the elements that have multiple tags
        /// </summary>
        public static void CheckDuplicates()
        {
            // get all the tags in the current view 
            List<IndependentTag> tags = TagUtils.GetAllTagsInView();

            // initialize the dictionary
            Dictionary<ElementId,List<IndependentTag>> elemAndTagsDict = new Dictionary<ElementId,List<IndependentTag>>();

            /// Collect the tags and its corresponding tagged element ids into a dictionary
            ///////////////////////////////////////////////////////////////////////////////
            foreach (IndependentTag tag in tags)
            {
                var elementId = tag.GetTaggedLocalElementIds().FirstOrDefault();

                if(elemAndTagsDict.ContainsKey(elementId))
                {
                    // add the tag to the list if 
                    elemAndTagsDict[elementId].Add(tag);
                }
                else
                {
                    elemAndTagsDict[elementId] = new List<IndependentTag>() { tag };
                }
                
            }
            /////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////

            /// clear the graphic over ride element list
            /// /////////////////////////////////////////
            if (TagOverlapManager.m_ElementIds != null && TagOverlapManager.m_ElementIds.Count > 0)
            {
                // clear the overrides if they are already present
                TagGraphicOverrider.DeleteOverrides(TagOverlapManager.m_ElementIds);
            }
            TagOverlapManager.m_ElementIds = new List<ElementId>();
            /// /////////////////////////////////////////
            /// /////////////////////////////////////////

            int count = 0;

            /// Elements which have multiple tags are added to the override element list
            /// ////////////////////////////////////////////////////////////////////////
            foreach (var kvp in elemAndTagsDict) 
            {
                if(kvp.Value.Count > 1)
                {
                    count++;
                    TagOverlapManager.m_ElementIds.Add(kvp.Key);
                    foreach(IndependentTag tag in kvp.Value)
                    {
                        TagOverlapManager.m_ElementIds.Add(tag.Id);
                    }
                }
            }
            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            if(count > 0)
            {
                TaskDialog.Show("Info", $"{count} element(s) have duplicate tags");
                TagGraphicOverrider.CreateOverrides(TagOverlapManager.m_ElementIds);
            }
            else
            {
                TaskDialog.Show("Info", "No Duplicates found");
            }
        }
    }
}

using Autodesk.Revit.DB;
using Sheeting_Automation.Source.Tags.TagCreate.TagResolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sheeting_Automation.Source.Tags.TagData;

namespace Sheeting_Automation.Source.Tags.TagCreator
{
    public class TagResolverManager
    {
        private List<List<Tag>> mOverlappingTagLists; 

        private List<TagResolverBase> mTagResolverList;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public TagResolverManager() 
        {
            // compute the overlapping tag lists
            mOverlappingTagLists = GetOverlappingTagLists(BoundingBoxCollector.IndependentTags);

            AddTagResolvers();
        }

        private void AddTagResolvers()
        {
            mTagResolverList = new List<TagResolverBase>
            {
                new TagResolverGeneric(),
                new TagResolverParallel()
            };
        }

        public void ResolveTags()
        {
            foreach (var resolver in mTagResolverList) 
            {
                // run each resolver logic 
                resolver.Resolve(ref  mOverlappingTagLists);

                // re- collect tags from the overlapping tag list
                BoundingBoxCollector.IndependentTags = GetTagListFromOverlapTagList(mOverlappingTagLists);

                //re-collect overlapping tags list from the tags 
                mOverlappingTagLists = GetOverlappingTagLists(BoundingBoxCollector.IndependentTags);
            }
        }

        /// <summary>
        ///  Get the overlap list of tags from the collected tags
        /// </summary>
        /// <returns>List of List of overlapping tags</returns>
        private List<List<Tag>> GetOverlappingTagLists(List<Tag> tags)
        {
            List<List<Tag>> overlappingTagLists = new List<List<Tag>>();

            foreach(var tag in tags)
            {
                bool isIntersecting = false;

                for (int i = 0; i < overlappingTagLists.Count; i++)
                {
                    foreach (var overlapTag  in overlappingTagLists[i])
                    {
                        //check if the bouding boxes are intersecting 
                        if(TagUtils.AreBoundingBoxesIntersecting(tag.newBoundingBox,overlapTag.newBoundingBox))
                        {
                            isIntersecting = true;
                            overlappingTagLists[i].Add(tag);
                            break;
                        }
                    }

                    // break if intersecting is found
                    if (isIntersecting) { break; }
                }
                if (!isIntersecting)
                {
                    overlappingTagLists.Add(new List<Tag> { tag });
                }

            }

            return overlappingTagLists;
        }


        private List<Tag> GetTagListFromOverlapTagList(List<List<Tag>> overlapTagsList)
        {
            List<Tag> tagList = new List<Tag>();

            foreach (var overlaptags in overlapTagsList)
            {
                foreach(var tag in overlaptags)
                {
                    tagList.Add(tag);
                }
            }

            return tagList;
        }
    }
}

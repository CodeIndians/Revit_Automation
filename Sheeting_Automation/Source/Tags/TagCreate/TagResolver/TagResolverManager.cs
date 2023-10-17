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
            mOverlappingTagLists = GetOverlappingTagLists();

            AddTagResolvers();
        }

        private void AddTagResolvers()
        {
            mTagResolverList = new List<TagResolverBase>
            {
                new TagResolverGeneric(),
            };
        }

        public void ResolveTags()
        {
            
        }



        private List<List<Tag>> GetOverlappingTagLists()
        {
            List<List<Tag>> overlappingTagLists = new List<List<Tag>>();

            foreach(var tag in BoundingBoxCollector.IndependentTags)
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
                            overlappingTagLists[i].Add(overlapTag);
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
    }
}

using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using static Sheeting_Automation.Source.Tags.TagData;

namespace Sheeting_Automation.Source.Tags.TagCreate.TagResolver
{
    public class TagResolverGeneric : TagResolverBase 
    {
        protected override List<Tag> ResolveTagList(List<Tag> tagsList, ref List<List<Tag>> overlapTagsList)
        {
            if(tagsList.Count <= 5)
            {

                for(int i = 0; i < tagsList.Count - 1; i++)
                {
                    for(int j = i+1; j < tagsList.Count; j++)
                    {
                        if (TagUtils.AreBoundingBoxesIntersecting(tagsList[i].newBoundingBox, tagsList[j].newBoundingBox))
                        {
                            if (TagUtils.GetBBRatio(tagsList[i]) < TagUtils.GetBBRatio(tagsList[j]))
                            {
                                BoundingBoxXYZ iBoundingBox;
                                PickBestBoundingBox(tagsList[i], ref overlapTagsList, out iBoundingBox);
                                if (iBoundingBox != null)
                                {
                                    var tag = tagsList[i];
                                    tag.newBoundingBox = iBoundingBox;
                                    tagsList[i] = tag;
                                }
                            }
                            else
                            {
                                BoundingBoxXYZ iBoundingBox;
                                PickBestBoundingBox(tagsList[j], ref overlapTagsList, out iBoundingBox);
                                if (iBoundingBox != null)
                                {
                                    var tag = tagsList[j];
                                    tag.newBoundingBox = iBoundingBox;
                                    tagsList[j] = tag;
                                }
                            }
                            
                        }
                    }
                }

            }

            return tagsList;
        }

        private void PickBestBoundingBox (Tag tag, ref List<List<Tag>> overlapTagsList, out BoundingBoxXYZ bestBoundingBox)
        {
            bestBoundingBox = null;

            foreach(var boundingBox in tag.bestBoundingBoxes)
            {
                // if the tag is intersecting with the element, meaning this is not a valid best box 
                if (TagUtils.AreBoundingBoxesIntersecting(boundingBox, 
                                                          BoundingBoxCollector.BoundingBoxesDict[tag.mElement.Id].FirstOrDefault()))
                {
                    // skip to the next bounding box
                    continue;
                }

                // variable to keep track of intersections 
                int intersectCount = 0;

                // check for overlaps with all the existing tags 
                foreach(var bbList in overlapTagsList)
                {
                    foreach(var bb in bbList)
                    {
                        if (bb.mElement.Id == tag.mElement.Id)
                            continue;

                        if(TagUtils.AreBoundingBoxesIntersecting(bb.newBoundingBox,boundingBox))
                            intersectCount++;
                    }
                }

                // no intersections 
                if(intersectCount == 0)
                {
                    bestBoundingBox = boundingBox;
                    break;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sheeting_Automation.Source.Tags.TagData;

namespace Sheeting_Automation.Source.Tags.TagCreate.TagResolver
{
    public class TagResolverPerpendicular : TagResolverParallel
    {
        protected override List<Tag> ResolveTagList(List<Tag> tagsList, ref List<List<Tag>> overlapTagsList)
        {
            //only try to resolve up to 5 overlaps 
            if (tagsList.Count <= 5)
            {
                for (int i = 0; i < tagsList.Count - 1; i++)
                {
                    for (int j = i + 1; j < tagsList.Count; j++)
                    {
                        if (TagUtils.AreBoundingBoxesIntersecting(tagsList[i].newBoundingBox, tagsList[j].newBoundingBox))
                        {
                            // assign to temporary tags to pass them by reference 
                            var iTag = tagsList[i];
                            var jTag = tagsList[j];

                            // update th tag positions parallel to the element 
                            UpdateTagsPerpendicular(ref iTag, ref jTag, ref overlapTagsList);

                            // update the tag elements 
                            tagsList[i] = iTag;
                            tagsList[j] = jTag;
                        }
                    }
                }
            }
            return tagsList;
        }

        private void UpdateTagsPerpendicular(ref Tag tag1, ref Tag tag2, ref List<List<Tag>> overlapTagsList)
        {
            //clear the move data list 
            moveDataList.Clear();

            // update move data list with tag1 data 
            MoveTagPerpendicular(ref tag1, ref overlapTagsList);

            // update move data list with tag2 data
            MoveTagPerpendicular(ref tag2, ref overlapTagsList);


            /// Update the final tag positions from the move data list
            /// //////////////////////////////////////////////////////
            if (moveDataList.Count > 0)
            {
                // sort the movedata list by distance 
                moveDataList.Sort(new DistanceComparer());

                var closestValidMoveData = moveDataList.FirstOrDefault();

                if (closestValidMoveData.mTag.mTag.Id.Equals(tag1.mTag.Id)) // closest is the first tag, update the first tag 
                {
                    tag1.newBoundingBox = closestValidMoveData.computedBoundingBox;
                }
                else if (closestValidMoveData.mTag.mTag.Id.Equals(tag2.mTag.Id)) // closest is the second tag , update the second tag
                {
                    tag2.newBoundingBox = closestValidMoveData.computedBoundingBox;
                }
            }
            /// //////////////////////////////////////////////////////
            /// //////////////////////////////////////////////////////
        }

        private void MoveTagPerpendicular(ref Tag tag, ref List<List<Tag>> overlapTagsList)
        {
            // Ignore bounding box ratio check while moving in the perpendicular direction
            //if (TagUtils.GetBBRatio(tag) >= 1)
            //    return;

            // get the bounding box of the given tag element
            // assuming that the element of the tag has only one bounding box
            var elementBoundingBox = BoundingBoxCollector.BoundingBoxesDict[tag.mElement.Id].FirstOrDefault();

            double perpendicularOffset = 2.5f;

            // this means that the element bounding box is horizontal
            if (Math.Abs(elementBoundingBox.Max.X - elementBoundingBox.Min.X) >
                Math.Abs(elementBoundingBox.Max.Y - elementBoundingBox.Min.Y))
            {
                var upOffset = elementBoundingBox.Max.Y + perpendicularOffset;
                var downOffset = elementBoundingBox.Min.Y - perpendicularOffset;

                // MOVE UP
                MoveTagUp(ref tag, upOffset, ref overlapTagsList);

                // MOVE DOWN
                MoveTagBottom(ref tag, downOffset, ref overlapTagsList);

            }

            else // this means that the element bounding box is vertical
            {
                var leftOffset = elementBoundingBox.Min.X - perpendicularOffset;
                var rightOffset = elementBoundingBox.Max.X + perpendicularOffset;

                //try bottom 
                MoveTagLeft(ref tag, leftOffset, ref overlapTagsList);

                //try up
                MoveTagRight(ref tag, rightOffset, ref overlapTagsList);

            }
        }
    }
}

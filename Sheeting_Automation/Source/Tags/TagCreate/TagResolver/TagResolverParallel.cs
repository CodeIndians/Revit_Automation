using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sheeting_Automation.Source.Tags.TagData;

namespace Sheeting_Automation.Source.Tags.TagCreate.TagResolver
{
    public class TagResolverParallel : TagResolverBase
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
                            UpdateTagsParallel(ref iTag, ref jTag, ref overlapTagsList);

                            // update the tag elements 
                            tagsList[i] = iTag;
                            tagsList[j] = jTag;
                        }
                    }
                }
            }
            return tagsList;
        }

        private void UpdateTagsParallel(ref Tag tag1, ref Tag tag2, ref List<List<Tag>> overlapTagsList)
        {
            //clear the move data list 
            moveDataList.Clear();

            // update move data list with tag1 data 
            MoveTagParallel(ref tag1, ref overlapTagsList);

            // update move data list with tag2 data
            MoveTagParallel(ref tag2, ref overlapTagsList);


            /// Update the final tag positions from the move data list
            /// //////////////////////////////////////////////////////
            if(moveDataList.Count > 0)
            {
                // sort the movedata list by distance 
                moveDataList.Sort(new DistanceComparer());

                var closestValidMoveData = moveDataList.FirstOrDefault();

                if(closestValidMoveData.mTag.mTag.Id.Equals(tag1.mTag.Id)) // closest is the first tag, update the first tag 
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

        private void MoveTagParallel(ref Tag tag, ref List<List<Tag>> overlapTagsList)
        {
            // dont perform this for elements that have tags bigger than elements 
            if (TagUtils.GetBBRatio(tag) >= 1)
                return;

            // get the bounding box of the given tag element
            // assuming that the element of the tag has only one bounding box
            var elementBoundingBox = BoundingBoxCollector.BoundingBoxesDict[tag.mElement.Id].FirstOrDefault();

            // this means that the element bounding box is horizontal
            if (Math.Abs(elementBoundingBox.Max.X - elementBoundingBox.Min.X) >
                Math.Abs(elementBoundingBox.Max.Y - elementBoundingBox.Min.Y))
            {
                var leftOffset = elementBoundingBox.Min.X;
                var rightOffset = elementBoundingBox.Max.X;

                // try to move left and add the data to the move list 
                MoveTagLeft(ref tag, leftOffset, ref overlapTagsList);

                // try to move right and add the data to the move list
                MoveTagRight(ref tag, rightOffset, ref overlapTagsList);

            }

            else // this means that the element bounding box is vertical
            {
                var bottomOffset = elementBoundingBox.Min.Y;
                var topOffset = elementBoundingBox.Max.Y;

                //try bottom 
                MoveTagBottom(ref tag, bottomOffset, ref overlapTagsList);

                //try up
                MoveTagUp(ref tag, topOffset, ref overlapTagsList);
               
            }

        }

        protected void MoveTagUp(ref Tag tag, double topOffset, ref List<List<Tag>> overlapTagsList)
        {
            var computedBoundingBox = new BoundingBoxXYZ();
            computedBoundingBox.Min = tag.newBoundingBox.Min;
            computedBoundingBox.Max = tag.newBoundingBox.Max;

            var moveOffSet = new XYZ(0, 0.3, 0);

            // move the tag till the edge of the element
            while(computedBoundingBox.Max.Y <= topOffset)
            {
                // move the tag in the up direction till the specified top offset
                computedBoundingBox.Min = computedBoundingBox.Min + moveOffSet;
                computedBoundingBox.Max = computedBoundingBox.Max + moveOffSet;

                /// check if the computed bounding box is valid
                /// ///////////////////////////////////////////
                if (TagUtils.AreBoundingBoxesIntersecting(computedBoundingBox, tag.nearestElementBoundingBoxes))
                    continue;

                // check if the new computed bounding box is 
                if (TagUtils.AreBoundingBoxesIntersecting(computedBoundingBox, tag.mElement.Id, overlapTagsList))
                    continue;
                else
                {
                    // create the move data 
                    MoveData moveData = new MoveData(Math.Abs(tag.newBoundingBox.Min.Y - computedBoundingBox.Min.Y),computedBoundingBox,tag);

                    //add the move data to the move data list 
                    moveDataList.Add(moveData);

                    // break the while loop
                    break;
                }
                /// ///////////////////////////////////////////
                /// ///////////////////////////////////////////
            }
        }

        protected void MoveTagBottom(ref Tag tag, double bottomOffset, ref List<List<Tag>> overlapTagsList)
        {
            var computedBoundingBox = new BoundingBoxXYZ();
            computedBoundingBox.Min = tag.newBoundingBox.Min;
            computedBoundingBox.Max = tag.newBoundingBox.Max;

            var moveOffSet = new XYZ(0, -0.3, 0);

            // move the tag till the edge of the element
            while (computedBoundingBox.Max.Y >= bottomOffset)
            {
                // move the tag in the bottom direction till the specified bottom offset
                computedBoundingBox.Min = computedBoundingBox.Min + moveOffSet;
                computedBoundingBox.Max = computedBoundingBox.Max + moveOffSet;

                /// check if the computed bounding box is valid
                /// ///////////////////////////////////////////
                if (TagUtils.AreBoundingBoxesIntersecting(computedBoundingBox, tag.nearestElementBoundingBoxes))
                    continue;

                // check if the new computed bounding box is 
                if (TagUtils.AreBoundingBoxesIntersecting(computedBoundingBox, tag.mElement.Id, overlapTagsList))
                    continue;
                else
                {
                    // create the move data 
                    MoveData moveData = new MoveData(Math.Abs(tag.newBoundingBox.Min.Y - computedBoundingBox.Min.Y), computedBoundingBox, tag);

                    //add the move data to the move data list 
                    moveDataList.Add(moveData);

                    // break the while loop
                    break;
                }
                /// ///////////////////////////////////////////
                /// ///////////////////////////////////////////

            }
        }

        protected void MoveTagRight(ref Tag tag, double rightOffset, ref List<List<Tag>> overlapTagsList)
        {
            var computedBoundingBox = new BoundingBoxXYZ();
            computedBoundingBox.Min = tag.newBoundingBox.Min;
            computedBoundingBox.Max = tag.newBoundingBox.Max;

            var moveOffSet = new XYZ(0.3, 0, 0);

            // move the tag till the edge of the element
            while (computedBoundingBox.Max.X <= rightOffset)
            {
                // move the tag in the right direction till the specified right offset
                computedBoundingBox.Min = computedBoundingBox.Min + moveOffSet;
                computedBoundingBox.Max = computedBoundingBox.Max + moveOffSet;

                /// check if the computed bounding box is valid
                /// ///////////////////////////////////////////
                if (TagUtils.AreBoundingBoxesIntersecting(computedBoundingBox, tag.nearestElementBoundingBoxes))
                    continue;

                // check if the new computed bounding box is 
                if (TagUtils.AreBoundingBoxesIntersecting(computedBoundingBox, tag.mElement.Id, overlapTagsList))
                    continue;
                else
                {
                    // create the move data 
                    MoveData moveData = new MoveData(Math.Abs(tag.newBoundingBox.Min.X - computedBoundingBox.Min.X), computedBoundingBox, tag);

                    //add the move data to the move data list 
                    moveDataList.Add(moveData);

                    // break the while loop
                    break;
                }
                /// ///////////////////////////////////////////
                /// ///////////////////////////////////////////

            }
        }

        protected void MoveTagLeft(ref Tag tag, double leftOffset, ref List<List<Tag>> overlapTagsList)
        {
            var computedBoundingBox = new BoundingBoxXYZ();
            computedBoundingBox.Min = tag.newBoundingBox.Min;
            computedBoundingBox.Max = tag.newBoundingBox.Max;

            var moveOffSet = new XYZ(-0.3, 0, 0);

            // move the tag till the edge of the element
            while (computedBoundingBox.Max.X >= leftOffset)
            {
                // move the tag in the left direction till the specified left offset
                computedBoundingBox.Min = computedBoundingBox.Min + moveOffSet;
                computedBoundingBox.Max = computedBoundingBox.Max + moveOffSet;

                /// check if the computed bounding box is valid
                /// ///////////////////////////////////////////
                if (TagUtils.AreBoundingBoxesIntersecting(computedBoundingBox, tag.nearestElementBoundingBoxes))
                    continue;

                // check if the new computed bounding box is 
                if (TagUtils.AreBoundingBoxesIntersecting(computedBoundingBox, tag.mElement.Id, overlapTagsList))
                    continue;
                else
                {
                    // create the move data 
                    MoveData moveData = new MoveData(Math.Abs(tag.newBoundingBox.Min.X - computedBoundingBox.Min.X), computedBoundingBox, tag);

                    //add the move data to the move data list 
                    moveDataList.Add(moveData);

                    // break the while loop
                    break;
                }
                /// ///////////////////////////////////////////
                /// ///////////////////////////////////////////
            }
        }
    }
}

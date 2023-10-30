using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sheeting_Automation.Source.Tags.TagData;

namespace Sheeting_Automation.Source.Tags.TagCreate.TagResolver
{
    public class TagResolverExhaustive : TagResolverBase
    {
        /// <summary>
        /// Struct for storing the movement data of both the tags 
        /// Usually stored as List of MoveDataEx
        /// this list is used in computing the best bounding boxes for each alogirthm
        /// </summary>
        protected struct MoveDataEx
        {
            public double distanceTag1;
            public double distanceTag2;
            public BoundingBoxXYZ computedBoundingBoxTag1;
            public BoundingBoxXYZ computedBoundingBoxTag2;
        }

        protected class DistanceComparerEx : IComparer<MoveDataEx>
        {
            // compare the combined distances 
            public int Compare(MoveDataEx x, MoveDataEx y)
            {
                return (x.distanceTag1 + x.distanceTag2).CompareTo((y.distanceTag1 + y.distanceTag2));
            }
        }

        protected struct MoveDataExSingle
        {
            public double distanceTag1;
            public BoundingBoxXYZ computedBoundingBoxTag1;
        }

        protected class DistanceComparerExSingle : IComparer<MoveDataExSingle>
        {
            // compare the combined distances 
            public int Compare(MoveDataExSingle x, MoveDataExSingle y)
            {
                return (x.distanceTag1).CompareTo((y.distanceTag1));
            }
        }

        protected class TagBoundary
        {
            public double MinX;
            public double MinY;
            public double MaxX;
            public double MaxY;

            public TagBoundary()
            {
                this.MaxY = 0;
                this.MinX = 0;
                this.MinY = 0;
                this.MaxX = 0;
            }
        }

        protected List<MoveDataEx> mMoveDataExList;
        protected List<MoveDataExSingle> mMoveDataExSingleList;

        protected override List<Tag> ResolveTagList(List<Tag> tagsList, ref List<List<Tag>> overlapTagsList)
        {
            //only try to resolve up to 5 overlaps 
            if (tagsList.Count <= 5)
            {
                int first = -1;
                int second = -1;
                for (int i = 0; i < tagsList.Count - 1; i++)
                {
                    //capture only single intersections
                    if(GetNoOfIntersections(i,ref tagsList) == 1)
                    {
                        // update the indexes
                        if (first == -1)
                            first = i;
                        else if (second == -1)
                            second = i;
                    }

                    // check if both the intersctions are found 
                    if ( first != -1 && second != -1)
                    {
                        // assign to temporary tags to pass them by reference 
                        var iTag = tagsList[first];
                        var jTag = tagsList[second];

                        // update th tag positions parallel to the element 
                        UpdateTagsExhaustive(ref iTag, ref jTag, ref overlapTagsList);

                        // update the tag elements 
                        tagsList[first] = iTag;
                        tagsList[second] = jTag;

                        //reset the stored indexes 
                        first = -1;
                        second = -1;
                    }
                }

                // it is possible that one tag can be left out without resolving 
                if(first != -1)
                {
                    var iTag = tagsList[first];

                    UpdateTagsExhaustive(ref iTag, ref overlapTagsList);

                    tagsList[first] = iTag;
                }
            }
            return tagsList;
        }

        private void UpdateTagsExhaustive(ref Tag tag1, ref Tag tag2, ref List<List<Tag>> overlapTagsList)
        {
            // clear the move data list / re - initialize
            mMoveDataExList = new List<MoveDataEx>();

            //Move tags exhaustive
            MoveTagsExhaustive(ref tag1, ref tag2, ref overlapTagsList);

            /// Update the final tag positions from the move data list
            /// //////////////////////////////////////////////////////
            if(mMoveDataExList.Count > 0)
            {
                //sort the move data ex list by distance
                mMoveDataExList.Sort(new DistanceComparerEx());

                // get the shortest best bounding boxes 
                var closestValidMoveData = mMoveDataExList.FirstOrDefault();

                // assign the new bounding boxes to tags 
                tag1.newBoundingBox = closestValidMoveData.computedBoundingBoxTag1;
                tag2.newBoundingBox = closestValidMoveData.computedBoundingBoxTag2;
            }
            /// //////////////////////////////////////////////////////
            /// //////////////////////////////////////////////////////
        }

        private void UpdateTagsExhaustive(ref Tag tag1, ref List<List<Tag>> overlapTagsList)
        {
            // clear the move data list / re - initialize
            mMoveDataExSingleList = new List<MoveDataExSingle>();

            //Move tags exhaustive
            MoveTagsExhaustive(ref tag1, ref overlapTagsList);

            /// Update the final tag positions from the move data list
            /// //////////////////////////////////////////////////////
            if (mMoveDataExSingleList.Count > 0)
            {
                //sort the move data ex list by distance
                mMoveDataExSingleList.Sort(new DistanceComparerExSingle());

                // get the shortest best bounding boxes 
                var closestValidMoveData = mMoveDataExSingleList.FirstOrDefault();

                // assign the new bounding boxes to tags 
                tag1.newBoundingBox = closestValidMoveData.computedBoundingBoxTag1;
            }
            /// //////////////////////////////////////////////////////
            /// //////////////////////////////////////////////////////
        }

        private void MoveTagsExhaustive(ref Tag tag1, ref Tag tag2, ref List<List<Tag>> overlapTagsList)
        {
            // initialize the tag boundaries for both the tags
            TagBoundary tagBoundary1 = new TagBoundary();
            TagBoundary tagBoundary2 = new TagBoundary();

            // update the tag boundaries of both the tags 
            UpdateTagBoundaries(ref tag1, ref tagBoundary1 );
            UpdateTagBoundaries(ref tag2, ref tagBoundary2 );

            MoveTagsExhaustiveInternal(ref tag1, ref tag2, ref tagBoundary1, ref tagBoundary2,ref overlapTagsList);
        }

        private void MoveTagsExhaustive(ref Tag tag1, ref List<List<Tag>> overlapTagsList)
        {
            // initialize the tag boundaries for both the tags
            TagBoundary tagBoundary1 = new TagBoundary();

            // update the tag boundaries of both the tags 
            UpdateTagBoundaries(ref tag1, ref tagBoundary1);

            MoveTagsExhaustiveInternal(ref tag1, ref tagBoundary1, ref overlapTagsList);
        }

        private void UpdateTagBoundaries(ref Tag tag, ref TagBoundary tagBoundary)
        {
            // get the bounding box of the given tag element
            // assuming that the element of the tag has only one bounding box
            var elementBoundingBox = BoundingBoxCollector.BoundingBoxesDict[tag.mElement.Id].FirstOrDefault();

            // boundary extending offset 
            var offSet = 2.5;

            // ratio is greater than 1 ( this will happen for elments like studs )
            if (TagUtils.GetBBRatio(tag) >= 1)
            {
                /// extend the boundaries beyound the element boundary box by offset
                /// //////////////////////////////////////////////////////
                tagBoundary.MinX = elementBoundingBox.Min.X - offSet;
                tagBoundary.MaxX = elementBoundingBox.Max.X + offSet;

                tagBoundary.MinY = elementBoundingBox.Min.Y - offSet;
                tagBoundary.MaxY = elementBoundingBox.Max.Y + offSet;
                /// //////////////////////////////////////////////////////
                /// //////////////////////////////////////////////////////
            }
            else // ratio is less than 1 (  elements like purlins where tags are smaller than elements )
            {
                /// check if the bounding box is horizontal or vertical and assign 
                /// the bounding boxes accordingly ///////////////////////////////
                if (Math.Abs(elementBoundingBox.Max.X - elementBoundingBox.Min.X) >
                    Math.Abs(elementBoundingBox.Max.Y - elementBoundingBox.Min.Y)) //horizontal  bounding box ( 
                {
                    // no offset applied on X direction
                    tagBoundary.MinX = elementBoundingBox.Min.X;
                    tagBoundary.MaxX = elementBoundingBox.Max.X;

                    // offset is applied on only Y direction
                    tagBoundary.MinY = elementBoundingBox.Min.Y - offSet;
                    tagBoundary.MaxY = elementBoundingBox.Max.Y + offSet;
                }
                else //vertical bounding box 
                {
                    // offset is applied only on X direction 
                    tagBoundary.MinX = elementBoundingBox.Min.X - offSet;
                    tagBoundary.MaxX = elementBoundingBox.Max.X + offSet;

                    // no offset is applied on Y direction
                    tagBoundary.MinY = elementBoundingBox.Min.Y;
                    tagBoundary.MaxY = elementBoundingBox.Max.Y;
                }
                /// ////////////////////////////////////////////////////////////// 
                /// ////////////////////////////////////////////////////////////// 

            }
        }

        /// <summary>
        /// Final move logic 
        /// </summary>
        /// <param name="tag1">first tag in the overlap tag list </param>
        /// <param name="tag2">second tag in the overlap tag list </param>
        /// <param name="tagBoundary1">tag 1 boundary struct</param>
        /// <param name="tagBoundary2">tag 2 boundary struct</param>
        /// <param name="overlapTagsList">complete overlap tags list </param>
        private void MoveTagsExhaustiveInternal(ref Tag tag1, ref Tag tag2, 
                                                ref TagBoundary tagBoundary1, ref TagBoundary tagBoundary2,
                                                ref List<List<Tag>> overlapTagsList)
        {
            /// capture width and height of tag1 and tag2
            /// /////////////////////////////////////////
            var widthTag1 = Math.Abs(tag1.newBoundingBox.Min.X - tag1.newBoundingBox.Max.X);
            var widthTag2 = Math.Abs(tag2.newBoundingBox.Min.X - tag2.newBoundingBox.Max.X);
            var heightTag1 = Math.Abs(tag1.newBoundingBox.Min.Y - tag1.newBoundingBox.Max.Y);
            var heightTag2 = Math.Abs(tag2.newBoundingBox.Min.Y - tag2.newBoundingBox.Max.Y);
            /// /////////////////////////////////////////
            /// /////////////////////////////////////////

            /// capture width and height of tagBoundary1 and tagBoundary2
            /// /////////////////////////////////////////
            var widthTagBoundary1 = Math.Abs(tagBoundary1.MaxX - tagBoundary1.MinX);
            var widthTagBoundary2 = Math.Abs(tagBoundary2.MaxX - tagBoundary2.MinX);
            var heightTagBoundary1 = Math.Abs(tagBoundary1.MaxY - tagBoundary1.MinY);
            var heightTagBoundary2 = Math.Abs(tagBoundary2.MaxY - tagBoundary2.MinY);
            /// /////////////////////////////////////////
            /// /////////////////////////////////////////

            // compute the bounding boxes for both the 
            var computedBoundingBox1 = GetTopLeftBoundingBox(ref tag1,tagBoundary1,widthTag1,heightTag1);
            var computedBoundingBox2 = GetTopLeftBoundingBox(ref tag2,tagBoundary2,widthTag2,heightTag2);

            /// Algorithm start ///////////////////////////////////////
            /// ///////////////////////////////////////////////////////
            var possibleBoundingBoxes1 = GetPossibleBoundingBoxes(computedBoundingBox1, widthTagBoundary1, heightTagBoundary1, widthTag1, heightTag1);
            var possibleBoundingBoxes2 = GetPossibleBoundingBoxes(computedBoundingBox2, widthTagBoundary2, heightTagBoundary2, widthTag2, heightTag2);

            foreach(var possibleBoundingBox1 in possibleBoundingBoxes1)
            {
                foreach(var possibleBoundingBox2 in possibleBoundingBoxes2)
                {
                    AddMoveDataEx(possibleBoundingBox1,possibleBoundingBox2,ref tag1,ref tag2, overlapTagsList);
                }
            }

            /// ///////////////////////////////////////////////////////
            /// ///////////////////////////////////////////////////////
        }

        /// <summary>
        /// Final move logic 
        /// </summary>
        /// <param name="tag1">first tag in the overlap tag list </param>
        /// <param name="tagBoundary1">tag 1 boundary struct</param>
        /// <param name="overlapTagsList">complete overlap tags list </param>
        private void MoveTagsExhaustiveInternal(ref Tag tag1,
                                                ref TagBoundary tagBoundary1,
                                                ref List<List<Tag>> overlapTagsList)
        {
            /// capture width and height of tag1 
            /// /////////////////////////////////////////
            var widthTag1 = Math.Abs(tag1.newBoundingBox.Min.X - tag1.newBoundingBox.Max.X);
            var heightTag1 = Math.Abs(tag1.newBoundingBox.Min.Y - tag1.newBoundingBox.Max.Y);
            /// /////////////////////////////////////////
            /// /////////////////////////////////////////

            /// capture width and height of tagBoundary1
            /// /////////////////////////////////////////
            var widthTagBoundary1 = Math.Abs(tagBoundary1.MaxX - tagBoundary1.MinX);
            var heightTagBoundary1 = Math.Abs(tagBoundary1.MaxY - tagBoundary1.MinY);
            /// /////////////////////////////////////////
            /// /////////////////////////////////////////

            // compute the bounding boxes for both the 
            var computedBoundingBox1 = GetTopLeftBoundingBox(ref tag1, tagBoundary1, widthTag1, heightTag1);

            /// Algorithm start ///////////////////////////////////////
            /// ///////////////////////////////////////////////////////
            var possibleBoundingBoxes1 = GetPossibleBoundingBoxes(computedBoundingBox1, widthTagBoundary1, heightTagBoundary1, widthTag1, heightTag1);

            foreach (var possibleBoundingBox1 in possibleBoundingBoxes1)
            {
               AddMoveDataExSingle(possibleBoundingBox1, ref tag1, overlapTagsList);
            }

            /// ///////////////////////////////////////////////////////
            /// ///////////////////////////////////////////////////////
        }

        private void AddMoveDataEx(BoundingBoxXYZ possibleBoundingBox1, BoundingBoxXYZ possibleBoundingBox2, ref Tag tag1, ref Tag tag2, List<List<Tag>> overlapTagsList)
        {
            //check element intersection 
            if (TagUtils.AreBoundingBoxesIntersecting(possibleBoundingBox1, BoundingBoxCollector.BoundingBoxesDict[tag1.mElement.Id].FirstOrDefault()))
                return;

            //check element intersection 
            if (TagUtils.AreBoundingBoxesIntersecting(possibleBoundingBox2, BoundingBoxCollector.BoundingBoxesDict[tag2.mElement.Id].FirstOrDefault()))
                return;

            // check if the given bounding boxes are intersecting first
            if (TagUtils.AreBoundingBoxesIntersecting(possibleBoundingBox1, possibleBoundingBox2))
                return;

            //check if the given bounding boxes intersect with the elements
            if (TagUtils.AreBoundingBoxesIntersecting(possibleBoundingBox1, tag1.nearestElementBoundingBoxes))
                return;
            if (TagUtils.AreBoundingBoxesIntersecting(possibleBoundingBox2, tag2.nearestElementBoundingBoxes))
                return;

            if(TagUtils.AreBoundingBoxesIntersecting(possibleBoundingBox1,tag1.mElement.Id,tag2.mElement.Id,overlapTagsList))
                return;
            if(TagUtils.AreBoundingBoxesIntersecting(possibleBoundingBox2,tag1.mElement.Id, tag2.mElement.Id, overlapTagsList))
                return;
            else
            {
                //create the move data
                MoveDataEx moveDataEx = new MoveDataEx();
                moveDataEx.computedBoundingBoxTag1 = possibleBoundingBox1;
                moveDataEx.computedBoundingBoxTag2 = possibleBoundingBox2;
                moveDataEx.distanceTag1 = TagUtils.GetDistanceFromElement(possibleBoundingBox1, BoundingBoxCollector.BoundingBoxesDict[tag1.mElement.Id].FirstOrDefault());
                moveDataEx.distanceTag2 = TagUtils.GetDistanceFromElement(possibleBoundingBox2, BoundingBoxCollector.BoundingBoxesDict[tag2.mElement.Id].FirstOrDefault());

                // add the move data to the list
                mMoveDataExList.Add(moveDataEx);
            }

        }

        private void AddMoveDataExSingle(BoundingBoxXYZ possibleBoundingBox1, ref Tag tag1, List<List<Tag>> overlapTagsList)
        {
            //check element intersection 
            if (TagUtils.AreBoundingBoxesIntersecting(possibleBoundingBox1, BoundingBoxCollector.BoundingBoxesDict[tag1.mElement.Id].FirstOrDefault()))
                return;

            //check if the given bounding boxes intersect with the elements
            if (TagUtils.AreBoundingBoxesIntersecting(possibleBoundingBox1, tag1.nearestElementBoundingBoxes))
                return;
            

            if (TagUtils.AreBoundingBoxesIntersecting(possibleBoundingBox1, tag1.mElement.Id, overlapTagsList))
                return;
            else
            {
                //create the move data
                MoveDataExSingle moveDataEx = new MoveDataExSingle();
                moveDataEx.computedBoundingBoxTag1 = possibleBoundingBox1;
                moveDataEx.distanceTag1 = TagUtils.GetDistanceFromElement(possibleBoundingBox1, BoundingBoxCollector.BoundingBoxesDict[tag1.mElement.Id].FirstOrDefault());

                // add the move data to the list
                mMoveDataExSingleList.Add(moveDataEx);
            }

        }

        /// <summary>
        /// Get the top left bounding box from the given width, height and tagboundary struct 
        /// </summary>
        /// <param name="tag">tag </param>
        /// <param name="tagBoundary">tag boundary struct</param>
        /// <param name="width">width of the tag</param>
        /// <param name="height">height of the tag</param>
        /// <returns></returns>
        private BoundingBoxXYZ GetTopLeftBoundingBox(ref Tag tag,TagBoundary tagBoundary, double width, double height)
        {
            // calculate the initial min point of the given tag
            double xBoxMin = tagBoundary.MinX;
            double yBoxMin = tagBoundary.MaxY - height;
            double zBoxMin = tag.newBoundingBox.Min.Z;

            // calculate the initial max point of the given tag
            double xBoxMax = tagBoundary.MinX + width;
            double yBoxMax = tagBoundary.MaxY;
            double zBoxMax = tag.newBoundingBox.Min.Z;

            // compute the bounding box from the min and max points 
            BoundingBoxXYZ topLeftBoundingBox = new BoundingBoxXYZ();
            topLeftBoundingBox.Min = new XYZ(xBoxMin, yBoxMin,zBoxMin);
            topLeftBoundingBox.Max = new XYZ(xBoxMax, yBoxMax, zBoxMax);

            return topLeftBoundingBox;
        }

        /// <summary>
        /// Get all the possible tag boundary boxes that could fit in the given boundary box 
        /// </summary>
        /// <param name="boundingBox">bounding box of the tag which is set to top left</param>
        /// <param name="widthTagBoundary">width of the bigger boundary box</param>
        /// <param name="heightTagBoundary">height of the bigger boundary box</param>
        /// <param name="widthTag">width of the tag</param>
        /// <param name="heightTag">height of the tag</param>
        /// <returns></returns>
        private List<BoundingBoxXYZ> GetPossibleBoundingBoxes(BoundingBoxXYZ boundingBox,
                                                              double widthTagBoundary, double heightTagBoundary,
                                                              double widthTag, double heightTag)
        {
            var boundingBoxList = new List<BoundingBoxXYZ>();

            var offset = 0.3;

            // assign the horizontal and vertical bounding boxes 
            var boundingBoxHorizontal = MoveBoundingBox(boundingBox, MoveDirection.Zero, 0);
            var boundingBoxVertical = MoveBoundingBox(boundingBox, MoveDirection.Zero, 0);

            // collect all the bounding boxes in the given tag boundary range 
            for (double i = 0; i < widthTagBoundary - widthTag; i = i + offset)
            {
                boundingBoxHorizontal = MoveBoundingBox(boundingBoxHorizontal, MoveDirection.Right, offset);
                boundingBoxVertical = MoveBoundingBox(boundingBoxHorizontal, MoveDirection.Zero, 0);
                boundingBoxList.Add(boundingBoxHorizontal);
                for (double j = 0; j < heightTagBoundary - heightTag; j = j + offset)
                {
                    boundingBoxVertical = MoveBoundingBox(boundingBoxVertical, MoveDirection.Down, offset);
                    boundingBoxList.Add(boundingBoxVertical);
                }
            }

            return boundingBoxList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="boundingBox">ref param of the bounding box that has to be moved</param>
        /// <param name="direction"></param>
        /// <param name="offset"></param>
        private BoundingBoxXYZ MoveBoundingBox(BoundingBoxXYZ boundingBox, MoveDirection direction, double offset )
        {
            
            XYZ offsetVector = new XYZ();

            if(direction == MoveDirection.Right)
            {
                offsetVector = new XYZ(offset,0,0);
            }
            else if(direction == MoveDirection.Down)
            {
                offsetVector = new XYZ(0, -offset, 0);
            }
            else
            {
                offsetVector = new XYZ(0, 0, 0);
            }

            // compute the new bounding box
            var newBoundingBox = new BoundingBoxXYZ();
            newBoundingBox.Min = boundingBox.Min + offsetVector;
            newBoundingBox.Max = boundingBox.Max + offsetVector;

            return newBoundingBox;
        }
    
        /// <summary>
        /// Get the no of intersections for a tag specified by the index in the list of tags
        /// </summary>
        /// <param name="index">index for which intersections are required</param>
        /// <param name="tagList">list of tags</param>
        /// <returns>no of intersections</returns>
        private int GetNoOfIntersections(int index,ref List<Tag> tagList)
        {
            if (index < 0 || index >= tagList.Count)
                return 0;

            var tag = tagList[index];

            int intersectionCount = 0;

            for ( int i = 0; i < tagList.Count; i++)
            {
                // do not check the intersection for the same tag
                if (i == index)
                    continue;

                //check for the intersection and increment the counter
                if(TagUtils.AreBoundingBoxesIntersecting(tag.newBoundingBox, tagList[i].newBoundingBox))
                    intersectionCount++;
            }

            return intersectionCount;
        }
    }
}

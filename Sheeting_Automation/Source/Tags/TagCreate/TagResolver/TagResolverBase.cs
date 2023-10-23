using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sheeting_Automation.Source.Tags.TagData;

namespace Sheeting_Automation.Source.Tags.TagCreate.TagResolver
{
    public class TagResolverBase
    {
        /// <summary>
        /// enum for all the possible move directions 
        /// </summary>
        protected enum MoveDirection
        {
            Up,
            Down,
            Left,
            Right,
            UpLeft,
            DownLeft,
            UpRight,
            DownRight
        }

        /// <summary>
        /// Struct for storing the tag movement data 
        /// Usually stored as List of MoveData
        /// this list is used in computing the best bounding boxes for each alogirthm
        /// </summary>
        protected struct MoveData
        {
            public double distance;
            public BoundingBoxXYZ computedBoundingBox;
            public Tag mTag;

            public MoveData(double distance, BoundingBoxXYZ computedBoundingBox, Tag mTag)
            {
                this.distance = distance;
                this.computedBoundingBox = computedBoundingBox;
                this.mTag = mTag;
            }
        }

        protected class DistanceComparer : IComparer<MoveData>
        {
            public int Compare(MoveData x, MoveData y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }

        protected List<MoveData> moveDataList;

        /// <summary>
        /// Each derived class should have its own implementation
        /// on how the overlaps are resolved
        /// </summary>
        /// <param name="tagList"></param>
        /// <returns>list of tags </returns>
        protected virtual List<Tag> ResolveTagList(List<Tag> tagList, ref List<List<Tag>> overlapTagsList)
        {
            return tagList;
        }

        /// <summary>
        /// calls ResolveTagList on each of the overlapping list 
        /// </summary>
        /// <param name="overlapTagsList"></param>
        public void Resolve(ref List<List<Tag>> overlapTagsList)
        {
            // intialize the move data list
            moveDataList = new List<MoveData>();

            for (int i = 0; i < overlapTagsList.Count; i++)
            {
                if (overlapTagsList[i].Count > 1)
                {
                    overlapTagsList[i] = ResolveTagList(overlapTagsList[i],ref overlapTagsList);
                }
            }
        }
    }
}

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sheeting_Automation.Source.Tags.TagData;

namespace Sheeting_Automation.Source.Tags
{
    public static class Tag2ElementMovement
    {
        private enum MoveDirection
        {
            Up,
            Down,
            Left,
            Right,
            UpLeft,
            UpRight,
            DownLeft,
            DownRight
        };

        private class MoveData
        {
            public BoundingBoxXYZ computedBoundingBox;
            public int moveOffset;

            public MoveData()
            {
                computedBoundingBox = null;
                moveOffset = 0;
            }
        };

        private class OffsetDistanceComparer: IComparer<MoveData>
        {
            public int Compare(MoveData x, MoveData y)
            {
                return x.moveOffset.CompareTo(y.moveOffset);
            }
        }

        public static void MoveTag(ref Tag tag)
        {
            List<MoveData> moveDataList = new List<MoveData>
            {
                Move(tag, MoveDirection.Up),
                Move(tag, MoveDirection.Down),
                Move(tag, MoveDirection.Left),
                Move(tag, MoveDirection.Right),
                Move(tag, MoveDirection.UpLeft),
                Move(tag, MoveDirection.UpRight),
                Move(tag, MoveDirection.DownLeft),
                Move(tag, MoveDirection.DownRight)
            };

            moveDataList.Sort(new OffsetDistanceComparer());

            tag.newBoundingBox = moveDataList[0].computedBoundingBox;

            foreach(var moveData in moveDataList)
            {
                tag.bestBoundingBoxes.Add(moveData.computedBoundingBox);
            }

        }

        private static MoveData Move(Tag tag, MoveDirection direction)
        {
            XYZ moveOffset = XYZ.Zero;

            MoveData moveData = new MoveData();
            moveData.computedBoundingBox = new BoundingBoxXYZ();
            moveData.computedBoundingBox.Min = tag.currentBoundingBox.Min;
            moveData.computedBoundingBox.Max = tag.currentBoundingBox.Max;

            if (direction == MoveDirection.Up)
                moveOffset = new XYZ(0, 0.3, 0);
            else if (direction == MoveDirection.Down)
                moveOffset = new XYZ(0, -0.3, 0);
            else if (direction == MoveDirection.Left)
                moveOffset = new XYZ(-0.3, 0, 0);
            else if (direction == MoveDirection.Right)
                moveOffset = new XYZ(0.3, 0, 0);
            else if (direction == MoveDirection.UpLeft)
                moveOffset = new XYZ(-0.3, 0.3, 0);
            else if (direction == MoveDirection.UpRight)
                moveOffset = new XYZ(0.3, 0.3, 0);
            else if (direction == MoveDirection.DownLeft)
                moveOffset = new XYZ(-0.3, -0.3, 0);
            else if (direction == MoveDirection.DownRight)
                moveOffset = new XYZ(0.3, -0.3, 0);

            while (moveData.moveOffset < 10) 
            {
                if(TagUtils.AreBoundingBoxesIntersecting(moveData.computedBoundingBox, tag.nearestElementBoundingBoxes))
                {
                    var oldBoundingBox = moveData.computedBoundingBox;
                    oldBoundingBox.Min = moveData.computedBoundingBox.Min + moveOffset;
                    oldBoundingBox.Max = moveData.computedBoundingBox.Max + moveOffset; 
                    moveData.computedBoundingBox = oldBoundingBox;
                }
                else
                {
                    //break if no overlap is detected
                    break;
                }

                moveData.moveOffset++;
            }

            if (moveData.moveOffset >= 10)
            {
                // revert to the original location 
                moveData.computedBoundingBox = new BoundingBoxXYZ();
                moveData.computedBoundingBox.Min = tag.currentBoundingBox.Min;
                moveData.computedBoundingBox.Max = tag.currentBoundingBox.Max;
            }

            return moveData;
        }
    }
}

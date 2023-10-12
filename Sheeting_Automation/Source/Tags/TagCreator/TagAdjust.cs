using Autodesk.Revit.DB;
using Sheeting_Automation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sheeting_Automation.Source.Tags.TagData;

namespace Sheeting_Automation.Source.Tags
{
    public static class TagAdjust
    {
        /// <summary>
        /// Adjust the tags considering only the elements 
        /// </summary>
        public static void AdjustTagsBasedOnElementsOnly()
        {
            // new bounding boxes will be udpated in this loop
            for(int i = 0; i < BoundingBoxCollector.IndependentTags.Count; i++)
            {
                BoundingBoxCollector.IndependentTags[i] = UpdateTagtoCenterOfElement(BoundingBoxCollector.IndependentTags[i]);
                BoundingBoxCollector.IndependentTags[i] = AdjustTagBasedOnElements(BoundingBoxCollector.IndependentTags[i]);
            }

            // updaate the tag location 
            UpdateTagLocation();
        }

        /// <summary>
        /// Adjust the tag based on the element bounding boxes
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>custom tag struct</returns>
        private static Tag AdjustTagBasedOnElements(Tag tag)
        {
            var elemBoundingBoxes = TagUtils.GetNearestElementBoundingBoxes(tag, ref BoundingBoxCollector.BoundingBoxesDict);

            TagMovement.MoveTag(ref tag,ref elemBoundingBoxes);

            return tag;
        }

        private static Tag UpdateTagtoCenterOfElement(Tag tag)
        {
            var elemBoundingBox = BoundingBoxCollector.BoundingBoxesDict[tag.mElement.Id].FirstOrDefault();

            var elemMidPoint = (elemBoundingBox.Min + elemBoundingBox.Max) / 2;

            var tagMidpoint = (tag.currentBoundingBox.Min + tag.currentBoundingBox.Max) / 2;

            var differenceVector = elemMidPoint - tagMidpoint;

            tag.centerVectorDifference = differenceVector;
            
            var tempCurrentBoudingBox = new BoundingBoxXYZ();

            tempCurrentBoudingBox.Min = tag.currentBoundingBox.Min + differenceVector;

            tempCurrentBoudingBox.Max = tag.currentBoundingBox.Max + differenceVector;

            tag.currentBoundingBox = new BoundingBoxXYZ();
            tag.currentBoundingBox.Min = tempCurrentBoudingBox.Min;
            tag.currentBoundingBox.Max = tempCurrentBoudingBox.Max;

            tag.newBoundingBox = new BoundingBoxXYZ();
            tag.newBoundingBox.Min = tempCurrentBoudingBox.Min;
            tag.newBoundingBox.Max = tempCurrentBoudingBox.Max;

            return tag;
        }

        /// <summary>
        /// Transaction to move the tag from current bounding box to the new bounding box
        /// </summary>
        private static void UpdateTagLocation()
        {
            // start the transaction to udpate the tags to the new bounding boxes
            using (Transaction tx = new Transaction(SheetUtils.m_Document))
            {
                // start the transaction 
                tx.Start("Moving Tags");

                // tags will be move to the new bounding boxes
                foreach (var tag in BoundingBoxCollector.IndependentTags)
                {
                    // calcuate the translation vector based on new and current bouding boxes 
                    XYZ translation = tag.newBoundingBox.Min - tag.currentBoundingBox.Min;

                    // Move the element based on the obtained translation vector 
                    ElementTransformUtils.MoveElement(SheetUtils.m_Document, tag.mTag.Id, translation + tag.centerVectorDifference);
                }

                //commit the transaction 
                tx.Commit();
            }
        }
    }
}

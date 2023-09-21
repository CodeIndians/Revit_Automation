using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Sheeting_Automation.Utils;
using System;
using System.Collections.Generic;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    public class Tag2DimensionOverlap : TagOverlapBase
    {
        private Dictionary<ElementId,Dimension> dimensionList;

        /// <summary>
        /// Get all the dimension ids in the current view
        /// </summary>
        /// <returns></returns>
        protected override List<ElementId> GetElementIds()
        {
            dimensionList = new Dictionary<ElementId,Dimension>();

            List<ElementId> elementIds = new List<ElementId>();

            // Create a filtered element collector 
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id);

            // Filter for elements of category windows
            collector.OfCategory(BuiltInCategory.OST_Dimensions);


            foreach (Element element in collector)
            {
                //TODO: Check if the dimensions bounding boxes are proper

                elementIds.Add(element.Id);

                dimensionList[element.Id] =  element as Dimension;
            }

            return elementIds;
        }

        /// <summary>
        /// Retrieve the bounding box of the element represented by its id on the active view 
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        protected List<BoundingBoxXYZ> GetBoundingBoxOfDimensionSegments(ElementId elementId)
        {
            Dimension dim = dimensionList[elementId];

            List<BoundingBoxXYZ> boundingBoxes = new List<BoundingBoxXYZ>();

            if(dim != null)
            {
                DimensionSegmentArray segments = dim.Segments;

                foreach (DimensionSegment segment in segments)
                {
                    BoundingBoxXYZ boundingBox = GetBoundingBoxOfSegment(segment, elementId) ;

                    boundingBoxes.Add(boundingBox);
                }
            }

            return boundingBoxes;
        }

        private BoundingBoxXYZ GetBoundingBoxOfSegment(DimensionSegment segment, ElementId elementId)
        {
            XYZ textPoint = segment.TextPosition;

            Dimension dim = dimensionList[elementId];

            var completeBoundingBox = dimensionList[elementId]?.get_BoundingBox(SheetUtils.m_Document.ActiveView);

            // Get the dimensions of the bounding box
            double width = Math.Abs(completeBoundingBox.Max.X - completeBoundingBox.Min.X);
            double height = Math.Abs(completeBoundingBox.Max.Y - completeBoundingBox.Min.Y);

            BoundingBoxXYZ newBoundingBox = new BoundingBoxXYZ();

            XYZ newMinPoint;
            XYZ newMaxPoint;

            // horizontal bounding box condition
            if (width > height)
            {
                // TODO: Based on the no of characters in the text 
                // get the hardcoded text box approximate bounding box
                //newMinPoint = new XYZ(textPoint.X - 1.5, completeBoundingBox.Min.Y, textPoint.Z);
                //newMaxPoint = new XYZ(textPoint.X + 1.5, completeBoundingBox.Max.Y, textPoint.Z);
                newMinPoint = new XYZ(textPoint.X - 1.5, textPoint.Y, textPoint.Z);
                newMaxPoint = new XYZ(textPoint.X + 1.5, textPoint.Y + 1.3, textPoint.Z);
            }
            else // vertical bounding box condition 
            {
                // TODO: Based on the no of characters in the text 
                // get the hardcoded text box approximate bounding box
                //newMinPoint = new XYZ(completeBoundingBox.Min.X, textPoint.Y - 1.5, textPoint.Z);
                //newMaxPoint = new XYZ(completeBoundingBox.Max.X, textPoint.Y + 1.5, textPoint.Z);
                newMinPoint = new XYZ(textPoint.X - 1.4, textPoint.Y - 1.5, textPoint.Z);
                newMaxPoint = new XYZ(textPoint.X , textPoint.Y + 1.5, textPoint.Z);
            }

            // create the bounding box with new min and max points 
            newBoundingBox.Min = newMinPoint;
            newBoundingBox.Max = newMaxPoint;

            return newBoundingBox;

        }

        public override List<ElementId> CheckOverlap()
        {
            var overlapElementIds = new List<ElementId>();

            // get the wall element ids
            var elementIds = GetElementIds();


            for (int i = 0; i < elementIds.Count; i++)
            {
                for (int j = 0; j < m_IndependentTags.Count; j++)
                {
                    foreach (BoundingBoxXYZ boundingBoxXYZ in GetBoundingBoxOfDimensionSegments(elementIds[i]))
                    {
                        if (TagUtils.AreBoudingBoxesIntersecting(boundingBoxXYZ,
                                                       m_IndependentTags[j].get_BoundingBox(SheetUtils.m_Document.ActiveView)))
                        {
                            if (!overlapElementIds.Contains(m_IndependentTags[j].Id))
                            {
                                overlapElementIds.Add(m_IndependentTags[j].Id);
                                overlapElementIds.AddRange(m_IndependentTags[j].GetTaggedLocalElementIds());
                            }

                            if (!overlapElementIds.Contains(elementIds[i]))
                            {
                                overlapElementIds.Add(elementIds[i]);
                            }
                        }
                    }
                }
            }

            return overlapElementIds;
        }

    }
}

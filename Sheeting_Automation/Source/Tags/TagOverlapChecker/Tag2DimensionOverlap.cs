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

                if(segments.Size == 0)
                {
                    // dimension has no segment, check the dimension directly
                    BoundingBoxXYZ boundingBox = GetBoundingBoxOfDimension(dim, elementId);

                    boundingBoxes.Add(boundingBox);
                }
            }

            return boundingBoxes;
        }

        private BoundingBoxXYZ GetBoundingBoxOfSegment(DimensionSegment segment, ElementId elementId)
        {
            XYZ textPoint = segment.TextPosition;

            int textLength = segment.ValueString.Length;

            return GetBoundingBox(textPoint, textLength, elementId);

        }


        private BoundingBoxXYZ GetBoundingBoxOfDimension(Dimension dimension, ElementId elementId)
        {
            XYZ textPoint = dimension.TextPosition;

            int textLength = dimension.ValueString.Length;

            return GetBoundingBox(textPoint,textLength,elementId);

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

        private BoundingBoxXYZ GetBoundingBox(XYZ textPoint, int textLength, ElementId elementId)
        {
            double offset = 1.0f; ; // this offset is by default
            double fixedHeightOffset = 1.35f;

            if (textLength <= 5)
                offset = 1.0;
            else if (textLength == 6)
                offset = 1.2;
            else if (textLength == 7)
                offset = 1.2;
            else if (textLength == 8)
                offset = 1.2f;
            else if (textLength == 9)
                offset = 1.4f;
            else if (textLength == 10)
                offset = 1.6f;
            else if (textLength == 11)
                offset = 1.8f;
            else if (textLength == 12)
                offset = 2.0f;
            else if (textLength == 13)
                offset = 2.2;
            else if (textLength >= 14)
                offset = 2.5;

            Dimension dim = dimensionList[elementId];

            var completeBoundingBox = dimensionList[elementId]?.get_BoundingBox(SheetUtils.m_Document.ActiveView);

            if (completeBoundingBox == null)
                return null;

            // Get the dimensions of the bounding box
            double width = Math.Abs(completeBoundingBox.Max.X - completeBoundingBox.Min.X);
            double height = Math.Abs(completeBoundingBox.Max.Y - completeBoundingBox.Min.Y);

            BoundingBoxXYZ newBoundingBox = new BoundingBoxXYZ();

            XYZ newMinPoint;
            XYZ newMaxPoint;

            // horizontal bounding box condition
            if (width > height)
            {
                newMinPoint = new XYZ(textPoint.X - offset, textPoint.Y, textPoint.Z);
                newMaxPoint = new XYZ(textPoint.X + offset, textPoint.Y + fixedHeightOffset, textPoint.Z);
            }
            else // vertical bounding box condition 
            {
                newMinPoint = new XYZ(textPoint.X - fixedHeightOffset, textPoint.Y - offset, textPoint.Z);
                newMaxPoint = new XYZ(textPoint.X, textPoint.Y + offset, textPoint.Z);
            }

            // create the bounding box with new min and max points 
            newBoundingBox.Min = newMinPoint;
            newBoundingBox.Max = newMaxPoint;

            return newBoundingBox;
        }

    }
}

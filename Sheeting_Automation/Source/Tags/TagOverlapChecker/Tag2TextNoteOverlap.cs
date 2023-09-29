using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Sheeting_Automation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    public class Tag2TextNoteOverlap : TagOverlapBase
    {
        /// <summary>
        /// Get all the text note element ids in the current view
        /// </summary>
        /// <returns></returns>
        protected override List<ElementId> GetElementIds()
        {
            List<ElementId> elementIds = new List<ElementId>();

            // Create a filtered element collector
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id);

            // Filter for elements of category text notes
            collector.OfCategory(BuiltInCategory.OST_TextNotes);

            foreach (Element element in collector)
            {
                elementIds.Add(element.Id);
            }

            return elementIds;
        }

        public override List<BoundingBoxXYZ> GetBoundingBoxesOfElement(ElementId elementId)
        {
            TextNote textNote = SheetUtils.m_Document.GetElement(elementId) as TextNote;

            List<BoundingBoxXYZ> textNoteBoundingBoxList = new List<BoundingBoxXYZ>();

            if (textNote != null)
            {
                // alignment = right, center, left
                var horAlignment = textNote.HorizontalAlignment;

                // top alignment point
                var textCordinate = textNote.Coord;

                // can be up or left point vector 
                var upDirection = textNote.UpDirection;

                // scale of the current view, used in the width and height calculation
                var scale = SheetUtils.m_Document.ActiveView.Scale;

                // height and width of the text note
                // TODO: Add offset if required
                var width = textNote.Width * scale;
                var height = textNote.Height * scale;

                // initialize max and min points
                XYZ minPoint = null;
                XYZ maxPoint = null;

                // up direction is to the north 
                if(upDirection.IsAlmostEqualTo(new XYZ(0,1,0)))
                {
                    if(horAlignment == HorizontalTextAlignment.Left)
                    {
                        minPoint = new XYZ(textCordinate.X, textCordinate.Y - height, textCordinate.Z);
                        maxPoint = new XYZ(textCordinate.X + width, textCordinate.Y, textCordinate.Z);
                    }
                    else if (horAlignment == HorizontalTextAlignment.Right)
                    {
                        minPoint = new XYZ(textCordinate.X - width, textCordinate.Y - height, textCordinate.Z);
                        maxPoint = textCordinate;
                    }
                    else if(horAlignment == HorizontalTextAlignment.Center)
                    {
                        minPoint = new XYZ(textCordinate.X - (width/2), textCordinate.Y - height, textCordinate.Z);
                        maxPoint = new XYZ(textCordinate.X + (width/2), textCordinate.Y, textCordinate.Z);
                    }
                }
                else if (upDirection.IsAlmostEqualTo(new XYZ(-1,0,0)))  // up direction is to the west 
                {
                    if (horAlignment == HorizontalTextAlignment.Left)
                    {
                        minPoint = textCordinate;
                        maxPoint = new XYZ(textCordinate.X + height, textCordinate.Y + width, textCordinate.Z);

                    }
                    else if (horAlignment == HorizontalTextAlignment.Right)
                    {
                        minPoint = new XYZ(textCordinate.X, textCordinate.Y - width, textCordinate.Z);
                        maxPoint = new XYZ(textCordinate.X + height, textCordinate.Y, textCordinate.Z);

                    }
                    else if (horAlignment == HorizontalTextAlignment.Center)
                    {
                        minPoint = new XYZ(textCordinate.X , textCordinate.Y - (width/2), textCordinate.Z);
                        maxPoint = new XYZ(textCordinate.X + height, textCordinate.Y + (width/2), textCordinate.Z);
                    }
                }
                else if (upDirection.IsAlmostEqualTo(new XYZ(1, 0, 0)))  // up direction is to the east 
                {
                    if (horAlignment == HorizontalTextAlignment.Left)
                    {
                        minPoint = new XYZ(textCordinate.X - height, textCordinate.Y - width, textCordinate.Z);
                        maxPoint = textCordinate;

                    }
                    else if (horAlignment == HorizontalTextAlignment.Right)
                    {
                        minPoint = new XYZ(textCordinate.X - height, textCordinate.Y, textCordinate.Z);
                        maxPoint = new XYZ(textCordinate.X, textCordinate.Y + width, textCordinate.Z);

                    }
                    else if (horAlignment == HorizontalTextAlignment.Center)
                    {
                        minPoint = new XYZ(textCordinate.X - height, textCordinate.Y - (width/2), textCordinate.Z);
                        maxPoint = new XYZ(textCordinate.X, textCordinate.Y + (width/2), textCordinate.Z);
                    }
                }
                else if (upDirection.IsAlmostEqualTo(new XYZ(0, -1, 0)))  // up direction is to the south 
                {
                    if (horAlignment == HorizontalTextAlignment.Left)
                    {
                        minPoint = new XYZ(textCordinate.X - width, textCordinate.Y , textCordinate.Z);
                        maxPoint = new XYZ(textCordinate.X , textCordinate.Y + height, textCordinate.Z);

                    }
                    else if (horAlignment == HorizontalTextAlignment.Right)
                    {
                        minPoint = textCordinate;
                        maxPoint = new XYZ(textCordinate.X + width, textCordinate.Y + height, textCordinate.Z);

                    }
                    else if (horAlignment == HorizontalTextAlignment.Center)
                    {
                        minPoint = new XYZ(textCordinate.X - (width/2), textCordinate.Y, textCordinate.Z);
                        maxPoint = new XYZ(textCordinate.X + (width/2), textCordinate.Y + height, textCordinate.Z);
                    }
                }

                BoundingBoxXYZ textNoteBoundingBox = new BoundingBoxXYZ();
                textNoteBoundingBox.Min = minPoint;
                textNoteBoundingBox.Max = maxPoint;

                textNoteBoundingBoxList.Add(textNoteBoundingBox);

                if(textNote.LeaderCount > 0)
                {
                    var leadersList = textNote.GetLeaders();

                    foreach (var leader in leadersList)
                    {
                        textNoteBoundingBoxList.AddRange(TagUtils.GetBoundingBoxes(leader));
                    }
                }

                return textNoteBoundingBoxList;

            }

            // return default bounding box if it is not a text note 
            return new List<BoundingBoxXYZ> { SheetUtils.m_Document.GetElement(elementId)?.get_BoundingBox(SheetUtils.m_Document.ActiveView) };
        }

    }
}

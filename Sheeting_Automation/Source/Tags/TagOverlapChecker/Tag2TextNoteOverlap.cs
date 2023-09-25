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

        protected override BoundingBoxXYZ GetBoundingBoxOfElement(ElementId elementId)
        {
            TextNote textNote = SheetUtils.m_Document.GetElement(elementId) as TextNote;

            TextNote tempNote = null;

            BoundingBoxXYZ textNoteBoundingBox = new BoundingBoxXYZ();

            if (textNote != null)
            {
                // transaction to create a text note with out leader from the same text note
                using (Transaction transaction = new Transaction(SheetUtils.m_Document, "Get TextNote Bounding Box"))
                {
                    transaction.Start();


                    var textNoteType = textNote.TextNoteType;

                    TextNoteOptions options = new TextNoteOptions();

                    tempNote = TextNote.Create(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id, textNote.Coord, textNote.Text,textNoteType.Id);

                    transaction.Commit();
                }

                // capture the bounding box of the text note
                textNoteBoundingBox = tempNote.get_BoundingBox(SheetUtils.m_Document.ActiveView);

                // transaction to delete the created text note
                using (Transaction transaction = new Transaction(SheetUtils.m_Document, "Dispose the note"))
                {
                    transaction.Start();

                    SheetUtils.m_Document.Delete(tempNote.Id);

                    transaction.Commit();
                }

                return textNoteBoundingBox;
            }

            // return default bounding box
            return SheetUtils.m_Document.GetElement(elementId)?.get_BoundingBox(SheetUtils.m_Document.ActiveView);

        }
    }
}

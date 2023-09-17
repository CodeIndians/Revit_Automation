using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Sheeting_Automation.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    internal class TagOverlapManager
    {
        // Element ids that are overlapping
        // These should be higlighted
        private List<ElementId> m_ElementIds;

        // List of overlap chckers 
        private List<TagOverlapBase> m_TagOverlapCheckers;

        //constructor
        public TagOverlapManager()
        {
            //intialize the list
            m_ElementIds = new List<ElementId>();

            m_TagOverlapCheckers = new List<TagOverlapBase>();

            InitializeOverlapCheckers();

            ProcessOverlapCheckers();
        }

        private void InitializeOverlapCheckers()
        {
            // clear if anything is already present 
            m_TagOverlapCheckers.Clear();

            // tag to tag overlap checker
            m_TagOverlapCheckers.Add(new Tag2TagOverlap());

        }

        private void ProcessOverlapCheckers()
        {
            foreach(var checker in  m_TagOverlapCheckers)
            {
                m_ElementIds.AddRange(checker.CheckOverlap());
            }
        }

        public void HighlightTags()
        {
            // Create a list to hold Reference objects
            List<Reference> references = new List<Reference>();

            foreach (ElementId elementId in m_ElementIds)
            {
                // Create a Reference object for each Element ID
                Reference reference = new Reference(SheetUtils.m_Document.GetElement(elementId));
                references.Add(reference);
            }

            // clear the previous selection
            SheetUtils.m_Selection.SetElementIds(new List<ElementId>());

            // required selection as list
            var list = references.Select(r => r.ElementId).ToList();

            // Add the new references to the selection
            SheetUtils.m_Selection.SetElementIds(list);

            // Refresh the document to display the selection
            SheetUtils.m_UIDocument.RefreshActiveView();

            if(m_ElementIds.Count > 0)
            {
                TaskDialog.Show("Info", $"{m_ElementIds.Count / 2} tags are overlapping");
            }
            else
            {
                TaskDialog.Show("Info", "No tags are overlapping");
            }
        }

    }
}

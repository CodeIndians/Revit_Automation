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
        public static  List<ElementId> m_ElementIds;

        // List of overlap chckers 
        private List<TagOverlapBase> m_TagOverlapCheckers;

        //constructor
        public TagOverlapManager()
        {
            if ( m_ElementIds != null && m_ElementIds.Count > 0 )
            {
                // delete the overrides
                TagGraphicOverrider.DeleteOverrides( m_ElementIds );
            }
            //intialize the list
            m_ElementIds = new List<ElementId>();

            m_TagOverlapCheckers = new List<TagOverlapBase>();

            InitializeOverlapCheckers();

            ProcessOverlapCheckers();
        }

        /// <summary>
        /// add the overlap checkers
        /// any new overlap checking functionality should go here
        /// </summary>
        private void InitializeOverlapCheckers()
        {
            TagOverlapBase.InitializeTags();

            // clear if anything is already present 
            m_TagOverlapCheckers.Clear();

            // tag to tag overlap checker
            m_TagOverlapCheckers.Add(new Tag2TagOverlap());

            //tag to wall overlap checker
            m_TagOverlapCheckers.Add(new Tag2WallOverlap());

            //tag to structural column overlap checker
            m_TagOverlapCheckers.Add(new Tag2StructColOverlap());

            //tag to window overlap checker
            m_TagOverlapCheckers.Add(new Tag2WindowOverlap());

            //tag to generic model overlap checker
            m_TagOverlapCheckers.Add(new Tag2GenModelOverlap());

            //tag to door overlap checker
            m_TagOverlapCheckers.Add(new Tag2DoorOverlap());

            //tag to text note overlap checker
            m_TagOverlapCheckers.Add(new Tag2TextNoteOverlap());

            //tag to dimension overlap checker
            m_TagOverlapCheckers.Add(new Tag2DimensionOverlap());

            //tag to detail item overlap checker
            m_TagOverlapCheckers.Add(new Tag2DetailOverlap());

            //tag to section view overlap checker
            m_TagOverlapCheckers.Add(new Tag2ViewOverlap());

            //tag to structural framing overlap checker
            m_TagOverlapCheckers.Add(new Tag2StructuralOverlap());
        }

        /// <summary>
        /// Collects all the over lapping elements from all the 
        /// overlapping checkers that are added via overlap manager
        /// </summary>
        private void ProcessOverlapCheckers()
        {
            foreach(var checker in  m_TagOverlapCheckers)
            {
                m_ElementIds.AddRange(checker.CheckOverlap());
            }
        }

        /// <summary>
        /// Highlights the collected element ids from the 
        /// overlap checkers
        /// </summary>
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

            TagGraphicOverrider.CreateOverrides(m_ElementIds);

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
                TaskDialog.Show("Info", $"{SheetUtils.m_Selection.GetElementIds().Count / 2 } overlap(s) detected.");
            }
            else
            {
                TaskDialog.Show("Info", "No tags are overlapping");
            }
        }

        public void CleanupTempTags()
        {
            TagOverlapBase.DeleteNoLeaderTags();
        }

    }
}

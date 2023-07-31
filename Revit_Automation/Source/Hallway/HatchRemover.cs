using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal class HatchRemover
    {
        private readonly Document mDocument;
        public HatchRemover(ref Document doc) 
        {
            mDocument = doc;

            RemoveHatches();
        }

        // Remove the external and internal hatches
        private void RemoveHatches()
        {
            FilteredElementCollector collector = new FilteredElementCollector(mDocument, mDocument.ActiveView.Id);
            ICollection<Element> filledRegionElements = collector.OfClass(typeof(FilledRegion)).ToElements();

            List<ElementId> elementIds = new List<ElementId>();

            // Process the collected filled region elements
            foreach (Element filledRegionElement in filledRegionElements)
            {
                var elementId = filledRegionElement.Id;
                FilledRegion filledRegion = filledRegionElement as FilledRegion;
                if (filledRegion != null)
                {
                    var name = filledRegion.Name;
                    elementIds.Add(elementId);
                }
            }

            // Delete all the element Ids
            using (Transaction transaction = new Transaction(mDocument, "Delete placeholder hatches"))
            {
                transaction.Start();
                foreach (var element in elementIds)
                    mDocument.Delete(element);

                transaction.Commit();
            }
        }
    }
}

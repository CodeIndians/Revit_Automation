using Autodesk.Revit.DB;
using Sheeting_Automation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    public class Tag2DoorOverlap : TagOverlapBase
    {
        /// <summary>
        /// Get all the door element ids in the current view
        /// </summary>
        /// <returns></returns>
        protected override List<ElementId> GetElementIds()
        {
            List<ElementId> elementIds = new List<ElementId>();

            // Create a filtered element collector 
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id);

            // Filter for elements of category doors
            collector.OfCategory(BuiltInCategory.OST_Doors);

            foreach (Element element in collector)
            {
                //TODO: Skip roll up doors for tag overlap

                elementIds.Add(element.Id);
            }

            return elementIds;
        }
    }
}

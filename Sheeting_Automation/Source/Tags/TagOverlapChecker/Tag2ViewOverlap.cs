using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure.StructuralSections;
using Sheeting_Automation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    public class Tag2ViewOverlap : TagOverlapBase
    {
        /// <summary>
        /// Get all the view ids in the current view
        /// </summary>
        /// <returns></returns>
        protected override List<ElementId> GetElementIds()
        {
            List<ElementId> elementIds = new List<ElementId>();

            // Create a filtered element collector
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_ActiveViewId);


            foreach (Element element in collector)
            {
                if (element.Category?.Name == "Views")
                {
                    elementIds.Add(element.Id);
                }
            }

            return elementIds;
        }
    }

}

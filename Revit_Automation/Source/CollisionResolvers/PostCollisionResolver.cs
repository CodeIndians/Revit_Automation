using Autodesk.Revit.DB;
using Revit_Automation.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Revit_Automation.Source.Utils.WarningSwallowers;

namespace Revit_Automation.Source.CollisionDetectors
{
    public class PostCollisionResolver : ICollisionInterface
    {
        public Document m_Document;
        public PostCollisionResolver(Document doc) { m_Document = doc; }

        public void HandleCollision(XYZ pt)
        {
            BoundingBoxContainsPointFilter filter = new BoundingBoxContainsPointFilter(pt);

            // Apply the filter to the elements in the active document
            // This filter will excludes all objects derived from View and objects derived from ElementType
            FilteredElementCollector collector = new FilteredElementCollector(m_Document);
            IList<Element> elements = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_StructuralColumns).ToElements();

            Debug.Write(string.Format("Total posts at a give point {0}, {1}", pt, elements.Count()));
        }

        public void PlaceObjectInClearSpace()
        {
            
        }
    }
}

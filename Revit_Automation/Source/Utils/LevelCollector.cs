using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Utils
{
    public class LevelCollector
    {
        public static IOrderedEnumerable<Level> levels = null;

        public static void FindAndSortLevels(Document doc)
        {
            levels = new FilteredElementCollector(doc)
                            .WherePasses(new ElementClassFilter(typeof(Level), false))
                            .Cast<Level>()
                            .OrderBy(e => e.Elevation);

        }
    }
}

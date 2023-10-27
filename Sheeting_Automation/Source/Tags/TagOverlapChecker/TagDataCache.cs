using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    static class TagDataCache
    {
        // dictionary to store the bounding boxes of the elements
        public static Dictionary<ElementId, List<BoundingBoxXYZ>> cachedBoundingBoxDict = new Dictionary<ElementId, List<BoundingBoxXYZ>>();

        /// <summary>
        /// Initialize the cached bouding box
        /// </summary>
        public static void Initialize()
        {
            // initialize the cached bounding box
            cachedBoundingBoxDict = new Dictionary<ElementId, List<BoundingBoxXYZ>> ();
        }

    }
}

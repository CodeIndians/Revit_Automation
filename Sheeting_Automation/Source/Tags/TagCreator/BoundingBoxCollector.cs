using Autodesk.Revit.DB;
using Sheeting_Automation.Source.Tags.TagOverlapChecker;
using System.Collections.Generic;

namespace Sheeting_Automation.Source.Tags
{
    public static class BoundingBoxCollector
    {
        public static Dictionary<ElementId, List<BoundingBoxXYZ>> BoundingBoxesDict;

        private static List<TagOverlapBase> overlapCheckers;
        
        // collect the bounding boxes
        public static void Initialize()
        {
            BoundingBoxesDict  = new Dictionary<ElementId, List<BoundingBoxXYZ>>();

            overlapCheckers = new List<TagOverlapBase>();

            // add the overlap checkers
            AddOverlapCheckers();

            // collect the bounding boxes
            Collect();
        }

        /// <summary>
        /// Add the overlap checkers for collecting the bouding boxes 
        /// </summary>
        private static void AddOverlapCheckers()
        {
            overlapCheckers.Clear();

            //tag to wall overlap checker
            overlapCheckers.Add(new Tag2WallOverlap());

            //tag to structural column overlap checker
            overlapCheckers.Add(new Tag2StructColOverlap());

            //tag to window overlap checker
            overlapCheckers.Add(new Tag2WindowOverlap());

            //tag to generic model overlap checker
            overlapCheckers.Add(new Tag2GenModelOverlap());

            //tag to door overlap checker
            overlapCheckers.Add(new Tag2DoorOverlap());

            //tag to text note overlap checker
            overlapCheckers.Add(new Tag2TextNoteOverlap());

            //tag to dimension overlap checker
            overlapCheckers.Add(new Tag2DimensionOverlap());

            //tag to detail item overlap checker
            overlapCheckers.Add(new Tag2DetailOverlap());

            //tag to section view overlap checker
            overlapCheckers.Add(new Tag2ViewOverlap());

            //tag to structural framing overlap checker
            overlapCheckers.Add(new Tag2StructuralOverlap());
        }
    
        /// <summary>
        /// Collect the bounding boxes from the overlap checkers 
        /// </summary>
        private static void Collect()
        {
            foreach(var checker in overlapCheckers)
            {
               foreach(var kvp in checker.GetAllBoundingBoxes())
                {
                    BoundingBoxesDict.Add(kvp.Key, kvp.Value);
                }
            }
        }
    
    }
}

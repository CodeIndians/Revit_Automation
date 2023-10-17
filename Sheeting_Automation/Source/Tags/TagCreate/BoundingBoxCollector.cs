using Autodesk.Revit.DB;
using Sheeting_Automation.Source.Tags.TagOverlapChecker;
using Sheeting_Automation.Utils;
using System.Collections.Generic;
using System.Linq;
using static Sheeting_Automation.Source.Tags.TagData;

namespace Sheeting_Automation.Source.Tags
{
    public static class BoundingBoxCollector
    {   
        /// <summary>
        /// list to collect the bouding boxes 
        /// </summary>
        public static Dictionary<ElementId, List<BoundingBoxXYZ>> BoundingBoxesDict;

        /// <summary>
        /// list of all the independent tags, this is our custom tag class
        /// </summary>
        public static List<Tag> IndependentTags;

        /// <summary>
        /// list of overlap checkers that are used 
        /// </summary>m
        private static List<TagOverlapBase> overlapCheckers;
        
        /// <summary>
        /// collect the bounding boxes
        /// this is called when loading the form on the worker thread
        /// </summary>
        public static void Initialize()
        {
            BoundingBoxesDict  = new Dictionary<ElementId, List<BoundingBoxXYZ>>();

            overlapCheckers = new List<TagOverlapBase>();

            IndependentTags = new List<Tag>();

            // add the overlap checkers
            AddOverlapCheckers();

            // collect the bounding boxes
            Collect();
        }

        /// <summary>
        /// Add the overlap checkers for collecting the bounding boxes 
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

        /// <summary>
        /// Update the bounding boxes of all the tags
        /// </summary>
        public static void UpdateTagBoundingBoxes()
        {
            for(int i = 0; i < IndependentTags.Count; i++)
            {
                var tag = IndependentTags[i];
                tag.currentBoundingBox = tag.mTag.get_BoundingBox(SheetUtils.m_ActiveView);
                tag.newBoundingBox = tag.mTag.get_BoundingBox(SheetUtils.m_ActiveView);
                IndependentTags[i] = tag;
            }
        }

    }
}

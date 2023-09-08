using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace Sheeting_Automation.Source.Tags
{
    public static class TagData
    {
        public static Dictionary<string, ElementId> TaggableCategoriesDict;

        public static Dictionary<string , ElementId> ViewCategoriesDict;

        public static void Initialize()
        {
            var dictTaggableCategories = TagUtils.GetTaggableCategories();

            // get all the element categories in the view 
            var dictCategoriesInView = TagUtils.GetElementCategoriesInView();

            TaggableCategoriesDict = dictTaggableCategories
                                                        .Where(kv => dictCategoriesInView.ContainsKey(kv.Key.Replace(" Tags", "")) || 
                                                                     dictCategoriesInView.ContainsKey(kv.Key.Replace(" Tags", "s")))
                                                        .ToDictionary(kv => kv.Key, kv => kv.Value);

            ViewCategoriesDict = dictCategoriesInView
                                                    .Where(kv => dictTaggableCategories.ContainsKey(kv.Key + " Tags") ||
                                                                 dictTaggableCategories.ContainsKey(kv.Key.Remove(kv.Key.Length - 1) + " Tags"))
                                                    .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public struct TagCreateFormData
        {
            public string CategoryColumn;
            public List<string> ElementColumn;
            public string TagColumn;
            public bool Leader;
        }

        public struct TagCheckFormData
        {
            public string CategoryColumn;
            public List<string> ElementColumn;
        }
    }
}

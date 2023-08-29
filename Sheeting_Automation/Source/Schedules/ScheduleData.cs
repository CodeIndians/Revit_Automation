using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace Sheeting_Automation.Source.Schedules
{
    public static class ScheduleData
    {
        public static Document DBDoc = null;

        public static Dictionary<string, ElementId> CategoryDictionary;

        public static Dictionary<string, ElementId> ViewTemplateDictionary;

        public static Dictionary<string, ElementId> PhaseDictionary;

        /// <summary>
        /// update all the schedule data
        /// called when initializing manager
        /// </summary>
        /// <param name="doc"></param>
        public static void UpdateScheduleData(ref Document doc)
        {
            DBDoc = doc;

            // return if the doc is invalid
            if (DBDoc == null)
            {
                TaskDialog.Show("Error", "Document is invalid");
                return;
            }

            //Update categories
            UpdateCategoryDictionary();

            //Update view templates
            UpdateViewTemplateDictionary();

            //Update phases
            UpdatePhaseDictionary();

        }
        /// <summary>
        /// Update categories
        /// </summary>
        private static void UpdateCategoryDictionary()
        {
            // re-initialize the dictionary
            CategoryDictionary = new Dictionary<string, ElementId>();

            FilteredElementCollector collector = new FilteredElementCollector(DBDoc);

            //get distinct categories of elements in the active view
            var categories =
                            collector
                                .OfClass(typeof(ElementType))
                                .WhereElementIsElementType()
                                .Select(x => x.Category)
                                .Distinct(new CategoryComparer())
                                .ToList();

            foreach (var category in categories)
            {
                if (category != null && category.CategoryType == CategoryType.Model && category.HasMaterialQuantities)
                {
                    if (!CategoryDictionary.ContainsKey(category.Name))
                    {
                        CategoryDictionary.Add(category.Name,category.Id);
                    }
                }
                
            }
        }
        
        /// <summary>
        /// Update view templates
        /// </summary>
        private static void UpdateViewTemplateDictionary()
        {
            // re-initialize the dictionary
            ViewTemplateDictionary = new Dictionary<string, ElementId>();

            // Create a filter for view templates
            ElementClassFilter viewTemplateFilter = new ElementClassFilter(typeof(View));

            // Get all view templates
            FilteredElementCollector collector = new FilteredElementCollector(DBDoc)
                .WherePasses(viewTemplateFilter)
                .WhereElementIsNotElementType();


            // Add the view templates to the list
            foreach (Element element in collector)
            {
                View view = element as View;
                if (view != null && view.IsTemplate && (view.ViewType == ViewType.Schedule))
                {
                    if (!ViewTemplateDictionary.ContainsKey(view.Name))
                    {
                        ViewTemplateDictionary.Add(view.Name, view.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Update Phase
        /// </summary>
        private static void UpdatePhaseDictionary()
        {
            //re-initialize the dictionary
            PhaseDictionary = new Dictionary<string, ElementId>();

            // Get all phases in the project
            FilteredElementCollector collector = new FilteredElementCollector(DBDoc)
                .OfClass(typeof(Phase));

            // Add the phases to the list
            foreach (Element element in collector)
            {
                Phase phase = element as Phase;
                if (phase != null)
                {
                    if (!PhaseDictionary.ContainsKey(phase.Name))
                    {
                        PhaseDictionary.Add(phase.Name, phase.Id);
                    }
                }
            }
        }
    
    }
}

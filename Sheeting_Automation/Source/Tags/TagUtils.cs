using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Sheeting_Automation.Source.Schedules;
using Sheeting_Automation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sheeting_Automation.Source.Tags.TagData;

namespace Sheeting_Automation.Source.Tags
{
    public class TagUtils
    {
        /// <summary>
        /// Returns the dictonaries of all the categories in the entire document
        /// </summary>
        /// <returns>Dictonary containing all the categories in the document</returns>
        public static Dictionary<string,ElementId> GetTaggableCategories()
        {

            ViewPlan viewPlan = SheetUtils.m_Document.ActiveView as ViewPlan;

            // return the empty dictonary if the current view is not view plan
            if (viewPlan == null)
            {
                TaskDialog.Show("Error", "Current view is not a view plan");
                return new Dictionary<string, ElementId>();
            }

            //initialize the dictionary
            Dictionary<string,ElementId> dictCategories = new Dictionary<string,ElementId>();

            // get all the elements in the current document 
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document);

            //get distinct categories of elements in the current document
            var categories = collector
                                .OfClass(typeof(ElementType))
                                .WhereElementIsElementType()
                                .Select(x => x.Category)
                                .Distinct(new CategoryComparer())
                                .ToList();

            // filter out the annotation categories
            foreach (Element element in collector)
            {
                Category category = element.Category;

                if (category != null && category.CategoryType == CategoryType.Annotation)
                {
                    if (!dictCategories.ContainsKey(category.Name))
                    {
                        dictCategories.Add(category.Name, category.Id);
                    }
                }

            }

            return dictCategories;

        }

        /// <summary>
        /// Returns the element categories in the current view 
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ElementId> GetElementCategoriesInView()
        {
            // Initialize a set to store unique categories
            Dictionary<string,ElementId> dictCategories = new Dictionary<string, ElementId>();

            // Iterate through the elements in the view and collect their categories
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id);
            foreach (Element element in collector)
            {
                // Get the category of the element
                Category category = element.Category;

                if (category != null && category.CategoryType == CategoryType.Model)
                {
                    if (!dictCategories.ContainsKey(category.Name))
                    {
                        dictCategories.Add(category.Name, category.Id);
                    }
                }
            }

            return dictCategories;
        }

        /// <summary>
        /// Returns the element family names in the current view 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public static Dictionary<string,List<ElementId>> GetElementFamilyNames(ElementId categoryId)
        {
            // Initialize an empty list to store Element IDs
            Dictionary<string, List<ElementId>> elementDict = new Dictionary<string, List<ElementId>>();

            // Use a filtered element collector to find elements of the specified category
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id);
            collector.OfCategoryId(categoryId);

            // Add the Element IDs to the list
            foreach (Element element in collector)
            {
                // Check if the element is a FamilyInstance
                if (element is FamilyInstance familyInstance)
                {
                    // Get the symbol associated with the FamilyInstance
                    FamilySymbol symbol = familyInstance.Symbol;

                    // Get the family name from the symbol
                    string familyName = symbol.Family.Name;

                    if (elementDict.ContainsKey(familyName))
                    {
                        elementDict[familyName].Add(element.Id);
                    }
                    else
                    {
                        elementDict[familyName] = new List<ElementId> { element.Id };
                    }
                }
                else if (element is Wall wallinstance)
                {
                    var familyName = wallinstance.WallType.FamilyName + " : " + wallinstance.Name;

                    if (elementDict.ContainsKey(familyName))
                    {
                        elementDict[familyName].Add(element.Id);
                    }
                    else
                    {
                        elementDict[familyName] = new List<ElementId> { element.Id };
                    }
                }

               
            }

            return elementDict;
        }

        /// <summary>
        /// Return the tag names in the document 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public static Dictionary<string, ElementId> GetAnnotationSymbolFamilyNames(ElementId categoryId)
        {
            // Initialize a list to store the annotation symbol family names
            Dictionary<string, ElementId> annotationSymbolFamilyNames = new Dictionary<string, ElementId>();


            // Use a filtered element collector to find family symbols in the annotation category
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document);
            collector.OfClass(typeof(FamilySymbol)).OfCategoryId(categoryId);

            foreach (Element element in collector)
            {
                if (element is FamilySymbol familySymbol)
                {
                    string combinedName = familySymbol.Family.Name + " : " + familySymbol.Name;
                    if (!annotationSymbolFamilyNames.ContainsKey(combinedName))
                    {
                        // Add the family name to the list
                        annotationSymbolFamilyNames.Add(combinedName, familySymbol.Id);
                    }
                }
            }

            return annotationSymbolFamilyNames;
        }

        /// <summary>
        ///  Checks if the current view is view plan
        /// </summary>
        /// <returns>true if the current view is view plan, false otherwise </returns>
        public static bool IsCurrentViewPlan()
        {
            ViewPlan viewPlan = SheetUtils.m_Document.ActiveView as ViewPlan;

            return (viewPlan != null);
        }

        /// <summary>
        /// Get the no tag value 
        /// if we pass Wall tags , Walls is returned
        /// </summary>
        /// <param name="tagsValue"></param>
        /// <returns></returns>
        public static string GetNoTagValue(string tagsValue)
        {
            // noTagvalue to remove the "Tags"
            string noTagValue = tagsValue;

            if (tagsValue.Contains(" Tags"))
            {
                noTagValue = tagsValue.Replace(" Tags", "");
            }

            // check for the categories in the view 
            foreach (var key in TagData.ViewCategoriesDict.Keys)
            {
                if (key.Contains(noTagValue))
                {
                    noTagValue = key;
                    break;
                }
            }

            return noTagValue;
        }

        /// <summary>
        /// Check if curve is horizontal or vertical
        /// </summary>
        /// <param name="locationCurve"></param>
        /// <returns></returns>
        public static TagOrientation GetCurveOrientation(LocationCurve locationCurve)
        {
            // get the curve as line 
            Line curveLine = locationCurve?.Curve as Line;

            if (curveLine != null)
            {
                XYZ direction = curveLine.Direction;

                if (IsAlmostEqual(Math.Abs(direction.X), 0.0) && IsAlmostEqual(Math.Abs(direction.Y), 1.0))
                {
                    // The curve is vertical (Y-axis direction)
                    return TagOrientation.Vertical;
                }
                else if (IsAlmostEqual(Math.Abs(direction.X), 1.0) && IsAlmostEqual(Math.Abs(direction.Y), 0.0))
                {
                    // The curve is horizontal (X-axis direction)
                    return TagOrientation.Horizontal;
                }
            }

            // The curve is treated as horizontal by default
            return TagOrientation.Horizontal;
        }

        // Helper method to compare floating-point values with tolerance
        private static bool IsAlmostEqual(double a, double b, double tolerance = 0.0001)
        {
            return Math.Abs(a - b) < tolerance;
        }
    }


}

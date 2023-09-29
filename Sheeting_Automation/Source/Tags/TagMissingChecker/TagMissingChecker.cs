using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Sheeting_Automation.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Tags
{
    public static class TagMissingChecker
    {

        private static List<ElementId> CollectedElementIdList;

        private static List<ElementId> CollectedIndependentTagIds;

        /// <summary>
        /// function for checking individual rows 
        /// </summary>
        /// <param name="formData"></param>
        public static void CheckTags(TagData.TagCheckFormData formData)
        {
            if(formData.CategoryColumn == null)
            {
                TaskDialog.Show("Error", "Empty form data");
                return;
            }

            // collect all the independent tags in the current view 
            CollectIndependentTags();

            // reinitialize the list 
            CollectedElementIdList = new List<ElementId>();

            // collect the elements as per the form selection
            CollectSelectedCategoryElements(formData);

            //validate if the tags are present for all the elements
            // create no tags for elements which dont have tags 
            int noOfTags = ValidateTagsAndCreateNoTags(formData.CategoryColumn);

            if(noOfTags > 0)
            {
                TaskDialog.Show("Error", $"Placing {noOfTags} No-Tags");
            }
            else if (noOfTags == 0)
            {
                TaskDialog.Show("Info", "Mo missing tags");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formDataList">Input is the entire form data</param>
        public static void CheckTags(List<TagData.TagCheckFormData> formDataList)
        {

            if(formDataList.Count == 0)
            {
                TaskDialog.Show("Error", "Empty form data");
                return;
            }

            // reinitialize the list 
            CollectedElementIdList = new List<ElementId>();

            // collect all the independent tags in the current view 
            CollectIndependentTags();

            int noTagsCount = 0;

            foreach (var formData in formDataList)
            {
                // reinitialize the list 
                CollectedElementIdList = new List<ElementId>();

                // collect the elements as per the form selection
                CollectSelectedCategoryElements(formData);

                //validate if the tags are present for all the elements
                // create no tags for elements which dont have tags 
                noTagsCount += ValidateTagsAndCreateNoTags(formData.CategoryColumn);
            }

            if (noTagsCount > 0)
            {
                TaskDialog.Show("Error", $"Placing {noTagsCount} No-Tags");
            }
            else if (noTagsCount == 0)
            {
                TaskDialog.Show("Info", "Mo missing tags");
            }

        }

        /// <summary>
        /// Collect the elements 
        /// </summary>
        /// <param name="formData"></param>
        private static void CollectSelectedCategoryElements(TagData.TagCheckFormData formData)
        {
            
            // get the no tag value
            var noTagValue = TagUtils.GetNoTagValue(formData.CategoryColumn);

            // get element family names ( 2nd form column ) 
            var elementDict = TagUtils.GetElementFamilyNames(TagData.ViewCategoriesDict[noTagValue]);

            // iterate through the selected element family names list
            // ( can be one element or ALL ) 
            foreach (var elementname in formData.ElementColumn)
            {
                // get all the element ids of a particular element family ( 2nd column ) 
                List<ElementId> elementIds = elementDict[elementname];

                // iterate all the element ids
                foreach (ElementId elementId in elementIds)
                {
                    // retrieve element from the element id 
                    Element element = SheetUtils.m_Document.GetElement(elementId);

                    if(element != null)
                    {
                        CollectedElementIdList.Add(elementId);
                    }
                }
            }
        }


        private static void CollectIndependentTags()
        {
            // Use the FilteredElementCollector to check if there are associated tags
            FilteredElementCollector tagCollector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id);
            tagCollector.OfClass(typeof(IndependentTag));

            CollectedIndependentTagIds = new List<ElementId>();

            foreach(IndependentTag tag in tagCollector)
            {
                CollectedIndependentTagIds.AddRange(tag.GetTaggedLocalElementIds());
            }
        }

        /// <summary>
        /// Checks if the given  
        /// </summary>
        /// <param name="categoryColumn"></param>
        /// <returns></returns>
        private static int ValidateTagsAndCreateNoTags(string categoryColumn)
        {
            List<ElementId> noTagElementIdList = new List<ElementId>();

            foreach(var elementId in CollectedElementIdList)
            {
                if(!CollectedIndependentTagIds.Contains(elementId))
                    noTagElementIdList.Add(elementId);
            }

            // place the tags for elements that dont have tags
            if(noTagElementIdList.Count > 0)
            {
                // retrieve tags dict 
                var tagsDict = TagUtils.GetAnnotationSymbolFamilyNames(TagData.TaggableCategoriesDict[categoryColumn]);

                ElementId noTagId = GetNoTagElementId(tagsDict);

                if(noTagId != ElementId.InvalidElementId)
                {
                    CreateNoTag(noTagId, noTagElementIdList);
                }
                else
                {
                    TaskDialog.Show("Error", "No tag family not found for " + categoryColumn);
                }
            }

            return noTagElementIdList.Count;

        }

        private static void CreateNoTag(ElementId noTagId, List<ElementId> noTagElementIdList)
        {
            using (Transaction transaction = new Transaction(SheetUtils.m_Document))
            {
                transaction.Start("Place No tags");

                // iterate all the element ids
                foreach (ElementId elementId in noTagElementIdList)
                {
                    // retrieve element from the element id 
                    Element element = SheetUtils.m_Document.GetElement(elementId);

                    if (element != null)
                    {
                        // create a ref with the element 
                        Reference reference = new Reference(element);

                        // extract the location curve of the element
                        LocationCurve locationCurve = element.Location as LocationCurve;

                        // extract the location point of the element
                        LocationPoint locationPoint = element.Location as LocationPoint;

                        // initialize the tag location
                        XYZ tagLocation = new XYZ();

                        //default orientation is horizontal
                        TagOrientation orientation = TagOrientation.Horizontal;

                        // handling curve or point 
                        if (locationCurve != null)
                        {
                            tagLocation = (locationCurve.Curve.GetEndPoint(0) + locationCurve.Curve.GetEndPoint(1)) / 2.0;
                            orientation = TagUtils.GetCurveOrientation(locationCurve);
                        }
                        else if (locationPoint != null)
                        {
                            tagLocation = locationPoint.Point;
                            orientation = TagOrientation.Horizontal;
                        }

                        // create the tag 
                        IndependentTag tag = IndependentTag.Create(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id, reference, false, TagMode.TM_ADDBY_CATEGORY, orientation, tagLocation);

                        // set the tag type 
                        tag.ChangeTypeId(noTagId);
                    }
                }
                
                transaction.Commit();
            }

            SelectElementsByElementIds(noTagElementIdList);
        }

        /// <summary>
        /// Fetch No Tag element id from the corresponding category.
        /// </summary>
        /// <param name="elementDict">Dictionary consisting of all the tags belonging to a specific category</param>
        /// <returns></returns>
        private static ElementId GetNoTagElementId(Dictionary<string,ElementId> elementDict)
        {
            ElementId elemId = ElementId.InvalidElementId;

            foreach (var key in elementDict.Keys)
            {
                if (key.Contains("No Tag"))
                {
                    return elementDict[key];
                }
            }

            return elemId;
        }

        private static void SelectElementsByElementIds(List<ElementId> elementIds)
        {
            // Create a list to hold Reference objects
            List<Reference> references = new List<Reference>();

            foreach (ElementId elementId in elementIds)
            {
                // Create a Reference object for each Element ID
                Reference reference = new Reference(SheetUtils.m_Document.GetElement(elementId));
                references.Add(reference);
            }

            // get the previous selection 
            var alreadySelectedElementIds = SheetUtils.m_Selection.GetElementIds();

            // required selection as list
            var list = references.Select(r => r.ElementId).ToList();

            //combine both selections 
            list.AddRange(alreadySelectedElementIds);

            // Add the new references to the selection
            SheetUtils.m_Selection.SetElementIds(list);

            // Refresh the document to display the selection
            SheetUtils.m_UIDocument.RefreshActiveView();
        }

    }
}

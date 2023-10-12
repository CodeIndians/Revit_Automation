using Autodesk.Revit.DB;
using Sheeting_Automation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Tags
{
    public class TagCreator
    {
        // data captured from the form
        private List<TagData.TagCreateFormData> mFormDataList;

        //ctor 
        public TagCreator(List<TagData.TagCreateFormData> tagFormData) 
        {
            mFormDataList = tagFormData;
        }

        /// <summary>
        /// Creates tag from the form data 
        /// </summary>
        public void CreateTags()
        {
            foreach(var formData in mFormDataList) 
            {
                CreateTag(formData);
            }
        }


        /// <summary>
        /// Create tag for each form row 
        /// </summary>
        /// <param name="formData"></param>
        private void CreateTag(TagData.TagCreateFormData formData)
        {
            // retrieve tags dict ( 3rd form column) 
            var tagsDict = TagUtils.GetAnnotationSymbolFamilyNames(TagData.TaggableCategoriesDict[formData.CategoryColumn]);

            // get the selected tag id 
            ElementId tagId = tagsDict[formData.TagColumn];

            // get the no tag value
            var noTagValue = TagUtils.GetNoTagValue(formData.CategoryColumn);

            // get element family names ( 2nd form column ) 
            var elementDict = TagUtils.GetElementFamilyNames(TagData.ViewCategoriesDict[noTagValue]);

            using (Transaction transaction = new Transaction(SheetUtils.m_Document))
            {
                transaction.Start("place tag");

                // iterate through the selected element family names list
                // ( can be one element or ALL ) 
                foreach(var elementname in formData.ElementColumn)
                {
                    // get all the element ids of a particular element family ( 2nd column ) 
                    List<ElementId> elementIds = elementDict[elementname];

                    // iterate all the element ids
                    foreach (ElementId elementId in elementIds)
                    {
                        // retrieve element from the element id 
                        Element element = SheetUtils.m_Document.GetElement(elementId);

                        if (element != null)
                        {
                            // create a ref with the element 
                            Reference reference = new Reference(element);

                            // get the first bounding box of the element from the collector
                            BoundingBoxXYZ elementBoundingBox = BoundingBoxCollector.BoundingBoxesDict[elementId].FirstOrDefault();

                            // place the tag at the mid point of the bounding box 
                            XYZ tagLocation = (elementBoundingBox.Min + elementBoundingBox.Max) / 2;

                            // create the tag 
                            IndependentTag tag = IndependentTag.Create(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id, reference, formData.Leader, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Vertical, tagLocation);

                            // set the tag type 
                            tag.ChangeTypeId(tagId);

                            // add all the tags 
                            BoundingBoxCollector.IndependentTags.Add(new TagData.Tag(tag, element));
                        }
                    }
                }

                transaction.Commit();
            }
        }
    }
}

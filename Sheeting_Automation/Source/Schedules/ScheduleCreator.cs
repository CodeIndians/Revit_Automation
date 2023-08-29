using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System;
using System.Windows.Media;

namespace Sheeting_Automation.Source.Schedules
{
    internal class ScheduleCreator
    {
        private Document mDoc;

        /// <summary>
        /// CTOR
        /// </summary>
        public ScheduleCreator()
        {
            // Initialize the DB document
            mDoc = ScheduleData.DBDoc;
        }

        /// <summary>
        /// Create the view schedule
        /// </summary>
        /// <param name="name"> schedule name</param>
        /// <param name="category"> schedule cateogory</param>
        /// <param name="phase"> buidling phase </param>
        /// <param name="viewTemplate">view template of the current schedule view</param>
        /// <param name="prefix"></param>
        /// <param name="start"></param>
        /// <param name="suffix"></param>
        public void Create(string name, string category, string phase, string viewTemplate, string prefix, string start, string suffix)
        {
            Transaction trans1 = new Transaction(mDoc, "Set Schedule Properties");

            trans1.Start();

            // Create a new view schedule
            ViewSchedule viewSchedule = ViewSchedule.CreateSchedule(mDoc, ScheduleData.CategoryDictionary[category]);

            // set the name of the view schedule
            viewSchedule.Name = name;

            // get the selected phase id from the dictionary
            ElementId phaseid = ScheduleData.PhaseDictionary[phase];

            //set the phase of the view schedule
            if (phaseid != null || phaseid != ElementId.InvalidElementId)
                viewSchedule.get_Parameter(BuiltInParameter.VIEW_PHASE).Set(phaseid);

            // get the selected viewtemplate id from the dictionary
            ElementId viewTemplateId = ScheduleData.ViewTemplateDictionary[viewTemplate];

            //set the view template of the view schedule
            if (viewTemplateId != null && viewTemplateId != ElementId.InvalidElementId)
                viewSchedule.ViewTemplateId = viewTemplateId;

            //commmit the transaction
            trans1.Commit();

            //Fill the element ids
            FillElementIds(viewSchedule);

            ItemizeInstancesInView(viewSchedule, true);
            ShowHideElementField(viewSchedule, true);

            List<string> elementIds = GetOrderedElementsInView(viewSchedule);

            ItemizeInstancesInView(viewSchedule, false);
            ShowHideElementField(viewSchedule, false);

            //show the count field
            ShowHideCountField(viewSchedule, true);

            // Fill the markers 
            FillMarkers(viewSchedule, prefix, start, suffix, elementIds);

            //hide the count field 
            ShowHideCountField(viewSchedule, false);

        }

        /// <summary>
        /// Fills the mark 
        /// </summary>
        /// <param name="viewSchedule"> </param>
        /// <param name="prefix"></param>
        /// <param name="start"></param>
        /// <param name="suffix"></param>
        /// <param name="orderedElementIds"></param>
        public void FillMarkers(ViewSchedule viewSchedule, string prefix, string start, string suffix, List<string> orderedElementIds)
        {

            TableData tableData = viewSchedule.GetTableData();

            TableSectionData tableSectionData = tableData.GetSectionData(SectionType.Body);

            int rows = tableSectionData.NumberOfRows;

            int colums = tableSectionData.NumberOfColumns;

            int countIndex = -1;

            // check for count 
            for (int i = 0; i < colums; i++)
            {
                var cellText = viewSchedule.GetCellText(SectionType.Body, 0, i);
                if (viewSchedule.GetCellText(SectionType.Body, 0, i) == "Count")
                {
                    countIndex = i;
                    break;
                }
            }

            // this means that Count is mapped to Qty
            if (countIndex == -1)
            {
                // check for count 
                for (int i = 0; i < colums; i++)
                {
                    var cellText = viewSchedule.GetCellText(SectionType.Body, 0, i);
                    if (viewSchedule.GetCellText(SectionType.Body, 0, i) == "Qty")
                    {
                        countIndex = i;
                        break;
                    }
                }
            }


            List<int> counts = new List<int>();

            // update the counts array from the view schedule table data
            for (int i = 1; i < rows; i++)
            {
                counts.Add(int.Parse(viewSchedule.GetCellText(SectionType.Body, i, countIndex)));
            }

            // update the markers 
            UpdateMarkers(viewSchedule, prefix, start, suffix, counts, orderedElementIds);
        }

        /// <summary>
        /// Show or hide the inbuilt Count field on the view schedule
        /// </summary>
        /// <param name="viewSchedule">view schedule</param>
        /// <param name="isShow">flag to show or hide Count field in the view schedule</param>
        private void ShowHideCountField(ViewSchedule viewSchedule, bool isShow)
        {
            Transaction trans2 = new Transaction(mDoc, "Hide Show count field");
            trans2.Start();

            ScheduleField countField = ScheduleUtils.GetScheduleFieldFromName(viewSchedule, "Count");

            if (countField.ColumnHeading != "Qty")
                countField.IsHidden = !isShow;

            trans2.Commit();
        }

        /// <summary>
        /// Show or hide Element filed on the view schedule
        /// </summary>
        /// <param name="viewSchedule">Current view </param>
        /// <param name="isShow"> set this to true to show this field else false</param>
        private void ShowHideElementField(ViewSchedule viewSchedule, bool isShow)
        {
            Transaction trans2 = new Transaction(mDoc, "Show or Hide element field");
            trans2.Start();

            ScheduleField countField = ScheduleUtils.GetScheduleFieldFromName(viewSchedule, "Element");

            if(countField == null)
            {
                MessageBox.Show("Element field is not present");
                return;
            }

            countField.IsHidden = !isShow;

            trans2.Commit();
        }

        /// <summary>
        /// Itemize or un itemize the instances in the view 
        /// </summary>
        /// <param name="viewSchedule">current view </param>
        /// <param name="itemize"> true to itemize, false to revert</param>
        private void ItemizeInstancesInView(ViewSchedule viewSchedule, bool itemize)
        {
            Transaction trans2 = new Transaction(mDoc, "Itemize instances");

            trans2.Start();

            ScheduleDefinition scheduleDefinition = viewSchedule.Definition;

            scheduleDefinition.IsItemized = itemize;

            trans2.Commit();
        }

        /// <summary>
        /// update the markers in the view schedule
        /// </summary>
        /// <param name="viewSchedule">view schedule</param>
        /// <param name="prefix">prefix string </param>
        /// <param name="start">start string ( 1 or 1.1 etc) </param>
        /// <param name="suffix">suffix string</param>
        /// <param name="Counts">group counts in the view schedule</param>
        /// <param name="orderedElementIds">list of element ids in the sorted order</param>
        private void UpdateMarkers(ViewSchedule viewSchedule, string prefix, string start, string suffix, List<int> Counts, List<string> orderedElementIds)
        {
            // Get all the elements in the view
            FilteredElementCollector collector = new FilteredElementCollector(mDoc, viewSchedule.Id);

            //IEnumerable<Element> sortedElements = GetSortedElements(collector, sortgroupFields, sortGroupFieldTypeNames);

            string[] splitStrings = start.Split('.');

            // start value is 1 by default 
            double startValue = 1;

            // if the value is decimal 
            if (splitStrings.Length == 2)
            {
                // prefix = prefix + integer part + "."
                prefix = prefix + splitStrings[0] + ".";

                // start value = decimal part
                startValue = double.Parse(splitStrings[1]);
            }
            // if the value is an integer 
            else if (splitStrings.Length == 1)
            {
                startValue = double.Parse(splitStrings[0]);
            }

            // Update the marks on the sorted elements ,
            // grouping them by the counts obtained from the view
            using (Transaction trans = new Transaction(mDoc, "Update Mark Parameter"))
            {
                trans.Start();

                int currentIndex = 0;

                // loop through the counts array 
                foreach (var count in Counts)
                {
                    // update the mark value , based on start value 
                    string markValue = prefix + startValue.ToString() + suffix;

                    for (int i = 0; i < count; i++)
                    {
                        // read the mark param
                        Parameter markParam = mDoc.GetElement(new ElementId(int.Parse(orderedElementIds[currentIndex]))).LookupParameter("Mark");

                        // set the mark parameter
                        if (markParam != null && !markParam.IsReadOnly)
                        {
                            markParam.Set(markValue);
                        }
                        // this is used to loop across all the elements
                        currentIndex++;
                    }

                    // increment the start value 
                    startValue++;
                }

                trans.Commit();
            }
        }

        /// <summary>
        /// Update the markers on the current view
        /// This is called when clicked on Update Schedule
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="start"></param>
        /// <param name="suffix"></param>
        public void UpdateMarkersCurrentView(string prefix, string start, string suffix)
        {
            // get the current view as view schedule 
            ViewSchedule viewSchedule = mDoc.ActiveView as ViewSchedule;

            // throw an error if this is not a view schedule
            if (viewSchedule == null)
            {
                TaskDialog.Show("Error", "Not a schedule view");
                return;
            }

            //Fill the element ids
            FillElementIds(viewSchedule);

            // Itemize the instances and show the element field
            ItemizeInstancesInView(viewSchedule, true);
            ShowHideElementField(viewSchedule, true);

            // capture the element ids
            List<string> elementIds = GetOrderedElementsInView(viewSchedule);

            //Hide element filed and unchceck itemize 
            ItemizeInstancesInView(viewSchedule, false);
            ShowHideElementField(viewSchedule, false);

            //show the count field
            ShowHideCountField(viewSchedule, true);

            // Fill the markers 
            FillMarkers(viewSchedule, prefix, start, suffix, elementIds);

            //hide the count field 
            ShowHideCountField(viewSchedule, false);
        }

        /// <summary>
        /// Update the param "Element" on each of the element in the given view schedule
        /// </summary>
        /// <param name="viewSchedule"></param>
        private void FillElementIds(ViewSchedule viewSchedule)
        {

            FilteredElementCollector collector = new FilteredElementCollector(mDoc, viewSchedule.Id);
            using (Transaction trans = new Transaction(mDoc, "Update Element Ids"))
            {
                trans.Start();

                foreach (Element element in collector)
                {
                    element.LookupParameter("Element").Set(element.Id.ToString());
                }

                trans.Commit();
            }
        }

        /// <summary>
        /// Get the element ids in the view in the sorted order specified by the view
        /// </summary>
        /// <param name="viewSchedule"></param>
        /// <returns></returns>
        private List<string> GetOrderedElementsInView(ViewSchedule viewSchedule)
        {
            List<string> elementIds = new List<string>();

            TableData tableData = viewSchedule.GetTableData();

            TableSectionData tableSectionData = tableData.GetSectionData(SectionType.Body);

            int rows = tableSectionData.NumberOfRows;

            int colums = tableSectionData.NumberOfColumns;

            int elementIndex = -1;

            // check for count 
            for (int i = 0; i < colums; i++)
            {
                var cellText = viewSchedule.GetCellText(SectionType.Body, 0, i);
                if (viewSchedule.GetCellText(SectionType.Body, 0, i) == "Element")
                {
                    elementIndex = i;
                    break;
                }
            }

            for (int i = 1; i < rows; i++)
            {
                elementIds.Add(viewSchedule.GetCellText(SectionType.Body, i, elementIndex));
            }

            return elementIds;
        }
    }
}

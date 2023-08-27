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

            //show the count field
            ShowHideCountField(viewSchedule, true);

            // Fill the markers 
            FillMarkers(viewSchedule,prefix,start,suffix);

            //hide the count field 
            ShowHideCountField(viewSchedule, false);

        }

        public void FillMarkers(ViewSchedule viewSchedule, string prefix, string start, string suffix)
        {
            //ViewSchedule viewSchedule = mDoc.ActiveView as ViewSchedule;

            //if (viewSchedule == null)
            //{
            //    TaskDialog.Show("Error", "Not a schedule view");
            //    return;
            //}

            TableData tableData = viewSchedule.GetTableData();

            TableSectionData tableSectionData = tableData.GetSectionData(SectionType.Body);

            int rows = tableSectionData.NumberOfRows;

            int colums = tableSectionData.NumberOfColumns;

            int countIndex = -1;

            for (int i = 0; i < colums; i++)
            {
                if (viewSchedule.GetCellText(SectionType.Body, 0, i) == "Count")
                {
                    countIndex = i;
                    break;
                }
            }

            List<int> counts = new List<int>();

            for (int i = 1; i < rows; i++)
            {
                counts.Add(int.Parse(viewSchedule.GetCellText(SectionType.Body, i, countIndex)));
            }

            List<string> sortGroupFields = GetSortGroupFieldNames(viewSchedule);

            UpdateMarkers(viewSchedule, prefix, start,suffix, counts,sortGroupFields);
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

            IList<ScheduleFieldId> fieldOrder = viewSchedule.Definition.GetFieldOrder();

            ScheduleField countField = ScheduleUtils.GetScheduleFieldFromName(viewSchedule,"Count");

            countField.IsHidden = !isShow;
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
        /// <param name="sortgroupFields">list of sort group fields in the given order</param>
        private void UpdateMarkers(ViewSchedule viewSchedule, string prefix, string start, string suffix, List<int> Counts, List<string> sortgroupFields)
        {
            // Get all the elements in the view
            FilteredElementCollector collector = new FilteredElementCollector(mDoc, viewSchedule.Id);

            IEnumerable<Element> sortedElements = GetSortedElements(collector, sortgroupFields);

            string[] splitStrings = start.Split('.');

            // start value is 1 by default 
            double startValue = 1;

            // if the value is decimal 
            if(splitStrings.Length ==2)
            {
                // prefix = prefix + integer part + "."
                prefix = prefix + splitStrings[0] + ".";

                // start value = decimal part
                startValue = double.Parse(splitStrings[1]);
            }
            // if the value is an integer 
            else if(splitStrings.Length == 1)
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
                foreach(var count in  Counts)
                {
                    // update the mark value , based on start value 
                    string markValue = prefix + startValue.ToString() + suffix;

                    for( int i = 0; i < count; i++ )
                    {
                        // read the mark param
                        Parameter markParam = sortedElements.ElementAt(currentIndex).LookupParameter("Mark");

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
        ///  Get the sort field group names on the schedule in the provided order 
        /// </summary>
        /// <param name="viewSchedule"></param>
        /// <returns>list of sort group field strings</returns>
        private List<string> GetSortGroupFieldNames(ViewSchedule viewSchedule)
        {
            List<string> fields = new List<string>();

            ScheduleDefinition scheduleDefinition = viewSchedule.Definition;

            List<ScheduleSortGroupField> sortGroupFields = scheduleDefinition.GetSortGroupFields().ToList();

            foreach(ScheduleSortGroupField sortGroupField in sortGroupFields)
            {
                ScheduleField field = scheduleDefinition.GetField(sortGroupField.FieldId);

                fields.Add(field.GetName());
            }

            return fields;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collector"> filtered element collector on the current view schedule </param>
        /// <param name="sortgroupFields">list of sort group field names </param>
        /// <returns> IEnumerable of sorted elements </returns>
        private IEnumerable<Element> GetSortedElements(FilteredElementCollector collector , List<string> sortgroupFields)
        {
            // Sort the elements based on the schedule field
            IEnumerable<Element> sortedElements = collector;

            if (sortgroupFields.Count == 1)
            {
                sortedElements = sortedElements.OrderBy(e => GetParameterValue(e, sortgroupFields[0]));
            }

            else if (sortgroupFields.Count == 2)
            {
                sortedElements = sortedElements.OrderBy(e => GetParameterValue(e, sortgroupFields[0]))
                                               .ThenBy(e=>GetParameterValue(e, sortgroupFields[1]));
            }

            else if(sortgroupFields.Count == 3)
            {
                sortedElements = sortedElements.OrderBy(e => GetParameterValue(e, sortgroupFields[0]))
                                              .ThenBy(e => GetParameterValue(e, sortgroupFields[1]))
                                              .ThenBy(e => GetParameterValue(e, sortgroupFields[2]));
            }
            else if(sortgroupFields.Count == 4)
            {
                sortedElements = sortedElements.OrderBy(e => GetParameterValue(e, sortgroupFields[0]))
                                              .ThenBy(e => GetParameterValue(e, sortgroupFields[1]))
                                              .ThenBy(e => GetParameterValue(e, sortgroupFields[2]))
                                              .ThenBy(e => GetParameterValue(e, sortgroupFields[3]));
            }
            else
            {
                // else case just sort based on the first field 
                // this is just a default condition
                sortedElements = sortedElements.OrderBy(e => GetParameterValue(e, sortgroupFields[0]));
            }

            return sortedElements;
        }

        /// <summary>
        /// Get the comparable type of the schedule field parameter 
        /// </summary>
        /// <param name="element"> Revit element </param>
        /// <param name="scheduleFieldName">schedule field name in string </param>
        /// <returns></returns>
        private IComparable GetParameterValue(Element element, string scheduleFieldName)
        {
            Parameter parameter = element.LookupParameter(scheduleFieldName);
            if (parameter != null)
            {
                if (parameter.StorageType == StorageType.Double)
                {
                    return parameter.AsDouble();
                }
                else if (parameter.StorageType == StorageType.Integer)
                {
                    return parameter.AsInteger();
                }
                else if(parameter.StorageType == StorageType.String)
                {
                    return parameter.AsString();
                }
                else if(parameter.StorageType == StorageType.ElementId)
                {
                    return parameter.AsValueString();
                }

            }
            // lets use this as the default type
            return parameter.AsValueString();
        }
    }
}

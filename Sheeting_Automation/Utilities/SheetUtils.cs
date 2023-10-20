

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace Sheeting_Automation.Utils
{
    public class SheetUtils
    {
        public static Document m_Document;

        public static Selection m_Selection;

        public static UIDocument m_UIDocument;

        public static ElementId m_ActiveViewId;

        public static View m_ActiveView;

        public static ViewDetailLevel m_DetailLevel;
        public static List<string> GetFloorPlans()
        {
            List<string> strFloorPlansList = new List<string>();

            FilteredElementCollector collector = new FilteredElementCollector(m_Document);
            collector.OfClass(typeof(ViewPlan));

            // Filter the collector to include only floor plans
            List<ViewPlan> floorPlans = collector.Cast<ViewPlan>()
                                                 .Where(vp => vp.ViewType == ViewType.FloorPlan)
                                                 .ToList();

            foreach (ViewPlan floorPlan in floorPlans)
            {
                strFloorPlansList.Add(floorPlan.Name.ToString());
            }
            return strFloorPlansList;
        }

        public static HashSet<string> GetSheetScales()
        {
            List<View> views = GetViews(m_Document);
            HashSet<string> scales = GetScales(views);

            return scales;
        }

        public static void SetDetailLevelToFine()
        {
            // return if the active view is not assigned
            if (m_ActiveView == null)
            {
                TaskDialog.Show("Error", "Active view is not set internally, contact dev team");
                return;
            }

            /// view template is not present in this case 
            /// /////////////////////////////////////////
            if(m_ActiveView.ViewTemplateId.IntegerValue == -1)
            {
                // store the current detail level
                m_DetailLevel = m_ActiveView.DetailLevel;

                // change the detail level only if it is not already set to Fine
                if(m_DetailLevel != ViewDetailLevel.Fine)
                {
                    using (Transaction transaction = new Transaction(m_Document))
                    {
                        transaction.Start("Set the view detail level to fine");

                        // change the detail level to fine 
                        m_ActiveView.DetailLevel = ViewDetailLevel.Fine;

                        transaction.Commit();
                    }
                }
            }
            else // this means that we have to change the view detail level on the view template
            {
                // get the view template from view template it 
                View viewTemplate = m_Document.GetElement(m_ActiveView.ViewTemplateId) as View;

                if (viewTemplate != null)
                {
                    // store the cuurent detail level on the view template
                    m_DetailLevel = viewTemplate.DetailLevel;

                    // change the detail level only if it is not already set to Fine
                    if (m_DetailLevel != ViewDetailLevel.Fine)
                    {
                        using (Transaction transaction = new Transaction(m_Document))
                        {
                            transaction.Start("Reset the view detail level to original");

                            // change the detail level to fine 
                            viewTemplate.DetailLevel = ViewDetailLevel.Fine;

                            transaction.Commit();
                        }
                    }
                }

            }
            /// /////////////////////////////////////////
            /// /////////////////////////////////////////

        }

        public static void ResetDetailLevel()
        {
            if (m_ActiveView == null)
            {
                TaskDialog.Show("Error", "Active view is not set internally, contact dev team");
                return;
            }

            /// view template is not present in this case 
            /// /////////////////////////////////////////
            if (m_ActiveView.ViewTemplateId.IntegerValue == -1)
            {
                // change the detail level only if it is not already set to Fine
                if (m_DetailLevel != ViewDetailLevel.Fine)
                {
                    using (Transaction transaction = new Transaction(m_Document))
                    {
                        transaction.Start("Set the view detail level to fine");

                        // Revert back to the original detail level
                        m_ActiveView.DetailLevel = m_DetailLevel;

                        transaction.Commit();
                    }
                }
            }
            else // this means that we have to change the view detail level on the view template
            {
                // get the view template from view template it 
                View viewTemplate = m_Document.GetElement(m_ActiveView.ViewTemplateId) as View;

                if (viewTemplate != null)
                {
                    // change the detail level only if it is not already set to Fine
                    if (m_DetailLevel != ViewDetailLevel.Fine)
                    {
                        using (Transaction transaction = new Transaction(m_Document))
                        {
                            transaction.Start("Set the view detail level to fine");

                            // change the detail level to fine 
                            viewTemplate.DetailLevel = m_DetailLevel;

                            transaction.Commit();
                        }
                    }
                }
            }
            /// /////////////////////////////////////////
            /// /////////////////////////////////////////
        }

        private static List<View> GetViews(Document doc)
        {
            // Create a filter to retrieve views
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(View));

            // Filter the collector to include only views (excluding templates and sheets)
            List<View> views = collector.Cast<View>()
                                         .Where(v => v.ViewType != ViewType.ProjectBrowser &&
                                                     v.ViewType != ViewType.SystemBrowser &&
                                                     !v.IsTemplate)
                                         .ToList();

            return views;
        }

        private static HashSet<string> GetScales(List<View> views)
        {
            HashSet<string> scales = new HashSet<string>();

            foreach (View view in views)
            {
                Parameter ViewScaleParam = view.LookupParameter("View Scale");
                if (ViewScaleParam != null)
                {
                   scales.Add(ViewScaleParam.AsValueString());
                }
            }

            return scales;
        }

    }
}



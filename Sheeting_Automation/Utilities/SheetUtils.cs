

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



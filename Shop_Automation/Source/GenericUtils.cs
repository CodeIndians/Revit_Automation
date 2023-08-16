

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shop_Automation.Utils
{
    public class GenericUtils
    {
        public static List<string> GetElevationViewType(Document doc)
        {
            List<string> list = new List<string>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            // Apply a filter to retrieve only view templates
            collector.OfClass(typeof(Autodesk.Revit.DB.View));

            // Retrieve view templates from the collector
            IEnumerable<Autodesk.Revit.DB.View> viewTemplates = collector.Cast<Autodesk.Revit.DB.View>()
                .Where(v => v.IsTemplate);

            // Loop through the retrieved view templates
            foreach (Autodesk.Revit.DB.View viewTemplate in viewTemplates)
            {
                Debug.WriteLine(string.Format("ViewName : {0}, View Type :{1}", viewTemplate.Name, viewTemplate.ViewType));
                // Access the properties of the view template
                string viewTemplateName = viewTemplate.Name;
                if (viewTemplate.ViewType == ViewType.Elevation ||
                    viewTemplate.ViewType == ViewType.Section ||
                    viewTemplate.ViewType == ViewType.Detail)
                    list.Add(viewTemplateName);

            }
            return list;
        }

        public static List<string> GetScheduleType(Document doc)
        {
            List<string> list = new List<string>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            // Apply a filter to retrieve only view templates
            collector.OfClass(typeof(Autodesk.Revit.DB.View));

            // Retrieve view templates from the collector
            IEnumerable<Autodesk.Revit.DB.View> viewTemplates = collector.Cast<Autodesk.Revit.DB.View>()
                .Where(v => v.IsTemplate);

            // Loop through the retrieved view templates
            foreach (Autodesk.Revit.DB.View viewTemplate in viewTemplates)
            {
                // Access the properties of the view template
                string viewTemplateName = viewTemplate.Name;
                if (viewTemplate.ViewType == ViewType.Schedule) 
                    list.Add(viewTemplateName);
                
            }

            return list;
        }

        public static List<string> GetTitleBlocks(Document doc)
        {
            List <string> list = new List<string>();

            // Create a filtered element collector to retrieve title blocks
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            // Apply a filter to retrieve only title blocks
            collector.OfClass(typeof(FamilySymbol));
            collector.OfCategory(BuiltInCategory.OST_TitleBlocks);

            // Retrieve title blocks from the collector
            IEnumerable<FamilySymbol> titleBlocks = collector.Cast<FamilySymbol>();

            // Loop through the retrieved title blocks
            foreach (FamilySymbol titleBlock in titleBlocks)
            {
                // Access the properties of the title block
                string titleBlockName = titleBlock.Name;
                ElementId titleBlockId = titleBlock.Id;

                list.Add (titleBlockName);
            }

            return list;
        }
    }
}



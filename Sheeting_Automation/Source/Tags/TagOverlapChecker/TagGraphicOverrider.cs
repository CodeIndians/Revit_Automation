using Autodesk.Revit.DB;
using Sheeting_Automation.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    public static class TagGraphicOverrider
    {
        public static void CreateOverrides(List<ElementId> elementIds)
        {
            // nothing to override
            if(elementIds == null || elementIds.Count == 0)
                return;

            OverrideGraphicSettings graphicSettings = new OverrideGraphicSettings();

            FilteredElementCollector elements = new FilteredElementCollector(SheetUtils.m_Document);
            FillPatternElement fillPatternElement = elements.OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);

            graphicSettings.SetSurfaceForegroundPatternColor(new Color(255, 0, 0));
            graphicSettings.SetSurfaceForegroundPatternId(fillPatternElement.Id);

            graphicSettings.SetCutForegroundPatternColor(new Color(255, 0, 0));
            graphicSettings.SetCutForegroundPatternId(fillPatternElement.Id);

            graphicSettings.SetCutLineColor(new Color(255, 0, 0));
            //graphicSettings.SetCutLinePatternId(fillPatternElement.Id);

            graphicSettings.SetProjectionLineColor(new Color(255, 0, 0));
            //graphicSettings.SetProjectionLinePatternId(fillPatternElement.Id);
            
            using (Transaction trans = new Transaction(SheetUtils.m_Document, "Set Overrides"))
            {
                trans.Start();

                foreach(ElementId elementId in elementIds)
                {
                    SheetUtils.m_Document.ActiveView.SetElementOverrides(elementId, graphicSettings);
                }

                trans.Commit();
            }

        }

        public static void DeleteOverrides(List<ElementId> elementIds) 
        {
            // nothing to reset
            if (elementIds == null || elementIds.Count == 0)
                return;

            OverrideGraphicSettings graphicSettings = new OverrideGraphicSettings();

            using (Transaction trans = new Transaction(SheetUtils.m_Document, "Set Overrides"))
            {
                trans.Start();

                foreach (ElementId elementId in elementIds)
                {
                    SheetUtils.m_Document.ActiveView.SetElementOverrides(elementId, graphicSettings);
                }

                trans.Commit();
            }
        }
    }
}

﻿using Autodesk.Revit.DB;
using Sheeting_Automation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    public class Tag2GenModelOverlap : TagOverlapBase
    {
        /// <summary>
        /// Get all the generic model element ids in the current view
        /// </summary>
        /// <returns></returns>
        protected override List<ElementId> GetElementIds()
        {
            List<ElementId> elementIds = new List<ElementId>();

            // Create a filtered element collector 
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_ActiveViewId);

            // Filter for elements of category generic model
            collector.OfCategory(BuiltInCategory.OST_GenericModel);

            foreach (Element element in collector)
            {
                elementIds.Add(element.Id);
            }

            return elementIds;
        }
    }
}

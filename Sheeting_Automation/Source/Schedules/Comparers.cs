﻿using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace Sheeting_Automation.Source.Schedules
{
    /// <summary>
    /// Category comparer to distinctly identify categories
    /// </summary>
    internal class CategoryComparer : IEqualityComparer<Category>
    {
        public bool Equals(Category x, Category y)
        {
            if (x == null || y == null) return false;

            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(Category obj)
        {
            return obj.Id.IntegerValue;
        }
    }
}

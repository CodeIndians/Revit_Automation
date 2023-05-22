
/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation
{
    ///<summary>
    /// This Class is used to read the in-built symbol families and select those necessary for placement in the model
    ///</summary>
    internal class SymbolCollector
    {
        public static FilteredElementCollector CollSymbols;

        public static void CollectColumnSymbols(Document doc) {
            if (CollSymbols == null)
            {
                SymbolCollector.CollSymbols = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralColumns);
            }
        }
        static public FamilySymbol GetSymbol( string strSymbolName, string strFamilyName)
        {
            FamilySymbol symbol = null;

            foreach (FamilySymbol famSymbol in CollSymbols)
            {
                if (famSymbol.FamilyName == strFamilyName && famSymbol.Name == strSymbolName)
                {
                    symbol = famSymbol;
                    break;
                }
            }
            return symbol;

        }
    }
}


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
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Revit_Automation
{
    ///<summary>
    /// This Class is used to read the in-built symbol families and select those necessary for placement in the model
    ///</summary>
    public class SymbolCollector
    {
        public enum FamilySymbolType
        {
            posts = 0, 
            bottomTracks = 1, 
            Beams
        }

        public static FilteredElementCollector CollSymbols;

        public static FilteredElementCollector WallSymbols;

        public static void CollectColumnSymbols(Document doc)
        {
            if (CollSymbols == null)
            {
                SymbolCollector.CollSymbols = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralColumns);
            }
        }

        internal static void CollectWallSymbols(Document doc)
        {
            SymbolCollector.WallSymbols = new FilteredElementCollector(doc);
            WallSymbols.OfClass(typeof(WallType));
        }

        public static FamilySymbol GetSymbol(string strSymbolName, string strFamilyName, FamilySymbolType famSymType)
        {
            FamilySymbol symbol = null;

            if (famSymType == FamilySymbolType.posts)
            {
                foreach (FamilySymbol famSymbol in CollSymbols)
                {
                    if (famSymbol.FamilyName == strFamilyName && famSymbol.Name == strSymbolName)
                    {
                        symbol = famSymbol;
                        break;
                    }
                }
            }

            return symbol;

        }

        public static WallType GetWall(string strSymbolName, string strFamilyName)
        {
            WallType wallType = null;

            foreach (WallType wall in WallSymbols)
            {
                if (wall.Name == strSymbolName && wall.FamilyName == strFamilyName)
                {
                    wallType = wall;
                    break;
                }
            }

            return wallType;
        }
    }
}

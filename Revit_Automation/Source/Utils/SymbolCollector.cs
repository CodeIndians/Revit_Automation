
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
        public static Document m_Document;

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

        public static FamilySymbol GetProjectSpecificationLineSymbol()
        {
            FamilySymbol familySymbol = null;
            FilteredElementCollector genericLineCollection = new FilteredElementCollector(m_Document);

            genericLineCollection.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_GenericModel);

            foreach (FamilySymbol famSymbol in genericLineCollection)
            {
                if (famSymbol.FamilyName == "Project Settings Line" && famSymbol.Name == "Project Settings Line")
                {
                    familySymbol = famSymbol;
                    break;
                }
            }

            return familySymbol;
        }

        public static FamilySymbol GetBottomOrTopTrackSymbol(string strSymbolName, string strFamilyName)
        {
            FamilySymbol familySymbol = null;
            FilteredElementCollector structFramingCollection = new FilteredElementCollector(m_Document);

            structFramingCollection.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralFraming);

            foreach (FamilySymbol famSymbol in structFramingCollection)
            {
                if (famSymbol.FamilyName == strFamilyName && famSymbol.Name == strSymbolName)
                {
                    familySymbol = famSymbol;
                    break;
                }
            }

            return familySymbol;
        }
        public static FamilySymbol GetInputLineSymbol()
        {
            FamilySymbol familySymbol = null;
            FilteredElementCollector genericLineCollection = new FilteredElementCollector(m_Document);

            genericLineCollection.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_GenericModel);

            foreach (FamilySymbol famSymbol in genericLineCollection)
            {
                if (famSymbol.FamilyName == "Line" && famSymbol.Name == "Line")
                {
                    familySymbol = famSymbol;
                    break;
                }
            }

            return familySymbol;
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

        public static List<string> GetCeeHeaders()
        {
            List<string> headers = new List<string>();

            FamilySymbol familySymbol = null;
            FilteredElementCollector genericLineCollection = new FilteredElementCollector(m_Document);

            genericLineCollection.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralFraming);

            foreach (FamilySymbol famSymbol in genericLineCollection)
            {
                if (famSymbol.FamilyName == "Cee Header")
                {
                    headers.Add(famSymbol.Name);
                }
            }

            return headers;
        }

        internal static FamilySymbol GetCeeHeadersFamily(string CeeHeaderName, string v)
        {
            FamilySymbol familySymbol = null;
            FilteredElementCollector ceeHeaderFamilies = new FilteredElementCollector(m_Document);

            ceeHeaderFamilies.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralFraming);

            foreach (FamilySymbol famSymbol in ceeHeaderFamilies)
            {
                if (famSymbol.FamilyName == v && famSymbol.Name == CeeHeaderName)
                {
                    return famSymbol;
                }
            }
            return null;
        }

        internal static FamilySymbol GetCompositeDeckSymbol(string deckName, string deckFamiyName)
        {
            FilteredElementCollector families = new FilteredElementCollector(m_Document);

            families.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralFraming);

            foreach (FamilySymbol famSymbol in families)
            {
                if (famSymbol.FamilyName == deckFamiyName && famSymbol.Name == deckName)
                {
                    return famSymbol;
                }
            }
            return null;
        }

        internal static FamilySymbol GetVoidFamilySymbol()
        {
            FilteredElementCollector families = new FilteredElementCollector(m_Document);

            families.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_GenericModel);

            foreach (FamilySymbol famSymbol in families)
            {
                if (famSymbol.FamilyName == "Roof Deck Void" && famSymbol.Name == "Void")
                {
                    return famSymbol;
                }
            }
            return null;
        }

        internal static IList<string> GetDeckNames()
        {
            IList<string> lstDeckName = new List<string>(); 
            FilteredElementCollector families = new FilteredElementCollector(m_Document);

            families.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralFraming);

            foreach (FamilySymbol famSymbol in families)
            {
                if (famSymbol.FamilyName == "Composite Deck" )
                {
                    lstDeckName.Add(famSymbol.Name);
                }
            }
            return lstDeckName;
        }

        internal static List<string> GetPurlinSymbols()
        {
            List<string> lstPurlinTypes = new List<string>();
            FilteredElementCollector families = new FilteredElementCollector(m_Document);

            families.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralFraming);

            foreach (FamilySymbol famSymbol in families)
            {
                if (famSymbol.FamilyName.Contains("Purlin"))
                {
                    lstPurlinTypes.Add(famSymbol.Name);
                }
            }
            return lstPurlinTypes;
        }

        internal static List<string> GetReceiverChannelSymbols()
        {
            List<string> lstReceiverChannelTypes = new List<string>();
            FilteredElementCollector families = new FilteredElementCollector(m_Document);

            families.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralFraming);

            foreach (FamilySymbol famSymbol in families)
            {
                if (famSymbol.FamilyName.Contains("Receiver channel"))
                {
                    lstReceiverChannelTypes.Add(famSymbol.Name);
                }
            }
            return lstReceiverChannelTypes;
        }

        internal static List<string> GetDragStrutTypes()
        {
            List<string> lstDragStrutTypes = new List<string>();
            FilteredElementCollector families = new FilteredElementCollector(m_Document);

            families.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralFraming);

            foreach (FamilySymbol famSymbol in families)
            {
                if (famSymbol.FamilyName.Contains("Drag Strut"))
                {
                    lstDragStrutTypes.Add(famSymbol.Name);
                }
            }
            return lstDragStrutTypes;
        }

        internal static List<string> GetEaveStrutTypes()
        {
            List<string> lstEaveStrutTypes = new List<string>();
            FilteredElementCollector families = new FilteredElementCollector(m_Document);

            families.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralFraming);

            foreach (FamilySymbol famSymbol in families)
            {
                if (famSymbol.FamilyName.Contains("Eave Strut"))
                {
                    lstEaveStrutTypes.Add(famSymbol.Name);
                }
            }
            return lstEaveStrutTypes;
        }

        internal static List<string> GetCompositeDeckTypes()
        {
            List<string> lstCompositeDeckTypes = new List<string>();
            FilteredElementCollector families = new FilteredElementCollector(m_Document);

            families.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralFraming);

            foreach (FamilySymbol famSymbol in families)
            {
                if (famSymbol.FamilyName.Contains("Composite Deck"))
                {
                    lstCompositeDeckTypes.Add(famSymbol.Name);
                }
            }
            return lstCompositeDeckTypes;
        }

        internal static List<string> GetRoofDeckTypes()
        {
            List<string> lstRoofDeckTypes = new List<string>();
            FilteredElementCollector families = new FilteredElementCollector(m_Document);

            families.OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralFraming);

            foreach (FamilySymbol famSymbol in families)
            {
                if (famSymbol.FamilyName.Contains("Roof Deck"))
                {
                    lstRoofDeckTypes.Add(famSymbol.Name);
                }
            }
            return lstRoofDeckTypes;
        }


    }
}

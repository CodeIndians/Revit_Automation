
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
        static public FamilySymbol T62columnType = null;
        static public FamilySymbol T62FlushTopFlushBottom = null;
        static public FamilySymbol T62FlushBottomFemaletop = null;
        static public FamilySymbol T62FemaleTopMaleBottom = null;
        static public FamilySymbol T62FlushTopMaleBottom = null;

        /// <summary>
        /// This method is used to collect and store the required symbols from the in-built families
        /// </summary>
        /// <param name="doc"> The pointer to the active document that is opened in Revit</param>
        static public void CollectColumnSymbols(Document doc)
        {
            string filePath = "C:\\temp\\example.txt"; // Path to the file to be created


            FamilySymbol StudColumnType = null;
            FilteredElementCollector coll = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_StructuralColumns);

            foreach (FamilySymbol symbol in coll)
            {
                if (symbol.FamilyName == "T62")
                {
                    if (symbol.Name == "4\" x 4\" x 2\" (Flush Bottom / Female Top)")
                        T62FlushBottomFemaletop = symbol;

                    if (symbol.Name == "4\" x 4\" x 2 1/2\" (Male Bottom / Female Top)")
                        T62FemaleTopMaleBottom = symbol;

                    if (symbol.Name == "4\" x 4\" x 2\" (Male Bottom / Flush Top)")

                        using (StreamWriter writer = new StreamWriter(filePath, true))
                        {
                            writer.WriteLine(symbol.Name);
                            writer.Close();
                        }
                }

                if (symbol.FamilyName == "Post")
                {
                    StudColumnType = symbol;
                }
            }
        }
    }
}

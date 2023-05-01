using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation
{
    internal class SymbolCollector
    {
        static public FamilySymbol T62columnType = null;
        static public FamilySymbol T62FlushTopFlushBottom = null;
        static public FamilySymbol T62FlushBottomFemaletop = null;
        static public FamilySymbol T62FemaleTopMaleBottom = null;
        static public FamilySymbol T62FlushTopMaleBottom = null;

        static public void CollectColumns(Document doc)
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

using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Revit_Automation
{
    internal class CDeckTrimmer
    {
        private Document doc;
        private Form1 form;

        public CDeckTrimmer(Document doc, Form1 form)
        {
            this.doc = doc;
            this.form = form;
        }
         
        internal void CreateModel(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            using (Transaction tx = new Transaction(doc))
            {
                form.PostMessage("");
                form.PostMessage($"\n Starting Trimming of Decks");
                tx.Start("Trimming Decks");
                IList<Element> framingElements = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_StructuralFraming).ToElements();

                IList<string> deckNames = SymbolCollector.GetDeckNames();
                IList<Element> deckElements = framingElements.Where(fe => deckNames.Contains(fe.Name)).ToList();

                foreach (Element deckElem in deckElements)
                {
                    BoundingBoxXYZ bb = deckElem.get_BoundingBox(doc.ActiveView);
                    Outline outline = new Outline(bb.Min, bb.Max);

                    BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

                    IList<Element> genericModelElements = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();
                    IList<Element> voidElements = genericModelElements.Where(fe => fe.Name == "Void").ToList();

                    foreach (Element voidElem in voidElements)
                    {
                        InstanceVoidCutUtils.AddInstanceVoidCut(doc, deckElem, voidElem);
                        
                    }
                }
                form.PostMessage($"\n Completed Trimming of Decks");
                tx.Commit();
            }
        }
    }
}
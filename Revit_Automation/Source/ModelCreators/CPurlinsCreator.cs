using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Revit_Automation
{
    internal class CPurlinsCreator
    {
        private Document doc;
        private Form1 form;

        public CPurlinsCreator(Document doc, Form1 form)
        {
            this.doc = doc;
            this.form = form;
        }

        internal void CreateModel(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            
        }
    }
}
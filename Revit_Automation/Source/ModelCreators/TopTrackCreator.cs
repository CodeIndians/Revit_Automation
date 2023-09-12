using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Revit_Automation
{
    internal class TopTrackCreator
    {
        private Document doc;
        private Form1 form;

        public TopTrackCreator(Document doc, Form1 form)
        {
            this.doc = doc;
            this.form = form;
        }

        internal void CreateModel(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            //throw new NotImplementedException();
        }
    }
}
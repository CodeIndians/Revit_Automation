using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    public class TagOverlapBase
    {
        protected List<IndependentTag> m_IndependentTags;
        public TagOverlapBase() 
        {
            m_IndependentTags = TagUtils.GetAllTagsInView();
        }

        public virtual List<ElementId> CheckOverlap()
        {
            return null;
        }
        
    }
}

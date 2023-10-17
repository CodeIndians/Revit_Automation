using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sheeting_Automation.Source.Tags.TagData;

namespace Sheeting_Automation.Source.Tags.TagCreate.TagResolver
{
    public class TagResolverBase
    {
       protected virtual List<Tag> ResolveTagList(List<Tag> tagList)
       {
            return tagList;
       }
    }
}

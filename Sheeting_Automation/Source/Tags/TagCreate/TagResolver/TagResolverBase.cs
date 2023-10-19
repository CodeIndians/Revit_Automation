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
        /// <summary>
        /// Each derived class should have its own implementation
        /// on how the overlaps are resolved
        /// </summary>
        /// <param name="tagList"></param>
        /// <returns>list of tags </returns>
        protected virtual List<Tag> ResolveTagList(List<Tag> tagList, ref List<List<Tag>> overlapTagsList)
        {
            return tagList;
        }

        /// <summary>
        /// calls ResolveTagList on each of the overlapping list 
        /// </summary>
        /// <param name="overlapTagsList"></param>
        public void Resolve(ref List<List<Tag>> overlapTagsList)
        {
            for (int i = 0; i < overlapTagsList.Count; i++)
            {
                if (overlapTagsList[i].Count > 1)
                {
                    overlapTagsList[i] = ResolveTagList(overlapTagsList[i],ref overlapTagsList);
                }
            }
        }
    }
}

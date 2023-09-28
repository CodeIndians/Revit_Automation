using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace Sheeting_Automation.Source.Tags.TagOverlapChecker
{
    public class Tag2TagOverlap : TagOverlapBase
    {
        public override List<ElementId> CheckOverlap()
        {
            List<ElementId> elementIds = new List<ElementId>();

            for(int i = 0; i < m_IndependentTags.Count - 1; i++)
            {
                for (int j = i +1;  j < m_IndependentTags.Count; j++)
                {
                    if (TagUtils.AreTagsIntersecting(m_IndependentTags[i], m_IndependentTags[j]))
                    {
                        if (!elementIds.Contains(m_IndependentTags[i].Id))
                        {
                            elementIds.Add(m_IndependentTags[i].Id);
                            elementIds.AddRange(m_IndependentTags[i].GetTaggedLocalElementIds());

                            var indexDiff = m_IndependentTags.Count - m_TagsWithLeaders.Count;
                            if (i >= indexDiff)
                                elementIds.Add(m_TagsWithLeaders[i - indexDiff].Id);
                        }

                        if (!elementIds.Contains(m_IndependentTags[j].Id))
                        {
                            elementIds.Add(m_IndependentTags[j].Id);
                            elementIds.AddRange(m_IndependentTags[j].GetTaggedLocalElementIds());

                            var indexDiff = m_IndependentTags.Count - m_TagsWithLeaders.Count;
                            if (j >= indexDiff)
                                elementIds.Add(m_TagsWithLeaders[j - indexDiff].Id);
                        }
                    }
                }
            }

            return elementIds;
        }
    }
}

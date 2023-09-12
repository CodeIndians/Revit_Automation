using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.ModelCreators
{
    internal class CeeHeaderAdjustment
    {
        private Document m_Document;
        private List<CeeHeaderAdjustments> lstCeeheaderAdjustments;
        private string m_strPhaseName = "";
        public CeeHeaderAdjustment(Document doc, List <CeeHeaderAdjustments> lst) 
        {
            m_Document = doc;
            lstCeeheaderAdjustments = lst;
        }
        public void AdjustHeaders()
        {
            // identify the single and double headers separately
            Dictionary<XYZ, XYZ> doubleHeaderCoordinates = new Dictionary<XYZ, XYZ>();
            Dictionary<XYZ, XYZ> singleHeaderCoordinates = new Dictionary<XYZ, XYZ>();

            foreach (CeeHeaderAdjustments ceeHeadersAdjust in lstCeeheaderAdjustments)
            {
                doubleHeaderCoordinates.Clear();
                singleHeaderCoordinates.Clear();

                FilteredElementCollector framingElements
                  = new FilteredElementCollector(m_Document, m_Document.ActiveView.Id)
                    .WhereElementIsNotElementType()
                    .OfCategory(BuiltInCategory.OST_StructuralFraming);

                List<Element> ceeHeaderElements = new List<Element>();

                // Gather all Cee Headers with given name
                foreach (Element element in framingElements)
                {
                    if (element is FamilyInstance familyInstance)
                    {
                        if (familyInstance.Symbol.Name == ceeHeadersAdjust.strCeeHeaderName)
                            ceeHeaderElements.Add(element);
                    }
                }

                if (ceeHeaderElements.Count > 0)
                {
                    Parameter phaseCreated = ceeHeaderElements[0].get_Parameter(BuiltInParameter.PHASE_CREATED);
                    m_strPhaseName = phaseCreated.AsValueString();
                }

                while (ceeHeaderElements.Count > 0)
                {
                    int iMatchIndex = -1;

                    bool bMatchFound = false;
                    XYZ startPt = null, endPt = null;
                    GenericUtils.GetlineStartAndEndPoints(ceeHeaderElements[0], out startPt, out endPt);

                    for (int j = 1; j < ceeHeaderElements.Count; j++)
                    {
                        XYZ matchStart = null, matchEnd = null;
                        GenericUtils.GetlineStartAndEndPoints(ceeHeaderElements[j], out matchStart, out matchEnd);

                        if (MathUtils.ApproximatelyEqual(startPt.X, matchStart.X)
                            && MathUtils.ApproximatelyEqual(startPt.Y, matchStart.Y)
                            && MathUtils.ApproximatelyEqual(endPt.X, matchEnd.X)
                            && MathUtils.ApproximatelyEqual(endPt.Y, matchEnd.Y))
                        {
                            bMatchFound = true;
                            doubleHeaderCoordinates.Add(startPt, endPt);
                            iMatchIndex = j;
                            break;
                        }
                    }

                    if (!bMatchFound)
                    {
                        singleHeaderCoordinates.Add(startPt, endPt); 
                    }
                    
                    // First delete the second element and then first, else the list indices will vary and delete unintended elements. 
                    if (iMatchIndex != -1)
                        ceeHeaderElements.RemoveAt(iMatchIndex);

                    ceeHeaderElements.RemoveAt(0);
                }

                Dictionary<XYZ, XYZ> selectedList = ceeHeadersAdjust.iCeeHeaderCount == 2 ? doubleHeaderCoordinates : singleHeaderCoordinates;

                foreach (KeyValuePair<XYZ, XYZ> kvp in selectedList)
                {
                    XYZ CeeHeaderStartPt = kvp.Key;
                    XYZ CeeHeaderEndPt  = kvp.Value;

                    // Cee Headerpoint is near the top floor, we have to adjust it to the base level
                    Level baseLevel = null, topLevel = null;
                    AdjustLevelOfthePoint(ref CeeHeaderStartPt, out baseLevel, out topLevel);
                    AdjustLevelOfthePoint(ref CeeHeaderEndPt, out baseLevel, out topLevel);

                    PostCreationUtils.PlaceStudForCeeHeader(m_Document, CeeHeaderStartPt, CeeHeaderEndPt, ceeHeadersAdjust.postType, ceeHeadersAdjust.postGuage, ceeHeadersAdjust.postCount, topLevel, baseLevel);
                }
            }
        }

        private void AdjustLevelOfthePoint(ref XYZ ceeHeaderStartPt, out Level baseLevel, out Level topLevel)
        {
            baseLevel = null ; topLevel = null;

            IOrderedEnumerable<Level> levels = new FilteredElementCollector(m_Document)
                                                .WherePasses(new ElementClassFilter(typeof(Level), false))
                                                .Cast<Level>()
                                                .OrderBy(e => e.Elevation);
            
            // Filter levels based on buldings to use
            List<Level> filteredLevels = new List<Level>();
            foreach (Level filteredlevel in levels)
            {
                if (filteredlevel.Name.Contains(m_strPhaseName))
                {
                    filteredLevels.Add(filteredlevel);
                }
            }

            for (int i = 0; i < filteredLevels.Count() - 1; i++)
            {
                Level tempLevel = filteredLevels.ElementAt(i);

                if ((ceeHeaderStartPt.Z < (tempLevel.Elevation + 1)) && (ceeHeaderStartPt.Z > (tempLevel.Elevation - 1)))
                {
                    topLevel = tempLevel;
                    baseLevel = filteredLevels.ElementAt(i - 1);
                    ceeHeaderStartPt = new XYZ(ceeHeaderStartPt.X, ceeHeaderStartPt.Y, baseLevel.Elevation);
                    break;
                }
            }
        }
    }
}

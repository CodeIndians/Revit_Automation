using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Utils
{
    public class WarningSwallowers
    {
        public class DuplicateColumnWarningSwallower : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                IList<FailureMessageAccessor> failList = new List<FailureMessageAccessor>();
                
                // Inside event handler, get all warnings
                failList = failuresAccessor.GetFailureMessages();
                foreach (FailureMessageAccessor failure in failList)
                {
                    // check FailureDefinitionIds against ones that you want to dismiss, 
                    FailureDefinitionId failID = failure.GetFailureDefinitionId();
                    
                    // prevent Revit from showing Unenclosed room warnings
                    if (failID == BuiltInFailures.OverlapFailures.DuplicateInstances || failID == BuiltInFailures.ColumnFailures.ColumnJoinNonhitFailure)
                    {
                        failuresAccessor.DeleteWarning(failure);
                    }
                }

                return FailureProcessingResult.Continue;
            }
        }
    }
}

using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace Revit_Automation.Source.Utils
{
    public class WarningSwallowers
    {
        public class DuplicateColumnWarningSwallower : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                _ = new List<FailureMessageAccessor>();

                // Inside event handler, get all warnings
                IList<FailureMessageAccessor> failList = failuresAccessor.GetFailureMessages();
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

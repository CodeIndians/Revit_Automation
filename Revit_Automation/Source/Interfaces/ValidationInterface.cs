using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source
{
    public interface IValidationInterface
    {
        bool ValidateCondition(int iConditionID);
    }
}

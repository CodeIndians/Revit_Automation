using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.ModelCreators
{
    public class T62Valiations : IValidationInterface
    {
        public T62Valiations() { }
        public bool ValidateCondition(int iConditionID)
        {
            return true;
        }
    }
}

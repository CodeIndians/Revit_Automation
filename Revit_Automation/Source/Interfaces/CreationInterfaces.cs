using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;

namespace Revit_Automation.Source
{
    public interface IModelCreator
    {
        void CreateModel(List<CustomTypes.InputLine> colInputLines, IOrderedEnumerable<Level> levels);

    }


}

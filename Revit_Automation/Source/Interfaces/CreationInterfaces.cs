using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace Revit_Automation.Source
{
    public interface IModelCreator
    {
        void CreateModel(List<CustomTypes.InputLine> colInputLines, IOrderedEnumerable<Level> levels);

    }


}

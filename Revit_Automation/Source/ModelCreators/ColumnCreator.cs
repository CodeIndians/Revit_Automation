using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.ModelCreators
{
    public class ColumnCreator : IModelCreator
    {
        public ColumnCreator() { }
        public void CreateModel(List<CustomTypes.InputLine> colInputLines, IOrderedEnumerable<Level> levels) 
        {
            ProcessInputLines(colInputLines, levels);
        }
        private  void ProcessInputLines(List<InputLine> inputLinesCollection, IOrderedEnumerable<Level> levels)
        {
            foreach (InputLine inputLine in inputLinesCollection)
            {
                if (!string.IsNullOrEmpty(inputLine.strT62Guage) && !string.IsNullOrEmpty(inputLine.strStudGuage))
                {
                    ProcessT62AndStudLine(inputLine, levels);
                }
                else if (!string.IsNullOrEmpty(inputLine.strT62Guage))
                {
                    ProcessT62InputLine(inputLine, levels);
                }
                else if (!string.IsNullOrEmpty(inputLine.strStudGuage))
                {
                    ProcessStudInputLine(inputLine, levels);
                }
            }
        }

        private  void ProcessStudInputLine(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
            // Identify the level of the line - To obtain the bottom and top level


        }

        private  void ProcessT62InputLine(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {

        }

        private  void ProcessT62AndStudLine(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {

        }

    }
}

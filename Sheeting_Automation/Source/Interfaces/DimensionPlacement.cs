using Autodesk.Revit.DB;
using System.Collections.Generic;


namespace Sheeting_Automation.Source.Interfaces
{
    internal abstract class DimensionPlacement
    {
        public abstract void AddDimensionLinesDiscrete();

        public abstract void AddDimensionLinesContinuous();

        public List<DimensionLine> DimensionLines;

        public struct DimensionLine
        {
            public Line line;
            public ReferenceArray referencesArray;
        }

        // discrete and continuous dim lines lists are initiated here
        public DimensionPlacement()
        {
            DimensionLines = new List<DimensionLine>();
        }

        public virtual void InitializeDimensionLines()
        {
            AddDimensionLinesDiscrete();
            AddDimensionLinesContinuous();
        }
    }
}

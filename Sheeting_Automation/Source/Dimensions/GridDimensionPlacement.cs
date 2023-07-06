using Autodesk.Revit.DB;
using Sheeting_Automation.Source.GeometryCollectors;
using Sheeting_Automation.Source.Interfaces;
using System;
using System.Collections.Generic;


namespace Sheeting_Automation.Source.Dimensions
{
    internal class GridDimensionPlacement: DimensionPlacement
    {
        GridCollector mGridCollector;
        CropRegionCollector mCropRegionCollector;
        float mOffset;

        public GridDimensionPlacement(GridCollector gridCollector, CropRegionCollector cropRegionCollector) 
        {
            mGridCollector = gridCollector;
            mCropRegionCollector = cropRegionCollector;
            mOffset = 2;

            InitializeDimensionLines();
        }

        public override void AddDimensionLinesDiscrete()
        {

            // second level - left side
            var dimensionXPOS = mCropRegionCollector.minX - 2 * mOffset;

            // second level from top 
            var dimensionYPOS = mCropRegionCollector.maxY + 2 * mOffset;

            // add discrete dimensions for horizontal grids
            for (var i = 0; i < mGridCollector.HorizontalLines.Count - 1; i++)
            {
                var name1 = mGridCollector.HorizontalLines[i].name;

                // Do not compute the dimensions again for main or half grids
                if (IsNameMainorHalf(name1))
                    continue;


                var beforeLine = mGridCollector.HorizontalLines[i - 1];
                var currentLine = mGridCollector.HorizontalLines[i];
                var afterLine = mGridCollector.HorizontalLines[i + 1];

                var computeLine = (Math.Abs(currentLine.start.Y - beforeLine.start.Y) < Math.Abs(currentLine.start.Y - afterLine.start.Y)) ? beforeLine : afterLine;


                XYZ lineStart = new XYZ(dimensionXPOS, currentLine.start.Y, currentLine.start.Z);
                XYZ lineEnd = new XYZ(dimensionXPOS, computeLine.start.Y, computeLine.start.Z);

                
                DimensionLine gridDimLIne = new DimensionLine();
                gridDimLIne.line = Line.CreateBound(lineStart, lineEnd);


                gridDimLIne.referencesArray = new ReferenceArray();
                gridDimLIne.referencesArray.Append(currentLine.gridReference);
                gridDimLIne.referencesArray.Append(computeLine.gridReference);
                
                DimensionLines.Add(gridDimLIne);
            }

            // add discrete dimensions for vertical grids
            for (var i = 0; i < mGridCollector.VerticalLines.Count - 1; i++)
            {
                var name1 = mGridCollector.VerticalLines[i].name;

                // Do not compute the dimensions again for main or half grids
                if (IsNameMainorHalf(name1))
                    continue;


                var beforeLine = mGridCollector.VerticalLines[i - 1];
                var currentLine = mGridCollector.VerticalLines[i];
                var afterLine = mGridCollector.VerticalLines[i + 1];

                var computeLine = (Math.Abs(currentLine.start.X - beforeLine.start.X) < Math.Abs(currentLine.start.X - afterLine.start.X)) ? beforeLine : afterLine;


                XYZ lineStart = new XYZ(currentLine.start.X, dimensionYPOS, currentLine.start.Z);
                XYZ lineEnd = new XYZ(computeLine.start.X, dimensionYPOS, computeLine.start.Z);


                DimensionLine gridDimLIne = new DimensionLine();
                gridDimLIne.line = Line.CreateBound(lineStart, lineEnd);


                gridDimLIne.referencesArray = new ReferenceArray();
                gridDimLIne.referencesArray.Append(currentLine.gridReference);
                gridDimLIne.referencesArray.Append(computeLine.gridReference);

                DimensionLines.Add(gridDimLIne);
            }
        }

        public override void AddDimensionLinesContinuous()
        {
            // fourth level -  left side
            var dimensionXPOS = mCropRegionCollector.minX - 4 * mOffset;
            
            // fourth level - from top
            var dimensionYPOS = mCropRegionCollector.maxY + 4 * mOffset;
            
            // Add dimension for horizontal grids
            XYZ lineStartX = new XYZ(dimensionXPOS, mGridCollector.HorizontalLines[0].start.Y, mGridCollector.HorizontalLines[0].start.Z);
            XYZ lineEndX = new XYZ(dimensionXPOS, mGridCollector.HorizontalLines[mGridCollector.HorizontalLines.Count -1].start.Y, mGridCollector.HorizontalLines[mGridCollector.HorizontalLines.Count - 1].start.Z);

            DimensionLine gridDimLIneX = new DimensionLine();
            gridDimLIneX.line = Line.CreateBound(lineStartX, lineEndX);

            gridDimLIneX.referencesArray = new ReferenceArray();
            for (var i = 0; i < mGridCollector.HorizontalLines.Count; i++)
            {
                var name = mGridCollector.HorizontalLines[i].name;

                // only consider A,A.5,B,B.5 and so on
                if (IsNameMainorHalf(name))
                {
                    gridDimLIneX.referencesArray.Append(mGridCollector.HorizontalLines[i].gridReference);
                }
            }
            DimensionLines.Add(gridDimLIneX);


            // Add dimensions for vertical grid
            XYZ lineStartY = new XYZ(mGridCollector.VerticalLines[0].start.X, dimensionYPOS, mGridCollector.VerticalLines[0].start.Z);
            XYZ lineEndY = new XYZ(mGridCollector.VerticalLines[mGridCollector.VerticalLines.Count - 1].start.X, dimensionYPOS, mGridCollector.VerticalLines[mGridCollector.VerticalLines.Count - 1].start.Z);

            DimensionLine gridDimLIneY = new DimensionLine();
            gridDimLIneY.line = Line.CreateBound(lineStartY, lineEndY);

            gridDimLIneY.referencesArray = new ReferenceArray();
            for (var i = 0; i < mGridCollector.VerticalLines.Count; i++)
            {
                var name = mGridCollector.VerticalLines[i].name;

                // only consider A,A.5,B,B.5 and so on
                if (IsNameMainorHalf(name))
                {
                    gridDimLIneY.referencesArray.Append(mGridCollector.VerticalLines[i].gridReference);
                }
            }
            DimensionLines.Add(gridDimLIneY);
        }

        private bool IsNameMainorHalf(string name)
        {
            bool isNameMainorHalf = true;
            if (name.EndsWith(".1") || name.EndsWith(".2") || name.EndsWith(".3") || name.EndsWith(".4") || name.EndsWith(".6") || name.EndsWith(".7") || name.EndsWith(".8") || name.EndsWith(".9"))
                isNameMainorHalf = false;
            return isNameMainorHalf;
        }
    }
}

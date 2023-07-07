using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.GeometryCollectors
{
    internal class CropRegionCollector
    {
        public struct BoundingLine
        {
            public XYZ start;
            public XYZ end;
            public BoundingLine(XYZ s, XYZ e)
            {
                start = s;
                end = e;
            }
        }

        public double minX;
        public double minY;
        public double maxX;
        public double maxY;

        private readonly Document mDocument;

        public List<BoundingLine> CropLinesList;
        public CropRegionCollector(ref Document document)
        {
            mDocument = document;

            CropLinesList = new List<BoundingLine>();

            minX = double.PositiveInfinity;
            minY = double.PositiveInfinity;
            maxX = double.NegativeInfinity;
            maxY = double.NegativeInfinity;

            // updat the crop region bounding lines 
            UpdateCropRegionBoundingLines(mDocument.ActiveView);

            //compute min and max points 
            ComputeMinMax();
        }

        private void UpdateCropRegionBoundingLines(View view)
        {
            // clear if any lines are present earlier
            CropLinesList.Clear();

            // check if the requested view is viewplan
            if (view is ViewPlan)
            {
                // get the crop region shape manager object 
                ViewCropRegionShapeManager vcrShapeMgr=  view.GetCropRegionShapeManager();

                // collect all the shapes 
                var shapes = vcrShapeMgr.GetCropShape();

                // Assuming that each plan will have a single crop region
                foreach (var shape in shapes)
                {
                    var lines = shape.ToList();

                    // add start and end points of each of the crop region bound 
                    foreach (var line in lines)
                    {
                        CropLinesList.Add(new BoundingLine(line.GetEndPoint(0), line.GetEndPoint(1)));
                    }
                }

                // write the data to file 
                WriteCropRegionToFile(@"C:\temp\crop.txt");
            }
        }

        private void WriteCropRegionToFile(string filePath)
        {
            // Create a StringBuilder to hold the CSV data
            StringBuilder sb = new StringBuilder();

            foreach (var cropLine in CropLinesList)
            {
                sb.AppendLine($" start = {cropLine.start.X},{cropLine.start.Y},{cropLine.start.Z} , end = {cropLine.end.X},{cropLine.end.Y},{cropLine.end.Z} ");
            }
            File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// Compute the min and max points
        /// </summary>
        private void ComputeMinMax()
        {
            foreach (var line in CropLinesList)
            {
                double startX = line.start.X;
                double startY = line.start.Y;
                double endX = line.end.X;
                double endY = line.end.Y;

                minX = Math.Min(minX, Math.Min(startX, endX));
                minY = Math.Min(minY, Math.Min(startY, endY));
                maxX = Math.Max(maxX, Math.Max(startX, endX));
                maxY = Math.Max(maxY, Math.Max(startY, endY));
            }
        }
    }
}

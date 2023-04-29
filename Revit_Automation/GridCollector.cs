using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Document = Autodesk.Revit.DB.Document;

namespace Revit_Automation
{
    internal class GridCollector
    {
        private readonly Document mDocument;
        private ICollection<Element> mGridCollection;
        private List<Tuple<XYZ, XYZ>> mHorizontalLines;
        private List<Tuple<XYZ, XYZ>> mVerticalLines;

        public ICollection<Element> GridCollection { get { return mGridCollection; } }
        public List<Tuple<XYZ, XYZ>> HorizontalLines { get { return mHorizontalLines; } }
        public List<Tuple<XYZ, XYZ>> VerticalLines { get { return mVerticalLines; } }


        public GridCollector(Document doc) 
        {
            mDocument = doc;
            Initialize();
        }
        private void Initialize()
        {
            FilteredElementCollector gridCollector = new FilteredElementCollector(mDocument);
            mGridCollection = gridCollector.OfCategory(BuiltInCategory.OST_Grids).ToElements();

            List<Tuple<XYZ, XYZ>> gridLines = new List<Tuple<XYZ, XYZ>>();

            // collect each line into a gridline tuple 
            foreach (Element element in mGridCollection)
            {
                Parameter archParameter = element.LookupParameter("Arch Grid");

                if(archParameter != null)
                {
                    int value = archParameter.AsInteger();
                    // Skip if Arch Grid is checkmarked
                    if (value == 1)
                        continue;
                }

                Parameter mainGridParameter = element.LookupParameter("Main Grid");

                if (mainGridParameter != null)
                {
                    int value = mainGridParameter.AsInteger();
                    // Skip if Main Grid is not checkmarked
                    if (value == 0)
                        continue;
                }

                // add the tuple grid lines
                Grid grid = element as Grid;
                if (grid != null)
                {
                    var pair = Tuple.Create(grid.Curve.GetEndPoint(0), grid.Curve.GetEndPoint(1));
                    gridLines.Add(pair);
                }
            }

            double precision = 0.0001;

            // Collect Horizontal and Vertical Lines
            mHorizontalLines = gridLines.Where(pair => Math.Abs(pair.Item1.Y - pair.Item2.Y) < precision).ToList().OrderBy(pair => pair.Item1.Y).ToList();
            mVerticalLines = gridLines.Where(pair => Math.Abs(pair.Item1.X - pair.Item2.X) < precision).ToList().OrderBy(pair => pair.Item1.X).ToList();
        }

        public bool Validate()
        {

            double precision = 0.0001;
            bool isEquidistant = true;

            // Check consecutive horizontal lines
            for (int i = 0; i < mHorizontalLines.Count - 1; i++)
            {
                double distance = mHorizontalLines[i + 1].Item1.X - mHorizontalLines[i].Item1.X;
                if (Math.Abs(distance - (mHorizontalLines[i].Item2 - mHorizontalLines[i].Item1).GetLength()) > precision)
                {
                    isEquidistant = false;
                    break;
                }
            }

            if (isEquidistant)
            {
                // Check consecutive vertical lines
                for (int i = 0; i < mVerticalLines.Count - 1; i++)
                {
                    double distance = mVerticalLines[i + 1].Item1.Y - mVerticalLines[i].Item1.Y;
                    if (Math.Abs(distance - (mVerticalLines[i].Item2 - mVerticalLines[i].Item1).GetLength()) > precision)
                    {
                        isEquidistant = false;
                        break;
                    }
                }
            }

            return isEquidistant;
        }
    }
}

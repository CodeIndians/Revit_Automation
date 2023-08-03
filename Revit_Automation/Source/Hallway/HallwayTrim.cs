using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Revit_Automation.Source.Hallway.HallwayGenerator;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Revit_Automation.Source.Hallway
{
    internal class HallwayTrim
    {
        private Document mDocument;

        private List<LabelLine> mHorLabelLines;

        private List<LabelLine> mVertLabelLines;

        private List<Element> mInternalLines;

        private List<Element> mExternalLines;

        public static bool bOnButtonClick = false;

        public HallwayTrim(ref Document doc, List<LabelLine> horLabelLines, List<LabelLine> verLabelLines) 
        {
            mDocument = doc;
            mHorLabelLines = horLabelLines;
            mVertLabelLines = verLabelLines;

            mInternalLines = new List<Element>();
            mExternalLines = new List<Element>();

            CollectInputLines();

        }

        private void CollectInputLines()
        {
            var lineCollector = new FilteredElementCollector(mDocument, mDocument.ActiveView.Id)
                                       .WhereElementIsNotElementType()
                                       .OfCategory(BuiltInCategory.OST_GenericModel);

            // collect lines into internal and external lines 
            foreach (Element elem in lineCollector)
            {
                LocationCurve locationCurve = elem.Location as LocationCurve;
                
                if (elem.IsHidden(mDocument.ActiveView))
                {
                    continue;
                }
                if (locationCurve != null)
                {
                    Line line = locationCurve.Curve as Line;
                    elem.LookupParameter("Wall Type").ToString().Contains("Ex");
                    if (line != null)
                    {
                        var wallType = elem.LookupParameter("Wall Type").AsString().Contains("Ex");
                        if (wallType)
                            mExternalLines.Add(elem);
                        else
                            mInternalLines.Add(elem);
                    }
                    else
                    {
                        continue;
                    }
                }
            }

        }
   
        public void TestMoveLine()
        {
            TransactionStatus status;
            Element elem = mInternalLines[29];

            using (Transaction trans = new Transaction(mDocument, "bla"))
            {
                trans.Start("Testing moving");
                LocationCurve locationCurve = elem.Location as LocationCurve;
                Line wallLine = locationCurve.Curve as Line;

                XYZ startPoint = wallLine.GetEndPoint(0);
                XYZ endPoint = wallLine.GetEndPoint(1);

                XYZ minimalMoveVector = new XYZ(0.0 /* =~ 3mm*/, 2.0, 0.0);
                startPoint += minimalMoveVector;
                endPoint += minimalMoveVector;

                //double start = wallLine.GetEndParameter(0);
                //double end = wallLine.GetEndParameter(1);

                
                //wallLine.MakeUnbound();
                //wallLine.MakeBound(start + 100, end + 100 );

                locationCurve.Curve = Line.CreateBound(startPoint, endPoint);

                //(elem.Location as LocationCurve).Curve = wallLine;
                
                status = trans.Commit();
            }

            if (status != TransactionStatus.Committed)
                MessageBox.Show("Commit failed");
        }

    }
}


using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.ModelCreators
{
    public class BottomTrackCreator : IModelCreator
    {
        public Document m_Document;
        public Form1 m_Form;
        public double dBottomTrackPreferredLength;
        public double dBottomTrackMaxLength;

        public BottomTrackCreator(Document doc, Form1 form) { 
            m_Document = doc;  
            m_Form = form;
            dBottomTrackMaxLength = GlobalSettings.s_dBottomTrackMaxLength;
            dBottomTrackPreferredLength = GlobalSettings.s_dBottomTrackPrefLength;
        }

        public void CreateModel(List<InputLine> colInputLines, IOrderedEnumerable<Autodesk.Revit.DB.Level> levels)
        {
            using (Transaction tx = new Transaction(m_Document))
            {
                m_Form.PostMessage("");
                m_Form.PostMessage("\n Placing Bottom Tracks");
                GenericUtils.SupressWarningsInTransaction(tx);
                tx.Start("Generating Model");
                PlaceBottomTracks(colInputLines, levels);
                m_Form.PostMessage("\n Finished Placing Bottom Tracks");
                tx.Commit();
            }
        }

        private void PlaceBottomTracks(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            Logger.logMessage("Method - PlaceBottomTracks");

            int iLineProcessing = 0;

            DateTime StartTime = DateTime.Now;

            double dCounter = 0;
            int iCounter = 1;
            double dIncrementFactor = 100 / colInputLines.Count;

            foreach (InputLine inputLine in colInputLines)
            {
                try
                {
                    iLineProcessing++;
                    //m_Form.PostMessage(string.Format("\n Placing Bottom Track at Line {0} / {1}", iLineProcessing, colInputLines.Count));
                    Logger.logMessage(string.Format("Placing Bottom Track at Line {0} / {1} : ID : {2}", iLineProcessing, colInputLines.Count, inputLine.id));

                    if (iCounter < 100 && (iCounter < dCounter))
                    {
                        iCounter = (int)Math.Ceiling(dCounter);
                        m_Form.UpdateProgress(iCounter);
                    }

                    PlaceBottomTrack(inputLine, levels);

                }
                catch (Exception e) { }
            }

            DateTime EndTime = DateTime.Now;

            TimeSpan timeDifference = EndTime - StartTime;
            double seconds = timeDifference.TotalSeconds;

            //m_Form.PostMessage(string.Format("\n Completed Placement of Bottom Tracks in {0} seconds", seconds));
        }

        private void PlaceBottomTrack(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
            Logger.logMessage("Method -PlaceBottomTrack");

            if (dBottomTrackMaxLength == 0 || dBottomTrackPreferredLength == 0)
            {
                TaskDialog.Show("Automatio Error", "Bottom Track Preferred/Max lengths are not set");
                return;
            }

            double dLineLength = 0.0;
            List<double> BTPlacementLengths = new List<double>();
            // Get Line End points.
            XYZ pt1 = null, pt2 = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);


            // Get the orientation of the line
            LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

            // Check for wall lines shorter than max length and process them as a single track
            // Else 
            if (lineType == LineType.Horizontal)
                dLineLength = (Math.Abs(pt2.X - pt1.X));
            else if (lineType == LineType.vertical)
                dLineLength = (Math.Abs(pt2.Y - pt1.Y));

            if (dLineLength > dBottomTrackMaxLength)
            {
                while (dLineLength > dBottomTrackMaxLength)
                {
                    BTPlacementLengths.Add(dBottomTrackPreferredLength);
                    dLineLength -= dBottomTrackPreferredLength;
                } 
                BTPlacementLengths.Add(dLineLength);
            }
            else
            {
                BTPlacementLengths.Add(dLineLength);
            }

            XYZ refPoint = pt1;
            
            // Place Bottom tracks. 
            foreach (double length in BTPlacementLengths)
            {
                XYZ endPoint = null;

                if (lineType == LineType.Horizontal)
                    endPoint = new XYZ(refPoint.X + length, refPoint.Y, refPoint.Z);
                else
                    endPoint = new XYZ(refPoint.X , refPoint.Y + length, refPoint.Z);

                Line newInputLine = Line.CreateBound(refPoint, endPoint);

                FamilySymbol symbol = GetBottomTrackSymbol(inputLine);

                if (symbol != null && !symbol.IsActive)
                    symbol.Activate();

                FamilyInstance lineElement = m_Document.GetElement(inputLine.id) as FamilyInstance;
                Level level = lineElement.Host as Level;

                FamilyInstance bottomTrackInstance = m_Document.Create.NewFamilyInstance(newInputLine, symbol, level, StructuralType.Beam);

                Parameter zJustification = bottomTrackInstance.get_Parameter(BuiltInParameter.Z_JUSTIFICATION);
                if (zJustification != null)
                {
                    zJustification.Set(((double)ZJustification.Origin));
                }
                StructuralFramingUtils.DisallowJoinAtEnd(bottomTrackInstance, 0);
                
                StructuralFramingUtils.DisallowJoinAtEnd(bottomTrackInstance, 1);

                //m_Form.PostMessage(string.Format("BottomTrack ID : {0}", bottomTrackInstance.Id));

                refPoint = endPoint;
            }
        }

        private FamilySymbol GetBottomTrackSymbol(InputLine inputLine)
        {
            string bottomTrackFamilyName = "Bottom Track";
            string bottomTrackSymbolName = "";
            if (string.IsNullOrEmpty(inputLine.strBottomTrackPunch))
                bottomTrackSymbolName = string.Format("{0} x {1}ga", inputLine.strBottomTrackSize, inputLine.strBottomTrackGuage);
            else
                bottomTrackSymbolName = string.Format("{0} x {1}ga x {2}", inputLine.strBottomTrackSize, inputLine.strBottomTrackGuage, inputLine.strBottomTrackPunch);

            FamilySymbol sym = SymbolCollector.GetBottomOrTopTrackSymbol(bottomTrackSymbolName, bottomTrackFamilyName);

            return sym; 
                  
        }
    }
}



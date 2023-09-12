using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Revit_Automation.Source.Utils.WarningSwallowers;

namespace Revit_Automation.Source.Utils
{
    public class GenericUtils
    {

        public static double FlangeWidth(string strColumnName)
        {
            double width = 0;
            string token = "x";
            string[] result = strColumnName.Split(new string[] { token }, StringSplitOptions.None);

            if (result[1].Contains(" 1\""))
            {
                return 0.083333;
            }
            else if (result[1].Contains("1 1/2\"") || result[1] == " 1 1/2")
            {
                return 0.125;
            }
            else if (result[1].Contains(" 2\"") || result[1] == " 2")
            {
                return 0.166666;
            }
            else if (result[1].Contains("2 1/2\"") || result[1] == " 2 1/2")
            {
                return 0.208333;
            }
            else if (result[1].Contains(" 3\"") || result[1] == " 3")
            {
                return 0.25;
            }
            else if (result[1].Contains("3 1/2\"") || result[1] == " 3 1/2")
            {
                return 0.291666;
            }
            else if (result[1].Contains("1 5/8\"") || result[1] == " 1 5/8")
            {
                return 0.135416;
            }
            return width;
        }

        public static double WebWidth(string strColumnName)
        {
            double width = 0;
            string token = "x";
            string[] result = strColumnName.Split(new string[] { token }, StringSplitOptions.None);

            if (result[0].Contains("4\""))
            {
                return 0.333333;
            }
            else if (result[0].Contains("6\""))
            {
                return 0.5;
            }
            else if (result[0].Contains("8\""))
            {
                return 0.666666;
            }
            else if (result[0].Contains("2 1/2\""))
            {
                return 0.208333;
            }
            else if (result[0].Contains("3 5/8\""))
            {
                return 0.302083;
            }

            return width;

        }

        public static XYZ GetLineOrientation(Element continuousLine)
        {
            XYZ LineOrientation = null;

            LocationCurve locationCurve = (LocationCurve)continuousLine.Location;
            XYZ pt1 = locationCurve.Curve.GetEndPoint(0);
            XYZ pt2 = locationCurve.Curve.GetEndPoint(1);

            LineType lineType = LineType.vertical;

            if (MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y))
            {
                lineType = LineType.Horizontal;
            }

            if (lineType == LineType.Horizontal)
            {
                XYZ start = pt1.X > pt2.X ? pt2 : pt1;
                XYZ end = pt1.X > pt2.X ? pt1 : pt2;
                LineOrientation = end - start;
            }

            if (lineType == LineType.vertical)
            {
                XYZ start = pt1.Y > pt2.Y ? pt2 : pt1;
                XYZ end = pt1.Y > pt2.Y ? pt1 : pt2;
                LineOrientation = end - start;
            }

            return LineOrientation;
        }

        public static XYZ GetLineOrientation(InputLine inputLine)
        {
            XYZ LineOrientation = null;

            LocationCurve locationCurve = inputLine.locationCurve;
            XYZ pt1 = locationCurve.Curve.GetEndPoint(0);
            XYZ pt2 = locationCurve.Curve.GetEndPoint(1);

            LineType lineType = LineType.vertical;

            if (MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y))
            {
                lineType = LineType.Horizontal;
            }

            if (lineType == LineType.Horizontal)
            {
                XYZ start = pt1.X > pt2.X ? pt2 : pt1;
                XYZ end = pt1.X > pt2.X ? pt1 : pt2;
                LineOrientation = end - start;
            }

            if (lineType == LineType.vertical)
            {
                XYZ start = pt1.Y > pt2.Y ? pt2 : pt1;
                XYZ end = pt1.Y > pt2.Y ? pt1 : pt2;
                LineOrientation = end - start;
            }

            return LineOrientation;
        }

        public static void SupressWarningsInTransaction(Transaction tx)
        {
            FailureHandlingOptions failureHandlingOptions =
                                tx.GetFailureHandlingOptions();

            DuplicateColumnWarningSwallower duplicateColumnWarningSwallower =
              new DuplicateColumnWarningSwallower();

            _ = failureHandlingOptions.SetFailuresPreprocessor(
              duplicateColumnWarningSwallower);

            _ = failureHandlingOptions.SetClearAfterRollback(
              true);

            tx.SetFailureHandlingOptions(
              failureHandlingOptions);
        }

        public static void GetlineStartAndEndPoints(Element Line, out XYZ start, out XYZ end)
        {
            LocationCurve locationCurve = (LocationCurve)Line.Location;
            start = null;
            end = null;
            if (locationCurve != null)
            {
                XYZ pt1 = locationCurve.Curve.GetEndPoint(0);
                XYZ pt2 = locationCurve.Curve.GetEndPoint(1);

                start = null; end = null;

                LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

                if (lineType == LineType.Horizontal)
                {
                    start = pt1.X > pt2.X ? pt2 : pt1;
                    end = pt1.X > pt2.X ? pt1 : pt2;
                }

                if (lineType == LineType.vertical)
                {
                    start = pt1.Y > pt2.Y ? pt2 : pt1;
                    end = pt1.Y > pt2.Y ? pt1 : pt2;
                }
            }
        }

        public static void GetlineStartAndEndPoints(Curve curve, out XYZ start, out XYZ end)
        {
            XYZ pt1 = curve.GetEndPoint(0);
            XYZ pt2 = curve.GetEndPoint(1);

            start = null; end = null;

            LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

            if (lineType == LineType.Horizontal)
            {
                start = pt1.X > pt2.X ? pt2 : pt1;
                end = pt1.X > pt2.X ? pt1 : pt2;
            }

            if (lineType == LineType.vertical)
            {
                start = pt1.Y > pt2.Y ? pt2 : pt1;
                end = pt1.Y > pt2.Y ? pt1 : pt2;
            }
        }
        public static void GetlineStartAndEndPoints(InputLine inputLine, out XYZ start, out XYZ end)
        {
            LocationCurve locationCurve = inputLine.locationCurve;
            XYZ pt1 = locationCurve.Curve.GetEndPoint(0);
            XYZ pt2 = locationCurve.Curve.GetEndPoint(1);

            start = null; end = null;

            LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

            if (lineType == LineType.Horizontal)
            {
                start = pt1.X > pt2.X ? pt2 : pt1;
                end = pt1.X > pt2.X ? pt1 : pt2;
            }

            if (lineType == LineType.vertical)
            {
                start = pt1.Y > pt2.Y ? pt2 : pt1;
                end = pt1.Y > pt2.Y ? pt1 : pt2;
            }
        }

        public static Element GetNearestFloorOrRoof(Level level, XYZ pt1, Document m_Document)
        {
            if (level == null)
                return null;

            Logger.logMessage("Method : GetNearestFloorOrRoof");

            List<FloorObject> floorObjects = FloorHelper.colFloors;

            Element elemID = null;

            // match the building name as the level
            List<FloorObject> filteredFloors = new List<FloorObject>();

            foreach (FloorObject floorObject in floorObjects)
            {
                if (level.Name.Contains(floorObject.strBuildingName))
                {
                    filteredFloors.Add(floorObject);
                }
            }


            foreach (FloorObject floor in filteredFloors)
            {
                if (floor.min == null || floor.max == null)
                {
                    continue;
                }

                // match the bounding box of the point with the Floor Range
                double Xmin, Xmax, Ymin, Ymax = 0.0;
                Xmin = Math.Min(floor.max.X, floor.min.X);
                Xmax = Math.Max(floor.max.X, floor.min.X);
                Ymin = Math.Min(floor.max.Y, floor.min.Y);
                Ymax = Math.Max(floor.max.Y, floor.min.Y);

                if (pt1.X >= Xmin && pt1.X <= Xmax && pt1.Y >= Ymin && pt1.Y <= Ymax)
                {
                    Element levelElement = m_Document.GetElement(floor.levelID);
                    Parameter elevationParam = levelElement.get_Parameter(BuiltInParameter.LEVEL_ELEV);
                    if (elevationParam != null)
                    {
                        if (MathUtils.IsWithInRange(elevationParam.AsDouble(), level.Elevation + 3, level.Elevation - 3))
                        {
                            elemID = m_Document.GetElement(floor.elemID);
                            break;
                        }
                    }
                }
            }
            return elemID;
        }

        public static Element GetRoofAtPoint(XYZ pt1, Document m_Document)
        {
            Logger.logMessage("Method : GetRoofAtPoint");

            RoofObject targetRoof;
            targetRoof.slopeLine = null;
            targetRoof.roofElementID = null;

            foreach (RoofObject roof in RoofUtility.colRoofs)
            {
                double Xmin, Xmax, Ymin, Ymax = 0.0;
                Xmin = Math.Min(roof.max.X, roof.min.X);
                Xmax = Math.Max(roof.max.X, roof.min.X);
                Ymin = Math.Min(roof.max.Y, roof.min.Y);
                Ymax = Math.Max(roof.max.Y, roof.min.Y);

                if (pt1.X >= Xmin && pt1.X <= Xmax && pt1.Y >= Ymin && pt1.Y <= Ymax)
                {
                    targetRoof = roof;
                    break;
                }
            }


            return targetRoof.roofElementID != null ? m_Document.GetElement(targetRoof.roofElementID) : null;
        }

        public static XYZ GetRoofSlopeDirection(XYZ pt1)
        {
            XYZ SlopeDirect = null;

            RoofObject targetRoof;
            targetRoof.slopeLine = null;

            foreach (RoofObject roof in RoofUtility.colRoofs)
            {
                double Xmin, Xmax, Ymin, Ymax = 0.0;
                Xmin = Math.Min(roof.max.X, roof.min.X);
                Xmax = Math.Max(roof.max.X, roof.min.X);
                Ymin = Math.Min(roof.max.Y, roof.min.Y);
                Ymax = Math.Max(roof.max.Y, roof.min.Y);

                if (pt1.X > Xmin && pt1.X < Xmax && pt1.Y > Ymin && pt1.Y < Ymax)
                {
                    targetRoof = roof;
                    break;
                }
            }

            Curve SlopeCurve = targetRoof.slopeLine;

            if (SlopeCurve != null)
            {
                XYZ start = SlopeCurve.GetEndPoint(0);
                XYZ end = SlopeCurve.GetEndPoint(1);

                XYZ slope = start.Z > end.Z ? (end - start) : (start - end);

                SlopeDirect = new XYZ(slope.X, slope.Y, 0.0);
            }
            return SlopeDirect;
        }

        public static void AdjustWallEndPoints(InputLine inputLine,ref XYZ startpt, ref List<XYZ> middleIntersections, ref XYZ endPt, LineType linetype, PanelDirection panelDirection)
        {
            double dParam = GenericUtils.GetPanelWidth(inputLine)/2.0 ;
            List<XYZ> modifiedCollection = new List<XYZ>();
            if (linetype == LineType.Horizontal && (panelDirection == PanelDirection.R || panelDirection == PanelDirection.U))
            {
                startpt = new XYZ(startpt.X, startpt.Y + dParam, startpt.Z);
                endPt = new XYZ(endPt.X, endPt.Y + dParam, endPt.Z);

                foreach (XYZ xyz in middleIntersections)
                {
                    XYZ temp = new XYZ(xyz.X, xyz.Y + dParam, xyz.Z);
                    modifiedCollection.Add(temp);
                }
                middleIntersections.Clear();
                middleIntersections = modifiedCollection;
            }
            if (linetype == LineType.Horizontal && (panelDirection == PanelDirection.L || panelDirection == PanelDirection.D))
            {
                startpt = new XYZ(startpt.X, startpt.Y - dParam, startpt.Z);
                endPt = new XYZ(endPt.X, endPt.Y - dParam, endPt.Z);

                foreach (XYZ xyz in middleIntersections)
                {
                    XYZ temp = new XYZ(xyz.X, xyz.Y - dParam, xyz.Z);
                    modifiedCollection.Add(temp);
                }
                middleIntersections.Clear();
                middleIntersections = modifiedCollection;
            }
            if (linetype == LineType.vertical && (panelDirection == PanelDirection.R || panelDirection == PanelDirection.U))
            {
                startpt = new XYZ(startpt.X + dParam, startpt.Y, startpt.Z);
                endPt = new XYZ(endPt.X + dParam, endPt.Y, endPt.Z);

                foreach (XYZ xyz in middleIntersections)
                {
                    XYZ temp = new XYZ(xyz.X + dParam, xyz.Y, xyz.Z);
                    modifiedCollection.Add(temp);
                }
                middleIntersections.Clear();
                middleIntersections = modifiedCollection;
            }
            if (linetype == LineType.vertical && (panelDirection == PanelDirection.L || panelDirection == PanelDirection.D))
            {
                startpt = new XYZ(startpt.X - dParam, startpt.Y, startpt.Z);
                endPt = new XYZ(endPt.X - dParam, endPt.Y, endPt.Z);

                foreach (XYZ xyz in middleIntersections)
                {
                    XYZ temp = new XYZ(xyz.X - dParam, xyz.Y, xyz.Z);
                    modifiedCollection.Add(temp);
                }
                middleIntersections.Clear();
                middleIntersections = modifiedCollection;
            }

        }

        internal static double GetPanelWidth(InputLine inputLine)
        {
            double dPanelThickness = 1.0/16.0; //  (3/4" to be default)

            PanelTypeGlobalParams pg = string.IsNullOrEmpty(inputLine.strPanelType) ?
                           GlobalSettings.lstPanelParams.Find(panelParams => panelParams.bIsUNO == true) :
                           GlobalSettings.lstPanelParams.Find(panelParams => panelParams.strWallName == inputLine.strPanelType);

            WallType wallType = SymbolCollector.GetWall(pg.strWallName, "Basic Wall");

            if (wallType != null) { dPanelThickness = wallType.Width; }
            return dPanelThickness;
        }

        /// <summary>
        /// Function to get the hatch id of the hallway region type
        /// Return null if the hallway type is not present
        /// </summary>
        public static ElementId GetHallwayHatchId(Document doc)
        {
            // hatch id for hallway hatch filled region 
            ElementId hatchId = null;

            // capture hallway hatch element ID
            var filledRegion = new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType));
            foreach (var region in filledRegion)
            {
                if (region.Name == "Hallway hatch")
                {
                    hatchId = region.Id;
                    break;
                }
            }

            return hatchId;
        }

        /// <summary>
        /// return hallway region if present otherwise return null
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>hallway hatch as filled region</returns>
        public static FilledRegion GetHallwayRegion(Document doc)
        {
            // collect all the filled regions
            FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
            ICollection<Element> filledRegions = collector.OfClass(typeof(FilledRegion)).ToElements();

            FilledRegion hallwayRegion = null;

            // collect hallway region from the filled regions based on hatch id 
            foreach (var region in filledRegions)
            {
                if (region.GetTypeId() == GetHallwayHatchId(doc))
                {
                    hallwayRegion = region as FilledRegion;
                    break;
                }
            }

            return hallwayRegion;
        }

        /// <summary>
        /// Check if any of the line interesects with the hallway
        /// </summary>
        /// <param name="doc">DB Document</param>
        /// <param name="startPt">start point</param>
        /// <param name="endPt">end point</param>
        /// <returns></returns>
        public static bool LineIntersectsHallway(Document doc,XYZ startPt, XYZ endPt)
        {
            //get the hallway region as filled region
            FilledRegion hallwayRegion = GetHallwayRegion(doc);

            if(hallwayRegion == null)
            {
                TaskDialog.Show("Error", "Hallway type or Hallway hatch is not present in this project");
                return false;
            }

            IList<CurveLoop> curveLoops = hallwayRegion.GetBoundaries();

            // construct a line from the given points 
            try
            {
                Line givenLine = Line.CreateBound(startPt, endPt);

                // iterate through all the curveloops
                foreach (CurveLoop curveLoop in curveLoops)
                {
                    // iterate through each curve in the curve loop
                    foreach (Curve curve in curveLoop)
                    {
                        //create a line from the curve, assuming that hallways always have line curves
                        Line hallwayLine = Line.CreateBound(curve.GetEndPoint(0), curve.GetEndPoint(1));

                        // return true if the lines are intersecting
                        if (givenLine.Intersect(hallwayLine) == SetComparisonResult.Overlap)
                            return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static LineType GetLineType(InputLine inputLine)
        {
            if (MathUtils.ApproximatelyEqual(inputLine.startpoint.X, inputLine.endpoint.X))
                return LineType.vertical;
            else
                return LineType.Horizontal;
        }
    }
}

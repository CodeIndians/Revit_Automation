using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.CollisionDetectors;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using static Autodesk.Revit.DB.SpecTypeId;

namespace Revit_Automation.Source.ModelCreators
{
    internal class WallCreator : IModelCreator
    {
        private Autodesk.Revit.DB.Document m_Document { get; set; }

        private Form1 m_Form { get; set; }
        
        public WallCreator(Autodesk.Revit.DB.Document doc, Form1 form)
        {

            m_Document = doc;
            m_Form = form;
        }

        public enum InputGridRelation
        {
            InputToLeftOfGrid = 0,
            InputToRightOfGrid = 1, 
            InputToTopOfGrid = 2,
            InputToBottomOfGrid
        }

        public void CreateModel(List<CustomTypes.InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            using (Transaction tx = new Transaction(m_Document))
            {
                GenericUtils.SupressWarningsInTransaction(tx);
                tx.Start("Generating Model");
                ProcessWallLines(colInputLines, levels);
                tx.Commit();
            }
        }

        private void ProcessWallLines(List<InputLine> colInputLines, IOrderedEnumerable<Level> levels)
        {
            Logger.logMessage("Method - ProcessWallLines");

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
                    m_Form.PostMessage(string.Format("\n Placing Wall at Line {0} / {1}", iLineProcessing, colInputLines.Count));
                    Logger.logMessage(string.Format("Placing Wall at Line {0} / {1} : ID : {2}", iLineProcessing, colInputLines.Count, inputLine.id));

                    if (iCounter < 100 && (iCounter < dCounter))
                    {
                        iCounter = (int)Math.Ceiling(dCounter);
                        m_Form.UpdateProgress(iCounter);
                    }

                    PlaceWall(inputLine, levels);

                }
                catch (Exception e){ }
            }

            DateTime EndTime = DateTime.Now;

            TimeSpan timeDifference = EndTime - StartTime;
            double seconds = timeDifference.TotalSeconds;

            m_Form.PostMessage(string.Format("\n Completed Placement of walls in {0} seconds", seconds));
        }

        private void PlaceWall(InputLine inputLine, IOrderedEnumerable<Level> levels)
        {
            Logger.logMessage("Method - PlaceWall");

            // Get Line End points.
            XYZ pt1 = null, pt2 = null;
            GenericUtils.GetlineStartAndEndPoints(inputLine, out pt1, out pt2);


            // Get the orientation of the line
            LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y)? LineType.Horizontal:  LineType.vertical;

            // Check for wall lines shorter that 25 Feet and Process only them
            if (lineType == LineType.Horizontal && (Math.Abs(pt2.X - pt1.X) > 25))
                return;
            else if (lineType == LineType.vertical && (Math.Abs(pt2.Y - pt1.Y) > 25))
                return;

            // Compute the end points of the wall to be placed
            XYZ wp1 = null, wp2 = null;
            bool bFlip = false;
            ComputeWallEndPoints(pt1, pt2, inputLine, lineType, out wp1, out wp2, out bFlip);

            // Identify the base and top levels,
            Level toplevel = null, baseLevel = null;

            // Filter levels based on buldings to use
            List<Level> filteredLevels = new List<Level>();
            foreach (Level filteredlevel in levels)
            {
                if (filteredlevel.Name.Contains(inputLine.strBuildingName))
                {
                    filteredLevels.Add(filteredlevel);
                }
            }

            for (int i = 0; i < filteredLevels.Count() - 1; i++)
            {
                Level tempLevel = filteredLevels.ElementAt(i);

                if ((pt2.Z < (tempLevel.Elevation + 1)) && (pt2.Z > (tempLevel.Elevation - 1)))
                {

                    baseLevel = tempLevel;
                    toplevel = filteredLevels.ElementAt(i + 1);

                    break;
                }
            }

            // Compute the top and bottom Attachment Object
            Element topAttachElement = null, bottomAttachElement = null;
            topAttachElement = GenericUtils.GetNearestFloorOrRoof(toplevel, pt1, m_Document );
            bottomAttachElement = GenericUtils.GetNearestFloorOrRoof(baseLevel, pt1, m_Document);

            // Wall options
            WallType wallType = SymbolCollector.GetWall("'U' Panel", "Basic Wall");

            // Create Wall Curve
            Line wallLine = Line.CreateBound(wp1, wp2);
            List<Curve> wallCurves = new List<Curve> { wallLine };
            
            // Place Wall
            Wall wall = Wall.Create(m_Document, wallLine, wallType.Id, baseLevel.Id, (toplevel.Elevation - baseLevel.Elevation) , 0 , false, false);
            
            // Disallow joins at start and End
            WallUtils.DisallowWallJoinAtEnd(wall, 0);
            WallUtils.DisallowWallJoinAtEnd(wall, 1);
            
        }

        /// <summary>
        /// Computes the wall end points based on the Input line and grid reference
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="inputLine"></param>
        /// <param name="lineType"></param>
        /// <param name="wp1"></param>
        /// <param name="wp2"></param>
        /// <param name="bFlip"></param>
        private void ComputeWallEndPoints(XYZ pt1, XYZ pt2, InputLine inputLine, LineType lineType, out XYZ wp1, out XYZ wp2, out bool bFlip)
        {
            wp1  = null;
            wp2 = null;
            bFlip = false;

            // Get ReleationShip between Grid and Inputl Line
            InputGridRelation relation = ComputeInputLineGridRelation(pt1, pt2, inputLine, lineType);

            //Compute the web width for a give input line
            double dwebWidth = GenericUtils.WebWidth(inputLine.strStudType);

            // Add a factor of half of panel width
            dwebWidth += 0.03125;
            // Compute the wall line points based on the relation and also flip
            switch (relation)
            {
                case InputGridRelation.InputToLeftOfGrid:
                    wp1 = new XYZ(pt1.X + dwebWidth/2 , pt1.Y, pt1.Z);
                    wp2 = new XYZ(pt2.X + dwebWidth / 2, pt2.Y, pt2.Z);
                    break;
                case InputGridRelation.InputToRightOfGrid:
                    wp1 = new XYZ(pt1.X - dwebWidth / 2, pt1.Y, pt1.Z);
                    wp2 = new XYZ(pt2.X - dwebWidth / 2, pt2.Y, pt2.Z);
                    bFlip = true;
                    break;
                case InputGridRelation.InputToTopOfGrid:
                    wp1 = new XYZ(pt1.X , pt1.Y - dwebWidth / 2, pt1.Z);
                    wp2 = new XYZ(pt2.X , pt2.Y - dwebWidth / 2, pt2.Z);
                    bFlip = true;
                    break;

                case InputGridRelation.InputToBottomOfGrid:
                    wp1 = new XYZ(pt1.X, pt1.Y + dwebWidth / 2, pt1.Z);
                    wp2 = new XYZ(pt2.X, pt2.Y + dwebWidth / 2, pt2.Z);
                    break;

                default: break;
            }
        }

        /// <summary>
        /// This method computes the relationshipo between given input line and nearest main grid line
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="inputLine"></param>
        /// <param name="lineType"></param>
        /// <returns></returns>

        private InputGridRelation ComputeInputLineGridRelation(XYZ pt1, XYZ pt2, InputLine inputLine, LineType lineType)
        {

            // Get Nearest main grid point and line point
            XYZ nearestMainGrid = null, linePoint = null;
            
           GetNearestMainGrid(pt1, pt2, inputLine, lineType, out nearestMainGrid, out linePoint);

            if (linePoint != null)
                pt1 = linePoint;

            if (lineType == LineType.Horizontal)
            {
                if (pt1.Y < nearestMainGrid.Y)
                    return InputGridRelation.InputToBottomOfGrid;
                else
                    return InputGridRelation.InputToTopOfGrid;
            }
            else 
            {
                if (pt1.X < nearestMainGrid.X)
                    return InputGridRelation.InputToLeftOfGrid;
                else
                    return InputGridRelation.InputToRightOfGrid;
            }
        }

        private void GetNearestMainGrid(XYZ pt1, XYZ pt2, InputLine inputLine, LineType lineType, out XYZ nearestMainGrid, out XYZ linePoint)
        {
            //Initialize the out parameters
            nearestMainGrid = null;
            linePoint = null;

            Logger.logMessage("Method : GetNearestMainGridLocation");

            double minDistance = double.MaxValue;
            XYZ nearestPoint = null, referencePoint = null;

            referencePoint = pt1;

            // Identify relevant grid lines collection
            List<Tuple<XYZ, XYZ>> gridLinesCollection = lineType == LineType.Horizontal ? GridCollector.mHorizontalMainLines : GridCollector.mVerticalMainLines;

            // Get the nearest Gridline point
            foreach (Tuple<XYZ, XYZ> gridline in gridLinesCollection)
            {
                XYZ point = lineType == LineType.vertical ? new XYZ(gridline.Item1.X, referencePoint.Y, referencePoint.Z) : new XYZ(referencePoint.X, gridline.Item1.Y, referencePoint.Z);

                double distance = point.DistanceTo(referencePoint);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPoint = point;
                }
            }

            // Check Whether the line and nearest grid are close to each other.
            if ((lineType == LineType.Horizontal && (Math.Abs(pt1.Y - nearestPoint.Y) < 1)) || (lineType == LineType.vertical && (Math.Abs(pt1.X - nearestPoint.X) < 1)))
            {
                nearestMainGrid = nearestPoint;
                linePoint = pt1;
            }

            // else get a pair of line and grid that are close to each other
            else
            {
                nearestMainGrid = nearestPoint;
                linePoint = ComputeLinePtAdjToMainGrid(pt1, nearestPoint, lineType);
            }
            
        }

        private XYZ ComputeLinePtAdjToMainGrid(XYZ pt1, XYZ nearestPoint, LineType lineType)
        {
            XYZ nearestLinePoint = null;

            // build a bounding box at a given point and identify those lines parallel to input line
            // Create a Outline, uses a minimum and maximum XYZ point to initialize the Bounding Box. 
            Outline myOutLn = new Outline(
                new XYZ(nearestPoint.X - 1,
                nearestPoint.Y - 1,
                nearestPoint.Z - 1),
                new XYZ(nearestPoint.X + 1,
                nearestPoint.Y + 1,
                nearestPoint.Z + 1));
            Logger.logMessage("Outline for Collection Objects Created");

            // Create a BoundingBoxIntersects filter with this Outline
            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);

            // Apply the filter to the elements in the active document to retrieve Input lines at a point
            FilteredElementCollector collector2 = new FilteredElementCollector(m_Document);
            IList<Element> InputLineElements = collector2.WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();
            Logger.logMessage("Bounding Box Filter Applied and Elements Collected");

            //Identify Line that is parallel to our given line
            foreach (Element inputLine in InputLineElements)
            {
                XYZ Linept1 = null, Linept2 = null;
                GenericUtils.GetlineStartAndEndPoints(inputLine, out Linept1, out Linept2);

                LineType lineorientation = Linept1.Y == Linept2.Y ? LineType.Horizontal : LineType.vertical;

                if (lineorientation == lineType)
                {
                    nearestPoint = Linept1;
                    break;
                }
            }
            return nearestPoint;
        }
    }
}

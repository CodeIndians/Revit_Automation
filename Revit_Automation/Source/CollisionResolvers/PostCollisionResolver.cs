using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Visual;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Interfaces;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;

namespace Revit_Automation.Source.CollisionDetectors
{
    /// <summary>
    /// Steps : 
    /// 1. At a give point identify the continuous line
    /// 2. Those posts having web parallel to the continuous line will not move
    /// 3. Those posts having web orientation 
    /// </summary>
    public class PostCollisionResolver : ICollisionInterface
    {
        
        public Document m_Document;
        
        public PostCollisionResolver(Document doc) { m_Document = doc; }

        public bool CheckStudCollisions(ElementId columnID)
        {
            FamilyInstance column = m_Document.GetElement(columnID) as FamilyInstance;
            XYZ newOrientation = column.FacingOrientation;

            BoundingBoxXYZ boundingBoxXYZ = column.get_BoundingBox(m_Document.ActiveView);

            XYZ min = new XYZ(boundingBoxXYZ.Min.X + 0.05, boundingBoxXYZ.Min.Y + 0.05, boundingBoxXYZ.Min.Z + 2.5);
            XYZ max = new XYZ(boundingBoxXYZ.Max.X - 0.05, boundingBoxXYZ.Max.Y - 0.05, boundingBoxXYZ.Max.Z - 2.5);

            Outline outline = new Outline(min, max);

            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

            ICollection<ElementId> columns = new FilteredElementCollector(m_Document).WherePasses(filter).OfCategory(BuiltInCategory.OST_StructuralColumns).ToElementIds();

            // Collect only those columns other than self
            columns = columns.Where(col => col != columnID).ToList();
            
            return columns.Count > 0 ;
        }
        /// <summary>
        /// Handles the collisions at a given location represented by the collission object
        /// </summary>
        /// <param name="collisionObject"></param>
        /// <returns></returns>
        public bool HandleCollision(CollisionObject collisionObject)
        {
            
            //// Create a Outline, uses a minimum and maximum XYZ point to initialize the Bounding Box. 
            //Outline myOutLn = new Outline(
            //    new XYZ(collisionObject.CollisionPoint.X - 0.5,
            //    collisionObject.CollisionPoint.Y - 0.5,
            //    collisionObject.CollisionPoint.Z - 0.5),
            //    new XYZ(collisionObject.CollisionPoint.X + 0.5,
            //    collisionObject.CollisionPoint.Y + 0.5,
            //    collisionObject.CollisionPoint.Z + 0.5));
            //Logger.logMessage("Outline for Collection Objects Created");
            
            //// Create a BoundingBoxIntersects filter with this Outline
            //BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);

            //// Apply the filter to the elements in the active document to retrieve posts at a point
            //FilteredElementCollector collector = new FilteredElementCollector(m_Document);
            //IList<Element> postElements = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_StructuralColumns).ToElements();

            //// Apply the filter to the elements in the active document to retrieve Input lines at a point
            //FilteredElementCollector collector2 = new FilteredElementCollector(m_Document);
            //IList<Element> InputLineElements = collector2.WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();
            //Logger.logMessage("Bounding Box Filter Applied and Elements Collected");

            ////Check for HSS at a given location, if present delete the colliding post
            //bool bhasHSS = CheckForHSS(postElements);
            //if (bhasHSS)
            //    return true;

            ////Ideally we should have 2 columns and 2 input lines in collision case.
            //if (InputLineElements.Count >= 2 && postElements.Count >= 2)
            //{
                
            //    // Identify the continous line and from it the Static Column
            //    Element continuousLine = IdentifyContinousLineAtPoint(collisionObject.CollisionPoint, InputLineElements);
            //    Logger.logMessage("Handle Collision : IdentifyContinousLineAtPoint ");
                
            //    if (continuousLine != null)
            //    {
            //        Element StaticColumn = IdentifyStaticPost(continuousLine, postElements);
            //        Logger.logMessage("Handle Collision : IdentifyStaticPost ");

            //        //Obtain the Webwidth from the Static Column
            //        double dWebWidth = StaticColumn == null ? 0 : GenericUtils.WebWidth(StaticColumn.Name);

            //        // Adjust the non static posts accordingly
            //        if (dWebWidth > 0)
            //        {
            //            MoveNonStaticColumns(InputLineElements, postElements, dWebWidth / 2, continuousLine);
            //            Logger.logMessage("Handle Collision : MoveNonStaticColumns ");
            //        }
            //    }
            //}

            return false;
        }

        private bool CheckForHSS(IList<Element> postElements)
        {
            Logger.logMessage("Handle Collision : Check for HSS ");
            foreach (Element colElement in postElements)
            {
                FamilyInstance post = m_Document.GetElement(colElement.Id) as FamilyInstance;

                if (post != null && post.Name.Contains("HSS"))
                    return true;
            }

            return false;
        }

        private void MoveNonStaticColumns(IList<Element> inputLineElements, IList<Element> postElements, double distanceToMove, Element continuousLine)
        {
            XYZ LineOrientation = GenericUtils.GetLineOrientation(continuousLine);

            foreach (Element colElement in postElements)
            {
                FamilyInstance post = m_Document.GetElement(colElement.Id) as FamilyInstance;
                XYZ postorientation = post.FacingOrientation;

                if (!MathUtils.IsParallel(postorientation, LineOrientation))
                {
                    XYZ AdjustedPostPoint = IdentifyNewPointForThePost(postorientation, post.Location, inputLineElements, distanceToMove, continuousLine);
                    Logger.logMessage("MoveNonStaticColumns : IdentifyNewPointForThePost");

                    MoveColumn(post.Id, AdjustedPostPoint);
                    Logger.logMessage("MoveNonStaticColumns : MoveColumn");
                }
            }
        }

        private XYZ IdentifyNewPointForThePost(XYZ postorientation, Location postLocation, IList<Element> inputLineElements, double distanceToMove, Element continuousLine)
        {
            XYZ newPoint = null;

            Logger.logMessage("Method : IdentifyNewPointForThePost");

            if (postLocation is LocationPoint location)
            {
                foreach (Element inputLine in inputLineElements)
                {
                    XYZ start = null, end = null;
                    GenericUtils.GetlineStartAndEndPoints(inputLine, out start, out end);

                    // Identify if the column is at start or end of a given line.
                    bool bColumnAtStart = MathUtils.ApproximatelyEqual(start.X, location.Point.X, 0.25) && MathUtils.ApproximatelyEqual(start.Y, location.Point.Y, 0.25);
                    bool bColumnAtEnd = MathUtils.ApproximatelyEqual(end.X, location.Point.X, 0.25) && MathUtils.ApproximatelyEqual(end.Y, location.Point.Y, 0.25);

                    XYZ lineOrientation = GenericUtils.GetLineOrientation(inputLine);

                    if ((bColumnAtStart && (MathUtils.CompareVectors(lineOrientation, postorientation) == "Anti-Parallel")) ||
                        (bColumnAtEnd && (MathUtils.CompareVectors(lineOrientation, postorientation) == "Parallel")))
                    {
                        double dDistance = ComputeDistanceToMove(continuousLine, start, end, distanceToMove);

                        LineType lineType = MathUtils.ApproximatelyEqual(start.Y, end.Y) ? LineType.Horizontal : LineType.vertical;

                        if (bColumnAtEnd && lineType == LineType.Horizontal)
                            newPoint = location.Point + new XYZ(-dDistance, 0, 0);
                        else if (bColumnAtEnd && lineType == LineType.vertical)
                            newPoint = location.Point + new XYZ(0 , -dDistance, 0);
                        if (bColumnAtStart && lineType == LineType.Horizontal)
                            newPoint = location.Point + new XYZ(dDistance, 0, 0);
                        if (bColumnAtStart && lineType == LineType.vertical)
                            newPoint = location.Point + new XYZ(0 , dDistance, 0);

                        break;
                    }  
                }
            }
            return newPoint;
        }

        /// <summary>
        ///  for cases like this --- | 
        /// </summary>
        /// <param name="continuousLine"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="distanceToMove"></param>
        /// <returns></returns>
        private double ComputeDistanceToMove(Element continuousLine, XYZ start, XYZ end, double distanceToMove)
        {
            double distToMove = 0;
            XYZ contStart = null, contEnd = null;
            GenericUtils.GetlineStartAndEndPoints(continuousLine, out contStart, out contEnd);

            LineType lineType = MathUtils.ApproximatelyEqual(start.Y, end.Y) ? LineType.Horizontal : LineType.vertical;

            if (lineType == LineType.vertical)
            {
                double dDist = Math.Min(Math.Abs(start.Y - contStart.Y), Math.Abs(end.Y - contEnd.Y));
                distToMove = distanceToMove - dDist;
            }
            if (lineType == LineType.Horizontal)
            {
                double dDist = Math.Min(Math.Abs(start.X - contStart.X), Math.Abs(end.X - contEnd.X));
                distToMove = distanceToMove - dDist;
            }

            return distToMove;
        }

        private void MoveColumn(ElementId columnId, XYZ newLocation)
        {

            if (newLocation == null || columnId == null)
                return;

            FamilyInstance column = m_Document.GetElement(columnId) as FamilyInstance;

            // Get the column's Location property
            Location location = column.Location;

            // Check if the column's location is a LocationPoint
            if (location is LocationPoint locationPoint)
            {
                // Set the new location for the column
                locationPoint.Point = newLocation;
                Logger.logMessage("MoveColumn : Change Location Point");
            }
        }

        private Element IdentifyStaticPost(Element continuousLine, IList<Element> postElements)
        {
            Logger.logMessage("Method : IdentifyStaticPost");

            Element StaticColumn = null;

            XYZ LineOrientation = GenericUtils.GetLineOrientation(continuousLine);

            foreach (Element column in postElements)
            {
                FamilyInstance post = m_Document.GetElement(column.Id) as FamilyInstance;
                XYZ postorientation = post.FacingOrientation;

                if (MathUtils.IsParallel(postorientation, LineOrientation))
                { 
                    StaticColumn = column;
                    break;
                }
            }
            return StaticColumn;
        }   

        private Element IdentifyContinousLineAtPoint(XYZ collisionPoint, IList<Element> inputLineElements)
        {

            Logger.logMessage("Method : IdentifyContinousLineAtPoint");

            Element continousElement = null;
            XYZ TracePoint1;
            XYZ TracePoint2;

            foreach (Element GenericLine in inputLineElements)
            {
                // When two lines intesect in a T junction and one line is extened a little bit
                collisionPoint = AdjustCollisionPointIfNecessary(collisionPoint, GenericLine, inputLineElements);
                
                // Get the location curve
                LocationCurve locationCurve = (LocationCurve)GenericLine.Location;
                XYZ pt1 = locationCurve.Curve.GetEndPoint(0);
                XYZ pt2 = locationCurve.Curve.GetEndPoint(1);

                LineType lineType = MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y) ? LineType.Horizontal : LineType.vertical;

                if (lineType == LineType.Horizontal)
                {
                    TracePoint1 = collisionPoint + new XYZ(0.1, 0, 0);
                    TracePoint2 = collisionPoint + new XYZ(-0.1, 0, 0);

                    XYZ start = pt1.X > pt2.X ? pt2 : pt1;
                    XYZ end = pt1.X > pt2.X ? pt1 : pt2;

                    if (TracePoint1.X > start.X && TracePoint1.X < end.X &&
                        TracePoint2.X > start.X && TracePoint2.X < end.X)
                    {
                        continousElement = GenericLine;
                        break;
                    }    
                }

                if (lineType == LineType.vertical)
                {
                    TracePoint1 = collisionPoint + new XYZ(0, 0.1, 0);
                    TracePoint2 = collisionPoint + new XYZ(0, -0.1, 0);

                    XYZ start = pt1.Y > pt2.Y ? pt2 : pt1;
                    XYZ end = pt1.Y > pt2.Y ? pt1 : pt2;

                    if (TracePoint1.Y > start.Y && TracePoint1.Y < end.Y &&
                        TracePoint2.Y > start.Y && TracePoint2.Y < end.Y)
                    {
                        continousElement = GenericLine;
                        break;
                    }
                }
            }
            return continousElement;
        }

        private XYZ AdjustCollisionPointIfNecessary(XYZ refPoint, Element genericLine, IList<Element> inputLineElements)
        {
            Logger.logMessage("Method : AdjustCollisionPointIfNecessary");

            XYZ AdjustedPoint = refPoint;
            List<XYZ> IntersectionPoints = new List<XYZ>();

            XYZ lineStart = null, lineEnd = null;
            GenericUtils.GetlineStartAndEndPoints(genericLine, out lineStart, out lineEnd);

            XYZ LineOrientation = GenericUtils.GetLineOrientation(genericLine);

            foreach (Element element in inputLineElements) 
            {
               XYZ elemOrientation = GenericUtils.GetLineOrientation(element);

                if (!MathUtils.IsParallel(LineOrientation, elemOrientation))
                {
                    XYZ elemStart = null, elemEnd = null;
                    GenericUtils.GetlineStartAndEndPoints(element, out elemStart, out elemEnd);

                    bool bInsersects = MathUtils.GetIntersectionPoint(new PointF((float)lineStart.X, (float)lineStart.Y),
                                                    new PointF((float)lineEnd.X, (float)lineEnd.Y),
                                                    new PointF((float)elemStart.X, (float)elemStart.Y),
                                                    new PointF((float)elemEnd.X, (float)elemEnd.Y),
                                                    out PointF ptIntesectionPoint);

                    if (bInsersects)
                    {
                        XYZ intesectPoint = new XYZ(ptIntesectionPoint.X, ptIntesectionPoint.Y, lineStart.Z);
                        IntersectionPoints.Add(intesectPoint);
                    }
                }
            }

            
            XYZ nearestPoint = null;
            double minDistance = double.MaxValue;
            
            foreach (XYZ intersectionPoint in IntersectionPoints)
            {
                double distance = AdjustedPoint.DistanceTo(intersectionPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPoint = intersectionPoint;
                }
            }

            return nearestPoint == null ? AdjustedPoint : nearestPoint; 
        }

        public void PlaceObjectInClearSpace()
        {
        }
    }
}

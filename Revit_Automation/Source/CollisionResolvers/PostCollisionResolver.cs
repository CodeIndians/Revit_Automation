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

        public bool HandleCollision(CollisionObject collisionObject)
        {
            
            // Create a Outline, uses a minimum and maximum XYZ point to initialize the Bounding Box. 
            Outline myOutLn = new Outline(
                new XYZ(collisionObject.CollisionPoint.X - 0.3,
                collisionObject.CollisionPoint.Y - 0.3,
                collisionObject.CollisionPoint.Z - 0.3),
                new XYZ(collisionObject.CollisionPoint.X + 0.3,
                collisionObject.CollisionPoint.Y + 0.3,
                collisionObject.CollisionPoint.Z + 0.3));

            // Create a BoundingBoxIntersects filter with this Outline
            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);

            // Apply the filter to the elements in the active document to retrieve posts at a point
            FilteredElementCollector collector = new FilteredElementCollector(m_Document);
            IList<Element> postElements = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_StructuralColumns).ToElements();

            // Apply the filter to the elements in the active document to retrieve Input lines at a point
            FilteredElementCollector collector2 = new FilteredElementCollector(m_Document);
            IList<Element> InputLineElements = collector2.WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();


            //Check for HSS at a given location, if present delete the colliding post
            bool bhasHSS = CheckForHSS(postElements);
            if (bhasHSS)
                return true;

            //Ideally we should have 2 columns and 2 input lines in collision case.
            if (InputLineElements.Count >= 2 && postElements.Count >= 2)
            {
                
                // Identify the continous line and from it the Static Column
                Element continuousLine = IdentifyContinousLineAtPoint(collisionObject.CollisionPoint, InputLineElements);

                if (continuousLine != null)
                {
                    Element StaticColumn = IdentifyStaticPost(continuousLine, postElements);

                    //Obtain the Webwidth from the Static Column
                    double dWebWidth = StaticColumn == null ? 0 : GenericUtils.WebWidth(StaticColumn.Name);

                    // Adjust the non static posts accordingly
                    if (dWebWidth > 0)
                        MoveNonStaticColumns(InputLineElements, postElements, dWebWidth / 2, continuousLine);
                }
            }

            return false;
        }

        private bool CheckForHSS(IList<Element> postElements)
        {
            foreach (Element colElement in postElements)
            {
                FamilyInstance post = m_Document.GetElement(colElement.Id) as FamilyInstance;

                if (post.Name.Contains("HSS"))
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
                    XYZ AdjustedPostPoint = IdentifyNewPointForThePost(postorientation, post.Location, inputLineElements, distanceToMove);
                    MoveColumn(post.Id, AdjustedPostPoint);
                }
            }
        }

        private XYZ IdentifyNewPointForThePost(XYZ postorientation, Location postLocation, IList<Element> inputLineElements, double distanceToMove)
        {
            XYZ newPoint = null;

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
                        LineType lineType = MathUtils.ApproximatelyEqual(start.Y, end.Y) ? LineType.Horizontal : LineType.vertical;

                        if (bColumnAtEnd && lineType == LineType.Horizontal)
                            newPoint = location.Point + new XYZ(-distanceToMove, 0, 0);
                        else if (bColumnAtEnd && lineType == LineType.vertical)
                            newPoint = location.Point + new XYZ(0 , -distanceToMove, 0);
                        if (bColumnAtStart && lineType == LineType.Horizontal)
                            newPoint = location.Point + new XYZ(distanceToMove, 0, 0);
                        if (bColumnAtStart && lineType == LineType.vertical)
                            newPoint = location.Point + new XYZ(0 , distanceToMove, 0);

                        break;
                    }  
                }
            }
            return newPoint;
        }

        private void MoveColumn(ElementId columnId, XYZ newLocation)
        {
            if (newLocation == null || columnId == null)
                return;

            FamilyInstance column = m_Document.GetElement(columnId) as FamilyInstance;

            using (Transaction tx = new Transaction(m_Document))
            {
                GenericUtils.SupressWarningsInTransaction(tx);

                _ = tx.Start("Change Orientation");

                // Get the column's Location property
                Location location = column.Location;

                // Check if the column's location is a LocationPoint
                if (location is LocationPoint locationPoint)
                {
                    // Set the new location for the column
                    locationPoint.Point = newLocation;
                }

                _ = tx.Commit();
            }
        }

        private Element IdentifyStaticPost(Element continuousLine, IList<Element> postElements)
        {
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
            Element continousElement = null;
            XYZ TracePoint1;
            XYZ TracePoint2;

            foreach (Element GenericLine in inputLineElements)
            {
                // Get the location curve
                LocationCurve locationCurve = (LocationCurve)GenericLine.Location;
                XYZ pt1 = locationCurve.Curve.GetEndPoint(0);
                XYZ pt2 = locationCurve.Curve.GetEndPoint(1);

                LineType lineType = LineType.vertical;

                if (MathUtils.ApproximatelyEqual(pt1.Y, pt2.Y))
                {
                    lineType = LineType.Horizontal;
                }

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

        public void PlaceObjectInClearSpace()
        {
        }
    }
}

using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.Interfaces;
using System.Collections.Generic;

namespace Revit_Automation.Source.CollisionDetectors
{
    public class PostCollisionResolver : ICollisionInterface
    {
        public Document m_Document;
        public PostCollisionResolver(Document doc) { m_Document = doc; }

        public void HandleCollision(CollisionObject collisionObject)
        {
            // Create a Outline, uses a minimum and maximum XYZ point to initialize the outline. 
            Outline myOutLn = new Outline(
                new XYZ(collisionObject.CollisionPoint.X - 0.1,
                collisionObject.CollisionPoint.Y - 0.1,
                collisionObject.CollisionPoint.Z - 0.1),
                new XYZ(collisionObject.CollisionPoint.X + 0.1,
                collisionObject.CollisionPoint.Y + 0.1,
                collisionObject.CollisionPoint.Z + 0.1));

            // Create a BoundingBoxIntersects filter with this Outline
            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);

            // Apply the filter to the elements in the active document
            FilteredElementCollector collector = new FilteredElementCollector(m_Document);
            IList<Element> postElements = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_StructuralColumns).ToElements();

            FilteredElementCollector collector2 = new FilteredElementCollector(m_Document);
            IList<Element> InputLineElements = collector2.WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel).ToElements();

            //Ideally we should have 2 columns and 2 input lines in collision case.
            if (InputLineElements.Count < 2 && postElements.Count < 2)
            {
                return;
            }

            _ = IdentifyContinousLineAtPoint(collisionObject.CollisionPoint, InputLineElements);

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
                }

                if (lineType == LineType.vertical)
                {
                    TracePoint1 = collisionPoint + new XYZ(0, 0.1, 0);
                    TracePoint2 = collisionPoint + new XYZ(0, -0.1, 0);
                }

            }
            return continousElement;
        }

        public void PlaceObjectInClearSpace()
        {

        }
    }
}

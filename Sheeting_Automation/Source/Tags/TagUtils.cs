using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Sheeting_Automation.Source.Schedules;
using Sheeting_Automation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sheeting_Automation.Source.Tags.TagData;

namespace Sheeting_Automation.Source.Tags
{
    public class TagUtils
    {
        /// <summary>
        /// Returns the dictonaries of all the categories in the entire document
        /// </summary>
        /// <returns>Dictonary containing all the categories in the document</returns>
        public static Dictionary<string, ElementId> GetTaggableCategories()
        {

            ViewPlan viewPlan = SheetUtils.m_Document.ActiveView as ViewPlan;

            // return the empty dictonary if the current view is not view plan
            if (viewPlan == null)
            {
                TaskDialog.Show("Error", "Current view is not a view plan");
                return new Dictionary<string, ElementId>();
            }

            //initialize the dictionary
            Dictionary<string, ElementId> dictCategories = new Dictionary<string, ElementId>();

            // get all the elements in the current document 
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document);

            //get distinct categories of elements in the current document
            var categories = collector
                                .OfClass(typeof(ElementType))
                                .WhereElementIsElementType()
                                .Select(x => x.Category)
                                .Distinct(new CategoryComparer())
                                .ToList();

            // filter out the annotation categories
            foreach (Element element in collector)
            {
                Category category = element.Category;

                if (category != null && category.CategoryType == CategoryType.Annotation)
                {
                    if (!dictCategories.ContainsKey(category.Name))
                    {
                        dictCategories.Add(category.Name, category.Id);
                    }
                }

            }

            return dictCategories;

        }

        /// <summary>
        /// Returns the element categories in the current view 
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ElementId> GetElementCategoriesInView()
        {
            // Initialize a set to store unique categories
            Dictionary<string, ElementId> dictCategories = new Dictionary<string, ElementId>();

            // Iterate through the elements in the view and collect their categories
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id);
            foreach (Element element in collector)
            {
                // Get the category of the element
                Category category = element.Category;

                if (category != null && category.CategoryType == CategoryType.Model)
                {
                    if (!dictCategories.ContainsKey(category.Name))
                    {
                        dictCategories.Add(category.Name, category.Id);
                    }
                }
            }

            return dictCategories;
        }

        /// <summary>
        /// Returns the element family names in the current view 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public static Dictionary<string, List<ElementId>> GetElementFamilyNames(ElementId categoryId)
        {
            // Initialize an empty list to store Element IDs
            Dictionary<string, List<ElementId>> elementDict = new Dictionary<string, List<ElementId>>();

            // Use a filtered element collector to find elements of the specified category
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id);
            collector.OfCategoryId(categoryId);

            // Add the Element IDs to the list
            foreach (Element element in collector)
            {
                // Check if the element is a FamilyInstance
                if (element is FamilyInstance familyInstance)
                {
                    // Get the symbol associated with the FamilyInstance
                    FamilySymbol symbol = familyInstance.Symbol;

                    // Get the family name from the symbol
                    string familyName = symbol.Family.Name;

                    if (elementDict.ContainsKey(familyName))
                    {
                        elementDict[familyName].Add(element.Id);
                    }
                    else
                    {
                        elementDict[familyName] = new List<ElementId> { element.Id };
                    }
                }
                else if (element is Wall wallinstance)
                {
                    var familyName = wallinstance.WallType.FamilyName + " : " + wallinstance.Name;

                    if (elementDict.ContainsKey(familyName))
                    {
                        elementDict[familyName].Add(element.Id);
                    }
                    else
                    {
                        elementDict[familyName] = new List<ElementId> { element.Id };
                    }
                }


            }

            return elementDict;
        }

        /// <summary>
        /// Return the tag names in the document 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public static Dictionary<string, ElementId> GetAnnotationSymbolFamilyNames(ElementId categoryId)
        {
            // Initialize a list to store the annotation symbol family names
            Dictionary<string, ElementId> annotationSymbolFamilyNames = new Dictionary<string, ElementId>();


            // Use a filtered element collector to find family symbols in the annotation category
            FilteredElementCollector collector = new FilteredElementCollector(SheetUtils.m_Document);
            collector.OfClass(typeof(FamilySymbol)).OfCategoryId(categoryId);

            foreach (Element element in collector)
            {
                if (element is FamilySymbol familySymbol)
                {
                    string combinedName = familySymbol.Family.Name + " : " + familySymbol.Name;
                    if (!annotationSymbolFamilyNames.ContainsKey(combinedName))
                    {
                        // Add the family name to the list
                        annotationSymbolFamilyNames.Add(combinedName, familySymbol.Id);
                    }
                }
            }

            return annotationSymbolFamilyNames;
        }

        /// <summary>
        ///  Checks if the current view is view plan
        /// </summary>
        /// <returns>true if the current view is view plan, false otherwise </returns>
        public static bool IsCurrentViewPlan()
        {
            ViewPlan viewPlan = SheetUtils.m_Document.ActiveView as ViewPlan;

            return (viewPlan != null);
        }

        /// <summary>
        /// Get the no tag value 
        /// if we pass Wall tags , Walls is returned
        /// </summary>
        /// <param name="tagsValue"></param>
        /// <returns></returns>
        public static string GetNoTagValue(string tagsValue)
        {
            // noTagvalue to remove the "Tags"
            string noTagValue = tagsValue;

            if (tagsValue.Contains(" Tags"))
            {
                noTagValue = tagsValue.Replace(" Tags", "");
            }

            // check for the categories in the view 
            foreach (var key in TagData.ViewCategoriesDict.Keys)
            {
                if (key.Contains(noTagValue))
                {
                    noTagValue = key;
                    break;
                }
            }

            return noTagValue;
        }

        /// <summary>
        /// Check if curve is horizontal or vertical
        /// </summary>
        /// <param name="locationCurve"></param>
        /// <returns></returns>
        public static TagOrientation GetCurveOrientation(LocationCurve locationCurve)
        {
            // get the curve as line 
            Line curveLine = locationCurve?.Curve as Line;

            if (curveLine != null)
            {
                XYZ direction = curveLine.Direction;

                if (IsAlmostEqual(Math.Abs(direction.X), 0.0) && IsAlmostEqual(Math.Abs(direction.Y), 1.0))
                {
                    // The curve is vertical (Y-axis direction)
                    return TagOrientation.Vertical;
                }
                else if (IsAlmostEqual(Math.Abs(direction.X), 1.0) && IsAlmostEqual(Math.Abs(direction.Y), 0.0))
                {
                    // The curve is horizontal (X-axis direction)
                    return TagOrientation.Horizontal;
                }
            }

            // The curve is treated as horizontal by default
            return TagOrientation.Horizontal;
        }

        /// <summary>
        /// Helper method to compare floating-point values with tolerance
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private static bool IsAlmostEqual(double a, double b, double tolerance = 0.0001)
        {
            return Math.Abs(a - b) < tolerance;
        }

        /// <summary>
        /// List out all the independent tags in the current view
        /// </summary>
        /// <returns>List of independent tags</returns>
        public static List<IndependentTag> GetAllTagsInView()
        {
            List<IndependentTag> independentTags = new List<IndependentTag>();

            // Create a filtered element collector to find all tags in the active view
            FilteredElementCollector tagCollector = new FilteredElementCollector(SheetUtils.m_Document, SheetUtils.m_Document.ActiveView.Id);
            tagCollector.OfClass(typeof(IndependentTag));

            foreach (IndependentTag tag in tagCollector)
            {
                independentTags.Add(tag);
            }

            return independentTags;
        }

        /// <summary>
        /// List out all the independent tags in the current view
        /// </summary>
        /// <returns>List of independent tags</returns>
        public static List<IndependentTag> GetTagsWithLeaders(List<IndependentTag> tags)
        {
            List<IndependentTag> independentTags = new List<IndependentTag>();

            foreach(IndependentTag tag in tags)
            {
                if(tag.HasLeader)
                    independentTags.Add(tag);
            }

            return independentTags;
        }

        /// <summary>
        /// Check if the given two tags are intersecting
        /// </summary>
        /// <param name="tag1"></param>
        /// <param name="tag2"></param>
        /// <returns>true if intersecting else false</returns>
        public static bool AreTagsIntersecting(IndependentTag tag1, IndependentTag tag2)
        {
            return AreBoundingBoxesIntersecting(tag1.get_BoundingBox(SheetUtils.m_ActiveView)
                                                , tag2.get_BoundingBox(SheetUtils.m_ActiveView));
        }

        /// <summary>
        /// Check if the given two bounding boxes are intersecting 
        /// </summary>
        /// <param name="bbox1"></param>
        /// <param name="bbox2"></param>
        /// <returns>true if intersecting else false</returns>
        public static bool AreBoundingBoxesIntersecting(BoundingBoxXYZ bbox1, BoundingBoxXYZ bbox2)
        {
            if (bbox1 == null || bbox2 == null)
                return false;

            // Check if bbox1 is to the left of bbox2 along the X-axis
            if (bbox1.Max.X < bbox2.Min.X || bbox1.Min.X > bbox2.Max.X)
            {
                return false;
            }

            // Check if bbox1 is below bbox2 along the Y-axis
            if (bbox1.Max.Y < bbox2.Min.Y || bbox1.Min.Y > bbox2.Max.Y)
            {
                return false;
            }

            // If none of the above conditions are met, the bounding boxes intersect
            return true;
        }

        /// <summary>
        ///  check if the bounding box is intersecting with any of the bounding box in the given list
        /// </summary>
        /// <param name="bbox1">bounding box</param>
        /// <param name="bboxList">bounding box list</param>
        /// <returns></returns>
        public static bool AreBoundingBoxesIntersecting(BoundingBoxXYZ bbox1, List<BoundingBoxXYZ> bboxList)
        {
            foreach(var bbox in bboxList)
            {
                if(AreBoundingBoxesIntersecting(bbox1,bbox))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// check if the passed bounding box is intersecting with the list of list of tags
        /// </summary>
        /// <param name="bbox"> bounding box of the tag </param>
        /// <param name="skipElemId">element id of the tag that needs to be skipped</param>
        /// <param name="overlapTagsList"> overlap tags list</param>
        /// <returns></returns>
        public static bool AreBoundingBoxesIntersecting(BoundingBoxXYZ bbox, ElementId skipElemId, List<List<Tag>> overlapTagsList)
        {
            int intersectCount = 0;

            // check for overlaps with all the existing tags 
            foreach (var bbList in overlapTagsList)
            {
                foreach (var bb in bbList)
                {
                    if (bb.mElement.Id == skipElemId)
                        continue;

                    if (TagUtils.AreBoundingBoxesIntersecting(bb.newBoundingBox, bbox))
                        intersectCount++;
                }
            }

            // return false if no intersections are found 
            if(intersectCount == 0) return false;

            // return true by default
            return true;
        }

        /// <summary>
        /// check if the passed bounding box is intersecting with the list of list of tags
        /// </summary>
        /// <param name="bbox">bounding box of the tag</param>
        /// <param name="skipElemId1">first skip element id</param>
        /// <param name="skipElemId2">second skip element id </param>
        /// <param name="overlapTagsList">overlap tags list</param>
        /// <returns></returns>
        public static bool AreBoundingBoxesIntersecting(BoundingBoxXYZ bbox, ElementId skipElemId1, ElementId skipElemId2, List<List<Tag>> overlapTagsList)
        {
            int intersectCount = 0;

            // check for overlaps with all the existing tags 
            foreach (var bbList in overlapTagsList)
            {
                foreach (var bb in bbList)
                {
                    // skip the elements specified by element ids 
                    if ((bb.mElement.Id == skipElemId1 ) || bb.mElement.Id == skipElemId2)
                        continue;

                    if (TagUtils.AreBoundingBoxesIntersecting(bb.newBoundingBox, bbox))
                        intersectCount++;
                }
            }

            // return false if no intersections are found 
            if (intersectCount == 0) return false;

            // return true by default
            return true;
        }


        /// <summary>
        /// Get the bounding box the geometry object
        /// </summary>
        /// <param name="geomObject"></param>
        /// <returns> Bounding Box</returns>
        public static BoundingBoxXYZ GetBoundingBox(GeometryObject geomObject) 
        {
            BoundingBoxXYZ boundingBoxXYZ = null;
            
            Line line = geomObject as Line;
            Arc arc = geomObject as Arc;
            Solid solid = geomObject as Solid;

            if (line != null)
            {
                XYZ minPoint = new XYZ(Math.Min(line.GetEndPoint(0).X, line.GetEndPoint(1).X) - 0.1f, 
                                       Math.Min(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y) - 0.1f,
                                       line.GetEndPoint(0).Z);
                XYZ maxPoint = new XYZ(Math.Max(line.GetEndPoint(0).X, line.GetEndPoint(1).X) + 0.1f,
                                       Math.Max(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y) + 0.1f,
                                       line.GetEndPoint(0).Z);

                boundingBoxXYZ = new BoundingBoxXYZ();
                boundingBoxXYZ.Min = minPoint;
                boundingBoxXYZ.Max = maxPoint;
            }

            if(arc != null)
            {
                XYZ midpoint = arc.Center; // Midpoint of the Arc
                double radius = arc.Radius;

                XYZ minPoint = new XYZ(midpoint.X - radius, midpoint.Y - radius, midpoint.Z - radius);
                XYZ maxPoint = new XYZ(midpoint.X + radius, midpoint.Y + radius, midpoint.Z + radius);

                boundingBoxXYZ = new BoundingBoxXYZ();
                boundingBoxXYZ.Min = minPoint;
                boundingBoxXYZ.Max = maxPoint;

            }

            if (solid != null)
            {
                // return null if the surface area is almost zero
                if (!IsAlmostEqual(solid.SurfaceArea, 0.0, 0.0001))
                {
                    List<XYZ> edgeVertices = new List<XYZ>();

                    // Iterate through the edges of the solid and collect their vertices
                    foreach (Edge edge in solid.Edges)
                    {
                        IList<XYZ> edgePoints = edge.Tessellate();
                        foreach (XYZ point in edgePoints)
                        {
                            edgeVertices.Add(point);
                        }
                    }

                    // Calculate the minimum and maximum coordinates from the edge vertices
                    double minX = edgeVertices.Min(p => p.X);
                    double minY = edgeVertices.Min(p => p.Y);
                    double minZ = edgeVertices.Min(p => p.Z);

                    double maxX = edgeVertices.Max(p => p.X);
                    double maxY = edgeVertices.Max(p => p.Y);
                    double maxZ = edgeVertices.Max(p => p.Z);

                    boundingBoxXYZ = new BoundingBoxXYZ
                    {
                        Min = new XYZ(minX, minY, minZ),
                        Max = new XYZ(maxX, maxY, maxZ)
                    };

                }
            }

            return boundingBoxXYZ;
        }

        public static List<BoundingBoxXYZ> GetBoundingBoxes(Leader leader)
        {
            List<BoundingBoxXYZ> boundingBoxesList = new List<BoundingBoxXYZ>();

            XYZ minPoint = new XYZ(Math.Min(leader.Anchor.X, leader.Elbow.X) - 0.1f,
                                       Math.Min(leader.Anchor.Y, leader.Elbow.Y) - 0.1f,
                                       leader.Anchor.Z);
            XYZ maxPoint = new XYZ(Math.Max(leader.Anchor.X, leader.Elbow.X) + 0.1f,
                                   Math.Max(leader.Anchor.Y, leader.Elbow.Y) + 0.1f,
                                   leader.Anchor.Z);

            var boundingBoxXYZ = new BoundingBoxXYZ();
            boundingBoxXYZ.Min = minPoint;
            boundingBoxXYZ.Max = maxPoint;
            boundingBoxesList.Add(boundingBoxXYZ);

            minPoint = new XYZ(Math.Min(leader.End.X, leader.Elbow.X) - 0.1f,
                                       Math.Min(leader.End.Y, leader.Elbow.Y) - 0.1f,
                                       leader.End.Z);
            maxPoint = new XYZ(Math.Max(leader.End.X, leader.Elbow.X) + 0.1f,
                                   Math.Max(leader.End.Y, leader.Elbow.Y) + 0.1f,
                                   leader.End.Z);

            boundingBoxXYZ = new BoundingBoxXYZ();
            boundingBoxXYZ.Min = minPoint;
            boundingBoxXYZ.Max = maxPoint;
            boundingBoxesList.Add(boundingBoxXYZ);


            return boundingBoxesList;
        }

        /// <summary>
        /// Get the family name of the passed element
        /// </summary>
        /// <param name="element">Element</param>
        /// <returns>family string name </returns>
        public static string GetFamilyNameOfElement(Element element)
        {
            // Check if the element has a valid Family object
            if (element is FamilyInstance familyInstance)
            {
                Family family = familyInstance.Symbol.Family;

                if (family != null)
                {
                    return family.Name;
                }
            }

            // Return null or an appropriate default value if the element doesn't have a family
            return null;
        }

        /// <summary>
        /// Get the nearest element bounding boxes 
        /// </summary>
        /// <param name="tag">custom tag struct</param>
        /// <param name="boundingBoxesDict">bounding box dictionary</param>
        /// <returns></returns>
        public static List<BoundingBoxXYZ> GetNearestElementBoundingBoxes(Tag tag, ref Dictionary<ElementId, List<BoundingBoxXYZ>> boundingBoxesDict)
        {
            List<BoundingBoxXYZ> nearestBoundingBoxes = new List<BoundingBoxXYZ>();

            //offset for creatting a reference bouding box
            XYZ offset = new XYZ(7, 7, 0);

            // create a reference bounding box from the element bounding box
            var referenceBoundingBox = new BoundingBoxXYZ();
            referenceBoundingBox.Min = tag.currentBoundingBox.Min - offset;
            referenceBoundingBox.Max = tag.currentBoundingBox.Max + offset;



            foreach (var kvp in boundingBoxesDict)
            {
                var elementBoundingBoxes = kvp.Value;
                ElementId elementId = kvp.Key;

                if (tag.mElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFraming)
                {
                    Element checkElement = SheetUtils.m_Document.GetElement(elementId);

                    if (checkElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFraming)
                    {
                        // skip different family elements for structural framing types 
                        if (TagUtils.GetFamilyNameOfElement(tag.mElement) != TagUtils.GetFamilyNameOfElement(checkElement))
                            continue;
                    }
                }

                if (elementBoundingBoxes != null)
                {
                    foreach (var elemBoundingBox in elementBoundingBoxes)
                    {
                        if (TagUtils.AreBoundingBoxesIntersecting(elemBoundingBox, referenceBoundingBox))
                            nearestBoundingBoxes.Add(elemBoundingBox);
                    }
                }
            }

            return nearestBoundingBoxes;
        }

        /// <summary>
        /// Get the orientation of the tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>Tag orientation</returns>
        public static TagOrientation GetTagOrientation(Tag tag)
        {
            TagOrientation tagOrientation = TagOrientation.Horizontal;

            var boundingBox = tag.currentBoundingBox;

            var width = Math.Abs(boundingBox.Max.X - boundingBox.Min.X);
            var height = Math.Abs(boundingBox.Max.Y - boundingBox.Min.Y);

            if (width < height)
                tagOrientation = TagOrientation.Vertical;

            return tagOrientation;
        }

        public static double GetBBRatio(Tag tag)
        {
            // default ratio is 1
            double ratio = 1.0;

            // get the x length and y length of the tag bounding box
            var xTagLength = Math.Abs(tag.currentBoundingBox.Max.X - tag.currentBoundingBox.Min.X);
            var yTagLength = Math.Abs(tag.currentBoundingBox.Max.Y - tag.currentBoundingBox.Min.Y);

            var elementBoundingBox = BoundingBoxCollector.BoundingBoxesDict[tag.mElement.Id].FirstOrDefault();

            //get the x length and y length of the element bounding box 
            var xElementLength = Math.Abs(elementBoundingBox.Max.X - elementBoundingBox.Min.X);
            var yElementLength = Math.Abs(elementBoundingBox.Max.Y - elementBoundingBox.Min.Y);

            // tag is vertical
            if (xTagLength < yTagLength)
            {
                // Y lengths 
                ratio = yTagLength / yElementLength;
            }
            else //tag is horizontal
            {
                // X lengths 
                ratio = xTagLength / xElementLength;
            }

            return ratio;
        }

        /// <summary>
        /// Get the distance between two bounding boxes ( mid points ) 
        /// </summary>
        /// <param name="boundingBox">bounding box that needs to be checked</param>
        /// <param name="elementBoundingBox">element bounding box </param>
        /// <returns></returns>
        public static double GetDistanceFromElement(BoundingBoxXYZ boundingBox, BoundingBoxXYZ elementBoundingBox)
        {
            XYZ boundingBoxMidPoint = (boundingBox.Max + boundingBox.Min) / 2;
            XYZ elementMidPoint = (elementBoundingBox.Max + elementBoundingBox.Min) / 2; ;

           return boundingBoxMidPoint.DistanceTo(elementMidPoint);
        }
    }


}

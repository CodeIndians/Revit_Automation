// This file is part of the  R A N O R E X  Project. | http://www.ranorex.com

using Autodesk.Revit.DB;
using Shop_Automation.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shop_Automation.Source
{
    

    public class AssemblyGenerator
    {
        static int s_stconnections = 0;
        static int s_stGenericModels = 0;
        static int st_Doors = 0;

        public static List<List<ElementId>> colAssemblies = new List<List<ElementId>>();
        public static Document doc;
        public static void GenerateAssemblies()
        {
            GetDetailsFromBluePrint();

            CollectElements();

            /*foreach (List<ElementId> listElemIds in colAssemblies)
            {
                using (Transaction transaction = new Transaction(doc))
                {

                    // First element should always be a Wall Type so that we can get Wall category naming ID
                    ElementId categoryId = doc.GetElement(listElemIds.First()).Category.Id;
                    transaction.Start("Create Assembly Instance");

                    if (AssemblyInstance.AreElementsValidForAssembly(doc, listElemIds, listElemIds.First()))
                    {
                        // Create an assembly with given elements.
                        AssemblyInstance assemblyInstance = AssemblyInstance.Create(doc, listElemIds, categoryId);
                        transaction.Commit(); // need to commit the transaction to complete the creation of the assembly instance so it can be accessed in the code below

                        if (transaction.GetStatus() == TransactionStatus.Committed)
                        {
                            transaction.Start("Set Assembly Name");
                            Wall wall = doc.GetElement(listElemIds.First()) as Wall;

                            Parameter mark = wall.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
                            if (mark != null)
                            {
                                // Rename the assembly as per the name in the mask parameter
                                assemblyInstance.AssemblyTypeName = mark.AsValueString();
                            }

                            transaction.Commit();
                        }

                        if (assemblyInstance.AllowsAssemblyViewCreation()) // check to see if views can be created for this assembly
                        {

                            if (transaction.GetStatus() == TransactionStatus.Committed)
                            {

                                transaction.Start("View Creation");
                                
                                // Retrieve title block and orientation of the wall
                                ElementId titleBlockId = GetTitleBlockID();
                                AssemblyDetailViewOrientation orientation = GetOrientation(listElemIds.First()); // First element should always be a Wall Type
                                
                                // Create Sheet
                                ViewSheet viewSheet = AssemblyViewUtils.CreateSheet(doc, assemblyInstance.Id, titleBlockId);

                                // Create Elevation view based on the orienatation 
                                ViewSection elevationTop = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.ElevationTop);
                                ViewSection elevationLeft = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.ElevationLeft);
                                ViewSection elevationRight = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.ElevationRight);
                                ViewSection elevationFront = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.ElevationFront);
                                ViewSection detailSectionA = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.DetailSectionA);
                                ViewSection detailSectionB = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.DetailSectionB);
                                ViewSection detailSectionH = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.HorizontalDetail);
                                ViewSchedule materialTakeoff = AssemblyViewUtils.CreateMaterialTakeoff(doc, assemblyInstance.Id);
                                ViewSchedule partList = AssemblyViewUtils.CreatePartList(doc, assemblyInstance.Id);
                                AssemblyViewUtils.CreateSingleCategorySchedule(doc, assemblyInstance.Id, BuiltInCategory.OST_Walls, );

                                Viewport.Create(doc, viewSheet.Id, elevationTop.Id, new XYZ(2, 2, 0));
                                Viewport.Create(doc, viewSheet.Id, elevationLeft.Id, new XYZ(1, 1.7, 0));
                                Viewport.Create(doc, viewSheet.Id, elevationRight.Id, new XYZ(2.5, 2, 0));
                                Viewport.Create(doc, viewSheet.Id, elevationFront.Id, new XYZ(2, 1, 0));
                                Viewport.Create(doc, viewSheet.Id, detailSectionA.Id, new XYZ(1.5, 1.25, 0));
                                Viewport.Create(doc, viewSheet.Id, detailSectionB.Id, new XYZ(0.5, 1.5, 0));
                                Viewport.Create(doc, viewSheet.Id, detailSectionH.Id, new XYZ(1.5, 2, 0));
                                ScheduleSheetInstance.Create(doc, viewSheet.Id, partList.Id, new XYZ(2.5, 2.5, 0));
                                ScheduleSheetInstance.Create(doc, viewSheet.Id, materialTakeoff.Id, new XYZ(2, 2.5, 0));
                                transaction.Commit();
                            }
                        }
                    }
                }
            }*/
        }

        private static void GetDetailsFromBluePrint()
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(AssemblyInstance));
            IEnumerable<AssemblyInstance> assemblies = collector.Cast<AssemblyInstance>().ToList();

            foreach (AssemblyInstance AI in assemblies)
            {
                Debug.WriteLine( AI.Name);
            }

        }

        /// <summary>
        /// Gets orientation of the wall -> Should always be from interior unless otherwise noted
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        private static AssemblyDetailViewOrientation GetOrientation(ElementId elementId)
        {
            Wall wall = doc.GetElement(elementId) as Wall;
            if (wall != null)
            {
                
            }
            return AssemblyDetailViewOrientation.ElevationTop;
        }

        /// <summary>
        /// Gets the title block to be associated to the sheet
        /// </summary>
        /// <returns></returns>
        private static ElementId GetTitleBlockID()
        {
            IEnumerable<FamilySymbol> collector =  new FilteredElementCollector(doc)
                                                 .OfClass(typeof(FamilySymbol))
                                                  .OfCategory(BuiltInCategory.OST_TitleBlocks)
                                                 .Cast<FamilySymbol>();
            foreach (FamilySymbol family in collector)
            {
                if (family.Name == GlobalSettings.s_SheetTemplate)
                    return family.Id;
            }

            return null;
        }

        /// <summary>
        /// Collects all the walls in the model for sheet creation
        /// </summary>
        private static void CollectElements()
        {
            // Create a filtered element collector to retrieve walls
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            // Apply a filter to retrieve only walls
            collector.OfClass(typeof(Wall));

            // Retrieve walls from the collector
            IEnumerable<Wall> walls = collector.Cast<Wall>();

            // Loop through the retrieved walls
            foreach (Wall wall in walls)
            {

                //Debug.Write(string.Format( "\n Wall {0} ", wall.Id));
                List<ElementId> elementIds = new List<ElementId>();
                
                // Add Wall and its connected elements
                elementIds.Add(wall.Id);

                // Get Connected Elements of the wall
                List<ElementId> connectedElements = GetConnectedElementsOfTheWall(wall);

                // Append to the collection
                elementIds.AddRange(connectedElements);

            }
        }

        /// <summary>
        /// Collects all the elements that are within the range of a given wall
        /// </summary>
        private static List<ElementId> GetConnectedElementsOfTheWall(Wall wall)
        {
            List<ElementId> elementIDs = new List<ElementId>();

            BoundingBoxXYZ boundingBox = wall.get_BoundingBox(doc.ActiveView);
            Outline outline = new Outline(boundingBox.Min, boundingBox.Max );
            
            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);


            // Apply the filter to the elements in the active document to retrieve Structural Connections
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<ElementId> structConnectionElements = collector.WherePasses(filter).OfCategory(BuiltInCategory.OST_StructConnections).ToElementIds();

            // Apply the filter to the elements in the active document to retrieve Input lines at a point
            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
            ICollection<ElementId> GenericModelElements = collector2.WherePasses(filter).OfCategory(BuiltInCategory.OST_GenericModel).ToElementIds();
            ICollection<ElementId> filteredGenericModelElements = FilterGenericModels(wall, GenericModelElements);

            // Apply the filter to the elements in the active document to retrieve Doors
            FilteredElementCollector collector3 = new FilteredElementCollector(doc);
            ICollection<ElementId> OpeningElements = collector3.WherePasses(filter).OfCategory(BuiltInCategory.OST_Doors).ToElementIds();

            //  Append all the collected elements
            elementIDs.AddRange(structConnectionElements); s_stconnections += structConnectionElements.Count;
            elementIDs.AddRange(filteredGenericModelElements);s_stGenericModels += filteredGenericModelElements.Count;
            elementIDs.AddRange(OpeningElements);st_Doors += OpeningElements.Count; 

            return elementIDs;
        }

        private static ICollection<ElementId> FilterGenericModels(Wall wall, ICollection<ElementId> genericModelElements)
        {
            BoundingBoxXYZ bb = wall.get_BoundingBox(doc.ActiveView);
            XYZ rangeMin = new XYZ ((bb.Min.X - 1), (bb.Min.Y - 1), bb.Min.Z);
            XYZ rangeMax = new XYZ ((bb.Max.X + 1), ( bb.Max.Y + 1), bb.Max.Z);

            ICollection<ElementId> retCol = new List<ElementId>();

            foreach (ElementId element in genericModelElements)
            {
                FamilyInstance genericModelFamily = doc.GetElement(element) as FamilyInstance;
                ElementId symbolId = genericModelFamily.Symbol.Id;

                FamilySymbol symbol = doc.GetElement(symbolId) as FamilySymbol;

                if (symbol.FamilyName == "FF Line" || symbol.FamilyName == "Support Line")
                {
                    LocationCurve lc = genericModelFamily.Location as LocationCurve;
                    if (lc != null)
                    {
                        XYZ lineStart = lc.Curve.GetEndPoint(0);
                        XYZ lineEnd = lc.Curve.GetEndPoint(1);
                        if (lineStart.X > rangeMin.X && lineStart.Y > rangeMin.Y &&
                            lineStart.X < rangeMax.X && lineStart.Y < rangeMax.Y &&
                            lineEnd.X > rangeMin.X && lineEnd.Y > rangeMin.Y &&
                            lineEnd.X < rangeMax.X && lineEnd.Y < rangeMax.Y)
                        {
                            retCol.Add(element);
                        }

                    }

                }
                else if (symbol.FamilyName == "Knouck Out Opening")
                {
                    Debug.Write(string.Format("{0},",element));
                    retCol.Add(element);
                } 
            }
            return retCol;
        }
    }
}



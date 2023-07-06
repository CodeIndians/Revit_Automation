using Autodesk.Revit.DB;
using Sheeting_Automation.Source.GeometryCollectors;
using Sheeting_Automation.Source.Interfaces;
using System;
using System.Collections.Generic;
using static Sheeting_Automation.Source.Interfaces.DimensionPlacement;

namespace Sheeting_Automation.Source.Dimensions
{
    internal class DimensionManager
    {
        Document mDocument;

        List<DimensionPlacement> mDimensionPlacements;

        List<DimensionLine> mAllDimensionLines;

        public DimensionManager(ref Document doc) 
        {
            mDocument = doc;
            mAllDimensionLines = new List<DimensionLine>();
            mDimensionPlacements = new List<DimensionPlacement>();
            Initialize();
        }

        private void AddPlacementObject(ref DimensionPlacement placement)
        {
            mDimensionPlacements.Add(placement);
        }

        private void Initialize()
        {
            var gridDimension = new GridDimensionPlacement(new GridCollector(ref mDocument), new CropRegionCollector(ref mDocument)) as DimensionPlacement;
            AddPlacementObject(ref gridDimension);
        }

        public void PlaceDimensions()
        {
            
            // Start a new transaction
            using (Transaction transaction = new Transaction(mDocument))
            {
                foreach (var placement in mDimensionPlacements)
                {
                    mAllDimensionLines.AddRange(placement.DimensionLines);
                }

                transaction.Start("Create Dimension");

                // Retrieve the active view for placing the dimension
                View activeView = mDocument.ActiveView;



                foreach (var dimLine in mAllDimensionLines)
                {
                    // create the new dimension
                   mDocument.Create.NewDimension(mDocument.ActiveView, dimLine.line, dimLine.referencesArray);
                }

                // Commit the transaction
                transaction.Commit();
            }
        }
    }
}

/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

using Autodesk.Revit.DB;
using Revit_Automation.Source;
using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace Revit_Automation
{
    internal class CeeHeaderBoundaries
    {
        private static Grid m_SpanStartingGrid;
        private static Grid m_NorthGrid;
        private static Grid m_SouthGrid;
        private static Grid m_EastGrid;
        private static Grid m_WestGrid;
        public static List<ElementId> selectedInputlines = new List<ElementId>();
        public static bool bSelectedModelling = false;
        internal static void SetBoundaries(Grid northgrid, Grid southgrid, Grid eastgrid, Grid westgrid)
        {
            m_NorthGrid = northgrid;
            m_SouthGrid = southgrid;
            m_EastGrid = eastgrid;
            m_WestGrid = westgrid;
           
        }

        internal static void SetSpanStartingGrid(Grid spanStartGrid)
        {
           m_SpanStartingGrid = spanStartGrid;
        }
        internal static List<XYZ> GetSpanStartingGrid()
        {
            List<XYZ> startingGridCoords = new List<XYZ>();

            XYZ startpt = null, endpt = null;
            GenericUtils.GetlineStartAndEndPoints(m_SpanStartingGrid.Curve, out startpt, out endpt);

            startingGridCoords.Add(startpt);
            startingGridCoords.Add(endpt);
            
            return startingGridCoords;
        }

        internal static List<XYZ> GetFirstBoundingBoxCoordinates()
        {
            LineType spanGridType = MathUtils.ApproximatelyEqual(m_SpanStartingGrid.Curve.GetEndPoint(0).X, m_SpanStartingGrid.Curve.GetEndPoint(1).X) ? LineType.vertical : LineType.Horizontal;

            Grid curve1 = null, curve2 = null;
            curve1 = (spanGridType == LineType.vertical) ? m_NorthGrid : m_EastGrid;
            curve2 = (spanGridType == LineType.vertical) ? m_SouthGrid : m_WestGrid;

            XYZ curve1Start = null, curve1End = null;
            GenericUtils.GetlineStartAndEndPoints(curve1.Curve, out curve1Start, out curve1End);

            XYZ curve2Start = null, curve2End = null;
            GenericUtils.GetlineStartAndEndPoints(curve2.Curve, out curve2Start, out curve2End);

            List<XYZ> result = new List<XYZ>();
            result.Add(curve2Start);
            result.Add(curve1Start);

            return result;
        }

        internal static XYZ GetExtentsEndPoint()
        {
            LineType spanGridType = MathUtils.ApproximatelyEqual(m_SpanStartingGrid.Curve.GetEndPoint(0).X, m_SpanStartingGrid.Curve.GetEndPoint(1).X) ? LineType.vertical : LineType.Horizontal;

            Grid curve1 = null, curve2 = null;
            curve1 = (spanGridType == LineType.vertical) ? m_EastGrid : m_NorthGrid;

            XYZ curve1Start = null, curve1End = null;
            GenericUtils.GetlineStartAndEndPoints(curve1.Curve, out curve1Start, out curve1End);

            return curve1Start;
        }
    }
}
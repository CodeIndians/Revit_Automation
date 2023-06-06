using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace Revit_Automation.CustomTypes
{

    /// <summary>
    /// Input line structure to parse all the required properties
    /// </summary>
    public struct InputLine
    {
        public LocationCurve locationCurve;
        public XYZ startpoint;
        public XYZ endpoint;
        public ElementId id;
        public string strT62Guage;
        public string strT62Type;
        public string strStudGuage;
        public string strStudType;
        public string strWallType;
        public string strTopTrackGuage;
        public string strTopTrackSize;
        public string strBottomTrackGuage;
        public string strBottomTrackSize;
        public string strBuildingName;
        public string strDoubleStudType;
        public string strMaterialType;
        public int dFlangeOfset;
        public double dOnCenter;
        public double dParapetHeight;
        public DirectionWithRespectToRoofSlope dirWRTRoofSlope;
        public List<XYZ> gridIntersectionPoints;
        public List<XYZ> mainGridIntersectionPoints;
    }

    public struct FloorObject
    {
        public XYZ min;
        public XYZ max;
        public ElementId levelID;
        public ElementId elemID;
        public string strBuildingName;
    }

    public struct RoofObject
    {
        public XYZ min;
        public XYZ max;
        public Curve slopeLine;
        public string strBuildingName;
        public ElementId roofElementID;
    }

    public struct CollisionObject
    {
        public XYZ CollisionPoint;
        public ElementId inputLineID;
        public ElementId collisionElementID;
    }
    public enum DirectionWithRespectToRoofSlope
    {
        Parallel = 0,
        Perpendicular
    }
}


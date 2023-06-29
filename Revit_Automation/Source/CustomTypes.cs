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
        public string strAdditionalPanelGuage;
        public string strAdditionalPanel;
        public string strBeamSize;
        public string strBracing;
        public string strCHeaderGuage;
        public string strCHeaderQuantity;
        public string strCHeaderSize;
        public string strColor;
        public string strHSSType;
        public string strPanelType;
        public string strMaterial;
        public string strPartitionPanelGuage;
        public string strRoofSystem;
        public string strRowName;
        public string strColorDoorHeader;
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
        public int dPartitionPanelEachSide;
        public double dOnCenter;
        public double dPanelOffsetHeight;
        public double dMaterialThickness;
        public double dMaterialHeight;
        public double dHSSHeight;
        public double dParapetHeight;
        public DirectionWithRespectToRoofSlope dirWRTRoofSlope;
        public List<XYZ> gridIntersectionPoints;
        public List<XYZ> mainGridIntersectionPoints;
        public bool bLineExtendedOrTrimmed;
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

    public struct PanelTypeGlobalParams
    {
        public string strWallName;
        public bool bIsUNO;
        public double iPanelGuage;
        public double iPanelClearance;
        public double iPanelMaxLap;
        public double iPanelMinLap;
        public string strPanelOrientation;
        public double iPanelPreferredLength;
        public double iPanelMaxLength;
        public double iPanelHeightOffset;
        public string strPanelHorizontalDirection;
        public string strPanelVerticalDirection;
        public double iPanelHourRate;
    }

    public enum WallPriority
    {
        Fire = 7,
        ExWithoutInsulation = 6,
        Insulation = 5,
        Ex =4,
        LB = 3,
        LBS = 2,
        NLBS = 1,
        NLB = 0
    }

}


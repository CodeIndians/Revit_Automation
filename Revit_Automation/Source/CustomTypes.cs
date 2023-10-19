using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Security.Policy;

namespace Revit_Automation.CustomTypes
{

    /// <summary>
    /// Input line structure to parse all the required properties
    /// </summary>
    public struct InputLine
    {
        public LocationCurve locationCurve { get; set; }
        public XYZ startpoint { get; set; }
        public XYZ endpoint { get; set; }
        public ElementId id { get; set; }
        public string strAdditionalPanelGuage { get; set; }
        public string strAdditionalPanel { get; set; }
        public string strBeamSize { get; set; }
        public string strBracing { get; set; }
        public string strCHeaderGuage { get; set; }
        public string strCHeaderQuantity { get; set; }
        public string strCHeaderSize { get; set; }
        public string strColor { get; set; }
        public string strHSSType { get; set; }
        public string strHorizontalPanelDirection { get; set; }
        public string strVerticalPanelDirection { get; set; }
        public string strPanelType { get; set; }
        public string strMaterial { get; set; }
        public string strPartitionPanelGuage { get; set; }
        public string strRoofSystem { get; set; }
        public string strRowName { get; set; }
        public string strColorDoorHeader { get; set; }
        public string strT62Guage { get; set; }
        public string strT62Type { get; set; }
        public string strStudGuage { get; set; }
        public string strStudType { get; set; }
        public string strWallType { get; set; }
        public string strTopTrackGuage { get; set; }
        public string strTopTrackSize { get; set; }
        public string strBottomTrackGuage { get; set; }
        public string strBottomTrackSize { get; set; }
        public string strBottomTrackPunch { get; set; }
        public string strBuildingName { get; set; }
        public string strDoubleStudType { get; set; }
        public string strMaterialType { get; set; }
        public int dFlangeOfset { get; set; }
        public int dPartitionPanelEachSide { get; set; }
        public double dOnCenter { get; set; }
        public double dPanelOffsetHeight { get; set; }
        public double dMaterialThickness { get; set; }
        public double dMaterialHeight { get; set; }
        public double dHSSHeight { get; set; }
        public double dParapetHeight { get; set; }
        public DirectionWithRespectToRoofSlope dirWRTRoofSlope { get; set; }
        public List<XYZ> gridIntersectionPoints { get; set; }
        public List<XYZ> mainGridIntersectionPoints { get; set; }
        public bool bLineExtendedOrTrimmed { get; set; }
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

    public struct CeeHeaderSettings
    {
        public bool bIsValidGrid;
        public string strGridName;
        public string ceeHeaderName;
        public string ceeHeaderCount;
        public string HallwayCeeHeaderName;
        public string HallwayCeeHeaderCount;
    }

    public struct CeeHeaderAdjustments
    {
        public string strCeeHeaderName;
        public int iCeeHeaderCount;
        public string postType;
        public string postGuage;
        public int postCount;
        public bool bChangeOrientation;
    }
    public enum WallPriority
    {
        Fire = 7,
        ExWithoutInsulation = 6,
        Insulation = 5,
        Ex = 4,
        LB = 3,
        LBS = 2,
        NLBS = 1,
        NLB = 0
    }

    public enum PanelDirection
    {
        B = 0, // Both
        L = 1, // Left
        R = 2, // Right
        U = 3, // Up 
        D = 4 // Down
    }

    public enum SortingStrategy
    {
        PreferredLength = 0,
        MaxLength = 1,
        Partitions = 2,
        MinimumJoints = 3
    }
    public enum LineRelations
    {
        StartExtendRight,
        StartExtendLeft,
        StartTrimRight,
        StartTrimLeft,
        StartTrimT,
        EndExtendRight,
        EndExtendLeft,
        EndTrimRight,
        EndTrimLeft,
        EndTrimT,
        NoStartIntersection,
        NoEndIntersection
    }

    public enum Direction
    {
        StartToEnd,
        EndToStart,
        NoDirection
    }

    public struct FramingSettings
    {
        public double dCeeHeaderDeckSpan;
        public double dCeeHeaderMaxLength;
        public double dDragStuctMaxLength;
        public string strDragStructType;
        public bool bDragStructContinuousAtHallway;
        public double dDragStrutLap;
        public double dEaveStructMaxLength;
        public string strEaveStructType;
        public string strEaveStructLocation;
        public double dEaveStrutLap;
        public string strFloorDeckType;
        public double dFloorDeckOverlap;
        public double dFloorDeckMaxSpan;
        public double dFloorDeckMaxLength;
        public string strRoofDeckType;
        public double dRoofDeckOverlap;
        public double dRoofDeckMaxSpan;
        public double dRoofDeckMaxLength;
        public bool bNLBSpliceAtRoof;
        public bool bTopTrackAtRakeSide;
        public bool bTopTrackSpliceAtWeb;
        public bool bToptrackRounfOff;
        public double dPurlinLap;
        public double dPurlinPreferredLength;
        public double dPurlinMaxSpans;
        public bool bPurlinContAtHallway;
        public bool bPurlOrientationChange;
        public bool bPurlinOverhang;
        public bool bPurlinRoundOff;
        public string strRecieverChannelType;
        public string strRecieverChannelGauge;
    }


    public struct PurlinTypeSettings
    {
        public double dOnCenter;
        public string strPurlinType;
        public string strPurlinGauge;
    }
}


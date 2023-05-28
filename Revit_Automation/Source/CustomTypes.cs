using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

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
        public double dFlangeOfset;
        public double dOnCenter;

        public List<XYZ> gridIntersectionPoints;
        public List<XYZ> mainGridIntersectionPoints;
    }

    public struct FloorObject
    {
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
    }
}


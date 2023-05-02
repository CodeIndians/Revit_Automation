using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.CustomTypes
{

    /// <summary>
    /// Input line structure to parse all the required properties
    /// </summary>
    struct InputLine
    {
        public LocationCurve locationCurve;
        public string strT62Guage;
        public string strT62Type;
        public string strStudGuage;
        public string strStudType;
        public string strWallType;
        public string strTopTrackGuage;
        public string strTopTrackSize;
        public string strBottomTrackGuage;
        public string strBottomTrackSize;
        public double dFlangeOfset;
        public double dOnCenter;
        public List<XYZ> gridIntersectionPoints;
    }
}


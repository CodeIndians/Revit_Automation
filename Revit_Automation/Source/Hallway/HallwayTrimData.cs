using Revit_Automation.CustomTypes;
using System.Collections.Generic;
using System.Data;

namespace Revit_Automation.Source.Hallway
{
    public static class HallwayTrimData
    {
        // horizontal lines data mapped to first data grid
        public static DataTable TrimDataHorizontal = new DataTable();

        // vertical lines data mapped to second data grid
        public static DataTable TrimDataVertical = new DataTable();

        public static List<HallwayLabelLine> HorizontalLabelLines = new List<HallwayLabelLine>();
        public static List<HallwayLabelLine> VerticalLabelLines = new List<HallwayLabelLine>();

        public static bool Validate()
        {


            if (TrimDataHorizontal.Rows.Count > 0 && TrimDataVertical.Rows.Count > 0)
            {
                // Data validation code
                foreach (DataRow row in TrimDataHorizontal.Rows)
                {


                    int top = int.Parse((row["Top"]).ToString());
                    int bottom = int.Parse((row["Bottom"]).ToString());

                    //if (top == 2 && bottom == 2)
                    //    return false;
                    //else if (top == 2 && bottom == 1)
                    //    return false;
                    //else if (top == 1 && bottom == 2)
                    //    return false;

                    // at any time, only one of them should be set
                    if (top != 0 && bottom != 0)
                        return false;

                }

                foreach (DataRow row in TrimDataVertical.Rows)
                {
                    int left = int.Parse((row["Left"]).ToString());
                    int right = int.Parse((row["Right"]).ToString());

                    //if (left == 2 && right == 2)
                    //    return false;
                    //else if (left == 2 && right == 1)
                    //    return false;
                    //else if (left == 1 && right == 2)
                    //    return false;

                    // at any time, only one of them should be set
                    if (left != 0 && right != 0)
                        return false;
                }


                //return true for now 
                return true;
            }

            return false;
        }
    }
}

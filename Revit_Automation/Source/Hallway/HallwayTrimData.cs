using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Revit_Automation.Source.Hallway.HallwayGenerator;

namespace Revit_Automation.Source.Hallway
{
    public static class HallwayTrimData
    {
        public static DataTable TrimDataHorizontal = new DataTable();

        public static DataTable TrimDataVertical = new DataTable();

        public static List<LabelLine> HorizontalLabelLines = new List<LabelLine>();
        public static List<LabelLine> VerticalLabelLines = new List<LabelLine>();

        public static bool Validate()
        {
            FileWriter.WriteDataTableToFile(TrimDataHorizontal, @"C:\temp\hor_label_data");
            FileWriter.WriteDataTableToFile(TrimDataVertical, @"C:\temp\ver_label_data");



            if (TrimDataHorizontal.Rows.Count > 0 && TrimDataVertical.Rows.Count > 0)
            {
                // Data validation code
                foreach(DataRow row in TrimDataHorizontal.Rows)
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

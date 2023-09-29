using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.ModelCreators.TopTrack
{
    internal class TopTrackRelations
    {
        public static Dictionary<string, string> SingleSelfPerpendicular = new Dictionary<string, string>();
        public static Dictionary<string, string> MultipleSelfPerpendicular = new Dictionary<string, string>();
        public static Dictionary<string, string> SelfParallel = new Dictionary<string, string>();
        public static Dictionary<string, string> SelfParallelHallway = new Dictionary<string, string>();
        public TopTrackRelations() {

            // Single Self and Perpendicular
            SingleSelfPerpendicular["LB-LB"] = "T2";
            SingleSelfPerpendicular["LB-NLB"] = "T2";
            SingleSelfPerpendicular["LBS-Insulation"] = "T2";
            SingleSelfPerpendicular["LBS-Fire"] = "T2";
            SingleSelfPerpendicular["LB-Ex"] = "T3";
            SingleSelfPerpendicular["LB-Ex w/ Insulation"] = "T2";
            SingleSelfPerpendicular["NLB-LB"] = "T1";
            SingleSelfPerpendicular["NLB-NLB"] = "T1";
            SingleSelfPerpendicular["NLB-Insulation"] = "T2";
            SingleSelfPerpendicular["NLB-Fire"] = "T2";
            SingleSelfPerpendicular["NLB-Ex"] = "T3";
            SingleSelfPerpendicular["NLB-Ex w/ Insulation"] = "T2";
            SingleSelfPerpendicular["Insulation-LB"] = "T2";
            SingleSelfPerpendicular["Insulation-NLB"] = "T2";
            SingleSelfPerpendicular["Insulation-Insulation"] = "T1";
            SingleSelfPerpendicular["Insulation-Fire"] = "T2";
            SingleSelfPerpendicular["Insulation-Ex"] = "T2";
            SingleSelfPerpendicular["Insulation-Ex w/ Insulation"] = "T1";
            SingleSelfPerpendicular["Fire-LB"] = "T2";
            SingleSelfPerpendicular["Fire-NLB"] = "T2";
            SingleSelfPerpendicular["Fire-Insulation"] = "T2";
            SingleSelfPerpendicular["Fire-Fire"] = "T1";
            SingleSelfPerpendicular["Fire-Ex"] = "T2";
            SingleSelfPerpendicular["Fire-Ex w/ Insulation"] = "T2";
            SingleSelfPerpendicular["Ex-LB"] = "T2";
            SingleSelfPerpendicular["Ex-NLB"] = "T2";
            SingleSelfPerpendicular["Ex-Insulation"] = "T2";
            SingleSelfPerpendicular["Ex-Fire"] = "T2";
            SingleSelfPerpendicular["Ex-Ex"] = "T1";
            SingleSelfPerpendicular["Ex-Ex w/ Insulation"] = "T2";
            SingleSelfPerpendicular["Ex w/ Insulation-LB"] = "T2";
            SingleSelfPerpendicular["Ex w/ Insulation-NLB"] = "T2";
            SingleSelfPerpendicular["Ex w/ Insulation-Insulation"] = "T2";
            SingleSelfPerpendicular["Ex w/ Insulation-Fire"] = "T2";
            SingleSelfPerpendicular["Ex w/ Insulation-Ex"] = "T2";
            SingleSelfPerpendicular["Ex w/ Insulation-Ex w/ Insulation"] = "T1";

            // Multiple Self and Perpendicular
            MultipleSelfPerpendicular["LB-LB"] = "T2";
            MultipleSelfPerpendicular["LB-NLB"] = "T2";
            MultipleSelfPerpendicular["LBS-Insulation"] = "T2";
            MultipleSelfPerpendicular["LBS-Fire"] = "T2";
            MultipleSelfPerpendicular["LB-Ex"] = "T2";
            MultipleSelfPerpendicular["LB-Ex w/ Insulation"] = "T2";
            MultipleSelfPerpendicular["NLB-LB"] = "T4";
            MultipleSelfPerpendicular["NLB-NLB"] = "T5";
            MultipleSelfPerpendicular["NLB-Insulation"] = "T2";
            MultipleSelfPerpendicular["NLB-Fire"] = "T2";
            MultipleSelfPerpendicular["NLB-Ex"] = "T2";
            MultipleSelfPerpendicular["NLB-Ex w/ Insulation"] = "T2";
            MultipleSelfPerpendicular["Insulation-LB"] = "T2";
            MultipleSelfPerpendicular["Insulation-NLB"] = "T2";
            MultipleSelfPerpendicular["Insulation-Insulation"] = "T4";
            MultipleSelfPerpendicular["Insulation-Fire"] = "T2";
            MultipleSelfPerpendicular["Insulation-Ex"] = "T2";
            MultipleSelfPerpendicular["Insulation-Ex w/ Insulation"] = "T2";
            MultipleSelfPerpendicular["Fire-LB"] = "T2";
            MultipleSelfPerpendicular["Fire-NLB"] = "T2";
            MultipleSelfPerpendicular["Fire-Insulation"] = "T2";
            MultipleSelfPerpendicular["Fire-Fire"] = "T1";
            MultipleSelfPerpendicular["Fire-Ex"] = "T2";
            MultipleSelfPerpendicular["Fire-Ex w/ Insulation"] = "T2";
            MultipleSelfPerpendicular["Ex-LB"] = "T2";
            MultipleSelfPerpendicular["Ex-NLB"] = "T2";
            MultipleSelfPerpendicular["Ex-Insulation"] = "T2";
            MultipleSelfPerpendicular["Ex-Fire"] = "T2";
            MultipleSelfPerpendicular["Ex-Ex"] = "T2";
            MultipleSelfPerpendicular["Ex-Ex w/ Insulation"] = "T2";
            MultipleSelfPerpendicular["Ex w/ Insulation-LB"] = "T2";
            MultipleSelfPerpendicular["Ex w/ Insulation-NLB"] = "T2";
            MultipleSelfPerpendicular["Ex w/ Insulation-Insulation"] = "T2";
            MultipleSelfPerpendicular["Ex w/ Insulation-Fire"] = "T2";
            MultipleSelfPerpendicular["Ex w/ Insulation-Ex"] = "T2";
            MultipleSelfPerpendicular["Ex w/ Insulation-Ex w/ Insulation"] = "T2";

            // Multiple Self and Perpendicular
            SelfParallel["LB-LB"] = "T2";
            SelfParallel["LB-NLB"] = "T2";
            SelfParallel["LBS-Insulation"] = "T6";
            SelfParallel["LBS-Fire"] = "T2";
            SelfParallel["LB-Ex"] = "T6";
            SelfParallel["LB-Ex w/ Insulation"] = "T6";
            SelfParallel["NLB-LB"] = "T2";
            SelfParallel["NLB-NLB"] = "T2";
            SelfParallel["NLB-Insulation"] = "T6";
            SelfParallel["NLB-Fire"] = "T2";
            SelfParallel["NLB-Ex"] = "T6";
            SelfParallel["NLB-Ex w/ Insulation"] = "T6";
            SelfParallel["Insulation-LB"] = "T6";
            SelfParallel["Insulation-NLB"] = "T6";
            SelfParallel["Insulation-Insulation"] = "T2";
            SelfParallel["Insulation-Fire"] = "T2";
            SelfParallel["Insulation-Ex"] = "T6";
            SelfParallel["Insulation-Ex w/ Insulation"] = "T6";
            SelfParallel["Fire-LB"] = "T2";
            SelfParallel["Fire-NLB"] = "T2";
            SelfParallel["Fire-Insulation"] = "T2";
            SelfParallel["Fire-Fire"] = "T2";
            SelfParallel["Fire-Ex"] = "T2";
            SelfParallel["Fire-Ex w/ Insulation"] = "T2";
            SelfParallel["Ex-LB"] = "T6";
            SelfParallel["Ex-NLB"] = "T6";
            SelfParallel["Ex-Insulation"] = "T6";
            SelfParallel["Ex-Fire"] = "T2";
            SelfParallel["Ex-Ex"] = "T2";
            SelfParallel["Ex-Ex w/ Insulation"] = "T6";
            SelfParallel["Ex w/ Insulation-LB"] = "T6";
            SelfParallel["Ex w/ Insulation-NLB"] = "T6";
            SelfParallel["Ex w/ Insulation-Insulation"] = "T6";
            SelfParallel["Ex w/ Insulation-Fire"] = "T2";
            SelfParallel["Ex w/ Insulation-Ex"] = "T6";
            SelfParallel["Ex w/ Insulation-Ex w/ Insulation"] = "T2";

            // Multiple Self and Perpendicular
            SelfParallelHallway["LB-LB"] = "T6";
            SelfParallelHallway["LB-NLB"] = "T2";
            SelfParallelHallway["LBS-Insulation"] = "T6";
            SelfParallelHallway["LBS-Fire"] = "T2";
            SelfParallelHallway["LB-Ex"] = "T6";
            SelfParallelHallway["LB-Ex w/ Insulation"] = "T6";
            SelfParallelHallway["NLB-LB"] = "T2";
            SelfParallelHallway["NLB-NLB"] = "T2";
            SelfParallelHallway["NLB-Insulation"] = "T2";
            SelfParallelHallway["NLB-Fire"] = "T2";
            SelfParallelHallway["NLB-Ex"] = "T2";
            SelfParallelHallway["NLB-Ex w/ Insulation"] = "T2";
            SelfParallelHallway["Insulation-LB"] = "T6";
            SelfParallelHallway["Insulation-NLB"] = "T2";
            SelfParallelHallway["Insulation-Insulation"] = "T6";
            SelfParallelHallway["Insulation-Fire"] = "T2";
            SelfParallelHallway["Insulation-Ex"] = "T6";
            SelfParallelHallway["Insulation-Ex w/ Insulation"] = "T6";
            SelfParallelHallway["Fire-LB"] = "T2";
            SelfParallelHallway["Fire-NLB"] = "T2";
            SelfParallelHallway["Fire-Insulation"] = "T2";
            SelfParallelHallway["Fire-Fire"] = "T2";
            SelfParallelHallway["Fire-Ex"] = "T2";
            SelfParallelHallway["Fire-Ex w/ Insulation"] = "T2";
            SelfParallelHallway["Ex-LB"] = "T6";
            SelfParallelHallway["Ex-NLB"] = "T2";
            SelfParallelHallway["Ex-Insulation"] = "T6";
            SelfParallelHallway["Ex-Fire"] = "T2";
            SelfParallelHallway["Ex-Ex"] = "T2";
            SelfParallelHallway["Ex-Ex w/ Insulation"] = "T2";
            SelfParallelHallway["Ex w/ Insulation-LB"] = "T6";
            SelfParallelHallway["Ex w/ Insulation-NLB"] = "T2";
            SelfParallelHallway["Ex w/ Insulation-Insulation"] = "T6";
            SelfParallelHallway["Ex w/ Insulation-Fire"] = "T2";
            SelfParallelHallway["Ex w/ Insulation-Ex"] = "T2";
            SelfParallelHallway["Ex w/ Insulation-Ex w/ Insulation"] = "T2";
        }
    }
}

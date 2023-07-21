// This file is part of the  R A N O R E X  Project. | http://www.ranorex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop_Automation.Source
{
    public class GlobalSettings
    {
        public static string s_SheetTemplate = "";
        public static string s_ViewDirection = "";
        public static List<KeyValuePair<string, string>> dicViewsToBeCreated = new List<KeyValuePair<string, string>>();

        internal static void Initialize()
        {
            s_SheetTemplate = "";
            s_ViewDirection = "";
            if (dicViewsToBeCreated != null)
                dicViewsToBeCreated.Clear();   
        }
    }
}



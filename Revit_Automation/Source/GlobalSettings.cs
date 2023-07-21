// This file is part of the  R A N O R E X  Project. | http://www.ranorex.com

using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source
{
    public class GlobalSettings
    {
        public static string s_strBuildingType;
        public static double s_dBottomTrackPrefLength;
        public static double s_dTopTrackPrefLength;
        public static double s_dTopTrackMaxLength;
        public static double s_dBottomTrackMaxLength;
        public static int s_PanelDirectionComputation;
        public static List<PanelTypeGlobalParams> lstPanelParams = new List<PanelTypeGlobalParams>();

        public static bool PopulateGlobalSettings()
        {
            bool bSettingsFound = false;
            
            lstPanelParams?.Clear();
            
            string strProjectSettings = InputLineUtility.GetProjectSettings();

            if (!string.IsNullOrEmpty(strProjectSettings)) 
            { 
                bSettingsFound = true;
                string[] settings = strProjectSettings.Split('|');

                // Building type Parameter
                s_strBuildingType = settings[0] == "0" ? "CC" : "NCC";
                
                // Bottom Track preferred Length Parameter
                s_dBottomTrackPrefLength = double.Parse(settings[1]) + (double.Parse(settings[2])/12);

                // Top Track Preferred Length Parameter
                s_dTopTrackPrefLength = double.Parse(settings[3]) + (double.Parse(settings[4])/12);

                // Bottom Track Max Length Paramter
                s_dBottomTrackMaxLength = double.Parse(settings[5]) + (double.Parse(settings[6])/12);

                // Top Track Max Length Parameter
                s_dTopTrackMaxLength = double.Parse(settings[7]) + (double.Parse(settings[8])/12);

                s_PanelDirectionComputation = int.Parse(settings[9]);

                // Row Corresponding to UNO Parameter
                int tempUNORow = int.Parse(settings[10]);

                int j = 11, rowNumber = 0 ; 
                
                // Panel Parameters
                while ( j <  settings.Length - 1 ) 
                {
                    PanelTypeGlobalParams panel = new PanelTypeGlobalParams();
                    panel.bIsUNO = (tempUNORow == rowNumber);
                    panel.strWallName = settings[j];
                    panel.iPanelGuage = double.Parse(settings[j + 1]);
                    panel.iPanelClearance = double.Parse(settings[j + 2]);
                    panel.iPanelMaxLap = double.Parse(settings[j + 3]);
                    panel.iPanelMinLap = double.Parse(settings[j + 4]);
                    panel.strPanelOrientation = settings[j + 5];
                    panel.iPanelPreferredLength = double.Parse(settings[j + 6]);
                    panel.iPanelMaxLength = double.Parse(settings[j + 7]);
                    panel.iPanelHeightOffset = double.Parse(settings[j + 8]);
                    panel.strPanelHorizontalDirection = settings[j + 9];
                    panel.strPanelVerticalDirection = settings[j + 10];
                    panel.iPanelHourRate = double.Parse(settings[j + 11]);

                    lstPanelParams.Add(panel);

                    j += 12;
                    rowNumber++;
                
                }
            }

            return bSettingsFound;
        }
    }
}



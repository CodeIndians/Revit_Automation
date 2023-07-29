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
        public static int s_bPanelAtHallway;
        public static string s_strPartitionStudType;
        public static string s_strHallwayPanelThickness;

        public static bool PopulateGlobalSettings()
        {
            ClearSettings();

            bool bSettingsFound = false;
            
            lstPanelParams?.Clear();
            
            string strProjectSettings = InputLineUtility.GetProjectSettings();

            if (!string.IsNullOrEmpty(strProjectSettings)) 
            { 
                bSettingsFound = true;
                string[] settings = strProjectSettings.Split('|');
                // Building type Parameter
                s_strBuildingType = settings[1] == "0" ? "CC" : "NCC";
                
                // Bottom Track preferred Length Parameter
                s_dBottomTrackPrefLength = double.Parse(settings[2]) + (double.Parse(settings[3])/12);

                // Top Track Preferred Length Parameter
                s_dTopTrackPrefLength = double.Parse(settings[4]) + (double.Parse(settings[5])/12);

                // Bottom Track Max Length Paramter
                s_dBottomTrackMaxLength = double.Parse(settings[6]) + (double.Parse(settings[7])/12);

                // Top Track Max Length Parameter
                s_dTopTrackMaxLength = double.Parse(settings[8]) + (double.Parse(settings[9])/12);

                s_PanelDirectionComputation = int.Parse(settings[10]);

                // Row Corresponding to UNO Parameter
                int tempUNORow = int.Parse(settings[11]);

                // Panel at Hallway 
                s_bPanelAtHallway = int.Parse(settings[12]);

                // Partition stud type
                s_strPartitionStudType = settings[13];

                // Hallway Panel Thickness
                s_strHallwayPanelThickness = settings[14];

                string strPanelSettings = settings[0];
                {
                    int j = 0, rowNumber = 0;
                    string[] panelSettings = strPanelSettings.Split(';');

                    while (j < panelSettings.Length - 1)
                    {
                        PanelTypeGlobalParams panel = new PanelTypeGlobalParams();
                        panel.bIsUNO = (tempUNORow == rowNumber);
                        panel.strWallName = panelSettings[j];
                        panel.iPanelGuage = double.Parse(panelSettings[j + 1]);
                        panel.iPanelClearance = double.Parse(panelSettings[j + 2]);
                        panel.iPanelMaxLap = double.Parse(panelSettings[j + 3]);
                        panel.iPanelMinLap = double.Parse(panelSettings[j + 4]);
                        panel.strPanelOrientation = panelSettings[j + 5];
                        panel.iPanelPreferredLength = double.Parse(panelSettings[j + 6]);
                        panel.iPanelMaxLength = double.Parse(panelSettings[j + 7]);
                        panel.iPanelHeightOffset = double.Parse(panelSettings[j + 8]);
                        panel.strPanelHorizontalDirection = panelSettings[j + 9];
                        panel.strPanelVerticalDirection = panelSettings[j + 10];
                        panel.iPanelHourRate = double.Parse(panelSettings[j + 11]);

                        lstPanelParams.Add(panel);

                        j += 12;
                        rowNumber++;
                    }
                }
            }

            return bSettingsFound;
        }

        internal static void UpdateSettings()
        {
           
            PopulateGlobalSettings();
        }

        private static void ClearSettings()
        {
            s_strBuildingType = "";
            s_dBottomTrackPrefLength = 0.0;
            s_dTopTrackPrefLength = 0.0;
            s_dTopTrackMaxLength = 0.0;
            s_dBottomTrackMaxLength = 0.0;
            s_PanelDirectionComputation = 0;
            lstPanelParams.Clear();
            s_bPanelAtHallway = 0;
            s_strPartitionStudType = "";
            s_strHallwayPanelThickness = "";

        }
    }
}



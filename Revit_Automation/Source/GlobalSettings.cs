﻿

using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Runtime;
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
        public static List<CeeHeaderSettings> lstCeeHeaderSettings = new List<CeeHeaderSettings>();
        public static int s_bPanelAtHallway;
        public static string s_strPartitionStudType;
        public static string s_strHallwayPanelThickness;
        public static string s_iPanelStrategy;
        public static string s_strDeckSpan;
        public static string s_strDragStructMaxLength;
        public static string s_strDragStructType;
        public static string s_strDragSturctContinuous;
        public static string s_EaveStructMaxLength;
        public static string s_EavStructType;
        public static string s_EaveStructLocation;
        public static int s_bSpliceNLBRoof ;
        public static int s_bTTRoundOff;
        public static int s_bTTAtRake;
        public static int s_bTTSpliceAtWeb;
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

                // Panel at Hallway 
                s_bPanelAtHallway = int.Parse(settings[12]);

                // Partition stud type
                s_strPartitionStudType = settings[13];

                // Hallway Panel Thickness
                s_strHallwayPanelThickness = settings[14];

                // Panel Placement Strategy
                s_iPanelStrategy = settings[15];

                // Deck span
                s_strDeckSpan = settings[16];

                // Drag Struct Max Length
                s_strDragStructMaxLength = settings[17];

                // Drag StructType
                s_strDragStructType = settings[18];

                // Drag Struct Continuous at hallway
                s_strDragSturctContinuous = settings[19];

                // Eave stuct Max length
                s_EaveStructMaxLength = settings[20];
                
                // Eave Struct type
                s_EavStructType = settings[21];
                
                // Eave Struct Location
                s_EaveStructLocation = settings[22];

                string strPanelSettings = settings[0];
                {
                    int j = 0;
                    string[] panelSettings = strPanelSettings.Split(';');

                    while (j < panelSettings.Length - 1)
                    {
                        PanelTypeGlobalParams panel = new PanelTypeGlobalParams();
                        panel.bIsUNO = bool.Parse(panelSettings[j++]);
                        panel.strWallName = panelSettings[j];
                        panel.iPanelGuage = string.IsNullOrEmpty(panelSettings[j + 1]) ?  0.0 : double.Parse(panelSettings[j + 1]);
                        panel.iPanelClearance = string.IsNullOrEmpty(panelSettings[j + 2]) ? 0.0 : double.Parse(panelSettings[j + 2]);
                        panel.iPanelMaxLap = string.IsNullOrEmpty(panelSettings[j + 3]) ? 0.0 : double.Parse(panelSettings[j + 3]);
                        panel.iPanelMinLap = string.IsNullOrEmpty(panelSettings[j + 4]) ? 0.0 : double.Parse(panelSettings[j + 4]);
                        panel.strPanelOrientation = panelSettings[j + 5];
                        panel.iPanelPreferredLength = string.IsNullOrEmpty(panelSettings[j + 6]) ? 0.0 : double.Parse(panelSettings[j + 6]);
                        panel.iPanelMaxLength = string.IsNullOrEmpty(panelSettings[j + 7]) ? 0.0 : double.Parse(panelSettings[j + 7]);
                        panel.iPanelHeightOffset = string.IsNullOrEmpty(panelSettings[j + 8]) ? 0.0 : double.Parse(panelSettings[j + 8]);
                        panel.strPanelHorizontalDirection = panelSettings[j + 9];
                        panel.strPanelVerticalDirection = panelSettings[j + 10];
                        panel.iPanelHourRate = string.IsNullOrEmpty(panelSettings[j + 11]) ? 0.0 : double.Parse(panelSettings[j + 11]); ;

                        lstPanelParams.Add(panel);

                        j += 12;
                    }
                }

                string strCeeHeaderSettings = settings[23];
                {
                    int j = 0;
                    string[] ceeHeaderSettings = strCeeHeaderSettings.Split(';');

                    while (j < ceeHeaderSettings.Length - 1)
                    {
                        CeeHeaderSettings ceeHeader = new CeeHeaderSettings();
                        ceeHeader.bIsValidGrid = bool.Parse(ceeHeaderSettings[j++]);
                        ceeHeader.strGridName = ceeHeaderSettings[j++];
                        ceeHeader.ceeHeaderName = ceeHeaderSettings[j++];
                        ceeHeader.ceeHeaderCount = ceeHeaderSettings[j++];
                        ceeHeader.HallwayCeeHeaderName = ceeHeaderSettings[j++];
                        ceeHeader.HallwayCeeHeaderCount = ceeHeaderSettings[j++];
                        lstCeeHeaderSettings.Add(ceeHeader);
                    }
                }

                s_bSpliceNLBRoof = int.Parse(settings[24]);
                s_bTTRoundOff = int.Parse(settings[25]);
                s_bTTAtRake = int.Parse(settings[26]);
                s_bTTSpliceAtWeb = int.Parse(settings[27]);

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
            lstCeeHeaderSettings.Clear();
            s_bPanelAtHallway = 0;
            s_strPartitionStudType = "";
            s_strHallwayPanelThickness = "";
        }
    }
}



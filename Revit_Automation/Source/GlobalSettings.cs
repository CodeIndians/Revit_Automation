

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
        public static FramingSettings framingSettings;
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
                s_strPartitionStudType = settings[13].ToString();

                // Hallway Panel Thickness
                s_strHallwayPanelThickness = settings[14].ToString();

                // Panel Placement Strategy
                s_iPanelStrategy = settings[15].ToString();

                // Deck span
                framingSettings.dCeeHeaderDeckSpan = string.IsNullOrEmpty(settings[16]) ? 0 : double.Parse(settings[16]) ;

                // Drag Struct Max Length
                framingSettings.dDragStuctMaxLength  = string.IsNullOrEmpty(settings[17]) ? 0 : double.Parse(settings[17]);

                // Drag StructType
                framingSettings.strDragStructType = settings[18].ToString();

                // Drag Struct Continuous at hallway
                framingSettings.bDragStructContinuousAtHallway = settings[19] == "0" ? true : false;

                // Eave stuct Max length
                framingSettings.dEaveStructMaxLength  = string.IsNullOrEmpty(settings[20]) ? 0 : double.Parse(settings[20]);

                // Eave Struct type
                framingSettings.strEaveStructType = settings[21].ToString();
                
                // Eave Struct Location
                framingSettings.strEaveStructLocation = settings[22].ToString();

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

                // Top track related settings
                framingSettings.bNLBSpliceAtRoof = settings[24] == "0" ? true : false;
                framingSettings.bToptrackRounfOff = settings[25] == "0" ? true : false;
                framingSettings.bTopTrackAtRakeSide = settings[26] == "0" ? true : false;
                framingSettings.bTopTrackSpliceAtWeb = settings[27] == "0" ? true : false;

                // Cee Header max length
                framingSettings.dCeeHeaderMaxLength = string.IsNullOrEmpty(settings[28]) ? 0 : double.Parse(settings[28]);
                
                // Floor or composite deck settings
                framingSettings.strFloorDeckType = settings[29].ToString();
                framingSettings.dFloorDeckOverlap = string.IsNullOrEmpty(settings[30]) ? 0 : double.Parse(settings[30]);
                framingSettings.dFloorDeckMaxSpan = string.IsNullOrEmpty(settings[31]) ? 0 : double.Parse(settings[31]);
                framingSettings.dFloorDeckMaxLength = string.IsNullOrEmpty(settings[32]) ? 0 : double.Parse(settings[32]);

                // Roof deck settings
                framingSettings.strRoofDeckType = settings[33].ToString();
                framingSettings.dRoofDeckOverlap = string.IsNullOrEmpty(settings[34]) ? 0 : double.Parse(settings[34]);
                framingSettings.dRoofDeckMaxSpan = string.IsNullOrEmpty(settings[35]) ? 0 : double.Parse(settings[35]);
                framingSettings.dRoofDeckMaxLength = string.IsNullOrEmpty(settings[36]) ? 0 : double.Parse(settings[36]);

                // Purlin Settings
                framingSettings.dPurlinLap = string.IsNullOrEmpty(settings[37]) ? 0 : double.Parse(settings[37]);
                framingSettings.dPurlinPreferredLength = string.IsNullOrEmpty(settings[38]) ? 0 : double.Parse(settings[38]);
                framingSettings.dPurlinMaxSpans = string.IsNullOrEmpty(settings[39]) ? 0 : double.Parse(settings[39]);
                framingSettings.bPurlinContAtHallway = settings[40] == "0" ? true : false;
                framingSettings.bPurlOrientationChange = settings[41] == "0" ? true: false;
                framingSettings.strRecieverChannelType = settings[42].ToString();
                framingSettings.strRecieverChannelGauge = settings[43].ToString();
                framingSettings.bPurlinOverhang = settings[44] == "0" ? true : false;
                framingSettings.bPurlinRoundOff = settings[45] == "0" ? true : false;

                string pulinSettings = settings[46];
                {
                    int j = 0;
                    string[] lstpurlinSettings = pulinSettings.Split(';');

                    while (j < lstpurlinSettings.Length - 1)
                    {
                        PurlinTypeSettings purlinTypeSettings = new PurlinTypeSettings();
                        
                        purlinTypeSettings.dOnCenter = string.IsNullOrEmpty(lstpurlinSettings[j]) ? 0 : double.Parse(lstpurlinSettings[j]);
                        j++;
                        purlinTypeSettings.strPurlinType = lstpurlinSettings[j++];
                        purlinTypeSettings.strPurlinGauge = lstpurlinSettings[j++];
                    }
                }

                framingSettings.dDragStrutLap = string.IsNullOrEmpty(settings[47]) ? 0 : double.Parse(settings[47]);
                framingSettings.dEaveStrutLap = string.IsNullOrEmpty(settings[48]) ? 0 : double.Parse(settings[48]);

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



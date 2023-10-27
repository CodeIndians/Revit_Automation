

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

                string strCeeHeaderSettings = settings[17];
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
                framingSettings.bNLBSpliceAtRoof = settings[18] == "0" ? true : false;
                framingSettings.bToptrackRounfOff = settings[19] == "0" ? true : false;
                framingSettings.bTopTrackAtRakeSide = settings[20] == "0" ? true : false;
                framingSettings.bTopTrackSpliceAtWeb = settings[21] == "0" ? true : false;

                // Cee Header max length
                framingSettings.dCeeHeaderMaxLength = string.IsNullOrEmpty(settings[22]) ? 0 : double.Parse(settings[22]);
                
                // Floor or composite deck settings
                framingSettings.strFloorDeckType = settings[23].ToString();
                framingSettings.dFloorDeckOverlap = string.IsNullOrEmpty(settings[24]) ? 0 : double.Parse(settings[24]);
                framingSettings.dFloorDeckMaxSpan = string.IsNullOrEmpty(settings[25]) ? 0 : double.Parse(settings[25]);
                framingSettings.dFloorDeckMaxLength = string.IsNullOrEmpty(settings[26]) ? 0 : double.Parse(settings[26]);

                // Roof deck settings
                framingSettings.strRoofDeckType = settings[27].ToString();
                framingSettings.dRoofDeckOverlap = string.IsNullOrEmpty(settings[28]) ? 0 : double.Parse(settings[28]);
                framingSettings.dRoofDeckMaxSpan = string.IsNullOrEmpty(settings[29]) ? 0 : double.Parse(settings[29]);
                framingSettings.dRoofDeckMaxLength = string.IsNullOrEmpty(settings[30]) ? 0 : double.Parse(settings[30]);

                // Purlin General Settings
                framingSettings.dPurlinLap = string.IsNullOrEmpty(settings[31]) ? 0 : double.Parse(settings[31]);
                framingSettings.dPurlinPreferredLength = string.IsNullOrEmpty(settings[32]) ? 0 : double.Parse(settings[32]);
                framingSettings.bPurlinContinuousAtInsulation = settings[33] == "0" ? true : false;
                framingSettings.bPurlinRoundOff = settings[34] == "0" ? true : false;

                // Purlin Roof Specific Settings
                string pulinSettings = settings[35];
                {
                    int j = 0;
                    string[] lstpurlinSettings = pulinSettings.Split(';');
                    List<PurlinTypeSettings> purlinSettingList = new List<PurlinTypeSettings>();
                    while (j < lstpurlinSettings.Length - 1)
                    {
                        PurlinTypeSettings purlinTypeSettings = new PurlinTypeSettings();
                        
                        purlinTypeSettings.strRoofName = lstpurlinSettings[j++];
                        purlinTypeSettings.strPurlinType = lstpurlinSettings[j++] ;
                        purlinTypeSettings.strPurlinGauge = lstpurlinSettings[j++];
                        purlinTypeSettings.dOnCenter = string.IsNullOrEmpty(lstpurlinSettings[j]) ? 0 : double.Parse(lstpurlinSettings[j]);
                        j++;
                        purlinTypeSettings.dMaxSpan = string.IsNullOrEmpty(lstpurlinSettings[j]) ? 0 : double.Parse(lstpurlinSettings[j]);
                        j++;
                        purlinTypeSettings.dExtWallThickness = string.IsNullOrEmpty(lstpurlinSettings[j]) ? 0 : double.Parse(lstpurlinSettings[j]);
                        j++;
                        purlinTypeSettings.strReceiverChannelType = lstpurlinSettings[j++];
                        purlinTypeSettings.strReceiverChannelGauge = lstpurlinSettings[j++];
                        purlinTypeSettings.bOrientationChange = lstpurlinSettings[j] == "0" ? true:false;
                        j++;
                        purlinSettingList.Add(purlinTypeSettings);
                    }
                    framingSettings.purlinSettings = purlinSettingList;
                }

                // Drag Strut Settings
                string dragStrutSettings = settings[36];
                {
                    int k = 0;
                    string[] lstDragStrutSettings = dragStrutSettings.Split(';');
                    List<DragStrutSettings> dragStrutSettingsList = new List<DragStrutSettings>();
                    while (k < lstDragStrutSettings.Length - 1)
                    {
                        if (lstDragStrutSettings[k] == "")
                            break;
                        DragStrutSettings dragStrutSet = new DragStrutSettings();
                        dragStrutSet.strRoofName = lstDragStrutSettings[k++];
                        dragStrutSet.strStrutType = lstDragStrutSettings[k++];
                        dragStrutSet.bContinuousAtHallway = lstDragStrutSettings[k] == "0" ? true : false;
                        k++;
                        dragStrutSet.dMaxLength = string.IsNullOrEmpty(lstDragStrutSettings[k]) ? 0 : double.Parse(lstDragStrutSettings[k]);
                        k++;
                        dragStrutSet.dLap = string.IsNullOrEmpty(lstDragStrutSettings[k]) ? 0 : double.Parse(lstDragStrutSettings[k]);
                        k++;
                    }
                    framingSettings.lstDragStrutSettings = dragStrutSettingsList;
                }

                // Eave Strut Settings
                string eaveStrutSettings = settings[37];
                {
                    int l = 0;
                    string[] lstEaveStrutSettings = eaveStrutSettings.Split(';');
                    List<EaveStrutSettings> eaveStrutSettingsList = new List<EaveStrutSettings>();
                    while (l < eaveStrutSettings.Length - 1)
                    {
                        if (lstEaveStrutSettings[l] == "")
                            break;
                        EaveStrutSettings eaveStrutSet = new EaveStrutSettings();
                        eaveStrutSet.strRoofName = lstEaveStrutSettings[l++];
                        eaveStrutSet.strStrutType = lstEaveStrutSettings[l++];
                        eaveStrutSet.strLocationOfStrut = lstEaveStrutSettings[l++];
                        eaveStrutSet.dMaxLength = string.IsNullOrEmpty(lstEaveStrutSettings[l]) ? 0 : double.Parse(lstEaveStrutSettings[l]);
                        l++;
                        eaveStrutSet.dLap = string.IsNullOrEmpty(lstEaveStrutSettings[l]) ? 0 : double.Parse(lstEaveStrutSettings[l]);
                        l++;

                    }
                    framingSettings.lstEaveStrutSettings = eaveStrutSettingsList;
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
            lstCeeHeaderSettings.Clear();
            s_bPanelAtHallway = 0;
            s_strPartitionStudType = "";
            s_strHallwayPanelThickness = "";
        }
    }
}



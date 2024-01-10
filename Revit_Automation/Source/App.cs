
/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

#region Namespaces
using Autodesk.Revit.UI;
using Revit_Automation.Dialogs;
using Revit_Automation.Source.Licensing;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Sheeting_Automation;
using System.Collections.Concurrent;

#endregion

namespace Revit_Automation
{
    internal class App : IExternalApplication
    {
        /// <summary>
        /// This method is called when Add-in is loaded into REVIT. It contains information related to all the commands
        /// Any new command that needs to be added has to follow the below scheme
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Result OnStartup(UIControlledApplication a)
        {
            if (LicenseValidator.ValidateLicense())
            {

                // Create a custom ribbon tab
                string tabName = "Modelling Automation";
                a.CreateRibbonTab(tabName);


                string tabName2 = "Dev Commands";
                a.CreateRibbonTab(tabName2);

                RibbonPanel settingsRB1 = a.CreateRibbonPanel(tabName2, "Debug Commands");

                AddRevitCommand(settingsRB1,
                "ProjectSettingsCMD",
                " Get Range",
                "Revit_Automation.GetRangeCommand",
                "Project Settings",
                "Debug.png");

                // Create Ribbon Panels
                RibbonPanel settingsRB = a.CreateRibbonPanel(tabName, "Settings");
                RibbonPanel PreProcessingRB = a.CreateRibbonPanel(tabName, "Pre Processing");
                RibbonPanel PostsRB = a.CreateRibbonPanel(tabName, "Structural Columns");
                RibbonPanel PartitionRB = a.CreateRibbonPanel(tabName, "Partitions");
                RibbonPanel DeckingRB = a.CreateRibbonPanel(tabName, "Decking");
                RibbonPanel FramingRB = a.CreateRibbonPanel(tabName, "Framing");


                //RibbonPanel HallWayRB = a.CreateRibbonPanel(tabName, "HallWays");

                AddRevitCommand(settingsRB,
                    "ProjectSettingsCMD",
                    "Project \n Settings",
                    "Revit_Automation.ProjectSettings",
                    "Project Settings",
                    "ProjectSettings.png");

                #region GENERIC_MODELLING

                // Compute Exterior panel direction
                AddRevitCommand(PreProcessingRB,
                "ExPanelDirComp",
                "Ex-Panel Dir",
                "Revit_Automation.ExteriorPanelDirectionComputation",
                "Place Panels at All Input Lines",
                "ProcessLines.png");

                // Extend Lines 
                AddRevitCommand(PreProcessingRB,
                    "ExtendLinesCMD",
                    "    Extend    ",
                    "Revit_Automation.ExtendLines",
                    "Extend and Trim All Input Lines",
                    "ProcessLines.png");

                // Trim Lines
                AddRevitCommand(PreProcessingRB,
                    "TrimLinesCMD",
                    "     Trim     ",
                    "Revit_Automation.TrimLines",
                    "Extend and Trim All Input Lines",
                    "ProcessLines.png");

                // Posts - ALL
                AddRevitCommand(PostsRB,
                    "PostsAtAlldLinesCMD",
                    "    Posts     ",
                    "Revit_Automation.PostsAtAllLines",
                    "Place posts at all Lines",
                    "Posts.png");

                // Bottom Tracks - ALL
                AddRevitCommand(PostsRB,
                      "BTAtAllLinesCMD",
                      "Bottom Tracks",
                      "Revit_Automation.BTAtAllLines",
                      "Place Bottom Tracks at All Input Lines",
                      "BottomTrack.png");

                // Panels - ALL
                AddRevitCommand(PartitionRB,
                    "PanelsAtAllCMD",
                    "    Panels    ",
                    "Revit_Automation.PanelsAtAllLines",
                    "Place Panels at All Input Lines",
                    "Walls.png");

                // Composite Deck
                AddRevitCommand(DeckingRB,
                    "CompositeDeckCMD",
                    "  Composite  \n    Deck    ",
                    "Revit_Automation.CompositeDeckCreator",
                    "Place Composite Deck",
                    "Deck.png");

                // Roof Deck
                AddRevitCommand(DeckingRB,
                    "RoofDeckCMD",
                    "   Roof Deck  ",
                    "Revit_Automation.RoofDeckCreator",
                    "Place Roof Deck",
                    "Deck.png");

                // Trim Composite / Roof Deck
                AddRevitCommand(DeckingRB,
                    "TrimDeckCMD",
                    "   Trim Deck  ",
                    "Revit_Automation.TrimDecks",
                    "Trim Roof Deck",
                    "Deck.png");

                // Cee Headers
                AddRevitCommand(FramingRB,
                    "CeeHeadersCMD",
                    "   C-Headers  ",
                    "Revit_Automation.CeeHeaders",
                    "Place C-Header at All Input Lines",
                    "Header.png");

                // Top Tracks
                AddRevitCommand(FramingRB,
                    "TopTracksCMD",
                    "Top Tracks",
                    "Revit_Automation.TopTracksCreator",
                    "Place Top Tracks",
                    "BottomTrack.png");

                // Purlins
                AddRevitCommand(FramingRB,
                    "PurlinsCMD",
                    "   Purlins    ",
                    "Revit_Automation.PurlinsCreator",
                    "Place Purlins",
                    "Purlin.png");

                // Drag Strut
                AddRevitCommand(FramingRB,
                    "DragStrutCMD",
                    "  Drag Strut  ",
                    "Revit_Automation.DragStrutCreator",
                    "Place DragStruts",
                    "Strut.png");

                // Eave Strut
                AddRevitCommand(FramingRB,
                    "EaveStrutCMD",
                    "  Eave Strut  ",
                    "Revit_Automation.EaveStrutCreator",
                    "Place EaveStruts",
                    "Strut.png");

                // 
                //AddRevitCommand(HallWayRB,
                //   "HallWayCreateHatch",
                //   "Create Hatch \n for Hallway",
                //   "Revit_Automation.CreateHatchForHallway",
                //   "Place Hatch at Rooms",
                //   "Hallway2.png");

                //AddRevitCommand(HallWayRB,
                //   "HallWayEditHatch",
                //   "Edit Hatch \n Selected",
                //   "Revit_Automation.EditHatchForHallway",
                //   "DeleEditte Hatch at Rooms",
                //   "Hallway2.png");

                //AddRevitCommand(HallWayRB,
                //   "HallwayLines",
                //   "Draw HallWay \n Lines",
                //   "Revit_Automation.DrawHallWayLines",
                //   "Draw HallWay",
                //   "Hallway.png");
                #endregion

                return Result.Succeeded;
            }
            else
            {
                MessageBox.Show("Revit Plugin license verification failed", "License Error");
                return Result.Failed;
            }
        }

        private void AddRevitCommand(RibbonPanel rb,
                                     string commandShortID,
                                     string commandDisplayName,
                                     string commandProgID,
                                     string tooltipMessage,
                                     string commandIconPath)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData btnData = new PushButtonData(
                    commandShortID,
                    commandDisplayName,
                    thisAssemblyPath,
                    commandProgID);

            PushButton pbtn = rb.AddItem(btnData) as PushButton;
            pbtn.ToolTip = tooltipMessage;
            string iconDirectory = "C:\\Program Files\\Autodesk\\Revit 2022\\AddIns\\Resources\\";
            string iconPath = iconDirectory + commandIconPath;

            if (File.Exists(iconPath))
            {
                BitmapImage btnImage = new BitmapImage(new Uri(iconPath));
                pbtn.LargeImage = btnImage;
            }

        }
            
        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}

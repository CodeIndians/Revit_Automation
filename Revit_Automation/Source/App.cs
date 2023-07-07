
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

#endregion

namespace Revit_Automation
{
    internal class App : IExternalApplication
    {
        /// <summary>
        /// This method is called when Add-in is loaded into REVIT. It contains information related to all the commands
        /// Any new command that needs to be added has to follow the below scheme
        /// 1 - Project Settings
        /// 2 - 20 - Commands based on Selection
        /// 21-40 - Commands For running on entire model
        /// 2 - Preprocess Lines at Selection
        /// 3 - Posts at selection
        /// 4 - Bottom Tracks at Selection
        /// 5 - Panels at Selection
        /// 6 - 20 - Reserved for Future
        /// 21 - Preprocess Lines
        /// 22 - Create Posts
        /// 23 - Create Bottom Tracks
        /// 24 - Create Panels
        /// 25 - 40  - Reserved for future
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Result OnStartup(UIControlledApplication a)
        {
            //if (LicenseValidator.ValidateLicense())
            {

                // Create a custom ribbon tab
                string tabName = "Modelling Automation";
                a.CreateRibbonTab(tabName);

                // Create Ribbon Panels
                RibbonPanel settingsRB = a.CreateRibbonPanel(tabName, "Settings");
                RibbonPanel SelectedModellingRB = a.CreateRibbonPanel(tabName, "Selected Modelling");
                RibbonPanel FullModellingRB = a.CreateRibbonPanel(tabName, "General Modelling");

                AddRevitCommand(settingsRB,
                    "ProjectSettingsCMD",
                    "Project \n Settings",
                    "Revit_Automation.ProjectSettings",
                    "Project Settings",
                    "ProjectSettings.png");

                #region SELCTED_MODELLING

                // Pre Process Selected Lines
                AddRevitCommand(SelectedModellingRB,
                    "ProcessSelectedLinesCMD",
                    "PreProcess \n Selected Lines",
                    "Revit_Automation.PreProcessSelectedLines",
                    "Extend and Trim Selected Input Lines",
                    "ProcessLines2.png");

                // posts at selection
                AddRevitCommand(SelectedModellingRB,
                    "PostsAtSelectedLinesCMD",
                    "Posts At \n Selected Lines",
                    "Revit_Automation.PostsAtSelectedLines",
                    "Place posts at Selected Input Lines",
                    "Posts.png");

                // Bottom tracks at selection
                AddRevitCommand(SelectedModellingRB,
                      "BTAtSelectedLinesCMD",
                      "Bottom Tracks At \n Selected Lines",
                      "Revit_Automation.BTAtSelectedLines",
                      "Place Bottom Tracks at Selected Input Lines",
                      "BottomTrack.png");

                // Panels at selection
                AddRevitCommand(SelectedModellingRB,
                    "PanelsAtSelectedLinesCMD",
                    "Panels At \n Selected Lines",
                    "Revit_Automation.PanelsAtSelectedLines",
                    "Place Panels at Selected Input Lines",
                    "Walls.png");

                #endregion

                #region GENERIC_MODELLING

                // Preprocess All lines 
                AddRevitCommand(FullModellingRB,
                    "ProcessAllLinesCMD",
                    "PreProcess \n All Lines",
                    "Revit_Automation.PreProcessAllLines",
                    "Extend and Trim All Input Lines",
                    "ProcessLines.png");

                // Posts - ALL
                AddRevitCommand(FullModellingRB,
                    "PostsAtAlldLinesCMD",
                    "Posts At \n All Lines",
                    "Revit_Automation.PostsAtAllLines",
                    "Place posts at all Lines",
                    "Model.png");

                // Bottom Tracks - ALL
                AddRevitCommand(FullModellingRB,
                      "BTAtAllLinesCMD",
                      "Bottom Tracks At \n All Lines",
                      "Revit_Automation.BTAtAllLines",
                      "Place Bottom Tracks at All Input Lines",
                      "BottomTrack.png");

                // Panels - ALL
                AddRevitCommand(FullModellingRB,
                    "PanelsAtAllCMD",
                    "Panels At \n All Lines",
                    "Revit_Automation.PanelsAtAllLines",
                    "Place Panels at All Input Lines",
                    "Walls.png");
                #endregion

                return Result.Succeeded;
            }
            //else
            //{
            //    MessageBox.Show("Revit Plugin license verification failed", "License Error");
            //    return Result.Failed;
            //}
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

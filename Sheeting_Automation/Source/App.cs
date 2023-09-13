

/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

#region Namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

#endregion

namespace Sheeting_Automation
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            if (Source.Licensing.LicenseValidator.ValidateLicense())
            {
                // Create a custom ribbon tab
                string tabName = "Sheeting Automation";
                a.CreateRibbonTab(tabName);

                //Create ribbon panels
                RibbonPanel dimensionsRB = a.CreateRibbonPanel(tabName, "Dimensions");
                RibbonPanel schedulesRB = a.CreateRibbonPanel(tabName, "Schedules");
                RibbonPanel tagsRB = a.CreateRibbonPanel(tabName, "Tags");

                AddRevitCommand(dimensionsRB,
                    "PlaceDimensionsCMD",
                    "Place Dimensions",
                    "Sheeting_Automation.PlaceDimensionsCommand",
                    "Place Dimensions",
                    "Sheets.png");

                AddRevitCommand(schedulesRB,
                    "CreateSchedulesCMD",
                    "Create Schedules",
                    "Sheeting_Automation.CreateSchedulesCommand",
                    "Create Schedules",
                    "Sheets.png");

                AddRevitCommand(schedulesRB,
                    "UpdateSchedulesCMD",
                    "Update Schedule",
                    "Sheeting_Automation.UpdateScheduleCommand",
                    "Update Schedule",
                    "Sheets.png");
                
                AddRevitCommand(tagsRB,
                    "Create Tags",
                    "Create Tags",
                    "Sheeting_Automation.Source.Tags.CreateTagsCommand",
                    "Create Tags",
                    "Sheets.png"); //TODO: use tags.png

                AddRevitCommand(tagsRB,
                    "Check Missing Tags",
                    "Check Missing \n Tags",
                    "Sheeting_Automation.Source.Tags.CheckTagsCountCommand",
                    "Check Missing Tags",
                    "Sheets.png"); //TODO: use tags.png

                AddRevitCommand(tagsRB,
                   "Check Tags Overlap",
                   "Check Tags \n Overlap",
                   "Sheeting_Automation.Source.Tags.CheckTagsOverlapCommand",
                   "Check Tags Overlap",
                   "Sheets.png"); //TODO: use tags.png

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



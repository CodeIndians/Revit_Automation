// This file is part of the  R A N O R E X  Project. | http://www.ranorex.com

/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

#region Namespaces
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
            //if (LicenseValidator.ValidateLicense())
            {
                // Create a custom ribbon tab
                string tabName = "Sheeting Automation";
                a.CreateRibbonTab(tabName);

                // Get dll assembly path
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
                RibbonPanel ribbonPanel = a.CreateRibbonPanel(tabName, "Sheeting");

                PushButtonData b1Data = new PushButtonData(
                    "testCMD1",
                    "Create Sheets",
                    thisAssemblyPath,
                    "Sheeting_Automation.Command");

                PushButton pb1 = ribbonPanel.AddItem(b1Data) as PushButton;
                pb1.ToolTip = "Create Sheets";
                string path1 = "C:\\Program Files\\Autodesk\\Revit 2022\\AddIns\\Resources\\Sheets.png";
                if (File.Exists(path1))
                {
                    BitmapImage pb1Image = new BitmapImage(new Uri(path1));
                    pb1.LargeImage = pb1Image;
                }

                return Result.Succeeded;
            }
            //else
            //{
            //    MessageBox.Show("Revit Plugin license verification failed", "License Error");
            //    return Result.Failed;
            //}
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}



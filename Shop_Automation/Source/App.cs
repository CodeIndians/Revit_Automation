

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

namespace Shop_Automation
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            //if (LicenseValidator.ValidateLicense())
            {
                // Create a custom ribbon tab
                string tabName = "Shop_Automation";
                a.CreateRibbonTab(tabName);

                // Get dll assembly path
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
                RibbonPanel ribbonPanel = a.CreateRibbonPanel(tabName, "Assembly Generation");

                // Assembly Settings
                PushButtonData b1Data = new PushButtonData(
                    "AssemblySettingsCMD",
                    "Assembly View \n Settings",
                    thisAssemblyPath,
                    "Shop_Automation.AssemblySettings");

                PushButton pb1 = ribbonPanel.AddItem(b1Data) as PushButton;
                pb1.ToolTip = "Assembly View Settings";
                string path1 = "C:\\Program Files\\Autodesk\\Revit 2022\\AddIns\\Resources\\AssemblySettings.png";
                if (File.Exists(path1))
                {
                    BitmapImage pb1Image = new BitmapImage(new Uri(path1));
                    pb1.LargeImage = pb1Image;
                }

                // Assembly creation
                PushButtonData b2Data = new PushButtonData(
                    "GenerateAssembliesCMD",
                    "Generate \n Assemblies",
                    thisAssemblyPath,
                    "Shop_Automation.GenerateAssemblies");

                PushButton pb2 = ribbonPanel.AddItem(b2Data) as PushButton;
                pb2.ToolTip = "Assembly View Settings";
                string path2 = "C:\\Program Files\\Autodesk\\Revit 2022\\AddIns\\Resources\\CreateAssemblies.png";
                if (File.Exists(path2))
                {
                    BitmapImage pb2Image = new BitmapImage(new Uri(path2));
                    pb2.LargeImage = pb2Image;
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



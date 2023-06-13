
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
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

#endregion

namespace Revit_Automation
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            _ = new Form2
            {
                StartPosition = FormStartPosition.CenterScreen,
                TopMost = true
            };
            //form.ShowDialog();

            //if (form.ValidLicense)
            {
                // Create a custom ribbon tab
                string tabName = "Automation Toolkit";
                a.CreateRibbonTab(tabName);

                // Add a new ribbon panel
                RibbonPanel ribbonPanel = a.CreateRibbonPanel(tabName, "Creation");

                // Add a new ribbon panel
                //RibbonPanel ribbonPanel2 = a.CreateRibbonPanel(tabName, "Validation");

                // Get dll assembly path
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

                // create push button for CurveTotalLength
                PushButtonData b1Data = new PushButtonData(
                    "testCMD",
                    "Generate \n Model",
                    thisAssemblyPath,
                    "Revit_Automation.Command");

                PushButton pb1 = ribbonPanel.AddItem(b1Data) as PushButton;
                pb1.ToolTip = "Place posts as per the existing grids";
                string path = "C:\\Program Files\\Autodesk\\Revit 2022\\AddIns\\Resources\\Model.png";

                if (File.Exists(path))
                {
                    BitmapImage pb1Image = new BitmapImage(new Uri(path));
                    pb1.LargeImage = pb1Image;
                }
                // Create source.


                PushButtonData b2Data = new PushButtonData(
                    "testCMD2",
                    "Posts At \n Selection",
                    thisAssemblyPath,
                    "Revit_Automation.Command2");

                PushButton pb2 = ribbonPanel.AddItem(b2Data) as PushButton;
                pb2.ToolTip = "Place posts At a selected Input line";
                string path2 = "C:\\Program Files\\Autodesk\\Revit 2022\\AddIns\\Resources\\Posts.png";
                if (File.Exists(path2))
                {
                    BitmapImage pb2Image = new BitmapImage(new Uri(path2));
                    pb2.LargeImage = pb2Image;
                }

                PushButtonData b3Data = new PushButtonData(
                    "testCMD3",
                    "Walls at \n Selection",
                    thisAssemblyPath,
                    "Revit_Automation.Command3");

                PushButton pb3 = ribbonPanel.AddItem(b3Data) as PushButton;
                pb3.ToolTip = "Place Walls At a selected Input line";
                string path3 = "C:\\Program Files\\Autodesk\\Revit 2022\\AddIns\\Resources\\Walls.png";
                if (File.Exists(path3))
                {
                    BitmapImage pb3Image = new BitmapImage(new Uri(path3));
                    pb3.LargeImage = pb3Image;
                }

                PushButtonData b4Data = new PushButtonData(
                    "testCMD4",
                    "Botom Tracks \n at Selection",
                    thisAssemblyPath,
                    "Revit_Automation.Command4");

                PushButton pb4 = ribbonPanel.AddItem(b4Data) as PushButton;
                pb4.ToolTip = "Place Bottom Tracks At a selected Input line";
                string path4 = "C:\\Program Files\\Autodesk\\Revit 2022\\AddIns\\Resources\\BottomTrack.png";
                if (File.Exists(path4))
                {
                    BitmapImage pb4Image = new BitmapImage(new Uri(path4));
                    pb4.LargeImage = pb4Image;
                }
                return Result.Succeeded;
            }
            //else
            //{
            //    return Result.Failed;
            //}
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}

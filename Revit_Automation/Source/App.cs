
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
        public Result OnStartup(UIControlledApplication a)
        {
            //if (LicenseValidator.ValidateLicense())
            {
                // Create a custom ribbon tab
                string tabName = "Modelling Automation";
                a.CreateRibbonTab(tabName);

                // Get dll assembly path
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

                RibbonPanel ribbonPanel2 = a.CreateRibbonPanel(tabName, "Settings");

                PushButtonData b5Data = new PushButtonData(
                    "testCMD5",
                    "Project \n Settings",
                    thisAssemblyPath,
                    "Revit_Automation.Command5");

                PushButton pb5 = ribbonPanel2.AddItem(b5Data) as PushButton;
                pb5.ToolTip = "Project Settings";
                string path5 = "C:\\Program Files\\Autodesk\\Revit 2022\\AddIns\\Resources\\ProjectSettings.png";
                if (File.Exists(path5))
                {
                    BitmapImage pb5Image = new BitmapImage(new Uri(path5));
                    pb5.LargeImage = pb5Image;
                }

                // Add a new ribbon panel
                RibbonPanel ribbonPanel = a.CreateRibbonPanel(tabName, "Creation");

                // create push button for Processing Input Lines
                PushButtonData b7Data = new PushButtonData(
                    "testCMD7",
                    "Pre Process \n Input Lines",
                    thisAssemblyPath,
                    "Revit_Automation.Command7");

                PushButton pb7 = ribbonPanel.AddItem(b7Data) as PushButton;
                pb7.ToolTip = "Extend and Trim Input Lines";
                string path7 = "C:\\Program Files\\Autodesk\\Revit 2022\\AddIns\\Resources\\ProcessLines.png";
                if (File.Exists(path7))
                {
                    BitmapImage pb7Image = new BitmapImage(new Uri(path7));
                    pb7.LargeImage = pb7Image;
                }

                // create push button for Processing Selected Input Lines
                PushButtonData b8Data = new PushButtonData(
                    "testCMD8",
                    "Pre Process \n Selected Lines",
                    thisAssemblyPath,
                    "Revit_Automation.Command8");

                PushButton pb8 = ribbonPanel.AddItem(b8Data) as PushButton;
                pb8.ToolTip = "Extend and Trim Input Lines";
                string path8 = "C:\\Program Files\\Autodesk\\Revit 2022\\AddIns\\Resources\\ProcessLines2.png";
                if (File.Exists(path8))
                {
                    BitmapImage pb8Image = new BitmapImage(new Uri(path8));
                    pb8.LargeImage = pb8Image;
                }


                // create push button for Model Creation
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


                // create push button for Post Creation at selected Lines
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

                // create push button for Panels Creation
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

                // create push button for Bottom Tracks Creation
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


/* Copyright (C) 2023 - CodeIndian Technologies  - All Rights Reserved
 * No part of this file should be copied, distributed or modified without
 * Proper appovals from the owner(s)
 * 
 */
/* -----------------------Revision History------------------------------------------
*/

#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit_Automation.Dialogs;
using System;
using System.IO;
using System.Collections.Generic;
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

            Form2 form = new Form2();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.TopMost = true;
            //form.ShowDialog();

            //if (form.ValidLicense)
            {
                // Create a custom ribbon tab
                String tabName = "Automation Toolkit";
                a.CreateRibbonTab(tabName);

                // Add a new ribbon panel
                RibbonPanel ribbonPanel = a.CreateRibbonPanel(tabName, "Automation");

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
                string path = "C:\\Users\\Administrator\\Downloads\\Gifs\\Model.png";

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
                string path2 = "C:\\Users\\Administrator\\Downloads\\Gifs\\Posts.png";
                if (File.Exists(path2))
                {
                    BitmapImage pb2Image = new BitmapImage(new Uri(path2));
                    pb2.LargeImage = pb2Image;
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

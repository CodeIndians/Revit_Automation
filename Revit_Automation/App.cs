#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media.Imaging;

#endregion

namespace Revit_Automation
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
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
                "Generate Model",
                thisAssemblyPath,
                "Revit_Automation.Command");

            PushButton pb1 = ribbonPanel.AddItem(b1Data) as PushButton;
            pb1.ToolTip = "Place posts as per the existing grids";
            BitmapImage pb1Image = new BitmapImage(new Uri("pack://application:,,,/Revit_Automation;component/Resources/Revit.png"));
            pb1.LargeImage = pb1Image;
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
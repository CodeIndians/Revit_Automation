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
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Threading;
using System.IO;

#endregion

namespace Revit_Automation
{
    enum LineType
    {
        Horizontal = 0,
        vertical
    }

    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            //TaskDialog.Show("Automation Toolkit", "Placing Columns"); // Can be used to show custom messages

            UIApplication uiapp = commandData.Application;

            Form1 form = new Form1();
            form.StartPosition = FormStartPosition.CenterScreen;
            //form.TopMost= true;
            form.ShowDialog();

            if (form.CanCreateModel)
                ModelCreator.CreateModel(uiapp, form);

            return Result.Succeeded;
        }
    }
}
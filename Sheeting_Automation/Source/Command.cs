using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Windows.Forms;
using Sheeting_Automation.Dialogs;
using Sheeting_Automation.Utils;
using Sheeting_Automation.Source.GeometryCollectors;

namespace Sheeting_Automation
{

    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;

            // Walls will be needed for the Properties Dialog
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            SheetUtils.m_Document = doc;

            SheetingConfiguration form = new SheetingConfiguration()
            {
                StartPosition = FormStartPosition.CenterScreen
            };


            _ = form.ShowDialog();

            if (form.startCollectingData)
            {
                // Start the collection logic here
                FloorGeometryCollector floorGeometry = new FloorGeometryCollector(ref doc);
                floorGeometry.Collect();
            }
            return Result.Succeeded;
        }
    }
}

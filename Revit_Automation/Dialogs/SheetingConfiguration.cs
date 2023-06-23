using Revit_Automation.Source.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Revit_Automation.Dialogs
{
    public partial class SheetingConfiguration : Form
    {
        public SheetingConfiguration()
        {
            InitializeComponent();

            List<string> listofFloorPlans  = SheetUtils.GetFloorPlans();
            foreach (string strFloorPlanName in listofFloorPlans)
            { 
                checkedListBox1.Items.Add(strFloorPlanName);
            }

            HashSet<string> listOfScales = SheetUtils.GetSheetScales();
            foreach (string strScales in listOfScales)
            {
                comboBox1.Items.Add(strScales);
            }

            radioButton1.Enabled = true;

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Enabled == false)
            {
                groupBox2.Enabled = false;
            }
            else
            {
                comboBox1.Enabled = false;
            }
        }
    }
}

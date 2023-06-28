using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sheeting_Automation.Utils;

namespace Sheeting_Automation.Dialogs
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

            radioButton1.Checked = true;

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == false)
            {
                groupBox2.Enabled = false;
                comboBox1.Enabled = true;
            }
            else
            {
                comboBox1.Enabled = false;
                groupBox2.Enabled = true;
            }
        }
    }
}

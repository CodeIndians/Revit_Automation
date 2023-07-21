
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Shop_Automation.Source;

namespace Shop_Automation.Dialogs
{
    public partial class AseemblySettings : System.Windows.Forms.Form
    {
        public static List<string> lstViewTemplates;
        public static List<string> lstScheduleTemplates;
        public static List<string> lstTitleBlocks;

        public AseemblySettings()
        {
            InitializeComponent();

            //clear Static.
            GlobalSettings.Initialize();

            DataGridViewComboBoxColumn dgvCmb = new DataGridViewComboBoxColumn();
            dgvCmb.Width = 150;
            dgvCmb.HeaderText = "View Type";
            dgvCmb.Items.Add("Plan View");
            dgvCmb.Items.Add("Elevation View");
            dgvCmb.Items.Add("Wall Schedule");
            dgvCmb.Items.Add("Door Schedule");
            dgvCmb.Items.Add("Struct Connection Schedule");
            dgvCmb.Items.Add("Material Take-off");
            dgvCmb.Items.Add("Generic Models Schedule");
            dgvCmb.Name = "cmbName";

            DataGridViewComboBoxColumn dgvCmb2 = new DataGridViewComboBoxColumn();
            dgvCmb2.HeaderText = "View Template";
            dgvCmb2.Width = 150;   
            foreach (string s in lstViewTemplates) {
                dgvCmb2.Items.Add(s);
            }
            dgvCmb2.Name = "cmbName2";

            dataGridView1.Columns.Add(dgvCmb);
            dataGridView1.Columns.Add(dgvCmb2);
            foreach (string s in lstTitleBlocks) { comboBox1.Items.Add(s); }
        }

        public List<string> m_ElevationViewTypes { get; internal set; }
        public List<string> m_ScheduleTypes { get; internal set; }

        private void button1_Click(object sender, EventArgs e)
        {
            // Validations

            if (comboBox1.SelectedIndex == -1)
            {
                TaskDialog.Show("Assembly Settings", "Please Select a valid view Template before closing");
                return;
            }

            if (comboBox2.SelectedIndex == -1)
            {
                TaskDialog.Show("Assembly Settings", "Please Select a valid View Direction before closing");
                return;
            }

            SaveViewCreationOptions();
            this.Close();
        }

        private void SaveViewCreationOptions()
        {
            GlobalSettings.s_ViewDirection = this.comboBox2.SelectedItem.ToString();
            GlobalSettings.s_SheetTemplate = this.comboBox1.SelectedItem.ToString();

            GlobalSettings.dicViewsToBeCreated.Clear();
            foreach (DataGridViewRow dr in dataGridView1.Rows)
            {
                if (!dr.IsNewRow)
                {
                    GlobalSettings.dicViewsToBeCreated.Add(new KeyValuePair<string, string>
                        (dr.Cells[0].Value.ToString(), dr.Cells[1].Value.ToString())) ;
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                DataGridViewComboBoxCell comboBoxCell = (DataGridViewComboBoxCell)dataGridView1.Rows[e.RowIndex].Cells[0];
                string selectedOption = comboBoxCell.Value.ToString();

                DataGridViewComboBoxCell comboBoxCell2 = (DataGridViewComboBoxCell)dataGridView1.Rows[e.RowIndex].Cells[1];

                if (selectedOption == "Plan View" || selectedOption == "Elevation View")
                {
                    PopulateTheViewTypeCombo(lstViewTemplates, comboBoxCell2);
                }
                else
                {
                    PopulateTheViewTypeCombo(lstScheduleTemplates, comboBoxCell2);
                }
            }
        }

        private void PopulateTheViewTypeCombo(List<string> lstTemplates, DataGridViewComboBoxCell comboBoxCell2)
        {
            comboBoxCell2.Items.Clear();
            foreach (string template in lstTemplates) { comboBoxCell2.Items.Add(template); }
        }
    }
}

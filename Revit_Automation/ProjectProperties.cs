using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Revit_Automation
{
    public partial class ProjectProperties : System.Windows.Forms.Form
    {
        private DataTable dataTable;
        private DataGridViewCheckBoxColumn radioButtonColumn;
        private Document m_doc;
        public ProjectProperties()
        {
            InitializeComponent();
            dataTable = new DataTable();

            // Initialize and configure the CheckBox column
            radioButtonColumn = new DataGridViewCheckBoxColumn();
            radioButtonColumn.HeaderText = "UNO";
            radioButtonColumn.Name = "RadioButtonColumn";

            dataTable.Columns.Add("Panel Type", typeof(string));
            dataTable.Columns.Add("Panel Clearance", typeof(int));
            dataTable.Columns.Add("Panel Max. Lap", typeof(int));
            dataTable.Columns.Add("Panel Min. Lap", typeof(int));
            dataTable.Columns.Add("Panel Orientation (H/V)", typeof(int));
            dataTable.Columns.Add("Preferred panel length", typeof(int));
            dataTable.Columns.Add("Max Panel Length", typeof(int));
            dataTable.Columns.Add("Panel Height Offset", typeof(int));
            dataTable.Columns.Add("Horizontal Panel Dir (U/D/B)", typeof(string));
            dataTable.Columns.Add("Vertical Panel Dir (L/R/B)", typeof(string));
            dataTable.Columns.Add("Hour rate", typeof(int));
            dataGridView1.Columns.Add(radioButtonColumn);
            dataGridView1.CellContentClick += dataGridView_CellContentClick;
            dataGridView1.DataSource = dataTable;

            foreach (WallType wall in SymbolCollector.WallSymbols)
            {
                dataTable.Rows.Add(wall.Name, 0, 0, 0, 0, 0, 0, 0, "", "", 0);
            }
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["RadioButtonColumn"].Index)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataGridViewCheckBoxCell checkBoxCell = row.Cells["RadioButtonColumn"] as DataGridViewCheckBoxCell;
                    if (row.Index == e.RowIndex)
                    {
                        checkBoxCell.Value = true;  // Check the clicked checkbox
                    }
                    else
                    {
                        checkBoxCell.Value = false; // Uncheck all other checkboxes
                    }
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

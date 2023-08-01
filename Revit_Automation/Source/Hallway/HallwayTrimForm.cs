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
using static Revit_Automation.Source.Hallway.HallwayGenerator;
using Form = System.Windows.Forms.Form;

namespace Revit_Automation.Source.Hallway
{
    public partial class HallwayTrimForm : Form
    {
        private Document mDocument;

        private List<LabelLine> mHorizontalLabelLines;

        private List<LabelLine> mVerticalLabelLines;

        public HallwayTrimForm( ref Document doc, List<LabelLine> horLabelLines, List<LabelLine> verLabelLines)
        {
            mDocument = doc;

            mHorizontalLabelLines = horLabelLines;

            mVerticalLabelLines = verLabelLines;

            InitializeComponent();

            InitializeHorizontalLinesGrid();

            InitializeVerticalLinesGrid();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void InitializeHorizontalLinesGrid()
        {
            // add the required columns 
            dataGridView1.Columns.Add("Line Name","Horizontal Line Name");
            dataGridView1.Columns.Add("Top", "Top");
            dataGridView1.Columns.Add("Bottom", "Bottom");

            // set autofill property
            dataGridView1.Columns["Line Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Top"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Bottom"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView1.Columns["Line Name"].ReadOnly = true;
            dataGridView1.Columns["Line Name"].DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;

            var zeroString = "0";

            foreach(var line in mHorizontalLabelLines)
            {
                dataGridView1.Rows.Add(line.mLabel, zeroString, zeroString);
            }

            // Attach CellValidating event handler
            dataGridView1.CellValidating += DataGridView1_CellValidating;

            // Donot alow users to add rows
            dataGridView1.AllowUserToAddRows = false;


        }

        private void DataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridView dataGridView = (DataGridView)sender;
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                // Validate the value only for specific columns (in this case, Column1)
                if (dataGridView.Columns[e.ColumnIndex].Name == "Top" || dataGridView.Columns[e.ColumnIndex].Name == "Bottom")
                {
                    if (!int.TryParse(e.FormattedValue.ToString(), out int value))
                    {
                        // The entered value is not a valid integer
                        dataGridView.Rows[e.RowIndex].ErrorText = "Invalid input. Please enter a number.";
                        e.Cancel = true; // Cancel the cell validation
                    }
                    else if (value < 0 || value > 2)
                    {
                        // The entered value is not within the specified range
                        dataGridView.Rows[e.RowIndex].ErrorText = "Please enter a number between 0 and 2.";
                        e.Cancel = true; // Cancel the cell validation
                    }
                    else
                    {
                        dataGridView.Rows[e.RowIndex].ErrorText = "";
                        e.Cancel = false;
                    }
                }
            }
        }

        private void InitializeVerticalLinesGrid()
        {
            dataGridView2.Columns.Add("Line Name", "Vertical Line Name");
            dataGridView2.Columns.Add("Left", "Left");
            dataGridView2.Columns.Add("Right", "Right");

            dataGridView2.Columns["Line Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView2.Columns["Left"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView2.Columns["Right"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView2.Columns["Line Name"].ReadOnly = true;
            dataGridView2.Columns["Line Name"].DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            

            var zeroString = "0";

            foreach (var line in mVerticalLabelLines)
            {
                dataGridView2.Rows.Add(line.mLabel, zeroString, zeroString);
            }

            // Donot alow users to add rows
            dataGridView2.AllowUserToAddRows = false;
        }
    }
}

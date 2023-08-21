using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace Revit_Automation.Source.Hallway
{
    public partial class HallwayTrimForm : Form
    {
        private Document mDocument;

        private List<HallwayLabelLine> mHorizontalLabelLines;

        private List<HallwayLabelLine> mVerticalLabelLines;

        public HallwayTrimForm(ref Document doc, List<HallwayLabelLine> horLabelLines, List<HallwayLabelLine> verLabelLines)
        {
            mDocument = doc;

            //TODO . We can remove one of these
            mHorizontalLabelLines = horLabelLines;

            mVerticalLabelLines = verLabelLines;

            HallwayTrimData.HorizontalLabelLines = horLabelLines;

            HallwayTrimData.VerticalLabelLines = verLabelLines;

            InitializeComponent();

            InitializeHorizontalLinesGrid();

            InitializeVerticalLinesGrid();
        }

        /// <summary>
        /// Trim Hallway Lines
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            PopulateHallwayData();

            if (HallwayTrimData.Validate())
                this.Close();
            else
                MessageBox.Show("Validation failed");

        }

        private void PopulateHallwayData()
        {
            HallwayTrimData.TrimDataHorizontal.Columns.Clear();
            HallwayTrimData.TrimDataHorizontal.Rows.Clear();

            // capture the horizontal labels
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                HallwayTrimData.TrimDataHorizontal.Columns.Add(column.HeaderText, typeof(string));
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataRow newRow = HallwayTrimData.TrimDataHorizontal.NewRow();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    newRow[cell.ColumnIndex] = cell.Value;
                }
                HallwayTrimData.TrimDataHorizontal.Rows.Add(newRow);
            }

            HallwayTrimData.TrimDataVertical.Columns.Clear();
            HallwayTrimData.TrimDataVertical.Rows.Clear();

            // capture the vertical labels
            foreach (DataGridViewColumn column in dataGridView2.Columns)
            {
                HallwayTrimData.TrimDataVertical.Columns.Add(column.HeaderText, typeof(string));
            }

            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                DataRow newRow = HallwayTrimData.TrimDataVertical.NewRow();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    newRow[cell.ColumnIndex] = cell.Value;
                }
                HallwayTrimData.TrimDataVertical.Rows.Add(newRow);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // not implemented for now
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // not implemented for now
        }

        private void InitializeHorizontalLinesGrid()
        {
            // add the required columns 
            dataGridView1.Columns.Add("Line Name", "Horizontal Line Name");
            dataGridView1.Columns.Add("Top", "Top");
            dataGridView1.Columns.Add("Bottom", "Bottom");

            // set autofill property
            dataGridView1.Columns["Line Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Top"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Bottom"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView1.Columns["Line Name"].ReadOnly = true;
            dataGridView1.Columns["Line Name"].DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;

            var zeroString = "0";

            foreach (var line in mHorizontalLabelLines)
            {
                dataGridView1.Rows.Add(line.mLabel, zeroString, zeroString);
            }

            // Attach CellValidating event handler
            dataGridView1.CellValidating += DataGridView1_CellValidating;

            // Do not allow users to add rows
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

        private void DataGridView2_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridView dataGridView = (DataGridView)sender;
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                // Validate the value only for specific columns (in this case, Column1)
                if (dataGridView.Columns[e.ColumnIndex].Name == "Left" || dataGridView.Columns[e.ColumnIndex].Name == "Right")
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

            // Attach CellValidating event handler
            dataGridView2.CellValidating += DataGridView2_CellValidating;

            // Donot alow users to add rows
            dataGridView2.AllowUserToAddRows = false;
        }
    }
}

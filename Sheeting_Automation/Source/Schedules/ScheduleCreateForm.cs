using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sheeting_Automation.Source.Schedules
{
    public partial class ScheduleCreateForm : Form
    {
        public ScheduleCreateForm()
        {
            InitializeComponent();

            //set the form properties
            SetForm();

            // initialize the data grid 
            InitializeDataGridView();
        }

        /// <summary>
        /// Form properties
        /// </summary>
        private void SetForm()
        {
            // Set form properties for a fixed size
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
        }


        private void InitializeDataGridView()
        {
            // Disable user's ability to add rows directly from the DataGridView
            dataGridView1.AllowUserToAddRows = false;

            // Define the column types that are required in the form
            DataGridViewTextBoxColumn sheetNameColumn = new DataGridViewTextBoxColumn();
            DataGridViewComboBoxColumn categoryColumn = new DataGridViewComboBoxColumn();
            DataGridViewComboBoxColumn viewTemplateColumn = new DataGridViewComboBoxColumn();
            DataGridViewComboBoxColumn phaseColumn = new DataGridViewComboBoxColumn();
            DataGridViewTextBoxColumn prefixColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn startColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn suffixColumn = new DataGridViewTextBoxColumn();
            DataGridViewButtonColumn deleteButtonColumn = new DataGridViewButtonColumn();

            // Add the column types to the form
            dataGridView1.Columns.Add(sheetNameColumn);
            dataGridView1.Columns.Add(categoryColumn);
            dataGridView1.Columns.Add(viewTemplateColumn);
            dataGridView1.Columns.Add(phaseColumn);
            dataGridView1.Columns.Add(prefixColumn);
            dataGridView1.Columns.Add(startColumn);
            dataGridView1.Columns.Add(suffixColumn);
            dataGridView1.Columns.Add(deleteButtonColumn);

            // Set column headers
            dataGridView1.Columns[0].HeaderText = "Schedule Name";
            dataGridView1.Columns[1].HeaderText = "Category";
            dataGridView1.Columns[2].HeaderText = "View Template";
            dataGridView1.Columns[3].HeaderText = "Phase";
            dataGridView1.Columns[4].HeaderText = "Prefix";
            dataGridView1.Columns[5].HeaderText = "Start";
            dataGridView1.Columns[6].HeaderText = "Suffix";
            dataGridView1.Columns[7].HeaderText = "Delete";

            // Set AutoSizeMode to Fill for all columns
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            // Populate combo box columns with the required data
            categoryColumn.DataSource = new List<string>(ScheduleData.CategoryDictionary.Keys);
            viewTemplateColumn.DataSource = new List<string>(ScheduleData.ViewTemplateDictionary.Keys);
            phaseColumn.DataSource = new List<string>(ScheduleData.PhaseDictionary.Keys);

            // Set the delete button column properties
            deleteButtonColumn.UseColumnTextForButtonValue = true;
            deleteButtonColumn.Text = "Delete";

            // add the cell content click call handler function
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;

            // Add the DataGridView to the form's controls
            Controls.Add(dataGridView1);
        }

        /// <summary>
        /// Create button handler function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createButton_Click(object sender, EventArgs e)
        {
            if(ValidateRows() == true)
            {
                var scheduleCreator = new ScheduleCreator();

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    string sheetName = row.Cells[0].Value?.ToString();
                    string category = row.Cells[1].Value?.ToString();
                    string viewTemplate = row.Cells[2].Value?.ToString();
                    string phase = row.Cells[3].Value?.ToString();
                    string prefix = row.Cells[4].Value?.ToString();
                    string start = row.Cells[5].Value?.ToString();
                    string suffix = row.Cells[6].Value?.ToString();

                    scheduleCreator.Create(sheetName, category, phase, viewTemplate, prefix,start,suffix);
                }

                // show the creation complete message
                MessageBox.Show($"{dataGridView1.Rows.Count} schedules are created");

                // close the form
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid data");
            }
        }

        /// <summary>
        /// Validate all the rows
        /// </summary>
        /// <returns>true if valid else false </returns>
        private bool ValidateRows()
        {
            dataGridView1.EndEdit(); // Commit any pending edits before validation

            bool isValid = true;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                // Validate specific columns for empty values
                string sheetName = row.Cells[0].Value?.ToString();
                string category = row.Cells[1].Value?.ToString();
                string viewTemplate = row.Cells[2].Value?.ToString();
                string phase = row.Cells[3].Value?.ToString();
                string prefix = row.Cells[4].Value?.ToString();
                string start = row.Cells[5].Value?.ToString();

                //except for the sufix all the rows must be filled
                if (string.IsNullOrEmpty(sheetName) || string.IsNullOrEmpty(category) ||
                    string.IsNullOrEmpty(viewTemplate) || string.IsNullOrEmpty(phase) ||
                    string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(start))
                {
                    row.ErrorText = "All fields must be filled";
                    isValid = false;
                }
                else
                {
                    row.ErrorText = string.Empty;
                }
            }

            return isValid;
        }

        // event handler function for clicking the cell
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView1.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                // delete the row when the delete button is pressed
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Delete")
                    dataGridView1.Rows.RemoveAt(e.RowIndex);
            }
        }

        // add the rows 
        private void addRows_Click(object sender, EventArgs e)
        {
            // Add a new row to the DataGridView
            int rowIndex = dataGridView1.Rows.Add();
        }
    }
}

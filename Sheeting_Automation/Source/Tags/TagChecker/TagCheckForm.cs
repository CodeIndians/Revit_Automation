using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sheeting_Automation.Source.Tags
{
    public partial class TagCheckForm : Form
    {
        public TagCheckForm()
        {
            InitializeComponent();

            SetForm();

            InitializeDataGridView();
        }

        /// <summary>
        /// Set the form properties here
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

            // Define the column types 
            DataGridViewComboBoxColumn modelCategoryTagColumn = new DataGridViewComboBoxColumn();
            DataGridViewComboBoxColumn familyNameColumn = new DataGridViewComboBoxColumn();
            DataGridViewButtonColumn checkTagButtonColumn = new DataGridViewButtonColumn();
            DataGridViewButtonColumn deleteButtonColumn = new DataGridViewButtonColumn();

            // add the columns 
            dataGridView1.Columns.Add(modelCategoryTagColumn);
            dataGridView1.Columns.Add(familyNameColumn);
            dataGridView1.Columns.Add(checkTagButtonColumn);
            dataGridView1.Columns.Add(deleteButtonColumn);

            // Set column headers
            dataGridView1.Columns[0].HeaderText = "Category Tag";
            dataGridView1.Columns[1].HeaderText = "Element Family";
            dataGridView1.Columns[2].HeaderText = "Check";
            dataGridView1.Columns[3].HeaderText = "Delete";

            // Set AutoSizeMode to Fill for all columns
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            // Populate combo box columns with the required data
            modelCategoryTagColumn.DataSource = new List<string>(TagData.TaggableCategoriesDict.Keys);

            // Set the delete button column properties
            deleteButtonColumn.UseColumnTextForButtonValue = true;
            deleteButtonColumn.Text = "Delete";

            // set the check button column properties 
            checkTagButtonColumn.UseColumnTextForButtonValue = true;
            checkTagButtonColumn.Text = "Check";

            // add the cell content click call handler function
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;


            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;


            dataGridView1.CurrentCellDirtyStateChanged += new EventHandler(dataGridView1_CurrentCellDirtyStateChanged);

            // Add the DataGridView to the form's controls
            Controls.Add(dataGridView1);
        }

        // This event handler manually raises the CellValueChanged event 
        // by calling the CommitEdit method. 
        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                // This fires the cell value changed handler below
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                // collect all the combo cells in the given row 
                DataGridViewComboBoxCell categoryComboBoxCell = (DataGridViewComboBoxCell)dataGridView1.Rows[e.RowIndex].Cells[0];
                DataGridViewComboBoxCell elementComboBoxCell = (DataGridViewComboBoxCell)dataGridView1.Rows[e.RowIndex].Cells[1];

                // Get the selected value from the first ComboBox
                string selectedValue = categoryComboBoxCell.Value.ToString();

                // noTagvalue to remove the "Tags"
                string noTagValue = TagUtils.GetNoTagValue(selectedValue);

                // get tag family names 
                var tagDict = TagUtils.GetAnnotationSymbolFamilyNames(TagData.TaggableCategoriesDict[selectedValue]);

                // get element family names in the view 
                var eleDict = TagUtils.GetElementFamilyNames(TagData.ViewCategoriesDict[noTagValue]);

                elementComboBoxCell.Items.Clear();

                elementComboBoxCell.Items.AddRange((new List<string>(eleDict.Keys)).ToArray());

                elementComboBoxCell.Items.Add("ALL");

                if (elementComboBoxCell.Items.Count > 0)
                    elementComboBoxCell.Value = elementComboBoxCell.Items[0];

            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView1.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                // delete the row when the delete button is pressed
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Delete")
                    dataGridView1.Rows.RemoveAt(e.RowIndex);
                else if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Check")
                {
                    var formData = CollectFormData(e.RowIndex);
                    TagChecker.CheckTags(formData);
                }
            }
        }

        // Add rows 
        private void addRowsButton_Click(object sender, EventArgs e)
        {
            // Add a new row to the DataGridView
            int rowIndex = dataGridView1.Rows.Add();
        }

        private void checkAllButton_Click(object sender, EventArgs e)
        {
            //TODO: Inmplement check all functionality 
            var formData = CollectFormData();
            TagChecker.CheckTags(formData);

            //this.Close();
        }

        private TagData.TagCheckFormData CollectFormData(int rowNum)
        {
            // intialize check form data struct
            TagData.TagCheckFormData formData = new TagData.TagCheckFormData();

            DataGridViewRow row = dataGridView1.Rows[rowNum];

            // skip if the row is empty 
            if (row.Cells[0].Value == null)
                return formData;

            // capture cell data into form data struct
            formData.CategoryColumn = row.Cells[0].Value.ToString();

            if (row.Cells[1].Value.ToString() != "ALL")
                formData.ElementColumn = new List<string> { row.Cells[1].Value.ToString() };
            else
            {
                formData.ElementColumn = (row.Cells[1] as DataGridViewComboBoxCell).Items.Cast<string>().ToList();
                formData.ElementColumn.RemoveAt(formData.ElementColumn.Count - 1);
            }

            return formData;
        }

        private List<TagData.TagCheckFormData> CollectFormData()
        {
            List<TagData.TagCheckFormData> formDataList = new List<TagData.TagCheckFormData>();

            // iterate through all the rows 
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                // intialize check form data struct
                TagData.TagCheckFormData formData = new TagData.TagCheckFormData();

                // skip if the row is empty 
                if (row.Cells[0].Value == null)
                    continue;

                // capture cell data into form data struct
                formData.CategoryColumn = row.Cells[0].Value.ToString();

                if (row.Cells[1].Value.ToString() != "ALL")
                    formData.ElementColumn = new List<string> { row.Cells[1].Value.ToString() };
                else
                {
                    formData.ElementColumn = (row.Cells[1] as DataGridViewComboBoxCell).Items.Cast<string>().ToList();
                    formData.ElementColumn.RemoveAt(formData.ElementColumn.Count - 1);
                }

                // add it to the list 
                formDataList.Add(formData);
            }

            return formDataList;
        }
    }
}

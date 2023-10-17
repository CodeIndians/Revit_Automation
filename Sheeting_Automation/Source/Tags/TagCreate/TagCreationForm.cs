using Sheeting_Automation.Source.Tags.TagCreator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Sheeting_Automation.Source.Tags
{
    public partial class TagCreationForm : Form
    {
        private int m_ActiveCellCol = -1;

        private BackgroundWorker worker = new BackgroundWorker();
        public TagCreationForm()
        {
            InitializeComponent();

            // set the form properties 
            SetForm();

            InitializeDataGridView();

            // configure the background worker
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            //start the background operation ( collecting bounding boxes)
            worker.RunWorkerAsync();
        }

        /// <summary>
        ///  Do the back ground work
        ///  Collect the bounding boxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            createButton.Enabled = false;
            BoundingBoxCollector.Initialize();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            createButton.Enabled = true;
            //var count = BoundingBoxCollector.BoundingBoxesDict.Count;
            //Console.WriteLine(count);
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
            DataGridViewComboBoxColumn familyTagColumn = new DataGridViewComboBoxColumn();
            DataGridViewCheckBoxColumn leaderColumn = new DataGridViewCheckBoxColumn();
            DataGridViewButtonColumn deleteButtonColumn = new DataGridViewButtonColumn();

            // add the columns 
            dataGridView1.Columns.Add(modelCategoryTagColumn);
            dataGridView1.Columns.Add(familyNameColumn);
            dataGridView1.Columns.Add(familyTagColumn);
            dataGridView1.Columns.Add(leaderColumn);
            dataGridView1.Columns.Add(deleteButtonColumn);

            // Set column headers
            dataGridView1.Columns[0].HeaderText = "Category Tag";
            dataGridView1.Columns[1].HeaderText = "Element Family";
            dataGridView1.Columns[2].HeaderText = "Tag family";
            dataGridView1.Columns[3].HeaderText = "Leader";
            dataGridView1.Columns[4].HeaderText = "Delete";

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

            // add the cell content click call handler function
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;


            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;


            dataGridView1.CurrentCellDirtyStateChanged += new EventHandler(dataGridView1_CurrentCellDirtyStateChanged);

            // Add the DataGridView to the form's controls
            Controls.Add(dataGridView1);

        }

        // This event handler manually raises the CellValueChanged event 
        // by calling the CommitEdit method. 
        void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                // This fires the cell value changed handler below
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                // collect all the combo cells in the given row 
                DataGridViewComboBoxCell categoryComboBoxCell = (DataGridViewComboBoxCell)dataGridView1.Rows[e.RowIndex].Cells[0];
                DataGridViewComboBoxCell elementComboBoxCell = (DataGridViewComboBoxCell)dataGridView1.Rows[e.RowIndex].Cells[1];
                DataGridViewComboBoxCell tagComboBoxCell = (DataGridViewComboBoxCell)dataGridView1.Rows[e.RowIndex].Cells[2];

                // Get the selected value from the first ComboBox
                string selectedValue = categoryComboBoxCell.Value.ToString();

                // noTagvalue to remove the "Tags"
                string noTagValue =  TagUtils.GetNoTagValue(selectedValue);

                // get tag family names 
                var tagDict = TagUtils.GetAnnotationSymbolFamilyNames(TagData.TaggableCategoriesDict[selectedValue]);

                tagComboBoxCell.Items.Clear();

                tagComboBoxCell.Items.AddRange((new List<string>(tagDict.Keys)).ToArray());

                if (tagComboBoxCell.Items.Count > 0)
                    tagComboBoxCell.Value = tagComboBoxCell.Items[0];

                // get element family names in the view 
                var eleDict = TagUtils.GetElementFamilyNames(TagData.ViewCategoriesDict[noTagValue]);

                elementComboBoxCell.Items.Clear();

                elementComboBoxCell.Items.AddRange((new List<string>(eleDict.Keys)).ToArray());

                elementComboBoxCell.Items.Add("ALL");

                if (elementComboBoxCell.Items.Count > 0)
                    elementComboBoxCell.Value = elementComboBoxCell.Items[0];

            }
        }

        // Add rows 
        private void addCategoryButtom_Click(object sender, EventArgs e)
        {
            // Add a new row to the DataGridView
            int rowIndex = dataGridView1.Rows.Add();
        }

        // Event handler function for clicking the cell
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView1.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                // delete the row when the delete button is pressed
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Delete")
                    dataGridView1.Rows.RemoveAt(e.RowIndex);
            }

        }

        private void createButton_Click(object sender, EventArgs e)
        {
            // collect the form data 
            var formDataList = CollectFormData();

            var tagCreator = new TagCreator.TagCreator(formDataList);

            //create tags at the default location
            tagCreator.CreateTags();

            // update the tag bounding box data structure
            BoundingBoxCollector.UpdateTagBoundingBoxes();

            //adjust the tags
            TagAdjust.AdjustTagsBasedOnElementsOnly();

            var tagResolveManager = new TagResolverManager();

            //close the create form
            this.Close();
        }

        // collect form data into TagFormData
        private List<TagData.TagCreateFormData> CollectFormData()
        {
            List < TagData.TagCreateFormData> formDataList = new List<TagData.TagCreateFormData>();

            // iterate through all the rows 
            foreach(DataGridViewRow row in dataGridView1.Rows)
            {
                // initialize  form data struct 
                TagData.TagCreateFormData formData = new TagData.TagCreateFormData();

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
                formData.TagColumn = row.Cells[2].Value.ToString();
                formData.Leader = ((row.Cells[3] as DataGridViewCheckBoxCell).Value == null)  ? false : true;

                // add it to the list 
                formDataList.Add(formData);
            }

            return formDataList;

        }
    }
}

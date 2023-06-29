﻿using Autodesk.Revit.DB;
using Revit_Automation.Source;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using static Autodesk.Revit.DB.SpecTypeId;

namespace Revit_Automation
{
    public partial class ProjectProperties : System.Windows.Forms.Form
    {
        private DataTable dataTable;
        private DataGridViewCheckBoxColumn radioButtonColumn;
        private Document m_doc;

        public static int iUNONumber;
        public ProjectProperties()
        {
            iUNONumber = -1;

            InitializeComponent();
  
            dataTable = new DataTable();

            // Initialize and configure the CheckBox column
            radioButtonColumn = new DataGridViewCheckBoxColumn();
            radioButtonColumn.HeaderText = "UNO";
            radioButtonColumn.Name = "RadioButtonColumn";

            dataTable.Columns.Add("Panel Type", typeof(string));
            dataTable.Columns.Add("Panel Gauge", typeof(double));
            dataTable.Columns.Add("Panel Clearance", typeof(double));
            dataTable.Columns.Add("Panel Max. Lap", typeof(double));
            dataTable.Columns.Add("Panel Min. Lap", typeof(double));
            dataTable.Columns.Add("Panel Orientation (H/V)", typeof(string));
            dataTable.Columns.Add("Preferred panel length", typeof(double));
            dataTable.Columns.Add("Max Panel Length", typeof(double));
            dataTable.Columns.Add("Panel Height Offset", typeof(double));
            dataTable.Columns.Add("Horizontal Panel Dir (U/D/B)", typeof(string));
            dataTable.Columns.Add("Vertical Panel Dir (L/R/B)", typeof(string));
            dataTable.Columns.Add("Hour rate", typeof(double));
            dataGridView1.Columns.Add(radioButtonColumn);
            dataGridView1.CellContentClick += dataGridView_CellContentClick;
            dataGridView1.DataSource = dataTable;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;

            string strProjectSettings = InputLineUtility.GetProjectSettings();

            if (string.IsNullOrEmpty(strProjectSettings))
                PopulateDefaultSettings();
            else
                PopulateSettingsFromString(strProjectSettings);
        }

       /// <summary>
       /// Populates the form from the settings stored on the project settings line
       /// </summary>
       /// <param name="strProjectSettings"></param>
        private void PopulateSettingsFromString(string strProjectSettings)
        {
            try
            {
                string[] settings = strProjectSettings.Split('|');

                // Building Type
                comboBox1.SelectedIndex = int.Parse(settings[0]);
                strProjectSettings += "|";

                // Bottom Track Preferred length
                textBox1.Text = settings[1];
                textBox6.Text = settings[2];

                //top track preferred length
                textBox3.Text = settings[3];
                textBox2.Text = settings[4];

                // Bottom Track max length
                textBox8.Text = settings[5];
                textBox7.Text = settings[6];

                //Top Track max length
                textBox10.Text = settings[7];
                textBox9.Text = settings[8]; 

                DataTable tempdataTable = (DataTable)dataGridView1.DataSource;

                int iUNORowNumber = int.Parse(settings[9]);

                // Save the number on the static
                iUNONumber = iUNORowNumber;
               
                // Fill the Data table
                int icounter = 10;
                while (icounter < settings.Length - 1)
                {
                    tempdataTable.Rows.Add(settings[icounter], settings[icounter + 1], settings[icounter + 2], settings[icounter + 3], settings[icounter + 4],
                        settings[icounter + 5], settings[icounter + 6], settings[icounter + 7], settings[icounter + 8], settings[icounter + 9],
                        settings[icounter + 10], settings[icounter + 11]);

                    icounter += 12;
                }

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataGridViewCheckBoxCell checkBoxCell = row.Cells["RadioButtonColumn"] as DataGridViewCheckBoxCell;
                    if (row.Index == iUNORowNumber)
                    {
                        checkBoxCell.Value = true;  // Check the clicked checkbox
                        break;
                    }
                }

                dataGridView1.Invalidate();
                dataGridView1.Update();
            }
            catch (Exception)
            {
                PopulateDefaultSettings();
            }
        }

        /// <summary>
        /// To make the checkbnox behave like a radio button, this delegate is used
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["RadioButtonColumn"].Index)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataGridViewCheckBoxCell checkBoxCell = row.Cells["RadioButtonColumn"] as DataGridViewCheckBoxCell;
                    if (row.Index == e.RowIndex)
                    {
                        checkBoxCell.Value = true; // Check the clicked checkbox
                        iUNONumber = row.Index; 
                    }
                    else
                    {
                        checkBoxCell.Value = false; // Uncheck all other checkboxes
                    }
                }
            }
        }

        /// <summary>
        /// When a tab selected is changed we are taking care of highlighting the selected checkbox for UNO
        /// This was needed because at the time of form load, the UNO setting is not reflected for some reason
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected tab index
            int selectedIndex = tabControl1.SelectedIndex;

            // Perform actions based on the selected tab index
            switch (selectedIndex)
            {
                case 1:
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        DataGridViewCheckBoxCell checkBoxCell = row.Cells["RadioButtonColumn"] as DataGridViewCheckBoxCell;
                        if (row.Index == iUNONumber)
                        {
                            checkBoxCell.Value = true; // Check the clicked checkbox
                            iUNONumber = row.Index;
                        }
                    }

                    break;

                default: break;
                    // Add more cases for additional tabs as needed
            }
        }
        /// <summary>
        /// Close Button 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Save Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            SerializeProjectSettings();
            this.Close();
        }

        /// <summary>
        /// This method is used to read the values from the form and save it to a Line
        /// </summary>
        private void SerializeProjectSettings()
        {
            string strProjectSettings = "";
            bool bFoundUNO = false;

            // Building Type
            strProjectSettings += comboBox1.SelectedIndex.ToString() ;
            strProjectSettings += "|";

            // Bottom Track Preferred length
            strProjectSettings += textBox1.Text.ToString() + "|" + textBox6.Text.ToString();
            strProjectSettings += "|";

            //top track preferred length
            strProjectSettings += textBox3.Text.ToString() + "|" + textBox2.Text.ToString();
            strProjectSettings += "|";

            // Bottom Track max length
            strProjectSettings += textBox8.Text.ToString() + "|" + textBox7.Text.ToString();
            strProjectSettings += "|";

            //Top Track max length
            strProjectSettings += textBox10.Text.ToString() + "|" + textBox9.Text.ToString();
            strProjectSettings += "|";

            DataTable dt = (DataTable)dataGridView1.DataSource;


            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell checkBoxCell = row.Cells["RadioButtonColumn"] as DataGridViewCheckBoxCell;
                if (checkBoxCell.Value != null && true == (bool)checkBoxCell.Value)
                {
                    strProjectSettings += row.Index.ToString();
                    strProjectSettings += "|";
                    bFoundUNO = true;
                    break;
                }
            }

            if (!bFoundUNO)
            {
                strProjectSettings += -1;
                strProjectSettings += "|";
            }

            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn column in row.Table.Columns)
                {
                    // Retrieve the value from the cell using column name or index
                    object cellValue = row[column.ColumnName];

                    // Perform any required parsing or data manipulation
                    if (cellValue != DBNull.Value)
                    {
                        string parsedValue = cellValue.ToString();
                        strProjectSettings += parsedValue;
                        strProjectSettings += "|";
                    }
                    else
                    {
                        string parsedValue = "0";
                        strProjectSettings += parsedValue;
                        strProjectSettings += "|";
                    }
                }    
            }

            InputLineUtility.SetProjectSettings(strProjectSettings);

        }

        /// <summary>
        /// This method populates the form with default values
        /// </summary>
        private void PopulateDefaultSettings()
        {
            DataTable tempdataTable = (DataTable)dataGridView1.DataSource;

            // Fill empty data
            foreach (string strWallType in InputLineUtility.wallTypes)
            {
                if (strWallType == "" || strWallType == " ")
                    continue;
                tempdataTable.Rows.Add(strWallType, 0, 0, 0, 0, " ", 0, 0, 0, " ", " ", 0);
            }

            // Set the data source on the Data Grid
            dataGridView1.DataSource = tempdataTable;
        }
    }
}

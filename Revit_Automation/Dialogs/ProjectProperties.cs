using Autodesk.Revit.DB;
using Revit_Automation.Source;
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

            InitializeComponent();

            dataGridView1.CellContentClick += dataGridView_CellContentClick;

            // TODO : Add values for DragStruct type, eve struct type, they are dynamic check boxes
            string strProjectSettings = InputLineUtility.GetProjectSettings();

            // Populate the settings from the Line or else default settings
            if (string.IsNullOrEmpty(strProjectSettings))
            {
                PopulateDefaultSettings();
                PopulateDefaultCHeaderSettings();
            }
            else
                PopulateSettingsFromString(strProjectSettings);

        }

        private void PopulateDefaultCHeaderSettings()
        {
            IOrderedEnumerable<Level> levels = LevelCollector.levels;

            // Fill empty data
            foreach (Level currLevel in levels)
            {
                DataGridViewRow row = new DataGridViewRow();

                List<string> ceeHeaderList = SymbolCollector.GetCeeHeaders();

                DataGridViewCell checkBoxCell = new DataGridViewCheckBoxCell();
                checkBoxCell.Value = true;


                DataGridViewCell GridCell = new DataGridViewTextBoxCell();
                GridCell.Value = currLevel.Name;

                DataGridViewComboBoxCell cell = new  DataGridViewComboBoxCell();
                foreach (string ceeheader in ceeHeaderList)
                    cell.Items.Add(ceeheader);

                DataGridViewComboBoxCell cell2 = new DataGridViewComboBoxCell();
                cell2.Items.Add("Single");
                cell2.Items.Add("Double");

                DataGridViewComboBoxCell cell3 = new DataGridViewComboBoxCell();
                foreach (string ceeheader in ceeHeaderList)
                    cell3.Items.Add(ceeheader);

                DataGridViewComboBoxCell cell4 = new DataGridViewComboBoxCell();
                cell4.Items.Add("Single");
                cell4.Items.Add("Double");

                row.Cells.Add(checkBoxCell);
                row.Cells.Add(GridCell);
                row.Cells.Add(cell);
                row.Cells.Add(cell2);
                row.Cells.Add(cell3);
                row.Cells.Add(cell4);

                dataGridView2.Rows.Add(row);
            }
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

                // The first one is panel settings row;
                string strPanelSettings = settings[0];
                
                string[] panelSettings = strPanelSettings.Split(';');
                int iCounter = 0;

                while (iCounter < panelSettings.Length - 1)
                {
                    DataGridViewRow row = new DataGridViewRow();

                    DataGridViewCell checkBoxCell = new DataGridViewCheckBoxCell();
                    checkBoxCell.Value = bool.Parse(panelSettings[iCounter++]);

                    DataGridViewCell panelType = new DataGridViewTextBoxCell();
                    panelType.Value = panelSettings[iCounter++];

                    DataGridViewCell panelGauge = new DataGridViewTextBoxCell();
                    panelGauge.Value = panelSettings[iCounter++];

                    DataGridViewCell panelClearance = new DataGridViewTextBoxCell();
                    panelClearance.Value = panelSettings[iCounter++];

                    DataGridViewCell panelMaxLap = new DataGridViewTextBoxCell();
                    panelMaxLap.Value = panelSettings[iCounter++];

                    DataGridViewCell panelMinLap = new DataGridViewTextBoxCell();
                    panelMinLap.Value = panelSettings[iCounter++];

                    DataGridViewCell panelOrientation = new DataGridViewTextBoxCell();
                    panelOrientation.Value = panelSettings[iCounter++];

                    DataGridViewCell panelPreferredLength = new DataGridViewTextBoxCell();
                    panelPreferredLength.Value = panelSettings[iCounter++];

                    DataGridViewCell maxPanelLength = new DataGridViewTextBoxCell();
                    maxPanelLength.Value = panelSettings[iCounter++];

                    DataGridViewCell panelHeightOffset = new DataGridViewTextBoxCell();
                    panelHeightOffset.Value = panelSettings[iCounter++];

                    DataGridViewCell horizontalPanelDirection = new DataGridViewTextBoxCell();
                    horizontalPanelDirection.Value = panelSettings[iCounter++];

                    DataGridViewCell verticalPanelDirection = new DataGridViewTextBoxCell();
                    verticalPanelDirection.Value = panelSettings[iCounter++];

                    DataGridViewCell hourRate = new DataGridViewTextBoxCell();
                    hourRate.Value = panelSettings[iCounter++];

                    row.Cells.Add(checkBoxCell);
                    row.Cells.Add(panelType);
                    row.Cells.Add(panelGauge);
                    row.Cells.Add(panelClearance);
                    row.Cells.Add(panelMaxLap);
                    row.Cells.Add(panelMinLap);
                    row.Cells.Add(panelOrientation);
                    row.Cells.Add(panelPreferredLength);
                    row.Cells.Add(maxPanelLength);
                    row.Cells.Add(panelHeightOffset);
                    row.Cells.Add(horizontalPanelDirection);
                    row.Cells.Add(verticalPanelDirection);
                    row.Cells.Add(hourRate);

                    dataGridView1.Rows.Add(row);
                }
                // Building Type
                comboBox1.SelectedIndex = int.Parse(settings[1]);
                strProjectSettings += "|";

                // Bottom Track Preferred length
                textBox1.Text = settings[2];
                textBox6.Text = settings[3];

                //top track preferred length
                textBox3.Text = settings[4];
                textBox2.Text = settings[5];

                // Bottom Track max length
                textBox8.Text = settings[6];
                textBox7.Text = settings[7];

                //Top Track max length
                textBox10.Text = settings[8];
                textBox9.Text = settings[9];

                //Panel Direction Computation
                comboBox2.SelectedIndex = int.Parse(settings[10]);

                //// UNO Row Setting
                //int iUNORowNumber = int.Parse(settings[11]);

                //// Save the number on the static
                //iUNONumber = iUNORowNumber;

                //foreach (DataGridViewRow row in dataGridView1.Rows)
                //{
                //    DataGridViewCheckBoxCell checkBoxCell = row.Cells["RadioButtonColumn"] as DataGridViewCheckBoxCell;
                //    if (row.Index == iUNORowNumber)
                //    {
                //        checkBoxCell.Value = true;  // Check the clicked checkbox
                //        break;
                //    }
                //}

                // Panel at hallway
                comboBox3.SelectedIndex = int.Parse(settings[12]);

                // Partition stud type
                textBox4.Text = settings[13].ToString();

                // Hallway Panel Thickness
                textBox5.Text = settings[14].ToString();

                // Panel Strategy
                comboBox8.SelectedIndex = int.Parse(settings[15]);

                //Deck Span
                textBox11.Text = settings[16].ToString();

                // Drag Struct Max length
                textBox12.Text = settings[17].ToString();

                //Drag Struct Type
                comboBox4.SelectedIndex = int.Parse(settings[18]);

                //Drag Struct continuous at hallway
                comboBox7.SelectedIndex = int.Parse(settings[19]);

                // Eave struct max length
                textBox13.Text = settings[20].ToString();

                //Eave Struct type
                comboBox4.SelectedIndex = int.Parse(settings[21]);

                //Eave Struct location
                comboBox6.SelectedIndex = int.Parse(settings[22]);

                string StrCeeHeaderSettings = settings[23].ToString();
                string[] strings = StrCeeHeaderSettings.Split(';');

                if (string.IsNullOrEmpty(StrCeeHeaderSettings))
                    PopulateDefaultCHeaderSettings();

                int jCounter = 0;

                while (jCounter < strings.Length - 1)
                {
                    DataGridViewRow dgvRow = new DataGridViewRow();

                    List<string> ceeHeaderList = SymbolCollector.GetCeeHeaders();

                    DataGridViewCell dgvCheckBoxCell = new DataGridViewCheckBoxCell();
                    dgvCheckBoxCell.Value = bool.Parse(strings[jCounter++]);


                    DataGridViewCell GridCell = new DataGridViewTextBoxCell();
                    GridCell.Value = strings[jCounter++];

                    DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();
                    foreach (string ceeheader in ceeHeaderList)
                        cell.Items.Add(ceeheader);
                    cell.Value = strings[jCounter++];

                    DataGridViewComboBoxCell cell2 = new DataGridViewComboBoxCell();
                    cell2.Items.Add("Single");
                    cell2.Items.Add("Double");
                    cell2.Value = strings[jCounter++];

                    DataGridViewComboBoxCell cell3 = new DataGridViewComboBoxCell();
                    foreach (string ceeheader in ceeHeaderList)
                        cell3.Items.Add(ceeheader);
                    cell3.Value = strings[jCounter++];

                    DataGridViewComboBoxCell cell4 = new DataGridViewComboBoxCell();
                    cell4.Items.Add("Single");
                    cell4.Items.Add("Double");
                    cell4.Value = strings[jCounter++];

                    dgvRow.Cells.Add(dgvCheckBoxCell);
                    dgvRow.Cells.Add(GridCell);
                    dgvRow.Cells.Add(cell);
                    dgvRow.Cells.Add(cell2);
                    dgvRow.Cells.Add(cell3);
                    dgvRow.Cells.Add(cell4);

                    dataGridView2.Rows.Add(dgvRow);
                }

                //Roof Perpendicular NLB splice
                comboBox9.SelectedIndex = int.Parse(settings[24]);

                // Round off
                comboBox10.SelectedIndex = int.Parse(settings[25]);
                
                // Top track at rake side
                comboBox11.SelectedIndex = int.Parse(settings[26]);
                
                // Top track splice at web
                comboBox12.SelectedIndex = int.Parse(settings[27]);


                // Cee Header Max length
                textBox14.Text = settings[28].ToString();

                // Floor Deck Type
                textBox15.Text = settings[29].ToString();

                // Floor Deck overlap
                textBox16.Text = settings[30].ToString();

                // Floor Deck max span
                textBox17.Text = settings[31].ToString();

                // Floor Deck Max length
                textBox18.Text = settings[32].ToString();

                // Roof Deck Type
                textBox22.Text = settings[33].ToString();

                // Roof Deck overlap
                textBox21.Text = settings[34].ToString();

                // Roof Deck max span
                textBox20.Text = settings[35].ToString();

                // Roof Deck Max length
                textBox19.Text = settings[36].ToString();

                dataGridView1.Invalidate();
                dataGridView1.Update();
            }
            catch (Exception)
            {
                PopulateDefaultSettings();
                PopulateDefaultCHeaderSettings();
            }
        }

        /// <summary>
        /// To make the checkbnox behave like a radio button, this delegate is used
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["UNO"].Index)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataGridViewCheckBoxCell checkBoxCell = row.Cells["UNO"] as DataGridViewCheckBoxCell;
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

            // Panel Settings
            DataTable dt = (DataTable)dataGridView1.DataSource;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell checkBoxCell = row.Cells[0] as DataGridViewCheckBoxCell;
                DataGridViewTextBoxCell panelType = row.Cells[1] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell panelGauge = row.Cells[2] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell panelClearance = row.Cells[3] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell panelMaxLap = row.Cells[4] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell panelMinLap = row.Cells[5] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell panelOrientation = row.Cells[6] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell panelPreferredLength = row.Cells[7] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell panelMaxLength = row.Cells[8] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell panelHeightOffset = row.Cells[9] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell horizontalPanelDirection = row.Cells[10] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell verticalPanelDirection = row.Cells[11] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell HourRate = row.Cells[12] as DataGridViewTextBoxCell;

                if (string.IsNullOrEmpty(panelType.Value?.ToString()))
                {
                    break; // This is the last row 
                }

                strProjectSettings += (checkBoxCell.Value != null)? checkBoxCell.Value.ToString() : "False";
                strProjectSettings += ";";

                strProjectSettings += panelType.Value?.ToString();
                strProjectSettings += ";";

                strProjectSettings += panelGauge.Value?.ToString();
                strProjectSettings += ";";

                strProjectSettings += panelClearance.Value?.ToString();
                strProjectSettings += ";";

                strProjectSettings += panelMaxLap.Value?.ToString();
                strProjectSettings += ";";

                strProjectSettings += panelMinLap.Value?.ToString();
                strProjectSettings += ";";

                strProjectSettings += panelOrientation.Value?.ToString();
                strProjectSettings += ";";

                strProjectSettings += panelPreferredLength.Value?.ToString();
                strProjectSettings += ";";

                strProjectSettings += panelMaxLength.Value?.ToString();
                strProjectSettings += ";";

                strProjectSettings += panelHeightOffset.Value?.ToString();
                strProjectSettings += ";";

                strProjectSettings += horizontalPanelDirection.Value?.ToString();
                strProjectSettings += ";";

                strProjectSettings += verticalPanelDirection.Value?.ToString();
                strProjectSettings += ";";

                strProjectSettings += HourRate.Value?.ToString();
                strProjectSettings += ";";
            }
            
            strProjectSettings += "|";

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

            // Panel Direction Computation
            strProjectSettings += comboBox2.SelectedIndex.ToString();
            strProjectSettings += "|";

            if (!bFoundUNO)
            {
                strProjectSettings += -1;
                strProjectSettings += "|";
            }

            // Panel At Hallway
            strProjectSettings += comboBox3.SelectedIndex.ToString();
            strProjectSettings += "|";

            // Partition stud type
            strProjectSettings += textBox4.Text.ToString();
            strProjectSettings += "|";

            // Hallway Panel Thickness
            strProjectSettings += textBox5.Text.ToString();
            strProjectSettings += "|";

            // Panel Strategy
            strProjectSettings += comboBox8.SelectedIndex.ToString();
            strProjectSettings += "|";

            //Deck Span
            strProjectSettings += textBox11.Text.ToString();
            strProjectSettings += "|";

            // Drag Struct Max length
            strProjectSettings += textBox12.Text.ToString();
            strProjectSettings += "|";

            //Drag Struct Type
            strProjectSettings += comboBox4.SelectedIndex.ToString();
            strProjectSettings += "|";

            //Drag Struct continuous at hallway
            strProjectSettings += comboBox7.SelectedIndex.ToString();
            strProjectSettings += "|";

            // Eave struct max length
            strProjectSettings += textBox13.Text.ToString();
            strProjectSettings += "|";

            //Eave Struct type
            strProjectSettings += comboBox4.SelectedIndex.ToString();
            strProjectSettings += "|";

            //Eave Struct location
            strProjectSettings += comboBox6.SelectedIndex.ToString();
            strProjectSettings += "|";

            string StrCeeHeaderSettings = "";
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                DataGridViewCheckBoxCell checkBoxCell = row.Cells[0] as DataGridViewCheckBoxCell;
                DataGridViewTextBoxCell GridCell = row.Cells[1] as DataGridViewTextBoxCell;
                DataGridViewComboBoxCell ceeHeader = row.Cells[2] as DataGridViewComboBoxCell;
                DataGridViewComboBoxCell NoOfCeeHeader = row.Cells[3] as DataGridViewComboBoxCell;
                DataGridViewComboBoxCell HallwayCeeHeader = row.Cells[4] as DataGridViewComboBoxCell;
                DataGridViewComboBoxCell HallwayCeeCount = row.Cells[5] as DataGridViewComboBoxCell;

                if (checkBoxCell == null || checkBoxCell.Value == null ) 
                {
                    break; // This is the last row 
                }
                StrCeeHeaderSettings += (checkBoxCell.Value != null) ? checkBoxCell.Value.ToString() : "False";
                StrCeeHeaderSettings += ";";

                StrCeeHeaderSettings += GridCell.Value?.ToString();
                StrCeeHeaderSettings+= ";";

                StrCeeHeaderSettings += ceeHeader.Value?.ToString();
                StrCeeHeaderSettings += ";";

                StrCeeHeaderSettings += NoOfCeeHeader.Value?.ToString();
                StrCeeHeaderSettings += ";";

                StrCeeHeaderSettings += HallwayCeeHeader.Value?.ToString();
                StrCeeHeaderSettings += ";";

                StrCeeHeaderSettings += HallwayCeeCount.Value?.ToString();
                StrCeeHeaderSettings += ";";

            }

            strProjectSettings += StrCeeHeaderSettings;
            strProjectSettings += "|";

            strProjectSettings += comboBox9.SelectedIndex.ToString();
            strProjectSettings += "|";
            
            strProjectSettings += comboBox10.SelectedIndex.ToString();
            strProjectSettings += "|";
            
            strProjectSettings += comboBox11.SelectedIndex.ToString();
            strProjectSettings += "|";
            
            strProjectSettings += comboBox12.SelectedIndex.ToString();
            strProjectSettings += "|";

            strProjectSettings += textBox14.Text.ToString();
            strProjectSettings += "|";

            strProjectSettings += textBox15.Text.ToString();
            strProjectSettings += "|";

            strProjectSettings += textBox16.Text.ToString();
            strProjectSettings += "|";

            strProjectSettings += textBox17.Text.ToString();
            strProjectSettings += "|";

            strProjectSettings += textBox18.Text.ToString();
            strProjectSettings += "|";

            strProjectSettings += textBox22.Text.ToString();
            strProjectSettings += "|";

            strProjectSettings += textBox21.Text.ToString();
            strProjectSettings += "|";

            strProjectSettings += textBox20.Text.ToString();
            strProjectSettings += "|";

            strProjectSettings += textBox19.Text.ToString();
            strProjectSettings += "|";

            InputLineUtility.SetProjectSettings(strProjectSettings);
            GlobalSettings.UpdateSettings();

        }

        /// <summary>
        /// This method populates the form with default values
        /// </summary>
        private void PopulateDefaultSettings()
        {
            // Fill empty data
            foreach (string strWallType in InputLineUtility.wallTypes)
            {
                if (strWallType == "" || strWallType == " ")
                    continue;
                dataGridView1.Rows.Add(false, strWallType, "", "", "", "", "", "","", "", " ", " ","");
            }
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {

        }

        private void label32_Click(object sender, EventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }
    }
}

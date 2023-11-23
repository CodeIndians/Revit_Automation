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
    public partial class ProjectPropertiesDG : System.Windows.Forms.Form
    {
        private DataTable dataTable;
        private DataGridViewCheckBoxColumn radioButtonColumn;
        private Document m_doc;

        public static int iUNONumber;
        public ProjectPropertiesDG()
        {

            InitializeComponent();

            PanelSettingsDG.CellContentClick += dataGridView_CellContentClick;

            // Roof Deck Combos 
            List<string> strRoofDeckTypes = SymbolCollector.GetRoofDeckTypes();
            foreach (string type in strRoofDeckTypes)
                comboBox4.Items.Add(type);

            // Composite Deck Combos.
            List<string> strCompositeDeckTypes = SymbolCollector.GetCompositeDeckTypes();
            foreach (string type in strCompositeDeckTypes)
                comboBox5.Items.Add(type);

            string strProjectSettings = InputLineUtility.GetProjectSettings();

            // Populate the settings from the Line or else default settings
            if (string.IsNullOrEmpty(strProjectSettings))
            {
                PopulateDefaultSettings();
                PopulateDefaultCHeaderSettings();
                PopulateDefaultPurlinSettings();
                PopulateDefaultDragStrutSettings();
                PopulateDefaultEaveStrutSettings();
            }
            else
                PopulateSettingsFromString(strProjectSettings);

        }

        private void PopulateDefaultEaveStrutSettings()
        {
            // Clear the contents before inserting new row s
            EaveStrutSettingsDG.Rows.Clear();

            foreach (string roofName in RoofUtility.NamedRoofs)
            {
                DataGridViewRow dgvRow = new DataGridViewRow();

                DataGridViewCell roofNameCell = new DataGridViewTextBoxCell();
                roofNameCell.Value = roofName;

                List<string> eaveStrutList = SymbolCollector.GetEaveStrutTypes();
                DataGridViewComboBoxCell dragStrutTypeCell = new DataGridViewComboBoxCell();
                foreach (string ds in eaveStrutList)
                    dragStrutTypeCell.Items.Add(ds);

                DataGridViewComboBoxCell locationCell = new DataGridViewComboBoxCell();
                locationCell.Items.Add("Low Eave");
                locationCell.Items.Add("High Eave");
                locationCell.Items.Add("Both");

                DataGridViewCell maxLengthCell = new DataGridViewTextBoxCell();

                DataGridViewCell LapCell = new DataGridViewTextBoxCell();

                dgvRow.Cells.Add(roofNameCell);
                dgvRow.Cells.Add(dragStrutTypeCell);
                dgvRow.Cells.Add(locationCell);
                dgvRow.Cells.Add(maxLengthCell);
                dgvRow.Cells.Add(LapCell);

                EaveStrutSettingsDG.Rows.Add(dgvRow);
            }
        }

        private void PopulateDefaultDragStrutSettings()
        {
            DragStrutSettingsDG.Rows.Clear();

            foreach (string roofName in RoofUtility.NamedRoofs)
            {
                DataGridViewRow dgvRow = new DataGridViewRow();

                DataGridViewCell roofNameCell = new DataGridViewTextBoxCell();
                roofNameCell.Value = roofName;

                List<string> dragStrutList = SymbolCollector.GetDragStrutTypes();
                DataGridViewComboBoxCell eaveStrutTypeCell = new DataGridViewComboBoxCell();
                foreach (string ds in dragStrutList)
                    eaveStrutTypeCell.Items.Add(ds);

                DataGridViewComboBoxCell atHallwayCell = new DataGridViewComboBoxCell();
                atHallwayCell.Items.Add("yes");
                atHallwayCell.Items.Add("no");

                DataGridViewCell maxLengthCell = new DataGridViewTextBoxCell();

                DataGridViewCell LapCell = new DataGridViewTextBoxCell();

                dgvRow.Cells.Add(roofNameCell);
                dgvRow.Cells.Add(eaveStrutTypeCell);
                dgvRow.Cells.Add(atHallwayCell);
                dgvRow.Cells.Add(maxLengthCell);
                dgvRow.Cells.Add(LapCell);

                DragStrutSettingsDG.Rows.Add(dgvRow);
            }
        }

        private void PopulateDefaultPurlinSettings()
        {
            PurlinSettingsDG.Rows.Clear();

            foreach (string roofName in RoofUtility.NamedRoofs)
            {
                DataGridViewRow dgvRow = new DataGridViewRow();

                DataGridViewCell roofNameCell = new DataGridViewTextBoxCell();
                roofNameCell.Value = roofName;

                List<string> purlinList = SymbolCollector.GetPurlinSymbols();
                DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();
                foreach (string purlin in purlinList)
                    cell.Items.Add(purlin);

                DataGridViewCell guageCell = new DataGridViewTextBoxCell();

                DataGridViewCell onCenterCell = new DataGridViewTextBoxCell();

                DataGridViewCell maxSpansCell = new DataGridViewTextBoxCell();

                DataGridViewCell thicknessCell = new DataGridViewTextBoxCell();

                List<string> receiverChannelSymbols = SymbolCollector.GetReceiverChannelSymbols();
                DataGridViewComboBoxCell receiverChannelCel = new DataGridViewComboBoxCell();
                foreach (string rc in receiverChannelSymbols)
                    receiverChannelCel.Items.Add(rc);

                DataGridViewCell receiverChannelGauge = new DataGridViewTextBoxCell();

                DataGridViewComboBoxCell orientationChangeCell = new DataGridViewComboBoxCell();
                orientationChangeCell.Items.Add("yes");
                orientationChangeCell.Items.Add("no");

                dgvRow.Cells.Add(roofNameCell);
                dgvRow.Cells.Add(cell);
                dgvRow.Cells.Add(guageCell);
                dgvRow.Cells.Add(onCenterCell);
                dgvRow.Cells.Add(maxSpansCell);
                dgvRow.Cells.Add(thicknessCell);
                dgvRow.Cells.Add(receiverChannelCel);
                dgvRow.Cells.Add(receiverChannelGauge);
                dgvRow.Cells.Add(orientationChangeCell);

                PurlinSettingsDG.Rows.Add(dgvRow);
            }
        }

        private void PopulateDefaultCHeaderSettings()
        {
            CeeHeaderSettingsDG.Rows.Clear();
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

                CeeHeaderSettingsDG.Rows.Add(row);
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

                    PanelSettingsDG.Rows.Add(row);
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

                string StrCeeHeaderSettings = settings[17].ToString();
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

                    CeeHeaderSettingsDG.Rows.Add(dgvRow);
                }

                //Roof Perpendicular NLB splice
                comboBox9.SelectedIndex = int.Parse(settings[18]);

                // Round off
                comboBox10.SelectedIndex = int.Parse(settings[19]);
                
                // Top track at rake side
                comboBox11.SelectedIndex = int.Parse(settings[20]);
                
                // Top track splice at web
                comboBox12.SelectedIndex = int.Parse(settings[21]);


                // Cee Header Max length
                textBox14.Text = settings[22].ToString();

                // Floor Deck Type
                comboBox5.Text = settings[23];

                // Floor Deck overlap
                textBox16.Text = settings[24].ToString();

                // Floor Deck max span
                textBox17.Text = settings[25].ToString();

                // Floor Deck Max length
                textBox18.Text = settings[26].ToString();

                // Roof Deck Type
                comboBox4.SelectedIndex = int.Parse(settings[27]);

                // Roof Deck overlap
                textBox21.Text = settings[28].ToString();

                // Roof Deck max span
                textBox20.Text = settings[29].ToString();

                // Roof Deck Max length
                textBox19.Text = settings[30].ToString();

                if (settings.Length > 30)
                {
                    // Purlin Lap
                    textBox23.Text = settings[31].ToString();

                    // Purlin Preferred Length
                    textBox24.Text = settings[32].ToString();

                    //Purlin Continuous at Insulation
                    comboBox13.SelectedIndex = int.Parse(settings[33]);

                    // Purlin Round Off
                    comboBox16.SelectedIndex = int.Parse(settings[34]);

                    // Purlin Size, gauge and OnCenter 
                    string strPurlinSettings = settings[35];
                    string[] purlinSettings = strPurlinSettings.Split(';');

                    if (string.IsNullOrEmpty(strPurlinSettings))
                        PopulateDefaultPurlinSettings();

                    int kCounter = 0;

                    while (kCounter < purlinSettings.Length - 1)
                    {
                        if (string.IsNullOrEmpty(purlinSettings[kCounter]))
                            break;

                        DataGridViewRow dgvRow = new DataGridViewRow();

                        DataGridViewCell roofNameCell = new DataGridViewTextBoxCell();
                        roofNameCell.Value = purlinSettings[kCounter++];

                        List<string> purlinList = SymbolCollector.GetPurlinSymbols();
                        DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();
                        foreach (string purlin in purlinList)
                            cell.Items.Add(purlin);
                        cell.Value = purlinSettings[kCounter++];

                        DataGridViewCell guageCell = new DataGridViewTextBoxCell();
                        guageCell.Value = purlinSettings[kCounter++];

                        DataGridViewCell onCenterCell = new DataGridViewTextBoxCell();
                        onCenterCell.Value = purlinSettings[kCounter++];

                        DataGridViewCell maxSpansCell = new DataGridViewTextBoxCell();
                        maxSpansCell.Value = purlinSettings[kCounter++];

                        DataGridViewCell thicknessCell = new DataGridViewTextBoxCell();
                        thicknessCell.Value = purlinSettings[kCounter++];

                        List<string> receiverChannelSymbols = SymbolCollector.GetReceiverChannelSymbols();
                        DataGridViewComboBoxCell receiverChannelCel = new DataGridViewComboBoxCell();
                        foreach (string rc in receiverChannelSymbols)
                            receiverChannelCel.Items.Add(rc);
                        receiverChannelCel.Value = purlinSettings[kCounter++];

                        DataGridViewCell receiverChannelGauge = new DataGridViewTextBoxCell();
                        receiverChannelGauge.Value = purlinSettings[kCounter++];

                        DataGridViewComboBoxCell orientationChangeCell = new DataGridViewComboBoxCell();
                        orientationChangeCell.Items.Add("yes");
                        orientationChangeCell.Items.Add("no");
                        orientationChangeCell.Value = purlinSettings[kCounter++];


                        dgvRow.Cells.Add(roofNameCell);
                        dgvRow.Cells.Add(cell);
                        dgvRow.Cells.Add(guageCell);
                        dgvRow.Cells.Add(onCenterCell);
                        dgvRow.Cells.Add(maxSpansCell);
                        dgvRow.Cells.Add(thicknessCell);
                        dgvRow.Cells.Add(receiverChannelCel);
                        dgvRow.Cells.Add(receiverChannelGauge);
                        dgvRow.Cells.Add(orientationChangeCell);

                        PurlinSettingsDG.Rows.Add(dgvRow);
                    }

                    // Drag Strut Settings
                    string strDragStrutSettings = settings[36];
                    string[] strDragStrutSettingsList = strDragStrutSettings.Split(';');

                    if (string.IsNullOrEmpty(strDragStrutSettings))
                        PopulateDefaultDragStrutSettings();

                    int lCounter = 0;

                    while (lCounter < strDragStrutSettingsList.Length - 1)
                    {
                        // Empty row condition
                        if (string.IsNullOrEmpty(strDragStrutSettingsList[lCounter]))
                            break;

                        DataGridViewRow dgvRow = new DataGridViewRow();

                        DataGridViewCell roofNameCell = new DataGridViewTextBoxCell();
                        roofNameCell.Value = strDragStrutSettingsList[lCounter++];

                        List<string> dragStrutList = SymbolCollector.GetDragStrutTypes();
                        DataGridViewComboBoxCell dragStrutTypeCell = new DataGridViewComboBoxCell();
                        foreach (string ds in dragStrutList)
                            dragStrutTypeCell.Items.Add(ds);


                        dragStrutTypeCell.Value = strDragStrutSettingsList[lCounter++];

                        DataGridViewComboBoxCell atHallwayCell = new DataGridViewComboBoxCell();
                        atHallwayCell.Items.Add("yes");
                        atHallwayCell.Items.Add("no");
                        atHallwayCell.Items.Add("");
                        atHallwayCell.Value = strDragStrutSettingsList[lCounter++];

                        DataGridViewCell maxLengthCell = new DataGridViewTextBoxCell();
                        maxLengthCell.Value = strDragStrutSettingsList[lCounter++];

                        DataGridViewCell LapCell = new DataGridViewTextBoxCell();
                        LapCell.Value = strDragStrutSettingsList[lCounter++];

                        dgvRow.Cells.Add(roofNameCell);
                        dgvRow.Cells.Add(dragStrutTypeCell);
                        dgvRow.Cells.Add(atHallwayCell);
                        dgvRow.Cells.Add(maxLengthCell);
                        dgvRow.Cells.Add(LapCell);

                        DragStrutSettingsDG.Rows.Add(dgvRow);
                    }

                    // Eave Strut Settings
                    string strEaveStrutSettings = settings[37];
                    string[] strEaveStrutSettingsList = strEaveStrutSettings.Split(';');

                    if (string.IsNullOrEmpty(strEaveStrutSettings))
                        PopulateDefaultEaveStrutSettings();

                    int mCounter = 0;

                    while (mCounter < strEaveStrutSettingsList.Length - 1)
                    {
                        // Empty row condition
                        if (string.IsNullOrEmpty(strEaveStrutSettingsList[mCounter]))
                            break;

                        DataGridViewRow dgvRow = new DataGridViewRow();

                        DataGridViewCell roofNameCell = new DataGridViewTextBoxCell();
                        roofNameCell.Value = strEaveStrutSettingsList[mCounter++];

                        List<string> eaveStrutList = SymbolCollector.GetEaveStrutTypes();
                        DataGridViewComboBoxCell eaveStrutTypeCell = new DataGridViewComboBoxCell();
                        foreach (string ds in eaveStrutList)
                            eaveStrutTypeCell.Items.Add(ds);
                        eaveStrutTypeCell.Value = strEaveStrutSettingsList[mCounter++];

                        DataGridViewComboBoxCell locationCell = new DataGridViewComboBoxCell();
                        locationCell.Items.Add("Low Eave");
                        locationCell.Items.Add("High Eave");
                        locationCell.Items.Add("Both");
                        locationCell.Items.Add("");
                        locationCell.Value = strEaveStrutSettingsList[mCounter++];

                        DataGridViewCell maxLengthCell = new DataGridViewTextBoxCell();
                        maxLengthCell.Value = strEaveStrutSettingsList[mCounter++];

                        DataGridViewCell LapCell = new DataGridViewTextBoxCell();
                        LapCell.Value = strEaveStrutSettingsList[mCounter++];

                        dgvRow.Cells.Add(roofNameCell);
                        dgvRow.Cells.Add(eaveStrutTypeCell);
                        dgvRow.Cells.Add(locationCell);
                        dgvRow.Cells.Add(maxLengthCell);
                        dgvRow.Cells.Add(LapCell);

                        EaveStrutSettingsDG.Rows.Add(dgvRow);
                    }
                }
                else
                {
                    PopulateDefaultPurlinSettings();
                    PopulateDefaultDragStrutSettings();
                    PopulateDefaultEaveStrutSettings();
                }

                PanelSettingsDG.Invalidate();
                PanelSettingsDG.Update();

                CeeHeaderSettingsDG.Invalidate();
                CeeHeaderSettingsDG.Update();

                PurlinSettingsDG.Invalidate();
                PurlinSettingsDG.Update();

                DragStrutSettingsDG.Invalidate();
                DragStrutSettingsDG.Update();

                EaveStrutSettingsDG.Invalidate();
                EaveStrutSettingsDG.Update();
            }
            catch (Exception)
            {
                PopulateDefaultSettings();
                PopulateDefaultCHeaderSettings();
                PopulateDefaultPurlinSettings();
                PopulateDefaultDragStrutSettings();
                PopulateDefaultEaveStrutSettings();
            }
        }

        /// <summary>
        /// To make the checkbnox behave like a radio button, this delegate is used
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == PanelSettingsDG.Columns["UNO"].Index)
            {
                foreach (DataGridViewRow row in PanelSettingsDG.Rows)
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
            string strPanelSettings = "";
            DataTable dt = (DataTable)PanelSettingsDG.DataSource;
            foreach (DataGridViewRow row in PanelSettingsDG.Rows)
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

                strPanelSettings += (checkBoxCell.Value != null)? checkBoxCell.Value.ToString() : "False";
                strPanelSettings += ";";

                strPanelSettings += panelType.Value?.ToString();
                strPanelSettings += ";";

                strPanelSettings += panelGauge.Value?.ToString();
                strPanelSettings += ";";

                strPanelSettings += panelClearance.Value?.ToString();
                strPanelSettings += ";";

                strPanelSettings += panelMaxLap.Value?.ToString();
                strPanelSettings += ";";

                strPanelSettings += panelMinLap.Value?.ToString();
                strPanelSettings += ";";

                strPanelSettings += panelOrientation.Value?.ToString();
                strPanelSettings += ";";

                strPanelSettings += panelPreferredLength.Value?.ToString();
                strPanelSettings += ";";

                strPanelSettings += panelMaxLength.Value?.ToString();
                strPanelSettings += ";";

                strPanelSettings += panelHeightOffset.Value?.ToString();
                strPanelSettings += ";";

                strPanelSettings += horizontalPanelDirection.Value?.ToString();
                strPanelSettings += ";";

                strPanelSettings += verticalPanelDirection.Value?.ToString();
                strPanelSettings += ";";

                strPanelSettings += HourRate.Value?.ToString();
                strPanelSettings += ";";
            }

            // Settings[0]
            strProjectSettings += strPanelSettings;
            strProjectSettings += "|";

            // Building Type // Settings[1]
            strProjectSettings += comboBox1.SelectedIndex.ToString() ;
            strProjectSettings += "|";

            // Bottom Track Preferred length
            // Settings[2], settings[3]
            strProjectSettings += textBox1.Text.ToString() + "|" + textBox6.Text.ToString();
            strProjectSettings += "|";

            //top track preferred length
            // Settings[4], settings [5]
            strProjectSettings += textBox3.Text.ToString() + "|" + textBox2.Text.ToString();
            strProjectSettings += "|";

            // Bottom Track max length
            // Settings[6], Settings[7]
            strProjectSettings += textBox8.Text.ToString() + "|" + textBox7.Text.ToString();
            strProjectSettings += "|";

            //Top Track max length
            // Settings[8], Settings[9]
            strProjectSettings += textBox10.Text.ToString() + "|" + textBox9.Text.ToString();
            strProjectSettings += "|";

            // Panel Direction Computation
            // Settings[10]
            strProjectSettings += comboBox2.SelectedIndex.ToString();
            strProjectSettings += "|";

            // Settings[11]
            if (!bFoundUNO)
            {
                strProjectSettings += -1;
                strProjectSettings += "|";
            }

            // Panel At Hallway
            // Settings[12]
            strProjectSettings += comboBox3.SelectedIndex.ToString();
            strProjectSettings += "|";

            // Partition stud type
            // Settings[13]
            strProjectSettings += textBox4.Text.ToString();
            strProjectSettings += "|";

            // Hallway Panel Thickness
            // Settings[14]
            strProjectSettings += textBox5.Text.ToString();
            strProjectSettings += "|";

            // Panel Strategy
            // Settings[15]
            strProjectSettings += comboBox8.SelectedIndex.ToString();
            strProjectSettings += "|";

            //Deck Span
            // Settings[16]
            strProjectSettings += textBox11.Text.ToString();
            strProjectSettings += "|";

           

            string StrCeeHeaderSettings = "";
            foreach (DataGridViewRow row in CeeHeaderSettingsDG.Rows)
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

            //Settings[17]
            strProjectSettings += StrCeeHeaderSettings;
            strProjectSettings += "|";

            //Settings[18] - //Roof Perpendicular NLB splice
            strProjectSettings += comboBox9.SelectedIndex.ToString();
            strProjectSettings += "|";
            
            // Settings [19] - Top Track Round Off
            strProjectSettings += comboBox10.SelectedIndex.ToString();
            strProjectSettings += "|";
            
            // Settings[20] - Top Track at rake side
            strProjectSettings += comboBox11.SelectedIndex.ToString();
            strProjectSettings += "|";
            
            //Settings[21] - Top track Splice at Web
            strProjectSettings += comboBox12.SelectedIndex.ToString();
            strProjectSettings += "|";

            //Settings[22] - Cee Header max length
            strProjectSettings += textBox14.Text.ToString();
            strProjectSettings += "|";

            // Settings [23] - Floor Deck Type
            strProjectSettings += comboBox5.Text.ToString();
            strProjectSettings += "|";

            //Settings [24] - Floor Deck Overlap
            strProjectSettings += textBox16.Text.ToString();
            strProjectSettings += "|";

            //Settings [25] - Floor Deck Max Span
            strProjectSettings += textBox17.Text.ToString();
            strProjectSettings += "|";

            //Settings [26] - Floor Deck Max Length
            strProjectSettings += textBox18.Text.ToString();
            strProjectSettings += "|";

            // Settings [27] - Roof Deck type
            strProjectSettings += comboBox4.SelectedIndex.ToString();
            strProjectSettings += "|";

            //Settings [28] - Roof  Deck Overlap
            strProjectSettings += textBox21.Text.ToString();
            strProjectSettings += "|";

            //Settings [29] - Roof Deck Max Span
            strProjectSettings += textBox20.Text.ToString();
            strProjectSettings += "|";

            //Settings [30] - Roof Deck Max Length
            strProjectSettings += textBox19.Text.ToString();
            strProjectSettings += "|";

            // Purlin related settings
            
            // Settings [31] - Purlin Lap 
            strProjectSettings += textBox23.Text.ToString();
            strProjectSettings += "|";

            // Settings [32] Purlin Preferred Length
            strProjectSettings += textBox24.Text.ToString();
            strProjectSettings += "|";

            //Settings [33] - Purlin Continuous at Insulation
            strProjectSettings += comboBox13.SelectedIndex.ToString();
            strProjectSettings += "|";

            // Settings[34] - Purlin Round Off
            strProjectSettings += comboBox16.SelectedIndex.ToString();
            strProjectSettings += "|";

            // Purlin Size, gauge and OnCenter - Purlin Settings Roof Wise
            string strPurlinSettings = "";
            foreach (DataGridViewRow dataGridViewRow in PurlinSettingsDG.Rows)
            {

                DataGridViewTextBoxCell roofCell = dataGridViewRow.Cells[0] as DataGridViewTextBoxCell;
                DataGridViewComboBoxCell purlinTypeCell = dataGridViewRow.Cells[1] as DataGridViewComboBoxCell;
                DataGridViewTextBoxCell purlinGaugeCell = dataGridViewRow.Cells[2] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell onCenterCell = dataGridViewRow.Cells[3] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell maxSpanCell = dataGridViewRow.Cells[4] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell thicknessCell = dataGridViewRow.Cells[5] as DataGridViewTextBoxCell;
                DataGridViewComboBoxCell receiverChannelCell = dataGridViewRow.Cells[6] as DataGridViewComboBoxCell;
                DataGridViewTextBoxCell receiverChannelGaugeCell = dataGridViewRow.Cells[7] as DataGridViewTextBoxCell;
                DataGridViewComboBoxCell OrientationChangeCell = dataGridViewRow.Cells[8] as DataGridViewComboBoxCell;

                strPurlinSettings += roofCell.Value?.ToString();
                strPurlinSettings += ";";

                strPurlinSettings += purlinTypeCell.Value?.ToString();
                strPurlinSettings += ";";

                strPurlinSettings += purlinGaugeCell.Value?.ToString();
                strPurlinSettings += ";";

                strPurlinSettings += onCenterCell.Value?.ToString();
                strPurlinSettings += ";";

                strPurlinSettings += maxSpanCell.Value?.ToString();
                strPurlinSettings += ";";

                strPurlinSettings += thicknessCell.Value?.ToString();
                strPurlinSettings += ";";

                strPurlinSettings += receiverChannelCell.Value?.ToString();
                strPurlinSettings += ";";

                strPurlinSettings += receiverChannelGaugeCell.Value?.ToString();
                strPurlinSettings += ";";

                strPurlinSettings += OrientationChangeCell.Value?.ToString();
                strPurlinSettings += ";";


            }

            // settings [35] Purlin Settings Roof Wise
            strProjectSettings += strPurlinSettings;
            strProjectSettings += "|";

            string strDragStrutSettings = "";
            foreach (DataGridViewRow dataGridViewRow in DragStrutSettingsDG.Rows)
            {

                DataGridViewTextBoxCell roofCell = dataGridViewRow.Cells[0] as DataGridViewTextBoxCell;
                DataGridViewComboBoxCell dragStrutTypeCell = dataGridViewRow.Cells[1] as DataGridViewComboBoxCell;
                DataGridViewComboBoxCell ContinuousAthallwayCell = dataGridViewRow.Cells[2] as DataGridViewComboBoxCell;
                DataGridViewTextBoxCell MaxLengthCell = dataGridViewRow.Cells[3] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell LapCell = dataGridViewRow.Cells[4] as DataGridViewTextBoxCell;

                strDragStrutSettings += roofCell.Value?.ToString();
                strDragStrutSettings += ";";

                strDragStrutSettings += dragStrutTypeCell.Value?.ToString();
                strDragStrutSettings += ";";

                strDragStrutSettings += ContinuousAthallwayCell.Value?.ToString();
                strDragStrutSettings += ";";

                strDragStrutSettings += MaxLengthCell.Value?.ToString();
                strDragStrutSettings += ";";

                strDragStrutSettings += LapCell.Value?.ToString();
                strDragStrutSettings += ";";
            }

            // settings [36] Drag Strut Settings
            strProjectSettings += strDragStrutSettings;                ;
            strProjectSettings += "|";

            string strEaveStrutSettings = "";
            foreach (DataGridViewRow dataGridViewRow in EaveStrutSettingsDG.Rows)
            {

                DataGridViewTextBoxCell roofCell = dataGridViewRow.Cells[0] as DataGridViewTextBoxCell;
                DataGridViewComboBoxCell eaveStrutTypeCell = dataGridViewRow.Cells[1] as DataGridViewComboBoxCell;
                DataGridViewComboBoxCell locationCell = dataGridViewRow.Cells[2] as DataGridViewComboBoxCell;
                DataGridViewTextBoxCell MaxLengthCell = dataGridViewRow.Cells[3] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell LapCell = dataGridViewRow.Cells[4] as DataGridViewTextBoxCell;

                strEaveStrutSettings += roofCell.Value?.ToString();
                strEaveStrutSettings += ";";

                strEaveStrutSettings += eaveStrutTypeCell.Value?.ToString();
                strEaveStrutSettings += ";";

                strEaveStrutSettings += locationCell.Value?.ToString();
                strEaveStrutSettings += ";";

                strEaveStrutSettings += MaxLengthCell.Value?.ToString();
                strEaveStrutSettings += ";";

                strEaveStrutSettings += LapCell.Value?.ToString();
                strEaveStrutSettings += ";";
            }

            // settings [37] Eave Strut Settings
            strProjectSettings += strEaveStrutSettings; ;
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
                PanelSettingsDG.Rows.Add(false, strWallType, "", "", "", "", "", "","", "", " ", " ","");
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

        private void label43_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}

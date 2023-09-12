using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using Revit_Automation.Source.ModelCreators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace Revit_Automation.Dialogs
{
    public partial class CeeHeaderAdjustmentsForm : Form
    {
        private Document m_Document;
        public CeeHeaderAdjustmentsForm(Autodesk.Revit.DB.Document doc)
        {
            m_Document = doc;
            InitializeComponent();
        }

        internal void AdjustHeaders()
        {
            using (Transaction tx = new Transaction(m_Document))
            {
                tx.Start("Adjusting Cee Headers");
                List<CeeHeaderAdjustments> lst = GatherSettings();
                CeeHeaderAdjustment ceeHeaderAdjustment = new CeeHeaderAdjustment(m_Document, lst);
                ceeHeaderAdjustment.AdjustHeaders();
                tx.Commit();
            }
        }

        internal List<CeeHeaderAdjustments> GatherSettings()
        {
            // Get the settings from the form
            List<CeeHeaderAdjustments> lstCeeHeaderAdjustments = new List<CeeHeaderAdjustments>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                CeeHeaderAdjustments ceeHeaderAdjustments = new CeeHeaderAdjustments();
                DataGridViewTextBoxCell ceeHeaderName = row.Cells[0] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell ceeHeaderCount = row.Cells[1] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell PostType = row.Cells[2] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell PostGuage = row.Cells[3] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell PostCount = row.Cells[4] as DataGridViewTextBoxCell;
                DataGridViewComboBoxCell bChangeOrientation = row.Cells[5] as DataGridViewComboBoxCell;

                //Empty row condition;
                if (string.IsNullOrEmpty(ceeHeaderName.Value?.ToString()))
                    break;

                ceeHeaderAdjustments.strCeeHeaderName = ceeHeaderName.Value.ToString();
                ceeHeaderAdjustments.iCeeHeaderCount = int.Parse(ceeHeaderCount.Value.ToString());
                ceeHeaderAdjustments.postType = PostType.Value.ToString();
                ceeHeaderAdjustments.postGuage = PostGuage.Value.ToString();
                ceeHeaderAdjustments.postCount = string.IsNullOrEmpty (PostCount.Value.ToString()) ? 0 : int.Parse(PostCount.Value.ToString());
                ceeHeaderAdjustments.bChangeOrientation = bChangeOrientation.Value?.ToString() == "Yes" ? true : false;

                if (!string.IsNullOrEmpty(ceeHeaderAdjustments.postGuage) && !string.IsNullOrEmpty(ceeHeaderAdjustments.postType))
                    lstCeeHeaderAdjustments.Add(ceeHeaderAdjustments);
            }
            return lstCeeHeaderAdjustments;
        }
        internal void PopulateData()
        {
            FilteredElementCollector framingElements
              = new FilteredElementCollector(m_Document, m_Document.ActiveView.Id)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_StructuralFraming);

            HashSet<string> headers = new HashSet<string>();

            foreach (Element element in framingElements)
            {
                string familyName = string.Empty;
                string symbolName = string.Empty;

                if (element is FamilyInstance familyInstance)
                {
                    familyName = familyInstance.Symbol.Family.Name;
                    if (!string.IsNullOrEmpty(familyName) && familyName == "Cee Header")
                        headers.Add(familyInstance.Symbol.Name);
                     
                }
            }

            foreach (string headername in headers)
            {
                for (int i = 0; i < 2; i++)
                {
                    DataGridViewRow row = new DataGridViewRow();

                    DataGridViewCell CeeHeaderName = new DataGridViewTextBoxCell();
                    CeeHeaderName.Value = headername;

                    DataGridViewCell CeeHeaderQty = new DataGridViewTextBoxCell();
                    CeeHeaderQty.Value = i == 0 ? "1" : "2";

                    DataGridViewCell PostName = new DataGridViewTextBoxCell();
                    PostName.Value = "";

                    DataGridViewCell Postgauge = new DataGridViewTextBoxCell();
                    Postgauge.Value = "";

                    DataGridViewCell PostQty = new DataGridViewTextBoxCell();
                    PostQty.Value = "";

                    DataGridViewComboBoxCell ChangeOrientation = new DataGridViewComboBoxCell();
                    ChangeOrientation.Items.Add("Yes");
                    ChangeOrientation.Items.Add("No");

                    row.Cells.Add(CeeHeaderName);
                    row.Cells.Add(CeeHeaderQty);
                    row.Cells.Add(PostName);
                    row.Cells.Add(Postgauge);
                    row.Cells.Add(PostQty);
                    row.Cells.Add(ChangeOrientation);



                    dataGridView1.Rows.Add(row);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = "Adjusting...";
            this.AdjustHeaders();
            button1.Text = "Finished";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

using Autodesk.Revit.DB;
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
                DataGridViewRow row = new DataGridViewRow();

                DataGridViewCell CeeHeaderName = new DataGridViewTextBoxCell();
                CeeHeaderName.Value = headername;

                DataGridViewCell CeeHeaderQty = new DataGridViewTextBoxCell();
                CeeHeaderQty.Value = "";

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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Revit_Automation.Dialogs
{
    public partial class CeeHeaderModeSelector : Form
    {
        public bool m_bCreation = true;
    
        public CeeHeaderModeSelector()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_bCreation = true;
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_bCreation = false;
            this.Hide();
        }
    }
}

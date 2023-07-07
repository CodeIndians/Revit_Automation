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
    public partial class LineProcessing : Form
    {
        public LineProcessing()
        {
            InitializeComponent();
        }

        public void LineExtendingMessage(string strMessage, int messageCode)
        {
            Refresh();

            richTextBox1.SelectionColor = GetColorByMessageCode(messageCode);

            richTextBox1.AppendText(strMessage);

            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private Color GetColorByMessageCode(int messageCode)
        {
            if (messageCode == 0)
                return Color.DarkCyan;
            if (messageCode == 1)
                return Color.Green;
            if (messageCode == 2)
                return Color.Magenta;
            else
                return Color.Orange;
        }

        public void LineTrimmingMessage(string strMessage, int messageCode)
        {
            Refresh();

            richTextBox2.SelectionColor = GetColorByMessageCode(messageCode);

            richTextBox2.AppendText(strMessage);

            richTextBox2.SelectionStart = richTextBox2.Text.Length;
            richTextBox2.ScrollToCaret();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

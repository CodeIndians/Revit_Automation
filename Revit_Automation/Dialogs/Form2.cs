using System;
using System.Net;
using System.Windows.Forms;

namespace Revit_Automation.Dialogs
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            ValidLicense = false;
            InitializeComponent();
            string hostName = Dns.GetHostName();

            textBox1.Text = Dns.GetHostEntry(hostName).AddressList[1].ToString();
            textBox3.Text = hostName;
        }

        public bool ValidLicense { get; internal set; }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Close")
            {
                Close();
            }

            if (richTextBox1.Text == "localhost")
            {
                label4.Visible = true;
                pictureBox1.Visible = true;
                ValidLicense = true;
            }
            else
            {
                label4.Text = "Couldn't lease a license";
                pictureBox1.Image = global::Revit_Automation.Properties.Resources.delete_button;
                label4.Visible = true;
                pictureBox1.Visible = true;
                ValidLicense = false;
            }
            button1.Text = "Close";

        }
    }
}

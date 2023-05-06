using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace Revit_Automation.Dialogs
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            this.ValidLicense = false;
            InitializeComponent();
            string hostName = Dns.GetHostName();

            this.textBox1.Text = Dns.GetHostEntry(hostName).AddressList[1].ToString(); 
        }

        public bool ValidLicense { get; internal set; }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.button1.Text == "Close")
                this.Close();

            if (this.richTextBox1.Text == "localhost")
            {
                this.label4.Visible = true;
                this.pictureBox1.Visible = true;
                this.ValidLicense = true;
            }
            else
            {
                this.label4.Text = "Couldn't lease a license";
                this.pictureBox1.Image = global::Revit_Automation.Properties.Resources.delete_button;
                this.label4.Visible = true;
                this.pictureBox1.Visible = true;
                this.ValidLicense = false;
            }
            this.button1.Text = "Close";
            
        }
    }
}

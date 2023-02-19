using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Revit_Automation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.progressBar1.Value= 0;
            this.richTextBox1.Text = "Beginning Creation of Model";
            this.button1.Text = "START";
        }

        public bool CanCreateModel { get; internal set; }

        public int ProgressMax { get; internal set; }
        public void PostMessage(string message)
        {
            this.richTextBox1.Text += message;
        }

        public void UpdateProgress(int iProgress)
        {
            this.progressBar1.Value += iProgress;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
            CanCreateModel=true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        internal void UpdateCompleted()
        {
            this.progressBar1.Value = 100;
            this.button1.Text = "Finish";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filePath = "C:\\Temp\\Automation_Log.txt"; // Path to the file to be created

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(this.richTextBox1.Text); // Write a string to the file
               
            }

            if (File.Exists(filePath))
            {
                Process.Start(filePath); // Launch the file using the default program
            }
        }
    }
}

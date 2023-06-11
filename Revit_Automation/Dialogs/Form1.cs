using Revit_Automation.Source.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Revit_Automation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            progressBar1.Value = 0;
            richTextBox1.Text = "Beginning Creation of Model";
            button1.Text = "START";
            radioButton1.Checked = true;
            radioButton3.Checked = true;
            checkedListBox1.SetItemChecked(0, true);
        }

        public bool CanCreateModel { get; internal set; }

        public int ProgressMax { get; internal set; }

        public void PostMessage(string message, bool bWarning = false)
        {
            Refresh();

            richTextBox1.SelectionColor = bWarning ? Color.Red : Color.Green;

            richTextBox1.AppendText(message);

            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();

            if (bWarning)
            {
                string substring = "\n";
                substring += message.Substring(34, 8);
                richTextBox2.AppendText(substring);
                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                richTextBox2.ScrollToCaret();
            }
        }

        public void UpdateProgress(int iProgress)
        {
            progressBar1.Value = iProgress;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Close();
            CanCreateModel = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        internal void UpdateCompleted()
        {
            progressBar1.Value = 100;
            button1.Text = "Finish";
        }
        internal void UpdateStarted()
        {
            progressBar1.Value = 0;
            button1.Text = "Generating";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filePath = "C:\\Temp\\Automation_Log.txt"; // Path to the file to be created

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(richTextBox1.Text); // Write a string to the file

            }

            if (File.Exists(filePath))
            {
                _ = Process.Start(filePath); // Launch the file using the default program
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
                Logger.LoggerLevel = 1;
            else
                Logger.LoggerLevel = 0;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
                Logger.LoggerLevel = 0;
            else
                Logger.LoggerLevel = 1;
        }
    }
}

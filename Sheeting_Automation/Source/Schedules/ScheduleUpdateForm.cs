using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sheeting_Automation.Source.Schedules
{
    public partial class ScheduleUpdateForm : Form
    {
        public ScheduleUpdateForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// clicking on the update button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if(ValidateUpdateData())
            {
                var scheduleCreator = new ScheduleCreator();

                scheduleCreator.UpdateMarkersCurrentView(prefixTextBox.Text,startTextBox.Text,suffixTextBox.Text);

                MessageBox.Show("Markers are updated");

                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid Data");
            }
        }

        private bool ValidateUpdateData()
        {
            bool isValid = true;

            if(startTextBox.Text.Length == 0 )
            {
                isValid = false;
            }
            else
            {
                if(!double.TryParse(startTextBox.Text, out double value) )
                    isValid = false;
            }

            return isValid;
        }
    }
}

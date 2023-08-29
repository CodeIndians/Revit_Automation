namespace Sheeting_Automation.Source.Schedules
{
    partial class ScheduleUpdateForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.prefixLabel = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.prefixTextBox = new System.Windows.Forms.TextBox();
            this.startTextBox = new System.Windows.Forms.TextBox();
            this.suffixTextBox = new System.Windows.Forms.TextBox();
            this.startLabel = new System.Windows.Forms.Label();
            this.suffixLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // prefixLabel
            // 
            this.prefixLabel.AutoSize = true;
            this.prefixLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.prefixLabel.Location = new System.Drawing.Point(23, 77);
            this.prefixLabel.Name = "prefixLabel";
            this.prefixLabel.Size = new System.Drawing.Size(67, 25);
            this.prefixLabel.TabIndex = 0;
            this.prefixLabel.Text = "Prefix:";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(264, 159);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(173, 48);
            this.button1.TabIndex = 1;
            this.button1.Text = "Update";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // prefixTextBox
            // 
            this.prefixTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.prefixTextBox.Location = new System.Drawing.Point(96, 72);
            this.prefixTextBox.Name = "prefixTextBox";
            this.prefixTextBox.Size = new System.Drawing.Size(122, 30);
            this.prefixTextBox.TabIndex = 2;
            // 
            // startTextBox
            // 
            this.startTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startTextBox.Location = new System.Drawing.Point(335, 72);
            this.startTextBox.Name = "startTextBox";
            this.startTextBox.Size = new System.Drawing.Size(114, 30);
            this.startTextBox.TabIndex = 3;
            // 
            // suffixTextBox
            // 
            this.suffixTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.suffixTextBox.Location = new System.Drawing.Point(578, 72);
            this.suffixTextBox.Name = "suffixTextBox";
            this.suffixTextBox.Size = new System.Drawing.Size(111, 30);
            this.suffixTextBox.TabIndex = 4;
            // 
            // startLabel
            // 
            this.startLabel.AutoSize = true;
            this.startLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startLabel.Location = new System.Drawing.Point(270, 77);
            this.startLabel.Name = "startLabel";
            this.startLabel.Size = new System.Drawing.Size(59, 25);
            this.startLabel.TabIndex = 5;
            this.startLabel.Text = "Start:";
            // 
            // suffixLabel
            // 
            this.suffixLabel.AutoSize = true;
            this.suffixLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.suffixLabel.Location = new System.Drawing.Point(505, 77);
            this.suffixLabel.Name = "suffixLabel";
            this.suffixLabel.Size = new System.Drawing.Size(67, 25);
            this.suffixLabel.TabIndex = 6;
            this.suffixLabel.Text = "Suffix:";
            // 
            // ScheduleUpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(701, 223);
            this.Controls.Add(this.suffixLabel);
            this.Controls.Add(this.startLabel);
            this.Controls.Add(this.suffixTextBox);
            this.Controls.Add(this.startTextBox);
            this.Controls.Add(this.prefixTextBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.prefixLabel);
            this.Name = "ScheduleUpdateForm";
            this.Text = "ScheduleUpdateForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label prefixLabel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox prefixTextBox;
        private System.Windows.Forms.TextBox startTextBox;
        private System.Windows.Forms.TextBox suffixTextBox;
        private System.Windows.Forms.Label startLabel;
        private System.Windows.Forms.Label suffixLabel;
    }
}
﻿namespace Sheeting_Automation.Source.Tags
{
    partial class TagMissingCheckForm
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.addRowsButton = new System.Windows.Forms.Button();
            this.checkAllButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(1441, 684);
            this.dataGridView1.TabIndex = 0;
            // 
            // addRowsButton
            // 
            this.addRowsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addRowsButton.Location = new System.Drawing.Point(280, 708);
            this.addRowsButton.Name = "addRowsButton";
            this.addRowsButton.Size = new System.Drawing.Size(306, 59);
            this.addRowsButton.TabIndex = 1;
            this.addRowsButton.Text = "Add Rows";
            this.addRowsButton.UseVisualStyleBackColor = true;
            this.addRowsButton.Click += new System.EventHandler(this.addRowsButton_Click);
            // 
            // checkAllButton
            // 
            this.checkAllButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkAllButton.Location = new System.Drawing.Point(832, 708);
            this.checkAllButton.Name = "checkAllButton";
            this.checkAllButton.Size = new System.Drawing.Size(290, 59);
            this.checkAllButton.TabIndex = 2;
            this.checkAllButton.Text = "Check All";
            this.checkAllButton.UseVisualStyleBackColor = true;
            this.checkAllButton.Click += new System.EventHandler(this.checkAllButton_Click);
            // 
            // TagCheckForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1440, 791);
            this.Controls.Add(this.checkAllButton);
            this.Controls.Add(this.addRowsButton);
            this.Controls.Add(this.dataGridView1);
            this.Name = "TagCheckForm";
            this.Text = "TagCheckForm";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button addRowsButton;
        private System.Windows.Forms.Button checkAllButton;
    }
}
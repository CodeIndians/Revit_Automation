namespace Revit_Automation.Dialogs
{
    partial class CeeHeaderAdjustmentsForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.CeeHeaderName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CeeHeaderQuantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PostType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PostGauge = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PostQuantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ChangeOrientation = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dataGridView1);
            this.groupBox1.Location = new System.Drawing.Point(42, 21);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(844, 216);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "CeeHeaders in the Model";
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CeeHeaderName,
            this.CeeHeaderQuantity,
            this.PostType,
            this.PostGauge,
            this.PostQuantity,
            this.ChangeOrientation});
            this.dataGridView1.Location = new System.Drawing.Point(17, 32);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(807, 150);
            this.dataGridView1.TabIndex = 0;
            // 
            // CeeHeaderName
            // 
            this.CeeHeaderName.HeaderText = "Cee Header Name";
            this.CeeHeaderName.MinimumWidth = 6;
            this.CeeHeaderName.Name = "CeeHeaderName";
            this.CeeHeaderName.Width = 125;
            // 
            // CeeHeaderQuantity
            // 
            this.CeeHeaderQuantity.HeaderText = "CeeHeader Quantity";
            this.CeeHeaderQuantity.MinimumWidth = 6;
            this.CeeHeaderQuantity.Name = "CeeHeaderQuantity";
            this.CeeHeaderQuantity.Width = 125;
            // 
            // PostType
            // 
            this.PostType.HeaderText = "Post Type";
            this.PostType.MinimumWidth = 6;
            this.PostType.Name = "PostType";
            this.PostType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.PostType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.PostType.Width = 125;
            // 
            // PostGauge
            // 
            this.PostGauge.HeaderText = "Post Gauge";
            this.PostGauge.MinimumWidth = 6;
            this.PostGauge.Name = "PostGauge";
            this.PostGauge.Width = 125;
            // 
            // PostQuantity
            // 
            this.PostQuantity.HeaderText = "Post Quantity";
            this.PostQuantity.MinimumWidth = 6;
            this.PostQuantity.Name = "PostQuantity";
            this.PostQuantity.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.PostQuantity.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.PostQuantity.Width = 125;
            // 
            // ChangeOrientation
            // 
            this.ChangeOrientation.HeaderText = "Change Orientation";
            this.ChangeOrientation.MinimumWidth = 6;
            this.ChangeOrientation.Name = "ChangeOrientation";
            this.ChangeOrientation.Width = 125;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(374, 254);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(160, 30);
            this.button1.TabIndex = 1;
            this.button1.Text = "Start Adjustment";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(774, 287);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(92, 32);
            this.button2.TabIndex = 2;
            this.button2.Text = "Close";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // CeeHeaderAdjustmentsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 331);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Name = "CeeHeaderAdjustmentsForm";
            this.Text = "CeeHeaderAdjustmentsForm";
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn CeeHeaderName;
        private System.Windows.Forms.DataGridViewTextBoxColumn CeeHeaderQuantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn PostType;
        private System.Windows.Forms.DataGridViewTextBoxColumn PostGauge;
        private System.Windows.Forms.DataGridViewTextBoxColumn PostQuantity;
        private System.Windows.Forms.DataGridViewComboBoxColumn ChangeOrientation;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}
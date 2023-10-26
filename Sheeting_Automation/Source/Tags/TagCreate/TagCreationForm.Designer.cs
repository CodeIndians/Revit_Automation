namespace Sheeting_Automation.Source.Tags
{
    partial class TagCreationForm
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
            this.addCategoryButtom = new System.Windows.Forms.Button();
            this.createButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(1, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(1489, 721);
            this.dataGridView1.TabIndex = 0;
            // 
            // addCategoryButtom
            // 
            this.addCategoryButtom.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addCategoryButtom.Location = new System.Drawing.Point(254, 738);
            this.addCategoryButtom.Name = "addCategoryButtom";
            this.addCategoryButtom.Size = new System.Drawing.Size(420, 64);
            this.addCategoryButtom.TabIndex = 1;
            this.addCategoryButtom.Text = "Add Model Tag Category";
            this.addCategoryButtom.UseVisualStyleBackColor = true;
            this.addCategoryButtom.Click += new System.EventHandler(this.addCategoryButtom_Click);
            // 
            // createButton
            // 
            this.createButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.createButton.Location = new System.Drawing.Point(849, 738);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(242, 64);
            this.createButton.TabIndex = 2;
            this.createButton.Text = "Create Tags";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // TagCreationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1490, 814);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.addCategoryButtom);
            this.Controls.Add(this.dataGridView1);
            this.Name = "TagCreationForm";
            this.Text = "TagCreationForm";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button addCategoryButtom;
        private System.Windows.Forms.Button createButton;
    }
}
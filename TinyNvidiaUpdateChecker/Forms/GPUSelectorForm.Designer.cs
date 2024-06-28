namespace TinyNvidiaUpdateChecker
{
    partial class GPUSelectorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GPUSelectorForm));
            ConfirmBtn = new System.Windows.Forms.Button();
            richTextBox1 = new System.Windows.Forms.RichTextBox();
            comboBox = new System.Windows.Forms.ComboBox();
            radioButtonDefault = new System.Windows.Forms.RadioButton();
            radioButtonDesktop = new System.Windows.Forms.RadioButton();
            radioButtonNotebook = new System.Windows.Forms.RadioButton();
            groupBoxType = new System.Windows.Forms.GroupBox();
            groupBoxType.SuspendLayout();
            SuspendLayout();
            // 
            // ConfirmBtn
            // 
            ConfirmBtn.Anchor = System.Windows.Forms.AnchorStyles.Top;
            ConfirmBtn.Enabled = false;
            ConfirmBtn.Location = new System.Drawing.Point(252, 249);
            ConfirmBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ConfirmBtn.Name = "ConfirmBtn";
            ConfirmBtn.Size = new System.Drawing.Size(90, 29);
            ConfirmBtn.TabIndex = 1;
            ConfirmBtn.Text = "Confirm";
            ConfirmBtn.UseVisualStyleBackColor = true;
            ConfirmBtn.Click += ConfirmBtn_Click;
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            richTextBox1.Location = new System.Drawing.Point(42, 23);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            richTextBox1.Size = new System.Drawing.Size(262, 77);
            richTextBox1.TabIndex = 10;
            richTextBox1.TabStop = false;
            richTextBox1.Text = "Multiple GPUs have been identified, which one do you want to search updates for?";
            // 
            // comboBox
            // 
            comboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox.FormattingEnabled = true;
            comboBox.Location = new System.Drawing.Point(12, 106);
            comboBox.Name = "comboBox";
            comboBox.Size = new System.Drawing.Size(330, 28);
            comboBox.TabIndex = 0;
            comboBox.SelectedIndexChanged += comboBox_SelectedIndexChanged;
            // 
            // radioButtonDefault
            // 
            radioButtonDefault.AutoSize = true;
            radioButtonDefault.Location = new System.Drawing.Point(6, 31);
            radioButtonDefault.Name = "radioButtonDefault";
            radioButtonDefault.Size = new System.Drawing.Size(94, 24);
            radioButtonDefault.TabIndex = 12;
            radioButtonDefault.TabStop = true;
            radioButtonDefault.Text = "Identified";
            radioButtonDefault.UseVisualStyleBackColor = true;
            // 
            // radioButtonDesktop
            // 
            radioButtonDesktop.AutoSize = true;
            radioButtonDesktop.Location = new System.Drawing.Point(116, 31);
            radioButtonDesktop.Name = "radioButtonDesktop";
            radioButtonDesktop.Size = new System.Drawing.Size(85, 24);
            radioButtonDesktop.TabIndex = 13;
            radioButtonDesktop.TabStop = true;
            radioButtonDesktop.Text = "Desktop";
            radioButtonDesktop.UseVisualStyleBackColor = true;
            // 
            // radioButtonNotebook
            // 
            radioButtonNotebook.AutoSize = true;
            radioButtonNotebook.Location = new System.Drawing.Point(221, 31);
            radioButtonNotebook.Name = "radioButtonNotebook";
            radioButtonNotebook.Size = new System.Drawing.Size(97, 24);
            radioButtonNotebook.TabIndex = 14;
            radioButtonNotebook.TabStop = true;
            radioButtonNotebook.Text = "Notebook";
            radioButtonNotebook.UseVisualStyleBackColor = true;
            // 
            // groupBoxType
            // 
            groupBoxType.Controls.Add(radioButtonDefault);
            groupBoxType.Controls.Add(radioButtonNotebook);
            groupBoxType.Controls.Add(radioButtonDesktop);
            groupBoxType.Enabled = false;
            groupBoxType.Location = new System.Drawing.Point(12, 140);
            groupBoxType.Name = "groupBoxType";
            groupBoxType.Size = new System.Drawing.Size(330, 102);
            groupBoxType.TabIndex = 15;
            groupBoxType.TabStop = false;
            groupBoxType.Text = "GPU Type";
            // 
            // GPUSelectorForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(354, 291);
            Controls.Add(groupBoxType);
            Controls.Add(comboBox);
            Controls.Add(richTextBox1);
            Controls.Add(ConfirmBtn);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "GPUSelectorForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "TinyNvidiaUpdateChecker - Choose GPU";
            Load += GPUSelectorForm_Load;
            groupBoxType.ResumeLayout(false);
            groupBoxType.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button ConfirmBtn;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ComboBox comboBox;
        private System.Windows.Forms.RadioButton radioButtonDefault;
        private System.Windows.Forms.RadioButton radioButtonDesktop;
        private System.Windows.Forms.RadioButton radioButtonNotebook;
        private System.Windows.Forms.GroupBox groupBoxType;
    }
}
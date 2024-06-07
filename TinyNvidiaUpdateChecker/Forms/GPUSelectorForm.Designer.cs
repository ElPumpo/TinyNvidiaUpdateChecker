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
            SuspendLayout();
            // 
            // ConfirmBtn
            // 
            ConfirmBtn.Anchor = System.Windows.Forms.AnchorStyles.Top;
            ConfirmBtn.Enabled = false;
            ConfirmBtn.Location = new System.Drawing.Point(234, 123);
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
            richTextBox1.Size = new System.Drawing.Size(244, 77);
            richTextBox1.TabIndex = 10;
            richTextBox1.TabStop = false;
            richTextBox1.Text = "Multiple GPUs have been identified, which one do you want to search updates for?";
            // 
            // comboBox
            // 
            comboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox.FormattingEnabled = true;
            comboBox.Location = new System.Drawing.Point(12, 124);
            comboBox.Name = "comboBox";
            comboBox.Size = new System.Drawing.Size(196, 28);
            comboBox.TabIndex = 0;
            comboBox.SelectedIndexChanged += comboBox_SelectedIndexChanged;
            // 
            // GPUSelectorForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(336, 171);
            Controls.Add(comboBox);
            Controls.Add(richTextBox1);
            Controls.Add(ConfirmBtn);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.Execut‌​ablePath);
            Name = "GPUSelectorForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "TinyNvidiaUpdateChecker - Choose GPU";
            Load += GPUSelectorForm_Load;
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button ConfirmBtn;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ComboBox comboBox;
    }
}
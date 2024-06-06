namespace TinyNvidiaUpdateChecker
{
    partial class LocationChooserForm
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
            ConfirmBtn = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            comboBox = new System.Windows.Forms.ComboBox();
            SuspendLayout();
            // 
            // ConfirmBtn
            // 
            ConfirmBtn.Enabled = false;
            ConfirmBtn.Location = new System.Drawing.Point(221, 67);
            ConfirmBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ConfirmBtn.Name = "ConfirmBtn";
            ConfirmBtn.Size = new System.Drawing.Size(75, 29);
            ConfirmBtn.TabIndex = 1;
            ConfirmBtn.Text = "Confirm";
            ConfirmBtn.UseVisualStyleBackColor = true;
            ConfirmBtn.Click += ConfirmBtn_Click;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(66, 25);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(187, 20);
            label1.TabIndex = 2;
            label1.Text = "Choose download location";
            // 
            // comboBox
            // 
            comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox.FormattingEnabled = true;
            comboBox.Location = new System.Drawing.Point(12, 68);
            comboBox.Name = "comboBox";
            comboBox.Size = new System.Drawing.Size(187, 28);
            comboBox.TabIndex = 3;
            comboBox.SelectedIndexChanged += comboBox_SelectedIndexChanged;
            // 
            // LocationChooserForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(308, 112);
            Controls.Add(comboBox);
            Controls.Add(label1);
            Controls.Add(ConfirmBtn);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "LocationChooserForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "TinyNvidiaUpdateChecker - Choose Download Location";
            Load += LocationChooserForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button ConfirmBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox;
    }
}
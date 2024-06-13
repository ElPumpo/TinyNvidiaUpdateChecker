namespace TinyNvidiaUpdateChecker.Forms
{
    partial class ComponentChooserForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComponentChooserForm));
            checkedListBox = new System.Windows.Forms.CheckedListBox();
            okButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            richTextBox = new System.Windows.Forms.RichTextBox();
            noneLabel = new System.Windows.Forms.LinkLabel();
            allLabel = new System.Windows.Forms.LinkLabel();
            SuspendLayout();
            // 
            // checkedListBox
            // 
            checkedListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkedListBox.FormattingEnabled = true;
            checkedListBox.Location = new System.Drawing.Point(12, 12);
            checkedListBox.Name = "checkedListBox";
            checkedListBox.Size = new System.Drawing.Size(387, 466);
            checkedListBox.TabIndex = 0;
            checkedListBox.SelectedValueChanged += checkedListBox_SelectedValueChanged;
            // 
            // okButton
            // 
            okButton.Location = new System.Drawing.Point(12, 511);
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(67, 29);
            okButton.TabIndex = 4;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(87, 515);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(285, 20);
            label1.TabIndex = 5;
            label1.Text = "Chose the components you want to install";
            // 
            // richTextBox
            // 
            richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            richTextBox.Location = new System.Drawing.Point(405, 12);
            richTextBox.Name = "richTextBox";
            richTextBox.ReadOnly = true;
            richTextBox.Size = new System.Drawing.Size(282, 466);
            richTextBox.TabIndex = 1;
            richTextBox.Text = "";
            // 
            // noneLabel
            // 
            noneLabel.AutoSize = true;
            noneLabel.Location = new System.Drawing.Point(12, 481);
            noneLabel.Name = "noneLabel";
            noneLabel.Size = new System.Drawing.Size(45, 20);
            noneLabel.TabIndex = 2;
            noneLabel.TabStop = true;
            noneLabel.Text = "None";
            noneLabel.LinkClicked += noneLabel_LinkClicked;
            // 
            // allLabel
            // 
            allLabel.AutoSize = true;
            allLabel.Location = new System.Drawing.Point(87, 481);
            allLabel.Name = "allLabel";
            allLabel.Size = new System.Drawing.Size(27, 20);
            allLabel.TabIndex = 3;
            allLabel.TabStop = true;
            allLabel.Text = "All";
            allLabel.LinkClicked += allLabel_LinkClicked;
            // 
            // ComponentChooserForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(699, 554);
            Controls.Add(allLabel);
            Controls.Add(noneLabel);
            Controls.Add(richTextBox);
            Controls.Add(label1);
            Controls.Add(okButton);
            Controls.Add(checkedListBox);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.Execut‌​ablePath);
            Name = "ComponentChooserForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Choose Components";
            Load += ComponentChooserForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.LinkLabel noneLabel;
        private System.Windows.Forms.LinkLabel allLabel;
    }
}
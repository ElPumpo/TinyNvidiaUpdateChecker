namespace TinyNvidiaUpdateChecker
{
    partial class SelectGPU
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
            this.SelectBtn = new System.Windows.Forms.Button();
            this.GPUBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SelectBtn
            // 
            this.SelectBtn.Location = new System.Drawing.Point(12, 102);
            this.SelectBtn.Name = "SelectBtn";
            this.SelectBtn.Size = new System.Drawing.Size(75, 23);
            this.SelectBtn.TabIndex = 1;
            this.SelectBtn.Text = "OK";
            this.SelectBtn.UseVisualStyleBackColor = true;
            this.SelectBtn.Click += new System.EventHandler(this.SelectBtn_Click);
            // 
            // GPUBox
            // 
            this.GPUBox.FormattingEnabled = true;
            this.GPUBox.ItemHeight = 16;
            this.GPUBox.Location = new System.Drawing.Point(12, 12);
            this.GPUBox.Name = "GPUBox";
            this.GPUBox.Size = new System.Drawing.Size(239, 84);
            this.GPUBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(93, 105);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Select your NVIDIA GPU";
            // 
            // SelectGPU
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(263, 137);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.GPUBox);
            this.Controls.Add(this.SelectBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SelectGPU";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TinyNvidiaUpdateChecker - GPU";
            this.Load += new System.EventHandler(this.SelectGPU_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button SelectBtn;
        private System.Windows.Forms.ListBox GPUBox;
        private System.Windows.Forms.Label label1;
    }
}
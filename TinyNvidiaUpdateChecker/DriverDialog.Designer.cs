using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    partial class DriverDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DriverDialog));
            this.DownloadInstallButton = new System.Windows.Forms.Button();
            this.DownloadBtn = new System.Windows.Forms.Button();
            this.NotesBtn = new System.Windows.Forms.Button();
            this.TitleLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SizeLabel = new System.Windows.Forms.Label();
            this.ReleasedLabel = new System.Windows.Forms.Label();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.IgnoreBtn = new System.Windows.Forms.Button();
            this.CopyUrlBtn = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // DownloadInstallButton
            // 
            this.DownloadInstallButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DownloadInstallButton.Location = new System.Drawing.Point(28, 160);
            this.DownloadInstallButton.Margin = new System.Windows.Forms.Padding(2);
            this.DownloadInstallButton.Name = "DownloadInstallButton";
            this.DownloadInstallButton.Size = new System.Drawing.Size(82, 41);
            this.DownloadInstallButton.TabIndex = 0;
            this.DownloadInstallButton.Text = "Download + Install";
            this.toolTip1.SetToolTip(this.DownloadInstallButton, "Download and install the drivers automatically");
            this.DownloadInstallButton.UseVisualStyleBackColor = true;
            this.DownloadInstallButton.Click += new System.EventHandler(this.DownloadInstallButton_Click);
            // 
            // DownloadBtn
            // 
            this.DownloadBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DownloadBtn.Location = new System.Drawing.Point(115, 160);
            this.DownloadBtn.Margin = new System.Windows.Forms.Padding(2);
            this.DownloadBtn.Name = "DownloadBtn";
            this.DownloadBtn.Size = new System.Drawing.Size(82, 41);
            this.DownloadBtn.TabIndex = 1;
            this.DownloadBtn.Text = "Download (+ Extract)";
            this.toolTip1.SetToolTip(this.DownloadBtn, "Downloads and extracts the graphics drivers, but won\'t run the installer");
            this.DownloadBtn.UseVisualStyleBackColor = true;
            this.DownloadBtn.Click += new System.EventHandler(this.DownloadBtn_Click);
            // 
            // NotesBtn
            // 
            this.NotesBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NotesBtn.Location = new System.Drawing.Point(201, 160);
            this.NotesBtn.Margin = new System.Windows.Forms.Padding(2);
            this.NotesBtn.Name = "NotesBtn";
            this.NotesBtn.Size = new System.Drawing.Size(82, 41);
            this.NotesBtn.TabIndex = 2;
            this.NotesBtn.Text = "View Release Notes";
            this.toolTip1.SetToolTip(this.NotesBtn, "Show the release notes, that contains the following: new additions, what\'s fixed," +
              " known issues and more");
            this.NotesBtn.UseVisualStyleBackColor = true;
            this.NotesBtn.Click += new System.EventHandler(this.NotesBtn_Click);
            // 
            // TitleLabel
            // 
            this.TitleLabel.AutoSize = true;
            this.TitleLabel.Location = new System.Drawing.Point(7, 7);
            this.TitleLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(245, 12);
            this.TitleLabel.TabIndex = 5;
            this.TitleLabel.Text = "New graphics card drivers are available!\r\n";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.SizeLabel);
            this.groupBox1.Controls.Add(this.ReleasedLabel);
            this.groupBox1.Controls.Add(this.VersionLabel);
            this.groupBox1.Location = new System.Drawing.Point(304, 30);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(150, 116);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "File Information";
            // 
            // SizeLabel
            // 
            this.SizeLabel.Location = new System.Drawing.Point(4, 78);
            this.SizeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SizeLabel.Name = "SizeLabel";
            this.SizeLabel.Size = new System.Drawing.Size(141, 33);
            this.SizeLabel.TabIndex = 6;
            this.SizeLabel.Text = "Size: ";
            this.SizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ReleasedLabel
            // 
            this.ReleasedLabel.Location = new System.Drawing.Point(4, 46);
            this.ReleasedLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ReleasedLabel.Name = "ReleasedLabel";
            this.ReleasedLabel.Size = new System.Drawing.Size(141, 33);
            this.ReleasedLabel.TabIndex = 5;
            this.ReleasedLabel.Text = "Released: ";
            this.ReleasedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // VersionLabel
            // 
            this.VersionLabel.Location = new System.Drawing.Point(4, 14);
            this.VersionLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(141, 33);
            this.VersionLabel.TabIndex = 5;
            this.VersionLabel.Text = "Version: ";
            this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.VersionLabel, "The version of the graphics drivers");
            // 
            // IgnoreBtn
            // 
            this.IgnoreBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.IgnoreBtn.Location = new System.Drawing.Point(287, 160);
            this.IgnoreBtn.Margin = new System.Windows.Forms.Padding(2);
            this.IgnoreBtn.Name = "IgnoreBtn";
            this.IgnoreBtn.Size = new System.Drawing.Size(82, 41);
            this.IgnoreBtn.TabIndex = 3;
            this.IgnoreBtn.Text = "Ignore";
            this.toolTip1.SetToolTip(this.IgnoreBtn, "Ignore the update this time");
            this.IgnoreBtn.UseVisualStyleBackColor = true;
            this.IgnoreBtn.Click += new System.EventHandler(this.IgnoreBtn_Click);
            // 
            // CopyUrlBtn
            // 
            this.CopyUrlBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CopyUrlBtn.Location = new System.Drawing.Point(373, 160);
            this.CopyUrlBtn.Name = "CopyUrlBtn";
            this.CopyUrlBtn.Size = new System.Drawing.Size(82, 41);
            this.CopyUrlBtn.TabIndex = 2;
            this.CopyUrlBtn.Text = "Copy File Url";
            this.toolTip1.SetToolTip(this.CopyUrlBtn, "Copy file url of the graphics drives");
            this.CopyUrlBtn.UseVisualStyleBackColor = true;
            this.CopyUrlBtn.Click += new System.EventHandler(this.CopyUrlBtn_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                  | System.Windows.Forms.AnchorStyles.Left)));
            this.webBrowser1.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser1.Location = new System.Drawing.Point(9, 22);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(15, 15);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.Size = new System.Drawing.Size(291, 134);
            this.webBrowser1.TabIndex = 4;
            this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted);
            // 
            // DriverDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(463, 211);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.IgnoreBtn);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.TitleLabel);
            this.Controls.Add(this.NotesBtn);
            this.Controls.Add(this.DownloadBtn);
            this.Controls.Add(this.DownloadInstallButton);
            this.Controls.Add(this.CopyUrlBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DriverDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TinyNvidiaUpdateChecker - Update Dialog";
            this.Load += new System.EventHandler(this.DriverDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button DownloadInstallButton;
        private System.Windows.Forms.Button DownloadBtn;
        private System.Windows.Forms.Label TitleLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.Label ReleasedLabel;
        private System.Windows.Forms.Button IgnoreBtn;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private ToolTip toolTip1;
        private Label SizeLabel;
        private Button CopyUrlBtn;
        private Button NotesBtn;
    }
}
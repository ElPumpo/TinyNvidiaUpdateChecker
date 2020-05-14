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
            this.titleLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.sizeLabel = new System.Windows.Forms.Label();
            this.releasedLabel = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.Label();
            this.IgnoreBtn = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // DownloadInstallButton
            // 
            this.DownloadInstallButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DownloadInstallButton.Location = new System.Drawing.Point(38, 214);
            this.DownloadInstallButton.Name = "DownloadInstallButton";
            this.DownloadInstallButton.Size = new System.Drawing.Size(109, 55);
            this.DownloadInstallButton.TabIndex = 0;
            this.DownloadInstallButton.Text = "Install Now";
            this.toolTip1.SetToolTip(this.DownloadInstallButton, resources.GetString("DownloadInstallButton.ToolTip"));
            this.DownloadInstallButton.UseVisualStyleBackColor = true;
            this.DownloadInstallButton.Click += new System.EventHandler(this.DownloadInstallButton_Click);
            // 
            // DownloadBtn
            // 
            this.DownloadBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DownloadBtn.Location = new System.Drawing.Point(153, 214);
            this.DownloadBtn.Name = "DownloadBtn";
            this.DownloadBtn.Size = new System.Drawing.Size(109, 55);
            this.DownloadBtn.TabIndex = 1;
            this.DownloadBtn.Text = "Download Only";
            this.toolTip1.SetToolTip(this.DownloadBtn, resources.GetString("DownloadBtn.ToolTip"));
            this.DownloadBtn.UseVisualStyleBackColor = true;
            this.DownloadBtn.Click += new System.EventHandler(this.DownloadBtn_Click);
            // 
            // NotesBtn
            // 
            this.NotesBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NotesBtn.Location = new System.Drawing.Point(268, 214);
            this.NotesBtn.Name = "NotesBtn";
            this.NotesBtn.Size = new System.Drawing.Size(109, 55);
            this.NotesBtn.TabIndex = 2;
            this.NotesBtn.Text = "View Release Notes";
            this.toolTip1.SetToolTip(this.NotesBtn, "View the full pdf release notes, which contains:\r\n- what\'s new\r\n- what\'s fixed\r\n-" +
        " open issues\r\n\r\nand more!");
            this.NotesBtn.UseVisualStyleBackColor = true;
            this.NotesBtn.Click += new System.EventHandler(this.NotesBtn_Click);
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Location = new System.Drawing.Point(9, 9);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(260, 17);
            this.titleLabel.TabIndex = 5;
            this.titleLabel.Text = "New graphics card drivers are available!\r\n";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.sizeLabel);
            this.groupBox1.Controls.Add(this.releasedLabel);
            this.groupBox1.Controls.Add(this.versionLabel);
            this.groupBox1.Location = new System.Drawing.Point(315, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 154);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "File Information";
            // 
            // sizeLabel
            // 
            this.sizeLabel.Location = new System.Drawing.Point(6, 104);
            this.sizeLabel.Name = "sizeLabel";
            this.sizeLabel.Size = new System.Drawing.Size(188, 44);
            this.sizeLabel.TabIndex = 6;
            this.sizeLabel.Text = "Size: ";
            this.sizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // releasedLabel
            // 
            this.releasedLabel.Location = new System.Drawing.Point(6, 62);
            this.releasedLabel.Name = "releasedLabel";
            this.releasedLabel.Size = new System.Drawing.Size(188, 44);
            this.releasedLabel.TabIndex = 5;
            this.releasedLabel.Text = "Released: ";
            this.releasedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // versionLabel
            // 
            this.versionLabel.Location = new System.Drawing.Point(6, 18);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(188, 44);
            this.versionLabel.TabIndex = 5;
            this.versionLabel.Text = "Version: ";
            this.versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.versionLabel, "The version of the graphics drivers");
            // 
            // IgnoreBtn
            // 
            this.IgnoreBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.IgnoreBtn.Location = new System.Drawing.Point(383, 214);
            this.IgnoreBtn.Name = "IgnoreBtn";
            this.IgnoreBtn.Size = new System.Drawing.Size(109, 55);
            this.IgnoreBtn.TabIndex = 3;
            this.IgnoreBtn.Text = "Ignore";
            this.IgnoreBtn.UseVisualStyleBackColor = true;
            this.IgnoreBtn.Click += new System.EventHandler(this.IgnoreBtn_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.AllowNavigation = false;
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.webBrowser1.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser1.Location = new System.Drawing.Point(12, 29);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.Size = new System.Drawing.Size(300, 179);
            this.webBrowser1.TabIndex = 4;
            this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted);
            // 
            // DriverDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(527, 281);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.IgnoreBtn);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.NotesBtn);
            this.Controls.Add(this.DownloadBtn);
            this.Controls.Add(this.DownloadInstallButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.Execut‌​ablePath);
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
        private System.Windows.Forms.Button NotesBtn;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Label releasedLabel;
        private System.Windows.Forms.Button IgnoreBtn;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private ToolTip toolTip1;
        private Label sizeLabel;
    }
}
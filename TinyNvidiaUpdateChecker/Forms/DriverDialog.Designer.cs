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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DriverDialog));
            DownloadInstallButton = new Button();
            DownloadBtn = new Button();
            NotesBtn = new Button();
            titleLabel = new Label();
            groupBox1 = new GroupBox();
            sizeLabel = new Label();
            releasedLabel = new Label();
            versionLabel = new Label();
            IgnoreBtn = new Button();
            webBrowser1 = new WebBrowser();
            toolTip1 = new ToolTip(components);
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // DownloadInstallButton
            // 
            DownloadInstallButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            DownloadInstallButton.Location = new System.Drawing.Point(38, 268);
            DownloadInstallButton.Margin = new Padding(3, 4, 3, 4);
            DownloadInstallButton.Name = "DownloadInstallButton";
            DownloadInstallButton.Size = new System.Drawing.Size(109, 69);
            DownloadInstallButton.TabIndex = 0;
            DownloadInstallButton.Text = "Install Now";
            toolTip1.SetToolTip(DownloadInstallButton, resources.GetString("DownloadInstallButton.ToolTip"));
            DownloadInstallButton.UseVisualStyleBackColor = true;
            DownloadInstallButton.Click += DownloadInstallButton_Click;
            // 
            // DownloadBtn
            // 
            DownloadBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            DownloadBtn.Location = new System.Drawing.Point(153, 268);
            DownloadBtn.Margin = new Padding(3, 4, 3, 4);
            DownloadBtn.Name = "DownloadBtn";
            DownloadBtn.Size = new System.Drawing.Size(109, 69);
            DownloadBtn.TabIndex = 1;
            DownloadBtn.Text = "Download Only";
            toolTip1.SetToolTip(DownloadBtn, resources.GetString("DownloadBtn.ToolTip"));
            DownloadBtn.UseVisualStyleBackColor = true;
            DownloadBtn.Click += DownloadBtn_Click;
            // 
            // NotesBtn
            // 
            NotesBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            NotesBtn.Location = new System.Drawing.Point(268, 268);
            NotesBtn.Margin = new Padding(3, 4, 3, 4);
            NotesBtn.Name = "NotesBtn";
            NotesBtn.Size = new System.Drawing.Size(109, 69);
            NotesBtn.TabIndex = 2;
            NotesBtn.Text = "View Release Notes";
            toolTip1.SetToolTip(NotesBtn, "View the full pdf release notes, which contains:\r\n- what's new\r\n- what's fixed\r\n- open issues\r\n\r\nand more!");
            NotesBtn.UseVisualStyleBackColor = true;
            NotesBtn.Click += NotesBtn_Click;
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Location = new System.Drawing.Point(9, 11);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new System.Drawing.Size(267, 20);
            titleLabel.TabIndex = 5;
            titleLabel.Text = "A new graphics card driver is available!\r\n";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            groupBox1.Controls.Add(sizeLabel);
            groupBox1.Controls.Add(releasedLabel);
            groupBox1.Controls.Add(versionLabel);
            groupBox1.Location = new System.Drawing.Point(315, 50);
            groupBox1.Margin = new Padding(3, 4, 3, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 4, 3, 4);
            groupBox1.Size = new System.Drawing.Size(200, 192);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "File Information";
            // 
            // sizeLabel
            // 
            sizeLabel.Location = new System.Drawing.Point(6, 130);
            sizeLabel.Name = "sizeLabel";
            sizeLabel.Size = new System.Drawing.Size(188, 55);
            sizeLabel.TabIndex = 6;
            sizeLabel.Text = "Size: ";
            sizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // releasedLabel
            // 
            releasedLabel.Location = new System.Drawing.Point(6, 78);
            releasedLabel.Name = "releasedLabel";
            releasedLabel.Size = new System.Drawing.Size(188, 55);
            releasedLabel.TabIndex = 5;
            releasedLabel.Text = "Released: ";
            releasedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // versionLabel
            // 
            versionLabel.Location = new System.Drawing.Point(6, 22);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new System.Drawing.Size(188, 55);
            versionLabel.TabIndex = 5;
            versionLabel.Text = "Version: ";
            versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            toolTip1.SetToolTip(versionLabel, "The version of the graphics drivers");
            // 
            // IgnoreBtn
            // 
            IgnoreBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            IgnoreBtn.Location = new System.Drawing.Point(383, 268);
            IgnoreBtn.Margin = new Padding(3, 4, 3, 4);
            IgnoreBtn.Name = "IgnoreBtn";
            IgnoreBtn.Size = new System.Drawing.Size(109, 69);
            IgnoreBtn.TabIndex = 3;
            IgnoreBtn.Text = "Ignore";
            IgnoreBtn.UseVisualStyleBackColor = true;
            IgnoreBtn.Click += IgnoreBtn_Click;
            // 
            // webBrowser1
            // 
            webBrowser1.AllowNavigation = false;
            webBrowser1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            webBrowser1.IsWebBrowserContextMenuEnabled = false;
            webBrowser1.Location = new System.Drawing.Point(12, 36);
            webBrowser1.Margin = new Padding(3, 4, 3, 4);
            webBrowser1.MinimumSize = new System.Drawing.Size(20, 25);
            webBrowser1.Name = "webBrowser1";
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Size = new System.Drawing.Size(300, 224);
            webBrowser1.TabIndex = 4;
            webBrowser1.WebBrowserShortcutsEnabled = false;
            webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;
            // 
            // DriverDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(527, 351);
            Controls.Add(webBrowser1);
            Controls.Add(IgnoreBtn);
            Controls.Add(groupBox1);
            Controls.Add(titleLabel);
            Controls.Add(NotesBtn);
            Controls.Add(DownloadBtn);
            Controls.Add(DownloadInstallButton);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 4, 3, 4);
            Name = "DriverDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TinyNvidiaUpdateChecker - Update Dialog";
            Load += DriverDialog_Load;
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
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
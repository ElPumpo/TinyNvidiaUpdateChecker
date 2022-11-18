using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    public partial class DriverDialog : Form
    {
        public static SelectedBtn selectedBtn;
        float notesScale;

        public DriverDialog()
        {
            InitializeComponent();
        }

        public static void ShowGUI()
        {
            // show the form
            using (var form = new DriverDialog()) {
                form.ShowDialog();
            }
        }

        private void DriverDialog_Load(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = MainConsole.releaseDesc;
            notesScale = this.CreateGraphics().DpiX;

            var dateDiff = (DateTime.Now - MainConsole.releaseDate).Days; // how many days between the two dates
            string daysAgoFromRelease = null;

            if (dateDiff == 1) {
                daysAgoFromRelease = $"{dateDiff} day ago";
            } else if (dateDiff < 1) {
                daysAgoFromRelease = "today"; // we only have the date and not time :/
            } else {
                daysAgoFromRelease = $"{dateDiff} days ago";
            }

            releasedLabel.Text += daysAgoFromRelease;
            toolTip1.SetToolTip(releasedLabel, MainConsole.releaseDate.ToShortDateString());

            versionLabel.Text += MainConsole.OnlineGPUVersion + $" (you're on {MainConsole.OfflineGPUVersion})";
            sizeLabel.Text += MainConsole.downloadFileSize;
        }

        private void NotesBtn_Click(object sender, EventArgs e)
        {
            try {
                Process.Start(MainConsole.pdfURL);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowser1.Document.ExecCommand("SelectAll", false, "null");
            webBrowser1.Document.ExecCommand("FontName", false, "Microsoft Sans Serif");
            if (notesScale > 96) {
                webBrowser1.Document.ExecCommand("FontSize", false, 1);
            } else {
                webBrowser1.Document.ExecCommand("FontSize", false, 2);
            }

            webBrowser1.Document.ExecCommand("Unselect", false, "null");
        }

        private void IgnoreBtn_Click(object sender, EventArgs e)
        {
            selectedBtn = SelectedBtn.IGNORE;
            Close();
        }

        private void DownloadInstallButton_Click(object sender, EventArgs e)
        {
            selectedBtn = SelectedBtn.DLINSTALL;
            Close();
        }

        private void DownloadBtn_Click(object sender, EventArgs e)
        {
            selectedBtn = SelectedBtn.DLEXTRACT;
            Close();
        }

        public enum SelectedBtn
        {
            DLINSTALL,
            DLEXTRACT,
            IGNORE
        }
    }
}

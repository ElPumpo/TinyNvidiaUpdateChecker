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

        public static string ShowGUI()
        {
            // show the form
            using (DriverDialog form = new DriverDialog()) {
                form.ShowDialog();
            }

            return null;
        }

        private void DriverDialog_Load(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = MainConsole.releaseDesc;

            notesScale = this.CreateGraphics().DpiX;

            int DateDiff = (DateTime.Now - MainConsole.releaseDate).Days; // how many days between the two dates
            string theDate = null;

            if (DateDiff == 1) {
                theDate = DateDiff + " day ago";
            } else if (DateDiff < 1) {
                theDate = "today"; // we only have the date and not time :/
            } else {
                theDate = DateDiff + " days ago";
            }

            ReleasedLabel.Text += theDate;
            toolTip1.SetToolTip(ReleasedLabel, MainConsole.releaseDate.ToShortDateString());

            VersionLabel.Text += MainConsole.OnlineGPUVersion + " (you're on " + MainConsole.OfflineGPUVersion + ")";

            SizeLabel.Text += Math.Round((MainConsole.downloadFileSize / 1024f) / 1024f) + " MB";
            toolTip1.SetToolTip(SizeLabel, MainConsole.downloadFileSize + " byte");
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

        private void CopyUrlBtn_Click(object sender, EventArgs e)
        {
            selectedBtn = SelectedBtn.COPYURL;
            Close();
        }

        public enum SelectedBtn
        {
            DLINSTALL,
            DLEXTRACT,
            IGNORE,
            COPYURL
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using TinyNvidiaUpdateChecker.Forms;

namespace TinyNvidiaUpdateChecker
{
    public partial class DriverDialog : Form
    {
        public static SelectedBtn selectedBtn;
        static DriverMetadata metadata;
        float notesScale;

        public DriverDialog()
        {
            InitializeComponent();
        }

        public static void ShowGUI(DriverMetadata _metadata)
        {
            metadata = _metadata;
            using DriverDialog form = new();
            form.ShowDialog();
        }

        private void DriverDialog_Load(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = metadata.releaseNotes;
            notesScale = this.CreateGraphics().DpiX;

            var dateDiff = (DateTime.Now - metadata.releaseDate).Days; // how many days between the two dates
            string daysAgoFromRelease;

            if (dateDiff == 1)
            {
                daysAgoFromRelease = $"{dateDiff} day ago";
            }
            else if (dateDiff < 1)
            {
                daysAgoFromRelease = "today"; // we only have the date and not time :/
            }
            else
            {
                daysAgoFromRelease = $"{dateDiff} days ago";
            }

            releasedLabel.Text += daysAgoFromRelease;
            toolTip1.SetToolTip(releasedLabel, metadata.releaseDate.ToShortDateString());

            versionLabel.Text += MainConsole.OnlineGPUVersion + $" (you're on {MainConsole.OfflineGPUVersion})";
            sizeLabel.Text += Math.Round((metadata.fileSize / 1024f) / 1024f) + " MiB";
            NotesBtn.Enabled = (metadata.pdfUrl != null);
        }

        private void NotesBtn_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(metadata.pdfUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowser1.Document.ExecCommand("SelectAll", false, "null");
            webBrowser1.Document.ExecCommand("FontName", false, "Microsoft Sans Serif");
            if (notesScale > 96)
            {
                webBrowser1.Document.ExecCommand("FontSize", false, 1);
            }
            else
            {
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
            contextMenuStrip1.Show(DownloadInstallButton, 0, DownloadInstallButton.Height);
        }

        private void DownloadBtn_Click(object sender, EventArgs e)
        {
            selectedBtn = SelectedBtn.DLEXTRACT;
            Close();
        }

        private void contextMenuStrip1_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                var hovered = contextMenuStrip1.Items
                    .OfType<ToolStripMenuItem>()
                    .FirstOrDefault(i => i.Bounds.Contains(contextMenuStrip1.PointToClient(Cursor.Position)));

                if (hovered != null && hovered.Text == "Keep driver files?")
                {
                    e.Cancel = true;
                }
            }
        }

        private void configButton_Click(object sender, EventArgs e)
        {
            ConfigurationForm configForm = new();
            Hide();
            configForm.OpenForm();
            Show();
        }

        private void installItem_Click(object sender, EventArgs e)
        {
            selectedBtn = keepCheckBox.Checked ? SelectedBtn.DLINSTALLCUSTOM : SelectedBtn.DLINSTALL;
            Close();
        }

        public enum SelectedBtn
        {
            DLINSTALL,
            DLINSTALLCUSTOM,
            DLEXTRACT,
            IGNORE
        }
    }
}

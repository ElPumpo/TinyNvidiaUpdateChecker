using System;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    public partial class DownloaderForm : Form {

        public DownloaderForm() => InitializeComponent();

        public async void DownloadFile(string downloadURL, string savePath)
        {
            Show();
            Focus();
            bool complete = false;

            EventHandler<float> progressHandler = (sender, progress) => {
                progressBar1.Value = (int)progress;
                if (((int)progress) == 100) { complete = true; }
            };

            try {
                MainConsole.HandleDownload(downloadURL, savePath, progressHandler);
            } catch { complete = true; }

            // TODO: causes high CPU usage!
            while (!complete) {
                Application.DoEvents();
            }

            Close();
        }
    }
}

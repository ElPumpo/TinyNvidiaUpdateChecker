using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    public partial class DownloaderForm : Form {

        private bool isDownloadComplete = false;

        public DownloaderForm()
        {
            InitializeComponent();
        }

        public void DownloadFile(Uri downloadURL, string savePath)
        {
            isDownloadComplete = false;
            using (WebClient webClient = new WebClient()) {

                webClient.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e) {
                    progressBar1.Value = e.ProgressPercentage;
                };

                webClient.DownloadFileCompleted += delegate (object sender, AsyncCompletedEventArgs e) {
                    if (e.Cancelled) {
                        File.Delete(savePath);
                    }
                    isDownloadComplete = true;
                };

                webClient.DownloadFileAsync(downloadURL, savePath); // begin download

                while(!isDownloadComplete) {
                    Application.DoEvents(); // TODO: causes high CPU usage!
                }
            }
        }
    }
}

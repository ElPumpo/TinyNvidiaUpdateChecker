using System;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    public partial class LocationChooserForm : Form
    {

        string selectedLanguageCode;
        string[] locationLanguageCode = { "cn", "de", "uk", "us", "es", "fr", "it", "jp", "kr", "pl", "tr", "ru"};
        string[] locationLabels = { "China", "Germany", "United Kingdom", "United States", "Spain", "France", "Italy", "Japan", "Korea", "Poland", "Turkish", "Russia" };

        public LocationChooserForm()
        {
            InitializeComponent();
        }

        public string OpenLocationChooserForm()
        {
            if (!MainConsole.confirmDL) {
                selectedLanguageCode = "uk";
                ShowDialog();

                return selectedLanguageCode;
            } else {
                return "uk";
            }
        }

        private void LocationChooserForm_Load(object sender, EventArgs e)
        {
            var index = 0;

            foreach (var item in locationLabels)
            {
                var list = new ListViewItem(item);
                var child = locationListView.Items.Add(list).Name = locationLanguageCode[index];
                index++;
            }
        }

        private void ConfirmBtn_Click(object sender, EventArgs e)
        {
            if (locationListView.SelectedIndices.Count == 1) {
                selectedLanguageCode = locationListView.SelectedItems[0].Name;
                Close();
            }
        }
    }
}

using System;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    public partial class LocationChooserForm : Form
    {

        string selectedLanguageCode;
        string[] locationLanguageCode = ["cn", "de", "uk", "us", "es", "fr", "it", "jp", "kr", "pl", "tr", "ru"];
        string[] locationLabels = ["China", "Germany", "United Kingdom", "United States", "Spain", "France", "Italy", "Japan", "Korea", "Poland", "Turkish", "Russia"];

        public LocationChooserForm()
        {
            InitializeComponent();
        }

        public string OpenForm()
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
            int i = 0;

            foreach (var item in locationLabels) {
                var child = comboBox.Items.Add(locationLabels[i]);
                i++;
            }
        }

        private void ConfirmBtn_Click(object sender, EventArgs e)
        {
            selectedLanguageCode = locationLanguageCode[comboBox.SelectedIndex];
            Close();
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConfirmBtn.Enabled = true;
        }
    }
}

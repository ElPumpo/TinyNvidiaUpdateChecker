using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    public partial class GPUSelectorForm : Form
    {
        List<GPU> gpuList = null;
        Dictionary<int, GPU> kvGpus = [];

        public GPUSelectorForm()
        {
            InitializeComponent();
        }

        public (string, string, bool, bool) OpenForm(List<GPU> _gpuList)
        {
            gpuList = _gpuList;
            ShowDialog();

            GPU gpu = kvGpus[comboBox.SelectedIndex];
            bool overrideType = !radioButtonDefault.Checked;
            bool overrideIsDesktop = radioButtonDesktop.Checked;

            if (overrideType) {
                if (overrideIsDesktop && gpu.type == "desktop") {
                    overrideType = false;
                }

                if (radioButtonNotebook.Checked && gpu.type == "notebook") {
                    overrideType = false;
                }
            }

            return (gpu.id.ToString(), gpu.name, overrideType, overrideIsDesktop);
        }

        private void GPUSelectorForm_Load(object sender, EventArgs e)
        {
            foreach (var gpu in gpuList.Where(x => x.isValidated)) {
                int index = comboBox.Items.Add(gpu.name);
                kvGpus.Add(index, gpu);
            }
        }

        private void ConfirmBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            groupBoxType.Enabled = true;
            ConfirmBtn.Enabled = true;

            radioButtonDefault.Checked = true;
            GPU gpu = kvGpus[comboBox.SelectedIndex];
            radioButtonDefault.Text = $"Identified\n({gpu.getFormattedType()})";
        }
    }
}

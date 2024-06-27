﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    public partial class GPUSelectorForm : Form
    {
        List<GPU> gpuList = null;
        Dictionary<int, int> validatedList = [];

        public GPUSelectorForm() => InitializeComponent();

        public string OpenForm(List<GPU> _gpuList)
        {
            gpuList = _gpuList;
            ShowDialog();

            return validatedList[comboBox.SelectedIndex].ToString();
        }

        private void GPUSelectorForm_Load(object sender, EventArgs e)
        {
            foreach (var gpu in gpuList.Where(x => x.isValidated)) {
                int index = comboBox.Items.Add(gpu.name);
                validatedList.Add(index, gpu.id);
            }
        }

        private void ConfirmBtn_Click(object sender, EventArgs e) => Close();

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e) => ConfirmBtn.Enabled = true;
    }
}

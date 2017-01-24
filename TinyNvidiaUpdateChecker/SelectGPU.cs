using System;
using System.Management;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{

    /*
    TinyNvidiaUpdateChecker - Check for NVIDIA GPU drivers, GeForce Experience replacer
    Copyright (C) 2016-2017 Hawaii_Beach

    This program Is free software: you can redistribute it And/Or modify
    it under the terms Of the GNU General Public License As published by
    the Free Software Foundation, either version 3 Of the License, Or
    (at your option) any later version.

    This program Is distributed In the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty Of
    MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License For more details.

    You should have received a copy Of the GNU General Public License
    along with this program.  If Not, see <http://www.gnu.org/licenses/>.
    */

    public partial class SelectGPU : Form
    {
        private static string finalGPU = null;

        public SelectGPU() {
            InitializeComponent();
        }

        private void SelectGPU_Load(object sender, EventArgs e) {
            fetchGPU();
        }

        private void fetchGPU() {

            string description = null;
            foreach (ManagementObject managementObject in new ManagementObjectSearcher("SELECT * FROM Win32_VideoController").Get()) {
                description = null; // flush value
                description = managementObject.Properties["Description"].Value.ToString();

                // if the graphics card isn't a ghost
                if(description != null) {
                    GPUBox.Items.Add(description);
                }
            }

            // select the first NVIDIA gpu automaticlly, as a recommended choice. NEAT
            foreach(string item in GPUBox.Items) {
                if(item.Contains("NVIDIA")) {
                    GPUBox.SelectedItem = item;
                    break;
                }
            }

        }

        private void SelectBtn_Click(object sender, EventArgs e)
        {
            if(GPUBox.SelectedItem != null) {
                finalGPU = GPUBox.SelectedItem.ToString();
                Close(); // close window
            } else {
                MessageBox.Show("You haven't selected a GPU!", "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string getGPU() {
            finalGPU = null;

            // show the form
            using(SelectGPU form = new SelectGPU()) {
                form.ShowDialog();
            }

            return finalGPU; // return final value
        }

    }
}

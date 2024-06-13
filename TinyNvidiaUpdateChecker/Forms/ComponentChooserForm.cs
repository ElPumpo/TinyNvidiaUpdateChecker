using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TinyNvidiaUpdateChecker.Handlers;

namespace TinyNvidiaUpdateChecker.Forms
{
    public partial class ComponentChooserForm : Form
    {
        List<Component> componentList;
        List<string> choosenComponents = [];
        int driverIdx = 0;

        public ComponentChooserForm()
        {
            InitializeComponent();
        }

        public List<string> OpenForm(List<Component> _componentList)
        {
            componentList = _componentList;
            ShowDialog();

            return choosenComponents;
        }

        private void ComponentChooserForm_Load(object sender, System.EventArgs e)
        {
            foreach (Component component in componentList)
            {
                string label = component.label;
                int idx = checkedListBox.Items.Add(label);
                component.index = idx;

                if (component.name == "Display.Driver") { driverIdx = idx; }
            }
        }

        private void checkedListBox_SelectedValueChanged(object sender, System.EventArgs e)
        {
            Component comp = componentList.Where(x => x.index == checkedListBox.SelectedIndex).First();
            string description = ComponentHandler.GetComponentDescription(comp.name);

            if (comp.dependencies.Count > 0)
            {
                bool first = true;

                foreach (KeyValuePair<string, string> dependency in comp.dependencies)
                {
                    if (first)
                    {
                        first = false;
                        description += "\n\nRequires:\n";
                    }

                    description += $"{dependency.Value}\n";
                }
            }

            richTextBox.Text = description;
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            Enabled = false;
            choosenComponents.Clear();

            Dictionary<string, bool> dependencyList = new() {
                {"Display.Driver", checkedListBox.CheckedIndices.Contains(driverIdx)}
            };

            foreach (int idx in checkedListBox.CheckedIndices)
            {
                Component comp = componentList.Where(x => x.index >= idx).First();
                choosenComponents.Add(comp.name);

                foreach (KeyValuePair<string, string> dependency in comp.dependencies)
                {
                    dependencyList.TryAdd(dependency.Key, false);
                }
            }

            foreach (int idx in checkedListBox.CheckedIndices)
            {
                Component comp = componentList.Where(x => x.index >= idx).First();

                if (dependencyList.ContainsKey(comp.name))
                {
                    dependencyList[comp.name] = true;
                }
            }

            bool canProceed = dependencyList.Where(x => x.Value == true).Count() == dependencyList.Count;

            if (canProceed)
            {
                Close();
            }
            else
            {
                string missingComponents = string.Join("\n", dependencyList.Where(x => !x.Value).Select(x => ComponentHandler.GetComponentLabelFromName(x.Key)));
                string message = $"You are missing components, please review your selection.\n\nMissing required components:\n{missingComponents}";
                MessageBox.Show(message, "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Enabled = true;
        }

        private void noneLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            for (int i = 0; i < checkedListBox.Items.Count; i++)
            {
                checkedListBox.SetItemChecked(i, false);
            }

            richTextBox.Text = "";
        }

        private void allLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            for (int i = 0; i < checkedListBox.Items.Count; i++)
            {
                checkedListBox.SetItemChecked(i, true);
            }
        }
    }
}

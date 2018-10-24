using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Newtonsoft.Json;

namespace LaunchPanel
{
    public partial class ConfigDlg : Form
    {
        public ConfigDlg(Config config, string fn)
        {
            InitializeComponent();
            InitializeEvents(config, fn);
            InitializeGrid(config);

            // If we don't do this, the first row, which is selected by default, doesn't fire the SelectionChanged event.
            dgvGroups.ClearSelection();
            dgvGroups.Rows[0].Selected = true;
        }

        protected void InitializeEvents(Config config, string fn)
        {
            dgvGroups.SelectionChanged += (_, __) => GroupSelected((Group)dgvGroups.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault()?.DataBoundItem);
            dgvButtons.SelectionChanged += (_, __) => ButtonSelected((LaunchButton)dgvButtons.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault()?.DataBoundItem);
            btnSave.Click += (_, __) => Save(config, fn);
        }

        protected void InitializeGrid(Config config)
        {
            DataGridViewComboBoxColumn launcherSelection = new DataGridViewComboBoxColumn();
            launcherSelection.ValueType = typeof(LauncherType);
            launcherSelection.DataPropertyName = "Launcher";
            // launcherSelection.Name = "Launcher";
            launcherSelection.HeaderText = "Launcher";
            launcherSelection.DataSource = Enum.GetValues(typeof(LauncherType));

            var editColumn = new DataGridViewTextBoxColumn();
            editColumn.DataPropertyName = "Name";
            editColumn.HeaderText = "Group Name";

            dgvGroups.Columns.Add(editColumn);
            dgvGroups.Columns.Add(launcherSelection);
            dgvGroups.DataSource = new BindingList<Group>(config.Groups); 
        }

        protected void GroupSelected(Group group)
        {
            if (group == null) return;

            dgvButtons.AutoGenerateColumns = false;
            dgvButtons.Columns.Clear();

            //DataGridViewComboBoxColumn launcherSelection = new DataGridViewComboBoxColumn();
            //launcherSelection.ValueType = typeof(LauncherType);
            //launcherSelection.DataPropertyName = "Launcher";
            //launcherSelection.Name = "Launcher";
            //launcherSelection.HeaderText = "Override Launcher";
            //launcherSelection.DataSource = Enum.GetValues(typeof(LauncherType));

            dgvButtons.Columns.Add(GetEditor("Name"));
            //dgvButtons.Columns.Add(GetEditor("Path"));
            //dgvButtons.Columns.Add(GetEditor("BackColor", "Back. Color"));
            //dgvButtons.Columns.Add(GetEditor("TextColor", "Text Color"));
            //dgvButtons.Columns.Add(GetEditor("Size"));
            //dgvButtons.Columns.Add(GetEditor("Location"));
            //dgvButtons.Columns.Add(GetEditor("Size"));
            //dgvButtons.Columns.Add(GetEditor("WindowCaption", "Wnd Caption"));
            //dgvButtons.Columns.Add(launcherSelection);

            dgvButtons.DataSource = new BindingList<LaunchButton>(group.Buttons);

            if (group.Buttons.Count > 0)
            {
                dgvButtons.ClearSelection();
                dgvButtons.Rows[0].Selected = true;
            }
            else
            {
                pgLaunchButton.SelectedObject = null;
            }
        }

        protected void ButtonSelected(LaunchButton button)
        {
            if (button == null) return;

            pgLaunchButton.SelectedObject = button;
        }

        protected DataGridViewTextBoxColumn GetEditor(string propertyName, string caption = null)
        {
            var editor = new DataGridViewTextBoxColumn();
            editor.DataPropertyName = propertyName;
            editor.HeaderText = caption ?? propertyName;

            return editor;
        }

        protected void Save(Config config, string fn)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(fn, json);
        }
    }
}

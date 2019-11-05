using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using elbemu_shared.Configuration;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace elbsms_ui
{
    public partial class ConfigurationForm : Form
    {
        private readonly Dictionary<string, List<Control>> _propertyControls;

        public ISystemConfiguration Configuration { get; }

        public ConfigurationForm(ISystemConfiguration configuration, string title)
        {
            InitializeComponent();

            _propertyControls = new Dictionary<string, List<Control>>();

            Configuration = configuration.Clone();

            PrepareUserInterface(title);
        }

        private void AddControlForProperty(string propertyName, Control control)
        {
            List<Control> controls;

            if (!_propertyControls.TryGetValue(propertyName, out controls))
            {
                controls = new List<Control>();
                _propertyControls.Add(propertyName, controls);
            }

            controls.Add(control);
        }

        private void PrepareUserInterface(string title)
        {
            configPanel.SuspendLayout();
            SuspendLayout();

            Text = title;

            PropertyInfo[] properties = Configuration.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (IGrouping<string, PropertyInfo> category in GroupPropertiesByCategory(properties))
            {
                AddSettingsGroup(category.Key ?? "Miscellaneous", category.ToList());
            }

            ApplyControlDependencies(properties);

            configPanel.ResumeLayout(false);
            configPanel.PerformLayout();
            ResumeLayout();
        }

        private void ApplyControlDependencies(IEnumerable<PropertyInfo> properties)
        {
            var dependencies = properties.Select(x => new { PropertyName = x.Name, DependsOn = x.GetCustomAttribute<DependsOnAttribute>() }).Where(x => x.DependsOn != null);

            foreach (var dependency in dependencies)
            {
                // we can only bind to checkbox controls
                if (_propertyControls[dependency.DependsOn.PropertyName].First() is CheckBox checkBox)
                {
                    foreach (Control control in _propertyControls[dependency.PropertyName])
                    {
                        AddDataBinding(control, nameof(control.Enabled), checkBox, nameof(checkBox.Checked));
                    }
                }
            }
        }

        private IOrderedEnumerable<IGrouping<string, PropertyInfo>> GroupPropertiesByCategory(IEnumerable<PropertyInfo> properties)
        {
            // controls in the panel display in the reverse of the order added, last added at top,
            // reverse the list to maintain the order they're declared, then put the null key at the
            // beginning, adding it first so displaying last
            return properties.Select(p => new
            {
                p.GetCustomAttribute<CategoryAttribute>()?.Category,
                Property = p
            })
            .GroupBy(x => x.Category, x => x.Property)
            .Reverse()
            .OrderByDescending(x => x.Key == null);
        }

        private void AddSettingsGroup(string groupName, List<PropertyInfo> properties)
        {
            TableLayoutPanel tableLayoutPanel = CreateTableLayoutPanel(groupName);

            PopulateConfigurationTable(tableLayoutPanel, properties);

            // add a groupbox
            var groupBox = new GroupBox()
            {
                Text = groupName,
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(3, 3, 3, 6),
            };

            groupBox.Controls.Add(tableLayoutPanel);

            // add a panel surrounding the groupbox to allow padding to be added to the bottom,
            // docking within a panel doesn't support margin
            var paddingPanel = new Panel()
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 0, 6),
            };

            paddingPanel.Controls.Add(groupBox);

            configPanel.Controls.Add(paddingPanel);
        }

        private static TableLayoutPanel CreateTableLayoutPanel(string groupName)
        {
            var tableLayoutPanel = new TableLayoutPanel()
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 4,
                Dock = DockStyle.Top,
                RowCount = 1,
                Name = $"{groupName}TableLayoutPanel",
            };

            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());

            tableLayoutPanel.RowStyles.Add(new RowStyle());
            return tableLayoutPanel;
        }

        private void AddDataBinding(IBindableComponent component, string propertyName, object dataSource, string dataMember,
            bool formattingEnabled = false, DataSourceUpdateMode updateMode = DataSourceUpdateMode.OnPropertyChanged)
        {
            component.DataBindings.Add(propertyName, dataSource, dataMember, formattingEnabled, updateMode);
        }

        private void PopulateConfigurationTable(TableLayoutPanel tableLayoutPanel, List<PropertyInfo> properties)
        {
            tableLayoutPanel.RowCount = properties.Count;

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyInfo property = properties[i];

                // if the property has a description attribute then use that, otherwise use the property name
                string description = property.GetCustomAttribute<DescriptionAttribute>()?.Description ?? property.Name;

                if (property.PropertyType == typeof(bool))
                {
                    AddCheckBox(tableLayoutPanel, i, property.Name, description);
                }
                else
                {
                    AddLabel(tableLayoutPanel, i, description);

                    if (property.PropertyType.BaseType == typeof(Enum))
                    {
                        AddComboBox(tableLayoutPanel, i, property.PropertyType, property.Name);
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        TextBox textBox = AddTextBox(tableLayoutPanel, i, property.Name);

                        BrowseAttribute pathAttribute;
                        if ((pathAttribute = property.GetCustomAttribute<BrowseAttribute>()) != null)
                        {
                            textBox.ReadOnly = true;
                            AddBrowseAndClearButtonsForTextBox(tableLayoutPanel, i, textBox, pathAttribute.PathType, property.Name);
                        }
                        else
                        {
                            tableLayoutPanel.SetColumnSpan(textBox, 3);
                        }
                    }
                }
            }
        }

        private void AddCheckBox(TableLayoutPanel tableLayoutPanel, int row, string propertyName, string description)
        {
            var checkBox = new CheckBox()
            {
                Name = $"{propertyName}CheckBox",
                Text = description,
                Dock = DockStyle.Fill,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(6, 3, 3, 3),
            };

            AddControlForProperty(propertyName, checkBox);

            AddDataBinding(checkBox, nameof(checkBox.Checked), Configuration, propertyName);
            tableLayoutPanel.Controls.Add(checkBox, 0, row);
            tableLayoutPanel.SetColumnSpan(checkBox, 4);
        }

        private void AddLabel(TableLayoutPanel tableLayoutPanel, int row, string description)
        {
            var label = new Label()
            {
                Text = description,
                Dock = DockStyle.Fill,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
            };

            tableLayoutPanel.Controls.Add(label, 0, row);
        }

        private TextBox AddTextBox(TableLayoutPanel tableLayoutPanel, int row, string propertyName)
        {
            var textBox = new TextBox()
            {
                Name = $"{propertyName}TextBox",
                Dock = DockStyle.Fill,
                TextAlign = HorizontalAlignment.Left,
            };

            AddControlForProperty(propertyName, textBox);

            AddDataBinding(textBox, nameof(textBox.Text), Configuration, propertyName);
            tableLayoutPanel.Controls.Add(textBox, 1, row);

            return textBox;
        }

        private void AddBrowseAndClearButtonsForTextBox(TableLayoutPanel tableLayoutPanel, int row, TextBox textBox, PathType pathType, string propertyName)
        {
            var browseButton = new Button() { Name = $"{propertyName}Browse", Text = "...", ClientSize = new Size(25, textBox.Height), Tag = textBox };
            switch (pathType)
            {
                case PathType.File:
                    browseButton.Click += (s, ev) =>
                    {
                        var tb = (TextBox)((Button)s).Tag;
                        string initialPath = !string.IsNullOrEmpty(tb.Text) ? Path.GetDirectoryName(tb.Text) : string.Empty;
                        tb.Text = DisplayOpenDialog(initialPath, false);
                    };
                    break;

                case PathType.Folder:
                    browseButton.Click += (s, ev) =>
                    {
                        var tb = (TextBox)((Button)s).Tag;
                        tb.Text = DisplayOpenDialog(tb.Text, true);
                    };
                    break;
            }

            var clearButton = new Button() { Name = $"{propertyName}Clear", Text = "Clear", ClientSize = new Size(40, textBox.Height), Tag = textBox };
            clearButton.Click += (s, ev) => ((TextBox)((Button)s).Tag).Text = string.Empty;

            AddControlForProperty(propertyName, browseButton);
            AddControlForProperty(propertyName, clearButton);

            tableLayoutPanel.Controls.Add(browseButton, 2, row);
            tableLayoutPanel.Controls.Add(clearButton, 3, row);
        }

        private void AddComboBox(TableLayoutPanel tableLayoutPanel, int row, Type enumType, string propertyName)
        {
            var comboBox = new ComboBox()
            {
                Name = $"{propertyName}ComboBox",
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Text",
                ValueMember = "Value",
            };

            comboBox.DataSource = Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(e => new { Name = Enum.GetName(enumType, e), Value = e })
                .Select(x => new
                {
                    Text = enumType.GetField(x.Name).GetCustomAttribute<DescriptionAttribute>()?.Description ?? x.Name,
                    x.Value
                }).ToList();

            AddControlForProperty(propertyName, comboBox);

            AddDataBinding(comboBox, nameof(comboBox.SelectedValue), Configuration, propertyName);

            tableLayoutPanel.Controls.Add(comboBox, 1, row);
            tableLayoutPanel.SetColumnSpan(comboBox, 3);
        }

        private string DisplayOpenDialog(string path, bool isFolderPicker)
        {
            using (var openFileDialog = new CommonOpenFileDialog() { InitialDirectory = path, IsFolderPicker = isFolderPicker })
            {
                if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    path = openFileDialog.FileName;
                }
            }

            return path;
        }
    }
}

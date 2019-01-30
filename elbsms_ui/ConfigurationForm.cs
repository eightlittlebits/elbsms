using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace elbsms_ui
{
    public partial class ConfigurationForm : Form
    {
        private IConfiguration _config;

        public IConfiguration Configuration { get { return _config; } }

        public ConfigurationForm(IConfiguration configuration, string title)
        {
            InitializeComponent();

            _config = configuration.Clone();

            PrepareUserInterface(title);
        }

        private void PrepareUserInterface(string title)
        {
            Text = title;

            var properties = _config.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();

            PopulateConfigurationTable(properties);
        }

        private void AddDataBinding(IBindableComponent component, string propertyName, object dataSource, string dataMember,
            bool formattingEnabled = false, DataSourceUpdateMode updateMode = DataSourceUpdateMode.OnPropertyChanged)
        {
            component.DataBindings.Add(propertyName, dataSource, dataMember, formattingEnabled, updateMode);
        }

        private void PopulateConfigurationTable(List<PropertyInfo> properties)
        {
            configTableLayoutPanel.RowCount = properties.Count;

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyInfo property = properties[i];

                // if the property has a description attribute then use that, otherwise use the property name
                string description = property.GetCustomAttribute<DescriptionAttribute>()?.Description ?? property.Name;

                if (property.PropertyType == typeof(bool))
                {
                    AddCheckBox(i, property.Name, description);
                }
                else
                {
                    AddLabel(i, description);

                    if (property.PropertyType.BaseType == typeof(Enum))
                    {
                        AddComboBox(i, property.PropertyType, property.Name);
                    }
                }
            }
        }

        private void AddCheckBox(int row, string propertyName, string description)
        {
            var checkBox = new CheckBox()
            {
                Text = description,
                Dock = DockStyle.Fill,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(6, 3, 3, 3),
            };

            AddDataBinding(checkBox, nameof(checkBox.Checked), _config, propertyName);
            configTableLayoutPanel.Controls.Add(checkBox, 0, row);
            configTableLayoutPanel.SetColumnSpan(checkBox, 4);
        }

        private void AddLabel(int row, string description)
        {
            var label = new Label()
            {
                Text = description,
                Dock = DockStyle.Fill,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
            };

            configTableLayoutPanel.Controls.Add(label, 0, row);
        }

        private void AddComboBox(int row, Type enumType, string propertyName)
        {
            var comboBox = new ComboBox()
            {
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

            AddDataBinding(comboBox, nameof(comboBox.SelectedValue), _config, propertyName);

            configTableLayoutPanel.Controls.Add(comboBox, 1, row);
            configTableLayoutPanel.SetColumnSpan(comboBox, 3);
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

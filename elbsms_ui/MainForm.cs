﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using elb_utilities.WinForms;

namespace elbsms_ui
{
    public partial class MainForm : Form
    {
        private static double _stopwatchFrequency = Stopwatch.Frequency;
        private static string _programNameVersion = $"{Application.ProductName} v{Application.ProductVersion}";

        private Configuration _config;

        private NotifyValue<bool> _emulationInitialised;

        private bool _emulationPaused;
        private bool _focusLostPauseState;

        public bool Paused
        {
            get => _emulationPaused;
            set { _emulationPaused = value; SetUIText(); }
        }

        public MainForm()
        {
            InitializeComponent();

            _config = Configuration.Load();

            _emulationInitialised = new NotifyValue<bool>(false);

            PrepareUserInterface();
            PrepareDataBindings();
        }

        private void PrepareUserInterface()
        {
            SetUIText();
        }

        private void SetUIText()
        {
            Text = _programNameVersion + (_emulationPaused ? "[PAUSED]" : string.Empty);

            aboutToolStripMenuItem.Text = $"About {Application.ProductName}";

            if (!_emulationInitialised)
            {
                statusToolStripStatusLabel.Text = "Ready";
            }
            else
            {
                statusToolStripStatusLabel.Text = _emulationPaused ? "Paused" : "Running";
            }
        }

        private void PrepareDataBindings()
        {
            void AddBinding(IBindableComponent component, string propertyName, object dataSource, string dataMember, bool formattingEnabled = false, DataSourceUpdateMode updateMode = DataSourceUpdateMode.OnPropertyChanged)
            {
                component.DataBindings.Add(propertyName, dataSource, dataMember, formattingEnabled, updateMode);
            }

            // emulation menu items enable
            AddBinding(pauseToolStripMenuItem, nameof(pauseToolStripMenuItem.Enabled), _emulationInitialised, nameof(_emulationInitialised.Value));
            AddBinding(resetToolStripMenuItem, nameof(resetToolStripMenuItem.Enabled), _emulationInitialised, nameof(_emulationInitialised.Value));

            // pause
            AddBinding(pauseToolStripMenuItem, nameof(pauseToolStripMenuItem.Checked), this, nameof(Paused));

            // options
            AddBinding(limitFrameRateToolStripMenuItem, nameof(limitFrameRateToolStripMenuItem.Checked), _config, nameof(_config.LimitFrameRate));
            AddBinding(pauseWhenFocusLostToolStripMenuItem, nameof(pauseWhenFocusLostToolStripMenuItem.Checked), _config, nameof(_config.PauseWhenFocusLost));
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (_config.PauseWhenFocusLost)
            {
                Paused = _focusLostPauseState;
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            if (_config.PauseWhenFocusLost && _emulationInitialised)
            {
                _focusLostPauseState = Paused;
                Paused = true;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _config.Save();

            base.OnFormClosing(e);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDialog.DisplayAboutDialog();
        }
    }
}

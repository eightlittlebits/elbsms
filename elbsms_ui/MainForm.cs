using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using elb_utilities.NativeMethods;
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

        private double _targetFrameTicks = 0;
        private long _lastFrameTimestamp;

        public bool Paused
        {
            get => _emulationPaused;
            set { _emulationPaused = value; SetUIText(); }
        }

        static bool ApplicationStillIdle => !User32.PeekMessage(out _, IntPtr.Zero, 0, 0, 0);

        public MainForm()
        {
            InitializeComponent();

            _config = Configuration.Load();

            _emulationInitialised = new NotifyValue<bool>(false);

            PrepareUserInterface();

            Application.Idle += (s, ev) => { while (_emulationInitialised && !_emulationPaused && ApplicationStillIdle) { RunFrame(); } };
        }

        private void PrepareUserInterface()
        {
            SetUIText();
            PrepareDataBindings();
        }

        private void SetUIText()
        {
            Text = _programNameVersion + (_emulationPaused ? " [PAUSED]" : string.Empty);

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
            void AddBinding(IBindableComponent component, string propertyName, object dataSource, string dataMember,
                bool formattingEnabled = false, DataSourceUpdateMode updateMode = DataSourceUpdateMode.OnPropertyChanged)
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

            // background settings
            AddBinding(this, nameof(this.Size), _config, nameof(_config.WindowSize));
        }

        private void RunFrame()
        {
            // run frame

            // render

            //sleep
            long elapsedTicks = Stopwatch.GetTimestamp() - _lastFrameTimestamp;

            if (_config.LimitFrameRate && elapsedTicks < _targetFrameTicks)
            {
                // get ms to sleep for, cast to int to truncate to nearest millisecond
                // take 1 ms off the sleep time as we don't always hit the sleep exactly, trade
                // burning extra cpu in the spin loop for accuracy
                int sleepMilliseconds = (int)((_targetFrameTicks - elapsedTicks) * 1000 / _stopwatchFrequency) - 1;

                if (sleepMilliseconds > 0)
                {
                    Thread.Sleep(sleepMilliseconds);
                }

                // spin for the remaining partial millisecond to hit target frame rate
                while ((Stopwatch.GetTimestamp() - _lastFrameTimestamp) < _targetFrameTicks) ;
            }

            _lastFrameTimestamp = Stopwatch.GetTimestamp();
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

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            e.Effect = DragDropEffects.None;

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.AllowedEffect.HasFlag(DragDropEffects.Copy))
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] strings && strings.Length == 1)
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            if (e.Data.GetData(DataFormats.FileDrop, false) is string[] files && files.Length == 1)
            {
                // validate file and load
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDialog.DisplayAboutDialog();
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}

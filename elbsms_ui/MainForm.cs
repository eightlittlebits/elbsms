using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using elb_utilities.Addins;
using elb_utilities.WinForms;
using elbemu_shared.Audio;
using elbsms_ui.NativeMethods;

namespace elbsms_ui
{
    public partial class MainForm : Form
    {
        private static readonly double _stopwatchFrequency = Stopwatch.Frequency;
        private static readonly string _programNameVersion = $"{Application.ProductName} v{Application.ProductVersion}";

        private Configuration _config;
        private RecentFileList _recentFiles;

        private readonly List<Type> _audioDeviceTypes;
        private IAudioDevice _audioDevice;

        private long _lastFrameTimestamp;
        private double _targetFrameTicks = 0;

        private NotifyValue<bool> _emulationInitialised = new NotifyValue<bool>();
        private bool _emulationPaused;
        private bool _focusLostPauseState;

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

            _audioDeviceTypes = LoadDevicesOfType<IAudioDevice>();

            PrepareUserInterface();

            InitialiseAudioDevice(_config.AudioDeviceType);

            Application.Idle += (s, ev) => { while (_emulationInitialised && !_emulationPaused && ApplicationStillIdle) { RunFrame(); } };
        }

        private List<Type> LoadDevicesOfType<T>()
        {
            var devices = new List<Type>();

            // get currently loaded assemblies excluding any in the GAC and any dynamically generated assemblies (xmlserlializer etc)
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.GlobalAssemblyCache && !a.IsDynamic).ToList();

#if DEBUG
            // this might be loaded if we've used the debugger before we reach this point
            loadedAssemblies.RemoveAll(x => x.FullName.StartsWith("Microsoft.VisualStudio.Debugger.Runtime"));
#endif

            // load any matching types in the currently loaded assemblies
            devices.AddRange(AddinLoader.GetImplementationsFromAssemblies<T>(loadedAssemblies));

            // load components from the plugins directory
            devices.AddRange(AddinLoader.Load<T>(Program.PluginsDirectory));

            return devices;
        }

        private void PrepareUserInterface()
        {
            SetUIText();
            PrepareDataBindings();
            PrepareRecentFiles();

            PopulateDeviceMenu(audioDeviceToolStripMenuItem, _audioDeviceTypes, InitialiseAudioDevice);
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

            // runtime bindings
            AddBinding(pauseToolStripMenuItem, nameof(pauseToolStripMenuItem.Checked), this, nameof(Paused));

            AddBinding(pauseToolStripMenuItem, nameof(pauseToolStripMenuItem.Enabled), _emulationInitialised, nameof(_emulationInitialised.Value));
            AddBinding(resetToolStripMenuItem, nameof(resetToolStripMenuItem.Enabled), _emulationInitialised, nameof(_emulationInitialised.Value));

            // configuration bindings
            AddBinding(this, nameof(this.Size), _config, nameof(_config.WindowSize));

            AddBinding(limitFrameRateToolStripMenuItem, nameof(limitFrameRateToolStripMenuItem.Checked), _config, nameof(_config.LimitFrameRate));
            AddBinding(pauseWhenFocusLostToolStripMenuItem, nameof(pauseWhenFocusLostToolStripMenuItem.Checked), _config, nameof(_config.PauseWhenFocusLost));
        }

        private void PrepareRecentFiles()
        {
            _recentFiles = new RecentFileList(recentFilesToolStripMenuItem, _config.RecentFiles);
            _recentFiles.RecentFileSelected += recentFiles_RecentFileSelected;
        }

        private void PopulateDeviceMenu(ToolStripMenuItem menuItem, List<Type> deviceTypes, Action<Type> initialiseAction)
        {
            foreach (var deviceType in deviceTypes)
            {
                var deviceMenuItem = new ToolStripMenuItem(deviceType.Name) { Tag = deviceType, CheckOnClick = true };
                deviceMenuItem.Click += (s, e) =>
                {
                    initialiseAction((Type)((ToolStripMenuItem)s).Tag);
                };

                menuItem.DropDownItems.Add(deviceMenuItem);
            }
        }

        private void InitialiseAudioDevice(Type audioDeviceType)
        {
            _config.AudioDeviceType = audioDeviceType;

            if (_audioDevice != null)
            {
                _audioDevice.Stop();
                _audioDevice.Dispose();
            }

            _audioDevice = (IAudioDevice)Activator.CreateInstance(audioDeviceType, this.Handle, 48000);
            _audioDevice.Play();

            foreach (ToolStripMenuItem menuItem in audioDeviceToolStripMenuItem.DropDownItems)
            {
                menuItem.Checked = (Type)menuItem.Tag == audioDeviceType;
            }
        }

        private T ShowConfigurationForm<T>(T configuration, string systemName) where T : SystemConfiguration<T>, new()
        {
            using var configForm = new ConfigurationForm<T>(configuration, $"{systemName} Configuration");

            if (configForm.ShowDialog() == DialogResult.OK)
            {
                return configForm.Configuration;
            }

            return configuration;
        }

        private void RunFrame()
        {
            // run frame

            // render
            _audioDevice.QueueAudio();

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();

                if (_audioDevice != null)
                {
                    _audioDevice.Stop();
                    _audioDevice.Dispose();
                }
            }

            base.Dispose(disposing);
        }

#pragma warning disable IDE1006 // Naming Styles

        private void recentFiles_RecentFileSelected(object sender, RecentFileSelectedEventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDialog.DisplayAboutDialog();
        }

        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

#pragma warning restore IDE1006 // Naming Styles
    }
}

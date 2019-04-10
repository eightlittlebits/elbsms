using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace  elb_utilities.WinForms
{
    public class RecentFileSelectedEventArgs : EventArgs
    {
        public string Filename { get; set; }
    }

    public class RecentFileList
    {
        private readonly ToolStripMenuItem _menuItem;
        private readonly List<string> _recentFiles;
        private readonly int _numEntries;

        public event EventHandler<RecentFileSelectedEventArgs> RecentFileSelected;

        public RecentFileList(ToolStripMenuItem menuItem, List<string> recentFiles, int numEntries = 10)
        {
            _menuItem = menuItem;
            _recentFiles = recentFiles;
            _numEntries = numEntries;

            UpdateMenuItem();
        }

        public void AddRecentFile(string filename)
        {
            // remove and insert to place at top
            _recentFiles.Remove(filename);
            _recentFiles.Insert(0, filename);

            // only store the top _numEntries (10) entries
            if (_recentFiles.Count > _numEntries) _recentFiles.RemoveAt(_numEntries);

            UpdateMenuItem();
        }

        private void RemoveRecentFile(string filename)
        {
            _recentFiles.Remove(filename);
            UpdateMenuItem();
        }

        private void UpdateMenuItem()
        {
            // remove existing entries
            _menuItem.DropDownItems.Clear();

            _menuItem.Enabled = _recentFiles.Count > 0;

            if (_menuItem.Enabled)
            {
                foreach (var recent in _recentFiles.Select((filename, index) => new { Index = index, Filename = filename }))
                {
                    var menuItem = new ToolStripMenuItem($"&{recent.Index} {recent.Filename}")
                    {
                        Tag = recent.Filename
                    };

                    menuItem.Click += (s, ev) =>
                    {
                        string filePath = (string)((ToolStripMenuItem)s).Tag;

                        if (!File.Exists(filePath))
                        {
                            if (MessageBox.Show($"{filePath} not found, remove from recent files?", "File not found", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                            {
                                RemoveRecentFile(filePath);
                            }
                        }
                        else
                            RecentFileSelected?.Invoke(this, new RecentFileSelectedEventArgs() { Filename = filePath });
                    };

                    _menuItem.DropDownItems.Add(menuItem);
                }
            }
        }
    }
}
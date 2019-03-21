using System;
using System.Windows.Forms;
using elbsms_ui.NativeMethods;

namespace elbsms_ui
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // set timer resolution to 1ms to try and get the sleep accurate in the wait loop
            WinMM.TimeBeginPeriod(1);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}

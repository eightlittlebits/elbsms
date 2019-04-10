using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using elb_utilities.NativeMethods;

namespace elbsms_ui
{
    static class Program
    {
        public const string PluginsDirectory = @".\plugins";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            // set timer resolution to 1ms to try and get the sleep accurate in the wait loop
            WinMM.TimeBeginPeriod(1);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        // https://weblog.west-wind.com/posts/2016/Dec/12/Loading-NET-Assemblies-out-of-Seperate-Folders
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string FindFileInPath(string path, string filename)
            {
                return Directory.EnumerateFiles(path, filename, SearchOption.AllDirectories).FirstOrDefault();
            }

            Debug.WriteLine($"{nameof(CurrentDomain_AssemblyResolve)} - {args.Name} {args.RequestingAssembly}");

            // Ignore missing resources
            if (args.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            // Try to load by filename - split out the filename of the full assembly name
            string pluginFilename = args.Name.Split(',')[0] + ".dll";

            // recursively check the plugins directory
            string assemblyLocation = FindFileInPath(PluginsDirectory, pluginFilename);

            if (!string.IsNullOrEmpty(assemblyLocation))
            {
                return Assembly.LoadFrom(assemblyLocation);
            }

            return null;
        }
    }
}

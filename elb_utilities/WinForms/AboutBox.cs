using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace elb_utilities.WinForms
{
    public class AboutDialog
    {
        public static void DisplayAboutDialog()
        {
            var assembly = Assembly.GetEntryAssembly();

            var configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? string.Empty;
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty;
            var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;

            var about = new StringBuilder();

            about.AppendFormat("{0} - Version {1} {2}", Application.ProductName, Application.ProductVersion, configuration);
            about.AppendLine();
            about.AppendLine();
            about.AppendLine(description);
            about.AppendLine();
            about.AppendLine(copyright);

            MessageBox.Show(about.ToString(), $"About {Application.ProductName}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

using System.ComponentModel;
using elbemu_shared.Configuration;

namespace elbsms_core
{
    public enum SystemRegion
    {
        [Description("Domestic (Japan)")]
        Domestic,
        [Description("Export (Rest of World")]
        Export
    }

    public enum VideoStandard
    {
        NTSC,
        PAL
    }

    public class MasterSystemConfiguration : SystemConfiguration<MasterSystemConfiguration>
    {
        protected override string FileName => "smsconfig.xml";

        [Category("Boot ROM"), Description("Use Boot ROM")]
        public bool UseBootRom { get; set; }
        [Category("Boot ROM"), Description("Boot ROM Location"), DependsOn(nameof(UseBootRom)), Browse(PathType.File)]
        public string BootRomPath { get; set; }

        [Category("Region"), Description("System Region")]
        public SystemRegion Region { get; set; }
        [Category("Region"), Description("Video Standard")]
        public VideoStandard VideoStandard { get; set; }
    }
}

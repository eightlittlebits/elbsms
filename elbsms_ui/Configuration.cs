using elb_utilities.Configuration;

namespace elbsms_ui
{
    public class Configuration : XmlConfiguration
    {
        protected override string FileName => "elbsms.config";

        public bool LimitFrameRate { get; set; }
    }
}

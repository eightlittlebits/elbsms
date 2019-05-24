using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;
using elb_utilities.Configuration;

namespace elbsms_ui
{
    public class Configuration : XmlConfiguration<Configuration>
    {
        protected override string FileName => "elbsms.xml";

        public Size WindowSize { get; set; } = new Size(561, 518);

        public bool LimitFrameRate { get; set; } = true;
        public bool PauseWhenFocusLost { get; set; } = true;

        [XmlArrayItem(ElementName = "RecentFile")]
        public List<string> RecentFiles { get; set; } = new List<string>();

        [XmlIgnore]
        public Type AudioDeviceType { get; set; } = typeof(Audio.NullAudioDevice);

        [XmlElement(ElementName = nameof(AudioDeviceType))]
        public string AudioDeviceAssemblyQualifiedName
        {
            get => AudioDeviceType.AssemblyQualifiedName;
            set => AudioDeviceType = Type.GetType(value);
        }
    }
}

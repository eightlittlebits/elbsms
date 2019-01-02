using System;
using System.IO;
using System.Xml.Serialization;

namespace elb_utilities.Configuration
{
    // Inspired by ConfigFile.cs, https://github.com/xdanieldzd/MasterFudgeMk2
    [Serializable]
    public abstract class XmlConfiguration 
    {
        [XmlIgnore]
        protected virtual string FileName { get; }

        public XmlConfiguration()
        {
            FileName = GetType().Name + ".xml";
        }

        public void Save()
        {
            Save(FileName);
        }

        public void Save(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                new XmlSerializer(GetType()).Serialize(stream, this);
            }
        }

        public static T Load<T>() where T : XmlConfiguration, new()
        {
            return Load<T>(new T().FileName);
        }

        public static T Load<T>(string filename) where T : XmlConfiguration, new()
        {
            T configuration;

            if (File.Exists(filename))
            {
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    configuration = (T)new XmlSerializer(typeof(T)).Deserialize(stream);
                }
            }
            else
            {
                configuration = new T();
                configuration.Save(filename);
            }

            return configuration;
        }
    }
}

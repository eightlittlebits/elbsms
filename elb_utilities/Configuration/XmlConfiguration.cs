using System;
using System.IO;
using System.Xml.Serialization;

namespace elb_utilities.Configuration
{
    // Inspired by ConfigFile.cs, https://github.com/xdanieldzd/MasterFudgeMk2
    [Serializable]
    public abstract class XmlConfiguration<T> where T : XmlConfiguration<T>, new()
    {
        [XmlIgnore]
        protected virtual string FileName => typeof(T).Name + ".xml";

        public void Save()
        {
            Save(FileName);
        }

        public void Save(string filename)
        {
            using var stream = new FileStream(filename, FileMode.Create);
            new XmlSerializer(typeof(T)).Serialize(stream, this);
        }

        public static T Load()
        {
            return Load(new T().FileName);
        }

        public static T Load(string filename)
        {
            T configuration;

            if (File.Exists(filename))
            {
                using var stream = new FileStream(filename, FileMode.Open);
                configuration = (T)new XmlSerializer(typeof(T)).Deserialize(stream);
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

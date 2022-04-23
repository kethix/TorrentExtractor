using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace TorrentExtractor
{
    public class Configuration
    {
        public ConfigXML ConfigXML { get; private set; }

        /// <summary>
        /// Initialize config object from xml file .\Config\Configuration.xml
        /// </summary>
        public Configuration()
        {
            XmlSerializer reader = new XmlSerializer(typeof(ConfigXML));
            try
            {
                using (StreamReader xmlfile = new StreamReader(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Config.xml")))
                    ConfigXML = (ConfigXML)reader.Deserialize(xmlfile);
            }
            catch (Exception ex)
            {
                Program.Logger.Info("*** *** *** *** *** *** *** *** *** *** *** *** *** ***");
                Program.Logger.Error(string.Concat(ex.HResult, " ", ex.Message));
                Environment.Exit(ex.HResult);
            }
        }
    }
}

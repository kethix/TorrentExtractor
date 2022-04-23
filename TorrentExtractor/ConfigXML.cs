using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace TorrentExtractor
{
    public class ConfigXML
    {
        [XmlElement("Folders")]
        public Folders Folders { get; set; }
        [XmlElement("FileFormats")]
        public FileFormats FileFormats { get; set; }
        [XmlElement("ExcludedFolders")]
        public ExcludedFolders ExcludedFolders { get; set; }
    }

    public class Folders
    {
        [XmlElement("Folder")]
        public List<Folder> Folder { get; set; }
    }

    public class Folder
    {
        [XmlAttribute("Category")]
        public string Categorie { get; set; }
        [XmlAttribute("Type")]
        public string Type { get; set; }
        [XmlText]
        public string Path { get; set; }
    }

    public class ExcludedFolders
    {
        [XmlElement("ExcludedFolder")]
        public List<ExcludedFolder> ExcludedFolder { get; set; }
    }

    public class ExcludedFolder
    {
        [XmlText]
        public string Name { get; set; }
    }

    public class FileFormats
    {
        [XmlElement("FileFormat")]
        public List<FileFormat> FileFormat { get; set; }
    }

    public class FileFormat
    {
        [XmlAttribute("Action")]
        public string Action { get; set; }
        [XmlText]
        public string Extension { get; set; }
    }
}
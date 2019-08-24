using System.Collections.Generic;
using System.Xml.Serialization;

namespace TorrentExtractor
{
    public class Config
    {
        [XmlElement("Extraction")]
        public Extraction Extraction { get; set; }
        [XmlElement("Logs")]
        public Logs Logs { get; set; }
    }

    public class Extraction
    {
        [XmlElement("Folders")]
        public Folders Folders { get; set; }
        [XmlElement("FileFormats")]
        public FileFormats FileFormats { get; set; }
    }

    public class Logs
    {
        [XmlElement("LogToFile")]
        public bool LogToFile { get; set; }
        [XmlElement("Level")]
        public int Level { get; set; }
        [XmlElement("File")]
        public string File { get; set; }
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
        [XmlAttribute("Season")]
        public bool Season { get; set; }
        [XmlAttribute("Subfolder")]
        public bool Subfolder { get; set; }
        [XmlText]
        public string Path { get; set; }
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
        public string Format { get; set; }
    }
}

using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace TorrentExtractor
{
    public class Torrent
    {
        public string SourceFullPath { get; private set; }
        public string SourceFolder { get; private set; }
        public List<string> FoldersToValidate { get; private set; }
        public string Categorie { get; private set; }
        public string DestinationFullPath { get; private set; }

        private Folder GetFolderLinkedToCategorie(string categorie, List<Folder> folders)
        {
            foreach (var folder in folders)
                if (folder.Categorie == categorie)
                    return folder;
            throw (new Exception("Categorie not configured in Config.xml"));
        }

        public Torrent(string sourceFullPath, string categorie, Configuration configuration)
        {
            SourceFullPath = sourceFullPath;
            SourceFolder = new DirectoryInfo(SourceFullPath).Name.ToLower();
            Categorie = categorie;
            TextInfo info = new CultureInfo("en-CA", false).TextInfo;
            FoldersToValidate = new List<string>();
            Folder folder = GetFolderLinkedToCategorie(categorie, configuration.ConfigXML.Folders.Folder);

            if (File.GetAttributes(sourceFullPath).HasFlag(FileAttributes.Directory))
            {
                FoldersToValidate.Add(sourceFullPath);
                IEnumerable<string> foldersInSourcePath = Directory.EnumerateDirectories(SourceFullPath, "*", SearchOption.AllDirectories);
                if (foldersInSourcePath.Count() > 0)
                    foreach (string folderInSourcePath in foldersInSourcePath)
                        if (configuration.ConfigXML.ExcludedFolders.ExcludedFolder.Where(x => x.Name.ToLower().Equals(Path.GetFileName(folderInSourcePath).ToLower())).Count() == 0)
                            FoldersToValidate.Add(folderInSourcePath);
            }

            if (folder.Type.ToLower().Equals("tv"))
            {
                // space \040       . \056      s \163      e \145      ( \050
                // .s##. or .s##e## or s##e##e##            same with 'space' instead of '.'
                if (Regex.Match(SourceFolder, @"[\040,\056]\163{1}\d{2}(\145\d{2}){1,2}[\040,\056]").Success)
                {
                    DestinationFullPath = Path.Combine(
                                            folder.Path,
                                            info.ToTitleCase(Regex.Match(SourceFolder, @".+(?=[\040,\056]\163{1}\d{2}(\145\d{2}){1,2}[\040,\056])").Value.Replace('.', ' ')),
                                            string.Concat("Season ", Regex.Match(SourceFolder, @"\163{1}\d{2}(\145\d{2}){1,2}").Value.Substring(1, 2).TrimStart(new Char[] { '0' }))
                                        );
                }
                // " season # " or " season ## "
                else if (Regex.Match(SourceFolder, @"[\040,\056]season \d{1,2}[\040,\056]").Success)
                {
                    DestinationFullPath = Path.Combine(
                                            folder.Path,
                                            info.ToTitleCase(Regex.Match(SourceFolder, @"[A-Z,a-z,\040]+(?=(\050)|([\040,\056]season \d{1,2}[\040,\056]))").Value.Trim().Replace('.', ' ')),
                                            info.ToTitleCase(Regex.Match(SourceFolder, @"[\040,\056]season \d{1,2}[\040,\056]").Value).Trim()
                                        );
                }
                // ".complete.series."
                else if (Regex.Match(SourceFolder, @"\056complete\056series\056").Success)
                {
                    DestinationFullPath = Path.Combine(
                        folder.Path,
                        info.ToTitleCase(Regex.Match(SourceFolder, @".+(?=\056complete\056series\056)").Value.Trim().Replace('.', ' '))
                    );
                }
                else
                    Environment.Exit(0);
            }
            else if (folder.Type.ToLower().Equals("movie"))
                DestinationFullPath = folder.Path;
            else
                Environment.Exit(0);

            if(!DestinationFullPath.EndsWith("\\"))
                DestinationFullPath = string.Concat(DestinationFullPath, "\\");
        }
    }
}

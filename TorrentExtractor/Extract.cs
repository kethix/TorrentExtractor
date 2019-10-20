using System;
using System.IO;
using System.Linq;
using System.Threading;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace TorrentExtractor
{
    /// <summary>
    /// Functions for extracting files
    /// </summary>
    class Extract
    {
        /// <summary>
        /// Scan files in a folder and take action on files if necessary
        /// </summary>
        /// <param name="folder">Folder containing the files</param>
        /// <param name="destination">Destination folder</param>
        /// <param name="config">Object that contains the Config.xml parameters</param>
        public static void TakeActionOnFolder(string folder, string destination, Config config)
        {
            foreach (var format in config.FileFormats.FileFormat)
            {
                var files = Directory.EnumerateFiles(folder, string.Format("*{0}", format.Extension), SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                    TakeActionOnFile(file, destination, format.Action);
            }
        }

        /// <summary>
        /// Take action on a specific file, either Copy or Extract
        /// </summary>
        /// <param name="file">File path</param>
        /// <param name="destination">Destination folder</param>
        /// <param name="action">Action to do on file</param>
        public static void TakeActionOnFile(string file, string destination, string action)
        {
            while (IsFileLocked(new FileInfo(file)))
                Thread.Sleep(2000);

            if (action.ToLower() == "copy")
            {
                if (!Directory.Exists(destination))
                    Directory.CreateDirectory(destination);
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), false);
            }
            else if (action.ToLower() == "extract")
            {
                using (var archive = ArchiveFactory.Open(file))
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(destination, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
            else
                throw new Exception("The action format configured in Config.xml is invalid. It should either be \"Copy\" or \"Extract\".");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="destination"></param>
        /// <param name="config">Object that contains the Config.xml parameters</param>
        public static void TakeActionOnFile(string file, string destination, Config config)
        {
            foreach (var format in config.FileFormats.FileFormat)
            {
                if (file.EndsWith(format.Extension))
                    TakeActionOnFile(file, destination, format.Action);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="config">Object that contains the Config.xml parameters</param>
        public static void UniExtract(string source, Tuple<string, bool> destination, Config config)
        {
            // If source is a folder
            if ((File.GetAttributes(source)).HasFlag(FileAttributes.Directory))
            {
                // Get all folders
                var folders = Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories);
                // For each folder in that source folder...
                if (folders.Count() > 0)
                {
                    foreach (var folder in folders)
                        // if the folder is not one of the excluded folders
                        if (config.ExcludedFolders.ExcludedFolder.Where(x => x.Name.ToLower().Contains(Path.GetFileName(folder).ToLower())).Count() == 0)
                        {
                            // Extract it in his own folder in subfolder destination if destination is for TV
                            if (destination.Item2)
                                TakeActionOnFolder(folder, Path.Combine(destination.Item1, Path.GetFileName(folder)), config);
                            else
                                TakeActionOnFolder(folder, destination.Item1, config);
                        }
                }
                // Validate files in source folders
                TakeActionOnFolder(source, destination.Item1, config);
            }
            else
                // Source is a file, take action on it
                TakeActionOnFile(source, destination.Item1, config);
        }

        /// <summary>
        /// Check if file is locked
        /// </summary>
        /// <param name="file">Info parameter</param>
        /// <returns></returns>
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch(IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return false;
        }
    }
}
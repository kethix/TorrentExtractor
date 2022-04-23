using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace TorrentExtractor
{
    /// <summary>
    /// Functions for extracting files
    /// </summary>
    class Extract
    {
        /// <summary>
        /// Take action on a specific file, either Copy or Extract
        /// </summary>
        /// <param name="file">File path</param>
        /// <param name="destination">Destination folder</param>
        /// <param name="action">Action to do on file</param>
        public static void TakeActionOnFile(string file, string destination, string action)
        {
            /*FileInfo f = new FileInfo(file);
            if (IsFileLocked(f))
            {
                Thread.Sleep(20000);
                if (IsFileLocked(f))
                    Environment.Exit(0);
            }*/

            if (action.ToLower() == "copy")
            {
                if (!Directory.Exists(destination))
                    Directory.CreateDirectory(destination);
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), false);
            }
            else if (action.ToLower() == "extract")
            {
                try
                {
                    using (Process exeProcess = new Process())
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo()
                        {
                            CreateNoWindow = false,
                            UseShellExecute = false,
                            FileName = Path.Combine(Environment.CurrentDirectory, "UnRAR", "UnRAR.exe"),
                            WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "UnRAR"),
                            WindowStyle = ProcessWindowStyle.Hidden,
                            Arguments = string.Concat("x -o+ -y \"", file, "\" \"", destination, "\""),
                            RedirectStandardOutput = true
                        };

                        exeProcess.StartInfo = startInfo;
                        exeProcess.Start();

                        Program.Logger.Info(exeProcess.StandardOutput.ReadToEnd());
                        
                        exeProcess.WaitForExit();
                    }
                }
                catch(Exception ex)
                {
                    Program.Logger.Error(string.Concat(ex.HResult, " ", ex.Message));
                    Environment.Exit(ex.HResult);
                }
            }
            else
                throw new Exception("The action format configured in Config.xml is invalid. It should either be \"Copy\" or \"Extract\".");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="config">Object that contains the Config.xml parameters</param>
        public static void DecideActionOnFiles(Torrent torrent, Configuration configuration)
        {
            // If source is a folder
            if (torrent.FoldersToValidate.Count > 0)
                foreach (string folder in torrent.FoldersToValidate)
                    foreach (FileFormat format in configuration.ConfigXML.FileFormats.FileFormat)
                    {
                        var files = Directory.EnumerateFiles(folder, string.Format("*{0}", format.Extension), SearchOption.TopDirectoryOnly);
                        foreach (string file in files)
                            TakeActionOnFile(file, torrent.DestinationFullPath, format.Action);
                    }
            else
                foreach (FileFormat format in configuration.ConfigXML.FileFormats.FileFormat)
                    if (torrent.SourceFullPath.EndsWith(format.Extension))
                        TakeActionOnFile(torrent.SourceFullPath, torrent.DestinationFullPath, format.Action);
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
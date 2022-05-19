using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
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
            Program.Logger.Info(string.Concat("                Action taken on file: ", action));

            if (action.ToLower() == "copy")
            {
                if (!Directory.Exists(destination))
                    Directory.CreateDirectory(destination);

                string filePath = Path.Combine(destination, Path.GetFileName(file));

                File.Copy(file, filePath, false);

                if (File.Exists(filePath))
                    Program.Logger.Info("File copied succesfully.");
                else
                    Program.Logger.Info("Something weird happened with the copy.");
            }
            else if (action.ToLower() == "extract")
            {
                string fileextracted = string.Empty;

                bool wasQkilled = false;
                FileInfo f = new FileInfo(file);
                for (int i = 1; IsFileLocked(f); i++)
                {
                    if (i == 5)
                    {
                        Program.Logger.Info(string.Concat("It's been 5 minutes and qBitorrent is not releasing file. Killing it!"));
                        Process[] ps = Process.GetProcessesByName("qBittorrent");
                        foreach (Process p in ps)
                            p.Kill();
                        wasQkilled = true;
                        Thread.Sleep(60000);
                    }
                    else if (i > 7)
                    {
                        Program.Logger.Info(string.Concat("That didn't fix shit, file is still locked...! Restarting qBitorrent... "));
                        Process.Start("C:\\Program Files\\qBittorrent\\qbittorrent.exe");
                        Program.Logger.Info(string.Concat("Welp, better luck next time with the extraction..."));
                        Environment.Exit(9595);
                    }
                    else
                    {
                        Program.Logger.Info(string.Concat("File is locked, waiting 1 minute."));
                        Thread.Sleep(60000);
                    }
                }
                try
                {
                    using (Process exeProcess = new Process())
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo()
                        {
                            CreateNoWindow = false,
                            UseShellExecute = false,
                            WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "UnRAR"),
                            FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "UnRAR", "UnRAR.exe"),
                            WindowStyle = ProcessWindowStyle.Hidden,
                            Arguments = string.Concat("lb \"", file, "\" \"", destination, "\""),
                            RedirectStandardOutput = true
                        };

                        exeProcess.StartInfo = startInfo;
                        exeProcess.Start();
                        fileextracted = exeProcess.StandardOutput.ReadToEnd().Trim();
                        exeProcess.WaitForExit();

                        startInfo.Arguments = string.Concat("x -o+ -y \"", file, "\" \"", destination, "\"");

                        Program.Logger.Info(string.Concat("           UnRAR.exe Extracting file: ", fileextracted));
                        Program.Logger.Debug(string.Concat("           UnRAR.exe Extract command: ", startInfo.FileName, " ", startInfo.Arguments));

                        exeProcess.StartInfo = startInfo;
                        exeProcess.Start();
                        
                        Program.Logger.Debug(exeProcess.StandardOutput.ReadToEnd());
                        
                        exeProcess.WaitForExit();

                        if (File.Exists(Path.Combine(destination, fileextracted)))
                            Program.Logger.Info("Extraction complete.");
                        else
                            Program.Logger.Info("Something weird happened with the extraction.");
                    }
                }
                catch(Exception ex)
                {
                    Program.Logger.Error(string.Concat(ex.HResult, " ", ex.Message));
                    Program.Logger.Error(string.Concat(ex.HResult, " ", ex.StackTrace));
                    Environment.Exit(ex.HResult);
                }
                finally
                {
                    if(wasQkilled)
                    {
                        Program.Logger.Info(string.Concat("Restarting qBitorrent..."));
                        Process.Start("C:\\Program Files\\qBittorrent\\qbittorrent.exe");
                    }
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
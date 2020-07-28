using System;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml.Serialization;

namespace TorrentExtractor
{
    /// <summary>
    /// Main class
    /// </summary>
    public class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Returns the destination folder
        /// </summary>
        /// <param name="sourcePath">Source folder of your downloaded files</param>
        /// <param name="categorie">Categorie sent by the torrent client</param>
        /// <returns>Tuple with destination string and a boolean for fixed destination path(1:Add to string, 0:Don't add to string)</returns>
        private static Tuple<string, bool> GetDestinationPath(string sourcePath, string categorie, Config config)
        {
            string destPath = string.Empty;
            string sourceFolder = new DirectoryInfo(sourcePath).Name.ToLower();
            TextInfo info = new CultureInfo("en-CA", false).TextInfo;
            foreach (var folder in config.Folders.Folder)
            {
                if (folder.Categorie.Equals(categorie))
                {
                    if (folder.Type.ToLower().Equals("tv"))
                    {
                        // ".s##e##." or ".s##."
                        if (new Regex(@"\x2Es\d{1,2}(e\d{2})?\x2E").IsMatch(sourceFolder))
                        {
                            foreach (var word in sourceFolder.Split('.'))
                            {
                                if (new Regex(@"s\d{2}(e\d{2})?").IsMatch(word))
                                    return new Tuple<string, bool>(Path.Combine(folder.Path, info.ToTitleCase(destPath.TrimStart(' ')), string.Concat("Season ", word.Substring(1, 2))), true);
                                else
                                    destPath = string.Concat(destPath, " ", word);
                            }
                        }
                        // ".complete."
                        else if (new Regex(@"\x2Ecomplete\x2E").IsMatch(sourceFolder))
                        {
                            foreach (var word in sourceFolder.Split('.'))
                            {
                                if (new Regex(@"complete").IsMatch(word))
                                    return new Tuple<string, bool>(Path.Combine(folder.Path, info.ToTitleCase(destPath.TrimStart(' '))), true);
                                else
                                    destPath = string.Concat(destPath, " ", word);
                            }
                        }
                        // Full Season/serie
                        // " (####) season ## " or " (####) season s## " or " (####) season ## s## "
                        else if (new Regex(@"\s\(\d{4}\)\sseason\s(d{1,2}\s)?s?\d{1,2}\s").IsMatch(sourceFolder))
                        {
                            if (sourceFolder.Contains(" season s"))
                                return new Tuple<string, bool>(Path.Combine(folder.Path, info.ToTitleCase(sourceFolder.Substring(0, sourceFolder.IndexOf('(')).Trim()), string.Concat("Season ", info.ToTitleCase(sourceFolder.Substring(sourceFolder.LastIndexOf(" season s") + " season s".Length, 2))).Trim()), true);
                            else
                                return new Tuple<string, bool>(Path.Combine(folder.Path, info.ToTitleCase(sourceFolder.Substring(0, sourceFolder.IndexOf('(')).Trim()), string.Concat("Season ", info.ToTitleCase(sourceFolder.Substring(sourceFolder.LastIndexOf(" season ") + " season ".Length, 2))).Trim()), true);
                        }
                        // or " s## "
                        else if (new Regex(@"\ss\d{1,2}\s").IsMatch(sourceFolder))
                        {
                            foreach (var word in sourceFolder.Split(' '))
                            {
                                if (new Regex(@"s\d{2}").IsMatch(word))
                                    return new Tuple<string, bool>(Path.Combine(folder.Path, info.ToTitleCase(destPath.TrimStart(' ')), string.Concat("Season ", word.Substring(1, 2))), true);
                                else
                                    destPath = string.Concat(destPath, " ", word);
                            }
                        }
                        else
                            return new Tuple<string, bool>(Path.Combine(folder.Path, info.ToTitleCase(sourceFolder)), true);
                        break;
                    }
                    else
                        return new Tuple<string, bool>(folder.Path, false);
                }
            }
            throw new Exception("Couldn't generate a destination path.");
        }

        /// <summary>
        /// Return config object from xml file .\Config\Configuration.xml
        /// </summary>
        /// <returns>Config object</returns>
        private static Config GetConfig()
        {
            XmlSerializer reader = new XmlSerializer(typeof(Config));
            try
            {
                using (StreamReader xmlfile = new StreamReader(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Config.xml")))
                    return (Config)reader.Deserialize(xmlfile);
            }
            catch (Exception ex)
            {
                Environment.Exit(ex.HResult);
            }
            throw new Exception("Random error when getting Config.xml file.");
        }

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args">Array of 2 arguments: source, categorie</param>
        public static void Main(string[] args)
        {
            Config config = GetConfig();

            if (args.Length != 2)
                Environment.Exit(10022);

            //TODO: Add some validations
            string sourcePath = args[0];
            string categorie = args[1];

            var destination = GetDestinationPath(sourcePath, categorie, config);

            if (string.IsNullOrEmpty(destination.Item1))
                Environment.Exit(0);

            Logger.Info("*** *** *** *** *** *** *** *** *** *** *** *** *** ***");
            Logger.Info(sourcePath);
            Logger.Info(categorie);
            Logger.Info(destination.Item1);

            // Process
            Extract.UniExtract(sourcePath, destination, config);
        }
    }
}
using System.Reflection;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using Serilog;

namespace TorrentExtractor
{
    class Program
    {
        private static Config GetConfig()
        {
            XmlSerializer reader = new XmlSerializer(typeof(Config));
            try
            {
                using (StreamReader xmlfile = new StreamReader(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Config\\Config.xml")))
                    return (Config)reader.Deserialize(xmlfile);
            }
            catch (FileNotFoundException)
            {
                Environment.Exit(2);
            }
            catch (DirectoryNotFoundException)
            {
                Environment.Exit(3);
            }
            throw new ArgumentException("Some random error when getting Config.xml file.");
        }

        private static void StartLogging(Config config)
        {
            try
            {
                if (config.Logs.LogToFile)
                    Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(config.Logs.File).CreateLogger();
                else
                    config.Logs.Level = -1;
            }
            catch
            {
                Environment.Exit(1);
            }
        }

        private static string GetDestinationPath(Config config, string sourcePath, string categorie)
        {
            Regex regexEpisode = new Regex(@"^S\d{2}(E\d{2})?$");
            Regex regexSeason = new Regex(@"^S\d{2}$");
            string destination = string.Empty;

            foreach (Folder folder in config.Extraction.Folders.Folder)
            {
                if (folder.Categorie == categorie)
                {
                    destination = folder.Path;

                    if (folder.Subfolder)
                    {
                        foreach (string splitted in sourcePath.Substring(3).Split('.'))
                        {
                            if (regexSeason.IsMatch(splitted) || regexEpisode.IsMatch(splitted))
                            {
                                if (folder.Season)
                                    destination = string.Format("{0}\\Season {1}\\", destination.Substring(0, destination.Length - 1), splitted.Substring(1, 2).TrimStart('0'));
                                break;
                            }
                            else
                                destination += string.Format("{0} ", splitted);
                        }
                    }
                    try
                    {
                        Directory.CreateDirectory(destination);
                    }
                    catch(UnauthorizedAccessException)
                    {
                        Log.Information("Need more access to create destination path: {0}", destination);
                        Environment.Exit(5);
                    }
                    catch(PathTooLongException)
                    {
                        Log.Information("The Path is too long: {0}", destination);
                        Environment.Exit(111);
                    }
                    catch(ArgumentNullException)
                    {
                        Log.Information("The Destination path is empty: {0}", destination);
                        Environment.Exit(160);
                    }
                    catch(DirectoryNotFoundException)
                    {
                        Log.Information("The Destination path is not valid: {0}", destination);
                        Environment.Exit(161);
                    }

                    if (config.Logs.Level > 0)
                        Log.Information("Destination path: {0}", destination);
                }
            }
            return destination;
        }

        static void Main(string[] args)
        {
            Config config = GetConfig();
            StartLogging(config);

            try
            {
                if (args.Length != 2)
                    throw new IndexOutOfRangeException("Number of arguments should be two: \"<Source Path>\" \"<Categorie>\".");
            }
            catch (IndexOutOfRangeException ex)
            {
                if (config.Logs.Level >= 0)
                    Log.Error(ex.ToString());
            }

            string sourcePath = args[0];
            string categorie = args[1];

            if (config.Logs.Level > 0)
            {
                Log.Information("Source path: {0}", sourcePath);
                Log.Information("Categorie: {0}", categorie);
            }

            string destination = GetDestinationPath(config, sourcePath, categorie);

            if (string.IsNullOrEmpty(destination))
            {
                Log.Information(string.Format("Nothing is configured for that categorie: \"{0}\"", categorie));
                Environment.Exit(0);
            }
                
            // Process
            if ((File.GetAttributes(sourcePath)).HasFlag(FileAttributes.Directory))
            {
                if (config.Logs.Level > 0)
                    Log.Information("Argument is a folder");

                Extract.FolderExtract(sourcePath, destination, config);
            }
            else
            {
                if (config.Logs.Level > 0)
                    Log.Information("Argument is a file");

                Extract.FileExtract(sourcePath, destination, config, string.Empty);
            }

            if (config.Logs.Level > 0)
                Log.Information("Job done!");
        }
    }
}
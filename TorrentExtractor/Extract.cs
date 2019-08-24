using Serilog;
using System;
using System.IO;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.Threading;


namespace TorrentExtractor
{
    class Extract
    {
        public static void FolderExtract(string folder, string destination, Config config)
        {
            if (config.Logs.Level > 1)
                Log.Debug("function FolderExtract({0}, {1}, {2})", folder, destination, config);

            string lastExtractedFile = string.Empty;
            foreach (string file in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories))
            {
                if (config.Logs.Level > 1)
                    Log.Debug("Using FileExtract() on file: {0}", file);

                lastExtractedFile = FileExtract(file, destination, config, lastExtractedFile);
            }
        }

        public static string FileExtract(string file, string destination, Config config, string lastExtractedFile)
        {
            if (config.Logs.Level > 1)
                Log.Debug("function FileExtract({0}, {1}, {2}, {3})", file, destination, config, lastExtractedFile);

            foreach (FileFormat format in config.Extraction.FileFormats.FileFormat)
            {
                if (format.Format == Path.GetExtension(file))
                {
                    if (format.Action.ToLower() == "copy")
                    {
                        if (!Path.GetFileNameWithoutExtension(file).Contains("sample"))
                        {
                            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
                            if (config.Logs.Level > 0)
                                Log.Information(string.Format("File copied: {0}", Path.GetFileName(file)));
                        }
                    }
                    else if (format.Action.ToLower() == "extract")
                    {
                        try
                        {
                            using (var archive = ArchiveFactory.Open(file))
                            {
                                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                                {
                                    if (entry.Key == lastExtractedFile)
                                        return lastExtractedFile;
                                    else
                                    {
                                        entry.WriteToDirectory(destination, new ExtractionOptions()
                                        {
                                            ExtractFullPath = false,
                                            Overwrite = true
                                        });

                                        if (config.Logs.Level > 0)
                                            Log.Information(string.Format("File extracted: {0}", entry.Key));

                                        return entry.Key;
                                    }
                                }
                            }
                        }
                        catch (IOException ex)
                        {
                            if (config.Logs.Level >= 0)
                            {
                                Log.Error(ex.ToString());
                                Log.Warning("Waiting 5 seconds before trying again.");
                            }
                            Thread.Sleep(5000);
                            FileExtract(file, destination, config, lastExtractedFile);
                        }
                    }
                    else
                        throw new Exception("The action format configured in Config.xml is invalid. It should either be Copy, or Extract.");
                }
            }
            return lastExtractedFile;
        }
    }
}

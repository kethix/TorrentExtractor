using System;

namespace TorrentExtractor
{
    /// <summary>
    /// Main class
    /// </summary>
    public class Program
    {
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly Configuration Configuration = new Configuration();

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args">Array of 2 arguments: source, categorie</param>
        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("You must pass 2 mandatory arguments: source and categorie.");

            Torrent torrent = new Torrent(args[0], args[1], Configuration);

            Logger.Info("*** *** *** *** *** *** *** *** *** *** *** *** *** *** *** *** *** *** *** *** *** *** *** *** *** *** *** ***");
            Logger.Info("                        arg 1 (Path): " + torrent.SourceFullPath);
            Logger.Info("                   arg 2 (Categorie): " + torrent.Categorie);
            Logger.Info("     Number of folder(s) to validate: " + torrent.FoldersToValidate.Count);
            Logger.Info("Data used to create destination path: " + torrent.SourceFolder);
            Logger.Info("                    Destination Path: " + torrent.DestinationFullPath);
            Extract.DecideActionOnFiles(torrent, Configuration);
        }
    }
}
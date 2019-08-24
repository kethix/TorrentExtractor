# TorrentExtractor
Automate the extractions of downloaded torrent files for movies and tv shows.

You can split files into separate folders and you can also split them by seasons.

The tool has been used and tested with qBittorent.

# Examples
A torrent named **TVShowName.S07E07.720p.WEB.H264-GrOuPnAmE** will end up in **ConfiguredPath\TVShowName\\**

# Configuration

Edit **Configuration.xml** to adjust the application to your needs.

Download the torrent in its own folder.

Apply a Category to your torrent.

In qBittorent, go into: Options > Downloads > Run external program on torrent completion

**InstallationFolder\\TorrentExtractor.exe "%F" "%L"**

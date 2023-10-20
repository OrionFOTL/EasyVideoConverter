using VideoConverter.Downloader.Models;

namespace VideoConverter.Downloader;
public interface IFFmpegDownloader
{
    event EventHandler<DownloadProgressEventArgs> DownloadProgressChanged;
    event EventHandler ExtractionFinished;
    event EventHandler ExtractionStarted;

    Task DownloadAndExtractFFmpeg();
}
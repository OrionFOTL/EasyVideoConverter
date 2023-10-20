namespace VideoConverter.Downloader.Models;

public class DownloadProgressEventArgs : EventArgs
{
    public required int BytesReceived { get; init; }

    public required int TotalBytesToReceive { get; init; }

    public double ProgressPercentage => (double)BytesReceived / TotalBytesToReceive;
}

using System.IO.Compression;
using VideoConverter.Downloader.Models;

namespace VideoConverter.Downloader;

public class GithubFFmpegDownloader : IFFmpegDownloader
{
    private readonly string _ffmpegUrl = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";

    public event EventHandler<DownloadProgressEventArgs> DownloadProgressChanged = delegate { };
    public event EventHandler ExtractionStarted = delegate { };
    public event EventHandler ExtractionFinished = delegate { };

    public async Task DownloadAndExtractFFmpeg()
    {
        var ffmpegZip = await DownloadFFmpegZip();
        using var ffmpegExeStream = GetFFmpegExeStream(ffmpegZip);
        await ExtractFFmpegExeToFile(ffmpegExeStream);
    }

    private async Task<byte[]> DownloadFFmpegZip()
    {
        var httpClient = new HttpClient();

        using var httpResponse = await httpClient.GetAsync(_ffmpegUrl, HttpCompletionOption.ResponseHeadersRead);
        httpResponse.EnsureSuccessStatusCode();

        var totalFileSize = httpResponse.Content.Headers.ContentLength ??
            throw new Exception($"Couldn't download FFmpeg executable from {_ffmpegUrl}; file length was null");

        var zipContents = new byte[(int)totalFileSize];

        using var contentStream = await httpResponse.Content.ReadAsStreamAsync();

        var bytesToRead = 100;
        var totalRead = 0;
        var nextPercentToReport = 0.01;
        var readCount = await contentStream.ReadAsync(zipContents.AsMemory(totalRead, bytesToRead));

        while (readCount > 0)
        {
            totalRead += readCount;

            double percentRead = totalRead / (double)totalFileSize;
            if (percentRead >= nextPercentToReport)
            {
                OnDownloadProgress(totalRead, (int)totalFileSize);
                nextPercentToReport += 0.01;
            }

            var bytesLeft = zipContents.Length - totalRead;
            var bytesToReadNext = Math.Min(bytesLeft, bytesToRead);

            readCount = await contentStream.ReadAsync(zipContents.AsMemory(totalRead, bytesToReadNext));
        }

        OnDownloadProgress(totalRead, (int)totalFileSize);

        return zipContents;
    }

    private void OnDownloadProgress(int bytesReceived, int totalBytesToReceive)
    {
        DownloadProgressEventArgs e = new()
        {
            BytesReceived = bytesReceived,
            TotalBytesToReceive = totalBytesToReceive
        };

        DownloadProgressChanged.Invoke(this, e);
    }

    private static Stream GetFFmpegExeStream(byte[] ffmpegZip)
    {
        var ms = new MemoryStream(ffmpegZip);
        var zipArchive = new ZipArchive(ms);

        var ffmpegExe = zipArchive.Entries.FirstOrDefault(e => e.Name is "ffmpeg.exe")
            ?? throw new Exception("Couldn't download ffmpeg.exe; no ffmpeg.exe found in downloaded ZIP");

        return ffmpegExe.Open();
    }

    private async Task ExtractFFmpegExeToFile(Stream ffmpegExeStream)
    {
        var programDirectory = AppDomain.CurrentDomain.BaseDirectory;
        using var fs = File.OpenWrite(Path.Combine(programDirectory, "ffmpeg.exe"));

        ExtractionStarted.Invoke(this, new());
        await ffmpegExeStream.CopyToAsync(fs);
        ExtractionFinished.Invoke(this, new());
    }
}

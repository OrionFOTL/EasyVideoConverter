using FFmpeg.NET;
using VideoConverter.FFmpegCheckers;
using VideoConverter.Models;
using VideoConverter.VideoInformation.Models;

namespace VideoConverter.VideoInformation;

public class XFFmpegVideoMetadataRetriever : IVideoMetadataRetriever
{
    private readonly IFFmpegFinder _ffmpegFinder;

    public XFFmpegVideoMetadataRetriever(IFFmpegFinder ffmpegFinder) => _ffmpegFinder = ffmpegFinder;

    public async Task<VideoMetadata> GetVideoData(FileInfo inputFile)
    {
        var engine = new Engine(_ffmpegFinder.FindFFmpegExecutable()?.FullName);

        var fileMetadata = await engine.GetMetaDataAsync(new InputFile(inputFile), CancellationToken.None);

        if (fileMetadata is null or { VideoData: null })
        {
            throw new ArgumentException("Input file is not a video file", nameof(inputFile));
        }

        return new VideoMetadata()
        {
            FilePath = inputFile.FullName,
            Format = ParseFormat(fileMetadata.VideoData.Format),
            Fps = (int)Math.Round(fileMetadata.VideoData.Fps, 0),
            Duration = fileMetadata.Duration,
            Resolution = ParseResolution(fileMetadata.VideoData.FrameSize),
        };
    }

    private static VideoFormat ParseFormat(string videoFormat)
    {
        return videoFormat switch
        {
            string x264 when x264.Contains("264") => VideoFormat.H264,
            string x265 when x265.Contains("265") => VideoFormat.H265,
            _ => VideoFormat.Other,
        };
    }

    private static Resolution ParseResolution(string frameSize)
    {
        var split = frameSize.Split('x');

        var width = int.Parse(split[0]);
        var height = int.Parse(split[1]);

        return new Resolution(width, height);
    }
}

using FFMpegCore;
using FFMpegCore.Enums;
using VideoConverter.Conversion.Models;
using VideoConverter.FFmpegCheckers;
using VideoConverter.Models;
using VideoConverter.VideoInformation;

namespace VideoConverter.Conversion;

public class FFMpegCoreConverter : IVideoConverter
{
    private readonly IVideoMetadataRetriever _videoMetadataRetriever;
    private readonly IFFmpegFinder _ffmpegFinder;

    public FFMpegCoreConverter(
        IVideoMetadataRetriever videoMetadataRetriever,
        IFFmpegFinder ffmpegFinder)
    {
        _videoMetadataRetriever = videoMetadataRetriever;
        _ffmpegFinder = ffmpegFinder;
    }

    public event EventHandler<ConversionProgressEventArgs> ConversionProgress = delegate { };
    public event EventHandler ConversionComplete = delegate { };
    public event EventHandler FirstPassComplete = delegate { };

    public async Task Convert(ConversionParameters conversionOptions)
    {
        void AddConversionOptions(FFMpegArgumentOptions o)
        {
            if (conversionOptions.Fps is { } fps)
            {
                o.WithFramerate(fps);
            }

            if (conversionOptions.Resolution is { } resolution)
            {
                o.Resize(resolution.Width, resolution.Height);
            }

            o.WithVideoCodec(conversionOptions.Format switch
            {
                VideoFormat.H265 => VideoCodec.LibX265,
                VideoFormat.H264 or _ => VideoCodec.LibX264,
            });
            o.Seek(conversionOptions.ClipRange?.Start);
            o.EndSeek(conversionOptions?.ClipRange?.End);
            o.WithFastStart();
        }

        var duration = conversionOptions.ClipRange is ClipRange clipRange
            ? clipRange.End - clipRange.Start
            : (await _videoMetadataRetriever.GetVideoData(new FileInfo(conversionOptions.InputFilePath))).Duration;

        var ffOptions = new FFOptions
        {
            BinaryFolder = _ffmpegFinder.FindFFmpegExecutable()?.Directory?.FullName
                ?? throw new InvalidOperationException("FFmpeg.exe not found")
        };

        if (conversionOptions.MaxFileSizeInMegabytes is double maxMegabytes and > 0)
        {
            var (audioKilobitsPerSecond, videoKilobitsPerSecond) = MaxBitratesCalculator.GetMaxBitrates(maxMegabytes, duration);

            // pass 1
            await FFMpegArguments
                .FromFileInput(conversionOptions.InputFilePath)
                .OutputToFile("NUL", true, o =>
                {
                    AddConversionOptions(o);
                    o.WithAudioBitrate(audioKilobitsPerSecond)
                     .WithVideoBitrate(videoKilobitsPerSecond)
                     .WithCustomArgument(conversionOptions.Format is VideoFormat.H265
                        ? "-x265-params pass=1 -f null"
                        : "-pass 1 -f null");
                })
                .NotifyOnProgress(percent => ConversionProgress?.Invoke(this, new() { ProcessedDuration = duration * percent / 100, TotalDuration = duration }), duration)
                .ProcessAsynchronously(throwOnError: true, ffOptions);

            FirstPassComplete?.Invoke(this, new());

            // pass 2
            await FFMpegArguments
                .FromFileInput(conversionOptions.InputFilePath)
                .OutputToFile(conversionOptions.OutputFileName, true, o =>
                {
                    AddConversionOptions(o);
                    o.WithAudioBitrate(audioKilobitsPerSecond)
                     .WithVideoBitrate(videoKilobitsPerSecond)
                     .WithCustomArgument(conversionOptions.Format is VideoFormat.H265
                        ? "-x265-params pass=2"
                        : "-pass 2");
                })
                .NotifyOnProgress(percent => ConversionProgress?.Invoke(this, new() { ProcessedDuration = duration * percent / 100, TotalDuration = duration }), duration)
                .ProcessAsynchronously(throwOnError: true, ffOptions);

            ConversionComplete?.Invoke(this, new());
        }
        else
        {
            await FFMpegArguments
                .FromFileInput(conversionOptions.InputFilePath)
                .OutputToFile(conversionOptions.OutputFileName, true, AddConversionOptions)
                .NotifyOnProgress(percent => ConversionProgress?.Invoke(this, new() { ProcessedDuration = duration * percent / 100, TotalDuration = duration }), duration)
                .ProcessAsynchronously(throwOnError: true, ffOptions);
        }
    }
}

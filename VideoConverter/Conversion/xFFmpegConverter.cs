using FFmpeg.NET;
using FFmpeg.NET.Enums;
using VideoConverter.Conversion.Models;
using VideoFormat = VideoConverter.Models.VideoFormat;

namespace VideoConverter.Conversion;

public class XFFMpegConverter : IVideoConverter
{
    public event EventHandler<ConversionProgressEventArgs> ConversionProgress = delegate { };
    public event EventHandler ConversionComplete = delegate { };
    public event EventHandler FirstPassComplete = delegate { };

    public async Task Convert(ConversionParameters conversionOptions)
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, _) => cts.Cancel();

        var inputFile = new InputFile(conversionOptions.InputFilePath);
        var outputFile = new OutputFile(conversionOptions.OutputFileName);

        var libConversionOptions = new ConversionOptions()
        {
            VideoFps = conversionOptions.Fps,
            VideoCodec = conversionOptions.Format switch
            {
                VideoFormat.H265 => VideoCodec.libx265,
                VideoFormat.H264 or _ => VideoCodec.libx264
            },
            VideoSize = conversionOptions.Height is not null ? VideoSize.Custom : VideoSize.Default,
            CustomHeight = conversionOptions.Height,
            Seek = conversionOptions.ClipRange?.Start,
            MaxVideoDuration = conversionOptions.ClipRange?.End - conversionOptions.ClipRange?.Start,
            VideoCodecPreset = VideoCodecPreset.fast,
        };

        var ffmpeg = new Engine();

        ffmpeg.Error += (_, e) => throw e.Exception;
        ffmpeg.Progress += (_, e) => ConversionProgress?.Invoke(this, new ConversionProgressEventArgs
        {
            ProcessedDuration = e.ProcessedDuration,
            TotalDuration = conversionOptions.ClipRange is ClipRange range
                ? range.End - range.Start
                : e.TotalDuration
        });

        if (conversionOptions.MaxFileSizeInMegabytes is double maxMegabytes and > 0)
        {
            var maxKilobits = maxMegabytes * 8000;
            var duration = libConversionOptions.MaxVideoDuration ?? (await ffmpeg.GetMetaDataAsync(inputFile, CancellationToken.None)).Duration;

            int kilobitsPerSecond = (int)(maxKilobits / duration.TotalSeconds);
            libConversionOptions.VideoBitRate = kilobitsPerSecond;

            // pass 1
            var nulFile = new OutputFile("NUL");

            libConversionOptions.ExtraArguments = conversionOptions.Format is VideoFormat.H265
                ? "-x265-params pass=1 -f null"
                : "-pass 1 -f null";

            await ffmpeg.ConvertAsync(inputFile, nulFile, libConversionOptions, cts.Token);
            FirstPassComplete?.Invoke(this, new());

            // pass 2
            libConversionOptions.ExtraArguments = conversionOptions.Format is VideoFormat.H265
                ? "-x265-params pass=2"
                : "-pass 2";

            ffmpeg.Complete += (_, e) => ConversionComplete?.Invoke(this, e);

            await ffmpeg.ConvertAsync(inputFile, outputFile, libConversionOptions, cts.Token);
        }
        else
        {
            ffmpeg.Complete += (_, e) => ConversionComplete?.Invoke(this, e);

            await ffmpeg.ConvertAsync(inputFile, outputFile, libConversionOptions, cts.Token);
        }
    }
}

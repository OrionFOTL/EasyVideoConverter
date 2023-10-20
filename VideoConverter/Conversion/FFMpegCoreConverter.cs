using FFMpegCore;
using VideoConverter.Conversion.Models;

namespace VideoConverter.Conversion;

public class FFMpegCoreConverter : IVideoConverter
{
    public event EventHandler<ConversionProgressEventArgs> ConversionProgress = delegate { };
    public event EventHandler ConversionComplete = delegate { };
    public event EventHandler FirstPassComplete = delegate { };

    public async Task Convert(ConversionParameters conversionOptions)
    {
        var fileInfo = FFProbe.Analyse(conversionOptions.InputFilePath);

        await FFMpegArguments
            .FromFileInput(conversionOptions.InputFilePath)
            .OutputToFile(@"C:/Users/Krzysiek/Desktop/output.mp4", true, options => options
                //.WithVideoCodec(conversionOptions.Codec)
                //.WithConstantRateFactor((int)conversionOptions.Crf.Value)
                .WithFramerate(conversionOptions.Fps.Value)
                //.WithVideoFilters(filterOptions => filterOptions
                //    .Scale((VideoSize)conversionOptions.Height))
                .Seek(conversionOptions.ClipRange?.Start)
                //.EndSeek(conversionOptions.End)
                .WithFastStart())
            .NotifyOnProgress(percent => Console.WriteLine($"Progress: {percent}%"), fileInfo.Duration - conversionOptions.ClipRange.Value.Start)
            .ProcessAsynchronously();
    }
}

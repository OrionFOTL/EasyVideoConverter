using VideoConverter.Conversion.Models;

namespace VideoConverter.Conversion;

public interface IVideoConverter
{
    event EventHandler<ConversionProgressEventArgs> ConversionProgress;
    event EventHandler ConversionComplete;
    event EventHandler FirstPassComplete;

    Task Convert(ConversionParameters conversionOptions);
}
using VideoConverter.Models;

namespace VideoConverter.Conversion.Models;

public record class ConversionParameters
{
    public required string InputFilePath { get; init; }

    public string OutputFileName { get; init; } = "output.mp4";

    public int? Fps { get; init; }

    public VideoFormat? Format { get; init; }

    public int? Height { get; init; }

    public double? MaxFileSizeInMegabytes { get; init; }

    public ClipRange? ClipRange { get; init; }
}

using VideoConverter.Models;

namespace VideoConverter.VideoInformation.Models;

public record VideoMetadata
{
    public required string FilePath { get; init; }

    public required int Fps { get; init; }

    public required VideoFormat Format { get; init; }

    public required Resolution Resolution { get; init; }

    public required TimeSpan Duration { get; init; }
}

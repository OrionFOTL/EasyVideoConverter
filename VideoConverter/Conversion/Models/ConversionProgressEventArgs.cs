namespace VideoConverter.Conversion.Models;

public class ConversionProgressEventArgs : EventArgs
{
    public required TimeSpan ProcessedDuration { get; init; }

    public required TimeSpan TotalDuration { get; init; }

    public double ProgressPercentage => ProcessedDuration / TotalDuration;
}

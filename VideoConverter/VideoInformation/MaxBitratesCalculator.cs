namespace VideoConverter.VideoInformation;

internal static class MaxBitratesCalculator
{
    private const double _maxAudioProportion = 0.15;

    public static (int AudioKilobitsPerSecond, int VideoKilobitsPerSecond) GetMaxBitrates(double maxMegabytes, TimeSpan duration)
    {
        var maxKilobits = maxMegabytes * 8000;
        var audioKilobits = duration.TotalSeconds * 160;

        if (audioKilobits > maxKilobits * _maxAudioProportion)
        {
            audioKilobits = maxKilobits * _maxAudioProportion;
        }

        var videoKilobits = maxKilobits - audioKilobits;

        int audioKilobitsPerSecond = (int)(audioKilobits / duration.TotalSeconds);
        int videoKilobitsPerSecond = (int)(videoKilobits / duration.TotalSeconds);

        return (audioKilobitsPerSecond, videoKilobitsPerSecond);
    }
}
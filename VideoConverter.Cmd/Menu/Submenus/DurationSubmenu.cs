using VideoConverter.Cmd.Menu.Submenus.Base;
using VideoConverter.Conversion.Models;
using VideoConverter.VideoInformation.Models;

namespace VideoConverter.Cmd.Menu.Submenus;

internal class DurationSubmenu : ISubmenu
{
    private TimeSpan _start = TimeSpan.Zero;
    private TimeSpan _end = TimeSpan.Zero;
    private TimeSpan _videoDuration = TimeSpan.Zero;

    public string Title => "Cut video";

    public string CurrentValueDescription => $"from {_start} to {_end:hh\\:mm\\:ss\\.ff}";

    public EditStatus EditStatus { get; private set; } = EditStatus.Uninitialised;

    public void SetValueFromInputVideo(VideoMetadata videoMetadata)
    {
        _start = TimeSpan.Zero;
        _end = videoMetadata.Duration;
        _videoDuration = videoMetadata.Duration;

        EditStatus = EditStatus.InheritedFromInputVideo;
    }

    public void PromptForValue()
    {
        while (true)
        {
            ColorWriter.WriteValuePrompt("Enter the start time in the hh:mm:ss format (press Enter for 00:00:00): ");

            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                _start = TimeSpan.Zero;
                break;
            }

            if (!TimeSpan.TryParse(input, out var startTime))
            {
                ColorWriter.WriteInputError("Invalid input");
                continue;
            }

            if (startTime < TimeSpan.Zero)
            {
                ColorWriter.WriteInputError("Start time can't be negative");
                continue;
            }

            if (startTime > _videoDuration)
            {
                ColorWriter.WriteInputError("Start time can't be later than the end of the video");
                continue;
            }

            _start = startTime;
            break;
        }

        while (true)
        {
            ColorWriter.WriteValuePrompt($"Enter the end time in the hh:mm:ss format (press Enter for {_videoDuration}): ");

            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                _end = _videoDuration;
                break;
            }

            if (!TimeSpan.TryParse(input, out var endTime))
            {
                ColorWriter.WriteInputError("Invalid input");
                continue;
            }

            if (endTime < _start)
            {
                ColorWriter.WriteInputError("Invalid input; end time must be later than start time");
                continue;
            }

            if (endTime > _videoDuration)
            {
                ColorWriter.WriteInputError("Invalid input; end time can't exceed video duration");
                continue;
            }

            _end = endTime;
            EditStatus = EditStatus.Customised;
            return;
        }
    }

    public ConversionParameters SetConversionParameter(ConversionParameters parameters) => parameters with { ClipRange = new(_start, _end) };

    public void LoadSavedValues(ConversionParameters loadedParameters)
    {
        if (loadedParameters.ClipRange is null)
        {
            return;
        }

        var clipRange = loadedParameters.ClipRange.Value;

        if (clipRange.Start < TimeSpan.Zero
            || clipRange.Start > _videoDuration
            || clipRange.End < TimeSpan.Zero
            || clipRange.End < clipRange.Start
            || clipRange.End > _videoDuration)
        {
            return;
        }

        _start = clipRange.Start;
        _end = clipRange.End;

        EditStatus = EditStatus.Customised;
    }
}

using VideoConverter.Cmd.Menu.Submenus.Base;
using VideoConverter.Conversion.Models;
using VideoConverter.Models;
using VideoConverter.VideoInformation.Models;

namespace VideoConverter.Cmd.Menu.Submenus;

internal class FormatSubmenu : ISubmenu
{
    private VideoFormat _format = VideoFormat.H264;

    public string Title => "Output format";

    public string CurrentValueDescription => _format switch { VideoFormat.H264 => "H.264", VideoFormat.H265 => "H.265", var other => other.ToString() };

    public EditStatus EditStatus { get; private set; } = EditStatus.Uninitialised;

    public void SetValueFromInputVideo(VideoMetadata videoMetadata)
    {
        _format = videoMetadata.Format switch
        {
            VideoFormat.Other => VideoFormat.H264,
            VideoFormat known => known
        };

        EditStatus = EditStatus.InheritedFromInputVideo;
    }

    public void PromptForValue()
    {
        ColorWriter.WriteValuePrompt("Choose the output format:");
        Console.WriteLine("1) H.264 - faster conversion, supported on Discord");
        Console.WriteLine("2) H.265 - slower conversion, better quality, smaller file size");

        while (true)
        {
            var input = Console.ReadKey();

            if (input.KeyChar is '1')
            {
                _format = VideoFormat.H264;
                break;
            }
            else if (input.KeyChar is '2')
            {
                _format = VideoFormat.H265;
                break;
            }
        }

        EditStatus = EditStatus.Customised;
    }

    public ConversionParameters SetConversionParameter(ConversionParameters parameters) => parameters with { Format = _format };

    public void LoadSavedValues(ConversionParameters loadedParameters)
    {
        if (loadedParameters.Format is VideoFormat format)
        {
            _format = format;
            EditStatus = EditStatus.Customised;
        }
    }
}

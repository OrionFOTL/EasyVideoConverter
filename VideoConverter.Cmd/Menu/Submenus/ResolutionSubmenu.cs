using VideoConverter.Cmd.Menu.Submenus.Base;
using VideoConverter.Conversion.Models;
using VideoConverter.VideoInformation.Models;

namespace VideoConverter.Cmd.Menu.Submenus;

internal class ResolutionSubmenu : ISubmenu
{
    private Resolution _resolution;

    public string Title => "Resolution";

    public string CurrentValueDescription => _resolution.ToString();

    public EditStatus EditStatus { get; private set; }

    public void SetValueFromInputVideo(VideoMetadata videoMetadata)
    {
        _resolution = videoMetadata.Resolution;
        EditStatus = EditStatus.InheritedFromInputVideo;
    }

    public void PromptForValue()
    {
        ColorWriter.WriteValuePrompt("Enter vertical resolution in pixels (720, 1080, etc):");

        while (true)
        {
            var input = Console.ReadLine();
            if (int.TryParse(input, out int newHeight) && newHeight > 0)
            {
                _resolution = _resolution.ResizeToHeight(newHeight);
                EditStatus = EditStatus.Customised;
                return;
            }

            ColorWriter.WriteInputError("Invalid input, please enter a positive number");
        }
    }

    public ConversionParameters SetConversionParameter(ConversionParameters parameters) => parameters with { Resolution = _resolution };

    public void LoadSavedValues(ConversionParameters loadedParameters)
    {
        if (loadedParameters.Resolution is Resolution resolution)
        {
            _resolution = _resolution.ResizeToHeight(resolution.Height);
            EditStatus = EditStatus.Customised;
        }
    }
}

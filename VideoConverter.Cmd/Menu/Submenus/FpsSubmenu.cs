using VideoConverter.Cmd.Menu.Submenus.Base;
using VideoConverter.Conversion.Models;
using VideoConverter.VideoInformation.Models;

namespace VideoConverter.Cmd.Menu.Submenus;

internal class FpsSubmenu : ISubmenu
{
    private int _fps = 0;

    public string Title => "Frames per second";

    public string CurrentValueDescription => $"{_fps} FPS";

    public EditStatus EditStatus { get; private set; } = EditStatus.Uninitialised;

    public void SetValueFromInputVideo(VideoMetadata videoMetadata)
    {
        _fps = videoMetadata.Fps;
        EditStatus = EditStatus.InheritedFromInputVideo;
    }

    public void PromptForValue()
    {
        ColorWriter.WriteValuePrompt("Enter the value of FPS (frames per second) to use.");

        while (true)
        {
            var input = Console.ReadLine();
            if (int.TryParse(input, out int fps) && fps > 0)
            {
                _fps = fps;
                EditStatus = EditStatus.Customised;
                return;
            }

            ColorWriter.WriteInputError("Invalid input, please enter a positive number");
        }
    }

    public ConversionParameters SetConversionParameter(ConversionParameters parameters) => parameters with { Fps = _fps };

    public void LoadSavedValues(ConversionParameters loadedParameters)
    {
        if (loadedParameters.Fps is int fps)
        {
            _fps = fps;
            EditStatus = EditStatus.Customised;
            return;
        }
    }
}

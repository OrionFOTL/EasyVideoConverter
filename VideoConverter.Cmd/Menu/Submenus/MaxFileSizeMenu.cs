using VideoConverter.Cmd.Menu.Submenus.Base;
using VideoConverter.Conversion.Models;
using VideoConverter.VideoInformation.Models;

namespace VideoConverter.Cmd.Menu.Submenus;

internal class MaxFileSizeMenu : ISubmenu
{
    private double _maxMegabytes;

    public string Title => "Max file size";

    public string CurrentValueDescription => $"{_maxMegabytes:F2} MB";

    public EditStatus EditStatus { get; private set; } = EditStatus.Uninitialised;

    public void SetValueFromInputVideo(VideoMetadata _) { }

    public void PromptForValue()
    {
        ColorWriter.WriteValuePrompt("Enter the maximum acceptable file size in megabytes.");

        while (true)
        {
            var input = Console.ReadLine();
            if (double.TryParse(input, out double maxMegabytes) && maxMegabytes > 0)
            {
                _maxMegabytes = maxMegabytes;
                EditStatus = EditStatus.Customised;
                return;
            }

            ColorWriter.WriteInputError("Invalid input, please enter a positive number");
        }
    }

    public ConversionParameters SetConversionParameter(ConversionParameters parameters) => parameters with { MaxFileSizeInMegabytes = _maxMegabytes };

    public void LoadSavedValues(ConversionParameters conversionParameters)
    {
        if (conversionParameters.MaxFileSizeInMegabytes is double maxMb)
        {
            _maxMegabytes = maxMb;
            EditStatus = EditStatus.Customised;
        }
    }
}

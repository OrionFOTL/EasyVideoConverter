using VideoConverter.Cmd.Menu.Submenus.Base;
using VideoConverter.Conversion.Models;
using VideoConverter.VideoInformation.Models;

namespace VideoConverter.Cmd.Menu.Submenus;

internal class OutputFileNameSubmenu : ISubmenu
{
    public string Title => "Output file name";

    public string CurrentValueDescription { get; private set; } = "output.mp4";

    public EditStatus EditStatus { get; private set; } = EditStatus.Uninitialised;

    public void SetValueFromInputVideo(VideoMetadata videoMetadata)
    {
        CurrentValueDescription = Path.GetFileNameWithoutExtension(videoMetadata.FilePath) + "_conv.mp4";
        EditStatus = EditStatus.InheritedFromInputVideo;
    }

    public void PromptForValue()
    {
        ColorWriter.WriteValuePrompt("Enter output file name without extension:");

        var input = Console.ReadLine() + ".mp4";

        while (File.Exists(input))
        {
            ColorWriter.WriteInputError("Input file with the same name already exists in current directory; choose a different name.");
            input = Console.ReadLine() + ".mp4";
        }

        CurrentValueDescription = input;
        EditStatus = EditStatus.Customised;
    }

    public ConversionParameters SetConversionParameter(ConversionParameters parameters) => parameters with { OutputFileName = CurrentValueDescription };

    public void LoadSavedValues(ConversionParameters conversionParameters)
    {
        if (EditStatus == EditStatus.Customised)
        {
            return;
        }

        CurrentValueDescription = ConstructOutputFilename(conversionParameters);
        EditStatus = EditStatus.InheritedFromInputVideo;
    }

    private static string ConstructOutputFilename(ConversionParameters initialParameters)
    {
        var str = Path.GetFileNameWithoutExtension(initialParameters.InputFilePath);

        if (initialParameters.Height is not null)
        {
            str += $"_{initialParameters.Height}p";
        }

        if (initialParameters.Fps is not null)
        {
            str += $"_{initialParameters.Fps}fps";
        }

        if (initialParameters.Format is not null)
        {
            str += $"_{initialParameters.Format}";
        }

        if (initialParameters.MaxFileSizeInMegabytes is not null)
        {
            str += $"_{initialParameters.MaxFileSizeInMegabytes}MB";
        }

        str += ".mp4";

        return str;
    }
}
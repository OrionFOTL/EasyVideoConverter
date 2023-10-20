using VideoConverter.Conversion.Models;
using VideoConverter.VideoInformation.Models;

namespace VideoConverter.Cmd.Menu.Submenus.Base;

internal interface ISubmenu
{
    string Title { get; }

    string CurrentValueDescription { get; }

    EditStatus EditStatus { get; }

    void SetValueFromInputVideo(VideoMetadata videoMetadata);

    void PromptForValue();

    ConversionParameters SetConversionParameter(ConversionParameters parameters);

    void LoadSavedValues(ConversionParameters loadedParameters);
}

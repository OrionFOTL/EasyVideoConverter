using System.Text.Json;
using Microsoft.Extensions.Options;
using Pastel;
using ShellProgressBar;
using VideoConverter.Cmd.Menu.Submenus;
using VideoConverter.Cmd.Menu.Submenus.Base;
using VideoConverter.Conversion;
using VideoConverter.Conversion.Models;
using VideoConverter.Downloader;
using VideoConverter.FFmpegCheckers;
using VideoConverter.Models;
using VideoConverter.VideoInformation;
using VideoConverter.VideoInformation.Models;

namespace VideoConverter.Cmd.Menu;

internal class MainMenu
{
    private readonly string _lastSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lastUsedSettings.json");
    private readonly FileInfo _inputFile;
    private readonly IFFmpegFinder _ffmpegChecker;
    private readonly IVideoMetadataRetriever _videoMetadataRetriever;
    private readonly IVideoConverter _converter;
    private readonly IFFmpegDownloader _ffmpegDownloader;

    private bool _firstPrint = true;
    private bool _lastSettingsLoaded = false;
    private ConversionParameters _conversionParameters;
    private readonly List<ISubmenu> _submenus = new();
    private ISubmenu? _currentSubmenu = null;

    public MainMenu(
        IOptions<InputFilePath> inputFilePath,
        IFFmpegFinder ffmpegChecker,
        IVideoMetadataRetriever videoMetadataRetriever,
        IVideoConverter converter,
        IFFmpegDownloader ffmpegDownloader,
        IEnumerable<ISubmenu> submenus)
    {
        _inputFile = new FileInfo(inputFilePath.Value.Path);
        _ffmpegChecker = ffmpegChecker;
        _videoMetadataRetriever = videoMetadataRetriever;
        _converter = converter;
        _ffmpegDownloader = ffmpegDownloader;
        _submenus.AddRange(submenus);
        _conversionParameters = new() { InputFilePath = _inputFile.FullName };

    }

    public async Task Loop()
    {
        ConsoleKeyInfo pressedKey;
        while (true)
        {
            await PrintMenu();
            if (_currentSubmenu is null)
            {
                ColorWriter.WriteActionPrompt("Enter a number or letter of a setting you'd like to change. All settings are optional.");
                pressedKey = Console.ReadKey();
                await ExecuteAction(pressedKey);
            }
            else
            {
                _currentSubmenu.PromptForValue();
                _conversionParameters = _currentSubmenu.SetConversionParameter(_conversionParameters);
                _submenus.FirstOrDefault(sm => sm is OutputFileNameSubmenu)?.LoadSavedValues(_conversionParameters);
                _currentSubmenu = null;
            }
        }
    }

    private async Task ExecuteAction(ConsoleKeyInfo pressedKey)
    {
        if (char.IsDigit(pressedKey.KeyChar))
        {
            var number = int.Parse(pressedKey.KeyChar.ToString());

            if (number > 0 && number <= _submenus.Count)
            {
                var submenu = _submenus[number - 1];
                _currentSubmenu = submenu;
            }
        }
        else if (pressedKey.Key is ConsoleKey.L)
        {
            LoadLastSettings();
            _lastSettingsLoaded = true;
        }
        else if (pressedKey.Key is ConsoleKey.X)
        {
            await StartConversion();
        }
    }

    private async Task PrintMenu()
    {
        Console.Clear();
        ColorWriter.WriteWelcome("Welcome to Video Converter 📼");
        Console.WriteLine();

        if (_firstPrint)
        {
            await DownloadFFmpegIfMissing();
        }

        Console.WriteLine($"➡️ Input file: {_inputFile.FullName}");
        Console.WriteLine();

        if (_firstPrint)
        {
            Console.Write("Analysing file...");
            var inputVideoParameters = await VerifyInputVideo();
            InitializeInputVideoParameters(inputVideoParameters);

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        PrintSubmenus();
        Console.WriteLine();

        Console.WriteLine($"L) {(_lastSettingsLoaded ? "✔️" : "💾")} Load last used settings");
        Console.WriteLine("X) ⚙️ Start conversion");
        Console.WriteLine();

        _firstPrint = false;
    }

    private async Task DownloadFFmpegIfMissing()
    {
        if (_ffmpegChecker.FindFFmpegExecutable() is not null)
        {
            return;
        }

        ColorWriter.WriteInfo("FFmpeg executable not found in PATH or in app directory; downloading...");

        var options = new ProgressBarOptions
        {
            ProgressCharacter = '─',
            ProgressBarOnBottom = false
        };

        using (var pbar = new ProgressBar(10_000, "Downloading...", options))
        {
            var progress = pbar.AsProgress<double>();

            _ffmpegDownloader.DownloadProgressChanged += (_, e) => progress.Report(e.ProgressPercentage);
            _ffmpegDownloader.ExtractionStarted += (_, _) => pbar.Message = "Extracting...";
            _ffmpegDownloader.ExtractionFinished += (_, _) => pbar.Message = "Finished";

            await _ffmpegDownloader.DownloadAndExtractFFmpeg();
        }

        Console.WriteLine();
    }

    private async Task<VideoMetadata> VerifyInputVideo()
    {
        return _inputFile.Exists
            ? await _videoMetadataRetriever.GetVideoData(_inputFile)
            : throw new Exception("Input file does not exist");
    }

    private void InitializeInputVideoParameters(VideoMetadata inputVideoMetadata)
    {
        foreach (var submenu in _submenus)
        {
            submenu.SetValueFromInputVideo(inputVideoMetadata);
        }
    }

    private void PrintSubmenus()
    {
        foreach (var (submenu, i) in _submenus.Select((sm, i) => (sm, ++i)))
        {
            bool customised = submenu.EditStatus is EditStatus.Customised;
            bool hasValue = submenu.EditStatus is EditStatus.InheritedFromInputVideo or EditStatus.Customised;
            bool isCurrentSubmenu = _currentSubmenu == submenu;

            int longestTitleLength = _submenus.Max(sm => sm.Title.Length);

            // 1)     FPS     -- fresh
            // 1)     FPS: 25 -- initial value from input video
            // 1) ✅ FPS: 30 -- edited value
            var index = i + ")";
            var checkmark = isCurrentSubmenu ? "✏️"
                          : customised ? "✔️"
                          : string.Empty;
            var title = submenu.Title;
            var colon = hasValue ? ":" : string.Empty;
            var value = customised ? submenu.CurrentValueDescription.Pastel(ConsoleColor.Green)
                      : hasValue ? submenu.CurrentValueDescription
                      : string.Empty;

            string finalString = $"{index} {checkmark,-3}{title.PadRight(longestTitleLength + 1)}{colon} {value}";

            Console.WriteLine(isCurrentSubmenu ? finalString.Pastel(ConsoleColor.Yellow) : finalString);
        }
    }

    private void LoadLastSettings()
    {
        if (!File.Exists(_lastSettingsPath))
        {
            return;
        }

        string file = File.ReadAllText(_lastSettingsPath);
        var lastSettings = JsonSerializer.Deserialize<ConversionParameters>(file);

        if (lastSettings is null)
        {
            return;
        }

        lastSettings = lastSettings with { InputFilePath = _conversionParameters.InputFilePath };

        foreach (var submenu in _submenus)
        {
            submenu.LoadSavedValues(lastSettings);
            if (submenu.EditStatus is EditStatus.Customised)
            {
                _conversionParameters = submenu.SetConversionParameter(_conversionParameters);
            }
        }
    }

    private async Task StartConversion()
    {
        _conversionParameters = _submenus.FirstOrDefault(sm => sm is OutputFileNameSubmenu)?.SetConversionParameter(_conversionParameters) ?? _conversionParameters;

        var json = JsonSerializer.Serialize(_conversionParameters, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _lastSettingsPath), json);

        var options = new ProgressBarOptions
        {
            ProgressCharacter = '─',
            ProgressBarOnBottom = false
        };

        bool twoPassEncoding = _conversionParameters.MaxFileSizeInMegabytes is not null;

        var convertingMessage = twoPassEncoding
            ? "Converting... Pass 1/2"
            : "Converting...";

        using (var pbar = new ProgressBar(10_000, convertingMessage, options))
        {
            var progress = pbar.AsProgress<double>();

            _converter.ConversionProgress += (_, e) => progress.Report(e.ProgressPercentage);
            _converter.FirstPassComplete += (_, _) => pbar.Message = "Converting... Pass 2/2";
            _converter.ConversionComplete += (_, _) =>
            {
                progress.Report(1);
                pbar.Message = "Finished";
            };

            await _converter.Convert(_conversionParameters);
        }

        Console.WriteLine();
        Console.WriteLine($"Done! File saved in {Path.Combine(Environment.CurrentDirectory, _conversionParameters.OutputFileName).Pastel(ColorWriter.WelcomeColor)}");
        Console.WriteLine("Press any key to exit...");
        _ = Console.ReadKey();
        Environment.Exit(0);
    }
}

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pastel;
using VideoConverter.Cmd.Menu;
using VideoConverter.Cmd.Menu.Submenus;
using VideoConverter.Cmd.Menu.Submenus.Base;
using VideoConverter.Conversion;
using VideoConverter.Downloader;
using VideoConverter.FFmpegCheckers;
using VideoConverter.Models;
using VideoConverter.VideoInformation;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("Welcome to Video Converter 📼");

if (args is not [var filePath, ..])
{
    Console.WriteLine();
    Console.WriteLine("Did not specify input file. Close this window and just drag&drop a video file onto the Video Converter EXE to begin.");
    Console.WriteLine("Exiting in 10 seconds...");
    await Task.Delay(TimeSpan.FromSeconds(10));
    return;
}

var serviceCollection = new ServiceCollection()
    .AddTransient<IFFmpegDownloader, GithubFFmpegDownloader>()
    .AddTransient<IFFmpegChecker, XFFmpegChecker>()
    .AddTransient<IVideoMetadataRetriever, XFFmpegVideoMetadataRetriever>()
    .AddTransient<IVideoConverter, XFFMpegConverter>()
    .AddTransient<MainMenu>()
    .AddTransient<ISubmenu, MaxFileSizeMenu>()
    .AddTransient<ISubmenu, FormatSubmenu>()
    .AddTransient<ISubmenu, ResolutionSubmenu>()
    .AddTransient<ISubmenu, FpsSubmenu>()
    .AddTransient<ISubmenu, DurationSubmenu>()
    .AddTransient<ISubmenu, OutputFileNameSubmenu>()
    .AddSingleton<IOptions<InputFilePath>>(new OptionsWrapper<InputFilePath>(new(filePath)));

var serviceProvider = serviceCollection.BuildServiceProvider();

var mainMenu = serviceProvider.GetRequiredService<MainMenu>();

try
{
    await mainMenu.Loop();
}
catch (Exception e)
{
    Console.WriteLine($"⛔ {e.Message}".Pastel(ConsoleColor.DarkRed));
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

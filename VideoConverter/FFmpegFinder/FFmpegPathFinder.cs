using VideoConverter.FFmpegCheckers;

namespace VideoConverter.FFmpegFinder;

public class FFmpegPathFinder : IFFmpegFinder
{
    private FileInfo? _ffmpegExecutable = null;

    public FileInfo? FindFFmpegExecutable() => _ffmpegExecutable ??= GetFFmpegExecutable();

    private static FileInfo? GetFFmpegExecutable()
    {
        var pathDirectories = Environment.GetEnvironmentVariable("PATH")?.Split(';') ?? Enumerable.Empty<string>();
        var searchedDirectories = new[] { AppDomain.CurrentDomain.BaseDirectory }.Concat(pathDirectories);

        Console.WriteLine("Searched: " + string.Join(";", searchedDirectories));

        return searchedDirectories
            .Select(d => new FileInfo(Path.Combine(d, "ffmpeg.exe")))
            .FirstOrDefault(fileInfo => fileInfo.Exists);
    }
}

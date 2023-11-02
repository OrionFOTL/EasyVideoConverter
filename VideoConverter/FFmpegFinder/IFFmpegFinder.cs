namespace VideoConverter.FFmpegCheckers;

public interface IFFmpegFinder
{
    FileInfo? FindFFmpegExecutable();
}
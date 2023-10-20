using FFmpeg.NET;

namespace VideoConverter.FFmpegCheckers;

public class XFFmpegChecker : IFFmpegChecker
{
    public bool FFMpegExists()
    {
        try
        {
            _ = new Engine();
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}

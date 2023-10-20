using VideoConverter.VideoInformation.Models;

namespace VideoConverter.VideoInformation;

public interface IVideoMetadataRetriever
{
    Task<VideoMetadata> GetVideoData(FileInfo inputFile);
}

namespace VideoTag.Server.Helpers;

public static class Ffmpeg
{
    public static async Task ExtractStillOnDisk(
        string videoFilePath,
        string thumbnailFilePath,
        int seekInSeconds,
        int width = 0,
        int height = 0)
    {
        var sizeArg = string.Empty;
        if (width != 0 && height != 0)
        {
            sizeArg = $"-vf \"scale={width}:{height}:force_original_aspect_ratio=decrease,pad={width}:{height}:-1:-1:color=black\"";
        }
        var arguments =
            $"-y -v error -ss {seekInSeconds} -i \"{videoFilePath}\" -frames:v 1 -q:v 5 {sizeArg} \"{thumbnailFilePath}\"";
        await ProcessAsyncHelper.RunProcessAsync("ffmpeg", arguments);
    }
    
    public static async Task<byte[]> ExtractStillInMemory(
        string videoFilePath,
        int seekInSeconds,
        int width = 0,
        int height = 0)
    {
        var sizeArg = string.Empty;
        if (width != 0 && height != 0)
        {
            sizeArg = $"-vf \"scale={width}:{height}:force_original_aspect_ratio=decrease,pad={width}:{height}:-1:-1:color=black\"";
        }
        var arguments =
            $"-y -v error -ss {seekInSeconds} -i \"{videoFilePath}\" -frames:v 1 -q:v 5 {sizeArg} -f image2 -";
        return await ProcessAsyncHelper.RunProcessAndReadByteArrayAsync("ffmpeg", arguments);
    }
}
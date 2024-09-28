using System.Text.RegularExpressions;

namespace VideoTag.Server.Helpers;

public static class Ffprobe
{
    private static readonly Regex VideoResolutionRegex = new(@"^\d+x\d+");
    
    public static async Task<int> GetVideoDurationInSecondsAsync(string videoPath)
    {
        var arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{videoPath}\"";
        var result = await ProcessAsyncHelper.RunProcessAndReadStringAsync("ffprobe", arguments);
        return (int)double.Parse(result);
    }

    public static async Task<string> GetVideoResolutionAsync(string videoPath)
    {
        var arguments = $"-v error -select_streams v:0 -show_entries stream=height,width -of csv=s=x:p=0 \"{videoPath}\"";
        var output = await ProcessAsyncHelper.RunProcessAndReadStringAsync("ffprobe", arguments);
        var match = VideoResolutionRegex.Match(output);
        if (!match.Success)
            throw new Exception("Invalid video resolution");
        return match.Value;
    }
}
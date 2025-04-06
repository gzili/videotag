namespace VideoTag.Server.Helpers;

public record VideoProperties(int Width, int Height, double Framerate, double DurationInSeconds, long Bitrate);

public static class Ffprobe
{
    public static async Task<VideoProperties> GetVideoPropertiesAsync(string path)
    {
        var arguments = $"-v error -select_streams v:0 -show_entries stream=width,height,r_frame_rate,duration,bit_rate -of csv=p=0 \"{path}\"";
        var output = await ProcessAsyncHelper.RunProcessAndReadStringAsync("ffprobe", arguments);
        
        var values = output.Split(',', 5 + 1);
        if (values.Length < 5)
            throw new Exception($"Expected at least 5 values but got {values.Length}");
        
        var width = int.Parse(values[0]);
        var height = int.Parse(values[1]);
        var framerate = ParseFramerate(values[2]);
        var durationInSeconds = double.Parse(values[3]);
        var bitrate = long.Parse(values[4]);

        return new VideoProperties(width, height, framerate, durationInSeconds, bitrate);
    }

    private static double ParseFramerate(string input)
    {
        var values = input.Split('/');
        var first = int.Parse(values[0]);
        var second = int.Parse(values[1]);
        var exactFramerate = (double)first / second;
        return double.Round(exactFramerate, 2);
    }
}
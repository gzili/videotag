using System.Text.Json;
using System.Text.Json.Serialization;

namespace VideoTag.Server.Helpers;

public record VideoProperties(int Width, int Height, double Framerate, double DurationInSeconds, long Bitrate);

public class FfprobeDto
{
    public class StreamDto
    {
        [JsonPropertyName("width")]
        public required int Width { get; set; }
        
        [JsonPropertyName("height")]
        public required int Height { get; set; }
        
        [JsonPropertyName("r_frame_rate")]
        public required string RFrameRate { get; set; }
    }
    
    public class FormatDto
    {
        [JsonPropertyName("duration")]
        public required string Duration { get; set; }
        
        [JsonPropertyName("bit_rate")]
        public required string BitRate { get; set; }
    }
    
    [JsonPropertyName("format")]
    public required FormatDto Format { get; set; }
    
    [JsonPropertyName("streams")]
    public required List<StreamDto> Streams { get; set; }
}

public static class Ffprobe
{
    public static async Task<VideoProperties> GetVideoPropertiesAsync(string path)
    {
        var arguments = $"-v error -show_entries format=duration,bit_rate -select_streams v:0 -show_entries stream=width,height,r_frame_rate -of json \"{path}\"";
        var output = await ProcessAsyncHelper.RunProcessAndReadStringAsync("ffprobe", arguments);

        var dto = JsonSerializer.Deserialize<FfprobeDto>(output);

        if (dto == null)
            throw new Exception("Deserialized JSON is null");

        var format = dto.Format;
        var stream = dto.Streams.First();

        return new VideoProperties(
            stream.Width,
            stream.Height,
            ParseFramerate(stream.RFrameRate),
            double.Parse(format.Duration),
            int.Parse(format.BitRate)
        );
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
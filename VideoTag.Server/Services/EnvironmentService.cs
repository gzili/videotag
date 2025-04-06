namespace VideoTag.Server.Services;

public interface IEnvironmentService
{
    string ThumbnailsDirectoryPath { get; }
}

public class EnvironmentService(IWebHostEnvironment webHostEnvironment) : IEnvironmentService
{
    public string ThumbnailsDirectoryPath { get; } = Path.Combine(webHostEnvironment.WebRootPath, "images");
}
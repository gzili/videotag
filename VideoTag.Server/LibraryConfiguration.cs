namespace VideoTag.Server;

public class LibraryConfiguration
{
    private const string LibraryKey = "Library";
    public static string LibraryPathKey = $"{LibraryKey}:LibraryPath";
    public static string AllowedFileExtensionsKey = $"{LibraryKey}:AllowedFileExtensions";
    
    public required string LibraryPath { get; init; }
    public required List<string> AllowedFileExtensions { get; init; }
    public required string ThumbnailDirectoryPath { get; init; }
}
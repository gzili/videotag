using System.ComponentModel.DataAnnotations;

namespace VideoTag.Server.Configuration;

public class SyncOptions
{
    public const string Sync = "Sync";

    public List<string> AllowedFileExtensions { get; set; } = [];

    [Required]
    public string LibraryPath { get; set; } = null!;

    public decimal DefaultThumbnailSeek { get; set; } = 0.2m;
    
    public string? ExcludePattern { get; set; }
}
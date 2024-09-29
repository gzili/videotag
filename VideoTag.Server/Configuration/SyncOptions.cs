using System.ComponentModel.DataAnnotations;

namespace VideoTag.Server.Configuration;

public class SyncOptions
{
    public const string Sync = "Sync";
    
    [MinLength(1, ErrorMessage = "{0} must contain at least {1} folder path")]
    public List<string> Folders { get; set; } = [];

    [MinLength(1, ErrorMessage = "{0} must contain at least {1} file extension")]
    public List<string> AllowedFileExtensions { get; set; } = [];

    public decimal DefaultThumbnailSeek { get; set; } = 0.2m;
    
    public string? ExcludePattern { get; set; }
}
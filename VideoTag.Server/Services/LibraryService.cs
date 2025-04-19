using Microsoft.Extensions.Options;
using VideoTag.Server.Configuration;
using VideoTag.Server.Entities;
using VideoTag.Server.Repositories;

namespace VideoTag.Server.Services;

public interface ILibraryService
{
    Task<List<string>> FindFilesMissingFromTheLibrary();
}

public class LibraryService(IOptions<SyncOptions> syncOptions, IVideoRepository videoRepository) : ILibraryService
{
    private readonly SyncOptions _syncOptions = syncOptions.Value;
    
    public async Task<List<string>> FindFilesMissingFromTheLibrary()
    {
        var missingFiles = new List<string>();

        foreach (var folder in _syncOptions.Folders)
        {
            await AddMissingFiles(missingFiles, folder);
        }

        return missingFiles;
    }

    private async Task AddMissingFiles(List<string> missingFiles, string directory)
    {
        var videoPaths = Directory
            .EnumerateFiles(directory, "*", SearchOption.AllDirectories)
            .Where(IsAllowedFileExtension);

        if (_syncOptions.ExcludePattern != null)
        {
            videoPaths = videoPaths.Where(path => !path.Contains(_syncOptions.ExcludePattern));
        }

        foreach (var path in videoPaths)
        {
            if (!await videoRepository.ExistsByFullPath(path))
            {
                missingFiles.Add(path);
            }
        }
    }

    private bool IsAllowedFileExtension(string path)
    {
        var extension = Path.GetExtension(path)[1..];
        return _syncOptions.AllowedFileExtensions.Any(allowedExtension => extension == allowedExtension);
    }
}
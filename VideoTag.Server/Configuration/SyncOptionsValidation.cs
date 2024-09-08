using Microsoft.Extensions.Options;

namespace VideoTag.Server.Configuration;

public class SyncOptionsValidation : IValidateOptions<SyncOptions>
{
    public ValidateOptionsResult Validate(string? name, SyncOptions options)
    {
        var errorMessage = "";

        if (!Path.Exists(options.LibraryPath))
        {
            errorMessage += $"Library path \"{options.LibraryPath}\" does not exist.\n";
        }

        return !string.IsNullOrEmpty(errorMessage) ? ValidateOptionsResult.Fail(errorMessage) : ValidateOptionsResult.Success;
    }
}
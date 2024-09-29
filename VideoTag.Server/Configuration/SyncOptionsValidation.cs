using Microsoft.Extensions.Options;

namespace VideoTag.Server.Configuration;

public class SyncOptionsValidation : IValidateOptions<SyncOptions>
{
    public ValidateOptionsResult Validate(string? name, SyncOptions options)
    {
        var errorMessage = "";

        foreach (var folder in options.Folders)
        {
            if (!Path.Exists(folder))
            {
                errorMessage += $"Folder \"{folder}\" does not exist.\n";
            }
        }

        return !string.IsNullOrEmpty(errorMessage) ? ValidateOptionsResult.Fail(errorMessage) : ValidateOptionsResult.Success;
    }
}
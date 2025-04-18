using VideoTag.Server.OneTimeCommands;

namespace VideoTag.Server.Extensions;

public static class WebApplicationExtensions
{
    public static void EnsureMigrationVersionUpdated(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var command = scope.ServiceProvider.GetRequiredService<UpdateMigrationVersionCommand>();
        command.Run();
    }
}
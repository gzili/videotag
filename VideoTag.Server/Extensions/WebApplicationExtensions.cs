using VideoTag.Server.StartupCommands;

namespace VideoTag.Server.Extensions;

public static class WebApplicationExtensions
{
    public static void RunStartupCommand<T>(this WebApplication app) where T : IStartupCommand
    {
        using var scope = app.Services.CreateScope();
        var resolvedCommand = scope.ServiceProvider.GetRequiredService<T>();
        resolvedCommand.Run();
    }
}

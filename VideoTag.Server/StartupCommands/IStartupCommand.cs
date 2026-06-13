namespace VideoTag.Server.StartupCommands;

public interface IStartupCommand
{
    Task Run();
}

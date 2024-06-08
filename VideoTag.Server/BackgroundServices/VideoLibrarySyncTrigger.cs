namespace VideoTag.Server.BackgroundServices;

public class VideoLibrarySyncTrigger
{
    public event EventHandler? Triggered;

    public void OnTriggered()
    {
        Triggered?.Invoke(this, EventArgs.Empty);
    }
}
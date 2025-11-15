namespace EasyAF.Shell.Services;

/// <summary>
/// Implementation of IBackstageService that coordinates backstage close requests.
/// </summary>
public class BackstageService : IBackstageService
{
    /// <summary>
    /// Raised when a close has been requested by a backstage child.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Requests that the Backstage be closed by the shell.
    /// </summary>
    public void RequestClose()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}

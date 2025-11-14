namespace EasyAF.Shell.Services;

/// <summary>
/// Provides a simple way for nested backstage views/viewmodels to request the Backstage to close.
/// </summary>
public interface IBackstageService
{
    /// <summary>
    /// Raised when a close has been requested by a backstage child.
    /// </summary>
    event EventHandler? CloseRequested;

    /// <summary>
    /// Requests that the Backstage be closed by the shell.
    /// </summary>
    void RequestClose();
}

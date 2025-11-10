using System.Windows;

namespace EasyAF.Core.Contracts;

/// <summary>
/// Service for displaying common user dialogs (message boxes, confirmations, etc.).
/// </summary>
public interface IUserDialogService
{
    /// <summary>
    /// Shows a simple message dialog with OK button.
    /// </summary>
    void ShowMessage(string message, string title = "Information");

    /// <summary>
    /// Shows an error dialog.
    /// </summary>
    void ShowError(string message, string title = "Error");

    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons.
    /// </summary>
    bool Confirm(string message, string title = "Confirm");

    /// <summary>
    /// Shows a three-button dialog with Yes/No/Cancel options.
    /// </summary>
    MessageBoxResult ConfirmWithCancel(string message, string title = "Confirm");
}

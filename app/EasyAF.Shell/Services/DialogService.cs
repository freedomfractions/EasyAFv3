using System.Windows;
using EasyAF.Core.Contracts;

namespace EasyAF.Shell.Services;

/// <summary>
/// WPF implementation of IUserDialogService using standard MessageBox.
/// </summary>
public class UserDialogService : IUserDialogService
{
    public void ShowMessage(string message, string title = "Information")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title = "Error")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public bool Confirm(string message, string title = "Confirm")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public MessageBoxResult ConfirmWithCancel(string message, string title = "Confirm")
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
    }
}

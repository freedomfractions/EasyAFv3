using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasyAF.Shell.ViewModels;
using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Shell.Styles;

/// <summary>
/// Code-behind for FileTabStyles.xaml.
/// </summary>
public partial class FileTabStylesCode : ResourceDictionary
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileTabStylesCode"/> class.
    /// </summary>
    public FileTabStylesCode()
    {
        InitializeComponent();
    }
    
    /// <summary>
    /// Handles mouse enter on a file tab item to set hover state.
    /// </summary>
    private void FileTabItem_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is FileTabItemViewModel vm)
        {
            vm.IsHovered = true;
        }
    }
    
    /// <summary>
    /// Handles mouse leave on a file tab item to clear hover state.
    /// </summary>
    private void FileTabItem_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is FileTabItemViewModel vm)
        {
            vm.IsHovered = false;
        }
    }
    
    /// <summary>
    /// Handles click on a file tab item to activate the document.
    /// </summary>
    private void FileTabItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is FileTabItemViewModel vm)
        {
            // Find the MainWindowViewModel by walking up the visual tree
            var window = Window.GetWindow(element);
            if (window?.DataContext is MainWindowViewModel mainVm)
            {
                mainVm.DocumentManager.ActiveDocument = vm.Document;
                Log.Debug("File tab clicked: {FileName}", vm.FileName);
                e.Handled = true;
            }
        }
    }
}

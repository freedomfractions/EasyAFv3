using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasyAF.Shell.ViewModels;

namespace EasyAF.Shell.Styles;

/// <summary>
/// Code-behind for FileTabStyles.xaml event handlers.
/// </summary>
/// <remarks>
/// Handles mouse events for file tab items (hover states and group expand/collapse).
/// </remarks>
public partial class FileTabStylesEvents
{
    /// <summary>
    /// Handles mouse enter on a file tab item to set hover state.
    /// </summary>
    public static void FileTabItem_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is FileTabItemViewModel vm)
        {
            vm.IsHovered = true;
        }
    }
    
    /// <summary>
    /// Handles mouse leave on a file tab item to clear hover state.
    /// </summary>
    public static void FileTabItem_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is FileTabItemViewModel vm)
        {
            vm.IsHovered = false;
        }
    }
    
    /// <summary>
    /// Handles click on a file tab group header to toggle expand/collapse.
    /// </summary>
    public static void FileTabGroup_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is FileTabGroupViewModel vm)
        {
            vm.IsExpanded = !vm.IsExpanded;
            e.Handled = true;
        }
    }
}

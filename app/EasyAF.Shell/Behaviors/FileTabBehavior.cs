using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasyAF.Shell.ViewModels;
using Serilog;

namespace EasyAF.Shell.Behaviors;

/// <summary>
/// Attached behavior for file tab items to handle click and hover events.
/// </summary>
public static class FileTabBehavior
{
    #region EnableFileTabBehavior Attached Property

    public static readonly DependencyProperty EnableFileTabBehaviorProperty =
        DependencyProperty.RegisterAttached(
            "EnableFileTabBehavior",
            typeof(bool),
            typeof(FileTabBehavior),
            new PropertyMetadata(false, OnEnableFileTabBehaviorChanged));

    public static bool GetEnableFileTabBehavior(DependencyObject obj)
    {
        return (bool)obj.GetValue(EnableFileTabBehaviorProperty);
    }

    public static void SetEnableFileTabBehavior(DependencyObject obj, bool value)
    {
        obj.SetValue(EnableFileTabBehaviorProperty, value);
    }

    private static void OnEnableFileTabBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Border border) return;

        if ((bool)e.NewValue)
        {
            // Attach event handlers
            border.MouseEnter += OnMouseEnter;
            border.MouseLeave += OnMouseLeave;
            border.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }
        else
        {
            // Detach event handlers
            border.MouseEnter -= OnMouseEnter;
            border.MouseLeave -= OnMouseLeave;
            border.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        }
    }

    #endregion

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is FileTabItemViewModel vm)
        {
            vm.IsHovered = true;
        }
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is FileTabItemViewModel vm)
        {
            vm.IsHovered = false;
        }
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

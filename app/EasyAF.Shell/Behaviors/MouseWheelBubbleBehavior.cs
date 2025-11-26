using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyAF.Shell.Behaviors;

/// <summary>
/// Attached behavior that prevents controls (ListBox, ListView, DataGrid, etc.) from absorbing
/// mouse wheel events, allowing them to bubble up to parent ScrollViewers.
/// </summary>
/// <remarks>
/// Use this on controls that have internal scrolling but you want the parent ScrollViewer
/// to handle the scroll when the control is scrolled to its limit or when it's not scrollable.
/// 
/// Example:
/// <code>
/// &lt;ListBox behaviors:MouseWheelBubbleBehavior.IsEnabled="True" /&gt;
/// </code>
/// </remarks>
public static class MouseWheelBubbleBehavior
{
    /// <summary>
    /// Identifies the IsEnabled attached property.
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(MouseWheelBubbleBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    /// <summary>
    /// Gets the IsEnabled property value.
    /// </summary>
    public static bool GetIsEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEnabledProperty);
    }

    /// <summary>
    /// Sets the IsEnabled property value.
    /// </summary>
    public static void SetIsEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(IsEnabledProperty, value);
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if ((bool)e.NewValue)
        {
            element.PreviewMouseWheel += Element_PreviewMouseWheel;
        }
        else
        {
            element.PreviewMouseWheel -= Element_PreviewMouseWheel;
        }
    }

    private static void Element_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled || sender is not DependencyObject element)
            return;

        // Find the parent ScrollViewer
        var scrollViewer = FindParent<ScrollViewer>(element);
        if (scrollViewer == null)
            return;

        // Forward the scroll to the parent ScrollViewer
        // Divide by 3 to make scrolling smoother (same as standard scroll speed)
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (e.Delta / 3.0));
        e.Handled = true;
    }

    /// <summary>
    /// Finds the first parent of a specific type in the visual tree.
    /// </summary>
    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject? parentObject = child;

        while (parentObject != null)
        {
            if (parentObject is T parent)
                return parent;

            parentObject = VisualTreeHelper.GetParent(parentObject);
        }

        return null;
    }
}

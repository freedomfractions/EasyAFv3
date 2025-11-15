using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EasyAF.Shell.Models.Backstage;

namespace EasyAF.Shell.Behaviors;

/// <summary>
/// Attached behavior that enables drag & drop reordering within Quick Access folders.
/// Allows users to drag RadioButton items to reorder the QuickAccessFolders collection.
/// </summary>
public static class QuickAccessDragBehavior
{
    private static Point _dragStartPoint;
    private static bool _isDragging;
    private static QuickAccessFolder? _draggedItem;

    #region IsEnabled Attached Property

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(QuickAccessDragBehavior),
        new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    #endregion

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ItemsControl itemsControl)
            return;

        if ((bool)e.NewValue)
        {
            itemsControl.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            itemsControl.PreviewMouseMove += OnPreviewMouseMove;
            itemsControl.Drop += OnDrop;
            itemsControl.AllowDrop = true;
        }
        else
        {
            itemsControl.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            itemsControl.PreviewMouseMove -= OnPreviewMouseMove;
            itemsControl.Drop -= OnDrop;
            itemsControl.AllowDrop = false;
        }
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _isDragging = false;

        // Find the QuickAccessFolder being clicked
        if (e.OriginalSource is DependencyObject source)
        {
            var radioButton = FindParent<RadioButton>(source);
            if (radioButton?.DataContext is QuickAccessFolder folder)
            {
                _draggedItem = folder;
            }
        }
    }

    private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _isDragging || _draggedItem == null)
            return;

        var currentPosition = e.GetPosition(null);
        var diff = _dragStartPoint - currentPosition;

        // Check if mouse moved enough to start drag
        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            _isDragging = true;

            var dragData = new DataObject("QuickAccessFolder", _draggedItem);
            DragDrop.DoDragDrop((DependencyObject)sender, dragData, DragDropEffects.Move);

            _isDragging = false;
            _draggedItem = null;
        }
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("QuickAccessFolder"))
            return;

        var itemsControl = sender as ItemsControl;
        if (itemsControl?.ItemsSource is not ObservableCollection<QuickAccessFolder> collection)
            return;

        var draggedFolder = e.Data.GetData("QuickAccessFolder") as QuickAccessFolder;
        if (draggedFolder == null)
            return;

        // Find the target item (the one we're dropping on)
        var targetElement = GetItemAtPosition(itemsControl, e.GetPosition(itemsControl));
        var targetFolder = targetElement?.DataContext as QuickAccessFolder;

        if (targetFolder == null || draggedFolder == targetFolder)
            return;

        // Reorder in the collection
        var oldIndex = collection.IndexOf(draggedFolder);
        var newIndex = collection.IndexOf(targetFolder);

        if (oldIndex >= 0 && newIndex >= 0)
        {
            collection.Move(oldIndex, newIndex);

            // Notify ViewModel to save the new order
            if (itemsControl.DataContext is ViewModels.Backstage.OpenBackstageViewModel vm)
            {
                vm.SaveQuickAccessFolders();
            }
        }

        e.Handled = true;
    }

    #region Helper Methods

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject? parentObject = child;

        while (parentObject != null)
        {
            if (parentObject is T parent)
                return parent;

            parentObject = System.Windows.Media.VisualTreeHelper.GetParent(parentObject);
        }

        return null;
    }

    private static FrameworkElement? GetItemAtPosition(ItemsControl itemsControl, Point position)
    {
        var hitTestResult = VisualTreeHelper.HitTest(itemsControl, position);
        if (hitTestResult == null)
            return null;

        var element = hitTestResult.VisualHit as DependencyObject;
        while (element != null)
        {
            if (element is FrameworkElement fe && fe.DataContext is QuickAccessFolder)
                return fe;

            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }

    #endregion
}

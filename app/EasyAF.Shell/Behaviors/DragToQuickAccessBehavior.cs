using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EasyAF.Shell.Models.Backstage;

namespace EasyAF.Shell.Behaviors;

/// <summary>
/// Attached behavior that enables dragging items FROM Recent Files/Folders/Browser
/// TO the Quick Access section to add them.
/// </summary>
public static class DragToQuickAccessBehavior
{
    private static Point _dragStartPoint;
    private static bool _isDragging;
    private static string? _draggedFolderPath;

    #region IsEnabled Attached Property

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(DragToQuickAccessBehavior),
        new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    #endregion

    #region QuickAccessTarget Attached Property

    /// <summary>
    /// Reference to the Quick Access ItemsControl that will receive drops.
    /// Set this on the ItemsControl that contains Quick Access folders.
    /// </summary>
    public static readonly DependencyProperty QuickAccessTargetProperty = DependencyProperty.RegisterAttached(
        "QuickAccessTarget",
        typeof(ItemsControl),
        typeof(DragToQuickAccessBehavior),
        new PropertyMetadata(null, OnQuickAccessTargetChanged));

    public static ItemsControl? GetQuickAccessTarget(DependencyObject obj) => (ItemsControl?)obj.GetValue(QuickAccessTargetProperty);
    public static void SetQuickAccessTarget(DependencyObject obj, ItemsControl? value) => obj.SetValue(QuickAccessTargetProperty, value);

    #endregion

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListView listView)
            return;

        if ((bool)e.NewValue)
        {
            listView.PreviewMouseLeftButtonDown += OnSourcePreviewMouseLeftButtonDown;
            listView.PreviewMouseMove += OnSourcePreviewMouseMove;
        }
        else
        {
            listView.PreviewMouseLeftButtonDown -= OnSourcePreviewMouseLeftButtonDown;
            listView.PreviewMouseMove -= OnSourcePreviewMouseMove;
        }
    }

    private static void OnQuickAccessTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ItemsControl oldTarget)
        {
            oldTarget.DragOver -= OnTargetDragOver;
            oldTarget.Drop -= OnTargetDrop;
            oldTarget.AllowDrop = false;
        }

        if (e.NewValue is ItemsControl newTarget)
        {
            newTarget.DragOver += OnTargetDragOver;
            newTarget.Drop += OnTargetDrop;
            newTarget.AllowDrop = true;
        }
    }

    #region Source (ListView) Event Handlers

    private static void OnSourcePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _isDragging = false;
        _draggedFolderPath = null;

        // Find the item being clicked
        if (e.OriginalSource is DependencyObject source)
        {
            var listViewItem = FindParent<ListViewItem>(source);
            if (listViewItem?.DataContext != null)
            {
                // Extract folder path based on item type
                var dataContext = listViewItem.DataContext;
                
                if (dataContext is RecentFileEntry file)
                {
                    _draggedFolderPath = file.DirectoryPath;
                }
                else if (dataContext is RecentFolderEntry folder)
                {
                    _draggedFolderPath = folder.FolderPath;
                }
                else if (dataContext is FolderBrowserEntry browserEntry)
                {
                    _draggedFolderPath = browserEntry.IsFolder ? browserEntry.FullPath : browserEntry.DirectoryPath;
                }
            }
        }
    }

    private static void OnSourcePreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _isDragging || string.IsNullOrEmpty(_draggedFolderPath))
            return;

        var currentPosition = e.GetPosition(null);
        var diff = _dragStartPoint - currentPosition;

        // Check if mouse moved enough to start drag
        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            _isDragging = true;

            var dragData = new DataObject("FolderPath", _draggedFolderPath);
            DragDrop.DoDragDrop((DependencyObject)sender, dragData, DragDropEffects.Copy);

            _isDragging = false;
            _draggedFolderPath = null;
        }
    }

    #endregion

    #region Target (Quick Access) Event Handlers

    private static void OnTargetDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("FolderPath"))
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private static void OnTargetDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("FolderPath"))
            return;

        var itemsControl = sender as ItemsControl;
        if (itemsControl?.DataContext is not ViewModels.Backstage.OpenBackstageViewModel vm)
            return;

        var folderPath = e.Data.GetData("FolderPath") as string;
        if (!string.IsNullOrEmpty(folderPath))
        {
            vm.AddToQuickAccessCommand.Execute(folderPath);
        }

        e.Handled = true;
    }

    #endregion

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

    #endregion
}

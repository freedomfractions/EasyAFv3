using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
    private static InsertionAdorner? _insertionAdorner;
    private static AdornerLayer? _adornerLayer;

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
            oldTarget.DragLeave -= OnTargetDragLeave;
            oldTarget.AllowDrop = false;
        }

        if (e.NewValue is ItemsControl newTarget)
        {
            newTarget.DragOver += OnTargetDragOver;
            newTarget.Drop += OnTargetDrop;
            newTarget.DragLeave += OnTargetDragLeave;
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
            RemoveInsertionAdorner();
        }
    }

    #endregion

    #region Target (Quick Access) Event Handlers

    private static void OnTargetDragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("FolderPath"))
        {
            RemoveInsertionAdorner();
            e.Effects = DragDropEffects.None;
            return;
        }

        var itemsControl = sender as ItemsControl;
        if (itemsControl == null)
        {
            RemoveInsertionAdorner();
            return;
        }

        // Find the target item and position
        var position = e.GetPosition(itemsControl);
        var targetElement = GetItemAtPosition(itemsControl, position);
        
        if (targetElement != null)
        {
            // Determine if we should insert above or below based on Y position
            var relativePosition = e.GetPosition(targetElement);
            var insertAbove = relativePosition.Y < targetElement.ActualHeight / 2;
            
            ShowInsertionAdorner(targetElement, insertAbove, position);
        }
        else if (itemsControl.Items.Count > 0)
        {
            // If not over an item but over the control, show at bottom of last item
            var lastItem = GetLastItemContainer(itemsControl);
            if (lastItem != null)
            {
                ShowInsertionAdorner(lastItem, false, position);
            }
        }
        else
        {
            RemoveInsertionAdorner();
        }

        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    private static void OnTargetDragLeave(object sender, DragEventArgs e)
    {
        RemoveInsertionAdorner();
    }

    private static void OnTargetDrop(object sender, DragEventArgs e)
    {
        RemoveInsertionAdorner();

        if (!e.Data.GetDataPresent("FolderPath"))
            return;

        var itemsControl = sender as ItemsControl;
        if (itemsControl?.DataContext is not ViewModels.Backstage.OpenBackstageViewModel vm)
            return;
        
        if (itemsControl.ItemsSource is not System.Collections.ObjectModel.ObservableCollection<QuickAccessFolder> collection)
            return;

        var folderPath = e.Data.GetData("FolderPath") as string;
        if (string.IsNullOrEmpty(folderPath))
            return;

        // Find insertion index based on drop position
        var position = e.GetPosition(itemsControl);
        var targetElement = GetItemAtPosition(itemsControl, position);
        int insertIndex = collection.Count; // Default to end

        if (targetElement?.DataContext is QuickAccessFolder targetFolder)
        {
            var relativePosition = e.GetPosition(targetElement);
            var insertAbove = relativePosition.Y < targetElement.ActualHeight / 2;
            
            insertIndex = collection.IndexOf(targetFolder);
            if (!insertAbove)
            {
                insertIndex++; // Insert after the target
            }
        }

        // Add the folder at the determined position
        vm.AddToQuickAccessCommand.Execute(folderPath);
        
        // If it was added, move it to the correct position
        if (collection.Count > 0)
        {
            var addedFolder = collection[collection.Count - 1]; // Just added at end
            if (insertIndex < collection.Count - 1)
            {
                collection.Move(collection.Count - 1, insertIndex);
                vm.SaveQuickAccessFolders();
            }
        }

        e.Handled = true;
    }

    #endregion

    #region Insertion Adorner

    private static void ShowInsertionAdorner(FrameworkElement targetElement, bool insertAbove, Point dropPosition)
    {
        // Remove old adorner if exists
        RemoveInsertionAdorner();

        // Find the RadioButton container (not just the content)
        var radioButton = FindRadioButtonContainer(targetElement);
        if (radioButton == null)
            return;

        // Get adorner layer
        _adornerLayer = AdornerLayer.GetAdornerLayer(radioButton);
        if (_adornerLayer == null)
            return;

        // Create and add new adorner
        _insertionAdorner = new InsertionAdorner(radioButton, insertAbove);
        _adornerLayer.Add(_insertionAdorner);
    }

    private static void RemoveInsertionAdorner()
    {
        if (_insertionAdorner != null && _adornerLayer != null)
        {
            _adornerLayer.Remove(_insertionAdorner);
            _insertionAdorner = null;
            _adornerLayer = null;
        }
    }

    /// <summary>
    /// Adorner that draws a horizontal insertion line above or below an element.
    /// </summary>
    private class InsertionAdorner : Adorner
    {
        private readonly bool _insertAbove;

        public InsertionAdorner(UIElement adornedElement, bool insertAbove) : base(adornedElement)
        {
            _insertAbove = insertAbove;
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var adornedElement = AdornedElement as FrameworkElement;
            if (adornedElement == null)
                return;

            // Get accent color from theme
            var accentBrush = Application.Current.TryFindResource("AccentBrush") as Brush ?? Brushes.DodgerBlue;
            var pen = new Pen(accentBrush, 2);

            var width = adornedElement.ActualWidth;
            var y = _insertAbove ? 0 : adornedElement.ActualHeight;

            // Draw horizontal line
            drawingContext.DrawLine(pen, new Point(0, y), new Point(width, y));

            // Draw small circle at left end
            drawingContext.DrawEllipse(accentBrush, pen, new Point(4, y), 4, 4);
        }
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

            parentObject = VisualTreeHelper.GetParent(parentObject);
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
            if (element is RadioButton rb && rb.DataContext is QuickAccessFolder)
                return rb;

            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }

    private static RadioButton? FindRadioButtonContainer(DependencyObject element)
    {
        while (element != null)
        {
            if (element is RadioButton rb && rb.DataContext is QuickAccessFolder)
                return rb;

            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }

    private static FrameworkElement? GetLastItemContainer(ItemsControl itemsControl)
    {
        if (itemsControl.Items.Count == 0)
            return null;

        var lastItem = itemsControl.Items[itemsControl.Items.Count - 1];
        var container = itemsControl.ItemContainerGenerator.ContainerFromItem(lastItem) as FrameworkElement;
        
        return container;
    }

    #endregion
}

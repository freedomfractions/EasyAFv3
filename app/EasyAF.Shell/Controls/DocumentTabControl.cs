using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EasyAF.Core.Contracts;

namespace EasyAF.Shell.Controls;

/// <summary>
/// Vertical document tab strip control with drag-drop reordering and close support.
/// </summary>
public class DocumentTabControl : ListBox
{
    static DocumentTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DocumentTabControl), new FrameworkPropertyMetadata(typeof(DocumentTabControl)));
    }

    public DocumentTabControl()
    {
        SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
        SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
        SelectionChanged += (_, __) => SelectedDocument = SelectedItem as IDocument;
        AllowDrop = true;
        PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        Drop += OnDrop;
    }

    private Point _dragStartPoint;
    private bool _isDragging;

    #region SelectedDocument DP
    public IDocument? SelectedDocument
    {
        get => (IDocument?)GetValue(SelectedDocumentProperty);
        set => SetValue(SelectedDocumentProperty, value);
    }

    public static readonly DependencyProperty SelectedDocumentProperty = DependencyProperty.Register(
        nameof(SelectedDocument), typeof(IDocument), typeof(DocumentTabControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDocumentChanged));

    private static void OnSelectedDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DocumentTabControl ctrl && e.NewValue != null)
        {
            ctrl.SelectedItem = e.NewValue;
        }
    }
    #endregion

    #region CloseDocumentCommand DP
    public ICommand? CloseDocumentCommand
    {
        get => (ICommand?)GetValue(CloseDocumentCommandProperty);
        set => SetValue(CloseDocumentCommandProperty, value);
    }

    public static readonly DependencyProperty CloseDocumentCommandProperty = DependencyProperty.Register(
        nameof(CloseDocumentCommand), typeof(ICommand), typeof(DocumentTabControl), new PropertyMetadata(null));
    #endregion

    #region RoutedCommand for internal binding
    private static RoutedUICommand? _closeTabCommand;
    public static RoutedUICommand CloseTabCommand => _closeTabCommand ??= new RoutedUICommand("Close Document", nameof(CloseTabCommand), typeof(DocumentTabControl));

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        CommandBindings.Add(new CommandBinding(CloseTabCommand, OnCloseTabExecuted, CanCloseTab));
    }

    private void CanCloseTab(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = e.Parameter is IDocument;
    }

    private void OnCloseTabExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is IDocument doc && CloseDocumentCommand != null)
        {
            if (CloseDocumentCommand.CanExecute(doc))
                CloseDocumentCommand.Execute(doc);
        }
    }
    #endregion

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(this);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _isDragging) return;
        var pos = e.GetPosition(this);
        var diff = _dragStartPoint - pos;
        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            if (SelectedItem is IDocument doc)
            {
                _isDragging = true;
                DragDrop.DoDragDrop(this, doc, DragDropEffects.Move);
                _isDragging = false;
            }
        }
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(IDocument)) && ItemsSource is ObservableCollection<IDocument> collection)
        {
            var dragged = (IDocument)e.Data.GetData(typeof(IDocument))!;
            var target = GetItemAtPoint(e.GetPosition(this));
            if (target == null || ReferenceEquals(target, dragged)) return;
            var oldIndex = collection.IndexOf(dragged);
            var newIndex = collection.IndexOf(target);
            if (oldIndex >= 0 && newIndex >= 0 && oldIndex != newIndex)
            {
                collection.RemoveAt(oldIndex);
                collection.Insert(newIndex, dragged);
                SelectedDocument = dragged;
            }
        }
    }

    private IDocument? GetItemAtPoint(Point point)
    {
        var element = InputHitTest(point) as DependencyObject;
        while (element != null)
        {
            if (element is ListBoxItem item)
            {
                return (IDocument)item.Content;
            }
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }
}

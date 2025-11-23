using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EasyAF.Core.Contracts;
using EasyAF.Shell.ViewModels;
using System.Linq;
using System.Collections.Specialized;

namespace EasyAF.Shell.Controls;

/// <summary>
/// Vertical document tab strip control with grouped files and drag-drop reordering.
/// </summary>
/// <remarks>
/// <para>
/// Displays open documents in a vertical list grouped by file type (module).
/// Features:
/// - Welcome tab at the top (ungrouped)
/// - File type groups with expand/collapse
/// - Dirty indicator (colored vertical bar)
/// - File metadata (name, path, last saved)
/// - Close button (visible on hover/active)
/// </para>
/// </remarks>
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
        SelectionChanged += OnSelectionChanged;
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
        nameof(SelectedDocument), typeof(IDocument), typeof(DocumentTabControl), 
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDocumentChanged));

    private static void OnSelectedDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DocumentTabControl ctrl && e.NewValue != null)
        {
            // Update selection in grouped view
            ctrl.SyncSelectionToDocument((IDocument)e.NewValue);
        }
    }
    
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // When user clicks a tab item, sync SelectedDocument
        if (SelectedItem is FileTabItemViewModel tabItem)
        {
            SelectedDocument = tabItem.Document;
        }
    }
    
    private void SyncSelectionToDocument(IDocument document)
    {
        // Find the matching FileTabItemViewModel and select it
        foreach (var item in Items)
        {
            if (item is FileTabGroupViewModel group)
            {
                foreach (var tabItem in group.Items)
                {
                    tabItem.IsActive = ReferenceEquals(tabItem.Document, document);
                    if (tabItem.IsActive)
                    {
                        SelectedItem = tabItem;
                    }
                }
            }
            else if (item is FileTabItemViewModel tabItem)
            {
                tabItem.IsActive = ReferenceEquals(tabItem.Document, document);
                if (tabItem.IsActive)
                {
                    SelectedItem = tabItem;
                }
            }
            else if (item is WelcomeTabViewModel welcome)
            {
                // Active when document is null
                welcome.IsActive = document == null;
                if (welcome.IsActive)
                {
                    SelectedItem = welcome;
                }
            }
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

    #region Drag & Drop (disabled for now - complex with groups)
    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(this);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        // Drag-drop disabled for grouped view
        // Can be enhanced later if needed
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        // Drag-drop disabled for grouped view
    }
    #endregion
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace EasyAF.Controls.DiffGrid;

public partial class DiffGrid : Control
{
    private DataGrid? _grid;
    private ScrollViewer? _gridScrollViewer;
    private ScrollBar? _vert;
    private ScrollBar? _horiz;
    private bool _suppressSync;
    private HwndSource? _hwndSource;
    private bool _skipNextSorting; // prevents double-toggling when both events fire

    // Edit-cancel snapshot
    private object? _editingRowObj;
    private int _editingColumnIndex = -1;
    private DiffEditSide _editingSide;
    private string? _editingOriginalValue;

    // Filters toggle
    public static readonly DependencyProperty EnableColumnFiltersProperty = DependencyProperty.Register(
        nameof(EnableColumnFilters), typeof(bool), typeof(DiffGrid), new PropertyMetadata(false, OnParamsChanged));
    public bool EnableColumnFilters { get => (bool)GetValue(EnableColumnFiltersProperty); set => SetValue(EnableColumnFiltersProperty, value); }

    // Per-column filter state
    private readonly Dictionary<int, ColumnFilterState> _filters = new();

    static DiffGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DiffGrid), new FrameworkPropertyMetadata(typeof(DiffGrid)));
        CommandManager.RegisterClassCommandBinding(typeof(DiffGrid), new CommandBinding(DiffGridCommands.SetNewOnly, OnSetNewOnlyExecuted, OnCommandCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(DiffGrid), new CommandBinding(DiffGridCommands.SetOldOnly, OnSetOldOnlyExecuted, OnCommandCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(DiffGrid), new CommandBinding(DiffGridCommands.SetDiff, OnSetDiffExecuted, OnCommandCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(DiffGrid), new CommandBinding(DiffGridCommands.TogglePrimaryContent, OnTogglePrimaryContentExecuted, OnCommandCanExecute));
    }

    public DiffGrid()
    {
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnCopyExecuted, OnCopyCanExecute));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnPasteExecuted, OnPasteCanExecute));
        AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnGridPreviewKeyDown), true);
        AddHandler(TextCompositionManager.PreviewTextInputEvent, new TextCompositionEventHandler(OnGridPreviewTextInput), true);
        AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown), true);
        AddHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(OnPreviewMouseWheel), true);
        AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnGridScrollChangedRouted), true);
        SizeChanged += (_, __) => SyncScrollbarsFromGrid();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public override void OnApplyTemplate()
    {
        // Detach from previous template parts if re-applying
        DetachFromTemplateParts();

        base.OnApplyTemplate();
        _grid = GetTemplateChild("PART_Grid") as DataGrid;
        _vert = GetTemplateChild("PART_VertScroll") as ScrollBar;
        _horiz = GetTemplateChild("PART_HorizScroll") as ScrollBar;
        if (_grid != null)
        {
            ConfigureGrid(_grid);
            BuildColumns();
            _grid.ItemsSource = ItemsSource;
            _grid.Loaded += (_, __) => HookGridScrollViewer();
            _grid.BeginningEdit += OnGridBeginningEdit;
            _grid.CellEditEnding += OnGridCellEditEnding;
            _grid.CurrentCellChanged += OnGridCurrentCellChanged;
            _grid.Sorting += OnGridSorting;
            // Explicitly listen for header clicks to trigger custom sort even for template columns
            _grid.AddHandler(DataGridColumnHeader.ClickEvent, new RoutedEventHandler(OnHeaderClicked));
        }
        if (_vert != null)
        {
            _vert.ValueChanged += OnExternalScrollChanged;
            _vert.PreviewMouseLeftButtonDown += OnScrollBarPreviewMouseLeftButtonDown;
        }
        if (_horiz != null)
        {
            _horiz.ValueChanged += OnExternalScrollChanged;
            _horiz.PreviewMouseLeftButtonDown += OnScrollBarPreviewMouseLeftButtonDown;
        }
        UpdateEditState();
        EnsureFiltersInitialized();
    }

    private void DetachFromTemplateParts()
    {
        try
        {
            if (_grid != null)
            {
                _grid.BeginningEdit -= OnGridBeginningEdit;
                _grid.CellEditEnding -= OnGridCellEditEnding;
                _grid.CurrentCellChanged -= OnGridCurrentCellChanged;
                _grid.Sorting -= OnGridSorting;
                _grid.RemoveHandler(DataGridColumnHeader.ClickEvent, new RoutedEventHandler(OnHeaderClicked));
            }
            if (_vert != null)
            {
                _vert.ValueChanged -= OnExternalScrollChanged;
                _vert.PreviewMouseLeftButtonDown -= OnScrollBarPreviewMouseLeftButtonDown;
            }
            if (_horiz != null)
            {
                _horiz.ValueChanged -= OnExternalScrollChanged;
                _horiz.PreviewMouseLeftButtonDown -= OnScrollBarPreviewMouseLeftButtonDown;
            }
        }
        catch { }
    }

    private void OnScrollBarPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (sender is not ScrollBar sb) return;
            // Ignore clicks originating on the Thumb to preserve drag behavior
            if (FindAncestor<Thumb>(e.OriginalSource as DependencyObject) != null) return;
            if (_gridScrollViewer == null)
            {
                // Fallback: let default behavior occur
                return;
            }

            var pos = e.GetPosition(sb);
            var thumb = FindDescendant<Thumb>(sb);
            if (thumb == null)
            {
                // No thumb found; fallback to adjusting by LargeChange
                if (sb.Orientation == Orientation.Vertical)
                {
                    double half = sb.ActualHeight /2.0;
                    if (pos.Y > half) _gridScrollViewer.PageDown(); else _gridScrollViewer.PageUp();
                }
                else
                {
                    double half = sb.ActualWidth /2.0;
                    if (pos.X > half) _gridScrollViewer.PageRight(); else _gridScrollViewer.PageLeft();
                }
                e.Handled = true;
                return;
            }

            // Compute thumb bounds relative to the scrollbar
            var topLeft = thumb.TranslatePoint(new Point(0,0), sb);
            var bottomRight = thumb.TranslatePoint(new Point(thumb.ActualWidth, thumb.ActualHeight), sb);

            if (sb.Orientation == Orientation.Vertical)
            {
                if (pos.Y > bottomRight.Y)
                    _gridScrollViewer.PageDown();
                else if (pos.Y < topLeft.Y)
                    _gridScrollViewer.PageUp();
                else
                    return; // Click inside thumb area – allow default
            }
            else
            {
                if (pos.X > bottomRight.X)
                    _gridScrollViewer.PageRight();
                else if (pos.X < topLeft.X)
                    _gridScrollViewer.PageLeft();
                else
                    return; // Click inside thumb area – allow default
            }

            e.Handled = true;
        }
        catch { }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var src = PresentationSource.FromVisual(this) as HwndSource;
            if (src != null && _hwndSource != src)
            { _hwndSource = src; _hwndSource.AddHook(WndProc); }
        } catch { }
        EnsureFiltersInitialized();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Ensure any in-flight edits are committed so data is not lost when view unloads
            CommitPendingEdits();

            // Close any open filter popups to avoid orphan visuals
            CloseAllFilterPopups();
            // Detach events from template parts
            DetachFromTemplateParts();
            if (_hwndSource != null) { _hwndSource.RemoveHook(WndProc); _hwndSource = null; }
        }
        catch { }
    }

    private void CloseAllFilterPopups()
    {
        try
        {
            if (_grid == null) return;
            for (int i =0; i < _grid.Columns.Count; i++)
            {
                var headerContent = _grid.Columns[i].Header;
                if (headerContent is HeaderModel hm && hm.FilterPopup != null)
                {
                    try { hm.FilterPopup.IsOpen = false; } catch { }
                    hm.FilterPopup = null;
                }
            }
        }
        catch { }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_MOUSEHWHEEL =0x020E;
        if (msg == WM_MOUSEHWHEEL)
        {
            if (!IsMouseOver) return IntPtr.Zero;
            int delta = (short)((wParam.ToInt64() >>16) &0xFFFF);
            if (delta !=0) { HandleHorizontalWheel(delta); handled = true; }
            return IntPtr.Zero;
        }
        return IntPtr.Zero;
    }

    private void ConfigureGrid(DataGrid grid)
    {
        grid.AutoGenerateColumns = false;
        grid.HeadersVisibility = DataGridHeadersVisibility.Column;
        // Enable header click sorting; we handle Sorting to avoid SortMemberPath binding errors
        grid.CanUserSortColumns = true;
        grid.CanUserResizeColumns = true;
        grid.IsReadOnly = !IsEditingEnabled;
        grid.CanUserAddRows = false;
        grid.CanUserDeleteRows = false;
        grid.EnableRowVirtualization = true;
        grid.EnableColumnVirtualization = true;
        grid.GridLinesVisibility = DataGridGridLinesVisibility.None;
        grid.AlternationCount =2;
        grid.RowHeaderWidth =0;
        grid.IsSynchronizedWithCurrentItem = false;
        grid.SelectionUnit = DataGridSelectionUnit.Cell;
        grid.SelectionMode = DataGridSelectionMode.Extended;
        grid.ClipboardCopyMode = DataGridClipboardCopyMode.ExcludeHeader;
        grid.Focusable = true;
        grid.IsTabStop = true;
        grid.SetValue(KeyboardNavigation.TabNavigationProperty, KeyboardNavigationMode.None);
        grid.SetValue(KeyboardNavigation.ControlTabNavigationProperty, KeyboardNavigationMode.None);
        grid.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
        grid.SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
        grid.SetValue(ScrollViewer.CanContentScrollProperty, true);
        grid.SetValue(ScrollViewer.IsDeferredScrollingEnabledProperty, true);

        var rowStyle = new Style(typeof(DataGridRow));
        rowStyle.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
        var altTrigger = new Trigger { Property = ItemsControl.AlternationIndexProperty, Value =1 };
        altTrigger.Setters.Add(new Setter(Control.BackgroundProperty, Application.Current.TryFindResource("RowBandingBrush") as Brush));
        rowStyle.Triggers.Add(altTrigger);
        grid.RowStyle = rowStyle;

        var cellStyle = new Style(typeof(DataGridCell));
        cellStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(4,2,4,2)));
        var selTrigger = new Trigger { Property = DataGridCell.IsSelectedProperty, Value = true };
        selTrigger.Setters.Add(new Setter(Panel.ZIndexProperty,1));
        cellStyle.Triggers.Add(selTrigger);
        grid.CellStyle = cellStyle;

        grid.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnCopyExecuted, OnCopyCanExecute));
        grid.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnPasteExecuted, OnPasteCanExecute));
        grid.TargetUpdated += (_, __) => BuildColumns();
    }

    private void OnHeaderClicked(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_grid == null) return;
            var src = e.OriginalSource as DependencyObject;

            // Ignore clicks coming from the filter button or the resize grippers
            if (FindAncestor<Button>(src) != null) return;
            var thumb = FindAncestor<Thumb>(src);
            if (thumb != null)
            {
                var name = (thumb as FrameworkElement)?.Name;
                if (!string.IsNullOrEmpty(name) && name.IndexOf("HeaderGripper", StringComparison.OrdinalIgnoreCase) >= 0)
                    return;
            }

            var header = FindAncestor<DataGridColumnHeader>(src);
            if (header?.Column == null) return;

            _skipNextSorting = true; // prevent subsequent Sorting event from toggling again
            var current = header.Column.SortDirection ?? ListSortDirection.Descending; // so next becomes Asc
            var nextDir = current == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            ApplyCustomSortForColumn(header.Column, nextDir);
            e.Handled = true;
        }
        catch { }
    }

    private void OnGridSorting(object? sender, DataGridSortingEventArgs e)
    {
        try
        {
            if (_grid == null) { e.Handled = true; return; }
            if (_skipNextSorting) { _skipNextSorting = false; e.Handled = true; return; }
            ApplyCustomSortForColumn(e.Column, e.Column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending);
            e.Handled = true;
        }
        catch { e.Handled = true; }
    }

    private void ApplyCustomSortForColumn(DataGridColumn column, ListSortDirection direction)
    {
        if (_grid == null) return;
        int colIndex = _grid.Columns.IndexOf(column);
        if (colIndex < 0) return;

        // Clear sort glyphs on other columns
        foreach (var c in _grid.Columns)
        {
            if (!ReferenceEquals(c, column)) c.SortDirection = null;
        }
        column.SortDirection = direction;

        var view = CollectionViewSource.GetDefaultView(_grid.ItemsSource) as ListCollectionView;
        if (view != null)
        {
            bool ascending = direction == ListSortDirection.Ascending;
            view.CustomSort = new ColumnCellComparer(this, colIndex, ascending, PrimarySide == DiffEditSide.New);
            view.Refresh();
        }
    }

    private sealed class ColumnCellComparer : System.Collections.IComparer
    {
        private readonly DiffGrid _owner;
        private readonly int _col;
        private readonly bool _asc;
        private readonly bool _useNew;
        public ColumnCellComparer(DiffGrid owner, int col, bool ascending, bool useNew)
        { _owner = owner; _col = col; _asc = ascending; _useNew = useNew; }
        public int Compare(object? x, object? y)
        {
            string sx = _owner.GetCellAsStringSafe(x, _col, _useNew) ?? string.Empty;
            string sy = _owner.GetCellAsStringSafe(y, _col, _useNew) ?? string.Empty;
            int cmp = StringComparer.CurrentCultureIgnoreCase.Compare(sx, sy);
            return _asc ? cmp : -cmp;
        }
    }

    // Safe wrapper for comparer (cannot access private in another nested context during partial compilation)
    private string? GetCellAsStringSafe(object? row, int colIndex, bool useNew)
    {
        if (row == null) return string.Empty;
        // Temporarily flip PrimarySide for retrieval to avoid duplicating access code
        var prev = PrimarySide;
        try { PrimarySide = useNew ? DiffEditSide.New : DiffEditSide.Old; return GetCellAsString(row, colIndex); }
        catch { return string.Empty; }
        finally { PrimarySide = prev; }
    }

    public static readonly DependencyProperty IsEditingEnabledProperty = DependencyProperty.Register(
        nameof(IsEditingEnabled), typeof(bool), typeof(DiffGrid), new PropertyMetadata(false, OnIsEditingEnabledChanged));
    public bool IsEditingEnabled { get => (bool)GetValue(IsEditingEnabledProperty); set => SetValue(IsEditingEnabledProperty, value); }
    private static void OnIsEditingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { if (d is DiffGrid dg && dg._grid != null) dg.UpdateEditState(); }

    public static readonly DependencyProperty ViewModeProperty = DependencyProperty.Register(
        nameof(ViewMode), typeof(DiffViewMode), typeof(DiffGrid), new PropertyMetadata(DiffViewMode.NewAndOld, OnParamsChanged));
    public DiffViewMode ViewMode { get => (DiffViewMode)GetValue(ViewModeProperty); set => SetValue(ViewModeProperty, value); }

    public static readonly DependencyProperty PrimarySideProperty = DependencyProperty.Register(
        nameof(PrimarySide), typeof(DiffEditSide), typeof(DiffGrid), new PropertyMetadata(DiffEditSide.New, OnParamsChanged));
    public DiffEditSide PrimarySide { get => (DiffEditSide)GetValue(PrimarySideProperty); set => SetValue(PrimarySideProperty, value); }

    public static readonly DependencyProperty EditSideProperty = DependencyProperty.Register(
        nameof(EditSide), typeof(DiffEditSide), typeof(DiffGrid), new PropertyMetadata(DiffEditSide.New));
    public DiffEditSide EditSide { get => (DiffEditSide)GetValue(EditSideProperty); set => SetValue(EditSideProperty, value); }

    public static readonly DependencyProperty IsOldEditLockedProperty = DependencyProperty.Register(
        nameof(IsOldEditLocked), typeof(bool), typeof(DiffGrid), new PropertyMetadata(false));
    public bool IsOldEditLocked { get => (bool)GetValue(IsOldEditLockedProperty); set => SetValue(IsOldEditLockedProperty, value); }

    public static readonly DependencyProperty PasteRespectsEditSideProperty = DependencyProperty.Register(
        nameof(PasteRespectsEditSide), typeof(bool), typeof(DiffGrid), new PropertyMetadata(true));
    public bool PasteRespectsEditSide { get => (bool)GetValue(PasteRespectsEditSideProperty); set => SetValue(PasteRespectsEditSideProperty, value); }

    public static readonly DependencyProperty AllowPasteAppendRowsProperty = DependencyProperty.Register(
        nameof(AllowPasteAppendRows), typeof(bool), typeof(DiffGrid), new PropertyMetadata(true));
    public bool AllowPasteAppendRows { get => (bool)GetValue(AllowPasteAppendRowsProperty); set => SetValue(AllowPasteAppendRowsProperty, value); }

    public static readonly DependencyProperty PasteRowFactoryProperty = DependencyProperty.Register(
        nameof(PasteRowFactory), typeof(Func<int, int, object>), typeof(DiffGrid), new PropertyMetadata(null));
    public Func<int, int, object>? PasteRowFactory { get => (Func<int, int, object>?)GetValue(PasteRowFactoryProperty); set => SetValue(PasteRowFactoryProperty, value); }

    public static readonly DependencyProperty DefaultTextWrappingProperty = DependencyProperty.Register(
        nameof(DefaultTextWrapping), typeof(TextWrapping), typeof(DiffGrid), new PropertyMetadata(TextWrapping.NoWrap, OnParamsChanged));
    public TextWrapping DefaultTextWrapping { get => (TextWrapping)GetValue(DefaultTextWrappingProperty); set => SetValue(DefaultTextWrappingProperty, value); }

    public static readonly DependencyProperty WrappedFieldKeysProperty = DependencyProperty.Register(
        nameof(WrappedFieldKeys), typeof(ISet<string>), typeof(DiffGrid), new PropertyMetadata(null, OnParamsChanged));
    public ISet<string>? WrappedFieldKeys { get => (ISet<string>?)GetValue(WrappedFieldKeysProperty); set => SetValue(WrappedFieldKeysProperty, value); }

    public static readonly DependencyProperty DescriptorProperty = DependencyProperty.Register(
        nameof(Descriptor), typeof(IDiffDescriptor), typeof(DiffGrid), new PropertyMetadata(null, OnParamsChanged));
    public IDiffDescriptor? Descriptor { get => (IDiffDescriptor?)GetValue(DescriptorProperty); set => SetValue(DescriptorProperty, value); }

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(DiffGrid), new PropertyMetadata(null, OnItemsSourceChanged));
    public IEnumerable? ItemsSource { get => (IEnumerable?)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DiffGrid dg || dg._grid == null) return;

        // Avoid redundant resets
        if (ReferenceEquals(e.NewValue, e.OldValue)) return;

        // Defer the swap to a safe moment to avoid CollectionView DeferRefresh exceptions
        dg.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => dg.ReplaceItemsSourceSafe(e.NewValue as IEnumerable)));
    }

    private void ReplaceItemsSourceSafe(IEnumerable? newSource)
    {
        if (_grid == null) return;

        try
        {
            // Try to end any pending add/edit transactions before swapping sources
            try
            {
                if (_grid.Items is IEditableCollectionView ecv)
                {
                    if (ecv.IsAddingNew) { try { ecv.CommitNew(); } catch { ecv.CancelNew(); } }
                    if (ecv.IsEditingItem) { try { ecv.CommitEdit(); } catch { ecv.CancelEdit(); } }
                }

                // Also ask DataGrid to commit any current edits
                try { _grid.CommitEdit(DataGridEditingUnit.Cell, true); } catch { }
                try { _grid.CommitEdit(DataGridEditingUnit.Row, true); } catch { }
                try { _grid.CancelEdit(); } catch { }
            }
            catch { }

            // Now perform the ItemsSource swap
            _grid.ItemsSource = newSource;

            // After swap, sync UI parts
            Dispatcher.BeginInvoke(new Action(() => { SyncScrollbarsFromGrid(); EnsureFiltersInitialized(); }), DispatcherPriority.Background);
        }
        catch (InvalidOperationException)
        {
            // If the collection is still in an edit transaction, try again a bit later
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => ReplaceItemsSourceSafe(newSource)));
        }
        catch
        {
            // Swallow - better to keep UI responsive than crash; caller will rebind later
        }
    }

    public static readonly DependencyProperty RowAccessorSelectorProperty = DependencyProperty.Register(
        nameof(RowAccessorSelector), typeof(Func<object, IDiffRowAccessor>), typeof(DiffGrid), new PropertyMetadata(null));
    public Func<object, IDiffRowAccessor>? RowAccessorSelector { get => (Func<object, IDiffRowAccessor>?)GetValue(RowAccessorSelectorProperty); set => SetValue(RowAccessorSelectorProperty, value); }

    [Obsolete("ValueOrder is deprecated and will be removed; roles are driven by PrimarySide/ViewMode.")]
    public static readonly DependencyProperty ValueOrderProperty = DependencyProperty.Register(
        nameof(ValueOrder), typeof(DiffValueOrder), typeof(DiffGrid), new PropertyMetadata(DiffValueOrder.NewTop, OnParamsChanged));
    [Obsolete("ValueOrder is deprecated and will be removed; roles are driven by PrimarySide/ViewMode.")]
    public DiffValueOrder ValueOrder { get => (DiffValueOrder)GetValue(ValueOrderProperty); set => SetValue(ValueOrderProperty, value); }

    public static readonly DependencyProperty RangePasteWhileEditingProperty = DependencyProperty.Register(
        nameof(RangePasteWhileEditing), typeof(RangePasteWhileEditingMode), typeof(DiffGrid), new PropertyMetadata(RangePasteWhileEditingMode.RespectEditor));
    public RangePasteWhileEditingMode RangePasteWhileEditing { get => (RangePasteWhileEditingMode)GetValue(RangePasteWhileEditingProperty); set => SetValue(RangePasteWhileEditingProperty, value); }

    private static void OnParamsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    { if (d is DiffGrid dg && dg._grid != null) { dg.BuildColumns(); dg.EnsureFiltersInitialized(); } }

    private static MultiBinding CreateEditRegionMultiBinding(string parameter)
    { var mb = new MultiBinding { Converter = EditRegionVisibilityConverter.Instance, ConverterParameter = parameter }; mb.Bindings.Add(new Binding(nameof(ViewMode)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DiffGrid), 1) }); mb.Bindings.Add(new Binding(nameof(EditSide)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DiffGrid), 1) }); return mb; }

    private static MultiBinding CreateRowMultiBinding(string which)
    { var mb = new MultiBinding { Converter = RoleRowIndexConverter.Instance, ConverterParameter = which }; mb.Bindings.Add(new Binding(nameof(ViewMode)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DiffGrid), 1) }); mb.Bindings.Add(new Binding(nameof(PrimarySide)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DiffGrid), 1) }); return mb; }

    private static MultiBinding CreateBottomMarginMultiBinding(string which)
    { var mb = new MultiBinding { Converter = RoleBottomMarginConverter.Instance, ConverterParameter = which }; mb.Bindings.Add(new Binding(nameof(ViewMode)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DiffGrid), 1) }); mb.Bindings.Add(new Binding(nameof(PrimarySide)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DiffGrid), 1) }); return mb; }

    private static MultiBinding CreateFontSizeMultiBinding(string which)
    { var mb = new MultiBinding { Converter = FontSizeByRowConverter.Instance, ConverterParameter = which }; mb.Bindings.Add(new Binding(nameof(ViewMode)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DiffGrid), 1) }); mb.Bindings.Add(new Binding(nameof(PrimarySide)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DiffGrid), 1) }); return mb; }

    private static void OnCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    { e.CanExecute = sender is DiffGrid; e.Handled = true; }

    private static void OnSetNewOnlyExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is DiffGrid dg)
        {
            dg.WithSelectionPreserved(() => { dg.PrimarySide = DiffEditSide.New; dg.ViewMode = DiffViewMode.NewOnly; });
            dg.RebuildAllFilterValues(); dg.ApplyFilters();
        }
    }

    private static void OnSetOldOnlyExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is DiffGrid dg)
        {
            dg.WithSelectionPreserved(() => { dg.PrimarySide = DiffEditSide.Old; dg.ViewMode = DiffViewMode.OldOnly; });
            dg.RebuildAllFilterValues(); dg.ApplyFilters();
        }
    }

    private static void OnSetDiffExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is DiffGrid dg)
        {
            dg.WithSelectionPreserved(() => { dg.ViewMode = DiffViewMode.NewAndOld; });
            dg.RebuildAllFilterValues(); dg.ApplyFilters();
        }
    }

    private static void OnTogglePrimaryContentExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is DiffGrid dg)
        {
            dg.WithSelectionPreserved(() =>
            {
                if (dg.ViewMode == DiffViewMode.NewAndOld) dg.PrimarySide = dg.PrimarySide == DiffEditSide.New ? DiffEditSide.Old : DiffEditSide.New;
                else if (dg.ViewMode == DiffViewMode.NewOnly) { dg.PrimarySide = DiffEditSide.Old; dg.ViewMode = DiffViewMode.OldOnly; }
                else if (dg.ViewMode == DiffViewMode.OldOnly) { dg.PrimarySide = DiffEditSide.New; dg.ViewMode = DiffViewMode.NewOnly; }
            });
            dg.RebuildAllFilterValues(); dg.ApplyFilters();
        }
    }

    public void CommitPendingEdits()
    {
        try
        {
            if (_grid == null) return;
            // Attempt to commit cell and row edits
            try { _grid.CommitEdit(DataGridEditingUnit.Cell, true); } catch { }
            try { _grid.CommitEdit(DataGridEditingUnit.Row, true); } catch { }

            // Commit edits on the underlying collection view if applicable
            if (_grid.Items is IEditableCollectionView ecv)
            {
                try { if (ecv.IsEditingItem) ecv.CommitEdit(); } catch { try { ecv.CancelEdit(); } catch { } }
                try { if (ecv.IsAddingNew) ecv.CommitNew(); } catch { try { ecv.CancelNew(); } catch { } }
            }
        }
        catch { }
    }
}

public enum RangePasteWhileEditingMode { RespectEditor, MultiCellOverridesEditor, AlwaysRange, ShortcutOnly }

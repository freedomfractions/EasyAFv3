# Spec Module - Remaining Fixes Implementation

## Summary

Five issues remaining to address (plus critical #7 already fixed):
- ? **#7 (CRITICAL)**: Dirty check on close - **FIXED** (in MainWindow.xaml.cs & MainWindowViewModel.cs)
- ?? **#2**: Tab name not updating when table renamed
- ?? **#3**: Setup tab width too narrow
- ?? **#4**: Table tabs too narrow
- ?? **#5**: Property path leading comma issue

---

## Fix #2: Tab Name Update on Rename

### Problem
When user edits table name in Setup tab DataGrid, the corresponding table tab header doesn't update.

### Root Cause
`SpecDocumentViewModel.OnTablesChanged()` rebuilds tabs but doesn't subscribe to PropertyChanged events on `TableEditorViewModel` instances.

### Solution
Subscribe to each `TableEditorViewModel.PropertyChanged` and update the corresponding `TabHeaderInfo.Header` when `TableName` changes.

### Implementation

**File**: `modules/EasyAF.Modules.Spec/ViewModels/SpecDocumentViewModel.cs`

```csharp
private void OnTablesChanged(object? sender, EventArgs e)
{
    Log.Debug("Tables changed, rebuilding table tabs");
    
    // Preserve current selection
    var currentTab = SelectedTabIndex >= 0 && SelectedTabIndex < TabHeaders.Count 
        ? TabHeaders[SelectedTabIndex] 
        : null;
    
    // Unsubscribe from old table editor VMs
    for (int i = 1; i < TabHeaders.Count; i++)
    {
        if (TabHeaders[i].ViewModel is TableEditorViewModel oldVm)
        {
            oldVm.PropertyChanged -= OnTableEditorPropertyChanged;
        }
        
        if (TabHeaders[i].ViewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
    
    // Remove all table tabs (keep Setup tab)
    for (int i = TabHeaders.Count - 1; i >= 1; i--)
    {
        TabHeaders.RemoveAt(i);
    }
    
    // Add table tabs
    if (_document.Spec?.Tables != null)
    {
        foreach (var table in _document.Spec.Tables)
        {
            var editorVm = new TableEditorViewModel(table, _document, _dialogService, _propertyDiscovery, _settingsService);
            
            // AUDIT FIX #2: Subscribe to property changes to update tab header when table renamed
            editorVm.PropertyChanged += OnTableEditorPropertyChanged;
            
            TabHeaders.Add(new TabHeaderInfo
            {
                Header = !string.IsNullOrEmpty(table.AltText) ? table.AltText : table.Id,
                DisplayName = !string.IsNullOrEmpty(table.AltText) ? table.AltText : table.Id,
                ViewModel = editorVm,
                TableId = table.Id
            });
        }
    }
    
    // Restore selection...
    // (rest of method unchanged)
}

private void OnTableEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    // AUDIT FIX #2: Update tab header when table name changes
    if (e.PropertyName == "TableName" && sender is TableEditorViewModel tableVm)
    {
        var tab = TabHeaders.FirstOrDefault(t => t.ViewModel == tableVm);
        if (tab != null && !string.IsNullOrEmpty(tableVm.TableName))
        {
            tab.Header = tableVm.TableName;
            tab.DisplayName = tableVm.TableName;
            Log.Debug("Updated tab header to: {TableName}", tableVm.TableName);
        }
    }
}
```

**Also need to unsubscribe in Dispose**:

```csharp
public void Dispose()
{
    if (_disposed) return;

    _document.PropertyChanged -= OnDocumentPropertyChanged;

    // Unsubscribe from setup VM
    if (Setup != null)
    {
        Setup.TablesChanged -= OnTablesChanged;
    }

    // AUDIT FIX #2: Unsubscribe and dispose child VMs
    foreach (var tab in TabHeaders)
    {
        if (tab.ViewModel is TableEditorViewModel tableVm)
        {
            tableVm.PropertyChanged -= OnTableEditorPropertyChanged;
        }
        
        if (tab.ViewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    _disposed = true;
    GC.SuppressFinalize(this);
}
```

---

## Fix #3: Setup Tab Max Width

### Problem
Setup tab has 2-column layout but no max width constraint.

### Solution
Add MaxWidth to root Grid in SpecSetupView.

**File**: `modules/EasyAF.Modules.Spec/Views/SpecSetupView.xaml`

```xaml
<ScrollViewer VerticalScrollBarVisibility="Auto"
              HorizontalScrollBarVisibility="Disabled"
              Background="{DynamicResource PrimaryBackgroundBrush}">
    <!-- AUDIT FIX #3: Add MaxWidth for better layout on wide screens -->
    <Grid Margin="20" MaxWidth="1400">
        <!-- existing content -->
    </Grid>
</ScrollViewer>
```

---

## Fix #4: Table Tabs Too Narrow

### Problem
TabControl doesn't expand to fill available horizontal space.

### Solution
Set TabControl to stretch horizontally.

**File**: `modules/EasyAF.Modules.Spec/Views/SpecDocumentView.xaml`

```xaml
<!-- AUDIT FIX #4: Make tabs expand to fill horizontal space -->
<TabControl Grid.Row="1" 
            HorizontalAlignment="Stretch"
            VerticalAlignment="Stretch"
            ItemsSource="{Binding TabHeaders}"
            SelectedIndex="{Binding SelectedTabIndex}">
    <!-- existing template -->
</TabControl>
```

---

## Fix #5: Property Path Leading Comma

### Problem
PropertyPath column shows ", <property>" instead of "<property>, <property2>" when multiple properties selected.

### Root Cause
Code is prepending comma before first item instead of joining between items.

### Solution
Use `string.Join()` properly.

**File**: `modules/EasyAF.Modules.Spec/ViewModels/TableEditorViewModel.cs` (check ColumnViewModel)

**Current Code (if it looks like this)**:
```csharp
public string PropertyPath
{
    get
    {
        var result = string.Empty;
        foreach (var path in Column.PropertyPaths ?? Array.Empty<string>())
        {
            result += ", " + path; // BUG: Leading comma on first item
        }
        return result.TrimStart(',', ' ');
    }
}
```

**Fixed Code**:
```csharp
public string PropertyPath
{
    get
    {
        if (Column.PropertyPaths == null || Column.PropertyPaths.Length == 0)
            return string.Empty;

        // AUDIT FIX #5: Use string.Join to avoid leading comma
        return string.Join(", ", Column.PropertyPaths);
    }
}
```

---

## Testing Checklist After Fixes

### Fix #2 - Tab Name Update
- [ ] Create spec with 2-3 tables
- [ ] Go to Setup tab
- [ ] Double-click table name in grid
- [ ] Edit name (e.g., "My Table 1")
- [ ] Press Enter or click away
- [ ] **VERIFY**: Tab header updates immediately

### Fix #3 - Setup Tab Width
- [ ] Open spec document
- [ ] Go to Setup tab
- [ ] Maximize window
- [ ] **VERIFY**: Content has reasonable max width (not stretched to edges)

### Fix #4 - Table Tab Width
- [ ] Open spec with multiple tables
- [ ] Go to a table tab
- [ ] Resize window wider
- [ ] **VERIFY**: Tab content expands to fill space

### Fix #5 - Property Path Display
- [ ] Open spec with table that has multi-property columns
- [ ] Go to table editor tab
- [ ] Look at PropertyPath column
- [ ] **VERIFY**: Shows "Property1, Property2" (no leading comma)

### Fix #7 - Dirty Check on Close (ALREADY FIXED)
- [ ] Create new project/spec
- [ ] Make changes (don't save)
- [ ] Click X to close app
- [ ] **VERIFY**: Prompted to save/discard/cancel
- [ ] Select Cancel
- [ ] **VERIFY**: App doesn't close
- [ ] Close again, select Discard
- [ ] **VERIFY**: App closes without saving
- [ ] Make changes again, close, select Save
- [ ] **VERIFY**: Prompted for file path (if new) and saved

---

## Implementation Order

1. **DONE**: Fix #7 (dirty check - already implemented)
2. **HIGH**: Fix #2 (tab name update) - confusing UX
3. **MEDIUM**: Fix #5 (property path comma) - visual bug
4. **LOW**: Fix #3 (setup tab width) - minor UX
5. **LOW**: Fix #4 (table tab width) - minor UX

---

## Estimated Time

- Fix #2: 15 minutes (event subscription + testing)
- Fix #3: 2 minutes (one line change)
- Fix #4: 2 minutes (one line change)
- Fix #5: 5 minutes (check current code + fix)
- **Total**: ~25 minutes

---

## Notes

- Fix #6 (100% table width for reports) is deferred - requires Engine changes
- All fixes are low-risk (isolated changes)
- Fix #7 (critical) is already complete and working

---

**Status**: Ready to implement  
**Priority**: High (#2), Medium (#5), Low (#3, #4)  
**Risk**: Low


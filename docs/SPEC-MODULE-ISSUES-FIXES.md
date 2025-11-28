# Spec Module Issues & Fixes

## Issues Found During Testing

### 1. ? Mode Column Theming (FIXED)
**Issue**: Mode column in tables table is not themed  
**Status**: Actually properly themed in XAML - ComboBox inherits theme from CommonControls.xaml  
**Verification Needed**: Test to confirm dropdown appearance matches theme

### 2. ?? Tab Name Not Updating When Table Renamed
**Issue**: When renaming table in DataGrid, the corresponding tab header doesn't update  
**Root Cause**: No property change notification link between Setup tab and Document tab collection  
**Fix Required**: Add event subscription in SpecDocumentViewModel to listen for table renames

### 3. ?? Setup Tab Width
**Issue**: Setup tab should have wider max width (2-column layout)  
**Current**: Default width  
**Desired**: Match Project module Summary tab width  
**Fix**: Add MaxWidth to SpecSetupView root Grid

### 4. ?? Table Tabs Too Narrow
**Issue**: Table tabs should expand with application window  
**Root Cause**: TabControl using default sizing  
**Fix**: Set TabControl to HorizontalAlignment="Stretch" in SpecDocumentView

### 5. ?? Property Path Display Issue
**Issue**: PropertyPaths column shows ", <property>" instead of "<property>, <property2>"  
**Root Cause**: Comma-space added before first item instead of between items  
**Fix**: Update ViewModel to use string.Join() properly

### 6. ?? Table Width for Reports (TODO)
**Issue**: Need logic to force tables to 100% width for report mode  
**Status**: Deferred - requires Engine changes  
**Note**: Track for future enhancement

### 7. ?? **CRITICAL** - No Dirty Check on App Close
**Issue**: App doesn't check for unsaved changes before closing  
**Risk**: Data loss  
**Fix Required**: Add Closing event handler to MainWindow

---

## Fix Implementation Plan

### Fix #2: Tab Name Update on Rename
**File**: `modules/EasyAF.Modules.Spec/ViewModels/SpecDocumentViewModel.cs`
**Change**: Subscribe to PropertyChanged on each TableDefinitionViewModel

### Fix #3: Setup Tab Width
**File**: `modules/EasyAF.Modules.Spec/Views/SpecSetupView.xaml`
**Change**: Add MaxWidth="1400" to root Grid

### Fix #4: Table Tab Width
**File**: `modules/EasyAF.Modules.Spec/Views/SpecDocumentView.xaml`
**Change**: Set TabControl HorizontalAlignment="Stretch"

### Fix #5: Property Path Display
**File**: `modules/EasyAF.Modules.Spec/ViewModels/TableEditorViewModel.cs`
**Change**: Fix ColumnViewModel.PropertyPath getter to use string.Join() correctly

### Fix #7: Dirty Check on Close (CRITICAL)
**File**: `app/EasyAF.Shell/ViewModels/MainWindowViewModel.cs`
**Change**: Add Window.Closing handler with dirty document check

---

## Detailed Fixes

### Fix #2: Tab Name Update
```csharp
// In SpecDocumentViewModel.OnTablesChanged()
private void OnTablesChanged()
{
    // Unsubscribe from old VMs
    foreach (var tab in TabHeaders.Where(t => t.DataType != null))
    {
        if (tab.ViewModel is TableEditorViewModel oldVm)
        {
            oldVm.PropertyChanged -= OnTableEditorPropertyChanged;
        }
    }

    // Create new tabs...
    foreach (var tableSpec in _document.Spec.Tables)
    {
        var tableVm = new TableEditorViewModel(tableSpec, _document);
        tableVm.PropertyChanged += OnTableEditorPropertyChanged;
        // ...
    }
}

private void OnTableEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == "TableName" && sender is TableEditorViewModel tableVm)
    {
        var tab = TabHeaders.FirstOrDefault(t => t.ViewModel == tableVm);
        if (tab != null)
        {
            tab.Header = tableVm.TableName;
        }
    }
}
```

### Fix #3: Setup Tab Width
```xaml
<Grid Margin="20" MaxWidth="1400">
    <!-- existing content -->
</Grid>
```

### Fix #4: Table Tab Width
```xaml
<TabControl HorizontalAlignment="Stretch"
            VerticalAlignment="Stretch"
            ItemsSource="{Binding TabHeaders}"
            SelectedIndex="{Binding SelectedTabIndex}">
    <!-- existing template -->
</TabControl>
```

### Fix #5: Property Path Display
```csharp
public string PropertyPath
{
    get
    {
        if (Column.PropertyPaths == null || Column.PropertyPaths.Length == 0)
            return string.Empty;

        // Use string.Join to avoid leading comma
        return string.Join(", ", Column.PropertyPaths);
    }
}
```

### Fix #7: Dirty Check on Close (CRITICAL)
```csharp
// In MainWindowViewModel constructor
public MainWindowViewModel(...)
{
    // Subscribe to window closing event
    Application.Current.MainWindow.Closing += OnWindowClosing;
}

private void OnWindowClosing(object? sender, CancelEventArgs e)
{
    // Check for dirty documents
    var dirtyDocs = _documentManager.Documents.Where(d => d.IsDirty).ToList();
    
    if (dirtyDocs.Any())
    {
        var result = _dialogService.ShowYesNoCancel(
            "Unsaved Changes",
            $"You have {dirtyDocs.Count} document(s) with unsaved changes. Save before closing?",
            "Save and close all documents?");
        
        if (result == null) // Cancel
        {
            e.Cancel = true;
            return;
        }
        
        if (result == true) // Yes - save all
        {
            foreach (var doc in dirtyDocs)
            {
                if (string.IsNullOrEmpty(doc.FilePath))
                {
                    // Need path - show save dialog
                    var saveResult = _dialogService.ShowSaveFileDialog(
                        doc.OwnerModule.SupportedFileExtensions.First(),
                        "Save Document");
                    
                    if (saveResult == null)
                    {
                        e.Cancel = true;
                        return;
                    }
                    
                    doc.OwnerModule.SaveDocument(doc, saveResult);
                }
                else
                {
                    doc.OwnerModule.SaveDocument(doc, doc.FilePath);
                }
            }
        }
        // If No, just close without saving
    }
}
```

---

## Priority Order

1. **CRITICAL**: Fix #7 (Dirty check on close) - prevents data loss
2. **HIGH**: Fix #2 (Tab name update) - confusing UX
3. **MEDIUM**: Fix #5 (Property path display) - visual bug
4. **LOW**: Fix #3 (Setup tab width) - minor UX
5. **LOW**: Fix #4 (Table tab width) - minor UX
6. **DEFERRED**: Fix #6 (100% table width) - future enhancement

---

## Testing Checklist After Fixes

- [ ] Rename table in Setup tab ? Tab header updates immediately
- [ ] Setup tab has reasonable max width (not too wide)
- [ ] Table tabs expand to fill horizontal space
- [ ] Property paths display correctly (no leading comma)
- [ ] App prompts to save on close if dirty documents exist
- [ ] Can cancel close operation
- [ ] Can save and close
- [ ] Can discard changes and close

---

**Implementation Status**: PENDING  
**Estimated Time**: 30-45 minutes  
**Risk Level**: Low (except #7 which is critical)

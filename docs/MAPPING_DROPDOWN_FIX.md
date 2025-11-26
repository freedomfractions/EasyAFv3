# Mapping Dropdown Fix - Implementation Plan

## Issues to Fix

### 1. **Binding Errors** ?
The ComboBox at line 182-190 in `ProjectSummaryView.xaml` is causing binding errors:
```
System.Windows.Data Error: 4 : Cannot find source for binding with reference 'RelativeSource FindAncestor, AncestorType='System.Windows.Controls.ItemsControl'
```

**Root Cause:** ComboBoxItems with inline `Tag` bindings inherit from a base style that tries to find ItemsControl ancestors.

**Fix:** Remove the implicit style inheritance (same issue as Composite Import Dialog).

---

### 2. **Smart Mapping File Management** ?
Current behavior keeps ALL browsed mapping files in history. We need:
- ? Scan `%AppData%/EasyAF/default_import_maps/` folder  
- ? Add all `.ezmap` files from that folder
- ? Add ONLY the last custom browsed file (not all history)
- ? Always show "Browse..." option

---

## Solution

### Part 1: Fix Binding Errors in XAML

**File:** `modules/EasyAF.Modules.Project/Views/ProjectSummaryView.xaml`

**Lines 182-195:**

```xaml
<!-- BEFORE (causes errors) -->
<ComboBox Grid.Row="10" Grid.Column="1" 
          ItemsSource="{Binding AvailableMappings}"
          SelectedItem="{Binding SelectedMapping}"
          VerticalAlignment="Center" 
          Margin="0,0,20,0" 
          HorizontalAlignment="Stretch">
    <ComboBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding DisplayName}"/>
        </DataTemplate>
    </ComboBox.ItemTemplate>
</ComboBox>

<!-- AFTER (no errors) - Add explicit theme bindings -->
<ComboBox Grid.Row="10" Grid.Column="1" 
          ItemsSource="{Binding AvailableMappings}"
          SelectedItem="{Binding SelectedMapping}"
          VerticalAlignment="Center" 
          Margin="0,0,20,0" 
          HorizontalAlignment="Stretch"
          Background="{DynamicResource ControlBackgroundBrush}"
          Foreground="{DynamicResource TextPrimaryBrush}"
          BorderBrush="{DynamicResource ControlBorderBrush}">
    <ComboBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding DisplayName}"/>
        </DataTemplate>
    </ComboBox.ItemTemplate>
</ComboBox>
```

---

### Part 2: Smart Mapping File Management

**File:** `modules/EasyAF.Modules.Project/ViewModels/ProjectSummaryViewModel.cs`

**Current Implementation (Broken):**
- `AvailableMappings` ObservableCollection
- `RefreshAvailableMappings()` method
- `BrowseMappingFileCommand` handler

**New Implementation:**

```csharp
// Constants
private const string DEFAULT_MAPS_FOLDER = "default_import_maps";
private string? _lastCustomMappingPath;  // Track ONLY the last browsed file

private void RefreshAvailableMappings()
{
    AvailableMappings.Clear();
    
    // 1. Scan default folder for .ezmap files
    var defaultFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EasyAF",
        DEFAULT_MAPS_FOLDER);
    
    if (Directory.Exists(defaultFolder))
    {
        var ezMapFiles = Directory.GetFiles(defaultFolder, "*.ezmap");
        foreach (var file in ezMapFiles.OrderBy(f => Path.GetFileName(f)))
        {
            AvailableMappings.Add(new MappingFileItem
            {
                DisplayName = Path.GetFileNameWithoutExtension(file),
                FilePath = file,
                Source = "Default"  // New property to track source
            });
        }
        
        Log.Debug("Loaded {Count} default mapping files", ezMapFiles.Length);
    }
    
    // 2. Add the LAST custom file (if exists and not already in list)
    if (!string.IsNullOrEmpty(_lastCustomMappingPath) && 
        File.Exists(_lastCustomMappingPath) &&
        !AvailableMappings.Any(m => 
            string.Equals(m.FilePath, _lastCustomMappingPath, StringComparison.OrdinalIgnoreCase)))
    {
        AvailableMappings.Add(new MappingFileItem
        {
            DisplayName = $"{Path.GetFileNameWithoutExtension(_lastCustomMappingPath)} (Custom)",
            FilePath = _lastCustomMappingPath,
            Source = "Custom"
        });
        
        Log.Debug("Added last custom mapping file: {Path}", _lastCustomMappingPath);
    }
    
    // 3. Always add "Browse..." option at the end
    AvailableMappings.Add(new MappingFileItem
    {
        DisplayName = "Browse...",
        FilePath = null,  // null indicates browse action
        Source = "Action"
    });
}

private void ExecuteBrowseMappingFile()
{
    var dialog = new OpenFileDialog
    {
        Filter = "EasyAF Mapping Files (*.ezmap)|*.ezmap|All Files (*.*)|*.*",
        Title = "Select Import Mapping File",
        CheckFileExists = true
    };

    if (dialog.ShowDialog() == true)
    {
        // Update ONLY the last custom file
        _lastCustomMappingPath = dialog.FileName;
        
        // Refresh list (will replace old custom file with new one)
        RefreshAvailableMappings();
        
        // Select the newly added custom file
        var newItem = AvailableMappings.FirstOrDefault(m => 
            m.Source == "Custom" && 
            string.Equals(m.FilePath, _lastCustomMappingPath, StringComparison.OrdinalIgnoreCase));
        
        if (newItem != null)
        {
            SelectedMapping = newItem;
            Log.Information("Selected custom mapping file: {Path}", _lastCustomMappingPath);
        }
    }
}
```

**Update MappingFileItem Model:**
```csharp
public class MappingFileItem : BindableBase
{
    private string _displayName = string.Empty;
    private string? _filePath;
    private string _source = "Default";  // NEW: "Default", "Custom", or "Action"

    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public string? FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    public string Source  // NEW
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }
}
```

---

## User Experience

### Before:
1. User browses for mapping file ? gets added to permanent list
2. User browses again ? **another entry** added to list
3. List grows forever with old custom files ?

### After:
1. Default folder scanned ? shows all `.ezmap` files from `%AppData%/EasyAF/default_import_maps/`
2. User browses for custom file ? **replaces** previous custom entry
3. User browses again ? **replaces** previous custom entry
4. List always clean: Default files + Last custom file + Browse... ?

---

## Testing Checklist

- [ ] No binding errors when dropdown opens
- [ ] Default folder files loaded on initialization
- [ ] Browse button works
- [ ] Selecting custom file updates selection
- [ ] Browsing again **replaces** (not adds) custom file
- [ ] Theme bindings work (Light/Dark mode)
- [ ] Selected mapping path displays correctly

---

## Files to Modify

1. ? `modules/EasyAF.Modules.Project/Views/ProjectSummaryView.xaml` (lines 182-195)
2. ? `modules/EasyAF.Modules.Project/ViewModels/ProjectSummaryViewModel.cs`
3. ? `modules/EasyAF.Modules.Project/Models/MappingFileItem.cs`

---

**Estimated Time:** 30 minutes  
**Risk Level:** LOW  
**Impact:** HIGH (fixes binding errors + improves UX)

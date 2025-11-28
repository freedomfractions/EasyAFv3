# Spec Setup Tab - Comprehensive Audit & Fixes

**Date**: 2025-11-28  
**Status**: ? **FIXES APPLIED** - Build Successful

---

## ?? **AUDIT FINDINGS**

### 1. **Table Grid Theming** - FIXED ?
**Problem**:
- Missing `RowBackground` property
- Missing `AlternatingRowBackground` for zebra striping
- Inconsistent with Map/Project modules

**Fix Applied**:
```xaml
RowBackground="{DynamicResource ControlBackgroundBrush}"
AlternatingRowBackground="{DynamicResource PrimaryBackgroundBrush}"
```

**Result**: Table grid now has proper alternating row colors matching other modules.

---

### 2. **Table Name Column - Not Editable** - FIXED ?
**Problem**:
- `TableDefinitionViewModel.TableName` was **read-only**
- Users couldn't rename tables inline

**Fix Applied** (`SpecSetupViewModel.cs`):
```csharp
public string TableName
{
    get => !string.IsNullOrEmpty(Table.AltText) ? Table.AltText : Table.Id;
    set
    {
        if (Table.AltText != value)
        {
            Table.AltText = value;
            _document.MarkDirty();
            RaisePropertyChanged();
        }
    }
}
```

**Result**: Table names are now editable with `UpdateSourceTrigger=PropertyChanged`.

---

### 3. **Mode Column - Empty Placeholder** - FIXED ?
**Problem**:
- `DataGridComboBoxColumn` had **no ItemsSource, no binding**
- Just showed "TODO: Bind to property" comment
- Column was completely non-functional

**Fix Applied** (`SpecSetupViewModel.cs` + `SpecSetupView.xaml`):
1. Added `Mode` property to `TableDefinitionViewModel`:
   ```csharp
   public string Mode
   {
       get => Table.Mode ?? "new";
       set
       {
           if (Table.Mode != value)
           {
               Table.Mode = value;
               _document.MarkDirty();
               RaisePropertyChanged();
           }
       }
   }
   ```

2. Added proper XAML binding:
   ```xaml
   <DataGridComboBoxColumn Header="Mode" 
                           Width="100"
                           SelectedItemBinding="{Binding Mode, UpdateSourceTrigger=PropertyChanged}">
       <DataGridComboBoxColumn.ElementStyle>
           <Style TargetType="ComboBox">
               <Setter Property="ItemsSource">
                   <Setter.Value>
                       <x:Array Type="sys:String" xmlns:sys="clr-namespace:System;assembly=mscorlib">
                           <sys:String>new</sys:String>
                           <sys:String>diff</sys:String>
                       </x:Array>
                   </Setter.Value>
               </Setter>
           </Style>
       </DataGridComboBoxColumn.ElementStyle>
   </DataGridComboBoxColumn>
   ```

**Result**: Mode column now functional with "new"/"diff" options.

---

### 4. **Map File Dropdown - Not Populating** - FIXED ?
**Problem**:
- `AvailableMapFiles` collection was **never populated**
- Dropdown was always empty
- Missing `LoadAvailableMapFiles()` method

**Fix Applied** (`SpecSetupViewModel.cs`):
```csharp
/// <summary>
/// AUDIT FIX: Load available map files from Documents\EasyAF\Maps directory
/// </summary>
private void LoadAvailableMapFiles()
{
    AvailableMapFiles.Clear();

    try
    {
        var mapsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "EasyAF",
            "Maps");

        if (Directory.Exists(mapsFolder))
        {
            var mapFiles = Directory.GetFiles(mapsFolder, "*.ezmap")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(name => name)
                .ToList();

            foreach (var mapFile in mapFiles)
            {
                AvailableMapFiles.Add(mapFile);
            }

            Log.Debug("Loaded {Count} map files from {Folder}", mapFiles.Count, mapsFolder);
        }
        else
        {
            Log.Warning("Maps folder does not exist: {Folder}", mapsFolder);
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to load available map files");
    }
}
```

Called in constructor:
```csharp
LoadAvailableMapFiles(); // FIX: Auto-populate map files dropdown
```

**Result**: Map file dropdown now auto-populates from `Documents\EasyAF\Maps\*.ezmap`.

---

### 5. **Data Types Column - Multi-Select Picker** - DEFERRED ??
**Problem**:
- Shows "(not configured)" as read-only text
- No way to actually SELECT data types for a table
- Should have interactive multi-select dropdown with checkboxes

**Why Deferred**:
This requires a custom `DataTypePicker` control similar to Project module's implementation. This is a **complex feature** requiring:
1. Custom UserControl with CheckedListBox
2. Fuzzy search capability
3. Property binding infrastructure
4. Additional 200-300 lines of code

**Recommendation**: Implement in **Task 27 or future enhancement** when ribbon commands are added.

**Current Status**: Functional but limited - users can still edit tables and columns without data type association.

---

## ?? **SUMMARY OF CHANGES**

| Issue | Status | Files Modified |
|-------|--------|----------------|
| Table Grid Theming | ? FIXED | `SpecSetupView.xaml` |
| Table Name Editing | ? FIXED | `SpecSetupViewModel.cs`, `SpecSetupView.xaml` |
| Mode Column Binding | ? FIXED | `SpecSetupViewModel.cs`, `SpecSetupView.xaml` |
| Map File Dropdown | ? FIXED | `SpecSetupViewModel.cs` |
| Data Types Picker | ?? DEFERRED | (Future enhancement) |

---

## ? **TESTING CHECKLIST**

Manual testing required:
- [ ] Table grid shows alternating row colors
- [ ] Can edit table names inline
- [ ] Mode dropdown shows "new" and "diff" options
- [ ] Map file dropdown populates from Documents\EasyAF\Maps
- [ ] Changing values marks document as dirty
- [ ] All changes persist on save/reload

---

## ?? **ROLLBACK INSTRUCTIONS**

If issues arise, revert these files:
1. `modules/EasyAF.Modules.Spec/ViewModels/SpecSetupViewModel.cs`
   - Remove `LoadAvailableMapFiles()` method
   - Revert `TableDefinitionViewModel.TableName` setter to read-only
   - Remove `Mode` property

2. `modules/EasyAF.Modules.Spec/Views/SpecSetupView.xaml`
   - Remove `RowBackground` and `AlternatingRowBackground` properties
   - Restore empty Mode column placeholder

---

## ?? **NOTES**

- All fixes follow established patterns from Map/Project modules
- Theme compliance maintained (100% DynamicResource bindings)
- MVVM pattern strictly followed (zero code-behind logic)
- Build successful with 0 errors, 0 warnings

**Next Steps**: Test in running application, then proceed with Task 27 (Ribbon Commands).

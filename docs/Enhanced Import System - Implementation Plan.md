# Enhanced Import System - Implementation Plan

**Date**: 2025-01-21  
**Branch**: phase-4-project-module  
**Status**: In Progress

---

## Overview

Comprehensive enhancement to the Project module's import system with:
- Drag-and-drop import zones
- Visual feedback (glow effects, color coding)
- Composite model scenario management  
- Import session tracking
- Default mapping configuration

---

## Implementation Phases

### ? Phase 1: Foundation & Theming (COMPLETE)

**Commit**: `3c43332` - feat: Add New/Old data color scheme for import zones

**Added**:
- `NewDataForegroundColor/Brush` - Red (#FFDC2626 light, #FFEF4444 dark)
- `NewDataBackgroundColor/Brush` - Light red/pink backgrounds
- `NewDataBorderColor/Brush` - Light red borders
- `NewDataGlowColor/Brush` - Bright red for drag-drop glow

- `OldDataForegroundColor/Brush` - Green (#FF059669 light, #FF22C55E dark)
- `OldDataBackgroundColor/Brush` - Light green backgrounds
- `OldDataBorderColor/Brush` - Light green borders
- `OldDataGlowColor/Brush` - Bright green for drag-drop glow

**Files Modified**:
- `app/EasyAF.Shell/Theme/Light.xaml`
- `app/EasyAF.Shell/Theme/Dark.xaml`

---

### ?? Phase 2: Drag-Drop Infrastructure (IN PROGRESS)

**Goals**:
1. Enable drag-drop on New Data/Old Data column background borders
2. Glow effect on drag-over (using theme colors)
3. File type validation (CSV, Excel only)
4. Trigger import on drop

**Files to Create/Modify**:
- `modules/EasyAF.Modules.Project/Behaviors/ImportDropBehavior.cs` - NEW
- `modules/EasyAF.Modules.Project/Views/ProjectSummaryView.xaml` - MODIFY
- `modules/EasyAF.Modules.Project/ViewModels/ProjectSummaryViewModel.cs` - MODIFY

**Implementation Details**:
```csharp
// Attached behavior for drag-drop
public class ImportDropBehavior : Behavior<Border>
{
    public static readonly DependencyProperty IsNewDataProperty = ...;
    public static readonly DependencyProperty ViewModelProperty = ...;
    
    protected override void OnAttached()
    {
        AssociatedObject.AllowDrop = true;
        AssociatedObject.DragEnter += OnDragEnter;
        AssociatedObject.DragLeave += OnDragLeave;
        AssociatedObject.Drop += OnDrop;
    }
    
    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (CanImportFile(e.Data))
        {
            // Show glow using NewDataGlowBrush or OldDataGlowBrush
            var border = (Border)sender;
            border.BorderBrush = IsNewData 
                ? Application.Current.FindResource("NewDataGlowBrush") 
                : Application.Current.FindResource("OldDataGlowBrush");
            border.BorderThickness = new Thickness(2);
            e.Effects = DragDropEffects.Copy;
        }
    }
}
```

---

### ?? Phase 3: Standard Model Import Flow

**Goals**:
1. Detect if data already exists in target dataset
2. Show overwrite warning dialog
3. Enhanced import result dialog

**Files to Create**:
- `modules/EasyAF.Modules.Project/Views/OverwriteWarningDialog.xaml` - NEW
- `modules/EasyAF.Modules.Project/ViewModels/OverwriteWarningViewModel.cs` - NEW
- `modules/EasyAF.Modules.Project/Models/ImportSession.cs` - NEW

**Import Session Tracking**:
```csharp
public class ImportSession
{
    public DateTime ImportedAt { get; set; }
    public string SourceFile { get; set; }
    public Dictionary<string, int> ImportedCounts { get; set; } // DataType -> Count
    public List<string> Scenarios { get; set; } // For composite models
    public bool IsNewData { get; set; }
}

// Add to DataSet
public class DataSet
{
    public List<ImportSession> ImportHistory { get; set; } = new();
}
```

**Overwrite Warning Dialog**:
```
?? Overwrite Existing Data?

The dataset already contains data:
  • 45 Arc Flash entries
  • 45 Short Circuit entries
  • 12 Buses
  
Importing will REPLACE all existing data.

This cannot be undone.

[Cancel]  [Overwrite]
```

---

### ?? Phase 4: Composite Model Import Dialog

**Goals**:
1. Scenario discovery from file
2. Three-column dialog layout:
   - Column 1: Scenarios in file
   - Column 2: Import action (radio buttons)
   - Column 3: Context inputs (rename/replace target)
3. Confirmation guard for replacements

**Files to Create**:
- `modules/EasyAF.Modules.Project/Views/CompositeImportDialog.xaml` - NEW
- `modules/EasyAF.Modules.Project/ViewModels/CompositeImportViewModel.cs` - NEW
- `modules/EasyAF.Modules.Project/ViewModels/ScenarioImportOptionViewModel.cs` - NEW

**Dialog Layout**:
```
???????????????????????????????????????????????????????????????
? Import Scenarios - composite_model.csv                      ?
???????????????????????????????????????????????????????????????
? Scenario        ? Import Action      ? Options              ?
???????????????????????????????????????????????????????????????
? Main-Min        ? ? Do not import   ?                      ?
?                 ? ? Import as new    ? [Enter scenario...]  ?
?                 ? ? Replace existing ? [Select target ?]    ?
???????????????????????????????????????????????????????????????
? Main-Max        ? ? Do not import    ?                      ?
?                 ? ? Import as new    ? [MainMax_2025___]    ?
?                 ? ? Replace existing ? [Select target ?]    ?
???????????????????????????????????????????????????????????????
? Service-Min     ? ? Do not import    ?                      ?
?                 ? ? Import as new    ? [Enter scenario...]  ?
?                 ? ? Replace existing ? [Service-Min    ?]   ?
???????????????????????????????????????????????????????????????
                                              [Cancel]  [OK]
```

**ViewModel**:
```csharp
public class ScenarioImportOptionViewModel : BindableBase
{
    public string ScenarioName { get; set; }
    public ScenarioImportAction Action { get; set; } // DoNotImport, ImportAsNew, ReplaceExisting
    public string? NewScenarioName { get; set; } // For ImportAsNew
    public string? TargetScenario { get; set; } // For ReplaceExisting
    public ObservableCollection<string> AvailableScenarios { get; set; } // For replace dropdown
}

public enum ScenarioImportAction
{
    DoNotImport,
    ImportAsNew,
    ReplaceExisting
}
```

---

### ?? Phase 5: Visual Feedback Enhancements

**Goals**:
1. Glow effect on statistics table cells after import
2. Fade-in ? hold 3s ? fade-out animation
3. Enhanced logging (separate log level for import history)

**Glow Animation**:
```xaml
<!-- In ProjectSummaryView.xaml statistics cells -->
<Border x:Name="CellGlowBorder" 
        Background="Transparent"
        BorderThickness="2"
        CornerRadius="4"
        Opacity="0">
    <Border.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard>
                <Storyboard x:Name="ImportGlowAnimation">
                    <!-- Fade in (0.3s) -->
                    <DoubleAnimation Storyboard.TargetName="CellGlowBorder"
                                     Storyboard.TargetProperty="Opacity"
                                     From="0" To="1" Duration="0:0:0.3"/>
                    <!-- Hold (3s) -->
                    <DoubleAnimation Storyboard.TargetName="CellGlowBorder"
                                     Storyboard.TargetProperty="Opacity"
                                     From="1" To="1" Duration="0:0:3" BeginTime="0:0:0.3"/>
                    <!-- Fade out (0.5s) -->
                    <DoubleAnimation Storyboard.TargetName="CellGlowBorder"
                                     Storyboard.TargetProperty="Opacity"
                                     From="1" To="0" Duration="0:0:0.5" BeginTime="0:0:3.3"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Border.Triggers>
    <Border.Style>
        <Style TargetType="Border">
            <Setter Property="BorderBrush" Value="{DynamicResource NewDataGlowBrush}"/> <!-- Or OldDataGlowBrush -->
        </Style>
    </Border.Style>
</Border>
```

**Logging Enhancement**:
```csharp
// Add custom log level: ImportHistory
// Separate from Verbose/Debug to allow filtering
Log.ForContext("ImportSession", sessionId)
   .Information("Import completed: {File} -> {DataType}={Count}", 
                fileName, dataType, importedCount);
```

---

### ?? Phase 6: Default Mapping Configuration

**Goals**:
1. Add mapping dropdown to Project Summary (below Project Type)
2. Populate from `~/EasyAF/ImportMaps/` folder
3. Save selection in Project
4. Use as default for import operations

**UI Layout** (ProjectSummaryView.xaml, Row 10):
```
??????????????????????????????????????????????????????????
? Project Type:  ? Standard  ? Composite                 ?
?                                                        ?
? Default Map:   [ArcFlash_v2.ezmap        ?]  [Browse] ?
??????????????????????????????????????????????????????????
```

**Files to Modify**:
- `modules/EasyAF.Modules.Project/Views/ProjectSummaryView.xaml` - Row 10 addition
- `modules/EasyAF.Modules.Project/ViewModels/ProjectSummaryViewModel.cs` - Add properties
- `lib/EasyAF.Data/Models/Project.cs` - Add `DefaultMappingPath` property

**ViewModel Additions**:
```csharp
public ObservableCollection<string> AvailableMappings { get; } = new();
public string? DefaultMappingPath { get; set; }

private void LoadAvailableMappings()
{
    var mapsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EasyAF", "ImportMaps");
    
    if (!Directory.Exists(mapsFolder))
        Directory.CreateDirectory(mapsFolder);
    
    AvailableMappings.Clear();
    foreach (var file in Directory.GetFiles(mapsFolder, "*.ezmap"))
    {
        AvailableMappings.Add(Path.GetFileName(file));
    }
}
```

---

## Future TODOs

### ?? Advanced Import Dialog
**Priority**: Medium  
**Effort**: 2-3 days

Allow per-file mapping selection for batch imports:
```
???????????????????????????????????????????????????????????
? Advanced Import                                         ?
???????????????????????????????????????????????????????????
? File             ? Mapping          ? Target Dataset    ?
???????????????????????????????????????????????????????????
? MainMax.csv      ? [ArcFlash ?]     ? ? New  ? Old      ?
? ServiceMin.xlsx  ? [ArcFlash ?]     ? ? New  ? Old      ?
? Bus_Data.csv     ? [Equipment ?]    ? ? New  ? Old      ?
???????????????????????????????????????????????????????????
                                        [Cancel]  [Import]
```

### ?? Default Mapping User Configuration
**Priority**: Low  
**Effort**: 1 day

Add UI in Options dialog for setting global default mapping:
- Settings > Import > Default Mapping: `[Select .ezmap file...]`

### ?? Undo/Redo Support
**Priority**: High (post-MVP)  
**Effort**: 1-2 weeks

Implement command pattern for import operations:
```csharp
public class ImportCommand : IUndoableCommand
{
    private DataSet _beforeSnapshot;
    private DataSet _afterSnapshot;
    private ImportSession _session;
    
    public void Execute() { /* import */ }
    public void Undo() { /* restore _beforeSnapshot */ }
    public void Redo() { /* restore _afterSnapshot */ }
}
```

**Challenges**:
- Deep cloning DataSet (large memory overhead)
- File I/O (need to cache imported files?)
- User expectations (undo after save?)

---

## Testing Strategy

### Unit Tests
- [ ] Scenario discovery from CSV/Excel
- [ ] Import action validation logic
- [ ] Overwrite detection
- [ ] Session tracking

### Integration Tests
- [ ] Drag-drop file import (New Data)
- [ ] Drag-drop file import (Old Data)
- [ ] Standard model overwrite warning
- [ ] Composite model scenario stitching
- [ ] Default mapping selection persistence

### Manual Test Cases
1. **Standard Import Flow**
   - Import into empty dataset
   - Import with overwrite warning
   - Verify statistics table updates

2. **Composite Import Flow**
   - Discover scenarios from file
   - Import as new with rename
   - Replace existing scenario
   - Verify multi-scenario display

3. **Drag-Drop**
   - Drag valid file (CSV/Excel) over New Data
   - Verify glow effect
   - Drop triggers import
   - Drag invalid file (PDF) shows no drop cursor

4. **Visual Feedback**
   - Glow animation plays on import
   - Fades out after 3 seconds
   - Correct color (red for New, green for Old)

---

## Dependencies

### NuGet Packages
- **System.Windows.Interactivity** (for Behaviors) - Already included
- **ClosedXML** (Excel parsing) - Already included

### Architecture Requirements
- Theme system supports runtime brush lookup ?
- DataSet supports composite keys ?
- ImportManager supports multi-file imports ?

---

## Rollback Plan

Each phase is independently committable. Rollback by:
1. Revert specific commit: `git revert <commit-hash>`
2. Remove feature flag if implemented
3. Hide UI elements via Visibility bindings

**Safe Rollback Points**:
- ? Phase 1 (Theming): Safe - just adds colors
- Phase 2 (Drag-drop): Safe - behavior is opt-in
- Phase 3 (Standard flow): Safe - fallback to old dialog
- Phase 4 (Composite dialog): Safe - only for Composite projects
- Phase 5 (Glow): Safe - animation enhancement only
- Phase 6 (Default mapping): Safe - uses existing path if not set

---

## Timeline Estimate

| Phase | Effort | Status |
|-------|--------|--------|
| Phase 1: Theming | 1 hour | ? DONE |
| Phase 2: Drag-Drop | 3-4 hours | ?? IN PROGRESS |
| Phase 3: Standard Flow | 2-3 hours | ? PENDING |
| Phase 4: Composite Dialog | 4-5 hours | ? PENDING |
| Phase 5: Visual Feedback | 2 hours | ? PENDING |
| Phase 6: Default Mapping | 2 hours | ? PENDING |
| **Total** | **14-17 hours** | **~6% complete** |

---

## Notes

- **Color Accessibility**: Tested with Color Oracle (colorblind simulator)
- **Performance**: Glow animations use GPU-accelerated composition
- **Logging**: Consider separate log file for import history (`import-history.log`)


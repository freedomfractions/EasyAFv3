# Import History Tree UI - Implementation Roadmap

**Feature**: Display import history in Project Summary tab showing files, data types, and scenario mappings

**Status**: Foundation Complete ? | UI Implementation Pending ?

---

## ? Completed (Current Commit)

1. **Data Model**
   - Created `ImportFileRecord.cs` with all required metadata
   - Added `ImportHistory` property to `Project` class
   - Updated `ProjectPersist` for serialization/deserialization

---

## ? Remaining Work

### Step 1: Record Import Operations

**File**: `modules/EasyAF.Modules.Project/ViewModels/ProjectSummaryViewModel.cs`

Update these methods to record import history:
- `ExecuteImport` (Standard mode)
- `ExecuteCompositeImport` (Composite mode)
- `ExecuteDropImport` (Drag-drop)

**Implementation**:
```csharp
private void RecordImportHistory(string[] filePaths, bool isNewData, EasyAF.Import.MappingConfig mappingConfig)
{
    _document.Project.ImportHistory ??= new List<ImportFileRecord>();
    
    foreach (var filePath in filePaths)
    {
        // Scan file to determine data types (same logic as PreScanFilesForScenarios)
        var tempDataSet = new DataSet();
        var importManager = new EasyAF.Import.ImportManager();
        importManager.Import(filePath, mappingConfig, tempDataSet);
        
        // Detect data types
        var dataTypes = new List<string>();
        if ((tempDataSet.ArcFlashEntries?.Count ?? 0) > 0) dataTypes.Add("Arc Flash");
        if ((tempDataSet.ShortCircuitEntries?.Count ?? 0) > 0) dataTypes.Add("Short Circuit");
        if ((tempDataSet.BusEntries?.Count ?? 0) > 0) dataTypes.Add("Bus");
        if ((tempDataSet.LVBreakerEntries?.Count ?? 0) > 0) dataTypes.Add("LV Breaker");
        if ((tempDataSet.FuseEntries?.Count ?? 0) > 0) dataTypes.Add("Fuse");
        // ... add other types as needed
        
        // Get scenario mappings (if any)
        var scenarios = tempDataSet.GetAvailableScenarios();
        var scenarioMappings = new Dictionary<string, string>();
        foreach (var scenario in scenarios)
        {
            scenarioMappings[scenario] = scenario; // Default: no rename
        }
        
        // Calculate entry count
        int entryCount = 0;
        if (tempDataSet.ArcFlashEntries != null) entryCount += tempDataSet.ArcFlashEntries.Count;
        if (tempDataSet.ShortCircuitEntries != null) entryCount += tempDataSet.ShortCircuitEntries.Count;
        // ... add other types
        
        var record = new ImportFileRecord(filePath, isNewData, dataTypes, scenarioMappings, entryCount);
        _document.Project.ImportHistory.Add(record);
    }
    
    _document.MarkDirty();
}
```

**For Composite Import**: Update scenario mappings with actual rename info from `ScenarioImportPlan`.

---

### Step 2: Create ViewModel for Tree Display

**New File**: `modules/EasyAF.Modules.Project/ViewModels/ImportHistoryNodeViewModel.cs`

```csharp
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// View model for a node in the import history tree.
    /// </summary>
    public class ImportHistoryNodeViewModel : BindableBase
    {
        /// <summary>Display text for this node.</summary>
        public string DisplayText { get; set; } = string.Empty;
        
        /// <summary>Icon glyph (Segoe MDL2 Assets).</summary>
        public string IconGlyph { get; set; } = "\uE8A5"; // Document
        
        /// <summary>Child nodes.</summary>
        public ObservableCollection<ImportHistoryNodeViewModel> Children { get; } = new();
        
        /// <summary>Indentation level (0 = file, 1 = data type, 2 = scenario).</summary>
        public int Level { get; set; }
        
        /// <summary>Whether this node is a scenario mapping (shows arrow).</summary>
        public bool IsScenarioMapping { get; set; }
        
        /// <summary>Original scenario name (if this is a mapping).</summary>
        public string? OriginalScenario { get; set; }
        
        /// <summary>Target scenario name (if renamed).</summary>
        public string? TargetScenario { get; set; }
    }
}
```

---

### Step 3: Add Tree Building Logic to ProjectSummaryViewModel

**Method to add**:
```csharp
private ObservableCollection<ImportHistoryNodeViewModel> BuildImportHistoryTree()
{
    var nodes = new ObservableCollection<ImportHistoryNodeViewModel>();
    
    if (_document.Project.ImportHistory == null || _document.Project.ImportHistory.Count == 0)
        return nodes;
    
    foreach (var record in _document.Project.ImportHistory.OrderByDescending(r => r.ImportedAt))
    {
        // File node
        var fileNode = new ImportHistoryNodeViewModel
        {
            DisplayText = $"{record.FileName} - {record.FilePath}",
            IconGlyph = "\uE8A5", // Document
            Level = 0
        };
        
        // Data type children
        foreach (var dataType in record.DataTypes)
        {
            var dataTypeNode = new ImportHistoryNodeViewModel
            {
                DisplayText = dataType,
                IconGlyph = "\uE8F1", // Database
                Level = 1
            };
            
            // Scenario mappings (if any)
            if (record.ScenarioMappings.Count > 0)
            {
                foreach (var mapping in record.ScenarioMappings)
                {
                    var scenarioNode = new ImportHistoryNodeViewModel
                    {
                        DisplayText = mapping.Key == mapping.Value 
                            ? mapping.Key 
                            : $"{mapping.Key} ? {mapping.Value}",
                        IconGlyph = "\uE8F4", // Tag
                        Level = 2,
                        IsScenarioMapping = true,
                        OriginalScenario = mapping.Key,
                        TargetScenario = mapping.Value
                    };
                    dataTypeNode.Children.Add(scenarioNode);
                }
            }
            
            fileNode.Children.Add(dataTypeNode);
        }
        
        nodes.Add(fileNode);
    }
    
    return nodes;
}
```

**Property to add**:
```csharp
public ObservableCollection<ImportHistoryNodeViewModel> ImportHistoryNodes { get; } = new();
```

**Call in RefreshStatistics()**:
```csharp
// Rebuild import history tree
ImportHistoryNodes.Clear();
foreach (var node in BuildImportHistoryTree())
{
    ImportHistoryNodes.Add(node);
}
```

---

### Step 4: Create UI Panel

**New File**: `modules/EasyAF.Modules.Project/Views/Panels/ImportHistoryPanel.xaml`

```xaml
<UserControl x:Class="EasyAF.Modules.Project.Views.Panels.ImportHistoryPanel"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <GroupBox Header="Import History" 
              Margin="0,0,22,16"
              HorizontalAlignment="Stretch"
              BorderBrush="{DynamicResource ControlBorderBrush}"
              Foreground="{DynamicResource TextPrimaryBrush}">
        
        <TreeView ItemsSource="{Binding ImportHistoryNodes}"
                  BorderThickness="0"
                  Background="Transparent">
            
            <TreeView.ItemContainerStyle>
                <Style TargetType="TreeViewItem">
                    <Setter Property="IsExpanded" Value="False"/>
                    <Setter Property="Foreground" Value="{DynamicResource TextPrimaryBrush}"/>
                </Style>
            </TreeView.ItemContainerStyle>
            
            <TreeView.ItemTemplate>
                <HierarchicalDataTemplate ItemsSource="{Binding Children}">
                    <StackPanel Orientation="Horizontal">
                        <!-- Icon -->
                        <TextBlock Text="{Binding IconGlyph}" 
                                   FontFamily="Segoe MDL2 Assets" 
                                   FontSize="14" 
                                   Margin="0,0,8,0" 
                                   VerticalAlignment="Center"
                                   Foreground="{DynamicResource AccentBrush}"/>
                        
                        <!-- Display Text -->
                        <TextBlock Text="{Binding DisplayText}" 
                                   VerticalAlignment="Center"
                                   Foreground="{DynamicResource TextPrimaryBrush}"/>
                    </StackPanel>
                </HierarchicalDataTemplate>
            </TreeView.ItemTemplate>
        </TreeView>
        
    </GroupBox>
</UserControl>
```

**Code-behind**:
```csharp
namespace EasyAF.Modules.Project.Views.Panels
{
    public partial class ImportHistoryPanel : UserControl
    {
        public ImportHistoryPanel()
        {
            InitializeComponent();
        }
    }
}
```

---

### Step 5: Add Panel to ProjectSummaryView

**File**: `modules/EasyAF.Modules.Project/Views/ProjectSummaryView.xaml`

Insert AFTER ProjectDataPanel and BEFORE ReportPanel:

```xaml
<!-- Import History (NEW) -->
<panels:ImportHistoryPanel Grid.Row="4" DataContext="{Binding}"/>
```

Update Grid.RowDefinitions to add a row for this panel.

---

## Testing Checklist

- [ ] Import a file in Standard mode ? verify entry appears in Import History
- [ ] Import multiple files ? verify all appear
- [ ] Import in Composite mode with scenario rename ? verify mapping shows correctly
- [ ] Save and reload project ? verify history persists
- [ ] Clear New Data ? verify history entries remain (or decide if they should be cleared)
- [ ] Expand/collapse tree nodes ? verify proper hierarchy
- [ ] Visual check: Icons, text formatting, indentation

---

## Optional Enhancements (Future)

1. **Right-click context menu** on file nodes:
   - "Remove from history"
   - "Show in Explorer"
   - "Re-import this file"

2. **Color coding**:
   - Green for New Data imports
   - Blue for Old Data imports

3. **Timestamp display**: Show "Imported 2 hours ago" or specific date/time

4. **Entry count badges**: Show count next to data type (e.g., "Arc Flash (14)")

5. **Filter/Search**: Allow user to filter history by file name or data type

---

## Implementation Notes

- Import history is **append-only** - never removed unless user explicitly clears it
- History is **project-specific** - each .ezproj has its own history
- Scenario mappings are stored as key-value pairs for future analysis/debugging
- File paths are stored as full paths for "Show in Explorer" functionality

---

**Next Steps**: Implement Step 1 (record imports), then test before proceeding to UI.

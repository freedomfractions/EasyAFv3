# Project Editor - Import Pipeline & Data Management Sidequest

**Module**: EasyAF.Modules.Project  
**Created**: 2025-01-25  
**Status**: Planning  
**Priority**: HIGH (blocks Task 22 - Build Project Ribbon Interface)

---

## ?? Overview

This sidequest implements a comprehensive **data import and management system** for the Project Editor module, allowing users to:

1. **Import data from CSV/Excel files** into project datasets
2. **Manage scenarios** (both Standard multi-scenario files and Composite single-scenario files)
3. **Selectively replace datatypes** (granular control over what gets updated)
4. **View and manage imported data** via enhanced Project Summary UI
5. **Handle drag-and-drop imports** with intelligent conflict resolution

---

## ??? Architecture Context

### Data Model Foundation

**Already Implemented** ?:
- `DataSet` class with composite keys for scenario-aware data types:
  - `ArcFlashEntries`: `Dictionary<(string Id, string Scenario), ArcFlash>`
  - `ShortCircuitEntries`: `Dictionary<(string Id, string Bus, string Scenario), ShortCircuit>`
- Equipment types (Bus, LVBreaker, Fuse, etc.) have **NO scenarios** (single instance per ID)

**Key Insight**: Files can contain **multiple scenarios** regardless of project type:
- **Standard projects**: Import file with all scenarios at once (Main-Min, Main-Max, Service-Min, Service-Max)
- **Composite projects**: Import separate files, but **each file may still contain multiple scenarios**

### Import Pipeline Requirement

**CRITICAL**: When importing a file (Standard OR Composite), the user must:

1. **See which scenarios are in the file** (discovered during import preview)
2. **Select which scenario(s) to import** (checkboxes for each discovered scenario)
3. **Assign/rename scenario names** (user can override "Main-Min" ? "Scenario 1", etc.)
4. **Choose merge strategy**:
   - **Replace All**: Overwrite existing scenarios with same name
   - **Merge**: Add to existing data (keep existing scenarios, add new ones)
   - **Skip Duplicates**: Only import scenarios that don't exist yet

---

## ?? Phase Breakdown

### **Phase A: Foundation** (Data Model Enhancements)
**Priority**: CRITICAL  
**Estimated Time**: 2-3 hours  
**Status**: Not Started

#### Tasks:

**A1. Create Data Type Metadata System**
- [ ] Create `DataTypeInfo.cs` model
  - Properties: `Name`, `DisplayName`, `HasScenarios`, `PropertyName`, `Icon`, `Category`
  - Example: `{ Name = "ArcFlash", DisplayName = "Arc Flash", HasScenarios = true }`
- [ ] Create `DataTypeRegistry.cs` - Static registry of all known data types
  - Method: `GetAllDataTypes()` ? List of DataTypeInfo
  - Method: `GetDataTypeByName(string name)` ? DataTypeInfo?
  - Method: `GetScenarioAwareDataTypes()` ? List of types with scenarios
  - Method: `GetEquipmentDataTypes()` ? List of equipment types (no scenarios)

**A2. Add Scenario Discovery to DataSet**
- [ ] Add extension method: `DataSet.GetAvailableScenarios()` ? `HashSet<string>`
  - Scans ArcFlashEntries and ShortCircuitEntries for unique Scenario values
- [ ] Add extension method: `DataSet.GetScenarioStatistics(string scenario)` ? `Dictionary<string, int>`
  - Returns counts per datatype for a specific scenario
  - Example: `{ "ArcFlash": 42, "ShortCircuit": 38 }`

**A3. Create Scenario Statistics Model**
- [ ] Create `ScenarioInfo.cs` model
  - Properties: `ScenarioName`, `DataTypeCounts`, `TotalEntries`, `IsUniform`
  - Method: `CompareWith(ScenarioInfo other)` ? `bool` (checks if counts match)
- [ ] Create `DataSetStatistics.cs` model
  - Properties: `Scenarios` (List<ScenarioInfo>), `HasUniformScenarios`, `TotalEntries`
  - Method: `GetConflicts()` ? List of datatypes with mismatched counts across scenarios

**Deliverables**:
- ? Metadata system for describing data types
- ? Scenario discovery and statistics
- ? Foundation for tree view (Phase C)

**Dependencies**: None (can start immediately)

---

### **Phase B: Import Dialog Foundation** (Basic Import)
**Priority**: HIGH  
**Estimated Time**: 3-4 hours  
**Status**: Not Started

#### Tasks:

**B1. Create Scenario Selection Dialog**
- [ ] Create `ScenarioSelectionDialog.xaml` window
  - **Purpose**: Let user choose which scenarios to import from a file
  - **Inputs**: 
    - `List<string> DiscoveredScenarios` (from file audit)
    - `List<string> ExistingScenarios` (from current DataSet)
  - **UI Components**:
    - ListView with checkboxes for each discovered scenario
    - TextBox to rename each scenario (editable in-place)
    - "Select All" / "Deselect All" buttons
    - Warning icon if scenario name conflicts with existing
    - Preview of what will be imported (entry counts per datatype)
  - **Outputs**: 
    - `Dictionary<string, string>` (originalName ? userRenamedName)
    - `List<string>` (selected scenario names)

**B2. Create Import Data Dialog**
- [ ] Create `ImportDataDialog.xaml` window
  - **Step 1**: File Selection
    - Browse button for CSV/Excel file
    - Recent files list
  - **Step 2**: Mapping Selection
    - ComboBox of available mapping configs (from Map Module)
    - "Create New Mapping..." button (launches Map Module)
    - Preview of file structure
  - **Step 3**: Scenario Selection (**NEW**)
    - Button: "Preview Scenarios..." ? Opens `ScenarioSelectionDialog`
    - Display: "Importing X scenarios: Main-Min, Main-Max..."
    - For Composite projects: Default to importing all scenarios
  - **Step 4**: Import Target
    - Radio buttons: "New Data" / "Old Data"
    - Checkbox: "Replace existing data" vs "Merge with existing"
  - **Step 5**: Confirmation
    - Summary: "Will import 42 ArcFlash entries, 38 ShortCircuit entries..."
    - Progress bar during import

**B3. Create ImportManager Service**
- [ ] Create `ImportManager.cs` in EasyAF.Import library
  - Method: `AuditFile(string filePath, IMappingConfig mapping)` ? `ImportAudit`
    - Returns: Discovered scenarios, datatype counts, validation warnings
  - Method: `ImportFile(string filePath, IMappingConfig mapping, ImportOptions options)` ? `DataSet`
    - Imports file into a new DataSet
  - Method: `MergeIntoDataSet(DataSet existing, DataSet imported, MergeOptions options)` ? `DataSet`
    - Handles scenario collision detection
    - Applies user's scenario rename mappings
    - Supports Replace/Merge/Skip strategies

**B4. Wire to Project Ribbon**
- [ ] Add "Import Data..." button to Project ribbon (Home tab)
- [ ] Command: `ImportDataCommand` in `ProjectSummaryViewModel`
  - Opens `ImportDataDialog`
  - On success: Updates Project.NewData or Project.OldData
  - Calls `RefreshStatistics()` to update UI

**Deliverables**:
- ? Multi-step import dialog with scenario selection
- ? Import service with scenario discovery and merging
- ? Ribbon integration

**Dependencies**: Phase A (for scenario discovery)

---

### **Phase C: Enhanced Project Summary UI** (Tree View)
**Priority**: HIGH  
**Estimated Time**: 5-6 hours  
**Status**: Not Started

#### Tasks:

**C1. Create Tree View ViewModels**
- [ ] Create `DataTypeNodeViewModel.cs`
  - Properties: 
    - `string Name` (e.g., "Arc Flash")
    - `string Icon` (Segoe MDL2 glyph)
    - `ObservableCollection<ScenarioNodeViewModel> Scenarios`
    - `int TotalEntries` (sum across all scenarios)
    - `bool HasUniformScenarios` (all scenarios have same count?)
    - `string StatisticsDisplay` (e.g., "42 entries" or "? Mixed")
  - Commands:
    - `ShowContextMenuCommand` ? Opens context menu
  - Methods:
    - `Refresh()` ? Reloads statistics from DataSet

- [ ] Create `ScenarioNodeViewModel.cs`
  - Properties:
    - `string ScenarioName`
    - `string ParentDataType`
    - `int EntryCount`
    - `string StatisticsDisplay`
  - Commands:
    - `DeleteScenarioCommand` (Composite projects only)
    - `ReplaceScenarioCommand`

**C2. Transform Project Summary Statistics Section**
- [ ] **Replace** current "New Data" / "Old Data" grids with **TreeView**
- [ ] XAML Structure:
  ```xaml
  <GroupBox Header="Data Files">
    <Grid>
      <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/> <!-- Header bar -->
        <RowDefinition Height="*"/>    <!-- Tree view -->
      </Grid.RowDefinitions>
      
      <!-- Header Bar -->
      <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,8">
        <TextBlock Text="New Data" FontWeight="SemiBold"/>
        <Button Content="Import..." Command="{Binding ImportDataCommand}" Margin="8,0,0,0"/>
        <Button Content="Reset All..." Command="{Binding ResetAllDataCommand}" Margin="4,0,0,0"/>
      </StackPanel>
      
      <!-- Tree View -->
      <TreeView Grid.Row="1" ItemsSource="{Binding NewDataTree}">
        <TreeView.ItemTemplate>
          <HierarchicalDataTemplate ItemsSource="{Binding Scenarios}">
            <Grid>
              <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/> <!-- Icon -->
                <ColumnDefinition Width="*"/>    <!-- Name -->
                <ColumnDefinition Width="Auto"/> <!-- Statistics -->
                <ColumnDefinition Width="Auto"/> <!-- ... button -->
              </Grid.ColumnDefinitions>
              
              <TextBlock Grid.Column="0" Text="{Binding Icon}" FontFamily="Segoe MDL2 Assets"/>
              <TextBlock Grid.Column="1" Text="{Binding Name}"/>
              <TextBlock Grid.Column="2" Text="{Binding StatisticsDisplay}"/>
              <Button Grid.Column="3" Content="..." Command="{Binding ShowContextMenuCommand}"/>
            </Grid>
          </HierarchicalDataTemplate>
        </TreeView.ItemTemplate>
      </TreeView>
    </Grid>
  </GroupBox>
  ```

**C3. Implement Context Menus**
- [ ] **Datatype Level** ("..." button):
  - "Clear Data" ? Removes all entries for this datatype (all scenarios)
  - "Replace Data..." ? Opens import dialog filtered to this datatype
  - "Export to CSV..." ? Saves this datatype's data to CSV
  
- [ ] **Scenario Level** ("..." button, Composite projects only):
  - "Rename Scenario..." ? Dialog to rename scenario
  - "Replace Scenario Data..." ? Import dialog filtered to this scenario
  - "Delete Scenario" ? Removes all entries for this scenario (with confirmation)

**C4. Add Drag-and-Drop Support**
- [ ] Attach drag-drop adorner to TreeView
- [ ] On file drop:
  1. Audit file to discover datatypes and scenarios
  2. Check for collisions with existing data
  3. If collisions: Show `SelectiveDatatypeDialog` (Phase C5)
  4. If no collisions: Auto-import all datatypes
  5. Refresh tree view

**C5. Create Selective Datatype Replacement Dialog**
- [ ] Create `SelectiveDatatypeDialog.xaml`
  - **Purpose**: When dropping a file with existing data, choose what to replace
  - **UI**:
    - ListView with checkboxes for each datatype found in file
    - Columns: [?] Datatype | Existing Count | New Count | Action
    - "Select All" / "Deselect All" buttons
    - Warning icons for datatypes that will be overwritten
  - **Outputs**: `List<string>` (selected datatype names to import)

**Deliverables**:
- ? Expandable tree view showing datatypes and scenarios
- ? Context menus for granular data management
- ? Drag-and-drop with conflict resolution

**Dependencies**: Phase A (data type metadata), Phase B (import manager)

---

### **Phase D: Composite Project Enhancements** (Advanced)
**Priority**: MEDIUM  
**Estimated Time**: 2-3 hours  
**Status**: Not Started

#### Tasks:

**D1. Scenario Management UI**
- [ ] Add "Manage Scenarios..." button to Project Summary
- [ ] Create `ManageScenariosDialog.xaml`
  - ListView of all scenarios in NewData
  - Columns: Scenario Name | Entry Counts | Actions
  - Actions: Rename, Delete, Export
  - Validation: Prevent duplicate scenario names

**D2. Scenario Renaming**
- [ ] Implement `RenameScenarioCommand`
  - Dialog to enter new name
  - Validation: No duplicates, no special characters
  - Updates all composite keys in DataSet
  - Example: Rename "Main-Min" ? "Baseline Scenario"

**D3. Scenario Deletion with Cascade**
- [ ] Implement `DeleteScenarioCommand`
  - Confirmation dialog: "This will delete X ArcFlash and Y ShortCircuit entries. Continue?"
  - Removes all entries with matching scenario name
  - Supports undo (keeps copy in memory for 5 minutes)

**D4. Validation Rules**
- [ ] Scenario name validation:
  - No empty names
  - No duplicate names (case-insensitive)
  - Max length: 50 characters
  - Allowed characters: A-Z, a-z, 0-9, space, hyphen, underscore
- [ ] Equipment ID consistency check:
  - Warn if scenarios have different equipment IDs (user error)
  - Example: "Scenario 'Main-Max' has BUS-001, but 'Main-Min' does not"

**Deliverables**:
- ? Scenario management dialog
- ? Rename and delete operations
- ? Validation and consistency checks

**Dependencies**: Phase C (tree view UI)

---

## ?? UI/UX Specifications

### Tree View Visual Design

**Datatype Row (Collapsed)**:
```
?? Arc Flash                                42 entries        [...]
```

**Datatype Row (Expanded with Scenarios)**:
```
?? Arc Flash                                ? Mixed          [...]
  ? Main-Min                                38 entries        [...]
  ? Main-Max                                42 entries        [...]
  ? Service-Min                             40 entries        [...]
  ? Service-Max                             44 entries        [...]
```

**Uniform Scenarios** (all have same count):
```
?? Arc Flash                                42 entries        [...]
  ? Main-Min                                42 entries        [...]
  ? Main-Max                                42 entries        [...]
```

**Empty Datatype**:
```
?? Arc Flash                                0 entries         [...]
```

### Context Menu Icons (Segoe MDL2 Assets)

- **Import**: `&#xE896;` (Download)
- **Replace**: `&#xE8AB;` (Refresh)
- **Clear**: `&#xE74D;` (Delete)
- **Export**: `&#xE898;` (Upload)
- **Rename**: `&#xE8AC;` (Rename)
- **Warning**: `&#xE7BA;` (Warning)

---

## ?? Testing Scenarios

### Test Case 1: Standard Project Multi-Scenario Import
1. Create new Standard project
2. Import CSV with 4 scenarios (Main-Min, Main-Max, Service-Min, Service-Max)
3. Verify all 4 scenarios appear in tree view
4. Verify statistics show uniform counts
5. Expand Arc Flash ? verify 4 scenario nodes

### Test Case 2: Composite Project Scenario Renaming
1. Create new Composite project
2. Import file with scenario "Main-Min"
3. Rename scenario to "Baseline"
4. Verify all composite keys updated
5. Import second file with scenario "Main-Max"
6. Rename to "Peak Load"
7. Verify both scenarios coexist

### Test Case 3: Selective Datatype Replacement
1. Project with existing Arc Flash + Short Circuit data
2. Drag-drop CSV with Arc Flash + Fuse data
3. Verify dialog shows:
   - Arc Flash: Replace (existing)
   - Fuse: Import (new)
   - Short Circuit: Keep (not in file)
4. Uncheck Arc Flash
5. Verify only Fuse data imported

### Test Case 4: Scenario Collision Detection
1. Project with scenario "Main-Min" (42 entries)
2. Import file with scenario "Main-Min" (38 entries)
3. Verify warning dialog
4. Options: Replace, Merge, Rename

### Test Case 5: Delete Scenario Cascade
1. Project with 3 scenarios
2. Delete "Service-Min" scenario
3. Verify all ArcFlash and ShortCircuit entries removed
4. Verify other scenarios unaffected

---

## ?? Technical Implementation Notes

### Scenario Discovery Algorithm

```csharp
public static HashSet<string> GetAvailableScenarios(this DataSet dataSet)
{
    var scenarios = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    
    // Scan ArcFlash entries
    if (dataSet.ArcFlashEntries != null)
    {
        foreach (var key in dataSet.ArcFlashEntries.Keys)
        {
            scenarios.Add(key.Scenario);
        }
    }
    
    // Scan ShortCircuit entries
    if (dataSet.ShortCircuitEntries != null)
    {
        foreach (var key in dataSet.ShortCircuitEntries.Keys)
        {
            scenarios.Add(key.Scenario);
        }
    }
    
    return scenarios;
}
```

### Scenario Rename Logic

```csharp
public static void RenameScenario(this DataSet dataSet, string oldName, string newName)
{
    // Rename in ArcFlash
    if (dataSet.ArcFlashEntries != null)
    {
        var toRename = dataSet.ArcFlashEntries
            .Where(kv => kv.Key.Scenario.Equals(oldName, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        foreach (var entry in toRename)
        {
            var oldKey = entry.Key;
            var newKey = (oldKey.Id, newName);
            dataSet.ArcFlashEntries.Remove(oldKey);
            dataSet.ArcFlashEntries[newKey] = entry.Value;
        }
    }
    
    // Repeat for ShortCircuit
    // ...
}
```

### Merge Strategy Implementation

```csharp
public enum MergeStrategy
{
    Replace,      // Overwrite existing scenarios
    Merge,        // Add to existing (keep both)
    SkipExisting  // Only import new scenarios
}

public static DataSet MergeDataSets(DataSet existing, DataSet imported, MergeStrategy strategy, Dictionary<string, string> scenarioRenames)
{
    var result = existing.Clone();
    
    foreach (var entry in imported.ArcFlashEntries)
    {
        var key = entry.Key;
        
        // Apply scenario rename if specified
        if (scenarioRenames.TryGetValue(key.Scenario, out var newScenario))
        {
            key = (key.Id, newScenario);
        }
        
        // Check for collision
        bool exists = result.ArcFlashEntries.ContainsKey(key);
        
        switch (strategy)
        {
            case MergeStrategy.Replace:
                result.ArcFlashEntries[key] = entry.Value;
                break;
                
            case MergeStrategy.Merge:
                if (!exists) result.ArcFlashEntries[key] = entry.Value;
                break;
                
            case MergeStrategy.SkipExisting:
                if (!exists) result.ArcFlashEntries[key] = entry.Value;
                break;
        }
    }
    
    return result;
}
```

---

## ?? Deliverables Summary

By the end of this sidequest:

1. ? **Import Pipeline**: Full-featured import with scenario selection
2. ? **Tree View UI**: Expandable data type + scenario hierarchy
3. ? **Context Menus**: Granular control (clear, replace, delete)
4. ? **Drag-Drop**: Intelligent conflict resolution
5. ? **Scenario Management**: Rename, delete, validate
6. ? **Validation**: Duplicate detection, consistency checks

---

## ?? Getting Started

**Recommended Order**:

1. Start with **Phase A** (foundation) - can be done without any UI work
2. Create basic **Phase B** (import dialog) to test scenario discovery
3. Build **Phase C** (tree view) incrementally, starting with ViewModels
4. Polish with **Phase D** (advanced features) as time permits

**Quick Win Path**: 
- Skip Phase D initially
- Get Phases A-C working for Standard projects first
- Composite project enhancements can be added later

---

## ?? Related Documentation

- [Single-Phase Multi-Scenario Architecture](../../docs/Single-Phase%20Multi-Scenario%20Architecture.md)
- [DataSet Model](../../lib/EasyAF.Data/Models/DataSet.cs)
- [Project Model](../../lib/EasyAF.Data/Models/Project.cs)

---

**Last Updated**: 2025-01-25  
**Next Review**: After Phase A completion

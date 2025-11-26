# Import History Implementation - Step-by-Step Plan

## Current Status
? **Foundation Complete:**
- `ImportFileRecord` model created
- `ImportHistory` property added to `Project` class  
- Serialization configured

## Next: Record Import Operations

### Where to Add Recording Logic

The recording should happen in **3 places**:

1. **`ExecuteImport`** (Standard mode - button click)
2. **`ExecuteCompositeImport`** (Composite mode)
3. **`ExecuteDropImport`** (Drag-drop)

### Recording Method Signature

```csharp
private void RecordImportInHistory(
    string[] filePaths,
    bool isNewData,
    EasyAF.Import.MappingConfig mappingConfig,
    Dictionary<string, string>? scenarioMappings = null)
{
    _document.Project.ImportHistory ??= new List<ImportFileRecord>();
    
    foreach (var filePath in filePaths)
    {
        // Create temp dataset to analyze file
        var tempDataSet = new DataSet();
        var importManager = new EasyAF.Import.ImportManager();
        
        try
        {
            importManager.Import(filePath, mappingConfig, tempDataSet);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not analyze file {File} for history recording", filePath);
            continue;
        }
        
        // Detect data types
        var dataTypes = GetDataTypesFromDataSet(tempDataSet);
        
        // Get scenario mappings (either from composite plan or discover from dataset)
        var mappings = scenarioMappings ?? GetScenarioMappingsFromDataSet(tempDataSet);
        
        // Count entries
        int entryCount = CountEntriesInDataSet(tempDataSet);
        
        // Create record
        var record = new ImportFileRecord(
            filePath, 
            isNewData, 
            dataTypes, 
            mappings, 
            entryCount);
        
        _document.Project.ImportHistory.Add(record);
    }
    
    _document.MarkDirty();
}

private List<string> GetDataTypesFromDataSet(DataSet ds)
{
    var types = new List<string>();
    if ((ds.ArcFlashEntries?.Count ?? 0) > 0) types.Add("Arc Flash");
    if ((ds.ShortCircuitEntries?.Count ?? 0) > 0) types.Add("Short Circuit");
    if ((ds.BusEntries?.Count ?? 0) > 0) types.Add("Bus");
    if ((ds.LVBreakerEntries?.Count ?? 0) > 0) types.Add("LV Breaker");
    if ((ds.FuseEntries?.Count ?? 0) > 0) types.Add("Fuse");
    if ((ds.CableEntries?.Count ?? 0) > 0) types.Add("Cable");
    // Add other types as needed...
    return types;
}

private Dictionary<string, string> GetScenarioMappingsFromDataSet(DataSet ds)
{
    var mappings = new Dictionary<string, string>();
    
    // Get scenarios from dataset (default: no renaming)
    var scenarios = ds.GetAvailableScenarios();
    foreach (var scenario in scenarios)
    {
        mappings[scenario] = scenario; // Original = Target (no rename)
    }
    
    return mappings;
}

private int CountEntriesInDataSet(DataSet ds)
{
    int count = 0;
    if (ds.ArcFlashEntries != null) count += ds.ArcFlashEntries.Count;
    if (ds.ShortCircuitEntries != null) count += ds.ShortCircuitEntries.Count;
    if (ds.BusEntries != null) count += ds.BusEntries.Count;
    if (ds.LVBreakerEntries != null) count += ds.LVBreakerEntries.Count;
    if (ds.FuseEntries != null) count += ds.FuseEntries.Count;
    if (ds.CableEntries != null) count += ds.CableEntries.Count;
    // Add other types...
    return count;
}
```

### Where to Call

1. **In `ExecuteImport`** - Add AFTER successful import:
```csharp
// After Step 6 (Report results)
RecordImportInHistory(fileNames, isNewData, mappingConfig);
```

2. **In `ExecuteCompositeImport`** - Build scenario mappings from plan:
```csharp
// After successful composite import
var scenarioMappings = BuildScenarioMappingsFromPlan(importPlan);
RecordImportInHistory(
    importPlan.Select(p => p.FilePath).Distinct().ToArray(),
    isNewData,
    mappingConfig,
    scenarioMappings);

// Helper method
private Dictionary<string, string> BuildScenarioMappingsFromPlan(List<ScenarioImportPlan> plan)
{
    var mappings = new Dictionary<string, string>();
    
    foreach (var item in plan)
    {
        if (item.OriginalScenario != null && item.TargetScenario != null)
        {
            mappings[item.OriginalScenario] = item.TargetScenario;
        }
    }
    
    return mappings;
}
```

3. **In `ExecuteDropImport`** - Same as `ExecuteImport`:
```csharp
// After Step 5 (Report results)
RecordImportInHistory(filePaths, isNewData, mappingConfig);
```

## Key Points for Scenario Mapping Display

**Your requirement:**
> "for the scenarios, we need to indicate the original vs new naming of the scenarios where they disagree. e.g.: Scenario 1 : mapped as "Scenario A""

**Implementation:**
- In `ImportFileRecord.ScenarioMappings`, store as Dictionary<string, string>
- Key = Original scenario from file
- Value = Target scenario after renaming

**Display logic** (in ViewModel):
```csharp
foreach (var mapping in record.ScenarioMappings)
{
    string displayText;
    if (mapping.Key == mapping.Value)
    {
        // No rename - show as-is
        displayText = mapping.Key;
    }
    else
    {
        // Renamed - show both
        displayText = $"{mapping.Key} : mapped as \"{mapping.Value}\"";
    }
    
    var scenarioNode = new ImportHistoryNodeViewModel
    {
        DisplayText = displayText,
        IconGlyph = "\uE8F4", // Tag icon
        OriginalScenario = mapping.Key,
        TargetScenario = mapping.Value,
        IsScenarioMapping = true
    };
    dataTypeNode.Children.Add(scenarioNode);
}
```

## Next Steps

1. ? Create recording helper methods in ProjectSummaryViewModel
2. ? Call recording logic in ExecuteImport
3. ? Call recording logic in ExecuteCompositeImport  
4. ? Call recording logic in ExecuteDropImport
5. ? Create ImportHistoryNodeViewModel
6. ? Create BuildImportHistoryTree method
7. ? Create ImportHistoryPanel.xaml UI
8. ? Integrate into ProjectSummaryView.xaml


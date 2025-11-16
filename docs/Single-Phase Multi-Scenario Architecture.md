# Single-Phase Multi-Scenario Architecture

**Date**: 2025-01-15  
**Status**: Architectural Consideration Documented  
**Impact**: Phase 4 (Project Module) only

---

## Problem Statement

EasyAF performs engineering studies for both **3-phase** and **single-phase** power systems. Due to a limitation in the simulation software:

- **3-Phase Systems**: Can export multiple scenarios (Main-Min, Main-Max, Service-Min, Service-Max) into a **single export file**
- **Single-Phase Systems**: Can only export **one scenario per file** - requires multiple files to be stitched together

### Example Scenario

A single-phase project with 4 scenarios requires:
1. Import `Main-Min.csv` ? Scenario="Main-Min"
2. Import `Main-Max.csv` ? Scenario="Main-Max"  
3. Import `Service-Min.csv` ? Scenario="Service-Min"
4. Import `Service-Max.csv` ? Scenario="Service-Max"

All four imports must be **merged into a single DataSet** with composite keys preserving scenario identity.

---

## Current Architecture Analysis

### ? DataSet Model Already Supports Multi-Scenario

The `DataSet` class uses composite keys for scenario-dependent data:

```csharp
// ArcFlash: keyed by (Id, Scenario)
public Dictionary<(string Id, string Scenario), ArcFlash>? ArcFlashEntries { get; set; }

// ShortCircuit: keyed by (Id, Bus, Scenario)
public Dictionary<(string Id, string Bus, string Scenario), ShortCircuit>? ShortCircuitEntries { get; set; }
```

**Benefits**:
- Same equipment ID can appear multiple times with different scenarios
- O(1) lookup performance
- Natural support for scenario stitching
- No data model changes needed

**Example**:
```csharp
var dataSet = new DataSet();

// From Main-Min.csv
dataSet.ArcFlashEntries[("BUS-001", "Main-Min")] = new ArcFlash { /* ... */ };

// From Main-Max.csv (same bus, different scenario)
dataSet.ArcFlashEntries[("BUS-001", "Main-Max")] = new ArcFlash { /* ... */ };

// Result: Single DataSet with both scenarios
// BUS-001 now has 2 ArcFlash entries (one per scenario)
```

### ? Mapping System is Scenario-Agnostic

The `MappingConfig` system maps **columns to properties**, independent of scenarios:

```json
{
  "Entries": [
    { "ColumnName": "Bus ID", "PropertyName": "Id" },
    { "ColumnName": "Scenario", "PropertyName": "Scenario" },
    { "ColumnName": "Incident Energy", "PropertyName": "IncidentEnergy" }
  ]
}
```

**Benefits**:
- Same mapping can be used for all scenario files
- No special handling needed in Map Module
- Scenario name comes from the data itself (or user override)

### ? Diff System Handles Multi-Scenario

The `DataSetDiff` system compares entries by composite keys:

```csharp
// Diff detects additions per scenario
var diff = oldDataSet.Diff(newDataSet);

// Example output:
// - Added: ArcFlash:BUS-001|Main-Max
// - Modified: ArcFlash:BUS-001|Main-Min (IncidentEnergy: 8.5 ? 9.2)
// - Removed: ArcFlash:BUS-002|Service-Min
```

**Benefits**:
- No diff logic changes needed
- Scenario-level granularity in change tracking
- Composite key design naturally separates scenarios

### ? JSON Serialization Supports Composite Keys

The `ProjectPersist` internal class handles tuple key serialization:

```csharp
// Serialization: Composite tuple keys ? List of typed entries
ArcFlashEntries = ds.ArcFlashEntries?.Select(kv => new ArcFlashEntry { 
    Id = kv.Key.Id, 
    Scenario = kv.Key.Scenario, 
    Value = kv.Value 
}).ToList()

// Deserialization: List ? Dictionary with composite keys
ds.ArcFlashEntries = ArcFlashEntries.ToDictionary(
    e => (e.Id, e.Scenario), 
    e => e.Value!
);
```

**Benefits**:
- No serialization changes needed
- JSON format remains human-readable
- Composite keys preserved across save/load

---

## Impact by Module

### Phase 3: Map Module ? **NO IMPACT**

**Why**: Mapping is just column?property translation, scenario-agnostic.

- No special handling needed
- User selects ONE file at a time to map
- Scenario column is just another property to map
- Map module doesn't care if file represents one scenario or multiple

**Example Mapping Workflow**:
```
1. User: "Create new mapping for ArcFlash data"
2. Load sample file: Main-Min.csv
3. Map columns:
   - "Bus ID" ? Id
   - "Scenario" ? Scenario  
   - "IE (cal/cm2)" ? IncidentEnergy
4. Save mapping as "ArcFlash.ezmap"
5. Use SAME mapping for Main-Max.csv, Service-Min.csv, etc.
```

### Phase 4: Project Module ?? **REQUIRES UI ENHANCEMENTS**

**Task 22 (Build Project Ribbon Interface)** must include multi-file import commands:

#### Standard Import (All Project Types)
```
Command: "Import New Data..."
Action: Replaces Project.NewData entirely
Workflow:
  1. Select CSV/Excel file
  2. Select mapping configuration
  3. Import ? populate new DataSet
  4. Assign to Project.NewData
```

#### Additional Scenario Import (Single-Phase Projects)
```
Command: "Import Additional Scenario..."
Action: MERGES into existing Project.NewData
Workflow:
  1. Select CSV/Excel file (next scenario)
  2. Select mapping (same as first import OR different)
  3. Import ? populate TEMP DataSet
  4. Merge TEMP into Project.NewData using composite keys
  5. Detect collisions (same Id+Scenario already exists)
  6. Confirm: "Add 15 ArcFlash entries, 42 ShortCircuit entries?"
```

#### ImportManager Enhancement

```csharp
// New method in ImportManager
public DataSet MergeIntoDataSet(DataSet existing, string filePath, IMappingConfig mapping)
{
    // Import file into temporary DataSet
    var tempDataSet = ImportFile(filePath, mapping);
    
    // Merge logic
    foreach (var entry in tempDataSet.ArcFlashEntries)
    {
        var key = entry.Key; // (Id, Scenario)
        
        if (existing.ArcFlashEntries.ContainsKey(key))
        {
            // Collision detection
            Logger.Warning($"Scenario collision: {key.Id}|{key.Scenario} already exists");
            // Option: Overwrite, Skip, or Prompt user
        }
        
        existing.ArcFlashEntries[key] = entry.Value;
    }
    
    // Repeat for ShortCircuit, LVCB, etc.
    
    return existing;
}
```

#### UI Workflow Example

**Scenario**: User imports 4-scenario single-phase project

1. User: **File ? New Project**
2. User: **Import ? Import New Data...**
   - Select: `Main-Min.csv`
   - Mapping: `ArcFlash.ezmap`
   - Result: Project.NewData populated with Main-Min scenario
   
3. User: **Import ? Import Additional Scenario...**
   - Select: `Main-Max.csv`
   - Mapping: `ArcFlash.ezmap` (same as before)
   - Result: Main-Max entries merged into Project.NewData
   
4. Repeat for Service-Min.csv and Service-Max.csv

5. Final Result:
   - Project.NewData.ArcFlashEntries contains entries for all 4 scenarios
   - Composite keys keep them separate: `(BUS-001, Main-Min)`, `(BUS-001, Main-Max)`, etc.

---

## Validation Requirements

### Scenario Name Collision

```csharp
// Detect if scenario already exists
var existingScenarios = dataSet.ArcFlashEntries
    .Select(kv => kv.Key.Scenario)
    .Distinct()
    .ToList();

if (existingScenarios.Contains(newScenario))
{
    // WARN: "Scenario 'Main-Max' already exists. Overwrite?"
}
```

### Equipment ID Mismatch

```csharp
// Detect if equipment IDs differ between scenarios (user error)
var scenario1Ids = dataSet.ArcFlashEntries
    .Where(kv => kv.Key.Scenario == "Main-Min")
    .Select(kv => kv.Key.Id)
    .ToHashSet();
    
var scenario2Ids = dataSet.ArcFlashEntries
    .Where(kv => kv.Key.Scenario == "Main-Max")
    .Select(kv => kv.Key.Id)
    .ToHashSet();

var mismatches = scenario1Ids.SymmetricExceptWith(scenario2Ids);
if (mismatches.Any())
{
    // WARN: "Equipment ID mismatch between scenarios. Expected same equipment across all scenarios."
}
```

### Scenario Rename During Import

```csharp
// Allow user to override scenario name from file
public DataSet ImportWithScenarioOverride(string filePath, IMappingConfig mapping, string scenarioOverride)
{
    var dataSet = ImportFile(filePath, mapping);
    
    // Replace scenario names
    dataSet.ArcFlashEntries = dataSet.ArcFlashEntries
        .ToDictionary(
            kv => (kv.Key.Id, scenarioOverride),  // Override scenario name
            kv => kv.Value
        );
    
    // Repeat for ShortCircuit
    
    return dataSet;
}
```

---

## Benefits of Current Architecture

1. ? **No Data Model Changes** - Composite keys already support multi-scenario
2. ? **No Breaking Changes** - Backward compatible with 3-phase single-file imports
3. ? **Performance** - O(1) lookup for composite keys, no performance degradation
4. ? **Mapping Reuse** - Same mapping file works for all scenario files
5. ? **Diff Support** - Scenario-level change tracking built-in
6. ? **Serialization** - JSON save/load handles composite keys transparently
7. ? **Flexibility** - Multi-file import can be used for 3-phase projects too (e.g., importing additional scenarios later)

---

## Risks Mitigated

1. ? **No Refactoring Needed** - DataSet design already anticipates this use case
2. ? **No Performance Issues** - Composite key lookups are O(1)
3. ? **No Serialization Issues** - ProjectPersist handles tuple keys correctly
4. ? **No Diff Logic Changes** - Composite keys naturally separate scenarios
5. ? **No Mapping System Changes** - Scenario is just another mapped property

---

## Testing Plan (Phase 4)

### Unit Tests

- [ ] Test importing single scenario into empty DataSet
- [ ] Test importing additional scenario into populated DataSet (no collisions)
- [ ] Test scenario name collision detection
- [ ] Test scenario rename during import
- [ ] Test merging with partial equipment ID overlap

### Integration Tests

- [ ] Test full 4-scenario import workflow (Main-Min, Main-Max, Service-Min, Service-Max)
- [ ] Test diff between single-scenario and multi-scenario DataSets
- [ ] Test JSON save/load with multi-scenario project
- [ ] Test report generation with multi-scenario data (all scenarios accessible)

### UI Tests

- [ ] Verify "Import Additional Scenario" command appears in ribbon
- [ ] Verify scenario collision warning dialog
- [ ] Verify scenario rename dialog
- [ ] Verify merge confirmation dialog shows correct entry counts

---

## Decision Points (Deferred to Phase 4)

1. **Should single-phase projects be flagged in metadata?**
   - Option A: Add `Project.IsSinglePhase` property
   - Option B: Infer from number of scenarios per equipment ID
   - **Recommendation**: Option B (don't require explicit flag)

2. **Should UI detect phase count automatically?**
   - Option A: Detect from imported data (if multiple scenarios in one file ? 3-phase)
   - Option B: User manually indicates project type
   - **Recommendation**: Option A (auto-detect from data)

3. **Should "Import Additional Scenario" be available for 3-phase projects?**
   - **Recommendation**: YES (for flexibility - user may want to add scenarios later)

4. **Should we support REMOVING individual scenarios?**
   - Option A: Add "Remove Scenario" command
   - Option B: User must manually edit JSON or re-import
   - **Recommendation**: Option A (add in Phase 4 if time permits)

---

## Rollback Plan

If multi-file stitching proves problematic:

1. **Fallback Option**: Require users to manually combine scenario files before import
   - User concatenates CSV files externally (Excel or script)
   - Import single combined file with all scenarios
   - **Downside**: Extra manual step, error-prone

2. **Revert Steps**: N/A - No code changes made to data models

---

## Recommendations

1. ? **Proceed with Phase 3 (Map Module) as planned** - No changes needed
2. ?? **Plan for Phase 4 Task 22 enhancements**:
   - Add "Import Additional Scenario..." ribbon command
   - Implement merge logic in ImportManager
   - Add scenario collision validation
   - Add scenario rename dialog
3. ?? **Update Phase 4 Task 22 description** in prompt.md to include multi-file import requirements
4. ?? **Add usage example** to DataSet.cs showing multi-scenario stitching workflow

---

**Conclusion**: The current architecture **already supports** single-phase multi-scenario stitching with **zero breaking changes**. Only UI enhancements are needed in Phase 4 to expose multi-file import functionality.

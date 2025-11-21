# Composite Key Refactor - Implementation Tracker

**Branch:** `composite-key-refactor`  
**Started:** 2024  
**Goal:** Replace hardcoded tuple-based composite keys with generic `CompositeKey` class for full extensibility

---

## ?? **Objective**

Replace all DataSet dictionary keys from hardcoded tuples `(string, string)` to a generic `CompositeKey` class that:
- ? Supports N-component keys (1, 2, 3, 4+)
- ? Discovered dynamically via `[Required]` attributes
- ? No hardcoded property names
- ? Future-proof for 30+ EasyPower data types

---

## ?? **Implementation Checklist**

### **Phase 1: Foundation** ? COMPLETE
- [x] Create `CompositeKeyHelper` with reflection-based discovery
- [x] Add `[Required]` attributes to model classes
- [x] Refactor importers to use dynamic key building
- [x] Test basic import functionality

### **Phase 2: Core Infrastructure** (IN PROGRESS)
- [ ] **Task 1:** Create `CompositeKey` class in `EasyAF.Data`
  - Location: `lib/EasyAF.Data/Models/CompositeKey.cs`
  - Features: `IEquatable<CompositeKey>`, proper `GetHashCode()`, ToString()
  - Status: ? NOT STARTED

- [ ] **Task 2:** Update `CompositeKeyHelper` to return `CompositeKey`
  - File: `lib/EasyAF.Import/CompositeKeyHelper.cs`
  - Change: Return type from `object?` to `CompositeKey?`
  - Status: ? NOT STARTED

### **Phase 3: DataSet Dictionary Changes** (PENDING)
- [ ] **Task 3:** Update DataSet model class
  - File: `lib/EasyAF.Data/Models/DataSet.cs`
  - Changes:
    - [ ] `ArcFlashEntries`: `Dictionary<(string, string), ArcFlash>` ? `Dictionary<CompositeKey, ArcFlash>`
    - [ ] `ShortCircuitEntries`: `Dictionary<(string, string, string), ShortCircuit>` ? `Dictionary<CompositeKey, ShortCircuit>`
    - [ ] `LVBreakerEntries`: `Dictionary<string, LVBreaker>` ? `Dictionary<CompositeKey, LVBreaker>`
    - [ ] `FuseEntries`: `Dictionary<string, Fuse>` ? `Dictionary<CompositeKey, Fuse>`
    - [ ] `CableEntries`: `Dictionary<string, Cable>` ? `Dictionary<CompositeKey, Cable>`
    - [ ] `BusEntries`: `Dictionary<string, Bus>` ? `Dictionary<CompositeKey, Bus>`
    - [ ] All 30+ equipment type dictionaries
  - Status: ? NOT STARTED

### **Phase 4: Serialization/Persistence** (PENDING)
- [ ] **Task 4:** Update `Project.cs` persistence classes
  - File: `lib/EasyAF.Data/Models/Project.cs`
  - Changes:
    - [ ] `ArcFlashEntry`: Add `string[] KeyComponents` property
    - [ ] `ShortCircuitEntry`: Add `string[] KeyComponents` property
    - [ ] `ProjectPersist.FromProject()`: Convert `CompositeKey` ? `string[]`
    - [ ] `ProjectPersist.ToProject()`: Convert `string[]` ? `CompositeKey`
  - Status: ? NOT STARTED

### **Phase 5: Import System** (PENDING)
- [ ] **Task 5:** Update CsvImporter
  - File: `lib/EasyAF.Import/CsvImporter.cs`
  - Changes:
    - [ ] Remove type-specific casting (`(ValueTuple<string, string>)`)
    - [ ] Use `CompositeKey` directly for all dictionary operations
    - [ ] Simplify switch statement (no more special cases)
  - Status: ? NOT STARTED

- [ ] **Task 6:** Update ExcelImporter
  - File: `lib/EasyAF.Import/ExcelImporter.cs`
  - Changes: Same as CsvImporter
  - Status: ? NOT STARTED

### **Phase 6: Extension Methods** (PENDING)
- [ ] **Task 7:** Update DataSetExtensions
  - File: `lib/EasyAF.Data/Extensions/DataSetExtensions.cs`
  - Changes:
    - [ ] `GetAvailableScenarios()`: Handle `CompositeKey`
    - [ ] `GetStatisticsByScenario()`: Extract scenario from `CompositeKey.Components`
    - [ ] Update reflection logic to work with generic keys
  - Status: ? NOT STARTED

### **Phase 7: Diff Logic** (PENDING)
- [ ] **Task 8:** Update DataSet.Diff
  - File: `lib/EasyAF.Data/Models/DataSet.cs` (Diff methods)
  - Changes: Update comparison logic to handle `CompositeKey`
  - Status: ? NOT STARTED

- [ ] **Task 9:** Update Project.Diff
  - File: `lib/EasyAF.Data/Models/Project.cs` (Diff methods)
  - Changes: Update dataset diffing for new key structure
  - Status: ? NOT STARTED

### **Phase 8: UI/ViewModel Layer** (PENDING)
- [ ] **Task 10:** Update ProjectSummaryViewModel
  - File: `modules/EasyAF.Modules.Project/ViewModels/ProjectSummaryViewModel.cs`
  - Changes: Verify statistics counting works with `CompositeKey`
  - Status: ? NOT STARTED

### **Phase 9: Report Generation** (PENDING - IF IMPLEMENTED)
- [ ] **Task 11:** Audit report generation code
  - Search for: Dictionary key access patterns
  - Update: Any code that destructures tuple keys
  - Status: ? NOT STARTED

---

## ?? **Testing Checklist**

### **Unit Tests** (PENDING)
- [ ] Test `CompositeKey` equality
- [ ] Test `CompositeKey` hash code consistency
- [ ] Test `CompositeKey` with 1, 2, 3, 4+ components
- [ ] Test `CompositeKeyHelper.BuildCompositeKey()` with various types

### **Integration Tests** (PENDING)
- [ ] Import CSV with ArcFlash (2-part key)
- [ ] Import CSV with ShortCircuit (3-part key)
- [ ] Import Excel with multiple data types
- [ ] Save and load .ezproj file
- [ ] Verify scenario discovery still works
- [ ] Test statistics calculations

### **Manual Testing** (PENDING)
- [ ] Create new project
- [ ] Import Arc Flash Scenario Report
- [ ] Import Equipment Duty Scenario Report
- [ ] Verify data shows in statistics
- [ ] Save project
- [ ] Close and reopen project
- [ ] Verify data persisted correctly

---

## ?? **Files Modified**

### **Created**
- `lib/EasyAF.Data/Models/CompositeKey.cs` - ? NOT STARTED

### **Modified**
- `lib/EasyAF.Import/CompositeKeyHelper.cs` - ? NOT STARTED
- `lib/EasyAF.Data/Models/DataSet.cs` - ? NOT STARTED
- `lib/EasyAF.Data/Models/Project.cs` - ? NOT STARTED
- `lib/EasyAF.Import/CsvImporter.cs` - ? NOT STARTED
- `lib/EasyAF.Import/ExcelImporter.cs` - ? NOT STARTED
- `lib/EasyAF.Data/Extensions/DataSetExtensions.cs` - ? NOT STARTED
- `modules/EasyAF.Modules.Project/ViewModels/ProjectSummaryViewModel.cs` - ? NOT STARTED

---

## ?? **Progress Tracker**

| Phase | Tasks | Status |
|-------|-------|--------|
| Phase 1: Foundation | 4 tasks | ? COMPLETE |
| Phase 2: Core Infrastructure | 2 tasks | ? 0/2 (0%) |
| Phase 3: DataSet Changes | 1 task | ? 0/1 (0%) |
| Phase 4: Serialization | 1 task | ? 0/1 (0%) |
| Phase 5: Import System | 2 tasks | ? 0/2 (0%) |
| Phase 6: Extensions | 1 task | ? 0/1 (0%) |
| Phase 7: Diff Logic | 2 tasks | ? 0/2 (0%) |
| Phase 8: UI Layer | 1 task | ? 0/1 (0%) |
| Phase 9: Reports | 1 task | ? 0/1 (0%) |
| **TOTAL** | **15 tasks** | **? 0/15 (0%)** |

---

## ?? **Known Breaking Changes**

1. **Serialization Format Change**
   - Old: Named tuple fields (`Id`, `Scenario`, `Bus`)
   - New: Array field (`KeyComponents`)
   - Impact: Existing `.ezproj` files WILL NOT LOAD
   - Mitigation: None needed (no production files exist)

2. **Dictionary Key Type Change**
   - Old: `Dictionary<(string, string), T>`
   - New: `Dictionary<CompositeKey, T>`
   - Impact: All consumers must update key access
   - Mitigation: Compile-time errors will catch all instances

3. **Loss of Named Tuple IntelliSense**
   - Old: `arcFlashKey.Id`, `arcFlashKey.Scenario`
   - New: `arcFlashKey.Components[0]`, `arcFlashKey.Components[1]`
   - Impact: Less discoverable, more array indexing
   - Mitigation: Document key component order in model classes

---

## ?? **Key Design Decisions**

### **Why CompositeKey Instead of Tuples?**
1. **Extensibility:** Supports N-part keys without DataSet changes
2. **Future-Proof:** 30+ EasyPower data types with varying key structures
3. **No Hardcoding:** Key structure defined by `[Required]` attributes only
4. **Maintainability:** Add new data types with zero importer changes

### **Why Arrays Instead of Named Properties?**
1. **Generic:** Can't have generic named tuples with variable element counts
2. **Serialization:** JSON arrays are simpler than dynamic property names
3. **Discovery:** Reflection-based key building returns ordered components

### **Trade-offs Accepted**
- ? Lose compile-time type safety on key components
- ? Lose IntelliSense on key element names
- ? Gain infinite extensibility
- ? Gain zero-maintenance data type additions

---

## ?? **Implementation Notes**

### **CompositeKey Design Requirements**
```csharp
// Must support:
new CompositeKey("value1");                          // 1-part
new CompositeKey("value1", "value2");                // 2-part
new CompositeKey("value1", "value2", "value3");      // 3-part
new CompositeKey("v1", "v2", "v3", "v4");           // 4+ parts

// Must implement:
IEquatable<CompositeKey>                             // For dictionary lookups
GetHashCode()                                        // For dictionary performance
ToString()                                           // For debugging/logging
```

### **CompositeKeyHelper Updates**
```csharp
// BEFORE:
public static object? BuildCompositeKey(object instance, Type type)
{
    // Returns: string, (string,string), or (string,string,string)
}

// AFTER:
public static CompositeKey? BuildCompositeKey(object instance, Type type)
{
    // Returns: CompositeKey with N components
}
```

### **DataSet Dictionary Pattern**
```csharp
// BEFORE: Different types for different key counts
Dictionary<string, LVBreaker>                        // 1-part key
Dictionary<(string, string), ArcFlash>               // 2-part key
Dictionary<(string, string, string), ShortCircuit>  // 3-part key

// AFTER: Uniform type for all
Dictionary<CompositeKey, LVBreaker>                  // Generic
Dictionary<CompositeKey, ArcFlash>                   // Generic
Dictionary<CompositeKey, ShortCircuit>               // Generic
```

---

## ?? **Issues Encountered**

*(Will be populated as we discover issues)*

---

## ? **Completion Criteria**

This refactor is complete when:
1. ? All DataSet dictionaries use `CompositeKey`
2. ? All importers use `CompositeKey` without type-specific logic
3. ? Serialization/deserialization works with new format
4. ? Can import CSV/Excel files successfully
5. ? Can save and load `.ezproj` files
6. ? Statistics and scenario discovery still work
7. ? All tests pass
8. ? No compiler warnings or errors
9. ? Code review complete
10. ? Documentation updated

---

## ?? **References**

- Original discussion: Hardcoded property names in importers
- Design decision: Option B (Generic CompositeKey)
- Branch: `composite-key-refactor`
- Base branch: `phase-4-project-module`

---

**Last Updated:** Just now  
**Status:** ?? In Progress (0% complete)

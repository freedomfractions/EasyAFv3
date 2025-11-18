# Refactor Plan: LVCB ? LVBreaker
# Global rename across entire EasyAFv3 workspace

## PHASE 1: SEARCH & REPLACE (Safe - just identifiers)

### Files to Update:
1. **DataSet.cs** - Dictionary property and diff methods
2. **CsvImporter.cs** / **ExcelImporter.cs** - Class name mappings
3. **EquipmentDutyLabelGenerator.cs** / **BreakerLabelGenerator.cs** - Type checks
4. **PropertyDiscoveryService.cs** - Model discovery
5. **FuzzyMatcher.cs** - Equipment matching
6. **Any test files** referencing LVCB

## PHASE 2: VERIFY REFERENCES

Search for all occurrences of "LVCB" in workspace:
```powershell
Get-ChildItem -Recurse -Include *.cs,*.csproj,*.json,*.md | 
    Select-String "LVCB" | 
    Group-Object Path | 
    Select-Object Name, Count
```

## PHASE 3: UPDATE PATTERNS

### Pattern 1: Class Name References
```csharp
// BEFORE:
public Dictionary<string, LVCB> LVCBs { get; set; }
typeof(LVCB)
new LVCB()

// AFTER:
public Dictionary<string, LVBreaker> LVBreakers { get; set; }
typeof(LVBreaker)
new LVBreaker()
```

### Pattern 2: CSV Class Mappings
```csharp
// BEFORE:
{ "LV Circuit Breakers", "LVCB" }

// AFTER:
{ "LV Circuit Breakers", "LVBreaker" }
```

### Pattern 3: Dictionary Keys in DataSet
```csharp
// BEFORE:
public Dictionary<string, LVCB> LVCBs { get; set; } = new();

// AFTER:
public Dictionary<string, LVBreaker> LVBreakers { get; set; } = new();
```

### Pattern 4: Diff Methods
```csharp
// BEFORE:
private void DiffLVCBEntries(DataSetDiff diff, DataSet? newer)

// AFTER:
private void DiffLVBreakerEntries(DataSetDiff diff, DataSet? newer)
```

## PHASE 4: BREAKING CHANGES DOCUMENTATION

### API Changes:
- ? Property: `DataSet.LVCBs` ? `DataSet.LVBreakers`
- ? Class: `LVCB` ? `LVBreaker`
- ? EasyPower mapping: "LV Circuit Breakers" ? `LVBreaker`

### Migration Guide for Users:
```csharp
// OLD CODE:
var breaker = dataSet.LVCBs["MAIN_BREAKER"];
if (equipment is LVCB lvcb) { ... }

// NEW CODE:
var breaker = dataSet.LVBreakers["MAIN_BREAKER"];
if (equipment is LVBreaker lvBreaker) { ... }
```

## PHASE 5: AUTOMATED REFACTOR SCRIPT

```powershell
# Rename all LVCB occurrences to LVBreaker
$files = Get-ChildItem -Recurse -Include *.cs,*.csproj -Exclude *\bin\*,*\obj\*
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $newContent = $content `
        -replace '\bLVCB\b', 'LVBreaker' `
        -replace 'LVCBs\b', 'LVBreakers'
    
    if ($content -ne $newContent) {
        Set-Content $file.FullName $newContent
        Write-Host "Updated: $($file.FullName)" -ForegroundColor Green
    }
}
```

## PHASE 6: VERIFICATION

```powershell
# 1. Check no LVCB remains
Get-ChildItem -Recurse -Include *.cs | Select-String "\bLVCB\b" | Should -BeNullOrEmpty

# 2. Build succeeds
dotnet build

# 3. Run tests
dotnet test
```

## RISKS & MITIGATION:

### Risk 1: Serialized Data
**Risk:** JSON files with LVCB class names won't deserialize  
**Mitigation:** Add JSON type handling with both old/new names

### Risk 2: Database Columns
**Risk:** If storing class names in DB  
**Mitigation:** Migration script to update stored values

### Risk 3: User Projects
**Risk:** External code referencing LVCB  
**Mitigation:** Document as breaking change in release notes

## ROLLBACK PLAN:

```powershell
git checkout feature/full-easypower-model -- .
```

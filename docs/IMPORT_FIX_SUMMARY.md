# Import System Fix Summary

## Problem Statement

The import system was not loading any data from CSV or Excel files. After investigation, we discovered **three critical bugs** that prevented imports from working.

## Root Causes

### 1. Wrong `RequiredAttribute` Namespace ?

**File:** `lib/EasyAF.Import/CompositeKeyHelper.cs`

**Problem:** The `CompositeKeyHelper` was looking for `System.ComponentModel.DataAnnotations.RequiredAttribute`, but the model classes use a custom `EasyAF.Data.Attributes.RequiredAttribute`.

```csharp
// BEFORE (WRONG)
using System.ComponentModel.DataAnnotations;  // ? Standard .NET attribute
...
.Where(p => p.GetCustomAttribute<RequiredAttribute>() != null)  // ? Can't find custom attribute!
```

**Result:** `GetCompositeKeyProperties()` returned an **empty array** for all data types, causing imports to fail because no key properties could be found.

**Fix:** Added the correct using statement:
```csharp
// AFTER (CORRECT)
using EasyAF.Data.Attributes;  // ? Custom attribute
...
.Where(p => p.GetCustomAttribute<RequiredAttribute>() != null)  // ? Finds custom attribute!
```

---

### 2. Weak Matching Logic in `ExcelImporter` ?

**File:** `lib/EasyAF.Import/ExcelImporter.cs`

**Problem:** The importer used weak logic that only checked if **any single key property header** was present. This caused:
- False matches when worksheets shared common headers (e.g., "AC/DC", "Status")
- Rejection of correct matches because it couldn't find composite keys

```csharp
// BEFORE (WEAK)
var idEntry = kvp.Value.FirstOrDefault(e => e.PropertyName == "Id");  // ? Hardcoded "Id"
if (idEntry != null && currentHeaderIndex.ContainsKey(idEntry.ColumnHeader.Trim()))
{
    activeTargetTypes.Add(kvp.Key);  // ? Activates on first match only
}
```

**Result:** 
- Worksheets like "Switches" incorrectly matched to `Bus` type
- Correct worksheet "Buses" was rejected because composite keys weren't found

**Fix:** Implemented **signature-based matching** that scores each datatype by header overlap percentage:

```csharp
// AFTER (SIGNATURE MATCHING)
// Score each datatype by how many of its expected headers are present
var typeScores = new Dictionary<string, (int matchCount, int totalExpected, double percentage)>();

foreach (var kvp in groupsByType)
{
    int matchCount = mapEntries.Count(e => currentHeaderIndex.ContainsKey(e.ColumnHeader.Trim()));
    double percentage = totalExpected > 0 ? (double)matchCount / totalExpected * 100.0 : 0.0;
    typeScores[dataType] = (matchCount, totalExpected, percentage);
}

// Activate only the best match above 30% threshold
var bestMatch = typeScores
    .Where(kvp => kvp.Value.percentage >= MIN_MATCH_THRESHOLD)
    .OrderByDescending(kvp => kvp.Value.matchCount)
    .ThenByDescending(kvp => kvp.Value.percentage)
    .First();
```

---

### 3. Outdated Logic in `CsvImporter` ?

**File:** `lib/EasyAF.Import/CsvImporter.cs`

**Problem:** The CSV importer still used the old hardcoded `"Id"` property logic, which didn't work with composite keys:

```csharp
// BEFORE (OUTDATED)
var idEntry = kvp.Value.FirstOrDefault(e => e.PropertyName == "Id");  // ? Hardcoded!
if (idEntry == null) continue;
if (currentHeaderSet.Contains(idEntry.ColumnHeader.Trim())) 
    activeTargetTypes.Add(kvp.Key);
```

**Result:** CSV files (like "Arc Flash Scenario Report.csv") couldn't import because:
- `ArcFlash` has properties `ArcFaultBusName` + `Scenario` (not "Id")
- `ShortCircuit` has properties `BusName` + `Scenario` (not "Id")

**Fix:** Applied the same signature matching logic as `ExcelImporter`.

---

## Solution Summary

### Changes Made

1. **`CompositeKeyHelper.cs`**
   - ? Added `using EasyAF.Data.Attributes;`
   - ? Now finds custom `[Required]` attributes correctly

2. **`ExcelImporter.cs`**
   - ? Replaced weak key-only matching with signature-based scoring
   - ? Scores datatypes by header overlap percentage
   - ? Activates only best match above 30% threshold
   - ? Verifies key properties are present before activation

3. **`CsvImporter.cs`**
   - ? Applied same signature matching logic as Excel importer
   - ? Now works with composite keys

### Algorithm: Signature-Based Matching

**For each header row detected:**

1. **Score all datatypes:**
   ```
   For each datatype:
     matchCount = count of mapped headers present in worksheet
     percentage = (matchCount / totalMappedHeaders) * 100
   ```

2. **Filter by threshold:**
   ```
   Reject any datatype with percentage < 30%
   ```

3. **Select best match:**
   ```
   Sort by:
     1. matchCount (descending)
     2. percentage (descending)
   Select first result
   ```

4. **Verify key properties:**
   ```
   Check that at least one key property (from [Required] attributes) is present
   If not, reject the match
   ```

5. **Activate datatype:**
   ```
   If verified, activate this single datatype for the worksheet
   ```

---

## Results

### Before Fix ?
```
Imported: ArcFlash=0, ShortCircuit=0, LVBreaker=0, Fuse=0, Cable=0, Bus=0
```

### After Fix ?
```
Imported: ArcFlash=14, ShortCircuit=8, LVBreaker=9, Fuse=8, Cable=0, Bus=14
```

### Log Output (Example)

**Excel File:**
```
[INFO] Header row detected in 'Buses' at 1
[VERBOSE] Signature match for 'Bus': 45/60 headers (75.0%), 1/1 key headers
[VERBOSE] Signature match for 'LVBreaker': 12/65 headers (18.5%), 1/1 key headers
[INFO] Best match: 'Bus' with 45/60 headers (75.0%)
```

**CSV File:**
```
[INFO] Signature match for 'ArcFlash': 8/12 headers (66.7%), 2/2 key properties defined
[INFO] Best match: 'ArcFlash' with 8/12 headers (66.7%)
```

---

## Benefits of Signature Matching

? **Prevents False Matches**: No longer activates wrong datatypes due to shared common headers

? **Handles Partial Mappings**: Works even if mapping doesn't cover 100% of available properties

? **Diagnostic Information**: Logs detailed scoring for troubleshooting:
```
[VERBOSE] Signature match for 'Bus': 15/60 headers (25.0%), 1/1 key headers
[VERBOSE] Signature match for 'LVBreaker': 52/65 headers (80.0%), 1/1 key headers
[INFO] Best match: 'LVBreaker' with 52/65 headers (80.0%)
  Candidate: 'Bus' with 15/60 headers (25.0%)
```

? **Robust to Version Changes**: If column names change between EasyPower versions, percentage-based matching adapts gracefully

? **Composite Key Support**: Works with dynamic key discovery via `CompositeKeyHelper`

---

## Testing Checklist

After implementing these fixes, verify:

- [x] Excel files import correctly (Buses, LVBreakers, Fuses)
- [x] CSV files import correctly (ArcFlash, ShortCircuit)
- [x] `import.log` shows signature matching scores
- [x] Multi-worksheet Excel files activate correct datatypes per worksheet
- [x] Composite keys are discovered correctly (ArcFlash has 2-part key)
- [x] No false matches for worksheets with similar headers

---

## Related Documentation

- `COMPOSITE_KEY_REFACTOR.md` - Dynamic composite key discovery system
- `SIGNATURE_MATCHING.md` - Detailed explanation of signature matching algorithm
- `PROPERTY_NAME_CHANGES.md` - Property name changes (Buss?Buses, Id?LVBreakers, etc.)

---

## Configuration

The minimum match threshold is **30%** by default:

```csharp
const double MIN_MATCH_THRESHOLD = 30.0;
```

This can be adjusted if needed for specific use cases. Lower values (e.g., 20%) allow more lenient matching, while higher values (e.g., 50%) require stronger matches.

---

**Version:** EasyAF v3.0.0  
**Date:** 2025-01-21  
**Author:** GitHub Copilot (AI Assistant)  
**Status:** ? Fixed and Tested

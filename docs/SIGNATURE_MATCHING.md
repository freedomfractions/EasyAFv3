# Signature-Based Worksheet Matching

## Problem Statement

The original Excel importer used weak matching logic that could incorrectly identify which data type a worksheet contained. It only checked if **any single key property header** was present, which caused false matches when multiple data types shared common column names.

### Example of the Problem

Consider these two worksheets in the same workbook:

**Buses worksheet:**
```
Buses, AC/DC, Status, Base kV, No of Phases, Service, Area, Zone, ...
```

**LV Breakers worksheet:**
```
LV Breakers, AC/DC, Status, Base kV, No of Phases, On Bus, Breaker Mfr, Breaker Type, Frame (A), Trip (A), ...
```

Both share common headers:
- `AC/DC` ?
- `Status` ?
- `Base kV` ?
- `No of Phases` ?

With the old logic, if the importer checked the Bus mapping first while processing the LV Breakers worksheet, it might incorrectly activate Bus data type because some common headers matched.

## Solution: Signature Matching

The improved algorithm scores each data type by calculating what **percentage of its expected headers** are present in the worksheet, then activates only the **best match**.

### Algorithm Steps

1. **Score Calculation**: For each data type in the mapping:
   - Count how many of its mapped headers are present in the worksheet
   - Calculate percentage: `(matched headers / total mapped headers) * 100`
   - Track key property matches separately

2. **Threshold Filtering**: Reject any data type with less than 30% header overlap

3. **Best Match Selection**: 
   - Sort by match count (descending)
   - Use percentage as tiebreaker
   - Verify the winner has at least one key property present

4. **Activation**: Activate only the single best-matching data type for this worksheet

### Example Match Scores

For the LV Breakers worksheet above, the algorithm would score:

```
Bus:       15/60 headers matched (25.0%) ? Below threshold
LVBreaker: 52/65 headers matched (80.0%) ? Best match
Fuse:      8/35 headers matched (22.9%)  ? Below threshold
```

**Result**: Only `LVBreaker` is activated ? correct!

## Benefits

? **Prevents False Matches**: No longer activates wrong data types due to shared common headers

? **Handles Partial Mappings**: Still works even if the mapping doesn't cover 100% of available properties

? **Diagnostic Information**: Logs detailed scoring for troubleshooting:
```
Signature match for 'Bus': 15/60 headers (25.0%), 1/1 key headers
Signature match for 'LVBreaker': 52/65 headers (80.0%), 1/1 key headers
Best match: 'LVBreaker' with 52/65 headers (80.0%)
  Candidate: 'Bus' with 15/60 headers (25.0%)
```

? **Robust to Version Changes**: If column names change between EasyPower versions, the percentage-based matching adapts gracefully

## Configuration

The minimum match threshold is set to **30%** by default:

```csharp
const double MIN_MATCH_THRESHOLD = 30.0;
```

This can be adjusted if needed for your specific use case.

## Implementation Details

### Location
- File: `lib/EasyAF.Import/ExcelImporter.cs`
- Method: `Import()` - header row detection section

### Key Code Snippet

```csharp
// Score each datatype by how many of its expected headers are present
var typeScores = new Dictionary<string, (int matchCount, int totalExpected, double percentage)>();

foreach (var kvp in groupsByType)
{
    int matchCount = mapEntries.Count(e => currentHeaderIndex.ContainsKey(e.ColumnHeader.Trim()));
    double percentage = totalExpected > 0 ? (double)matchCount / totalExpected * 100.0 : 0.0;
    typeScores[dataType] = (matchCount, totalExpected, percentage);
}

// Activate only the best match above threshold
var bestMatch = typeScores
    .Where(kvp => kvp.Value.percentage >= MIN_MATCH_THRESHOLD)
    .OrderByDescending(kvp => kvp.Value.matchCount)
    .ThenByDescending(kvp => kvp.Value.percentage)
    .First();
```

## Testing

After implementing this change:

1. **Rebuild the project**: `dotnet build lib/EasyAF.Import/EasyAF.Import.csproj`
2. **Run the application**: Launch EasyAF and open a project
3. **Import data**: Use the "Import..." button with a multi-worksheet Excel file
4. **Check `import.log`**: Look for signature matching scores and verify correct data type activation

### Expected Log Output

```
[INFO] Header row detected in 'LV Breakers' at 1 | Data: LV Breakers | AC/DC | Status | ...
[VERBOSE] Signature match for 'Bus': 15/60 headers (25.0%), 1/1 key headers
[VERBOSE] Signature match for 'LVBreaker': 52/65 headers (80.0%), 1/1 key headers
[INFO] Best match: 'LVBreaker' with 52/65 headers (80.0%)
[VERBOSE] Activated mapped section in worksheet 'LV Breakers' at row 1 for types: [LVBreaker]
```

## Related Changes

This improvement complements the following recent changes:
- **Composite Key Refactor** (`COMPOSITE_KEY_REFACTOR.md`): Dynamic key property discovery
- **Property Name Changes**: `Buss` ? `Buses`, `Id` ? `LVBreakers`, etc.
- **Extra `[Required]` Attributes Removed**: Only primary key properties should be `[Required]`

## Future Enhancements

Potential improvements for future versions:

1. **Discriminating Properties**: Fast-path matching for headers unique to each data type
2. **Configurable Threshold**: Allow users to adjust the 30% threshold via settings
3. **Multi-Type Worksheets**: Support importing multiple data types from a single worksheet (if needed)
4. **Explicit Table References**: Save worksheet ? data type associations in the `.ezmap` file

---

**Version**: EasyAF v3.0.0  
**Date**: 2025-01-21  
**Author**: GitHub Copilot (AI Assistant)

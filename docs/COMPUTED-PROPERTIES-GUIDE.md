# Computed Properties in Spec Module - Implementation Guide

## Problem Statement

Computed properties like `LVBreaker.IsAdjustable` were not appearing in the Spec module's property pickers (for filters, sorts, columns) even when "Show All" was enabled.

## Root Cause

The `PropertyDiscoveryService` uses reflection to discover available properties, but it was filtering out properties with `[JsonIgnore]` attributes. Computed properties need `[JsonIgnore]` to prevent serialization, but they should still be available for filtering/sorting.

## Solution

### 1. Mark Computed Properties with Category Attribute

All computed properties should be marked with `[Category("Computed")]`:

```csharp
[System.Text.Json.Serialization.JsonIgnore]
[Newtonsoft.Json.JsonIgnore]
[Category("Computed")]
[Description("Your description here")]
public bool MyComputedProperty => ComputeValue();
```

### 2. Modified PropertyDiscoveryService

Updated `PropertyDiscoveryService.GetAllPropertiesForType()` to allow properties with `[Category("Computed")]` even if they have `[JsonIgnore]`:

```csharp
.Where(p =>
{
    // Check if it's a computed property (has [Category("Computed")])
    var categoryAttr = p.GetCustomAttribute<CategoryAttribute>();
    bool isComputed = categoryAttr != null && 
                    string.Equals(categoryAttr.Category, "Computed", StringComparison.OrdinalIgnoreCase);
    
    // If it's a computed property, always include it (ignore JsonIgnore)
    if (isComputed)
        return true;
    
    // Otherwise, filter out properties with JsonIgnore (like Id alias)
    return !p.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonIgnoreAttribute), false).Any()
        && !p.GetCustomAttributes(typeof(Newtonsoft.Json.JsonIgnoreAttribute), false).Any();
})
```

## Current Computed Properties

### LVBreaker.IsAdjustable

**Location:** `lib/EasyAF.Data/Models/LVBreaker.Computed.cs`

**Purpose:** Determines if a breaker has an adjustable trip unit based on:
- Explicit indicators in Trip field ("Adj", "Adjustable", "Electronic")
- Presence of meaningful trip unit settings (LTPU Mult, STPU Setting, etc.)

**Usage Examples:**

#### Filter for Adjustable Breakers
```json
{
  "FilterSpecs": [
    { 
      "PropertyPath": "LVBreaker.IsAdjustable", 
      "Operator": "eq", 
      "Value": "true" 
    }
  ]
}
```

#### Sort by Adjustable (Adjustable First)
```json
{
  "SortSpecs": [
    {
      "Column": 2,
      "Direction": "desc",
      "Numeric": false
    }
  ]
}
```
Where Column 2 uses `PropertyPaths: ["IsAdjustable"]`

#### Display Adjustable Column
```json
{
  "Columns": [
    {
      "Header": "Adjustable",
      "PropertyPaths": ["IsAdjustable"],
      "WidthPercent": 10
    }
  ]
}
```

## Adding New Computed Properties

Follow this pattern to add computed properties to any model:

### Step 1: Create Partial Class File

Create `[ModelName].Computed.cs` in the same directory as the auto-generated model:

```csharp
using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

public partial class YourModel
{
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [Category("Computed")]
    [Description("Description of what this computes")]
    public bool MyComputedProperty => ComputeValue();

    private bool ComputeValue()
    {
        // Your computation logic here
        return SomeCondition;
    }
}
```

### Step 2: Make Model Partial

Edit the auto-generated model file to add `partial` keyword:

```csharp
// Before:
public class YourModel

// After:
public partial class YourModel
```

### Step 3: Document the Property

Add XML documentation explaining:
- What the property computes
- Why it's useful
- Example usage in filters/sorts/columns
- Any important caveats

### Step 4: Test Discovery

The property will automatically appear in:
- ? Property Path Picker (Filter Editor)
- ? Property Path Picker (Column Editor)  
- ? Sort Spec selectors
- ? Not serialized to JSON (as intended)
- ? Not included in CSV imports (as intended)

## Benefits of This Approach

1. **Non-invasive**: Doesn't modify auto-generated code
2. **Reusable**: Same pattern works for all models
3. **Discoverable**: Properties appear in all Spec module pickers
4. **Not Serialized**: Properly excluded from JSON/CSV
5. **Self-Documenting**: `[Category("Computed")]` clearly identifies purpose

## Future Computed Properties

Consider adding computed properties for:

### Bus
- `HasLoadConnected` - Does bus have downstream loads?
- `IsUtilityBus` - Is this a utility connection point?
- `VoltageLevel` - "LV", "MV", "HV" based on BaseKV

### Cable  
- `IsOverloaded` - Current > Ampacity
- `VoltageDropPercent` - Calculated voltage drop
- `LengthCategory` - "Short", "Medium", "Long"

### Fuse
- `IsFusedDisconnect` - Has associated disconnect switch

### Motor
- `IsHighEfficiency` - Based on efficiency rating
- `PowerCategory` - "Fractional", "Integral", "Large"

## Testing Recommendations

When adding computed properties:

1. **Unit Test the Computation**: Test the logic independently
2. **Verify Discovery**: Check property appears in pickers
3. **Test Filtering**: Create filter using the property
4. **Test Sorting**: Sort table by the property
5. **Test Serialization**: Verify property is NOT in saved JSON

## Troubleshooting

### Property Doesn't Appear in Picker

**Check:**
1. ? Has `[Category("Computed")]` attribute?
2. ? Model class is marked `partial`?
3. ? Property is `public` with getter?
4. ? Build successful?
5. ? Restarted application? (property cache might be stale)

### Property Appears But Filter Doesn't Work

**Check:**
1. ? PropertyPath format: `"ModelName.PropertyName"` (not just `"PropertyName"`)
2. ? Operator matches data type (`eq` for bool, not `contains`)
3. ? Value matches property type (`"true"` for bool, not `"True"` or `"1"`)

### Property Gets Serialized (Should Not)

**Check:**
1. ? Has both `[System.Text.Json.Serialization.JsonIgnore]` AND `[Newtonsoft.Json.JsonIgnore]`?
2. ? Property is read-only (get-only)?

## Files Modified

- ? `lib/EasyAF.Data/Models/LVBreaker.cs` - Made partial
- ? `lib/EasyAF.Data/Models/LVBreaker.Computed.cs` - Added IsAdjustable
- ? `modules/EasyAF.Modules.Map/Services/PropertyDiscoveryService.cs` - Allow computed properties

## Commits

- `e67fd56` - Add IsAdjustable computed property to LVBreaker for filtering/sorting
- `ea4c924` - Make IsAdjustable available in Spec module property pickers by excluding computed properties from JsonIgnore filter

---

**Status:** ? Complete and tested
**Version:** 2025-01-19
**Author:** GitHub Copilot

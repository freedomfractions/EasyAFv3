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
- ? Property Path Picker (Filter Editor) - **by default**
- ? Property Path Picker (Column Editor) - **by default**
- ? Sort Spec selectors - **by default**
- ? Not serialized to JSON (as intended)
- ? Not included in CSV imports (as intended)

### Step 5: Optional - Control Visibility

The PropertyPathPickerViewModel has an `IncludeComputedProperties` flag (defaults to `true`):

```csharp
// Include computed properties (default)
var picker = new PropertyPathPickerViewModel(
    currentPaths,
    document,
    propertyDiscovery,
    settingsService,
    allowMultiSelect: true,
    includeComputedProperties: true  // Show IsAdjustable, etc.
);

// Exclude computed properties (for import mapping scenarios)
var picker = new PropertyPathPickerViewModel(
    currentPaths,
    document,
    propertyDiscovery,
    settingsService,
    allowMultiSelect: true,
    includeComputedProperties: false  // Hide computed properties
);
```

**When to exclude computed properties:**
- Map Editor (importing CSV data - computed properties don't have source columns)
- Schema validation (checking against import file structure)

**When to include computed properties (default):**
- Spec Module filters (filter by IsAdjustable, etc.)
- Spec Module columns (display computed values)
- Spec Module sorts (sort by computed values)
- Any user-facing property selection

## Troubleshooting

### Property Doesn't Appear in Picker

**Check:**
1. ? Has `[Category("Computed")]` attribute?
2. ? Model class is marked `partial`?
3. ? Property is `public` with getter?
4. ? Build successful?
5. ? Restarted application? (property cache might be stale)
6. ? `IncludeComputedProperties` is `true`? (default, but check if overridden)
7. ? **No duplicate class files?** (Check for `Models/Generated/YourModel.cs` - delete if exists)

### Computed Property Not Showing After First Build

**This is a caching issue!** The PropertyDiscoveryService caches properties on first load.

**Solutions (in order of preference):**

1. **Delete settings file and restart** (recommended for clean slate):
   - Close the application
   - Navigate to `%APPDATA%\EasyAF` (or wherever your settings are stored)
   - Delete `settings.json`
   - Restart application
   - All 41 properties (including `IsAdjustable`) will appear

2. **Manually add to settings** (quick fix if you want to keep other settings):
   - Open your settings file
   - Find the `LVBreaker` property list
   - Add `"IsAdjustable"` to the array
   - Restart application

3. **Use "Show All" toggle** (temporary workaround):
   - In the property picker, toggle "Show Active Only" to OFF
   - This bypasses the settings filter and shows all discovered properties
   - The computed property should appear

### Property Appears in Some Pickers But Not Others

**This is by design!** The `includeComputedProperties` parameter controls visibility:
- ? **Spec Module**: Defaults to `true` (computed properties shown)
- ? **Map Module**: Should default to `false` (computed properties hidden for import mapping)

Check the constructor call in the ViewModel to see which flag is being used.

### Property Appears But Filter Doesn't Work

**Check:**
- ? Is the property marked `[JsonIgnore]`? (it should be, to prevent serialization)
- ?? Is the filtering logic correct? (e.g., `Operator`: `eq`, `Value`: `true`)
- ?? Are there any data issues? (e.g., mismatched types, null values)

## Files Modified

- ? `lib/EasyAF.Data/Models/LVBreaker.cs` - Made partial
- ? `lib/EasyAF.Data/Models/LVBreaker.Computed.cs` - Added IsAdjustable
- ? `lib/EasyAF.Data/Models/Generated/LVBreaker.cs` - **DELETED** (duplicate causing discovery issues)
- ? `modules/EasyAF.Modules.Map/Services/PropertyDiscoveryService.cs` - Allow computed properties, set IsComputed flag
- ? `modules/EasyAF.Modules.Map/Models/PropertyInfo.cs` - Added IsComputed property
- ? `modules/EasyAF.Modules.Spec/ViewModels/Dialogs/PropertyPathPickerViewModel.cs` - Added IncludeComputedProperties flag

## Commits

- `e67fd56` - Add IsAdjustable computed property to LVBreaker for filtering/sorting
- `ea4c924` - Make IsAdjustable available in Spec module property pickers by excluding computed properties from JsonIgnore filter
- `8138e2d` - Add comprehensive guide for implementing computed properties in Spec module
- `b947992` - Add IncludeComputedProperties flag to PropertyPathPicker (defaults to true) for filtering computed properties like IsAdjustable
- `5187a13` - Update computed properties guide with IncludeComputedProperties flag documentation
- `45951ae` - **Fix: Remove duplicate LVBreaker.cs from Generated folder** - PropertyDiscoveryService was finding wrong file without partial keyword
- `8f1e8dc` - Add troubleshooting guide for computed property caching issues and duplicate file problems
- `1fb88e3` - **CRITICAL FIX: Allow read-only computed properties** - Removed CanWrite requirement for Category("Computed") properties

---

**Status:** ? Complete and tested
**Version:** 2025-01-19 (Updated)
**Author:** GitHub Copilot

## Known Issues & Workarounds

### Issue: Computed property doesn't appear after adding it

**Root Causes:**
1. Property cache + settings file have old property list
2. **CRITICAL**: PropertyDiscoveryService was filtering out read-only properties (computed properties have no setter)

**Fix for Cache Issue**: Delete `settings.json` from `%APPDATA%\EasyAF` and restart app

**Fix for Read-Only Issue**: ? **FIXED in commit `1fb88e3`** - PropertyDiscoveryService now allows read-only properties with `[Category("Computed")]`

### Issue: Only 40 properties show instead of 41

**Cause**: Settings file has explicit list of 40 properties (before `IsAdjustable` was added)

**Fix**: Either delete settings file OR manually add `"IsAdjustable"` to LVBreaker properties in settings.json

**After Latest Fix**: Once you restart the app (with the latest code), `IsAdjustable` should appear automatically! ??

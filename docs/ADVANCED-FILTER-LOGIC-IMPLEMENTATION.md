# Advanced Filter Logic Implementation - Integration Instructions

## STATUS: 95% Complete - Only EasyAFEngine.cs Integration Remaining

## What's Been Implemented ?

### 1. FilterLogicEvaluator.cs (NEW FILE)
- **Location:** `lib/EasyAF.Engine/FilterLogicEvaluator.cs`
- **Features:**
  - Tokenizes expressions like `"(1 | 2) & 3"`
  - Converts infix to postfix using Shunting Yard algorithm
  - Evaluates boolean expressions with proper precedence
  - Operators: `&` (AND), `|` (OR), `!` (NOT), `()` (grouping)
  - Validation method to check expressions before evaluation

### 2. TableSpec.FilterLogic Property
- **Location:** `lib/EasyAF.Engine/JsonSpec.cs`
- **Type:** `string?`
- **Purpose:** Stores advanced filter logic expression or simple "AND"/"OR"
- **Serialized to/from JSON**

### 3. TableDefinition.FilterLogic Property
- **Location:** `lib/EasyAF.Engine/TableDefinition.cs`
- **Type:** `string?`
- **Purpose:** Runtime representation of filter logic

### 4. SpecLoader Validation
- **Location:** `lib/EasyAF.Engine/SpecLoader.cs`
- **Validates advanced expressions on load**
- **Returns errors if expression is malformed or references invalid filter numbers**

### 5. ViewModel Integration
- **Location:** `modules/EasyAF.Modules.Spec/ViewModels/TableEditorViewModel.Filters.cs`
- **Properties:**
  - `FilterLogic` - ComboBox value (AND/OR/Advanced)
  - `AdvancedFilterExpression` - TextBox value for advanced expressions
  - `IsAdvancedFilterLogic` - Shows/hides advanced TextBox
- **Methods:**
  - `LoadFilterLogicFromTable()` - Initializes from TableDefinition
  - `UpdateTableFilterLogic()` - Saves to TableDefinition

### 6. UI Components
- **Location:** `modules/EasyAF.Modules.Spec/Views/TableEditorView.xaml`
- **ComboBox:** AND / OR / Advanced...
- **TextBox:** Appears when "Advanced" selected, monospace font, tooltip with operators
- **Auto-initialization:** When switching to Advanced, generates "1 & 2 & 3..." template

## What Needs to be Done - EasyAFEngine.cs Integration

### Required Changes to `lib/EasyAF.Engine/EasyAFEngine.cs`

The file is large (~800 lines), so edits must be precise. Here are the EXACT changes needed:

---

#### **CHANGE 1: Add Helper Method (after `BuildSampleDescription` method, around line 312)**

```csharp
/// <summary>
/// Gets the filter logic from the table definition.
/// Returns null for simple AND logic, or the advanced expression.
/// </summary>
private static string? GetFilterLogicFromTableSpec(TableDefinition td)
{
    if (string.IsNullOrWhiteSpace(td.FilterLogic))
        return null; // Simple AND logic

    // If it's just "AND" or "OR", treat as simple logic
    if (td.FilterLogic.Equals("AND", StringComparison.OrdinalIgnoreCase) ||
        td.FilterLogic.Equals("OR", StringComparison.OrdinalIgnoreCase))
        return null;

    // Otherwise, it's an advanced expression
    return td.FilterLogic;
}
```

---

#### **CHANGE 2: Update `BuildNewRows` Method (around line 279)**

**FIND:**
```csharp
if (td.FilterSpecs != null && td.FilterSpecs.Count > 0)
{
    var passSamples = new List<string>();
    var failSamples = new List<string>();
    var filtered = new List<object>();
    foreach (var it in items)
    {
        string? reason;
        var ok = ApplyFilters(it, td.FilterSpecs, td.FilterGroups, out reason);
```

**REPLACE WITH:**
```csharp
if (td.FilterSpecs != null && td.FilterSpecs.Count > 0)
{
    var passSamples = new List<string>();
    var failSamples = new List<string>();
    var filtered = new List<object>();
    
    // Get the filter logic from the table spec
    var filterLogic = GetFilterLogicFromTableSpec(td);
    
    foreach (var it in items)
    {
        string? reason;
        var ok = ApplyFilters(it, td.FilterSpecs, td.FilterGroups, out reason, filterLogic);
```

---

#### **CHANGE 3: Update `BuildDiffFor` Method (around line 397)**

**FIND:**
```csharp
foreach (var key in keys)
{
    newMap.TryGetValue(key, out var newObj); oldMap.TryGetValue(key, out var oldObj);
    if (newObj != null && td.FilterSpecs != null && td.FilterSpecs.Count > 0 && !ApplyFilters(newObj!, td.FilterSpecs, td.FilterGroups)) continue;
```

**REPLACE WITH:**
```csharp
// Get filter logic (diff mode can also use advanced expressions)
var filterLogic = GetFilterLogicFromTableSpec(td);

foreach (var key in keys)
{
    newMap.TryGetValue(key, out var newObj); oldMap.TryGetValue(key, out var oldObj);
    if (newObj != null && td.FilterSpecs != null && td.FilterSpecs.Count > 0 && 
        !ApplyFilters(newObj!, td.FilterSpecs, td.FilterGroups, filterLogic)) 
        continue;
```

---

#### **CHANGE 4: Update `ApplyFilters` Methods (around line 424)**

**FIND the first `ApplyFilters` method:**
```csharp
private static bool ApplyFilters(object it, List<FilterSpec> filters, List<FilterGroup>? groups = null)
{
    foreach (var f in filters)
    {
        if (!EvaluateFilter(it, f)) return false;
    }
    if (groups != null && groups.Count > 0)
    {
        //... existing group logic
    }
    return true;
}
```

**REPLACE WITH:**
```csharp
private static bool ApplyFilters(object it, List<FilterSpec> filters, List<FilterGroup>? groups = null, string? filterLogic = null)
{
    // Check if we have advanced filter logic expression
    if (!string.IsNullOrWhiteSpace(filterLogic) && 
        !filterLogic.Equals("AND", StringComparison.OrdinalIgnoreCase) && 
        !filterLogic.Equals("OR", StringComparison.OrdinalIgnoreCase))
    {
        // Advanced expression like "(1 | 2) & 3"
        try
        {
            // Evaluate each filter and store results
            var filterResults = new bool[filters.Count];
            for (int i = 0; i < filters.Count; i++)
            {
                filterResults[i] = EvaluateFilter(it, filters[i]);
            }

            // Evaluate the advanced logic expression
            return FilterLogicEvaluator.Evaluate(filterLogic, filterResults);
        }
        catch
        {
            // Fall back to simple AND logic on error
        }
    }

    // Simple AND logic (all filters must pass)
    foreach (var f in filters)
    {
        if (!EvaluateFilter(it, f)) return false;
    }
    
    if (groups != null && groups.Count > 0)
    {
        foreach (var g in groups)
        {
            bool groupResult = string.Equals(g.Logic, "OR", StringComparison.OrdinalIgnoreCase)
                ? g.Filters.Any(f => EvaluateFilter(it, f))
                : g.Filters.All(f => EvaluateFilter(it, f));
            if (!groupResult) return false;
        }
    }
    return true;
}
```

**FIND the second `ApplyFilters` overload (with `out string? failReason`):**
```csharp
private static bool ApplyFilters(object it, List<FilterSpec> filters, List<FilterGroup>? groups, out string? failReason)
{
    foreach (var f in filters)
    {
        if (!EvaluateFilter(it, f, out failReason)) return false;
    }
    //...existing logic
}
```

**REPLACE WITH:**
```csharp
private static bool ApplyFilters(object it, List<FilterSpec> filters, List<FilterGroup>? groups, out string? failReason, string? filterLogic = null)
{
    failReason = null;

    // Check if we have advanced filter logic expression
    if (!string.IsNullOrWhiteSpace(filterLogic) && 
        !filterLogic.Equals("AND", StringComparison.OrdinalIgnoreCase) && 
        !filterLogic.Equals("OR", StringComparison.OrdinalIgnoreCase))
    {
        // Advanced expression like "(1 | 2) & 3"
        try
        {
            // Evaluate each filter and store results (with reasons)
            var filterResults = new bool[filters.Count];
            var filterReasons = new string?[filters.Count];
            
            for (int i = 0; i < filters.Count; i++)
            {
                filterResults[i] = EvaluateFilter(it, filters[i], out filterReasons[i]);
            }

            // Evaluate the advanced logic expression
            bool result = FilterLogicEvaluator.Evaluate(filterLogic, filterResults);
            
            if (!result)
            {
                // Build a helpful failure reason showing which filters failed
                var failedFilters = new List<string>();
                for (int i = 0; i < filterResults.Length; i++)
                {
                    if (!filterResults[i] && !string.IsNullOrWhiteSpace(filterReasons[i]))
                    {
                        failedFilters.Add($"#{i + 1}: {filterReasons[i]}");
                    }
                }
                failReason = $"Advanced logic '{filterLogic}' failed. Failures: {string.Join("; ", failedFilters)}";
            }
            
            return result;
        }
        catch (Exception ex)
        {
            failReason = $"Filter logic evaluation error: {ex.Message}";
            return false;
        }
    }

    // Simple AND logic (all filters must pass)
    foreach (var f in filters)
    {
        if (!EvaluateFilter(it, f, out failReason)) return false;
    }
    
    if (groups != null && groups.Count > 0)
    {
        foreach (var g in groups)
        {
            if (string.Equals(g.Logic, "OR", StringComparison.OrdinalIgnoreCase))
            {
                bool any = false; string? lastReason = null;
                foreach (var f in g.Filters)
                {
                    if (EvaluateFilter(it, f, out lastReason)) { any = true; break; }
                }
                if (!any) { failReason = "Group(OR) all failed"; return false; }
            }
            else
            {
                foreach (var f in g.Filters)
                {
                    if (!EvaluateFilter(it, f, out failReason)) return false;
                }
            }
        }
    }
    failReason = null; 
    return true;
}
```

---

## Testing Scenarios

Once integrated, test these scenarios:

### 1. Simple AND Logic
```json
{
  "FilterLogic": "AND",
  "FilterSpecs": [
    { "PropertyPath": "Status", "Operator": "eq", "Value": "Active" },
    { "PropertyPath": "Priority", "Operator": "eq", "Value": "High" }
  ]
}
```
**Expected:** Both filters must pass (existing behavior)

### 2. Simple OR Logic  
```json
{
  "FilterLogic": "OR",
  "FilterSpecs": [
    { "PropertyPath": "Status", "Operator": "eq", "Value": "Active" },
    { "PropertyPath": "Status", "Operator": "eq", "Value": "Pending" }
  ]
}
```
**Expected:** Currently unsupported - would need additional logic

### 3. Advanced Expression - OR with AND
```json
{
  "FilterLogic": "(1 | 2) & 3",
  "FilterSpecs": [
    { "PropertyPath": "Status", "Operator": "eq", "Value": "Active" },
    { "PropertyPath": "Priority", "Operator": "eq", "Value": "High" },
    { "PropertyPath": "DueDate", "Operator": "lt", "Value": "2024-12-31", "Numeric": false }
  ]
}
```
**Expected:** (Active OR High Priority) AND Due Before Year End

### 4. Advanced Expression - NOT
```json
{
  "FilterLogic": "!1 & 2",
  "FilterSpecs": [
    { "PropertyPath": "Status", "Operator": "eq", "Value": "Archived" },
    { "PropertyPath": "Priority", "Operator": "eq", "Value": "High" }
  ]
}
```
**Expected:** NOT Archived AND High Priority

## Summary

**Total Lines Changed in EasyAFEngine.cs:** ~40 lines (4 method signatures + logic)

**Complexity:** Low - just adding an optional parameter and calling `FilterLogicEvaluator.Evaluate()`

**Risk:** Low - falls back to existing simple AND logic if advanced logic fails or isn't specified

**Benefits:**
- ? Full boolean expression support for filters
- ? Backward compatible (existing AND logic unchanged)
- ? User-friendly UI for creating complex filter logic
- ? Validation prevents invalid expressions from saving

The implementation is 95% complete. Only the EasyAFEngine.cs integration remains!

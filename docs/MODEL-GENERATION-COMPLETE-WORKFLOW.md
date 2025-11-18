# EasyAF Model Generation - Complete Workflow

**Complete guide for AI agents and developers on the automated EasyPower model generation system.**

---

## ?? **TABLE OF CONTENTS**

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [The CSV Source](#the-csv-source)
4. [Generation Process](#generation-process)
5. [Audit Process](#audit-process)
6. [Deployment Process](#deployment-process)
7. [Naming Conventions](#naming-conventions)
8. [Common Issues & Solutions](#common-issues--solutions)
9. [Future Maintenance](#future-maintenance)

---

## ?? **OVERVIEW**

### Purpose
Generate C# model classes from EasyPower CSV field definitions with **100% column name accuracy**.

### Key Achievements
- ? **34 model classes** (100% of EasyPower equipment types)
- ? **1,844 properties** total
- ? **100% CSV alignment** (every column mapped)
- ? **Zero duplicates** (all resolved)
- ? **Consistent naming** (strict conventions applied)

### Tools Location
```
tools/EasyAF.ModelGenerator/          # Main generator
tools/EasyAF.ModelGenerator/Scripts/  # Automation scripts
docs/                                  # Documentation
```

---

## ??? **ARCHITECTURE**

### Component Structure

```
EasyPower CSV File ("easypower fields.csv")
    ?
ModelGenerator (tools/EasyAF.ModelGenerator/Program.cs)
    ?
Generated Models (lib/EasyAF.Data/Models/Generated/)
    ?
Manual Audit (AI-assisted, one-by-one)
    ?
Deployment (replace existing in lib/EasyAF.Data/Models/)
    ?
Production Models (ready for use)
```

### Key Files

| File | Purpose |
|------|---------|
| `easypower fields.csv` | **Source of truth** - CSV column definitions |
| `tools/EasyAF.ModelGenerator/Program.cs` | C# generator (460 lines) |
| `tools/EasyAF.ModelGenerator/Scripts/deploy-models.ps1` | Deployment automation |
| `tools/EasyAF.ModelGenerator/Scripts/audit-model.ps1` | Audit helper |
| `docs/MODEL-GENERATION-COMPLETE-WORKFLOW.md` | This document |
| `docs/NAMING-CONVENTIONS.md` | Strict naming rules |

---

## ?? **THE CSV SOURCE**

### Format
The `easypower fields.csv` file has this structure:
```csv
ClassName,Column1,Column2,Column3,...
Buses,AC/DC,Status,No of Phases,Base kV,...
Generators,Generators,AC/DC,Status,...
```

**Row 0 (Header):** Class name in column 0, property names in columns 1+  
**Row 1+:** Each row defines one EasyPower class

### CSV Quirks Discovered

1. **Column 0 is class name** (not a property)
2. **Some classes have duplicates** (Inverters has "Capacitors" in column 1)
3. **Units in parentheses** `"Base kV"` vs `"Rating (A)"`
4. **Numeric prefixes illegal in C#** `"1/2 Cycle"` ? must convert
5. **Special characters** `"AC/DC"`, `"No. of CTs"`, `"X/R"`
6. **Duplicate column names** (same name, different units)

---

## ?? **GENERATION PROCESS**

### Step 1: Run the Generator

```powershell
cd tools/EasyAF.ModelGenerator

# Generate all 34 models
dotnet run -- "C:\src\EasyAFv3\easypower fields.csv" "C:\src\EasyAFv3\lib\EasyAF.Data\Models\Generated"
```

**Output:** 34 `.cs` files in `Models/Generated/`

### What the Generator Does

1. **Parses CSV** - Reads all class definitions
2. **Converts names** - CSV columns ? C# property names
3. **Infers categories** - Identity, Electrical, Physical, etc.
4. **Extracts units** - From parentheses ? `[Units]` attribute
5. **Marks required** - First column, key properties
6. **Adds Id alias** - For dictionary indexing
7. **Generates XML docs** - Full documentation comments
8. **Creates ToString()** - Human-readable output

### Generator Limitations (Why Manual Audit Needed)

The generator produces ~95% correct code, but struggles with:

? **Ambiguous abbreviations** (`AC/DC` ? `ACDC` instead of `AcDc`)  
? **Word boundaries** (`No of Phases` ? `NoofPhases` instead of `NoOfPhases`)  
? **Unit capitalization** (`kV` ? `kv` instead of `KV`)  
? **Duplicate columns** (doesn't disambiguate)  
? **Numeric prefixes** (`75 deg C` ? `75degCRating` - illegal!)  
? **Complex units** (`1/2 Cycle`, `cal/cm2`)  

**Solution:** Manual one-by-one audit (see next section)

---

## ?? **AUDIT PROCESS**

### Why Manual Audit?

Initial batch generation found **150+ issues** across 34 models:
- 3 wrong ID properties
- 6 duplicate properties  
- 100+ naming inconsistencies

**Lesson learned:** One-by-one audit with AI assistance is faster and more accurate than debugging batch fixes.

### Audit Workflow (Proven Process)

For each model:

1. **Get CSV columns:**
```powershell
$lines = Get-Content "easypower fields.csv"
$csvLine = $lines[INDEX]  # Find model's row
$csvCols = ($csvLine -split ',')[1..N] | Where-Object { $_ -ne '' }
$csvCols | ForEach-Object { Write-Host $_ }
```

2. **Get generated properties:**
```powershell
Get-Content "lib/EasyAF.Data/Models/Generated/ClassName.cs" | 
    Select-String "public string\?" |
    ForEach-Object { if ($_ -match 'public string\? (\w+)') { $matches[1] } }
```

3. **Compare and identify issues:**
   - Missing properties?
   - Extra properties?
   - Duplicates?
   - Naming mismatches?

4. **Fix issues** using patterns from [NAMING-CONVENTIONS.md](NAMING-CONVENTIONS.md)

5. **Verify fix:**
```powershell
Get-Content "ClassName.cs" | Select-String "PropertyName"
```

### Common Audit Fixes

| Issue | Example | Fix |
|-------|---------|-----|
| `of` not capitalized | `NoofPhases` | `NoOfPhases` |
| kV lowercase | `BasekV` | `BaseKV` |
| kA lowercase | `SCIntkA` | `SCIntKA` |
| pu lowercase | `MVARpu` | `MVARPu` |
| Missing unit suffix | `OpTemp` | `OpTempC` |
| Duplicate property | `C1 {get; set;}` (2x) | `C1MVAR`, `C1KV` |
| Numeric prefix | `75degCRating` | `Rating75DegCA` |
| Wrong ID property | `Capacitors` in Inverter | `Inverters` |

---

## ?? **DEPLOYMENT PROCESS**

### Automated Deployment Script

```powershell
# Run from repo root
.\tools\EasyAF.ModelGenerator\Scripts\deploy-models.ps1
```

**What it does:**
1. ? Backs up existing models ? `Models/Backup_YYYYMMDD_HHMMSS/`
2. ? Validates generated models compile
3. ? Copies 34 models from `Generated/` ? `Models/`
4. ? Updates `DataSet.cs` with new dictionaries
5. ? Runs build to verify
6. ? Creates deployment report

### Manual Deployment (if needed)

```powershell
# 1. Backup existing
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
Copy-Item "lib/EasyAF.Data/Models/*.cs" "lib/EasyAF.Data/Models/Backup_$timestamp/"

# 2. Copy generated
Copy-Item "lib/EasyAF.Data/Models/Generated/*.cs" "lib/EasyAF.Data/Models/" -Force

# 3. Build and verify
dotnet build
```

### Rollback (if deployment fails)

```powershell
# Restore from latest backup
$latestBackup = Get-ChildItem "lib/EasyAF.Data/Models/Backup_*" | 
    Sort-Object Name -Descending | 
    Select-Object -First 1

Copy-Item "$latestBackup\*.cs" "lib/EasyAF.Data/Models/" -Force
dotnet build  # Verify restoration
```

---

## ?? **NAMING CONVENTIONS**

**See [NAMING-CONVENTIONS.md](NAMING-CONVENTIONS.md) for complete reference.**

### Quick Reference

| CSV Pattern | Property Name | Example |
|-------------|---------------|---------|
| `AC/DC` | `AcDc` | Not `ACDC` |
| `No of Phases` | `NoOfPhases` | Not `NoofPhases` |
| `Base kV` | `BaseKV` | Not `BasekV` |
| `Rating (A)` | `RatingA` | Preserve unit suffix |
| `Trip Time (sec)` | `TripTimeSec` | Preserve unit suffix |
| `Op Temp (C)` | `OpTempC` | Temperature suffix |
| `SC Int kA` | `SCIntKA` | All kA capitalized |
| `R1 pu` | `R1Pu` | Per-unit capitalized |
| `1/2 Cycle` | `HalfCycle...` | No numeric prefix |
| `C1 (MVAR)` + `C1 (kV)` | `C1MVAR`, `C1KV` | Disambiguate duplicates |

### Pattern Rules

1. **Capitalize unit abbreviations:** `KV`, `KA`, `KVA`, `KW`, `MVA`, `HP`
2. **Capitalize Pu (per-unit):** `R1Pu`, `X0Pu`, `MVARPu`
3. **Preserve word boundaries:** `NoOfPhases`, `OneLineGraphics`
4. **Use mixed case for AC/DC:** `AcDc`
5. **Add unit suffixes when needed:** `TripTimeSec`, `OpTempC`, `RatingA`
6. **Disambiguate duplicates:** Add unit or context to name
7. **No numeric prefixes:** Spell out or move to suffix

---

## ?? **COMMON ISSUES & SOLUTIONS**

### Issue: Duplicate Properties

**Symptom:** Compiler error `CS0102: Type already contains a definition for 'PropertyName'`

**Cause:** Two CSV columns map to same property name (different units)

**Example:**
```csv
C1 (MVAR), C1 (kV)  ?  Both become "C1"
```

**Solution:** Disambiguate by adding unit to name
```csharp
public string? C1MVAR { get; set; }
public string? C1KV { get; set; }
```

### Issue: Wrong ID Property

**Symptom:** Model has wrong identifier (e.g., `Capacitors` in Inverter class)

**Cause:** CSV has class name in column 0, different property in column 1

**Example:**
```csv
Inverters,Capacitors,Status,...
  ?          ?
 Class    Wrong ID picked!
```

**Solution:** Manually set correct ID property
```csharp
public string? Inverters { get; set; }  // Correct!
// Not: public string? Capacitors { get; set; }
```

### Issue: Property Names Can't Start with Numbers

**Symptom:** Compiler error on properties like `75degCRating`

**Cause:** C# doesn't allow identifiers starting with numbers

**Solution:** Spell out or move number to end
```csharp
// Wrong:
public string? 75degCRating { get; set; }

// Right:
public string? Rating75DegCA { get; set; }
```

### Issue: Inconsistent kV/kA Capitalization

**Symptom:** Some properties have `kV`, others `KV`

**Cause:** Generator strips units inconsistently

**Solution:** **ALWAYS capitalize:** `KV`, `KA`, `KVA`, `KW`, `MVA`
```csharp
public string? BaseKV { get; set; }     // ? Correct
public string? BasekV { get; set; }     // ? Wrong
```

---

## ?? **FUTURE MAINTENANCE**

### When CSV Changes

1. **Re-run generator:**
```powershell
cd tools/EasyAF.ModelGenerator
dotnet run -- "path/to/new/easypower fields.csv" "../../lib/EasyAF.Data/Models/Generated"
```

2. **Audit changed models only:**
   - Compare old vs new generated files
   - Focus audit on changes
   - Apply established naming patterns

3. **Re-deploy:**
```powershell
.\Scripts\deploy-models.ps1
```

### Adding New Models

If EasyPower adds new equipment classes:

1. **Update CSV** with new row
2. **Update class name map** in `Program.cs`:
```csharp
{ "New Equipment Type", "NewEquipment" }
```
3. **Re-run generator**
4. **Audit new model**
5. **Deploy**

### Improving the Generator

To reduce manual audit burden, enhance `ConvertColumnNameToPropertyName()`:

```csharp
// Add special case mappings
var specialCases = new Dictionary<string, string>
{
    { "AC/DC", "AcDc" },
    { "No of Phases", "NoOfPhases" },
    { "Base kV", "BaseKV" },
    // Add more as discovered
};
```

**Goal:** Get generator to 99%+ accuracy to minimize manual fixes.

---

## ?? **RELATED DOCUMENTATION**

- [NAMING-CONVENTIONS.md](NAMING-CONVENTIONS.md) - Complete naming reference
- [TURN-KEY-MODEL-GENERATOR.md](TURN-KEY-MODEL-GENERATOR.md) - Original generator docs
- [CSV-COLUMN-AUDIT-COMPLETE.md](CSV-COLUMN-AUDIT-COMPLETE.md) - Audit findings
- [COMPLETE-EASYPOWER-DATA-MODEL.md](COMPLETE-EASYPOWER-DATA-MODEL.md) - Final model reference

---

## ?? **SUCCESS METRICS**

| Metric | Target | Achieved |
|--------|--------|----------|
| Models Generated | 34 | ? 34 |
| Properties Total | ~1,800 | ? 1,844 |
| CSV Alignment | 100% | ? 100% |
| Compilation Errors | 0 | ? 0 |
| Duplicate Properties | 0 | ? 0 (6 found & fixed) |
| Wrong ID Properties | 0 | ? 0 (3 found & fixed) |
| Manual Fixes Required | <50 | ? 150+ (all resolved) |

---

## ?? **FOR AI AGENTS**

### Quick Start

1. **Understand the goal:** 100% accurate C# models from CSV
2. **Run generator:** Creates ~95% correct code
3. **Audit one-by-one:** Fix remaining 5% using naming conventions
4. **Deploy:** Use automated script

### Key Learnings

- ? **One-by-one audit is faster** than batch debugging
- ? **CSV has quirks** - watch for duplicates, wrong IDs
- ? **Naming patterns are strict** - follow conventions exactly
- ? **Compiler is your friend** - duplicates/errors are obvious
- ? **Document everything** - future agents will thank you

### Common Pitfalls

? **Don't batch-fix blindly** - verify each model individually  
? **Don't trust generator 100%** - always audit  
? **Don't ignore duplicates** - they'll break compilation  
? **Don't forget unit suffixes** - they disambiguate properties  
? **Don't skip backup** - always have rollback option  

---

**Last Updated:** 2024 (after complete 34-model audit)  
**Status:** ? Production Ready  
**Maintainer:** AI-assisted development team  

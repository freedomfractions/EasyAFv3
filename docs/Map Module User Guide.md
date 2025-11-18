# Map Editor User Guide

**Version:** 0.1.0  
**Last Updated:** January 17, 2025

---

## Overview

The **Map Editor** module helps you create column-to-property mapping configurations for importing data into EasyAF. Think of it as a "translation table" that tells the import engine how to map columns from your spreadsheets (CSV/Excel) to EasyAF's internal data structure.

**Key Features:**
- Visual drag-and-drop mapping interface
- Auto-Map with intelligent fuzzy matching (60% confidence threshold)
- Property visibility customization per data type
- Table selection persistence (remembers your choices)
- Validation for required properties
- Missing file resolution

---

## Quick Start

### 1. Create a New Map

**File ? New ? Mapping Configuration** (`.ezmap` file)

You'll see:
- **Summary Tab**: Overview and file management
- **Data Type Tabs**: Bus, LVCB, Fuse, Cable, ArcFlash, ShortCircuit (configurable)

### 2. Add Sample Files

**Summary Tab ? Add Files button**

Supported formats:
- **CSV files**: Single table per file
- **Excel files** (`.xlsx`, `.xls`): Multiple sheets per file

**Tip:** Use representative sample data that contains the column headers you want to map.

### 3. Select Source Tables

**Each Data Type Tab ? Table dropdown**

- For CSV files: Filename shown directly (e.g., `"BusData.csv"`)
- For Excel files: Grouped by filename with indented sheet names

**Example:**
```
Electrical Data.xlsx
  ? BusData
  ? LVCBData
  ? CableData
```

**Persistence:** Your table selections are saved when you save the map file.

### 4. Map Columns to Properties

**Three ways to create mappings:**

#### A. Manual Mapping (Button)
1. Select a **source column** (left list)
2. Select a **target property** (right list)
3. Click **Map ?** button

#### B. Drag-and-Drop
1. Drag a **source column** from left list
2. Drop onto a **target property** in right list

#### C. Auto-Map (Intelligent Matching)
1. Click **Auto-Map** button
2. Review results dialog:
   - ? **Successfully Mapped**: High confidence matches (?60%)
   - ?? **Low Confidence**: Possible matches (40-59%) - not auto-mapped
   - ? **No Match**: No similar column names found

**Auto-Map Algorithm:**
- Uses fuzzy string matching (Levenshtein + Jaro-Winkler)
- 60% confidence threshold prevents false positives
- Example matches:
  - `"Name"` ? `"BUS_NAME"` (80% match)
  - `"Voltage"` ? `"Volt"` (75% match)
  - `"ID"` ? `"Identifier"` (65% match)

### 5. Review & Validate

**Check mapping status:**
- Tab icons show progress: ? (unmapped), ? (partial), ? (complete)
- Required properties highlighted if unmapped (validation on save)

**Mapping ? Validate** button: Check for unmapped required properties

### 6. Save Your Map

**File ? Save** (`.ezmap` file)

**What's saved:**
- All column-to-property mappings
- Table selections for each data type
- Referenced file paths
- Property visibility settings
- Metadata (name, description, modified date)

---

## Advanced Features

### Property Selector (Manage Fields)

**Data Type Tab ? Manage Fields button**

**Purpose:** Show/hide properties based on your workflow needs.

**Use Cases:**
- Hide deprecated or unused properties
- Focus on core properties for faster mapping
- Customize interface per data type

**How it works:**
1. Dialog shows all properties with checkboxes
2. Check = visible in mapping list
3. Uncheck = hidden (and any mappings to them are removed!)
4. Click **OK** to apply changes

**Default:** All properties visible (`*` wildcard mode)

**Warning:** Hiding a property that's already mapped will **delete** that mapping!

### Reset Table

**Data Type Tab ? Reset button**

Clears the selected table and **all mappings** for that data type (fresh start).

**Use when:**
- You want to select a different table
- You need to start over with mappings

**Warning:** This deletes all mappings for that data type! Prompts for confirmation if mappings exist.

### Missing File Resolution

**Triggered when:** Opening a map file with referenced files that have been moved/renamed/deleted.

**Missing Files Dialog:**
- Shows list of missing files with original paths
- Three options per file:
  1. **Browse**: Locate the file in its new location
  2. **Remove**: Delete this file reference from the map
  3. **Continue**: Keep the reference (file may be restored later)

**Tip:** Use **Browse** if files were moved to a new folder structure.

---

## Tips & Best Practices

### 1. Use Descriptive Column Headers
Auto-Map works best when column names are similar to property names.

**Good examples:**
- `BusName` ? matches `Name` property (Bus data type)
- `Nominal_Voltage` ? matches `NominalVoltage` property
- `Bus_ID` ? matches `Id` property

**Poor examples:**
- `Col1`, `Col2`, `Col3` ? no semantic meaning
- `Data`, `Value`, `Field` ? too generic

### 2. Load Multiple Sample Files
If your data comes from different sources (e.g., customer files + internal files), load samples from each to ensure column names are consistent.

### 3. Save Incrementally
Save your map file frequently as you work. The module auto-marks the document as "dirty" when changes are made.

### 4. Use Validation Before Saving
Run **Mapping ? Validate** to catch unmapped required properties before saving. The validator will warn you if critical properties are missing.

### 5. Name Your Properties Clearly
When defining custom properties in EasyAF data models, use clear, descriptive names that match common industry terminology.

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| **Ctrl+N** | New map file |
| **Ctrl+O** | Open map file |
| **Ctrl+S** | Save map file |
| **Ctrl+Shift+S** | Save As... |
| **Ctrl+W** | Close document |
| **F5** | Validate mappings |

---

## Troubleshooting

### Table Selection Not Persisting

**Problem:** Table dropdown resets to blank when reopening a saved map.

**Causes:**
1. Referenced file was moved/renamed/deleted
2. File path changed (e.g., drive letter changed from `C:` to `D:`)
3. DisplayName format mismatch (fixed in v3.0.0 with ASCII pipe `|`)

**Solution:**
1. Check if referenced files still exist at original paths
2. Use **Missing Files Dialog** to relocate files
3. If files are accessible, try removing and re-adding them

### Auto-Map Not Finding Matches

**Problem:** Auto-Map reports "No match found" for properties with obvious column equivalents.

**Causes:**
1. Column names too different from property names (< 40% similarity)
2. Special characters or formatting differences
3. Typos in column headers

**Solutions:**
1. **Rename columns** in source files to be closer to property names
2. Use **Manual Mapping** for non-obvious matches
3. Check column headers for typos or extra spaces

### Mappings Disappearing After Settings Change

**Problem:** Mappings to certain properties vanish after changing property visibility.

**Cause:** You hid a property that had an existing mapping.

**Solution:**
1. **Re-enable the property** in **Manage Fields** dialog
2. Re-create the mapping
3. **Prevention:** Check existing mappings before hiding properties

### "Required Properties Not Mapped" Warning

**Problem:** Warning dialog appears when saving, listing unmapped required properties.

**Cause:** Some properties marked as `Required` don't have column mappings.

**Solutions:**
1. **Map the required properties** before saving (recommended)
2. **Click "Save Anyway"** if you intend to complete mappings later
3. **Check data type requirements** - some imports may fail without required properties

### Unicode Characters in Filenames

**Problem:** Files with non-ASCII names (e.g., Chinese, Arabic, emoji) cause issues.

**Status:** Fixed in v3.0.0 - uses safe ASCII pipe (`|`) character for internal storage.

**Solution:** No action needed - should work correctly now.

---

## File Format (`.ezmap`)

Map files are JSON-based with the following structure:

```json
{
  "MapName": "My Custom Mapping",
  "Description": "Maps customer data to EasyAF format",
  "DateModified": "2025-01-17T10:30:00Z",
  "SoftwareVersion": "0.1.0",
  "MapVersion": "1.0",
  "ReferencedFiles": [
    { "FilePath": "C:\\Data\\BusData.csv" },
    { "FilePath": "C:\\Data\\Electrical.xlsx" }
  ],
  "TableReferences": {
    "Bus": "BusData.csv | BusData",
    "LVCB": "Electrical.xlsx | LVCBData"
  },
  "ImportMap": [
    {
      "TargetType": "Bus",
      "PropertyName": "Name",
      "ColumnHeader": "BUS_NAME"
    },
    {
      "TargetType": "Bus",
      "PropertyName": "NominalVoltage",
      "ColumnHeader": "Voltage"
    }
  ]
}
```

**Compatibility:** `.ezmap` files can be loaded by:
- EasyAF Map Editor (this module)
- EasyAF Import Module (uses `ImportMap` array, ignores metadata)

---

## Glossary

| Term | Definition |
|------|------------|
| **Data Type** | A category of data (Bus, LVCB, Cable, etc.) corresponding to EasyAF object types |
| **Source Column** | A column header from your CSV/Excel file (e.g., `"BUS_NAME"`) |
| **Target Property** | A property in EasyAF's data model (e.g., `Name`, `Voltage`) |
| **Mapping** | An association between a source column and target property |
| **Required Property** | A property that must be mapped for successful import |
| **DisplayName** | Internal identifier for table references (format: `"FileName | TableName"`) |
| **Auto-Map** | Intelligent algorithm that matches columns to properties based on name similarity |
| **Fuzzy Matching** | Algorithm that finds similar strings even with spelling differences |

---

## Support & Feedback

**Questions?** Check the EasyAF documentation or contact support.

**Found a bug?** Report it via GitHub Issues: https://github.com/freedomfractions/EasyAFv3/issues

**Feature requests?** We'd love to hear your ideas for improving the Map Editor!

---

**Happy Mapping!** ???

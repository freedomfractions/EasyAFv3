# Map Editor Introduction

## What is the Map Editor?

The **Map Editor** helps you create column-to-property mapping configurations for importing data into EasyAF. Think of it as a "translation table" that tells the import engine how to map columns from your spreadsheets (CSV/Excel) to EasyAF's internal data structure.

## Key Features

- **Visual Interface**: Drag-and-drop mapping instead of manual JSON editing
- **Auto-Map**: Intelligent fuzzy matching (60% confidence threshold)
- **Property Management**: Show/hide properties per data type
- **Table Persistence**: Remembers your table selections
- **Multi-File Support**: CSV and Excel (single/multi-sheet)
- **Validation**: Warns about unmapped required properties

## Quick Start

### 1. Create a New Map

**File ? New ? Mapping Configuration** (`.ezmap` file)

You'll see:
- **Summary Tab**: Overview and file management
- **Data Type Tabs**: Bus, LVCB, Fuse, Cable, etc. (configurable)

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
1. Click **Auto-Map** button in ribbon
2. Review results dialog showing confidence scores
3. Low-confidence matches (40-59%) are not auto-mapped

### 5. Save Your Map

**File ? Save** (`.ezmap` file)

Your mappings, table selections, and settings are all saved together.

## What Gets Saved?

- All column-to-property mappings
- Table selections for each data type
- Referenced file paths
- Property visibility settings
- Metadata (name, description, modified date)

## Next Steps

- Learn about [Creating Mappings](map.mapping) in detail
- Understand [Property Management](map.properties)
- Read [Troubleshooting](map.troubleshooting) tips

# Creating Mappings

## Overview

Mappings connect columns from your source files to properties in EasyAF's data model. Each mapping tells the import engine: "When I see column X in the file, put that value into property Y."

## Manual Mapping

### Using the Map Button

1. **Select a source column** from the left list (Source Columns)
2. **Select a target property** from the right list (Target Properties)
3. **Click the "Map ?" button** in the center

The property will now show a checkmark (?) and the column will display which property it's mapped to.

### Drag-and-Drop

1. **Click and hold** on a source column
2. **Drag** to a target property
3. **Release** to create the mapping

Faster than clicking the button for quick mappings!

## Auto-Map (Intelligent Matching)

Click the **Auto-Map** button in the ribbon to automatically match columns to properties.

### How It Works

Auto-Map uses **fuzzy string matching** to find similar names:
- Compares column names to property names
- Uses Levenshtein + Jaro-Winkler algorithms
- Scores each match from 0% (no similarity) to 100% (exact match)

### Confidence Threshold

- **?60% confidence**: Mapping created automatically ?
- **40-59% confidence**: Shown in dialog but NOT mapped ??
- **<40% confidence**: Not shown (no similarity)

### Example Matches

| Column Name | Property Name | Score | Result |
|-------------|---------------|-------|--------|
| `BUS_NAME` | `Name` | 80% | ? Auto-mapped |
| `Nominal_Voltage` | `NominalVoltage` | 92% | ? Auto-mapped |
| `ID` | `Identifier` | 65% | ? Auto-mapped |
| `Util` | `UtilizationPercent` | 55% | ?? Low confidence (not mapped) |
| `Col1` | `Name` | 15% | ? No match |

### Auto-Map Results Dialog

After running Auto-Map, you'll see a summary:

- **? Successfully Mapped**: High confidence matches that were created
- **?? Low Confidence**: Possible matches below 60% threshold (review manually)
- **? No Match**: Properties with no similar column names

**Tip:** Review low-confidence matches - they might be correct but need manual confirmation.

## Updating Mappings

### Replace an Existing Mapping

If you try to map a column/property that's already mapped:
1. Dialog appears: "Column X is already mapped to property Y. Replace?"
2. Click **Yes** to replace, **No** to cancel

### Unmapping

To remove a mapping:
1. Select the **target property** (right list)
2. Click the **Unmap** button
3. Both column and property indicators clear

## Mapping Indicators

- **Property list**: ? = mapped, - = unmapped
- **Column list**: Shows "DataType.PropertyName" when mapped
- **Tab icons**: ? unmapped, ? partial, ? complete

## Required Properties

Properties marked as **Required** must be mapped for successful import.

When you save, validation checks for unmapped required properties:
- If found: Warning dialog with list of missing mappings
- You can choose to:
  - **Go back and map them** (recommended)
  - **Save anyway** (import may fail later)

## Best Practices

### 1. Use Descriptive Column Headers

Auto-Map works best when column names resemble property names:

**Good:**
- `BusName`, `Bus_Name`, `BUS NAME` ? matches `Name` property
- `Voltage_Nom`, `NominalVoltage` ? matches `NominalVoltage`

**Poor:**
- `Col1`, `Data`, `Value` ? too generic, no matches

### 2. Start with Auto-Map

1. Run Auto-Map first to handle obvious matches
2. Review low-confidence suggestions
3. Manually map remaining properties

### 3. Map Required Properties First

Focus on required properties before optional ones. Use the **Validate** button to check what's missing.

### 4. Verify Data Types

Make sure column data matches property data type:
- `Voltage` property expects numbers, not text
- `Name` property expects text, not dates

(Future versions will show data type warnings)

## Next Steps

- Learn about [Property Management](map.properties)
- Read [Troubleshooting](map.troubleshooting) tips
- Explore [Advanced Features](map.advanced)

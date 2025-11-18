# Troubleshooting

## Table Selection Not Persisting

### Problem
Table dropdown resets to blank when reopening a saved map.

### Possible Causes
1. Referenced file was moved, renamed, or deleted
2. File path changed (e.g., drive letter changed from `C:` to `D:`)
3. Network drive not accessible

### Solutions

**1. Check if file still exists**
- Open Windows Explorer
- Navigate to the original file path
- If missing, use **Missing Files Dialog** to relocate

**2. Use Missing Files Dialog**
When you open a map with missing files:
1. Dialog appears listing missing files
2. Click **Browse** next to each file
3. Navigate to the new location
4. Click **OK**

Table selections will restore once files are found.

**3. Remove and re-add files**
If files moved permanently:
1. Remove old file reference (Summary tab)
2. Add file from new location
3. Select table again
4. Re-create mappings if needed

## Auto-Map Not Finding Matches

### Problem
Auto-Map reports "No match found" for properties with obvious column equivalents.

### Possible Causes
1. Column names too different from property names (<40% similarity)
2. Special characters or formatting differences
3. Typos in column headers
4. Abbreviated column names

### Solutions

**1. Rename columns in source files**

Make column names closer to property names:

**Before:**
```
Col1, Col2, Col3
```

**After:**
```
BusName, Voltage, Id
```

Auto-Map success rate increases dramatically!

**2. Check for typos**

Common mistakes:
- Extra spaces: `"Bus Name"` vs `"BusName"`
- Underscores: `"Bus_Name"` vs `"BusName"`
- Case differences (shouldn't matter, but verify)

**3. Use manual mapping**

For unique column names, manual mapping is faster than trying to make Auto-Map work.

**4. Review low-confidence matches**

Check the "Low Confidence" section in Auto-Map results - the match might be correct but below 60% threshold.

## Mappings Disappearing After Settings Change

### Problem
Mappings to certain properties vanish after changing property visibility.

### Cause
You hid a property that had an existing mapping.

### Solution

**Prevention:**
1. Before hiding properties, check mapping count
2. If properties are mapped, note which ones
3. Only hide unmapped properties

**Recovery:**
1. Re-enable the property in **Manage Fields**
2. Re-create the mapping

**Tip:** There's no undo for this - be careful when hiding properties!

## "Required Properties Not Mapped" Warning

### Problem
Warning dialog appears when saving, listing unmapped required properties.

### Cause
Some properties marked as `Required` don't have column mappings.

### Solutions

**1. Map the required properties (recommended)**
- Click **Cancel** on save dialog
- Go back to mapping tab
- Map each required property
- Try saving again

**2. Save anyway (risky)**
- Click **Save Anyway**
- Import may fail later if required data is missing
- Only do this if you plan to complete mappings later

**3. Check data type requirements**
- Some data types need specific properties
- Bus might require: `Id`, `Name`, `Voltage`
- Verify what's actually required for your import

### Why Are Properties Required?

Properties are marked `Required` by the data model definition. This ensures critical data is always present for calculations and reports.

Example:
- Bus must have `Voltage` (needed for calculations)
- Equipment must have `Id` (needed for uniqueness)

## Unicode Characters in Filenames

### Problem (Fixed in v3.0.0)
Files with non-ASCII names (Chinese, Arabic, emoji) cause table selection issues.

### Status
? **Fixed** - Map Editor now uses safe ASCII pipe (`|`) character internally.

### Solution
No action needed - should work correctly in v3.0.0+.

If you still encounter issues:
1. Verify you're running v3.0.0 or later
2. Re-save the map file with the new version
3. Report the issue if problem persists

## File Format Errors

### Problem
Error when opening a `.ezmap` file: "Invalid JSON format" or "Map file corrupted".

### Causes
1. File edited manually with syntax errors
2. File saved incompletely (disk full, crash during save)
3. File corrupted by other software

### Solutions

**1. Restore from backup**
Windows keeps file history in some cases:
1. Right-click file ? Properties ? Previous Versions
2. Select a working version
3. Click **Restore**

**2. Check JSON syntax**
Open file in text editor (Notepad++, VS Code):
- Check for missing commas, brackets, quotes
- Use a JSON validator online
- Fix syntax errors carefully

**3. Start fresh**
If file is severely corrupted:
1. Create new map file
2. Add sample files again
3. Re-create mappings (tedious but reliable)

**Tip:** Save frequently to minimize loss!

## Performance Issues

### Problem
Large files (thousands of columns) cause slow UI or high memory usage.

### Causes
1. Too many columns in source file
2. Too many properties enabled
3. Memory-intensive filtering/searching

### Solutions

**1. Use smaller sample files**
You don't need the entire dataset - just representative columns:
- First 100 rows of data is usually enough
- Extract just the columns you need

**2. Hide unused properties**
Use **Manage Fields** to hide properties you won't map. This reduces:
- Memory usage
- UI rendering time
- Search/filter overhead

**3. Close other documents**
If multiple maps are open, close ones you're not actively working on.

**4. Restart application**
If memory usage grows over time, restart EasyAF to clear cache.

## Dialog Doesn't Close

### Problem
Help or Settings dialog remains open after clicking Close/OK.

### Cause (Fixed in v3.0.0)
Dialog close logic wasn't wired correctly in earlier versions.

### Solution
? **Fixed** in v3.0.0 - dialogs now close properly.

If you encounter this:
1. Verify you're on v3.0.0+
2. Click the window X button as workaround
3. Report bug if problem persists

## Module Not Loading

### Problem
Map Editor menu option missing or `.ezmap` files can't be opened.

### Causes
1. Map module not installed
2. Module DLL not in correct folder
3. Module initialization failed

### Solutions

**1. Check module folder**
Verify file exists:
```
<EasyAF Install>\Modules\EasyAF.Modules.Map.dll
```

**2. Check logs**
Open logs folder (Help ? Open Logs Folder):
- Look for module loading errors
- Check for missing dependencies

**3. Reinstall application**
If module is corrupted, reinstall EasyAF to restore default modules.

## Getting More Help

### Log Files
Always check logs first:
1. **Help ? Open Logs Folder**
2. Open most recent log file
3. Look for ERROR or WARNING messages

### Report Issues
If problem persists:
1. Collect log files
2. Note steps to reproduce
3. Submit GitHub issue: https://github.com/freedomfractions/EasyAFv3/issues

Include:
- EasyAF version
- Operating system
- Error message (exact text)
- Steps to reproduce
- Sample files (if possible)

## Next Steps

- Review [Creating Mappings](map.mapping)
- Learn about [Property Management](map.properties)
- Read [Advanced Features](map.advanced)

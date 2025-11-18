# Property Management

## Overview

The **Manage Fields** feature lets you control which properties are visible in the mapping interface. This helps focus on relevant properties for your workflow.

## Opening Property Selector

**Data Type Tab ? Manage Fields button** (in ribbon or on tab)

The Property Selector dialog shows all available properties with checkboxes.

## How It Works

### Checkbox States

- **Checked (?)**: Property is **visible** in mapping list
- **Unchecked ( )**: Property is **hidden** from mapping list

Changes take effect immediately when you click **OK**.

### Default State

By default, all properties are visible (wildcard mode: `"*"`).

## Hiding Properties

To hide a property:
1. **Uncheck** the property in the dialog
2. Click **OK**

The property disappears from the target properties list.

### ?? Important Warning

**If you hide a property that's already mapped, the mapping will be DELETED!**

Before hiding properties:
1. Check if any are mapped (look for ? indicators)
2. If you hide a mapped property:
   - Mapping is removed from the document
   - Source column indicator clears
   - **Cannot be undone** without remapping

## Showing Properties

To show a previously hidden property:
1. Open **Manage Fields** dialog
2. **Check** the property
3. Click **OK**

The property reappears in the target properties list.

## Use Cases

### Focus on Core Properties

Hide advanced/rarely-used properties:
```
? Name
? Voltage
? Id
  Description          (hidden)
  Manufacturer         (hidden)
  InstallationDate     (hidden)
```

### Workflow-Specific Views

Different teams may need different property sets:

**Electrical Team:**
```
? Voltage
? Current
? Impedance
  Manufacturer         (hidden)
  Location             (hidden)
```

**Asset Management Team:**
```
? Manufacturer
? InstallationDate
? Location
  Voltage              (hidden)
  Impedance            (hidden)
```

### Clean Up Legacy Properties

Hide deprecated properties that are no longer used:
```
? Name
? Voltage
  OldFieldName         (hidden - deprecated)
  LegacyId             (hidden - deprecated)
```

## Per-Data-Type Settings

Property visibility is **per data type**:
- Hiding `Name` in Bus doesn't affect LVCB
- Each data type has independent settings
- Settings are saved with the application (not per map file)

## Settings Persistence

Property visibility settings are saved to:
```
%AppData%\EasyAF\settings.json
```

Settings persist across:
- ? Application restarts
- ? Map file changes
- ? Different mapping sessions

## Resetting to Defaults

To show all properties again:
1. Open **Manage Fields**
2. Click **Select All** (if available)
3. OR manually check all properties
4. Click **OK**

(Future version may include "Reset to Defaults" button)

## Search and Filter

The Property Selector dialog may include search in future versions:
- Filter by property name
- Filter by category
- Search descriptions

## Tips

### 1. Start with All Visible

When creating your first map, leave all properties visible. Hide them later once you know which ones you need.

### 2. Document Your Choices

Keep notes on why you hid certain properties (especially if working in a team).

### 3. Review Before Hiding

Check the mapping count at the top of the tab:
- "Mapped: 5 / Available: 20"
- If you hide 10 properties, Available drops to 10
- If any of those 10 were mapped, those mappings are deleted

### 4. Use Descriptive Names

When defining custom properties, use clear names so it's obvious which ones to show/hide.

## Related Features

- **Property Descriptions**: Hover tooltips show what each property is for
- **Required Property Indicators**: Required properties may have special icons (future)
- **Category Grouping**: Group related properties together (future)

## Next Steps

- Learn about [Advanced Features](map.advanced)
- Read [Troubleshooting](map.troubleshooting) tips

# Phase 5: Spec Module - Complete Specification

## Overview

The **Spec Module** enables users to create and edit `.ezspec` files that define table specifications for reports and labels. This module defines how data flows OUT of EasyAF projects into Word document templates.

**File Extension:** `.ezspec`  
**Module Name:** "Spec Editor"  
**Version:** 3.0.0

---

## Architecture Foundation

### Data Model Integration

**Leverages existing `EasyAF.Engine` classes:**
- `SpecFileRoot` - Root JSON structure
- `TableSpec` - Table definition
- `ColumnSpec` - Column definition  
- `TableFormattingSpec` - Table formatting options
- `CellConditionSpec` / `RowConditionSpec` - Conditional formatting
- `FilterSpec` / `SortSpec` - Advanced table features
- `PropertyMappingSpec` - Project metadata mappings

**All JSON serialization/deserialization handled by `SpecLoader.cs` (already exists in EasyAF.Engine)**

---

## Module Structure

```
modules/EasyAF.Modules.Spec/
??? SpecModule.cs                          # IDocumentModule implementation
??? Models/
?   ??? SpecDocument.cs                    # Wraps SpecFileRoot
?   ??? TableEditorState.cs                # UI state for table editor
??? ViewModels/
?   ??? SpecDocumentViewModel.cs           # Document coordinator
?   ??? SpecSetupViewModel.cs              # Setup tab VM
?   ??? TableEditorViewModel.cs            # Table tab VM
?   ??? DataTypePickerViewModel.cs         # Multi-select dropdown VM
?   ??? ColumnEditorViewModel.cs           # Column properties editor VM
?   ??? ExpressionBuilderViewModel.cs      # Expression builder dialog VM
?   ??? ConditionalFormatViewModel.cs      # Conditional formatting VM
??? Views/
?   ??? SpecDocumentView.xaml              # TabControl host
?   ??? SpecSetupView.xaml                 # Setup tab UI
?   ??? TableEditorView.xaml               # Table editor tab UI
?   ??? Controls/
?       ??? DataTypePickerControl.xaml     # Advanced multi-select dropdown
?       ??? PropertyPaletteControl.xaml    # Right column property menu
?       ??? TableCanvasControl.xaml        # WYSIWYG table preview
?       ??? ColumnEditorDialog.xaml        # Column properties dialog
?       ??? ExpressionBuilderDialog.xaml   # Expression builder
?       ??? ConditionalFormatDialog.xaml   # Conditional formatting editor
??? Services/
?   ??? PropertyDiscoveryService.cs        # Filter properties by data type
?   ??? MapValidationService.cs            # Validate spec against map
??? Converters/
    ??? TableModeToStringConverter.cs      # Label/Report display
    ??? ColumnTypeToGlyphConverter.cs      # Column type icons
```

---

## UI Design - Tab 1: Setup Tab

### Layout Pattern
**Same two-column pattern as Map/Project modules for consistency**

**Left Column (60%):**
- Table Definitions GroupBox
- Statistics GroupBox

**Right Column (40%):**
- Map Validation GroupBox

---

### Section 1: Table Definitions (Top Left)

**GroupBox: "Tables"**

**DataGrid Columns:**
1. **Table Name** (TextBox, 200px)
   - Maps to Word template AltText
   - Required, unique validation

2. **Data Types** (Custom Dropdown, 300px)
   - Multi-select with fuzzy search
   - Shows "3 of 34 selected" when closed
   - Click to open advanced picker

3. **Columns** (Read-only, 80px)
   - Calculated from column list

4. **Mode** (ComboBox, 100px)
   - Options: "Label", "Report"
   - Affects dimension defaults

**Buttons (below grid):**
- **Add Table** - Adds new row with defaults
- **Remove Table** - Deletes selected (with confirmation)
- **Move Up / Move Down** - Reorder tables

---

### Custom Data Type Picker Control

**Popup/Flyout design (similar to Map module property visibility):**

```
????????????????????????????????????????
? Select Data Types                    ?
????????????????????????????????????????
? ?? Search: [fuzzy filter___________] ?
????????????????????????????????????????
? ? ArcFlash                           ?
? ? Bus                                ?
? ? Cable                              ?
? ? Capacitor                          ?
? ... (scroll, 34 types total)         ?
????????????????????????????????????????
? Selected: 1 of 34                    ?
????????????????????????????????????????
?           [OK]    [Cancel]           ?
????????????????????????????????????????
```

**Behavior:**
- Search filters visible items (doesn't affect checked state)
- OK applies selection and closes
- Cancel reverts changes and closes
- Display shows count when closed (e.g., "1 of 34 selected")

---

### Section 2: Statistics (Bottom Left)

**GroupBox: "Table Statistics"**

**TreeView/ListView showing:**
- **Per-table breakdown:**
  - Table Name
  - Column count
  - Unique properties used
  - Data types referenced
- **Overall summary:**
  - Total tables
  - Total columns
  - Most-used properties

---

### Section 3: Map Validation (Right Column)

**GroupBox: "Map Validation"**

**Controls:**
- **Map File:** ComboBox (dropdown list of available .ezmap files) + Browse button
- **Validate Button** (accent style)
- **Results Panel:** DataGrid or ListBox

**Validation Results Display:**
```
????????????????????????????????????????
? ? Bus.Buses - Mapped                ?
? ? Bus.BaseKV - Mapped               ?
? ??  ArcFlash.AFBoundary - Missing   ?
? ? LVBreaker.FrameA - Error: Type   ?
?     mismatch                         ?
????????????????????????????????????????
```

**Color Coding:**
- ? Green: Property is mapped
- ?? Yellow: Property not mapped (warning)
- ? Red: Validation error (type mismatch, etc.)

---

## UI Design - Tab 2+: Table Editor Tabs (WYSIWYG)

### Two-Row Layout

**Row 1: Table Properties (Auto height)**  
**Row 2: WYSIWYG Table Canvas (*)**

---

### Row 1: Table Properties Panel

**GroupBox: "Table Settings"**

**3-Column Grid Layout:**

**Column 1: Dimension & Mode**
- **Dimension Mode:** RadioButtons
  - ? Label (dimensioned)
  - ? Report (full width)
  - ? Custom (W × H input boxes)
- **Table Mode:** ComboBox
  - Options: "New Data", "Diff"

**Column 2: Formatting**
- **Row Banding:** CheckBox + Color picker
- **Border Style:** ComboBox (None, Simple, Grid, Professional)
- **Header Fill:** Color picker
- **Alternate Row Fill:** Color picker

**Column 3: Typography**
- **Font Family:** ComboBox (Arial, Calibri, Times, etc.)
- **Font Size:** NumericUpDown (8-72 pt)
- **Horizontal Align:** ComboBox (Left, Center, Right)
- **Vertical Align:** ComboBox (Top, Middle, Bottom)

---

### Row 2: WYSIWYG Table Canvas (Two-Column Split)

**Left Column (70% width): Table Preview Canvas**

**WYSIWYG Table Canvas Control:**

```
????????????????????????????????????????????????????????
? Ruler: |--20%--|--15%--|--12%--|-----53%-----|       ?
????????????????????????????????????????????????????????
? Bus ID        ? Voltage  ? Margin ? Status         ? ? Header
??????????????????????????????????????????????????????
? SWGR-1A       ? 480V     ? 25.5%  ? Active         ? ? Row 1 (white)
??????????????????????????????????????????????????????
? SWGR-1A       ? 480V     ? 18.2%  ? Active         ? ? Row 2 (banded)
?               ?          ?        ?                ?   Merged cells
??????????????????????????????????????????????????????
? SWGR-2B       ? 208V     ? 12.1%  ? Inactive       ? ? Row 3 (white)
??????????????????????????????????????????????????????
? SWGR-3C       ? 4160V    ? 8.7%   ? Active         ? ? Row 4 (banded)
??????????????????????????????????????????????????????
```

**Features:**
- Visual ruler showing column widths (% or inches)
- Header row with styled background
- 4 dummy data rows (for reports) demonstrating:
  - Row banding (alternating colors)
  - Merged cells (identical adjacent values)
  - Conditional formatting (applied colors)
- Interactive column selection (click to select)
- Drag column borders to resize (visual feedback)

---

**Right Column (30% width, fixed): Two panels**

**Panel 1 (Top 60%): Property Palette**
```
????????????????????????????????????????
? ?? Search: [_______________]         ?
????????????????????????????????????????
? Available Properties:                 ?
? (filtered by selected data types)    ?
?                                       ?
? ?? Bus (expand/collapse)             ?
?   ?? ?? Buses                        ?
?   ?? ?? BaseKV                       ?
?   ?? ?? NoOfPhases                   ?
? ?? ArcFlash                          ?
?   ?? ?? Buses                        ?
?   ?? ... (more properties)           ?
????????????????????????????????????????
```

**Interaction:**
- Double-click property ? Adds column with `{DataType.PropertyName}`
- Drag property ? Drop onto column to change source
- Context menu ? Copy property path to clipboard

**Panel 2 (Bottom 40%): Quick Actions**
```
????????????????????????????????????????
? Column Actions:                       ?
? [Add Property Column]                 ?
? [Add Literal Column]                  ?
? [Add Expression Column]               ?
? [Add Format Column]                   ?
????????????????????????????????????????
? Selected Column:                      ?
? Header: "Bus ID"                      ?
? [Edit Properties...]                  ?
? [Add Condition...]                    ?
? [Remove Column]                       ?
? [Move Left] [Move Right]              ?
????????????????????????????????????????
```

---

## Column Editor Dialog

**Modal Dialog for editing individual columns**

### Tab 1: Basic Properties

**Column Header:** TextBox (required)  
**Column Type:** RadioButtons
- ? Property Path (single property)
- ? Multiple Properties (concatenated with separator)
- ? Format String (composite with tokens)
- ? Expression (numeric calculation)
- ? Literal (static text)

**Based on selected type:**

**If Property Path:**
- Property: ComboBox (filtered by data types)
- Units: TextBox (optional suffix, e.g., "kV", "A")

**If Multiple Properties:**
- Properties: CheckedListBox (select multiple)
- Separator: TextBox (default: newline)

**If Format String:**
- Format: TextBox with token picker button
- Example: `{Bus.Buses} - {Bus.BaseKV} kV`
- **Insert Token** button ? opens property picker

**If Expression:**
- Expression: TextBox with helper button
- Opens **Expression Builder Dialog**
- Number Format: TextBox (e.g., `0.##`, `#,##0.00`)

**If Literal:**
- Text: TextBox (static content)

---

### Tab 2: Formatting

**Column Width:**
- ? Auto (distributes remaining space)
- ? Fixed Percentage: NumericUpDown (1-100%)

**Font Overrides:**
- ? Use table defaults
- Font Family: ComboBox
- Font Size: NumericUpDown
- Bold / Italic: CheckBoxes

**Alignment:**
- Horizontal: ComboBox (Left, Center, Right)
- Vertical: ComboBox (Top, Middle, Bottom)

**Special Options:**
- ? Merge Vertically (combines identical adjacent cells)
- ? Remove Duplicate Lines (within cell)

---

### Tab 3: Conditional Formatting

**Cell Conditions List:**

| Condition | Fill | Text Color | Actions |
|-----------|------|------------|---------|
| Value > 100 | Yellow | Red | ?? ??? |
| Contains "Error" | Red | White | ?? ??? |

**Buttons:**
- **Add Condition** - Opens Conditional Format Dialog
- **Edit Condition** - Edit selected condition
- **Remove Condition** - Delete condition

---

## Expression Builder Dialog

**Purpose:** Help users create numeric expressions visually

**Layout:**

**Left Panel: Property Picker**
```
Available Properties:
? Bus.BaseKV
? Bus.BusBracing
? LVBreaker.FrameA
? LVBreaker.TripA
... (filtered by data types)
```

**Center Panel: Expression Builder**
```
Expression:
????????????????????????????????????????
? ({LVBreaker.FrameA} - {LVBreaker.    ?
?  TripA}) / {LVBreaker.FrameA} * 100  ?
????????????????????????????????????????

Operators:
[+] [-] [*] [/] [(] [)] [Clear]

Insert Property: [dropdown ?]
```

**Right Panel: Preview**
```
Result (sample data):
????????????????????????????????????????
? Sample 1: 25.5                        ?
? Sample 2: 18.2                        ?
? Sample 3: ERROR (div by zero)        ?
????????????????????????????????????????

Number Format: [0.##_______] [?]
Formatted:
????????????????????????????????????????
? Sample 1: 25.5                        ?
? Sample 2: 18.2                        ?
? Sample 3: ERROR                       ?
????????????????????????????????????????
```

**Buttons:**
- [OK] - Apply expression
- [Cancel] - Discard changes
- [Test Expression] - Validate syntax

---

## Conditional Format Dialog

**Purpose:** Define cell/row conditional formatting rules

**Condition Type:** RadioButtons
- ? Cell Condition (applies to this column only)
- ? Row Condition (applies to entire row)

**Property:** ComboBox (or use rendered cell value)  
- Option: "Use rendered value" (matches cell text)
- Option: "Use property: {dropdown}"

**Operator:** ComboBox
- Equal to
- Not equal to
- Less than
- Less than or equal
- Greater than
- Greater than or equal
- Contains
- Starts with
- Ends with

**Value:** TextBox (or property path)
- Option: "Fixed value: {textbox}"
- Option: "Compare to property: {dropdown}"

**Numeric Comparison:** CheckBox  
**Case Insensitive:** CheckBox

**Format to Apply:**

**Target:** ComboBox
- Cell Background
- Cell Text Color
- Row Background
- Row Text Color

**Fill Color:** Color picker  
**Text Color:** Color picker  
**Bold / Italic:** CheckBoxes

**Preview:**
```
????????????????????????????????????????
? Sample Cell Text                      ? ? Shows formatting
????????????????????????????????????????
```

---

## Map Validation Service

**Purpose:** Validate that all PropertyPaths used in the spec are mapped in the selected map file

**Cross-Module Integration:**
```csharp
// CROSS-MODULE EDIT: 2025-01-28 Task#28
// Modified for: Spec module map validation
// Related modules: EasyAF.Modules.Map
// Rollback instructions: Remove GetMappedProperties() method from MapModule
```

**Map Module Helper Method:**
```csharp
public class MapModule : IDocumentModule
{
    // CROSS-MODULE EDIT: Added for Spec module validation
    public List<string> GetMappedProperties(string mapFilePath)
    {
        var config = MappingConfig.LoadFromFile(mapFilePath);
        return config.TableMappings
            .SelectMany(tm => tm.FieldMappings.Values)
            .Distinct()
            .ToList();
    }
}
```

**Spec Module Service:**
```csharp
public interface IMapValidationService
{
    MapValidationResult ValidateSpec(SpecFileRoot spec, string mapFilePath);
}

public class MapValidationResult
{
    public List<ValidationEntry> Results { get; set; }
    public int MappedCount { get; set; }
    public int UnmappedCount { get; set; }
    public int ErrorCount { get; set; }
}

public class ValidationEntry
{
    public string PropertyPath { get; set; }    // e.g., "Bus.BaseKV"
    public ValidationStatus Status { get; set; } // Mapped, Missing, Error
    public string? Message { get; set; }         // Details
}

public enum ValidationStatus { Mapped, Missing, Error }
```

---

## Phase 5 Task Breakdown

### Task 24: Create Spec Module Structure ? **IN PROGRESS**
**Deliverables:**
- Create `EasyAF.Modules.Spec` project (WPF, .NET 8)
- Add references: `EasyAF.Core`, `EasyAF.Engine`, `EasyAF.Data`
- Implement `SpecModule.cs` (IDocumentModule)
- Create `SpecDocument.cs` (wraps `SpecFileRoot`)
- Create folder structure (ViewModels/, Views/, Models/, Services/, Controls/, Converters/)
- Register `.ezspec` extension

**Acceptance Criteria:**
- ? Module compiles with zero errors
- ? Can create new .ezspec document
- ? Can save/load .ezspec files (via SpecLoader)
- ? Module isolation maintained

---

### Task 25: Build Setup Tab
**Deliverables:**
- Create `SpecDocumentViewModel.cs` (coordinator, same pattern as Map/Project)
- Create `SpecSetupViewModel.cs` (table grid + statistics)
- Create `SpecSetupView.xaml` (two-column layout matching Map/Project style)
- Implement `DataTypePickerControl.xaml` (fuzzy search + multi-select checkboxes)
- Implement statistics panel (TreeView showing table breakdown)
- Implement map validation panel (dropdown + validate button + results grid)

**Acceptance Criteria:**
- ? Can add/remove tables
- ? Can select data types (fuzzy search works)
- ? Can set table names and modes
- ? Statistics update in real-time
- ? Can select map file for validation
- ? Theme compliance (DynamicResource bindings)
- ? MVVM strict (zero code-behind)

---

### Task 26: Build Table Editor Tab (WYSIWYG)
**Deliverables:**
- Create `TableEditorViewModel.cs` (per-table editor)
- Create `TableEditorView.xaml` (two-row layout)
- Implement table properties panel (dimension mode, formatting)
- Implement **TableCanvasControl.xaml** (WYSIWYG preview with ruler)
- Implement `PropertyPaletteControl.xaml` (right panel, 30% width)
- Implement `ColumnEditorDialog.xaml` (modal dialog for column properties)
- Implement `ExpressionBuilderDialog.xaml`
- Implement `ConditionalFormatDialog.xaml`
- Add/Edit/Remove/Reorder column commands

**Acceptance Criteria:**
- ? WYSIWYG table preview shows ruler with column widths
- ? Dummy data rows demonstrate banding and merging
- ? Can add/remove/reorder columns
- ? Can edit column properties (header, source, width)
- ? Property palette filters by selected data types
- ? Expression builder validates syntax
- ? Conditional formatting rules apply visually
- ? Table settings persist on save
- ? Theme compliance
- ? MVVM strict

---

### Task 27: Implement Spec Ribbon Commands
**Deliverables:**
- Create ribbon tab: "Table Management"
  - Commands: Add Table, Remove Table, Duplicate Table
- Create ribbon tab: "Column Operations"
  - Commands: Add Column, Edit Column, Remove Column
  - Commands: Move Left, Move Right, Auto-Size
- Create ribbon tab: "Validation"
  - Commands: Validate Map, Validate Spec, Export Summary

**Acceptance Criteria:**
- ? All commands wired through ViewModel
- ? Commands enable/disable appropriately
- ? Keyboard shortcuts work (Ctrl+N for new table, etc.)

---

### Task 28: Add Spec Validation & Polish
**Deliverables:**
- Implement `MapValidationService.cs`
  - Validates PropertyPaths against map file
  - Returns color-coded results
- Implement column uniqueness validation (warn on duplicate headers)
- Implement required field validation (table name, AltText)
- Polish expression builder (syntax highlighting?)
- Polish conditional format dialog (preview rendering)
- Create validation results panel (DataGrid with status icons)

**Cross-Module Edit Required:**
- Add helper method to Map module: `GetMappedProperties()` returns `List<string>`
- Document with full rollback instructions

**Acceptance Criteria:**
- ? Map validation detects missing mappings
- ? Validation results color-coded correctly
- ? Duplicate column headers show warning
- ? Required fields validated on save
- ? Expression syntax validated
- ? Conditional formatting previews correctly

---

## Success Criteria

**Phase 5 Complete When:**
- ? Can create/edit/save .ezspec files
- ? Setup tab manages tables with data type selection
- ? WYSIWYG table editor shows visual preview
- ? Table editor supports columns (property, format, expression, literal)
- ? Expression builder creates valid expressions
- ? Conditional formatting rules apply visually
- ? Map validation detects unmapped properties
- ? Statistics show field usage
- ? All ribbon commands functional
- ? Theme-compliant (Light/Dark work)
- ? MVVM strict (zero code-behind)
- ? Round-trip testing (save/load verified)

**Deferred to Phase 5b:**
?? VSTO Word Add-in (template creation tool)

---

## Design Consistency Checklist

? Same DocumentViewModel coordinator pattern (Map/Project proven)  
? Same two-column layout for Setup tab (matches Map/Project)  
? TabHeaderInfo class for tab metadata  
? IDisposable implementation  
? ViewModel property on Document model  
? GroupBox for logical sections  
? DynamicResource theme bindings (100%)  
? CommonControls.xaml styles  
? Settings integration (default paths for specs)  
? Help system integration (IHelpProvider)  
? Dirty tracking and save prompts  

---

## WYSIWYG Implementation Notes

**Dummy Data Generation Strategy:**

For **Reports** (4 rows to demonstrate banding/merging):
```csharp
// Example: Bus table
Row 1: Buses="SWGR-1A", BaseKV="0.48", ...  (white bg)
Row 2: Buses="SWGR-1A", BaseKV="0.48", ...  (banded, merged Buses cell)
Row 3: Buses="SWGR-2B", BaseKV="0.208", ... (white bg)
Row 4: Buses="SWGR-3C", BaseKV="4.16", ...  (banded bg)
```

For **Labels** (all cells populated):
```csharp
// Example: Breaker label table
All cells filled with sample data matching column PropertyPaths
```

**Visual Feedback:**
- Selected column: Border highlight
- Hover states: Subtle background change
- Drag handles: Between column headers for resizing
- Ruler: Top bar showing width segments

---

End of Specification

# Phase 4 Reference Code Research

**Date:** 2025-01-20  
**Purpose:** Analyze DiffGrid and ProjectView before Phase 4 implementation

---

## ?? Reference Code Locations

### DiffGrid Control (EasyAFv2)
**Location:** `C:\src\EasyAFv3\sandbox\EasyAFv2\src\Controls\ModularUI.Controls.DiffGrid\`

**Files Found:**
- `DiffGrid.Control.cs` - Main control implementation
- `DiffGrid.State.cs` - **CRITICAL** - State management
- `DiffGrid.Selection.cs` - Selection handling
- `DiffGrid.Scrolling.cs` - Scroll position management
- `DiffGrid.Editing.cs` - Cell editing
- `DiffGrid.Filtering.cs` - Filter state
- `DiffGrid.Filters.State.cs` - Filter persistence
- `DiffGrid.Columns.cs` - Column configuration
- `DiffGrid.Clipboard.cs` - Copy/paste
- `DiffGrid.Navigation.cs` - Keyboard navigation
- `DiffGrid.Abstractions.cs` - Interfaces/base classes
- `DiffGridHostExtensions.cs` - Integration helpers

**Tests:** `C:\src\EasyAFv3\sandbox\EasyAFv2\tests\ModularUI.Controls.DiffGrid.Tests\`

### ProjectView (Stopgap)
**Location:** `C:\src\EasyAFv3\sandbox\EasyAF-stopgap\EasyAF.UI\Views\`

**Files:**
- `ProjectView.xaml` - **UI layout** (metadata GroupBox)
- `ProjectView.xaml.cs` - Code-behind (not MVVM)

---

## ?? Research Tasks

### Task 1: DiffGrid State Preservation Analysis
**File to study:** `DiffGrid.State.cs`

**Questions:**
1. How does it store selection state?
2. How does it store scroll position?
3. Does it use absolute pixels or logical positions?
4. Does it handle DPI changes?
5. What properties are exposed for binding?

### Task 2: DiffGrid Integration Requirements
**Files to study:** `DiffGridHostExtensions.cs`, `DiffGridConfigurator.cs`

**Questions:**
1. What dependencies does it have?
2. How does it integrate with MVVM?
3. Does it require special setup?
4. What events does it expose?

### Task 3: ProjectView Metadata Layout
**File to study:** `ProjectView.xaml`

**Questions:**
1. What fields are in the metadata GroupBox?
2. What's the exact layout (rows, columns)?
3. What labels are used?
4. What control types (TextBox, DatePicker, etc.)?

---

## ?? Next Steps

1. Read DiffGrid.State.cs - understand state management
2. Read DiffGrid.Scrolling.cs - understand scroll preservation
3. Read DiffGrid.Selection.cs - understand selection handling
4. Read ProjectView.xaml - document metadata layout
5. Create detailed findings document
6. Test DiffGrid in isolation
7. Document integration plan

---

**Status:** Research in progress  
**Next:** Read DiffGrid.State.cs

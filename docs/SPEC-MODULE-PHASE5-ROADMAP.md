# Spec Module - Phase 5 Development Roadmap

## ?? Project Overview

**Goal:** Build a complete Spec Editor module that generates executable `.ezspec` files for the EasyAF reporting engine.

**Reference Spec:** `sandbox/EasyAFv2/reference/EasyAF_legacy/EasyAFEngine/samples/lb_af_report_specs.ezspec`

**Current Status:** Phase 1 (MVP) complete - moving to Phase 2 (Core Report Features)

---

## ? Phase 1: MVP (COMPLETE)

### Document Management
- [x] Create new spec documents
- [x] Open existing `.ezspec` files
- [x] Save spec documents (with proper ViewModel ? DTO sync)
- [x] Dirty state tracking
- [x] Tab-based UI (Setup + dynamic table tabs)

### Table Management
- [x] Add/remove/reorder tables (Setup tab)
- [x] Edit table names (AltText)
- [x] Select table mode (new/diff)
- [x] Dynamic table editor tabs

### Column Management
- [x] Add/remove/reorder columns (Table Editor tabs)
- [x] Edit column headers
- [x] Set column width percentages
- [x] Property path picker dialog (multi-select with search)
- [x] Format string editing (basic)

### Infrastructure
- [x] Direct reference architecture (no explicit compile step needed)
- [x] SyncFromViewModel() before save
- [x] Theme-aware UI throughout
- [x] Proper MVVM (zero code-behind logic)

**Completion Date:** 2025-01-28  
**Status:** ? **100% Complete**

---

## ?? Phase 2: Core Report Features (IN PROGRESS)

**Priority:** HIGH - Without these, specs won't generate functional reports

### 2.1 FilterSpecs Editor
**Status:** ? Not Started  
**Priority:** ?? **HIGH** - Core feature used in every reference table

**Reference Structure:**
```json
"FilterSpecs": [
  { "PropertyPath": "TripUnit.Adjustable", "Operator": "eq", "Value": "true", "Numeric": false }
]
```

**Tasks:**
- [ ] Add FilterSpecs UI to Table Editor tab
- [ ] Support operators: eq, ne, gt, lt, gte, lte, contains
- [ ] Property path picker for FilterSpec.PropertyPath
- [ ] Support Numeric flag (string vs numeric comparison)
- [ ] Support RightPropertyPath (property-to-property comparison)
- [ ] Multi-filter support (AND logic)

**Acceptance Criteria:**
- [ ] Can add/remove filters per table
- [ ] Filters serialize to TableSpec.FilterSpecs correctly
- [ ] UI shows filter summary (e.g., "TripUnit.Adjustable = true")

---

### 2.2 SortSpecs Editor
**Status:** ? Not Started  
**Priority:** ?? **HIGH** - Professional reports need sorted data

**Reference Structure:**
```json
"SortSpecs": [
  { "Column": 1, "Direction": "asc", "Numeric": false },
  { "Column": 2, "Direction": "asc", "Numeric": false }
]
```

**Tasks:**
- [ ] Add SortSpecs UI to Table Editor tab
- [ ] Multi-column sort support (ordered list)
- [ ] Direction: asc/desc
- [ ] Numeric flag (string vs numeric sort)
- [ ] Reorder sort criteria (up/down buttons)

**Acceptance Criteria:**
- [ ] Can define multi-level sort (e.g., by Bus then by Id)
- [ ] Can reorder sort criteria
- [ ] Sorts serialize to TableSpec.SortSpecs correctly

---

### 2.3 Table-Level Toggles
**Status:** ? Not Started  
**Priority:** ?? **MEDIUM**

**Tasks:**
- [ ] AllowRowBreakAcrossPages (checkbox in Table Editor)
- [ ] HideIfNoDiff (checkbox, only enabled when Mode="diff")
- [ ] Wire to TableSpec properties

**Acceptance Criteria:**
- [ ] Toggles persist to JSON
- [ ] HideIfNoDiff disabled when Mode="new"

---

### 2.4 EmptyMessage/EmptyFormatting
**Status:** ? Not Started  
**Priority:** ?? **MEDIUM** - UX improvement

**Reference Structure:**
```json
"EmptyMessage": "No adjustable trip breakers.",
"EmptyFormatting": {
  "FontName": "Arial",
  "FontSize": 11,
  "HorizontalAlignment": "center",
  "Bold": true,
  "Fill": "FFFFFF",
  "TextColor": "000000"
}
```

**Tasks:**
- [ ] Add EmptyMessage TextBox to Table Editor
- [ ] Add EmptyFormatting panel (expandable section)
- [ ] Font picker (name, size, bold)
- [ ] Alignment picker (horizontal/vertical)
- [ ] Color pickers (Fill, TextColor - hex without #)

**Acceptance Criteria:**
- [ ] Empty message configurable per table
- [ ] Formatting options persist to JSON
- [ ] Preview of formatted empty message (optional)

---

## ?? Phase 3: Visual Polish (PLANNED)

**Priority:** MEDIUM - Makes reports visually meaningful

### 3.1 Conditional Formatting - Row Level
**Status:** ? Not Started

**Reference Structure:**
```json
"RowConditions": [
  {
    "PropertyPath": "DutyKA",
    "RightPropertyPath": "RatingKA",
    "Operator": "gt",
    "Numeric": true,
    "TextColor": "FF0000",
    "Target": "Text"
  }
]
```

**Tasks:**
- [ ] RowConditions editor (DataGrid in Table Editor)
- [ ] Property comparison support (PropertyPath vs RightPropertyPath)
- [ ] Color picker for TextColor/Fill
- [ ] Target selector (Text, Fill, Row)
- [ ] Multiple conditions per table

---

### 3.2 Conditional Formatting - Cell Level
**Status:** ? Not Started

**Reference Structure:**
```json
"GlobalCellConditions": [
  {
    "Operator": "contains",
    "Value": "Was",
    "MatchRendered": true,
    "Fill": "FFFF00"
  }
]
```

**Tasks:**
- [ ] GlobalCellConditions editor
- [ ] Per-column Conditions (ColumnSpec.Conditions)
- [ ] MatchRendered flag (pre-render vs post-render matching)
- [ ] Color pickers

---

### 3.3 Table Formatting (Global)
**Status:** ? Not Started

**Reference Structure:**
```json
"Formatting": {
  "FontName": "Arial",
  "FontSize": 10,
  "HorizontalAlignment": "left",
  "VerticalAlignment": "center"
}
```

**Tasks:**
- [ ] TableFormattingSpec editor (expandable panel)
- [ ] Font picker (name, size)
- [ ] Alignment pickers (horizontal/vertical)
- [ ] HeaderFill, AlternateRowFill color pickers
- [ ] RemoveDuplicateLines toggles
- [ ] FitToWindow toggle
- [ ] MergeBreakColumns editor

---

## ?? Phase 4: Advanced Features (FUTURE)

**Priority:** LOW to MEDIUM - Power user features

### 4.1 Expression/NumberFormat
**Status:** ? Not Started

**Reference:**
```json
{
  "Header": "MARGIN %",
  "Expression": "(({RatingKA}-{DutyKA})/{RatingKA})*100",
  "NumberFormat": "0.0"
}
```

**Tasks:**
- [ ] Expression editor (multi-line TextBox with syntax help)
- [ ] Token picker (property path insertion)
- [ ] NumberFormat picker (.NET format strings)
- [ ] Expression validation (optional - check syntax)

---

### 4.2 Column-Level Formatting
**Status:** ? Not Started

**Tasks:**
- [ ] Per-column FontName, FontSize
- [ ] Per-column HorizontalAlignment, VerticalAlignment
- [ ] Override global table formatting

---

### 4.3 Advanced Column Features
**Status:** ? Not Started

**Tasks:**
- [ ] MergeVertically toggle (per column)
- [ ] Literal text mode (overrides PropertyPaths/Format)
- [ ] JoinWith customization (default is \n)
- [ ] Name field (internal column identifier)

---

### 4.4 FilterGroups (Advanced)
**Status:** ? Not Started

**Reference:**
```json
"FilterGroups": [
  {
    "Logic": "OR",
    "Filters": [...]
  }
]
```

**Tasks:**
- [ ] FilterGroups UI (nested structure)
- [ ] AND/OR logic selection
- [ ] Group management (add/remove/nest)

---

## ?? Overall Progress

### By Phase

| Phase | Status | Completion | Priority |
|-------|--------|------------|----------|
| Phase 1: MVP | ? Complete | 100% | - |
| Phase 2: Core Features | ?? In Progress | 0% | ?? HIGH |
| Phase 3: Visual Polish | ?? Planned | 0% | ?? MEDIUM |
| Phase 4: Advanced | ?? Planned | 0% | ?? LOW |

### By Feature Category

| Category | Complete | In Progress | Not Started | Total |
|----------|----------|-------------|-------------|-------|
| Document Management | 4 | 0 | 0 | 4 |
| Table Management | 4 | 0 | 0 | 4 |
| Column Management | 6 | 0 | 0 | 6 |
| Filtering/Sorting | 0 | 0 | 8 | 8 |
| Formatting | 0 | 0 | 15 | 15 |
| Advanced Features | 0 | 0 | 12 | 12 |
| **TOTAL** | **14** | **0** | **35** | **49** |

**Overall Completion:** 28.6% (14/49 features)

---

## ?? Current Sprint Focus

### Sprint Goal: Complete Phase 2 Core Features

**Target Completion:** TBD

**Current Task:** 2.1 FilterSpecs Editor

**Blockers:** None

---

## ?? Notes & Decisions

### Architecture Decisions
- **Direct Reference Pattern:** ViewModels hold direct references to DTOs (TableSpec, ColumnSpec). No explicit Compile() step needed.
- **SyncFromViewModel:** Called in SpecModule.SaveDocument() to ensure ViewModel state is written to SpecFileRoot before serialization.
- **Tab Structure:** Setup tab manages tables, individual table tabs manage columns.
- **Client Profile Module (Deferred):** `.ezclient` files will define custom metadata fields for clients. A dedicated Client Profile Editor module will manage these files. Projects reference client profiles to load custom field definitions. This prevents typos and ensures validation of custom metadata. PropertyMappings in specs reference both built-in Project fields and custom fields from client profiles.

### Deferred Features
- Map validation (planned for later)
- Table statistics (data type usage counts - planned for later)
- WYSIWYG preview (out of scope for Phase 5)
- Template scraping (may integrate later)
- **Client Profile Editor Module** (separate module - deferred to future phase)
- **PropertyMappings Editor** (deferred to VSTO plugin - metadata handling is not the Spec module's responsibility)

### Deferred Modules
- **Client Profile Editor**: Single-tab interface for managing `.ezclient` files
  - Two-column table: Property Name | Default Value
  - Saved to `Documents\EasyAF\Clients\`
  - Referenced by Project module for custom metadata fields
  - Prevents typos and ensures validated custom property definitions
- **VSTO Plugin (Future)**: Will handle PropertyMappings
  - Reads Project built-in fields + custom fields from `.ezclient`
  - Writes Word document properties directly
  - Spec module only defines table structures (not metadata mappings)

### Technical Debt
- None identified yet

---

## ?? Related Documents

- [SPEC-MODULE-ISSUES-FIXES.md](SPEC-MODULE-ISSUES-FIXES.md) - Issue tracker
- [SPEC-MODULE-REMAINING-FIXES.md](SPEC-MODULE-REMAINING-FIXES.md) - Remaining work
- Reference spec: `sandbox/EasyAFv2/reference/EasyAF_legacy/EasyAFEngine/samples/lb_af_report_specs.ezspec`
- Engine DTOs: `lib/EasyAF.Engine/JsonSpec.cs`
- **Future**: Client Profile Module specification (TBD)

---

## ?? Change Log

| Date | Author | Change |
|------|--------|--------|
| 2025-01-28 | AI Assistant | Initial roadmap created from audit results |
| | | Phase 1 marked complete |
| | | Phase 2-4 planned with priorities |
| | | Added Client Profile Module architecture decision |
| | | Documented deferred Client Profile Editor module |
| | | **Removed PropertyMappings from Phase 2** (deferred to VSTO plugin) |
| | | Recalculated completion: 28.6% (14/49 features) |
| | | Updated current task: 2.1 FilterSpecs Editor |

---

**Last Updated:** 2025-01-28  
**Document Owner:** Phase 5 Development Team  
**Status:** ?? Active Development

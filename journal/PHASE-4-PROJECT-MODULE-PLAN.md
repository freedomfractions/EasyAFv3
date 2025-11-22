# Phase 4: Project Module - Implementation Plan

## ?? Overview

The Project Module enables users to create and manage `.ezproj` files that:
- Store project metadata (name, client, location, etc.)
- Track datasets for each data type (Old Data vs New Data)
- Compare old vs new using DiffGrid visualization
- Generate professional reports using loaded specs
- Support batch processing workflows

---

## ?? Tasks (From Development Prompt)

### Task 18: Create Project Module Structure ? (NEXT)
**Goal:** Create project structure and register with shell

**Deliverables:**
- [ ] `modules/EasyAF.Modules.Project/` folder structure
- [ ] `ProjectModule.cs` implementing `IDocumentModule`
- [ ] Register with shell `ModuleCatalog`
- [ ] File type: `.ezproj`
- [ ] Icon: Project folder icon

**Acceptance Criteria:**
- [ ] Module loads on shell startup
- [ ] "New Project" creates empty ProjectDocument
- [ ] Can save/load .ezproj files

---

### Task 19: Design Project Data Model
**Goal:** Create ProjectDocument class with metadata and datasets

**Deliverables:**
- [ ] `Models/ProjectDocument.cs` implementing `IDocument`
- [ ] Project metadata (name, client, location, date)
- [ ] `DataSet<T>` collections for each data type
- [ ] Change tracking for IsDirty flag
- [ ] Serialization to JSON

**Key Properties:**
```csharp
public class ProjectDocument : IDocument
{
    // Metadata
    public string ProjectName { get; set; }
    public string ClientName { get; set; }
    public string Location { get; set; }
    public DateTime DateCreated { get; set; }
    
    // Datasets (Old vs New)
    public DataSet NewData { get; set; }
    public DataSet OldData { get; set; }
    
    // Spec references
    public string? ReportSpecPath { get; set; }
    public string? LabelSpecPath { get; set; }
}
```

**Acceptance Criteria:**
- [ ] Can create new ProjectDocument
- [ ] Metadata persists to JSON
- [ ] DataSets serialize correctly
- [ ] IsDirty tracking works

---

### Task 20: Build Project Overview Tab
**Goal:** Create UI for project metadata editing

**Deliverables:**
- [ ] `ViewModels/ProjectOverviewViewModel.cs`
- [ ] `Views/ProjectOverviewView.xaml`
- [ ] Editable metadata fields
- [ ] Summary statistics (record counts)
- [ ] Status indicators (Old/New data loaded)

**UI Layout:**
```
???????????????????????????????????????
? Project Information                 ?
? ??????????????????????????????????? ?
? ? Name:     [____________]        ? ?
? ? Client:   [____________]        ? ?
? ? Location: [____________]        ? ?
? ? Date:     2025-01-19            ? ?
? ??????????????????????????????????? ?
?                                     ?
? Data Summary                        ?
? ??????????????????????????????????? ?
? ? Data Type  ? Old   ? New   ? ?  ? ?
? ? Bus        ? 125   ? 130   ? +5 ? ?
? ? LVBreaker  ? 42    ? 45    ? +3 ? ?
? ? Cable      ? 210   ? 208   ? -2 ? ?
? ??????????????????????????????????? ?
???????????????????????????????????????
```

**Acceptance Criteria:**
- [ ] All metadata fields editable
- [ ] Statistics auto-update
- [ ] Theme support (Light/Dark)

---

### Task 21: Create Dynamic Data Tabs
**Goal:** Generate tabs dynamically based on loaded data types

**Deliverables:**
- [ ] `ViewModels/DataTypeTabViewModel.cs`
- [ ] `Views/DataTypeTabView.xaml` (reusable)
- [ ] `Controls/DiffGrid.xaml` (custom control)
- [ ] Tab generation based on `DataSet` contents

**Tab Structure:**
- Overview tab (always first)
- One tab per data type with data (e.g., "Buses", "LV Breakers")
- Each tab shows DiffGrid comparing Old vs New

**DiffGrid Features:**
```
???????????????????????????????????????????????
? ID      ? Old Value ? New Value ? Status    ?
???????????????????????????????????????????????
? BUS-001 ? 480V      ? 480V      ? Unchanged ?
? BUS-002 ? 208V      ? 120V      ? Modified  ?
? BUS-003 ? -         ? 480V      ? Added     ?
? BUS-004 ? 600V      ? -         ? Removed   ?
???????????????????????????????????????????????
```

**Color Coding:**
- Unchanged: Default text
- Modified: Orange/Yellow highlight
- Added: Green highlight
- Removed: Red highlight (strikethrough)

**Acceptance Criteria:**
- [ ] Tabs generate dynamically
- [ ] DiffGrid shows all changes
- [ ] Color coding works
- [ ] Can filter by status (Added/Modified/Removed)

---

### Task 22: Build Project Ribbon Interface
**Goal:** Add ribbon commands for project operations

**Deliverables:**
- [ ] Ribbon tab: "Data Management"
  - Import Old Data
  - Import New Data
  - Clear Old Data
  - Clear New Data
  - Refresh Statistics
  
- [ ] Ribbon tab: "Output Generation"
  - Select Report Spec
  - Select Label Spec
  - Generate Reports
  - Generate Labels
  - Export to PDF

- [ ] Ribbon tab: "Analysis Tools"
  - Show Changes Only
  - Show All Records
  - Export Diff to Excel
  - Validation Summary

**Acceptance Criteria:**
- [ ] All commands implemented
- [ ] Ribbon updates based on document state
- [ ] Commands enable/disable appropriately

---

### Task 23: Implement Report Generation
**Goal:** Generate reports using loaded spec + project data

**Deliverables:**
- [ ] `Services/ReportGenerationService.cs`
- [ ] Spec selection dialog
- [ ] Progress reporting with cancellation
- [ ] PDF output generation
- [ ] Error handling & validation

**Workflow:**
1. User clicks "Generate Reports"
2. Dialog shows available specs (.ezspec files)
3. User selects spec + output folder
4. Service processes each data type
5. Progress bar shows completion
6. PDFs generated in output folder

**Acceptance Criteria:**
- [ ] Can select spec file
- [ ] Progress bar updates
- [ ] Cancellation works
- [ ] PDFs generate correctly
- [ ] Errors logged and displayed

---

## ??? Architecture Decisions

### DataSet Integration
**Leverage existing `EasyAF.Data.Models.DataSet`:**
- Already has composite key support
- Serializes to JSON via `ProjectPersist`
- Supports multi-scenario stitching
- **No changes needed!**

### DiffGrid Design
**Custom WPF Control:**
- Virtualized for performance
- Theme-aware styling
- Sortable/Filterable
- Reusable across all data types

### Import Strategy
**Reuse Map Module's ImportManager:**
- User selects .ezmap file
- Imports data into `DataSet`
- Merges if "Import Additional Scenario"
- Validates against mapping config

---

## ?? Phase 4 Milestones

### Milestone 1: Basic Project Management (Tasks 18-19)
**Goal:** Can create, save, load .ezproj files  
**ETA:** 3-5 days

### Milestone 2: Data Visualization (Tasks 20-21)
**Goal:** DiffGrid shows Old vs New comparison  
**ETA:** 5-7 days

### Milestone 3: Ribbon & Commands (Task 22)
**Goal:** All project operations available  
**ETA:** 3-4 days

### Milestone 4: Report Generation (Task 23)
**Goal:** Can generate PDFs from project data  
**ETA:** 5-7 days

**Total Phase 4 ETA:** 16-23 days

---

## ?? Success Criteria

### Must Have
- [ ] Can create new .ezproj files
- [ ] Can import Old Data using .ezmap
- [ ] Can import New Data using .ezmap
- [ ] DiffGrid shows Added/Modified/Removed records
- [ ] Can generate reports using .ezspec
- [ ] Full theme support (Light/Dark)
- [ ] MVVM strict (zero code-behind)

### Nice to Have
- [ ] Batch processing (multiple projects)
- [ ] Export diff to Excel
- [ ] Validation summary report
- [ ] Historical project tracking
- [ ] Project templates

### Future Enhancements (Phase 5+)
- [ ] Cloud sync
- [ ] Multi-user collaboration
- [ ] Advanced analytics
- [ ] Custom report templates
- [ ] Mobile companion app

---

## ?? Getting Started

### Next Actions:
1. ? Merge Phase 3 to master
2. ? Create `phase-4-project-module` branch
3. ?? **Start Task 18:** Create Project Module Structure
   - Create folder: `modules/EasyAF.Modules.Project/`
   - Add project to solution
   - Implement `ProjectModule.cs`
   - Register with shell

### First Command:
```powershell
# After Git operations complete:
# Create module folder structure
New-Item -ItemType Directory -Path "modules/EasyAF.Modules.Project/ViewModels"
New-Item -ItemType Directory -Path "modules/EasyAF.Modules.Project/Views"
New-Item -ItemType Directory -Path "modules/EasyAF.Modules.Project/Models"
New-Item -ItemType Directory -Path "modules/EasyAF.Modules.Project/Services"
New-Item -ItemType Directory -Path "modules/EasyAF.Modules.Project/Controls"
```

---

## ?? References

- **Development Prompt:** `EasyAF V3 Development Prompt.md` (Tasks 18-23)
- **Phase 3 Completion:** `PHASE-3-COMPLETE-COMMIT.md`
- **DataSet Documentation:** `docs/CORE-6-MODEL-MAPPINGS.md`
- **Import Architecture:** `lib/EasyAF.Import/`

---

**Status:** Ready to Begin Phase 4 ??  
**Branch:** `phase-4-project-module`  
**First Task:** Task 18 - Create Project Module Structure

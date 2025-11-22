# EasyAF V3 Development Prompt

## CRITICAL RULES FOR AI AGENTS

### 🚨 PRIMARY DIRECTIVE
**FOLLOW THE TARGETED TASK LIST EXACTLY AS WRITTEN. DO NOT BREAK DOWN TASKS INTO SUB-TASKS.**

### Core Development Principles
1. **Strict Task Adherence**: Complete each task exactly as specified without decomposition
2. **Module Isolation**: Each module must be completely self-contained with zero hard dependencies on other modules
3. **Cross-Module Edit Documentation**: Any cross-module edit MUST include a comment block:
   ```csharp
   // CROSS-MODULE EDIT: [Date] [Task#]
   // Modified for: [Specific reason]
   // Related modules: [List of modules affected]
   // Rollback instructions: [How to revert if needed]
   ```
4. **State Preservation**: Document state must persist perfectly across all navigation events
5. **No Code-Behind Logic**: Views must contain ONLY InitializeComponent() and event wire-up
6. **Theme-First Design**: Every visual element must use theme resources, never hard-coded colors

## Architecture Foundation

### Technology Stack
- **Framework**: WPF (.NET 8.0)
- **UI Library**: Fluent.Ribbon (latest stable)
- **MVVM Framework**: Prism 9.0
- **DI Container**: Unity or DryIoc (Prism-compatible)
- **Logging**: Serilog with multiple sinks
- **Settings**: JSON-based with hot-reload capability
- **Theme Engine**: Custom resource dictionary system with runtime switching

### Solution Structure
```
EasyAF.V3/
├── EasyAF.Core/                    # Shared infrastructure
│   ├── Contracts/                  # Module interfaces
│   ├── Services/                   # Core services
│   ├── Theme/                      # Theme engine
│   └── Logging/                    # Centralized logging
├── EasyAF.Shell/                   # Container application
│   ├── ViewModels/
│   ├── Views/
│   ├── Services/
│   └── Bootstrapper.cs
├── EasyAF.Modules.Map/             # Map editor module
├── EasyAF.Modules.Project/        # Project editor module  
├── EasyAF.Modules.Spec/           # Spec editor module
└── EasyAF.Libraries/               # Existing EasyAF libraries
```

## Task List - Phase 1: Core Infrastructure

### Task 1: Create Solution Structure
Create new solution with projects: EasyAF.Core, EasyAF.Shell. Add NuGet packages: Fluent.Ribbon, Prism.Unity, Serilog. Create folder structure as specified above.

### Task 2: Implement Theme Engine
Create ThemeManager class in EasyAF.Core that loads theme dictionaries from embedded resources. Implement IThemeService with methods: ApplyTheme(themeName), GetAvailableThemes(). Create Light.xaml and Dark.xaml resource dictionaries with complete brush definitions.

### Task 3: Create Module Contract System
Define IModule interface with properties: ModuleName, FileExtensions, DocumentIcon. Define IDocumentModule with methods: CreateNewDocument(), OpenDocument(path), SaveDocument(), GetRibbonTabs(). Define IModuleCatalog for module registration.

### Task 4: Implement Logging Infrastructure  
Configure Serilog with sinks for: Console, File (rolling), Debug. Create ILoggerService wrapper for module access. Implement log viewer control for shell status bar.

### Task 5: Create Settings Management System
Implement ISettingsService with methods: GetSetting<T>(key), SetSetting<T>(key, value), GetModuleSettings(moduleName). Create SettingsManager using System.Text.Json. Implement file watcher for hot-reload.

## Task List - Phase 2: Shell Application

### Task 6: Create Shell Window
Create main window with: Fluent Ribbon (empty), Vertical tab strip panel (left side), Document content area, Status bar with log viewer. Apply theme support to all elements.

### Task 7: Implement Document Tab System
Create DocumentTabControl as ItemsControl with custom template. Implement tab item template with: Icon, Title, Close button, Modified indicator. Add drag-drop reordering support.

### Task 8: Create Module Loader Service
Implement IModuleLoader that discovers modules via reflection. Create module initialization pipeline. Implement ribbon tab injection system for loaded modules.

### Task 9: Implement Document Manager
Create IDocumentManager service managing open documents. Implement document state persistence across switches. Add dirty state tracking and save prompts.

### Task 10: Create File Management System
Implement New/Open/Save/SaveAs commands in shell. Add recent files tracking (persist in settings). Implement file associations for module types.

### Task 11: Build Settings Dialog
Create modal dialog with tab control. Add Application tab for global settings. Implement dynamic module tabs registration. Add theme selector with live preview.

## Task List - Phase 3: Map Module

### Task 12: Create Map Module Structure
Create EasyAF.Modules.Map project. Implement MapModule class inheriting IDocumentModule. Register module with shell on startup.

### Task 13: Implement Map Data Model
Create MappingConfiguration class holding column-to-property mappings. Implement MappingEntry class with validation rules. Add serialization support for .ezmap files.

### Task 14: Build Map Editor View
Create tabbed interface with: Sample Data tab (file list), Mapping tab (association grid), Preview tab (results view). Implement all views using UserControls with zero code-behind.

### Task 15: Create Map Ribbon Tabs
Implement ribbon tabs: File Operations (Load samples, Save mapping), Mapping Tools (Auto-detect, Validate, Clear). Wire commands through ViewModel only.

### Task 16: Implement Dynamic Property Discovery
Use reflection to discover target class properties. Cache property info for performance. Implement property type validation.

### Task 17: Add Mapping Validation System
Implement one-to-one mapping enforcement. Add duplicate detection with user warnings. Create validation summary panel.

## Task List - Phase 4: Project Module

### Task 18: Create Project Module Structure
Create EasyAF.Modules.Project project. Implement ProjectModule class. Register with shell ModuleCatalog.

### Task 19: Design Project Data Model
Create Project class with metadata properties. Implement DataSet collections for each data type. Add change tracking for all properties.

### Task 20: Build Project Overview Tab
Create overview UserControl with project metadata fields. Add summary statistics display panel. Implement data binding to ProjectViewModel.

### Task 21: Create Dynamic Data Tabs
Implement tab generation based on loaded data types. Create reusable DataTypeView UserControl. Add DiffGrid control for data comparison.

### Task 22: Build Project Ribbon Interface
Create ribbon tabs: Data Management, Output Generation, Analysis Tools. Implement all commands in ProjectViewModel.

### Task 23: Implement Report Generation
Create report spec selection dialog. Implement report generation pipeline. Add progress reporting with cancellation.

## Task List - Phase 5: Spec Module

### Task 24: Create Spec Module Structure
Create EasyAF.Modules.Spec project. Implement SpecModule class with IDocumentModule. Register with ModuleCatalog.

### Task 25: Build Table Definition Model
Create TableDefinition class with column specifications. Implement ColumnDefinition with formatting options. Add report vs label mode flag.

### Task 26: Create Table Editor Interface
Build dynamic tab generation for each table. Create column editor grid control. Implement drag-drop column reordering.

### Task 27: Implement Spec Ribbon Commands
Add ribbon tabs: Table Management, Column Operations. Wire all commands through ViewModel.

### Task 28: Add Spec Validation
Implement column uniqueness checking. Add data type compatibility validation. Create validation results panel.

## Task List - Phase 6: Integration

### Task 29: Implement Inter-Module Communication
Create EventAggregator for module messaging. Define standard message types. Implement publish/subscribe in each module.

### Task 30: Create Batch Processing System
Add batch job definition interface. Implement job queue with progress tracking. Create batch results summary view.

### Task 31: Build Module Discovery System
Implement plugin folder scanning. Add module version checking. Create module dependency resolver.

### Task 32: Performance Optimization
Implement lazy loading for document content. Add virtualization to all data grids. Create memory usage monitoring.

### Task 33: Final Integration Testing
Test all module combinations. Verify theme application completeness. Validate settings persistence.

## Module Interface Specifications

### IModule (Required for all modules)
```csharp
public interface IModule
{
    string ModuleName { get; }
    string ModuleVersion { get; }
    string[] SupportedFileExtensions { get; }
    ImageSource ModuleIcon { get; }
    void Initialize(IUnityContainer container);
    void Shutdown();
}
```

### IDocumentModule (For document-based modules)
```csharp
public interface IDocumentModule : IModule
{
    IDocument CreateNewDocument();
    IDocument OpenDocument(string filePath);
    void SaveDocument(IDocument document, string filePath);
    RibbonTabItem[] GetRibbonTabs(IDocument activeDocument);
    bool CanHandleFile(string filePath);
}
```

### Optional Module Help System (IHelpProvider)
Modules may optionally provide end-user help pages without modifying the base `IModule` contract by implementing `IHelpProvider`.

`IHelpProvider` Contract:
```csharp
public interface IHelpProvider
{
    IEnumerable<HelpPageDescriptor> GetHelpPages();
}

public record HelpPageDescriptor(
    string Id,            // stable unique id e.g. "map.intro"
    string Title,         // user-visible title
    string Category,      // logical grouping e.g. "Getting Started"
    string ResourcePath,  // embedded markdown resource path or relative file path
    string[]? Keywords = null // optional extra search terms
);
```
Implementation Guidelines:
1. Embed Markdown files as `EmbeddedResource` or `Resource` in the module project.
2. Use a predictable folder (recommended: `Help/` root in module project).
3. ResourcePath should match the build action path (e.g. `Help/Intro.md`).
4. Keep `Id` stable; changing it breaks deep links and search index references.
5. Prefer short Category names; hierarchy can be simulated with `Mapping/Advanced` if needed.
6. Provide Keywords to enhance search (synonyms, abbreviations).
7. Keep each page focused; large topics should be split into multiple pages.
8. Use first-level heading (`# Title`) matching the descriptor Title.
9. Avoid hard-coded colors or theme references; UI screenshots should be neutral or dual-theme if added later.

Search Behavior:
- Title, Category, and Keywords are indexed.
- Shell aggregates pages via `IHelpCatalog` when modules load.

Example:
```csharp
public class MapModule : IModule, IHelpProvider
{
    public IEnumerable<HelpPageDescriptor> GetHelpPages() => new[]
    {
        new HelpPageDescriptor("map.intro", "Introduction", "Getting Started", "Help/Intro.md", new[]{"overview","basics"}),
        new HelpPageDescriptor("map.mapping", "Creating Mappings", "Mapping", "Help/CreatingMappings.md", new[]{"associate","link"})
    };
}
```
Future Extensions (Non-breaking):
- Localization via adding locale-specific resources (e.g. `Help/Intro.en-US.md`).
- Rich metadata (e.g. `RequiresModuleFeature`, `Order`) can be added with a new descriptor type if needed.
- A build analyzer can later enforce presence of at least one help page.

Shell Responsibilities:
- Detect modules implementing `IHelpProvider` on load.
- Ignore duplicates (Id uniqueness enforced; first wins).
- Provide Help ribbon tab & dialog (TOC + search) aggregating all registered pages.

Rollback Instructions:
- Remove `IHelpProvider`, `HelpPageDescriptor`, `IHelpCatalog` registrations and delete related files.

## Critical Implementation Notes

### Theme Application Rules
1. Never use hard-coded colors - always use ThemeResource bindings
2. Every control must have explicit style assignment
3. Create control templates for any custom controls
4. Test both light and dark themes after every UI change

### Centralized Theming Architecture
**Location:** `app/EasyAF.Shell/Styles/CommonControls.xaml`

All standard WPF controls have themed styles defined in CommonControls.xaml:
- **Buttons**: BaseButtonStyle (keyed), AccentButtonStyle (keyed)
- **TextBox**: Implicit style with focus/disabled states
- **ListBox/ListBoxItem**: Implicit styles with selection/hover states
- **TabControl/TabItem**: Implicit styles with active/inactive tab states
- **CheckBox**: Implicit style with check mark and all interaction states
- **ScrollBar/Thumb**: Implicit styles for vertical and horizontal scrollbars

**Theme Resources:** All styles reference brushes from `Theme/Light.xaml` and `Theme/Dark.xaml`:
- Background: WindowBackgroundBrush, PrimaryBackgroundBrush, SecondaryBackgroundBrush, ControlBackgroundBrush
- Text: TextPrimaryBrush, TextSecondaryBrush, TextTertiaryBrush, TextDisabledBrush
- Borders: ControlBorderBrush, ControlBorderHoverBrush, ControlBorderFocusBrush
- Tabs: TabActiveBackgroundBrush, TabActiveBorderBrush, TabInactiveBackgroundBrush, TabInactiveBorderBrush
- Buttons: ButtonBackgroundBrush, ButtonForegroundBrush, ButtonBorderBrush, ButtonHoverBackgroundBrush, ButtonDisabledBackgroundBrush, ButtonDisabledForegroundBrush
- Highlights: HighlightSelectionBrush, ReadonlyBackgroundBrush, AccentBrush, AccentHoverBrush

**Custom Controls:** Specialized controls (DocumentTabControl) have dedicated resource dictionaries in `Styles/` folder.

**Usage:** CommonControls.xaml is merged in App.xaml, making all styles available application-wide. Controls automatically inherit these styles unless explicitly overridden.

### Module Isolation Requirements
1. Modules cannot reference each other directly
2. All inter-module communication via EventAggregator
3. Shared data types must be in EasyAF.Core
4. Module views cannot access Shell views directly

### Performance Requirements
1. Document switching must be instant (<50ms)
2. Module loading must not block UI thread
3. Large data sets must use virtualization
4. Memory per document should not exceed 100MB

### Documentation Standards
1. Every public class/method must have XML documentation
2. Complex algorithms require step-by-step comments
3. Cross-module edits require rollback instructions
4. ViewModels must document their command contracts

## Testing Checkpoints

After each task completion, verify:
1. Application compiles without warnings
2. No regression in existing functionality
3. Theme properly applied to new elements
4. Module isolation maintained
5. Settings correctly persisted
6. Memory usage within bounds
7. All MVVM patterns followed (no code-behind logic)

## Error Handling Strategy

1. All module operations wrapped in try-catch
2. Errors logged with full context via Serilog
3. User-friendly error messages in UI
4. Graceful deterioration when modules fail
5. Document recovery on unexpected shutdown

---

**Remember: COMPLETE EACH TASK EXACTLY AS SPECIFIED. DO NOT DECOMPOSE INTO SUB-TASKS.**

---

## Development Journal

### Journal Entry Format
```
Date: [ISO 8601 with US Central timezone, e.g., 2025-01-11T13:00:00-06:00]
Task: [Task number and name]
Status: [In Progress | Paused | Complete | Blocked]
Blocking Issue: [If blocked, describe the issue]
Cross-Module Edits: [List any files modified outside current module]
Notes: [Any important observations or decisions]
Next Task: [What should be worked on next]
```

### ⚠️ CRITICAL JOURNAL RULES FOR AI AGENTS
1. **NEVER DELETE OLD JOURNAL ENTRIES** - The journal is a permanent historical record
2. **Only modify the entry for the task you are currently working on**
3. **Add new entries at the TOP of the Active Journal Entries section**
4. **Supplemental fixes to completed tasks should be added to the original task entry, not create new entries**
5. **All entries remain in the journal permanently - no exceptions**

### Active Journal Entries
**NOTE: Newest entries appear at the top**

```
Date: 2025-01-20T11:45:00-06:00
Task: Task 20 - Build Project Overview Tab
Status: Complete
Blocking Issue: None
Cross-Module Edits:
- modules/EasyAF.Modules.Project/ViewModels/ProjectDocumentViewModel.cs: Main coordinator
- modules/EasyAF.Modules.Project/ViewModels/ProjectSummaryViewModel.cs: Summary tab VM
- modules/EasyAF.Modules.Project/Views/ProjectDocumentView.xaml: TabControl view
- modules/EasyAF.Modules.Project/Views/ProjectSummaryView.xaml: Metadata + statistics view
- modules/EasyAF.Modules.Project/Models/ProjectDocument.cs: Added ViewModel property
- modules/EasyAF.Modules.Project/ProjectModule.cs: ViewModel wiring
- modules/EasyAF.Modules.Project/EasyAF.Modules.Project.csproj: Added UseWindowsForms
Notes:
✅ COMPLETE: Project Summary tab with exact stopgap layout

VIEWMODELS CREATED:
1. ProjectDocumentViewModel (Coordinator)
   - Manages tab collection (Summary + future data tabs)
   - Tab selection/activation
   - Document-level state
   - Implements IDisposable
   - TabHeaderInfo class for tab metadata

2. ProjectSummaryViewModel
   - 12 metadata properties (2-way binding to Project.*)
   - 4 file path properties (Map, Spec, Template, Output)
   - 10 statistics properties (New/Old data counts)
   - 6 commands (Add/Remove files, Browse Map/Spec/Template/Output)
   - RefreshStatistics() method (manual, since Project doesn't impl INotifyPropertyChanged)
   - Implements IDisposable

VIEWS CREATED:
1. ProjectDocumentView.xaml
   - TabControl bound to TabHeaders collection
   - ContentControl with DataTemplate mapping
   - ViewModel → View mapping (ProjectSummaryViewModel → ProjectSummaryView)
   - Theme-compliant (DynamicResource bindings)

2. ProjectSummaryView.xaml
   - EXACT STOPGAP LAYOUT (two-column Grid)
   - LEFT COLUMN:
     * Project GroupBox (metadata 8-row Grid)
     * Report GroupBox (Map/Spec/Template/Output paths)
   - RIGHT COLUMN:
     * Project Summary GroupBox (New/Old data statistics)
   - All theme brush bindings
   - Zero code-behind logic

METADATA FIELDS (exact stopgap layout):
Row 0: LB Project Number (140px) | Site Name (*)
Row 1: Client | Study Engineer
Row 2: Address Line 1 (spans 3 columns)
Row 3: Address Line 2 (spans 3 columns)
Row 4: Address Line 3 (spans 3 columns)
Row 5: City | State (40px, 2 char) | Zip (80px, 5 digit)
Row 6: Study Date | Revision Month
Row 7: Comments (multiline, 80px height, spans 3 columns)

REPORT SECTION:
- Map path + Browse button
- Spec path + Browse button
- Template path + Browse button
- Output folder + Browse button (FolderBrowserDialog)

STATISTICS (right column):
New Data:
- Buses, Breakers, Fuses, Short Circuit, Arc Flash

Old Data:
- Buses, Breakers, Fuses, Short Circuit, Arc Flash

MVVM COMPLIANCE:
✅ Zero code-behind logic (only InitializeComponent)
✅ All logic in ViewModels
✅ Commands for all interactions
✅ 2-way property bindings with UpdateSourceTrigger=PropertyChanged
✅ IsDirty tracking (manual via MarkDirty())

THEME COMPLIANCE:
✅ All brushes use DynamicResource
✅ GroupBox borders use ControlBorderBrush
✅ TextBlocks use TextPrimaryBrush/TextSecondaryBrush
✅ No hard-coded colors

PROJECT INTEGRATION:
- ProjectModule.CreateNewDocument() creates ProjectDocumentViewModel
- ProjectModule.OpenDocument() creates ProjectDocumentViewModel
- ProjectDocument.ViewModel property stores VM for shell rendering
- Shell's DataTemplate system will render view based on ViewModel type

DEFERRED TO TASK 21:
- File import functionality (CSV/Excel → DataSet)
- Dynamic data type tabs
- DiffGrid control integration

BUILD STATUS: ✅ Successful compilation (0 errors, 0 warnings)

ARCHITECTURE NOTES:
- Same pattern as Map Module (proven successful)
- Project class doesn't impl INotifyPropertyChanged (manual MarkDirty)
- ViewModels bind directly to Project.* properties
- Statistics refresh is manual (called after import operations)
- UseWindowsForms enabled for FolderBrowserDialog

Next Task: Task 21 - Create Dynamic Data Tabs (DiffGrid integration)
Rollback Instructions: Remove ViewModels/, Views/ files; remove ViewModel property from ProjectDocument
```

```
Date: 2025-01-20T10:20:00-06:00
Task: Task 18 & 19 - Project Module Structure and Data Model
Status: Complete
Blocking Issue: None
Cross-Module Edits:
- modules/EasyAF.Modules.Project/EasyAF.Modules.Project.csproj: New WPF class library project (net8.0-windows)
- modules/EasyAF.Modules.Project/ProjectModule.cs: IDocumentModule implementation
- modules/EasyAF.Modules.Project/Models/ProjectDocument.cs: IDocument wrapper
- EasyAFv3.sln: Added Project module project to solution
Notes:
✅ COMPLETE: Project module structure AND data model created

TASK 18 - MODULE STRUCTURE:
FOLDER STRUCTURE:
- modules/EasyAF.Modules.Project/
  - ViewModels/ (empty, ready for Task 20)
  - Views/ (empty, ready for Task 20-21)
  - Models/ (ProjectDocument.cs created)
  - Services/ (empty, ready for future services)
  - Controls/ (ready for DiffGrid import in Task 21)

PROJECT CONFIGURATION:
- WPF class library targeting net8.0-windows
- Added project references:
  * EasyAF.Core (interfaces, services)
  * EasyAF.Data (Project class, DataSet)
  * EasyAF.Import (for future import integration)
- Added NuGet packages:
  * Prism.Unity (9.0.537)
  * Serilog (4.3.0) - Updated to match Core dependency
- XML documentation enabled

PROJECTMODULE CLASS:
- Implements IDocumentModule interface
  * ModuleName: "Project Editor"
  * ModuleVersion: "3.0.0"
  * SupportedFileExtensions: ["ezproj"]
  * SupportedFileTypes: IReadOnlyList<FileTypeDefinition> (EasyAF Project Files)
  * Initialize(IUnityContainer): Correct signature
- All interface members implemented:
  * CreateNewDocument(): Creates ProjectDocument with OwnerModule
  * OpenDocument(filePath): Loads via ProjectDocument.LoadFrom()
  * SaveDocument(document, filePath): Delegates to ProjectDocument.SaveAs()
  * GetRibbonTabs(): Placeholder for Task 22
  * CanHandleFile(): Extension-based validation
  * Shutdown(): Cleanup stub

TASK 19 - DATA MODEL:
PROJECTDOCUMENT CLASS:
- Implements IDocument interface completely:
  * FilePath (string?): Persisted location
  * Title (string): Derived from filename or "Untitled Project"
  * IsDirty (bool): Unsaved changes tracking
  * OwnerModule (IDocumentModule): Reference to ProjectModule
  * MarkDirty(): Sets IsDirty = true
  * MarkClean(): Sets IsDirty = false
- Wraps existing EasyAF.Data.Models.Project class
- Factory methods:
  * CreateNew(): Empty project with initialized DataSets
  * LoadFrom(filePath): Deserializes via Project.LoadFromFile()
- Save operations:
  * Save(): Saves to current FilePath
  * SaveAs(filePath): Saves to new location, updates FilePath
- Implements INotifyPropertyChanged for MVVM binding
- Comprehensive XML documentation

PROJECT CLASS ANALYSIS:
✅ ALL required metadata properties already exist in Project class!
- LBProjectNumber ✓
- SiteName ✓
- Client ✓
- StudyEngineer ✓
- AddressLine1, AddressLine2, AddressLine3 ✓
- City, State, Zip ✓
- StudyDate (string, compatible with DatePicker) ✓
- Revision (for "Revision Month") ✓
- Comments ✓
- NewData, OldData (DataSet) ✓
NO PROJECT CLASS MODIFICATIONS NEEDED!

ARCHITECTURE DECISIONS:
- Lightweight wrapper pattern (same as MapDocument)
- Project class is the source of truth
- ProjectDocument only adds IDocument interface compliance
- ViewModels will bind to ProjectDocument.Project.* properties
- Manual IsDirty tracking (no automatic property monitoring)
- Module isolation maintained (no references to other modules)

BUILD STATUS: ✅ Successful compilation (0 errors, 0 warnings)

READY FOR NEXT STEPS:
1. Task 20: Build Summary tab (metadata GroupBox + file management)
2. Task 21: Import DiffGrid and create data tabs

Next Task: Task 20 - Build Project Overview Tab (Summary with metadata + file management)
Rollback Instructions: Remove modules/EasyAF.Modules.Project/ folder and remove from EasyAFv3.sln
```

```
Date: 2025-01-19T23:15:00-06:00
Task: Phase 3 Complete - Map Module with 34 Data Models
Status: Complete
Blocking Issue: None
Cross-Module Edits:
- modules\EasyAF.Modules.Map\EasyAF.Modules.Map.csproj: New WPF class library project (net8.0-windows)
- modules\EasyAF.Modules.Map\MapModule.cs: IDocumentModule implementation with complete XML documentation
- EasyAFv3.sln: Added Map module project to solution
Notes:
✅ COMPLETE: Map module project structure created
- Created WPF class library targeting net8.0-windows
- Added project references: EasyAF.Core, EasyAF.Import, EasyAF.Data
- Added NuGet packages: Prism.Unity (9.0.537), Serilog (3.1.1)
- Created folder structure: ViewModels/, Views/, Models/, Services/
- Implemented MapModule class with IDocumentModule interface:
  - ModuleName: "Map Editor"
  - ModuleVersion: "3.0.0"
  - SupportedFileExtensions: ["ezmap"]
  - SupportedFileTypes: EasyAF Mapping Configuration Files (.ezmap)
  - All public members fully documented with XML comments
  - Placeholder implementations with TODO markers for Tasks 13-16
  - CanHandleFile: Extension-based validation for .ezmap files
- Module follows strict MVVM principles (zero code-behind mandate)
- Module isolation maintained (no references to other modules)
BUILD STATUS: ✅ Successful compilation
ARCHITECTURE NOTES:
- Module registered with solution but NOT yet wired to shell's ModuleLoader
- Shell will discover module automatically once module DLL is copied to Modules/ folder
- Module icon placeholder (null) - can add embedded resource later
- Initialize() method ready for service registration in Task 13
- Document view hosting will use DataTemplate approach (per shell architecture decision)
Next Task: Task 13 - Implement Map Data Model
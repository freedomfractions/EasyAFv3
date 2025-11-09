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

## Development Journal

### Journal Entry Format
```
Date: [ISO 8601]
Task: [Task number and name]
Status: [In Progress | Paused | Complete | Blocked]
Blocking Issue: [If blocked, describe the issue]
Cross-Module Edits: [List any files modified outside current module]
Notes: [Any important observations or decisions]
Next Task: [What should be worked on next]
```

### Active Journal Entries
```
[AI Agents will maintain journal entries here during development]
```

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

## Critical Implementation Notes

### Theme Application Rules
1. Never use hard-coded colors - always use ThemeResource bindings
2. Every control must have explicit style assignment
3. Create control templates for any custom controls
4. Test both light and dark themes after every UI change

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
4. Graceful degradation when modules fail
5. Document recovery on unexpected shutdown

---

**Remember: COMPLETE EACH TASK EXACTLY AS SPECIFIED. DO NOT DECOMPOSE INTO SUB-TASKS.**
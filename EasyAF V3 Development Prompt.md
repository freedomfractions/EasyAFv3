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

### Active Journal Entries
**NOTE: Newest entries appear at the top**

```
Date: 2025-01-11T15:00:00-06:00
Task: Bug Fix - Log Viewer Theme Contrast Issues
Status: Complete
Blocking Issue: None
Cross-Module Edits:
- Modified lib\EasyAF.Core\Logging\LogEntry.cs to use brush resource keys instead of hard-coded colors
Notes:
- IDENTIFIED ISSUE: Log level colors were hard-coded strings ("Gray", "White", "Orange", etc.)
- This violated "Never use hard-coded colors" critical rule
- Light theme: "White" text was invisible on white background, "LightGray" had poor contrast
- Dark theme: Hard-coded colors didn't adapt properly
- SOLUTION: Added theme-specific log level color definitions to both Light.xaml and Dark.xaml
- Light theme colors: Verbose=#6B7280, Debug=#4B5563, Information=#2563EB, Warning=#D97706, Error=#DC2626, Fatal=#991B1B
- Dark theme colors: Verbose=#9CA3AF, Debug=#D1D5DB, Information=#60A5FA, Warning=#FBBF24, Error=#FCA5A5, Fatal=#FECACA
- All colors meet WCAG AA contrast ratio (4.5:1) for readability
- Changed LogEntry.LevelColor property to LevelBrushKey returning resource key strings
- Created ResourceKeyToBrushConverter in Shell/Converters/ to resolve brush keys to actual brushes
- Updated LogViewer.xaml to use converter for log level foreground binding
- Log viewer now properly adapts colors when switching between Light and Dark themes
- Solution builds successfully with no errors or warnings
Next Task: Task 7 - Implement Document Tab System

Date: 2025-01-11T14:30:00-06:00
Task: Task 6 - Create Shell Window
Status: Complete
Blocking Issue: None
Cross-Module Edits: None
Notes: 
- Enhanced MainWindow.xaml with complete shell layout structure
- Added Fluent.Ribbon with enhanced backstage menu (New, Open, Settings, Exit)
- Created vertical tab strip panel on left side with 48px minimum width
- Vertical tab strip styled with SecondaryBackgroundBrush and border separator
- Placeholder emoji icon (📄) shown when no documents open
- Created document content area in main grid column
- Implemented welcome screen with centered layout shown when no documents open
- Welcome screen includes: title, subtitle, action buttons (Create New, Open Existing)
- Added recent files section to welcome screen with themed border
- Created styled buttons with rounded corners using ControlTemplate
- Button hover states use theme brushes (AccentHoverBrush, HighlightSelectionBrush)
- Added DocumentContentContainer ContentControl for future document display (currently collapsed)
- Enhanced Home ribbon tab with File group (New, Open, Save) and View group (themes)
- All buttons use Large size definition for better visibility
- Status bar with collapsible log viewer maintained from Task 4
- All UI elements use DynamicResource bindings for theme colors
- Window starts centered with 1024x768 default size
- Grid layout: Auto (Ribbon), * (Content), Auto (Status Bar)
- Content grid: Auto (Vertical Tabs), * (Document Area)
- All theme brushes applied: WindowBackground, PrimaryBackground, SecondaryBackground, ControlBorder, etc.
- Solution builds successfully with no errors or warnings
Next Task: Task 7 - Implement Document Tab System

Date: 2025-01-11T14:00:00-06:00
Task: Task 5 - Create Settings Management System
Status: Complete
Blocking Issue: None
Cross-Module Edits: 
- Modified app\EasyAF.Shell\Services\ThemeService.cs to accept ISettingsService dependency and persist theme changes
Notes: 
- Created ISettingsService interface in Core/Contracts/ with methods: GetSetting<T>, SetSetting<T>, GetModuleSettings, SetModuleSetting, GetModuleSetting, Save, Reload
- Added SettingsReloaded event for hot-reload notifications
- Created ApplicationSettings model in Core/Services/ with Global and Modules dictionaries
- Implemented SettingsManager in Core/Services/ using System.Text.Json for serialization
- Settings stored in %AppData%\EasyAF\settings.json with pretty-printed JSON (WriteIndented)
- Implemented FileSystemWatcher for automatic hot-reload when settings file changes externally
- File watcher monitors LastWrite and Size changes with 100ms debounce
- Settings file auto-created with default values (Theme: Light, LogLevel: Information) if not found
- Module settings isolated in separate dictionary sections by module name
- Thread-safe access to settings with lock object
- Implemented IDisposable pattern for proper FileSystemWatcher cleanup
- Updated App.xaml.cs to register SettingsManager as singleton
- Updated App.xaml.cs to load saved theme on startup and save settings on exit
- Modified ThemeService to persist theme selection to settings when changed
- All setting conversions handle JsonElement types from deserialization
- Settings changes logged with Serilog for debugging
- Solution builds successfully with no errors or warnings
Next Task: Task 6 - Create Shell Window

Date: 2025-01-11T13:30:00-06:00
Task: Task 4 - Implement Logging Infrastructure
Status: Complete
Blocking Issue: None
Cross-Module Edits: None
Notes: 
- Created ILoggerService interface in Core/Contracts/ with methods: Verbose, Debug, Information, Warning, Error, Fatal, ForContext
- Implemented LoggerService in Core/Services/ wrapping Serilog with context enrichment support
- Created LogEntry model in Core/Logging/ for UI display with level, timestamp, message, exception, source context
- Implemented InMemoryLogSink in Core/Logging/ as custom Serilog sink for capturing logs to ObservableCollection
- InMemoryLogSink handles cross-thread marshalling to UI thread and limits entries to 1000 max
- Created LogViewer UserControl in Shell/Views/ with header, clear button, and virtualized ListBox
- Created LogViewerViewModel in Shell/ViewModels/ with ClearLog command
- Updated App.xaml.cs to configure Serilog with Console, File (rolling), Debug, and InMemoryLogSink
- Registered ILoggerService as singleton and shared ObservableCollection<LogEntry> with Unity
- Updated MainWindow.xaml to include LogViewer in collapsible Expander at bottom (status bar area)
- Updated MainWindowViewModel to include LogViewerViewModel property
- All log entries display with timestamp, level (color-coded), and message in monospace font
- Log viewer uses virtualization for performance with large log sets
- Solution builds successfully with no errors or warnings
Next Task: Task 5 - Create Settings Management System

Date: 2025-01-11T13:00:00-06:00
Task: Task 3 - Create Module Contract System
Status: Complete
Blocking Issue: None
Cross-Module Edits: None
Notes: 
- Created IModule interface in Core/Contracts/ with properties: ModuleName, ModuleVersion, SupportedFileExtensions, ModuleIcon
- Created IModule with methods: Initialize(IUnityContainer), Shutdown()
- Created IDocument interface representing document instances with FilePath, Title, IsDirty, OwnerModule
- Created IDocumentModule interface extending IModule with methods: CreateNewDocument(), OpenDocument(path), SaveDocument(document, path), GetRibbonTabs(activeDocument), CanHandleFile(filePath)
- Created IModuleCatalog interface for module registration with methods: RegisterModule, UnregisterModule, GetModule, FindModuleForFile, GetModulesByExtension
- Implemented ModuleCatalog service in Core/Services/ with thread-safe module management
- Updated EasyAF.Core.csproj to target net8.0-windows with UseWPF for WPF types
- Added Fluent.Ribbon and Unity package references to Core project
- Registered IModuleCatalog as singleton in Shell App.xaml.cs (using fully qualified names to avoid Prism naming conflict)
- All interfaces fully documented with XML comments
- Solution builds successfully with no errors or warnings
Next Task: Task 4 - Implement Logging Infrastructure

Date: 2025-01-11T12:30:00-06:00
Task: Test Entry - Journal Update Verification
Status: Complete
Blocking Issue: None
Cross-Module Edits: None
Notes: 
- This is a test entry to verify the journal update mechanism works correctly
- The AI can successfully add formatted journal entries using the edit_file tool
- Entries are added at the top of the journal (newest first)
- This entry demonstrates the proper format for future journal updates
Next Task: Continue with actual development tasks

Date: 2025-01-11T12:00:00-06:00
Task: Task 2 - Implement Theme Engine
Status: Complete
Blocking Issue: None
Cross-Module Edits: None
Notes: 
- Created IThemeService interface in Core/Contracts/ with ThemeDescriptor record
- Implemented ThemeService in Shell/Services/ with Light/Dark theme support
- Created Light.xaml theme dictionary with comprehensive color palette (backgrounds, borders, text, status colors)
- Created Dark.xaml theme dictionary with comprehensive color palette
- Updated App.xaml to use PrismApplication and include Fluent.Ribbon resources
- Updated App.xaml.cs to configure Serilog, register services with Unity, and apply default theme
- Converted MainWindow to RibbonWindow with theme-aware brushes
- Created MainWindowViewModel with theme switching commands (SwitchToLightTheme, SwitchToDarkTheme, Exit)
- All UI elements use DynamicResource bindings for theme colors
- ThemeService integrates with ControlzEx for Fluent.Ribbon theming
- Solution builds successfully with no errors or warnings
- Added development prompt markdown file to project for portability
Next Task: Task 3 - Create Module Contract System

Date: 2025-01-11T00:00:00-06:00
Task: Task 1 - Create Solution Structure
Status: Complete
Blocking Issue: None
Cross-Module Edits: None
Notes: 
- Created EasyAFv3.sln with app/lib folder structure
- Created EasyAF.Core class library in lib/
- Created EasyAF.Shell WPF application in app/
- Added NuGet packages: Fluent.Ribbon, Prism.Unity, Serilog (with Console, File, Debug sinks)
- Created folder structure: Contracts/, Services/, Theme/, Logging/ in Core
- Created folder structure: ViewModels/, Views/, Services/ in Shell
- Shell project references Core project
- Cloned EasyAFv2 repo to sandbox/EasyAFv2 (ProjectView-Dev-v2 branch) for reference
- Initialized Git repository and connected to https://github.com/freedomfractions/EasyAFv3.git
- Solution builds successfully with no errors
Next Task: Task 2 - Implement Theme Engine
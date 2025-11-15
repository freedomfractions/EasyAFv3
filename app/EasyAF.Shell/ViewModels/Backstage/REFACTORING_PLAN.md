# OpenBackstageViewModel Refactoring Plan

## Current State
- **File**: `OpenBackstageViewModel.cs`
- **Lines**: 1,245
- **Commands**: 19
- **Properties**: 23

## Target Structure

### 1. SearchViewModel (~100 lines)
**Responsibility**: Search and filtering logic
- Properties: `SearchText`
- Methods:
  - `DebounceSearch()`
  - `ApplySearchFilter()`
  - `MatchesFileSearch()`
  - `MatchesFolderSearch()`
  - `MatchesBrowserEntrySearch()`
  - `MatchesFolderFileSearch()`

### 2. RecentFilesViewModel (~150 lines)
**Responsibility**: Recent Files tab logic
- Properties: `RecentFiles`, `_allRecentFiles`
- Commands: `OpenFileCommand`, `TogglePinCommand`, `CopyPathCommand`, `RemoveFromListCommand`, `OpenFileLocationCommand`
- Methods:
  - `LoadRecentFilesFromService()`
  - `OnRecentFilesChanged()`
  - `IsPinnedFile()`
  - `SavePinnedFiles()`

### 3. RecentFoldersViewModel (~100 lines)
**Responsibility**: Recent Folders tab logic
- Properties: `RecentFolders`, `_allRecentFolders`
- Commands: `OpenFolderCommand`, `CopyFolderPathCommand`, `RemoveFolderFromListCommand`
- Methods:
  - `LoadSampleRecentFolders()` (TODO: replace with service)

### 4. QuickAccessViewModel (~250 lines)
**Responsibility**: Quick Access management
- Properties: `QuickAccessFolders`, `SelectedQuickAccessFolder`
- Commands: `SelectQuickAccessFolderCommand`, `AddToQuickAccessCommand`, `RemoveFromQuickAccessCommand`
- Methods:
  - `SaveQuickAccessFolders()`
  - `LoadQuickAccessFolders()`
  - `LoadDefaultQuickAccessFolders()`

### 5. FolderBrowserViewModel (~200 lines)
**Responsibility**: Folder browsing logic
- Properties: `BrowserEntries`, `CurrentBrowsePath`, `CanNavigateUp`
- Commands: `OpenBrowserEntryCommand`, `NavigateUpCommand`, `CopyBrowserPathCommand`
- Methods:
  - `LoadBrowserEntries()`
  - `ApplyBrowserEntryFilter()`

### 6. OpenBackstageViewModel (~250 lines) - ORCHESTRATOR
**Responsibility**: Coordinate all sub-ViewModels
- Properties: `Mode`, `RecentTab`
- Commands: `SelectRecentCommand`, `BrowseCommand`, `FocusSearchCommand`
- Child ViewModels:
  - `SearchViewModel`
  - `RecentFilesViewModel`
  - `RecentFoldersViewModel`
  - `QuickAccessViewModel`
  - `FolderBrowserViewModel`
- Events: `FileSelected`, `FolderSelected`, `FocusSearchRequested`, `ScrollToTopRequested`

## Progress Tracker

- [ ] Step 1: Create `SearchViewModel` 
- [ ] Step 2: Create `RecentFilesViewModel`
- [ ] Step 3: Create `RecentFoldersViewModel`
- [ ] Step 4: Create `QuickAccessViewModel`
- [ ] Step 5: Create `FolderBrowserViewModel`
- [ ] Step 6: Refactor `OpenBackstageViewModel` to use child VMs
- [ ] Step 7: Update XAML bindings (if needed)
- [ ] Step 8: Test all functionality
- [ ] Step 9: Commit and push

## Notes
- **No XAML changes** should be needed - bindings will stay the same
- Each ViewModel will be unit-testable in isolation
- Orchestrator VM will delegate to child VMs

## Rollback Plan
Tag: `v0.1.0-backstage-working` (already created)

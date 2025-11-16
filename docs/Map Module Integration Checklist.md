# Map Module - Final Integration Checklist

## Current Status

? **Map Module Code**: Complete
- ViewModels created with proper MVVM
- Views created with zero code-behind
- Models defined with IDocument integration
- Services registered and functional
- Build successful with zero errors

? **Shell Integration**: Partial
- Module DLL copied to Shell output folder (via project reference)
- Module Discovery infrastructure in place (ModuleLoader)
- Container passed to modules for service registration
- Document Manager ready to host documents

## What's Working

1. ? **Module Discovery** - ModuleLoader scans assemblies
2. ? **Service Registration** - MapModule.Initialize() registers services
3. ? **File Associations** - .ezmap files recognized
4. ? **Document Model** - MapDocument implements IDocument
5. ? **ViewModel Wiring** - MapDocument.ViewModel property set

## What Needs Testing/Verification

### 1. Module Discovery Verification

**Question:** Is the MapModule being discovered and registered?

**How to Verify:**
- Run the app and check the debug output/logs
- Look for: `"Module loaded: Map Editor v3.0.0"`
- If not found, check `ModuleCatalog.Modules.Count`

**Expected Result:** Map Editor should appear in the About dialog's module list

**If Not Working:**
- Check if `EasyAF.Modules.Map.dll` is in the output folder
- Verify `MapModule` class is public and not abstract
- Verify `MapModule` has a parameterless constructor

---

### 2. New Command Enablement

**Question:** Is the File ? New command enabled?

**How to Verify:**
- Open the app
- Check if the "New" button in the ribbon is enabled (not grayed out)
- Open backstage ? should see "New" tab with module selector

**Expected Result:** New command enabled, Map Editor appears in module dropdown

**Root Cause if Disabled:**
```csharp
private bool CanExecuteNew() => SelectedModule != null;
```

`SelectedModule` is set in FileCommandsViewModel constructor:
```csharp
_selectedModule = AvailableDocumentModules.FirstOrDefault();
```

This depends on `_moduleCatalog.DocumentModules` being populated.

**If Not Working:**
- Module not discovered OR
- Module not implementing `IDocumentModule` OR
- ModuleLoader ran before ModuleCatalog was ready

---

### 3. Document View Hosting

**Question:** When a MapDocument is created, does its view display?

**Current Approach:** DataTemplate in App.xaml
```xaml
<DataTemplate DataType="{x:Type mapVM:MapDocumentViewModel}">
    <mapView:MapDocumentView />
</DataTemplate>
```

**Problem:** App.xaml file appears corrupted in the file viewer (missing `<Application>` tag)

**How to Verify:**
1. Check if App.xaml actually has proper structure
2. Create a new Map document via File ? New
3. Check if MapDocumentView appears in the main content area

**Expected Result:** Tabbed interface with "Summary" tab visible

**If Not Working:**
- DataTemplate not registered (check App.xaml structure)
- ContentControl not bound to document's ViewModel
- Assembly references wrong in DataTemplate

---

### 4. MainWindow Content Binding

**Question:** Is the main content area bound to the active document?

**Required:** MainWindow.xaml should have:
```xaml
<ContentControl Content="{Binding SelectedDocument.ViewModel}" />
```

OR rely on DataTemplate selector based on document type.

**Expected Result:** When you select a document tab, its view appears in the main area

**If Not Working:**
- No binding between DocumentTabControl.SelectedDocument and ContentControl.Content
- DataTemplate not found for the ViewModel type

---

## Action Plan

### Step 1: Verify Module Discovery (5 minutes)

Run the app and check logs:

```
Expected in debug output:
[INFO] EasyAF v3 starting up
[INFO] Module loaded: Map Editor v3.0.0
[INFO] Module registered: Map Editor v3.0.0
```

**Test:** Help ? About ? Check "Loaded Modules" list

**If Module NOT Discovered:**
- Add logging to MapModule constructor
- Check bin/Debug/net8.0-windows folder for EasyAF.Modules.Map.dll
- Verify the DLL isn't blocked (Windows file security)

---

### Step 2: Fix App.xaml Structure (2 minutes)

The App.xaml file appears to have lost its root `<Application>` tag. 

**Current (broken):**
```xaml
<Application.Resources>
    ...
</Application.Resources>
```

**Should be:**
```xaml
<Application x:Class="EasyAF.Shell.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mapVM="clr-namespace:EasyAF.Modules.Map.ViewModels;assembly=EasyAF.Modules.Map"
             xmlns:mapView="clr-namespace:EasyAF.Modules.Map.Views;assembly=EasyAF.Modules.Map">
    <Application.Resources>
        <ResourceDictionary>
            <!-- Theme resources... -->
            
            <!-- Map Module View Template -->
            <DataTemplate DataType="{x:Type mapVM:MapDocumentViewModel}">
                <mapView:MapDocumentView />
            </DataTemplate>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

**Fix:** Manually restore the proper structure or use git to check the actual file

---

### Step 3: Verify Content Binding (3 minutes)

Check Main Window.xaml for document content area binding.

**Look for something like:**
```xaml
<ContentControl Content="{Binding DocumentManager.ActiveDocument}" />
```

**Should actually be:**
```xaml
<ContentControl>
    <ContentControl.Content>
        <Binding Path="DocumentManager.ActiveDocument.ViewModel" />
    </ContentControl.Content>
</ContentControl>
```

The DataTemplate will then resolve `MapDocumentViewModel` ? `MapDocumentView`

---

### Step 4: Test End-to-End (10 minutes)

1. Run the app
2. File ? New
3. Select "Map Editor" from dropdown
4. Click "Create"
5. **Expected:** MapDocumentView appears with Summary tab

**If it works:**
- ? Module discovery working
- ? Document creation working
- ? View hosting working
- ? MVVM wiring working

**If it doesn't work:**
- Check which step failed
- Add logging to each step
- Verify data binding in Snoop or Live Visual Tree

---

## Known Issues to Address

### Issue 1: App.xaml File Corruption

The file viewer shows an incomplete App.xaml (missing root tag). This might be:
- A display bug in the viewer
- Actual file corruption from the edit_file tool

**Resolution:** Check the actual file with `git show HEAD:app/EasyAF.Shell/App.xaml`

---

### Issue 2: DataTemplate Assembly References

The xmlns declarations in the DataTemplate might not resolve if:
- The assembly isn't loaded yet
- The namespace is wrong
- The assembly name changed

**Test:** Add xmlns to App.xaml root instead of inline in DataTemplate

---

### Issue 3: Document Content Display

The audit findings mentioned ContentControl binding missing. Need to verify:
- Is `SelectedDocument` exposed on MainWindowViewModel?
- Is it bound to the ContentControl?
- Does the ContentControl use `Content="{Binding SelectedDocument.ViewModel}"`?

---

## Success Criteria

When everything is working, this workflow should succeed:

1. **Launch App** ? No errors, Map module in About dialog
2. **File ? New** ? Button enabled, "Map Editor" in dropdown
3. **Select Map Editor ? Create** ? New document appears in tabs
4. **Tab shows:** "Untitled Map" with ? indicator
5. **Main area shows:** MapDocumentView with Summary tab
6. **Summary tab contains:** Metadata fields and status

---

## Next Session Plan

**Priority 1:** Fix App.xaml structure
**Priority 2:** Verify module discovery
**Priority 3:** Test New command
**Priority 4:** Verify view hosting

**Estimated Time:** 30 minutes to full working state

---

## Fallback Plan

If DataTemplate approach proves problematic:

**Alternative:** Add View property to IDocument
```csharp
public interface IDocument
{
    // ...existing properties...
    FrameworkElement View { get; }
}
```

Then bind directly:
```xaml
<ContentControl Content="{Binding SelectedDocument.View}" />
```

This is more explicit but couples documents to WPF controls.

---

**Last Updated:** 2025-01-15 20:00 CST
**Status:** Ready for testing
**Confidence:** ?? Medium (pending verification of App.xaml structure)

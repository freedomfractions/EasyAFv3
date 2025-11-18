# DATA MODEL REFACTOR PLAN - v0.2.0

**Date:** January 17, 2025  
**Branch:** `master` ? `feature/full-easypower-model`  
**Status:** ?? **BREAKING CHANGES AHEAD**

---

## ?? CRITICAL WARNING

This document outlines a **MAJOR REFACTOR** of the core EasyAF data model classes. This is a **BREAKING CHANGE** that will:

1. **Replace ALL existing properties** in Bus, LVCB, Fuse, Cable, ArcFlash, ShortCircuit classes
2. **Add property attributes** (Category, Units, Description, Required) - new dependency for all consumers
3. **Enforce GZip compression** for all project file saves (`.eafproj` files)
4. **Update PropertyDiscoveryService** to use category-based filtering
5. **Impact ALL libraries** that consume `EasyAF.Data` models

---

## ?? CURRENT STATE (v0.1.0)

### Existing Property Counts:
- **Bus**: 60 properties (partial coverage)
- **LVCB**: 95 properties (includes flattened TripUnit, good coverage)
- **Cable**: 20 properties (minimal coverage)
- **Fuse**: 50 properties (partial coverage)
- **ArcFlash**: 20 properties (minimal coverage)
- **ShortCircuit**: 17 properties (minimal coverage)

### Existing Design Patterns:
? All properties are `string?` (nullable strings)  
? Flat structure (no nested objects except legacy `LVCB.TripUnit`)  
? Comprehensive XML documentation with column name references  
? `ToString()` overrides for debugging  
? Integration with `DiffUtil` for change tracking  

---

## ?? REFACTOR GOALS (v0.2.0)

### 1. Complete EasyPower Column Coverage
**Source:** `sample-easypower-export.csv` (rectangular dataset with all 6 data types)

**Process:**
1. Parse CSV to extract ALL unique column headers per class
2. Normalize column names to PascalCase (trim whitespace, remove special chars)
3. Map each column to a property with appropriate name
4. Add XML documentation with original column name reference

**Result:** ~300-400 total properties across all 6 classes

### 2. Property Attribute System
**New Attributes:**
```csharp
[AttributeUsage(AttributeTargets.Property)]
public class CategoryAttribute : Attribute
{
    public string Category { get; }
    public CategoryAttribute(string category) => Category = category;
}

[AttributeUsage(AttributeTargets.Property)]
public class UnitsAttribute : Attribute
{
    public string Units { get; }
    public UnitsAttribute(string units) => Units = units;
}

[AttributeUsage(AttributeTargets.Property)]
public class DescriptionAttribute : Attribute
{
    public string Description { get; }
    public DescriptionAttribute(string description) => Description = description;
}

[AttributeUsage(AttributeTargets.Property)]
public class RequiredAttribute : Attribute { }
```

**Categories Defined:**
- **Identity**: Id, Name, Status, DeviceCode
- **Electrical**: Voltage, Phases, Frequency, Current
- **Physical**: Manufacturer, Type, Style, Model
- **Protection**: AIC, Ratings, Trip Settings, Fuses
- **Location**: Area, Zone, Facility, Coordinates
- **Reliability**: Failure Rate, Repair Cost, MTTR
- **Study Results**: Arc Flash, Short Circuit, Load Flow
- **Metadata**: Comments, DataStatus, Labels

### 3. LVCB Class Cleanup
**Current Issues:**
- Property naming inconsistencies (e.g., `ContCurrentA` vs `Cont A`)
- Redundant properties from legacy imports
- Unclear abbreviations

**Resolution:**
1. **Standardize all property names** (consistent PascalCase)
2. **Remove duplicate/redundant properties**
3. **Clarify abbreviations in XML docs**
4. **Keep flat structure** (no re-nesting TripUnit)
5. **Remove obsolete `TripUnit` property entirely** (no testing to break)

### 4. GZip Compression Enforcement
**Current State:**
- `ProjectPersist` class has GZip support (`SaveGZip`, `LoadGZip` methods)
- NOT enforced - users can save raw JSON

**New Behavior:**
- ? **Reading**: Accept both `.eafproj` (raw JSON) AND `.eafproj.gz` (GZip)
- ? **Writing**: ALWAYS save as `.eafproj.gz` (GZip compressed)
- ? **Backward Compatibility**: Can still open old raw JSON files
- ? **File Size**: Expected ~90% reduction (JSON compresses extremely well)

**Implementation:**
```csharp
// In ProjectPersist.Save():
public static void Save(Project project, string filePath)
{
    // ALWAYS save as GZip
    var gzipPath = filePath.EndsWith(".gz") ? filePath : filePath + ".gz";
    SaveGZip(project, gzipPath);
}

// In ProjectPersist.Load():
public static Project Load(string filePath)
{
    // Try GZip first, fallback to raw JSON
    if (File.Exists(filePath + ".gz"))
        return LoadGZip(filePath + ".gz");
    else if (File.Exists(filePath))
        return LoadRaw(filePath); // Backward compatibility
    else
        throw new FileNotFoundException();
}
```

### 5. PropertyDiscoveryService Updates
**Current:** Returns all properties as flat list  
**New:** Group properties by category using `[Category]` attribute

**Benefits:**
- Map module can show categorized property lists
- Better UX (e.g., "Electrical" section, "Protection" section)
- Easier to hide entire categories via settings

---

## ?? FILES TO MODIFY

### Core Data Models (BREAKING CHANGES):
- ? `lib/EasyAF.Data/Models/Bus.cs` - **FULL REWRITE**
- ? `lib/EasyAF.Data/Models/LVCB.cs` - **FULL REWRITE** (cleanup + expand)
- ? `lib/EasyAF.Data/Models/Cable.cs` - **FULL REWRITE**
- ? `lib/EasyAF.Data/Models/Fuse.cs` - **FULL REWRITE**
- ? `lib/EasyAF.Data/Models/ArcFlash.cs` - **FULL REWRITE**
- ? `lib/EasyAF.Data/Models/ShortCircuit.cs` - **FULL REWRITE**

### New Attribute Classes:
- ? `lib/EasyAF.Data/Attributes/CategoryAttribute.cs` - **NEW FILE**
- ? `lib/EasyAF.Data/Attributes/UnitsAttribute.cs` - **NEW FILE**
- ? `lib/EasyAF.Data/Attributes/DescriptionAttribute.cs` - **NEW FILE**
- ? `lib/EasyAF.Data/Attributes/RequiredAttribute.cs` - **NEW FILE**

### Persistence Layer:
- ? `lib/EasyAF.Data/ProjectPersist.cs` - **MODIFY** (enforce GZip)

### Discovery Service:
- ? `modules/EasyAF.Modules.Map/Services/PropertyDiscoveryService.cs` - **MODIFY** (add category support)

### Import Engine:
- ? `lib/EasyAF.Import/ImportManager.cs` - **VALIDATE** (ensure compatibility with new properties)

### Export Engine:
- ? `lib/EasyAF.Export/ReportEngine.cs` - **VALIDATE** (ensure Word template generation works)

---

## ?? BREAKING CHANGES IMPACT ANALYSIS

### **EasyAF.Data** (Core Library):
- **Breaking:** ALL model classes have different property sets
- **Impact:** Any code accessing specific properties by name will break
- **Mitigation:** Property names mostly preserved where possible; new properties added

### **EasyAF.Import** (Import Engine):
- **Breaking:** Mapping configurations reference property names
- **Impact:** Existing `.ezmap` files may have orphaned mappings to renamed/removed properties
- **Mitigation:** Map module will show warnings for unmapped properties

### **EasyAF.Export** (Report Engine):
- **Breaking:** Word templates reference property paths
- **Impact:** Existing templates may have broken placeholders
- **Mitigation:** Templates will show `{PropertyName}` as literal text if property not found

### **EasyAF.Engine** (Calculation Engine):
- **Breaking:** Calculations may reference specific property names
- **Impact:** Engine may fail if expected properties renamed
- **Mitigation:** Review all calculation code after refactor

### **EasyAF.Shell** (UI):
- **Breaking:** No direct impact (uses PropertyDiscovery for reflection-based access)
- **Impact:** Minimal - UI dynamically discovers properties
- **Mitigation:** None needed

### **EasyAF.Modules.Map** (Map Module):
- **Breaking:** PropertyDiscoveryService signature changes (now returns categorized properties)
- **Impact:** UI needs to handle category groups
- **Mitigation:** Update DataTypeMappingViewModel to group by category

---

## ?? MIGRATION CHECKLIST

### Pre-Refactor (THIS COMMIT):
- [x] Create this refactor plan document
- [x] Document current state (property counts, patterns)
- [x] Identify all impacted files
- [x] Create detailed commit message
- [x] Push to master as "pre-refactor checkpoint"

### Phase 1: Data Model Attributes (Day 1)
- [ ] Create attribute classes (Category, Units, Description, Required)
- [ ] Add attributes to existing properties (test infrastructure)
- [ ] Update PropertyDiscoveryService to read attributes
- [ ] Test Map module with categorized properties
- [ ] Commit: "feat(data): Add property attribute system"

### Phase 2: Parse EasyPower Export (Day 1)
- [ ] Load `sample-easypower-export.csv`
- [ ] Parse column headers per class (Bus, LVCB, Fuse, Cable, ArcFlash, ShortCircuit)
- [ ] Normalize column names (trim, PascalCase, remove special chars)
- [ ] Generate property scaffolding (name, type, XML comment)
- [ ] Review for duplicates/conflicts
- [ ] Commit: "chore(data): Generate property scaffolding from EasyPower export"

### Phase 3: Bus Model Rewrite (Day 2)
- [ ] Replace all Bus properties with complete set
- [ ] Add Category/Units/Description attributes
- [ ] Mark required properties (Id, Voltage, Phases)
- [ ] Update ToString() method
- [ ] Test import with real data file
- [ ] Commit: "feat(data): Complete Bus model with full EasyPower property set"

### Phase 4: LVCB Model Cleanup & Expansion (Day 2)
- [ ] Standardize all property names (fix inconsistencies)
- [ ] Add missing properties from export
- [ ] Remove obsolete TripUnit property
- [ ] Add Category/Units/Description attributes
- [ ] Test import with real data file
- [ ] Commit: "feat(data): Refactor LVCB model - cleanup + full property set"

### Phase 5: Cable/Fuse Models (Day 3)
- [ ] Rewrite Cable model (expand from 20 ? ~60 properties)
- [ ] Rewrite Fuse model (expand from 50 ? ~80 properties)
- [ ] Add attributes to all properties
- [ ] Test imports
- [ ] Commit: "feat(data): Complete Cable and Fuse models"

### Phase 6: ArcFlash/ShortCircuit Models (Day 3)
- [ ] Rewrite ArcFlash model (expand from 20 ? ~40 properties)
- [ ] Rewrite ShortCircuit model (expand from 17 ? ~30 properties)
- [ ] Add attributes to all properties
- [ ] Test imports
- [ ] Commit: "feat(data): Complete ArcFlash and ShortCircuit study models"

### Phase 7: GZip Enforcement (Day 4)
- [ ] Update ProjectPersist.Save() to always use GZip
- [ ] Update ProjectPersist.Load() to try GZip first
- [ ] Add file extension handling (.eafproj vs .eafproj.gz)
- [ ] Test backward compatibility (load old raw JSON files)
- [ ] Update file dialogs to show .eafproj.gz extension
- [ ] Commit: "feat(persist): Enforce GZip compression for all project saves"

### Phase 8: Validation & Testing (Day 4-5)
- [ ] Run full import test suite (all 6 data types)
- [ ] Test round-trip (import ? save ? load ? export)
- [ ] Verify Map module category grouping
- [ ] Test GZip file size reduction
- [ ] Update user documentation
- [ ] Commit: "test(data): Validate full model refactor with real data"

### Post-Refactor:
- [ ] Update `docs/Map Module User Guide.md` with category examples
- [ ] Create migration guide for existing `.ezmap` files
- [ ] Tag as `v0.2.0-model-refactor`
- [ ] Update README with new property counts
- [ ] Close this refactor plan document

---

## ?? ROLLBACK PLAN

If critical issues arise after merge:

### Option A: Revert Entire Refactor
```powershell
git revert <refactor-merge-commit-hash>
git push origin master
```

### Option B: Fix Forward
```powershell
git checkout -b hotfix/model-fix
# Fix specific issues
git commit -am "hotfix: Fix data model issue"
git push origin hotfix/model-fix
# Merge to master
```

### Option C: Cherry-Pick Good Changes
```powershell
git checkout -b feature/model-v2
git cherry-pick <attribute-commit>
git cherry-pick <gzip-commit>
# Skip problematic commits
git push origin feature/model-v2
```

---

## ?? EXPECTED OUTCOMES

### Property Counts After Refactor:
- **Bus**: 60 ? **~120 properties** (100% coverage)
- **LVCB**: 95 ? **~110 properties** (cleanup + additions)
- **Cable**: 20 ? **~60 properties** (3x expansion)
- **Fuse**: 50 ? **~80 properties** (60% increase)
- **ArcFlash**: 20 ? **~40 properties** (2x expansion)
- **ShortCircuit**: 17 ? **~30 properties** (75% increase)

**Total**: ~260 properties ? **~440 properties** (70% increase)

### File Size Impact:
- **Raw JSON**: ~500 KB typical project ? **~1.2 MB** (property expansion)
- **GZip Compressed**: ~50 KB typical project ? **~120 KB** (still 90% reduction)

### Performance Impact:
- **PropertyDiscovery**: Reflection overhead increases linearly with property count
- **Import Speed**: Minimal impact (column mapping is O(1))
- **Diff Operations**: May increase ~50% (more properties to compare)
- **Serialization**: GZip compression/decompression adds ~50ms per save/load

---

## ?? COMMIT MESSAGE TEMPLATE

```
refactor(data)!: BREAKING - Complete EasyPower data model with attributes

BREAKING CHANGES:
- All 6 data model classes (Bus, LVCB, Fuse, Cable, ArcFlash, ShortCircuit) rewritten
- Property counts increased from ~260 to ~440 (full EasyPower coverage)
- Added property attribute system (Category, Units, Description, Required)
- GZip compression now enforced for all project file saves
- LVCB class cleaned up (standardized naming, removed obsolete TripUnit property)

MIGRATION REQUIRED:
- Existing .ezmap files may have orphaned mappings (validation warnings shown)
- Existing report templates may have broken placeholders
- Update any code directly accessing model properties by name

NEW FEATURES:
- Category-based property grouping in Map module
- Units display for numeric properties
- Description tooltips for all properties
- Required field validation

FILES CHANGED:
- lib/EasyAF.Data/Models/*.cs (6 files rewritten)
- lib/EasyAF.Data/Attributes/*.cs (4 new attribute classes)
- lib/EasyAF.Data/ProjectPersist.cs (enforce GZip)
- modules/EasyAF.Modules.Map/Services/PropertyDiscoveryService.cs (category support)

TESTING:
- Validated with sample-easypower-export.csv (all 6 data types)
- Round-trip tested (import ? save ? load ? export)
- GZip compression verified (~90% file size reduction)
- Backward compatibility confirmed (can load old raw JSON files)

See docs/DATA MODEL REFACTOR PLAN.md for complete details.
```

---

## ?? FINAL WARNING

**DO NOT PROCEED** with this refactor unless:
1. ? You have backed up all existing `.eafproj` project files
2. ? You have committed all pending changes to master
3. ? You understand this is a **BREAKING CHANGE**
4. ? You are prepared to update all downstream code
5. ? You have allocated 4-5 days for full refactor + testing

**PROCEED?** If yes, create feature branch and begin Phase 1.

---

**Last Updated:** January 17, 2025  
**Next Review:** After Phase 8 completion  
**Owner:** EasyAF Development Team

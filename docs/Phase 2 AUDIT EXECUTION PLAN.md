# Phase 2: Library Audit & Cleanup - EXECUTION PLAN

## Overview
Systematic audit and cleanup of the 4 newly migrated libraries (33 source files) to ensure high-quality, well-documented code before Phase 3.

---

## ? Audit Strategy

### Session 1: Quick Wins & Critical Items (THIS SESSION)
**Goal**: Get mapping-critical files documented ASAP  
**Time**: 1-2 hours  
**Files**: 3-5 high-priority files

**Priority Order:**
1. ??? `MappingConfig.cs` - Critical for mapping module
2. ??? `MappingEntry.cs` - Core mapping data structure  
3. ?? `ImportManager.cs` - Understanding the import workflow
4. ?? `DataSet.cs` - Understanding data structure
5. ? `DiffUtil.cs` - Helpful for mapping preview

### Session 2: Data Models (NEXT)
**Goal**: Complete all 13 data models  
**Time**: 2-3 hours  
**Files**: ArcFlash, Cable, Fuse, LVCB, ShortCircuit, TripUnit, Project, etc.

### Session 3: Engine Files (LATER)
**Goal**: Document spec engine  
**Time**: 2-3 hours  
**Files**: 9 engine files

### Session 4: Export Files (LATER)
**Goal**: Document export/templating  
**Time**: 1-2 hours  
**Files**: 4 export files

---

## ? Session 1: Detailed Plan

### File 1: MappingConfig.cs ???
**Current State**: Minimal documentation  
**Goal**: Comprehensive documentation with examples

**Tasks:**
- [ ] Add class-level summary with purpose
- [ ] Add detailed `<remarks>` with:
  - File format (.ezmap JSON structure)
  - Validation workflow explanation
  - PascalCase enforcement rationale
  - Usage scenarios
- [ ] Document all properties with examples
- [ ] Add `Validate()` method documentation with error examples
- [ ] Add code example for typical usage
- [ ] Document serialization considerations
- [ ] Add thread-safety notes

**Deliverables:**
- Fully documented `MappingConfig.cs`
- Sample `.ezmap` file created in `docs/samples/`
- Usage guide in comments

---

### File 2: MappingEntry.cs ???
**Current State**: Basic documentation  
**Goal**: Clear explanation of each property

**Tasks:**
- [ ] Add class-level summary
- [ ] Document all properties:
  - `TargetType` - which model class (Bus, ArcFlash, etc.)
  - `PropertyName` - which property on that class
  - `ColumnHeader` - source column name
  - `Required` - validation flag
  - `Severity` - error vs warning
  - `DefaultValue` - fallback value
  - `Aliases` - alternative column names
- [ ] Add property relationship documentation
- [ ] Add validation rules for each property
- [ ] Add usage examples

**Deliverables:**
- Fully documented `MappingEntry.cs`
- Clear understanding of mapping entry structure

---

### File 3: ImportManager.cs ??
**Current State**: Unknown  
**Goal**: Understand import orchestration

**Tasks:**
- [ ] Review current implementation
- [ ] Add class-level documentation
- [ ] Document import workflow (step-by-step)
- [ ] Document how MappingConfig is applied
- [ ] Add error handling documentation
- [ ] Document logging integration
- [ ] Add usage examples

**Deliverables:**
- Documented import workflow
- Understanding of data flow

---

### File 4: DataSet.cs ??
**Current State**: Unknown  
**Goal**: Understand data container

**Tasks:**
- [ ] Add class-level documentation
- [ ] Document dictionary structures
- [ ] Document composite key format (for ArcFlash, ShortCircuit)
- [ ] Add Diff() method documentation
- [ ] Performance notes for large datasets
- [ ] Add usage examples

**Deliverables:**
- Clear understanding of data structure
- Diff workflow documented

---

### File 5: DiffUtil.cs ?
**Current State**: Unknown  
**Goal**: Understand comparison utilities

**Tasks:**
- [ ] Add class-level documentation
- [ ] Document generic methods
- [ ] Add performance notes
- [ ] Add thread-safety notes
- [ ] Add usage examples

**Deliverables:**
- Documented diff utility
- Understanding of property comparison

---

## ? Code Quality Checklist (All Files)

For each file reviewed, check:

### Documentation
- [ ] Class has `<summary>` and `<remarks>`
- [ ] All public properties have `<summary>`
- [ ] All public methods have `<summary>`, `<param>`, `<returns>`
- [ ] Complex logic has inline comments
- [ ] Non-obvious decisions have `// NOTE:` comments
- [ ] TODOs are captured with `// TODO:`

### Nullable Annotations
- [ ] Properties correctly marked with `?` where appropriate
- [ ] Method parameters checked for null
- [ ] Return types correctly annotated
- [ ] No suppressed nullable warnings without justification

### Code Style
- [ ] Consistent naming (PascalCase for public, camelCase for private)
- [ ] No unused usings
- [ ] Consistent indentation
- [ ] No magic strings or numbers (use constants)
- [ ] Regions used appropriately (if at all)

### Performance
- [ ] No obvious LINQ inefficiencies
- [ ] String operations optimized
- [ ] Collection initialization appropriate
- [ ] Lazy loading where beneficial

---

## ? Sample .ezmap File Structure

Create this file: `docs/samples/sample-mapping.ezmap`

```json
{
  "SoftwareVersion": "3.0.0",
  "MapVersion": "1.0",
  "ImportMap": [
    {
      "TargetType": "Bus",
      "PropertyName": "Id",
      "ColumnHeader": "Bus ID",
      "Required": true,
      "Severity": "Error",
      "Aliases": ["BusId", "Bus_ID", "ID"]
    },
    {
      "TargetType": "Bus",
      "PropertyName": "Name",
      "ColumnHeader": "Bus Name",
      "Required": true,
      "Severity": "Error"
    },
    {
      "TargetType": "Bus",
      "PropertyName": "Voltage",
      "ColumnHeader": "Nominal Voltage",
      "Required": false,
      "Severity": "Warning",
      "DefaultValue": "480V"
    }
  ]
}
```

---

## ? Documentation Template

Use this template for each class:

```csharp
/// <summary>
/// [One-line description of the class purpose]
/// </summary>
/// <remarks>
/// <para>
/// [Detailed explanation of the class purpose and usage]
/// </para>
/// <para>
/// [Additional context: file formats, workflows, relationships]
/// </para>
/// <para>
/// [Performance, thread-safety, or other important notes]
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Typical usage example
/// var config = new MappingConfig
/// {
///     SoftwareVersion = "3.0.0",
///     ImportMap = new List&lt;MappingEntry&gt;
///     {
///         new() { TargetType = "Bus", PropertyName = "Id", ColumnHeader = "Bus ID" }
///     }
/// };
/// 
/// var errors = config.Validate();
/// if (errors.Any()) 
/// {
///     // Handle validation errors
/// }
/// </code>
/// </example>
public class MappingConfig
{
    // ...
}
```

---

## ? Success Criteria for Session 1

Session 1 is complete when:

- [ ] `MappingConfig.cs` has comprehensive XML documentation
- [ ] `MappingEntry.cs` has complete property documentation
- [ ] Sample `.ezmap` file created
- [ ] Import workflow understood and documented
- [ ] DataSet structure documented
- [ ] All 5 files build without warnings
- [ ] No nullable reference warnings in reviewed files
- [ ] Audit results documented

---

## ? Next Steps After Session 1

1. Update Phase 2 checklist with completed items
2. Commit changes with message: `docs: complete Session 1 of library audit (mapping-critical files)`
3. Decide: Continue with Session 2 (data models) or proceed to Phase 3?

---

**Session 1 Start**: [TIMESTAMP]  
**Session 1 End**: [TIMESTAMP]  
**Files Completed**: 0 / 5  
**Status**: ? Ready to Begin

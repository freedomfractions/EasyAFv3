# EasyAF Library Migration Summary

## Overview
This document summarizes the migration of core backend libraries from the sandbox (`EasyAF-stopgap`) to the new .NET 8.0 EasyAFv3 solution structure.

**Migration Date**: 2024  
**Source**: `sandbox\EasyAF-stopgap\` (.NET 6.0)  
**Target**: `lib\` (.NET 8.0)  
**Status**: ? **Complete and Building Successfully**

---

## Migrated Libraries

### 1. **EasyAF.Data** (`lib\EasyAF.Data\`)
**Namespace**: `EasyAF.Data.Models`  
**Purpose**: Core data models and structures for electrical equipment and project data  
**Source**: `EasyAFLib`

#### Files Migrated (13 files):
- **ArcFlash.cs** - Arc flash study data model with incident energy calculations
- **Bus.cs** - Electrical bus/switchgear equipment model (extensively documented)
- **Cable.cs** - Cable/conductor equipment model
- **DataSet.cs** - Container for all project data types with diff capabilities
- **DiffModels.cs** - Models for representing changes between datasets
- **DiffUtil.cs** - Utility methods for comparing objects and generating diffs
- **Fuse.cs** - Fuse protection device model
- **GlobalSettings.cs** - Application-wide settings and configuration
- **LVCB.cs** - Low-voltage circuit breaker model
- **Project.cs** - Top-level project container with metadata
- **ShortCircuit.cs** - Short circuit study data model
- **TripUnit.cs** - Circuit breaker trip unit settings model
- **VersionUtil.cs** - Version comparison and validation utilities

#### Key Features:
- All properties are strings to preserve source data fidelity
- Extensive XML documentation with original column name mappings
- Diff/comparison capabilities for tracking data changes
- Support for complex composite keys (e.g., `(Id, Scenario)` for ArcFlash)

#### Dependencies:
- `Newtonsoft.Json` (v13.0.3) - JSON serialization

---

### 2. **EasyAF.Import** (`lib\EasyAF.Import\`)
**Namespace**: `EasyAF.Import`  
**Purpose**: Data import functionality with flexible mapping configuration  
**Source**: `EasyAFImportLib`

#### Files Migrated (7 files):
- **MappingConfig.cs** - Core mapping configuration model for `.ezmap` files
  - Defines how external file columns map to internal data properties
  - Validation logic for duplicate/missing mappings
  - PascalCase enforcement for JSON field names
  - Support for aliases, default values, and severity levels
- **CsvImporter.cs** - CSV file import implementation
- **ExcelImporter.cs** - Excel file import implementation  
- **IImporter.cs** - Common importer interface
- **ImportManager.cs** - Orchestrates import process using mapping configs
- **ImportOptions.cs** - Configuration options for import operations
- **Logger.cs** - Import-specific logging functionality

#### Key Features:
- **MappingEntry** model with:
  - `TargetType` - Data model class name (e.g., "Bus", "ArcFlash")
  - `PropertyName` - Property name within that class
  - `ColumnHeader` - Source file column name to map from
  - `Required`, `Aliases`, `Severity`, `DefaultValue` fields
- Validation ensures no duplicate mappings and all required fields present
- Support for loading/saving `.ezmap` files with strict validation

#### Dependencies:
- `Newtonsoft.Json` (v13.0.3) - JSON serialization
- `EPPlus` (v7.0.0) - Excel file handling
- References: `EasyAF.Data`

---

### 3. **EasyAF.Engine** (`lib\EasyAF.Engine\`)
**Namespace**: `EasyAF.Engine`  
**Purpose**: Table specification engine for report/label generation  
**Source**: `EasyAFEngine`

#### Files Migrated (9 files):
- **EasyAFEngine.cs** - Main engine orchestrating table evaluation
- **Exceptions.cs** - Custom exception types for spec processing
- **ExpressionCompiler.cs** - Compiles and caches expressions for performance
- **JsonSpec.cs** - JSON-based specification model
- **ProjectContext.cs** - Runtime context for table evaluation
- **SimpleExpressionEvaluator.cs** - Expression evaluation engine
- **SpecLoader.cs** - Loads and validates table specifications
- **TableDefinition.cs** - Defines structure and content of output tables
- **TableEvaluationResult.cs** - Results from evaluating a table spec

#### Key Features:
- Expression-based column definitions
- Context-aware evaluation with access to project data
- Support for custom functions and data lookups
- Table generation from data sets

#### Dependencies:
- `Newtonsoft.Json` (v13.0.3) - JSON serialization
- References: `EasyAF.Data`

---

### 4. **EasyAF.Export** (`lib\EasyAF.Export\`)
**Namespace**: `EasyAF.Export`  
**Purpose**: Export functionality for generating reports and labels  
**Source**: `EasyAFExportLib`

#### Files Migrated (4 files + templates):
- **BreakerLabelGenerator.cs** - Generates circuit breaker labels from templates
- **DocxTemplate.cs** - DOCX template processing and tag replacement
- **EquipmentDutyLabelGenerator.cs** - Generates equipment duty labels
- **TemplateResources.cs** - Manages embedded template resources

#### Template Files (Embedded Resources):
- `Breaker.dotx` - Circuit breaker label template
- `Equip.dotx` / `EquipOD.dotx` - Equipment duty label templates
- `ATT Arc Flash Study Template Rev 2024-01-05.docx` - Report template
- `EasyPower_Title_Block.xsd` - Title block schema
- `TitleBlockTemplate.xml` - Title block template

#### Key Features:
- DOCX template-based output generation
- Tag replacement engine for populating templates
- Label printing with data from spec evaluation
- Embedded template resources for portability

#### Dependencies:
- `DocumentFormat.OpenXml` (v2.20.0) - DOCX manipulation (version locked for compatibility)
- References: `EasyAF.Data`, `EasyAF.Engine`

---

## Namespace Migration Map

| Original Namespace | New Namespace |
|-------------------|---------------|
| `EasyAFLib` | `EasyAF.Data.Models` |
| `EasyAFImportLib` | `EasyAF.Import` |
| `EasyAFEngine` | `EasyAF.Engine` |
| `EasyAFExportLib` | `EasyAF.Export` |

---

## Project Configuration

All projects updated with:
- ? **Target Framework**: .NET 8.0
- ? **Nullable Reference Types**: Enabled
- ? **Implicit Usings**: Enabled
- ? **XML Documentation**: Enabled (`GenerateDocumentationFile=true`)
- ? **Authors & Description**: Populated

---

## Build Status

? **All projects build successfully with no errors**

---

## Next Steps (Phase 2: Documentation & Cleanup)

### High Priority
1. **Enhance XML Documentation**
   - Add missing `<param>`, `<returns>`, `<exception>` tags
   - Expand `<summary>` descriptions for complex methods
   - Document threading/async behavior where applicable
   
2. **Nullable Reference Type Annotations**
   - Review all nullable annotations for correctness
   - Add `[NotNull]`, `[MaybeNull]` attributes where appropriate
   - Fix any nullable warnings

3. **Code Modernization**
   - Replace tuple syntax with modern ValueTuple where beneficial
   - Consider pattern matching improvements
   - Use collection expressions (C# 12) where appropriate

### Medium Priority
4. **Dependency Review**
   - Evaluate migration from `Newtonsoft.Json` to `System.Text.Json`
   - Update EPPlus to latest stable if needed
   - Document why DocumentFormat.OpenXml is pinned to 2.20.0

5. **Performance Optimization**
   - Profile diff operations for large datasets
   - Consider span/memory optimizations for file parsing
   - Cache compiled expressions in ExpressionCompiler

### Lower Priority
6. **Testing**
   - Migrate unit tests from sandbox (if applicable)
   - Add integration tests for mapping workflows
   - Test template rendering with sample data

7. **Refactoring Opportunities**
   - Consider separating Models folder into subfolders (Equipment, Studies, etc.)
   - Extract interfaces for better testability
   - Consider dependency injection patterns

---

## Critical Notes for Mapping Module Development

### MappingConfig Structure
The mapping module UI will primarily interact with `EasyAF.Import.MappingConfig`:

```csharp
public class MappingConfig
{
    public string SoftwareVersion { get; set; }
    public string? MapVersion { get; set; }
    public List<MappingEntry> ImportMap { get; set; }
}

public class MappingEntry
{
    public string TargetType { get; set; }      // e.g., "Bus", "ArcFlash"
    public string PropertyName { get; set; }     // e.g., "Id", "Voltage"
    public string ColumnHeader { get; set; }     // Source column name
    public bool Required { get; set; }
    public string[]? Aliases { get; set; }
    public MappingSeverity Severity { get; set; }
    public string? DefaultValue { get; set; }
}
```

### Key Classes to Reference
- **Data Models**: All classes in `EasyAF.Data.Models` namespace can be mapping targets
- **Reflection**: Use `Type.GetProperties()` to dynamically discover mappable properties
- **Validation**: Use `MappingConfig.Validate()` to check for errors before saving

### File Format
Mapping files (`.ezmap`) are JSON:
```json
{
  "SoftwareVersion": "9.6",
  "MapVersion": "1.0",
  "ImportMap": [
    {
      "TargetType": "Bus",
      "PropertyName": "Id",
      "ColumnHeader": "Bus Name",
      "Required": true,
      "Severity": "Error"
    },
    // ... more entries
  ]
}
```

---

## Notes on Modularity

**Cross-Library Dependencies** (documented per prompt requirements):
- ? `EasyAF.Import` references `EasyAF.Data` - Required for accessing data models during import
- ? `EasyAF.Engine` references `EasyAF.Data` - Required for table evaluation context
- ? `EasyAF.Export` references both `EasyAF.Data` and `EasyAF.Engine` - Required for rendering

These are intentional and documented dependencies that follow the natural data flow:
**Data Models ? Import/Engine ? Export**

---

## Sandbox Reference

The original code remains in `sandbox\EasyAF-stopgap\` for reference purposes:
- **Do not modify sandbox code** - It's read-only reference material
- **Do not ship sandbox** - Excluded from deployment
- Used for comparing behavior or understanding design decisions

---

**End of Migration Summary**

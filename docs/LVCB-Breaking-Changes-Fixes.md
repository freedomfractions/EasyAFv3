# LVCB TripUnit Flattening - Breaking Changes Fix List

**Status:** In Progress  
**Date:** January 17, 2025

## Changes Made:
1. ? LVCB.cs - Removed nested TripUnit property, flattened all to TripUnit* properties
2. ? TripUnit.cs - Deleted (no longer needed)
3. ? Project.cs - Updated NormalizeManufacturers to use flattened properties

## Files Still Needing Fixes:

### lib/EasyAF.Engine/ProjectContext.cs (Line 31)
**Issue:** References `lvcb.TripUnit.Type` and `lvcb.TripUnit.Style`  
**Fix:** Change to `lvcb.TripUnitType` and `lvcb.TripUnitStyle`

### lib/EasyAF.Import/CsvImporter.cs (Line 174-176)
**Issue:** Creates `new TripUnit()` and assigns to `lvcb.TripUnit`  
**Fix:** Import directly to `TripUnit*` properties on LVCB (no nested object)

### lib/EasyAF.Import/ExcelImporter.cs (Line 77)
**Issue:** Creates `new TripUnit()` and assigns to `lvcb.TripUnit`  
**Fix:** Import directly to `TripUnit*` properties on LVCB (no nested object)

### lib/EasyAF.Export/BreakerLabelGenerator.cs (Line 135)
**Issue:** Method signature uses `TripUnit` parameter  
**Fix:** Change to accept `LVCB` parameter instead, access flattened properties

## Strategy:
- Engine: Simple property rename
- Import (CSV/Excel): Remove TripUnit object creation, map directly to LVCB.TripUnit* properties
- Export: Refactor to work with LVCB directly instead of nested TripUnit

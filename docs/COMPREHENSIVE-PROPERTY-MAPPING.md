# Comprehensive Property Mapping for ALL Models
# OLD property names ? NEW generated model property names

## ShortCircuit Model
- Bus ? BusName
- DutyKA ? (need to check actual property name)
- Device ? EquipmentName

## All Models - First Property Pattern
- Old: Same as class name (e.g., LVCB.Id, Load.Id)
- New: Pluralized (e.g., LVBreakers, Loads)

## Solution: 
1. Check all generated models for actual property names
2. Update all code references to use new names
3. Add legacy compatibility layers where needed

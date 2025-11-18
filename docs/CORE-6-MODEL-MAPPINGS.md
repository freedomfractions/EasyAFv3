# CORE MODEL PROPERTY MAPPINGS (Old ? New)
# These 6 models have existing code dependencies that need updating

## 1. LVBreaker (formerly LVCB)
### ID Property
- LVCB ? LVBreakers (first property)
- Id ? LVBreakers (alias)

### Manufacturer Properties  
- Manufacturer ? BreakerMfr
- Style ? BreakerStyle
- TripUnitManufacturer ? TripMfr
- TripUnitType ? TripType
- TripUnitStyle ? TripStyle

### No changes needed (these stayed the same or don't exist in old model)
- Most other properties are new or don't have code references

## 2. ShortCircuit
### ID Properties
- Bus ? BusName
- Device/Equipment ? EquipmentName
- ShortCircuit (first prop) ? BusName (ID alias points to BusName)

### No other major code dependencies

## 3. ArcFlash
### ID Property
- Id ? ArcFaultBusName (first property)

### No other major code dependencies

## 4. Bus
### ID Property  
- Bus/Buses ? Buss (first property, yes it's "Buss" with double s)
- Id ? Buss

### No other major code dependencies

## 5. Fuse
### ID Property
- Fuse/Fuses ? Fuses (first property)
- Id ? Fuses

### No major code dependencies

## 6. Cable
### ID Property
- Cable/Cables ? Cables (first property)  
- Id ? Cables

### No major code dependencies

## FILES TO UPDATE:
1. ? Project.cs - DONE (BreakerMfr, TripMfr, etc.)
2. ? BreakerLabelGenerator.cs - DONE (LVBreaker properties)
3. ? ProjectContext.cs - DONE (FormatBreakerSummary)
4. EquipmentDutyLabelGenerator.cs - ShortCircuit.BusName
5. Any tests or UI code

# DataSet Expansion Plan - All 34 Equipment Models

## Current State (12 equipment types)
1. ? ArcFlash (composite key)
2. ? ShortCircuit (composite key)
3. ? LVBreaker
4. ? Fuse
5. ? Cable
6. ? Bus
7. ? Transformer2W
8. ? Motor
9. ? Generator
10. ? Utility
11. ? Capacitor
12. ? Load

## Missing Equipment Types (22 to add)
### Switchgear & Distribution
13. Panel
14. MCC
15. Busway
16. TransmissionLine

### Breakers & Protection
17. HVBreaker
18. Relay
19. Switch
20. ATS

### Transformers
21. Transformer3W
22. ZigzagTransformer

### Current Sensors
23. CT
24. Meter
25. POC

### Reactive Components
26. Shunt
27. CLReactor
28. Filter

### Power Electronics
29. AFD
30. UPS
31. Inverter
32. Rectifier

### Renewables & Storage
33. Photovoltaic
34. Battery

## Implementation Strategy
1. Add dictionary properties in alphabetical order (keeps code organized)
2. Add Diff methods for each (clone pattern from existing)
3. Update DataSetPersist for JSON serialization
4. Update ProjectContext for UI access
5. Update EasyAFEngine for table routing

## Code Changes Required
- ? DataSet.cs - Add 22 new Dictionary properties
- ? DataSet.cs - Add 22 new Diff methods
- ? Project.cs (DataSetPersist) - Add 22 properties
- ? ProjectContext.cs - Add 22 accessor methods
- ?? EasyAFEngine.cs - Add routing for all 22 types (future)
- ?? CsvImporter.cs - Already handles dynamic types via reflection
- ?? ExcelImporter.cs - Already handles dynamic types via reflection

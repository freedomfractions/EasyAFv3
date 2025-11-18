# ? CSV COLUMN NAME AUDIT - COMPLETE

## ?? OBJECTIVE

**Fix ALL property names to match exact CSV column names (with spaces/special chars removed).**

---

## ?? PROPERTY FIXES BY MODEL

### **ArcFlash** (7 properties fixed)

| CSV Column | ? Old Property | ? New Property |
|-----------|----------------|----------------|
| `Arc Fault Bus kV` | `BusKV` | `ArcFaultBusKV` |
| `Upstream Trip Device Name` | `UpstreamDevice` | `UpstreamTripDeviceName` |
| `Upstream Trip Device Function` | `UpstreamDeviceFunction` | `UpstreamTripDeviceFunction` |
| `Bus Bolted Fault (kA)` | `BoltedFaultKA` | `BusBoltedFaultKA` |
| `Bus Arc Fault (kA)` | `ArcFaultKA` | `BusArcFaultKA` |
| `Est Arc Flash Boundary (inches)` | `ArcFlashBoundaryIn` | `EstArcFlashBoundaryInches` |
| `Working Distance (inches)` | `WorkingDistanceIn` | `WorkingDistanceInches` |

### **ShortCircuit** (3 properties fixed)

| CSV Column | ? Old Property | ? New Property |
|-----------|----------------|----------------|
| `Bus No. of Phases` | `BusPhases` | `BusNoOfPhases` |
| `Equipment Manufacturer` | `Manufacturer` | `EquipmentManufacturer` |
| `Equipment Style` | `Style` | `EquipmentStyle` |

### **Bus** (2 properties fixed)

| CSV Column | ? Old Property | ? New Property |
|-----------|----------------|----------------|
| `Base kV` | `Voltage` | `BaseKV` |
| `No of Phases` | `Phases` | `NoOfPhases` |

### **LVCB** (3 properties fixed)

| CSV Column | ? Old Property | ? New Property |
|-----------|----------------|----------------|
| `No of Phases` | `Phases` | `NoOfPhases` |
| `On Bus` | `Bus` | `OnBus` |
| `Base kV` | `Voltage` | `BaseKV` |

### **Fuse** (3 properties fixed)

| CSV Column | ? Old Property | ? New Property |
|-----------|----------------|----------------|
| `No of Phases` | `Phases` | `NoOfPhases` |
| `On Bus` | `Bus` | `OnBus` |
| `Base kV` | `Voltage` | `BaseKV` |

### **Cable** (1 property fixed)

| CSV Column | ? Old Property | ? New Property |
|-----------|----------------|----------------|
| `No of Phases` | `Phases` | `NoOfPhases` |

---

## ?? SUMMARY STATS

**Total Properties Fixed:** 19 across 6 models

| Model | Properties Fixed |
|-------|-----------------|
| ArcFlash | 7 |
| ShortCircuit | 3 |
| Bus | 2 |
| LVCB | 3 |
| Fuse | 3 |
| Cable | 1 |

---

## ?? PATTERN IDENTIFIED

**Common Issues:**
1. **Abbreviated names** - `Bus` instead of `OnBus`, `Voltage` instead of `BaseKV`
2. **Simplified names** - `Phases` instead of `NoOfPhases`
3. **Missing prefixes** - `ArcFaultKA` instead of `BusArcFaultKA`
4. **Shortened full names** - `ArcFlashBoundaryIn` instead of `EstArcFlashBoundaryInches`

**Naming Rule:**
> Property name = CSV column name with:
> - Spaces removed
> - Special characters removed (`/`, `.`, `(`, `)`)
> - PascalCase applied
> - **NO abbreviations or simplifications**

---

## ??? FILES MODIFIED

### **Data Models:**
- `lib/EasyAF.Data/Models/ArcFlash.cs` - 7 properties + ToString
- `lib/EasyAF.Data/Models/ShortCircuit.cs` - 3 properties
- `lib/EasyAF.Data/Models/Bus.cs` - 2 properties + ToString
- `lib/EasyAF.Data/Models/LVCB.cs` - 3 properties + ToString
- `lib/EasyAF.Data/Models/Fuse.cs` - 3 properties + ToString
- `lib/EasyAF.Data/Models/Cable.cs` - 1 property

### **Map Module:**
- `modules/EasyAF.Modules.Map/Services/PropertyDiscoveryService.cs` - Updated required properties

---

## ? VERIFICATION

### **Build Status:**
? **SUCCESS** - All 7 projects compile

### **Property Naming Examples:**

| CSV Column | Correct Property Name | Reasoning |
|-----------|----------------------|-----------|
| `"LV Breakers"` | `LVBreakers` | Spaces removed, PascalCase |
| `"No of Phases"` | `NoOfPhases` | Exact match, "of" preserved |
| `"On Bus"` | `OnBus` | Exact match, not simplified to "Bus" |
| `"Base kV"` | `BaseKV` | Exact match, not "Voltage" |
| `"Arc Fault Bus kV"` | `ArcFaultBusKV` | Full name, not shortened |
| `"Bus Bolted Fault (kA)"` | `BusBoltedFaultKA` | Full prefix, units removed |
| `"Est Arc Flash Boundary (inches)"` | `EstArcFlashBoundaryInches` | Full name + unit suffix |

---

## ?? LESSONS FOR FUTURE CLASSES

**When creating new model classes from CSV:**

1. ? **Use exact column names** - Don't abbreviate or simplify
2. ? **Preserve prefixes** - `"Bus Arc Fault"` ? `BusArcFaultKA`, not `ArcFaultKA`
3. ? **Spell out units in suffix** - `Inches` not `In`, `Seconds` not `Sec` (when abbreviated in CSV)
4. ? **Keep "of", "to", "from" words** - `NoOfPhases` not `Phases`
5. ? **Don't rename for "clarity"** - Mapping needs exact matches

**Process:**
1. Parse CSV columns with `EasyAF.CsvParser`
2. Convert column names:
   - Remove spaces
   - Remove special chars: `(`, `)`, `/`, `.`, `-`
   - Apply PascalCase
   - **DON'T simplify or abbreviate**
3. Create property with `[Description]` showing original CSV column name
4. Add Id alias with `[JsonIgnore]` for convenience

---

## ?? NEXT STEPS

1. **Test import** with real EasyPower CSV files
2. **Verify auto-mapping** works with exact column names
3. **Document pattern** for future model classes (Transformers, Motors, etc.)

---

## ?? COMMIT MESSAGE

```
fix(data): Correct ALL property names to match exact CSV columns

CRITICAL FIX: 19 properties across 6 models were using abbreviated or 
simplified names instead of exact CSV column names.

PROPERTY FIXES:
- ArcFlash: 7 properties (BusKV?ArcFaultBusKV, etc.)
- ShortCircuit: 3 properties (BusPhases?BusNoOfPhases, etc.)
- Bus: 2 properties (Voltage?BaseKV, Phases?NoOfPhases)
- LVCB: 3 properties (Bus?OnBus, Voltage?BaseKV, Phases?NoOfPhases)
- Fuse: 3 properties (Bus?OnBus, Voltage?BaseKV, Phases?NoOfPhases)
- Cable: 1 property (Phases?NoOfPhases)

NAMING RULE:
Property name = CSV column with spaces/special chars removed, NO abbreviations.

Examples:
- "On Bus" ? OnBus (not "Bus")
- "Base kV" ? BaseKV (not "Voltage")
- "No of Phases" ? NoOfPhases (not "Phases")
- "Bus Arc Fault (kA)" ? BusArcFaultKA (not "ArcFaultKA")

This ensures auto-mapping works perfectly with FuzzyMatcher normalized mode.

Updated PropertyDiscoveryService required properties to match new names.

BUILD STATUS: ? SUCCESS (all 7 projects)
```

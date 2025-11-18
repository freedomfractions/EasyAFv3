# EasyPower CSV Parse Results

**Generated:** January 17, 2025  
**Source:** `easypower fields.csv`  
**Tool:** `tools/EasyAF.CsvParser`

---

## Core 6 Classes - Column Counts

| EasyAF Class | EasyPower Name | Property Count |
|--------------|----------------|----------------|
| **ArcFlash** | Arc Flash Scenario Report | **18** |
| **Bus** | Buses | **63** |
| **Cable** | Cables | **91** |
| **Fuse** | Fuses | **52** |
| **LVCB** | LV Breakers | **97** |
| **ShortCircuit** | Equipment Duty Scenario Report | **15** |
| **TOTAL** | | **336 properties** |

---

## Comparison to Current Model (v0.1.0)

| Class | Current Properties | New Properties | Change |
|-------|-------------------|----------------|--------|
| ArcFlash | 20 | 18 | -2 (cleanup) |
| Bus | 60 | 63 | +3 |
| Cable | 20 | 91 | +71 (**4.5x**) |
| Fuse | 50 | 52 | +2 |
| LVCB | 95 | 97 | +2 |
| ShortCircuit | 17 | 15 | -2 (cleanup) |
| **TOTAL** | **262** | **336** | **+74 (+28%)** |

---

## Next Steps

1. ? Parse CSV (COMPLETE)
2. Normalize column names to PascalCase
3. Map to existing properties where possible
4. Add missing properties
5. Add attributes (Category, Units, Description, Required)
6. Test imports

---

## Full Column Lists

### ArcFlash (18 properties)
```
Arc Fault Bus Name
Worst Case
Scenario
Arc Fault Bus kV
Upstream Trip Device Name
Upstream Trip Device Function
Equip Type
Electrode Configuration
Electrode Gap (mm)
Bus Bolted Fault (kA)
Bus Arc Fault (kA)
Trip Time (sec)
Opening Time (sec)
Arc Time (sec)
Est Arc Flash Boundary (inches)
Working Distance (inches)
Incident Energy (cal/cm2)
Comments
```

### Bus (63 properties)
```
Buses
AC/DC
Status
Base kV
No of Phases
Service
Area
Zone
Device Code
Manufacturer
Type
Bus Rating (A)
Bus Bracing (kA)
Test Standard
Material
Mounting
Dn Conn kVA
Dn Conn FLA
Dn Demand kVA
Dn Demand FLA
Dn Code kVA
Dn Code FLA
Equipment
AF Solution
Forced To Energy (cal/cm2)
Forced To Arc Boundary (in)
AF Option
AF Output
Working Distance Setting
Working Distance (in)
Electrode Gap Setting
Electrode Gap (mm)
Electrode Configuration Setting
Electrode Configuration
Enclosure Size Setting
Enclosure Height (in)
Enclosure Width (in)
Enclosure Depth (in)
Stored AFB (in)
Stored AF IE (cal/cm2)
Stored AF PPE
SC Sym kA (ANSI)
SC Sym kA (IEC)
Label Comment
# Labels To Print
Failure Rate (/year)
Repair Time (h)
Replace Time (h)
Repair Cost
Replace Cost
Action Upon Failure
Downtime Cost (h)
Reliability Source
Reliability Category
Reliability Class
Facility
Location Name
Location Description
X
Y
Floor
Data Status
Comment
```

### Cable (91 properties)
```
Cables
AC/DC
Status
Conn Type
No of Phases
From ID
From Bus
To ID
To Bus
Rated kVA
Base kV
C.E. Applied
Length
Length Unit
Derating Factor
Total Equiv Length
Use Load FLA for Ampacity
Cables in Parallel
Phase/Neutral
Size
# Runs
Material
Insulation
Duct/Air
Spacing
L-L Spacing (in)
L-G Spacing (in)
Cable Mfr
Cable Type
Cable Description
Nonmetallic Jacket
# Sets
# Conductors
Temp Rating (C)
Lay Type
Cable Constant
Impedance Type
Nom Cross Section (kcmil)
Nom Conductor OD (in)
Nom Cable OD (in)
Resistance (ohms/ft)
GMR (ft)
Reactance @ 1 ft (ohms/mile)
Reactance @ 60 Hz (ohms/ft)
Ampacity (A)
Neutral
Neutral Size
# Neutral Runs
Neutral Material
Neutral Insulation
Neutral Cable Mfr
Neutral Cable Type
Neutral Cable Description
Neutral Nonmetallic Jacket
Neutral # Sets
Neutral # Conductors
Neutral Temp Rating (C)
Neutral Lay Type
Neutral Cable Constant
Neutral Impedance Type
Neutral Nom Cross Section (kcmil)
Neutral Nom Conductor OD (in)
Neutral Nom Cable OD (in)
Neutral Resistance (ohms/ft)
Neutral GMR (ft)
Neutral Reactance @ 1 ft (ohms/mile)
Neutral Reactance @ 60 Hz (ohms/ft)
Neutral Ampacity (A)
Ground
Ground Size
# Ground Runs
Ground Material
Ground Insulation
Raceway Type
Raceway Material
Conduit Size
# Conduits
Free Air
% Fill
Length Note
Show Symbol
Data Status
Comment
```

### Fuse (52 properties)
```
Fuses
AC/DC
Status
No of Phases
On Bus
Base kV
Conn Type
Standard
Normal State
Options
Fuse Mfr
Fuse Type
Fuse Style
Model
TCC kV
Size
SC Int kA
SC Test X/R
SC Test Std
TCC Clipping
TCC Mom kA
TCC Int kA
TCC 30 Cyc kA
IEC Breaking kA
IEC TCC Initial kA
IEC TCC Breaking kA
IEC TCC Breaking Time
IEC TCC SS kA
Switch Manufacturer
Switch Type
Switch Style
Switch Cont A
Switch Mom kA
Mtr O/L Mfr
Mtr O/L Type
Mtr O/L Style
Motor FLA
Service Factor
PCC kVA Demand
PCC Isc/ILoad
Failure Rate (/year)
Repair Time (h)
Replace Time (h)
Repair Cost
Replace Cost
Action Upon Failure
Reliability Source
Reliability Category
Reliability Class
SC Failure Mode %
Data Status
Comment
```

### LVCB (97 properties)
```
LV Breakers
AC/DC
Status
No of Phases
On Bus
Base kV
Conn Type
Class
Options
Breaker Mfr
Breaker Type
Breaker Style
Cont Current (A)
Frame (A)
ST ZSI
ST ZSI I2T
ST ZSI Delay
Inst ZSI
Gnd ZSI
Gnd ZSI I2T
Gnd ZSI Delay
Self Restrain
T-ZSI
Fuse Mfr
Fuse Type
Fuse Style
Fuse Size
Mtr O/L Mfr
Mtr O/L Type
Mtr O/L Style
Motor FLA
Service Factor
Standard
SC Rating Based On
SC Int kA
IEC Breaking kA
SC Test Std
TCC Clipping
TCC Mom kA
TCC Int kA
TCC 30 Cyc kA
TCC Gnd Mom kA
TCC Gnd Int kA
TCC Gnd 30 Cyc kA
IEC TCC Initial kA
IEC TCC Breaking kA
IEC TCC Breaking Time
IEC TCC SS kA
IEC TCC Gnd Initial kA
IEC TCC Gnd Breaking kA
IEC TCC Gnd Breaking Time
IEC TCC Gnd SS kA
Normal State
PCC kVA Demand
PCC Isc/ILoad
Failure Rate (/year)
Repair Time (h)
Replace Time (h)
Repair Cost
Replace Cost
Action Upon Failure
Reliability Source
Reliability Category
Reliability Class
SC Failure Mode %
Data Status
Mfr
Adjustable
Type
Style
Trip Plug
Sensor Frame
LTPU
LTPU Mult
LTPU (A)
LTD Band
LTD Curve
Trip Adjust
Trip Pickup
STPU
STPU (A)
STD Band
STPU I2T
STD I2T
Inst
Inst Override
Inst (A)
Inst Override Pickup (A)
Maint
Maint Setting
Maint (A)
Gnd Sensor
GFPU
GFPU (A)
GFD
GFD I2T
Gnd Maint Pickup
Gnd (A)
Comment
```

### ShortCircuit (15 properties)
```
Bus Name
Equipment Name
Worst Case
Scenario
Fault Type
Vpu
Bus Base kV
Bus No. of Phases
Equipment Manufacturer
Equipment Style
Test Standard
1/2 Cycle Rating (kA)
1/2 Cycle Duty (kA)
1/2 Cycle Duty (%)
Comments
```

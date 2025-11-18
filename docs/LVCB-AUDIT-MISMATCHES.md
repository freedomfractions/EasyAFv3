# LVCB CSV Column Audit - Mismatches Found

## CSV Columns vs LVCB.cs Properties

### MISSING from LVCB.cs (in CSV but not in code):
1. **Trip** - Missing entirely!
2. **Trip Mfr** - We have "Mfr" but should be "Trip Mfr"
3. **Trip Type** - We have "Type" but should be "Trip Type"  
4. **Trip Style** - We have "Style" but should be "Trip Style"
5. **Plug/Tap/Trip** - Missing!
6. **LTPU Setting** - Missing!
7. **Trip (A)** - Missing!
8. **LT Curve** - We have "LTD Curve" but CSV is "LT Curve"
9. **STPU Setting** - Missing!
10. **STPU Band** - We have "STD Band" but should be "STPU Band"
11. **Inst Setting** - Missing!
12. **Inst Ovr Pickup (A)** - We have "Inst Override Pickup (A)" - close but different
13. **Maint Mode** - We have "Maint" but should be "Maint Mode"
14. **Gnd Pickup** - Missing!
15. **Gnd Delay** - Missing!
16. **Gnd I2T** - Missing!
17. **Gnd (A)** - Missing (we have it in trip unit but not as "Gnd (A)")
18. **Gnd Maint (A)** - We have it

### EXTRA in LVCB.cs (in code but not in CSV):
1. **Cont Current (A)** - Not in CSV!
2. **Adjustable** - Not in CSV!
3. **Trip Plug** - Not in CSV (CSV has "Plug/Tap/Trip")
4. **LTPU** - CSV has "LTPU Setting"
5. **LTPU (A)** - Not in CSV list
6. **LTD Curve** - CSV has "LT Curve"
7. **STPU** - CSV has "STPU Setting"
8. **STPU (A)** - Not explicitly in CSV
9. **STD Band** - CSV has "STPU Band"
10. **STD I2T** - Not in CSV
11. **Inst** - CSV has "Inst Setting"
12. **Inst Override** - CSV has just "Inst Override"
13. **Inst (A)** - CSV has "Inst (A)"
14. **Maint** - CSV has "Maint Mode"
15. **GFPU** - CSV has "Gnd Pickup"
16. **GFPU (A)** - Not in CSV
17. **GFD** - CSV has "Gnd Delay"
18. **GFD I2T** - CSV has "Gnd I2T"

### CRITICAL ISSUE:
**The LVCB model appears to be mixing trip unit property names with non-standard abbreviations!**

The CSV has:
- **Trip** (the trip unit rating/setting)
- **Trip Mfr**, **Trip Type**, **Trip Style** (trip unit properties)
- **Plug/Tap/Trip** (plug/tap/trip rating selector)
- **LTPU Setting**, **STPU Setting**, **Inst Setting** (pickup settings)
- **Trip (A)** (trip current in amps)
- **LT Curve** (NOT "LTD Curve")
- **STPU Band** (NOT "STD Band")
- **Gnd Pickup**, **Gnd Delay**, **Gnd I2T**, **Gnd (A)** (ground fault properties)

But our code has abbreviated/non-matching names!

## ACTION REQUIRED:
**COMPLETE REWRITE of LVCB.cs to match EXACT CSV columns** (all 97 properties)

# EasyAF Model Naming Conventions

**Strict naming rules established through comprehensive 34-model audit (1,844 properties).**

---

## ?? **CORE PRINCIPLES**

1. **CSV column names are source of truth**
2. **Preserve semantic meaning** (don't over-abbreviate)
3. **C# naming standards** (PascalCase, no special chars, no numeric prefixes)
4. **Consistency above all** (same pattern = same conversion)
5. **Units belong in property names** when they disambiguate

---

## ?? **UNIVERSAL PATTERNS**

### Pattern 1: Electrical Unit Abbreviations

**ALWAYS capitalize electrical unit abbreviations:**

| CSV | Property | ?/? |
|-----|----------|------|
| `Base kV` | `BaseKV` | ? |
| `Base kV` | `BasekV` | ? |
| `SC Int kA` | `SCIntKA` | ? |
| `SC Int kA` | `SCIntkA` | ? |
| `Rating (kVA)` | `RatingKVA` | ? |
| `Rating (kVA)` | `RatingkVA` | ? |

**Units:** `KV`, `KA`, `KVA`, `KW`, `MVA`, `MVAR`, `HP`, `VA`

### Pattern 2: Per-Unit (pu)

**Capitalize "Pu" in per-unit values:**

| CSV | Property | ?/? |
|-----|----------|------|
| `R1 pu` | `R1Pu` | ? |
| `R1 pu` | `R1pu` | ? |
| `MVAR pu` | `MVARPu` | ? |
| `MVAR pu` | `MVARpu` | ? |

### Pattern 3: Temperature Suffix

**Add "C" suffix for Celsius temperatures:**

| CSV | Property | ?/? |
|-----|----------|------|
| `Op Temp (C)` | `OpTempC` | ? |
| `Op Temp (C)` | `OpTemp` | ? |
| `Ambient Temp (C)` | `AmbientTempC` | ? |
| `IEC Field Temp (C)` | `IECFieldTempC` | ? |

### Pattern 4: Time Suffixes

**Preserve time unit in property name:**

| CSV | Property | ?/? |
|-----|----------|------|
| `Trip Time (sec)` | `TripTimeSec` | ? |
| `Trip Time (sec)` | `TripTime` | ? |
| `Repair Time (h)` | `RepairTimeH` | ? |
| `Failure Rate (/year)` | `FailureRatePerYear` | ? |

### Pattern 5: Distance/Length Suffixes

**Add unit suffix for distances:**

| CSV | Property | ?/? |
|-----|----------|------|
| `Working Distance (inches)` | `WorkingDistanceInches` | ? |
| `Working Distance (in)` | `WorkingDistanceIn` | ? |
| `Electrode Gap (mm)` | `ElectrodeGapMM` | ? |

### Pattern 6: Current Suffixes

**Add "A" for amperage when needed:**

| CSV | Property | ?/? |
|-----|----------|------|
| `Rating (A)` | `RatingA` | ? |
| `Trip (A)` | `TripA` | ? |
| `Frame (A)` | `FrameA` | ? |
| `Cont Current (A)` | `ContCurrentA` | ? |

### Pattern 7: Energy Units

**Preserve complex energy units:**

| CSV | Property | ?/? |
|-----|----------|------|
| `Incident Energy (cal/cm2)` | `IncidentEnergyCalPerCm2` | ? |
| `Forced To Energy (cal/cm2)` | `ForcedToEnergyCalPerCm2` | ? |

---

## ?? **SPECIAL CASES**

### AC/DC

**Use mixed case:**

```csharp
public string? AcDc { get; set; }  // ? Correct
public string? ACDC { get; set; }  // ? Wrong
```

**Rationale:** Follows C# naming for acronyms that are pronounceable

### "No of" / "No." Pattern

**Capitalize "Of":**

| CSV | Property | ?/? |
|-----|----------|------|
| `No of Phases` | `NoOfPhases` | ? |
| `No of Phases` | `NoofPhases` | ? |
| `No. of CTs` | `NoOfCTs` | ? |

### "One-line" Compounds

**Preserve word boundaries:**

| CSV | Property | ?/? |
|-----|----------|------|
| `One-line Graphics` | `OneLineGraphics` | ? |
| `One-line Graphics` | `OnelineGraphics` | ? |

### Slash Separators

**Remove slash, capitalize both parts:**

| CSV | Property | ?/? |
|-----|----------|------|
| `X/R` | `XRRatio` | ? (semantic) |
| `No/Ph` | `NoPerPh` | ? (semantic) |
| `HP/kVA` | `HPKVA` | ? |
| `L-R (Ohms)` | `LROhms` | ? |

---

## ?? **DUPLICATE HANDLING**

### When Same Name, Different Units

**Add unit suffix to BOTH properties:**

| CSV Columns | Properties | ?/? |
|-------------|-----------|------|
| `C1 (MVAR)`, `C1 (kV)` | `C1MVAR`, `C1KV` | ? |
| `C1 (MVAR)`, `C1 (kV)` | `C1` (both) | ? Duplicate! |

**Examples from real models:**

```csharp
// Filter model:
public string? C1MVAR { get; set; }
public string? C1KV { get; set; }
public string? C2MVAR { get; set; }
public string? C2KV { get; set; }

// Filter model - per-unit variants:
public string? GpuR { get; set; }     // G pu (R)
public string? GpuL { get; set; }     // G pu (L)
public string? MVARpuC1 { get; set; } // MVAR pu (C1)
public string? MVARpuC2 { get; set; } // MVAR pu (C2)
```

### When Same Name, Different Standards

**Add standard suffix:**

| CSV Columns | Properties | ?/? |
|-------------|-----------|------|
| `SC Sym kA (ANSI)`, `SC Sym kA (IEC)` | `SCSymKAAnsi`, `SCSymKAIec` | ? |

**Example:**
```csharp
// Bus model:
public string? SCSymKAAnsi { get; set; }
public string? SCSymKAIec { get; set; }

// ATS model:
public string? SCSymKAAnsi { get; set; }
public string? SCSymKAIec { get; set; }
```

### When Same Base, Different Roles

**Add context suffix:**

| CSV Columns | Properties | ?/? |
|-------------|-----------|------|
| `Trip` (selector), `Trip (A)` (current) | `Trip`, `TripA` | ? |

**Example:**
```csharp
// LVCB model:
public string? Trip { get; set; }      // Trip unit selector
public string? TripA { get; set; }     // Trip current in amps
```

---

## ?? **NUMERIC PREFIX HANDLING**

**C# doesn't allow identifiers starting with numbers.**

### Pattern: Spell Out Fractions

| CSV | Property | ?/? |
|-----|----------|------|
| `1/2 Cycle Rating (kA)` | `HalfCycleRatingKA` | ? |
| `1/2 Cycle Rating (kA)` | `12CycleRatingKA` | ? Invalid! |
| `1/2 Cycle Duty (%)` | `HalfCycleDutyPercent` | ? |

### Pattern: Move Number to End

| CSV | Property | ?/? |
|-----|----------|------|
| `75 deg C Rating (A)` | `Rating75DegCA` | ? |
| `75 deg C Rating (A)` | `75DegCRating` | ? Invalid! |

### Pattern: Spell Out Ordinals

| CSV | Property | ?/? |
|-----|----------|------|
| `3RG Ohm` | `ThreeRGOhm` | ? |
| `3RG Ohm` | `3RGOhm` | ? Invalid! |

---

## ?? **OHMS NOTATION**

**When "Ohms" is in parentheses, add to property name:**

| CSV | Property | ?/? |
|-----|----------|------|
| `R (Ohms)` | `ROhms` | ? |
| `L-R (Ohms)` | `LROhms` | ? |
| `RG Ohm` | `RGOhm` | ? (already in name) |

---

## ?? **PERCENT HANDLING**

**When (%) disambiguates, add "Percent" suffix:**

| CSV | Property | ?/? |
|-----|----------|------|
| `SC Failure Mode %` | `SCFailureModePercent` | ? |
| `Efficiency %` | `EfficiencyPercent` | ? |
| `PF %` | `PFPercent` | ? |

**Example:**
```csharp
// ShortCircuit model:
public string? HalfCycleDutyKA { get; set; }       // 1/2 Cycle Duty (kA)
public string? HalfCycleDutyPercent { get; set; }  // 1/2 Cycle Duty (%)
```

---

## ?? **SPECIAL ABBREVIATIONS**

### Established Abbreviations (Keep Short)

| Full | Abbreviation | Example |
|------|--------------|---------|
| Primary | `Pri` | `PriBaseKV` |
| Secondary | `Sec` | `SecBaseKV` |
| Tertiary | `Ter` | `TerBaseKV` |
| Downstream | `Dn` | `DnConnKVA` |
| Control | `Ctl` | `CtlKVPu` |
| Maximum | `Max` | `MaxKV` |
| Minimum | `Min` | `KAMin` |
| Ground | `Gnd` | `GndSensor` |
| Motor Overload | `MtrOL` | `MtrOLMfr` |
| Manufacturer | `Mfr` | `BreakerMfr` |
| Impedance | `Imped` | `ImpedKVA` |
| Connected | `Conn` | `ConnKVA` |
| Commission | `Comm` | `CommKVA` |

### IEC Notation

**Keep IEC prefix intact:**

```csharp
public string? IECBreakingKA { get; set; }
public string? IECTCCInitialKA { get; set; }
public string? IECRatedKV { get; set; }
```

### TCC (Time-Current Curve) Notation

**Keep TCC prefix:**

```csharp
public string? TCCMomKA { get; set; }
public string? TCCIntKA { get; set; }
public string? TCC30CycKA { get; set; }
public string? TCCGndMomKA { get; set; }
```

---

## ?? **RELIABILITY PROPERTIES**

**Standard reliability naming:**

```csharp
public string? FailureRatePerYear { get; set; }  // Always /year
public string? RepairTimeH { get; set; }         // Always hours
public string? ReplaceTimeH { get; set; }        // Always hours
public string? DowntimeCostH { get; set; }       // Cost per hour
public string? RepairCost { get; set; }          // No unit ($ implied)
public string? ReplaceCost { get; set; }         // No unit ($ implied)
```

---

## ?? **CATEGORY INFERENCE**

**The generator infers categories from column names:**

| Pattern | Category |
|---------|----------|
| Contains "Bus", "Base kV", "kVA" | `Electrical` |
| Contains "Manufacturer", "Type", "Style" | `Physical` |
| Contains "Trip", "Relay", "Fuse" | `Protection` |
| Contains "Area", "Zone", "Floor" | `Location` |
| Contains "Failure", "Repair", "Replace" | `Reliability` |
| Contains "Status", "ID", "Name" (first) | `Identity` |
| Contains "Data Status", "Comment" | `Metadata` |
| Contains "Setting", "Setpoint", "Tap" | `Control` |
| Default | `General` |

---

## ? **VALIDATION CHECKLIST**

Before deployment, verify each model:

- [ ] No duplicate property names
- [ ] No properties starting with numbers
- [ ] All kV/kA/kVA capitalized
- [ ] All pu capitalized as Pu
- [ ] Temperature properties have C suffix
- [ ] Time properties have appropriate suffix
- [ ] Distance properties have unit suffix
- [ ] Complex units preserved (CalPerCm2)
- [ ] Reliability properties use standard names
- [ ] ID property matches class name
- [ ] ToString() references correct ID
- [ ] Compiles without errors

---

## ?? **FOR AI AGENTS**

### Quick Pattern Matching

When you see this in CSV ? Convert to this in C#:

```
"Base kV"           ? BaseKV
"No of Phases"      ? NoOfPhases
"AC/DC"             ? AcDc
"Trip Time (sec)"   ? TripTimeSec
"Rating (A)"        ? RatingA
"Op Temp (C)"       ? OpTempC
"Failure Rate (/year)" ? FailureRatePerYear
"SC Int kA"         ? SCIntKA
"R1 pu"             ? R1Pu
"1/2 Cycle (kA)"    ? HalfCycleKA
```

### Red Flags (Likely Errors)

```csharp
public string? BasekV           // ? Should be BaseKV
public string? NoofPhases       // ? Should be NoOfPhases
public string? ACDC             // ? Should be AcDc
public string? TripTime         // ??  Might need TripTimeSec
public string? Rating           // ??  Might need RatingA
public string? 75degCRating     // ? Invalid - numeric prefix
public string? C1 (duplicate)   // ? Must disambiguate
```

---

**Last Updated:** 2024 (after 34-model, 1,844-property audit)  
**Status:** ? Established & Validated  
**Source:** Comprehensive manual audit findings  

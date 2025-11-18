# AI AGENT INSTRUCTIONS: Fix Generated C# Model Classes

## OBJECTIVE
Fix all syntax errors and missing models in the Generated C# classes to make them production-ready.

---

## CRITICAL FIXES REQUIRED

### 1. ? REMOVE `<` AND `>` CHARACTERS FROM PROPERTY NAMES
**Problem:** Comparison operators are invalid in C# identifiers

**Fix Pattern:**
```
< ? LessThan
> ? GreaterThan
```

**Examples:**
```csharp
// WRONG:
public string? R1Pu<50 { get; set; }
public string? X1Pu>50 { get; set; }

// CORRECT:
public string? R1PuLessThan50 { get; set; }
public string? X1PuGreaterThan50 { get; set; }
```

**Files Affected:** `Panel.cs`, `MCC.cs`

---

### 2. ? FIX MALFORMED `[Description]` ATTRIBUTES
**Problem:** Missing closing `]` bracket causes syntax errors

**Fix Pattern:**
```csharp
// WRONG:
[Description("1/2 Cycle Duty (%)")
[Units("%")]

// CORRECT:
[Description("1/2 Cycle Duty (%)")]
[Units("%")]
```

**Action:** Check ALL 34 files for this pattern

**Files Likely Affected:** `EquipmentDuty.cs` (but verify all)

---

### 3. ? ADD MISSING MODEL
**Problem:** `ShortCircuit.cs` is missing from the 34 required models

**Action:** Generate `ShortCircuit.cs` from CSV **row 1** using the same template as other models

**CSV Row 1 Columns:**
```
Equipment Duty Report,Buses,AC/DC,Status,... (extract all columns from row 1)
```

**Required Structure:**
```csharp
[EasyPowerClass("Equipment Duty Report")]
public class ShortCircuit
{
    public string? ShortCircuit { get; set; }  // First property matches class name
    // ... other properties
    
    public string? Id
    {
        get => ShortCircuit;  // Points to first property
        set => ShortCircuit = value;
    }
}
```

---

### 4. ? REMOVE UNWANTED MODEL
**Problem:** `EquipmentDuty.cs` exists but is NOT in the required 34 models

**Action:** DELETE `EquipmentDuty.cs`

---

### 5. ? VERIFY ALL 34 MODELS EXIST WITH CORRECT NAMES

**Required Models (EXACT NAMES - do not change):**
1. ArcFlash
2. ShortCircuit *(ADD THIS)*
3. Bus
4. Panel
5. MCC
6. Motor
7. Generator
8. Cable
9. Busway
10. TransmissionLine
11. CLReactor
12. Transformer2W
13. Transformer3W
14. ZigzagTransformer
15. LVBreaker
16. HVBreaker
17. Relay
18. CT
19. Fuse
20. Switch
21. ATS
22. Utility
23. Load
24. Shunt
25. Capacitor
26. Filter
27. AFD
28. UPS
29. Inverter
30. Rectifier
31. Photovoltaic
32. Battery
33. Meter
34. POC

**Total: Must be EXACTLY 34 .cs files**

---

## STRICT PROPERTY NAMING RULES

### ? MUST Rules:
1. **Start with uppercase letter** (PascalCase)
2. **NO special characters** in property names: `<`, `>`, `%`, `+`, `^`, `/`, `-` (except in compound words)
3. **NO numeric prefixes** (e.g., `2XPickup` ? `TwoXPickup`)

### ? Replacement Rules:
| Character | Replace With | Example |
|-----------|--------------|---------|
| `<` | `LessThan` | `R1Pu<50` ? `R1PuLessThan50` |
| `>` | `GreaterThan` | `X1Pu>50` ? `X1PuGreaterThan50` |
| `%` | `Percent` | `Scaling%` ? `ScalingPercent` |
| `+` | `Plus` | `0+3X` ? `0Plus3X` |
| `/` | Remove or semantic | `X/R` ? `XRRatio` |
| `^2` | `Squared` | `I^2t` ? `ISquaredT` |

### ? Capitalization Rules:
1. **Electrical units:** `KV`, `KA`, `KVA`, `MVA`, `MW`, `MVAR`, `HP`
2. **Per-unit:** `Pu` (not `pu` or `PU`)
3. **SCADA:** `Scada` (not `SCADA`)
4. **AC/DC:** `AcDc` (not `ACDC`)

---

## VALIDATION CHECKLIST

Before submitting, verify:

- [ ] **Exactly 34 .cs files** (no more, no less)
- [ ] **ShortCircuit.cs exists**
- [ ] **EquipmentDuty.cs does NOT exist**
- [ ] **All property names start with uppercase**
- [ ] **No `<`, `>`, `%`, `+`, `^` in property names**
- [ ] **All `[Description]` and `[Units]` attributes have closing `]`**
- [ ] **All property names are valid C# identifiers**
- [ ] **Build test passes:**

```bash
dotnet build lib\EasyAF.Data\EasyAF.Data.csproj
```

**Expected output:** `Build succeeded`

---

## FILES TO FIX

| File | Issue | Fix |
|------|-------|-----|
| `Panel.cs` | `<` and `>` in property names | Replace with `LessThan`/`GreaterThan` |
| `MCC.cs` | `<` and `>` in property names | Replace with `LessThan`/`GreaterThan` |
| `EquipmentDuty.cs` | Malformed `[Description]` | Fix closing `]` OR delete file |
| `ShortCircuit.cs` | MISSING | Generate from CSV row 1 |
| **ALL FILES** | Verify no malformed attributes | Check all `[Description]` and `[Units]` |

---

## SUCCESS CRITERIA

? **All 34 files compile without errors**  
? **All property names are valid C# identifiers**  
? **All required models present**  
? **No unwanted models**  
? **Build passes**

---

## NOTES

- **Transformer2W** and **LVBreaker** are the CORRECT names (do not change to Transformer or LVCB)
- Keep ALL existing property names that are already correct
- Only fix the specific issues listed above
- Preserve all XML documentation, attributes, and structure

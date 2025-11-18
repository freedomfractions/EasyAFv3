# EasyAF Model Generator Scripts

**Automation scripts for model generation, audit, and deployment.**

---

## ?? **Scripts in This Folder**

| Script | Purpose | Usage |
|--------|---------|-------|
| `deploy-models.ps1` | Deploy audited models to production | `.\deploy-models.ps1` |
| `audit-model.ps1` | Audit individual model against CSV | `.\audit-model.ps1 -ModelName Bus` |

---

## ?? **deploy-models.ps1**

Deploys audited models from `Generated/` to production `Models/` folder.

### Usage

```powershell
# Standard deployment (with prompts)
.\deploy-models.ps1

# Dry run (see what would happen)
.\deploy-models.ps1 -DryRun

# Skip backup (not recommended)
.\deploy-models.ps1 -SkipBackup

# Force deployment even if build fails
.\deploy-models.ps1 -Force
```

### What It Does

1. ? Validates generated models exist
2. ? Checks generated models compile
3. ? Creates timestamped backup of existing models
4. ? Lists models to be deployed
5. ? Prompts for confirmation (unless `-Force`)
6. ? Copies models to production folder
7. ? Validates final build
8. ? Auto-rollback if deployment fails

### Output

```
========================================
  EasyAF Model Deployment v1.0
========================================

[1/7] Validating generated models...
  Found 34 generated models

[2/7] Checking generated models compile...
  ? All models compile successfully

[3/7] Creating backup of existing models...
  ? Backed up 34 models to: lib\EasyAF.Data\Models\Backup_20241215_143022

[4/7] Models to deploy:
  [REPLACE] ArcFlash.cs
  [REPLACE] Bus.cs
  [NEW] Battery.cs
  ...

[5/7] Ready to deploy 34 models.
Continue? (y/N): y

[6/7] Deploying models...
  ? Deployed: ArcFlash.cs
  ? Deployed: Bus.cs
  ...

[7/7] Final validation...
  ? Final build successful!

========================================
  DEPLOYMENT SUMMARY
========================================

MODE: Live Deployment
Generated Models Found: 34
Models Deployed: 34
Errors: 0
Backup Location: lib\EasyAF.Data\Models\Backup_20241215_143022

========================================

? DEPLOYMENT SUCCESSFUL!
All 34 models are now live in production.
```

---

## ?? **audit-model.ps1**

Audits an individual model against CSV source to identify issues.

### Usage

```powershell
# Audit specific model
.\audit-model.ps1 -ModelName Bus

# Use custom CSV path
.\audit-model.ps1 -ModelName Bus -CsvPath "C:\path\to\easypower fields.csv"
```

### What It Does

1. ? Reads CSV columns for the model
2. ? Reads generated properties from .cs file
3. ? Compares counts
4. ? Detects duplicate properties
5. ? Checks for common naming issues
6. ? Provides fix suggestions

### Output

```
========================================
  Model Audit Helper: Bus
========================================

[CSV] Found 63 columns in source

CSV Columns:
    1. Buses
    2. AC/DC
    3. Status
    ...

----------------------------------------

[GENERATED] Found 63 properties in model

Generated Properties:
    1. Buses
    2. ACDC
    3. Status
    ...

----------------------------------------

COMPARISON:
  ? Property count MATCHES (63 == 63)

  ? DUPLICATE PROPERTIES FOUND:
    'SCSymkA' appears 2 times

COMMON ISSUES CHECK:
  ? Found 3 potential issues:
    - Lowercase kV found (should be KV): BasekV
    - Lowercase 'of' found (should be 'Of'): NoofPhases
    - ACDC found (should be AcDc)

QUICK FIX SUGGESTIONS:
# Common PowerShell fix patterns:

$content = Get-Content "lib\...\Bus.cs" -Raw

# Fix AC/DC
$content = $content -replace 'public string\? ACDC \{', 'public string? AcDc {'

# Fix No of Phases
$content = $content -replace 'public string\? Noofphases \{', 'public string? NoOfPhases {'

...
```

---

## ?? **Common Workflows**

### Initial Generation ? Audit ? Deploy

```powershell
# 1. Generate all models
cd tools\EasyAF.ModelGenerator
dotnet run -- "..\..\easypower fields.csv" "..\..\lib\EasyAF.Data\Models\Generated"

# 2. Audit each model (repeat for all 34)
cd Scripts
.\audit-model.ps1 -ModelName Bus
.\audit-model.ps1 -ModelName Cable
# ... fix issues found ...

# 3. Test deployment
.\deploy-models.ps1 -DryRun

# 4. Deploy for real
.\deploy-models.ps1
```

### After CSV Update

```powershell
# 1. Regenerate affected models
cd tools\EasyAF.ModelGenerator
dotnet run -- "..\..\easypower fields.csv" "..\..\lib\EasyAF.Data\Models\Generated"

# 2. Audit changed models only
cd Scripts
.\audit-model.ps1 -ModelName ChangedModel

# 3. Deploy
.\deploy-models.ps1
```

### Rollback After Failed Deployment

```powershell
# Find latest backup
$latest = Get-ChildItem "lib\EasyAF.Data\Models\Backup_*" | 
    Sort-Object Name -Descending | 
    Select-Object -First 1

# Restore
Copy-Item "$latest\*.cs" "lib\EasyAF.Data\Models\" -Force

# Verify
dotnet build lib\EasyAF.Data\EasyAF.Data.csproj
```

---

## ??? **Prerequisites**

- PowerShell 5.1 or later
- .NET 8 SDK installed
- EasyAFv3 repository cloned
- `easypower fields.csv` in repository root

---

## ?? **Related Documentation**

- [MODEL-GENERATION-COMPLETE-WORKFLOW.md](../../docs/MODEL-GENERATION-COMPLETE-WORKFLOW.md) - Complete workflow guide
- [NAMING-CONVENTIONS.md](../../docs/NAMING-CONVENTIONS.md) - Naming rules reference
- [TURN-KEY-MODEL-GENERATOR.md](../../docs/TURN-KEY-MODEL-GENERATOR.md) - Generator documentation

---

**Last Updated:** 2024  
**Maintainer:** AI-assisted development team  

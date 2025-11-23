# EasyAF ClickOnce Deployment Guide

## Overview

This guide explains how to publish EasyAF updates to the network share for automatic distribution to users.

**Target Audience:** Developers and IT staff responsible for deploying application updates.

---

## Quick Start (Automated Publishing)

### Prerequisites
- Visual Studio 2022 with .NET 8 SDK
- Write access to the network deployment share
- PowerShell 5.1 or later

### Steps

1. **Update version number** (if needed):
   - Open `app/EasyAF.Shell/EasyAF.Shell.csproj`
   - Increment `<ApplicationVersion>` or `<Version>`

2. **Run the publish script**:
   ```powershell
   .\Publish-EasyAF.ps1
   ```

3. **Verify deployment**:
   - Check output folder for `EasyAF.application` file
   - Test installation from network share

That's it! Users will auto-update on next launch.

---

## Configuration

Edit `publish-config.json` to customize deployment settings:

```json
{
  "publishLocation": "\\\\yourserver\\apps\\EasyAF",
  "installUrl": "\\\\yourserver\\apps\\EasyAF",
  "applicationName": "EasyAF",
  "publisherName": "Your Company Name"
}
```

### Key Settings

| Setting | Description | Example |
|---------|-------------|---------|
| `publishLocation` | Where to deploy files | `\\server\apps\EasyAF` |
| `installUrl` | Where users install from | Same as publishLocation |
| `updateMode` | Update behavior | `Foreground` (on launch) |
| `updateInterval` | Check frequency | `1` day |
| `minimumRequiredVersion` | Force update version | `1.2.0.0` or empty |

---

## Manual Publishing (Visual Studio)

If you prefer to use Visual Studio's GUI:

### First-Time Setup

1. **Open Solution**
   - Launch Visual Studio 2022
   - Open `EasyAFv3.sln`

2. **Configure Publish Settings**
   - Right-click `EasyAF.Shell` project ? **Publish**
   - Click **New Profile** ? **ClickOnce**
   - Choose **Folder** as target

3. **Set Locations**
   - **Publishing folder location**: `\\yourserver\apps\EasyAF\`
   - **Installation folder URL**: `\\yourserver\apps\EasyAF\`
   - Click **Next**

4. **Update Settings**
   - ? **The application should check for updates**
   - Update location: `\\yourserver\apps\EasyAF\`
   - Check frequency: **Before the application starts**
   - Click **Next**

5. **Application Information**
   - **Publisher name**: Your Company Name
   - **Product name**: EasyAF Power System Analysis
   - **Support URL**: (optional)
   - Click **Finish**

6. **File Associations**
   - In Publish wizard, go to **Options** ? **File Associations**
   - Add `.ezmap`:
     - Extension: `ezmap`
     - Description: `EasyAF Mapping Configuration`
     - ProgID: `EasyAF.MapFile`
     - Icon: `Resources\FileIcons\ezmap.ico`
   - Add `.ezproj`:
     - Extension: `ezproj`
     - Description: `EasyAF Project File`
     - ProgID: `EasyAF.ProjectFile`
     - Icon: `Resources\FileIcons\ezproj.ico`
   - Click **OK**

7. **Publish**
   - Click **Publish Now**
   - Wait for completion

### Subsequent Updates

1. **Increment Version** (optional but recommended)
   - Open `EasyAF.Shell` project properties
   - Go to **Publish** tab
   - Click **Updates** ? Update version number

2. **Publish**
   - Click **Publish Now** button
   - Files copied to network share

---

## User Installation

### First-Time Installation

Send users these instructions:

1. Navigate to: `\\yourserver\apps\EasyAF`
2. Double-click `EasyAF.application`
3. Click **Install** when prompted
4. Application installs to user profile
5. Start menu shortcut created automatically

### Auto-Updates

- Users launch EasyAF normally
- App checks for updates on startup
- Updates download and apply automatically
- No user intervention required

### Manual Update Check

If users want to check manually:
- **Help** ? **About** ? **Check for Updates**

---

## Version Numbering Strategy

### Recommended Format: `Major.Minor.Build.Revision`

**Example:** `1.2.15.0`

- **Major (1)**: Breaking changes, new architecture
- **Minor (2)**: New features, modules
- **Build (15)**: Bug fixes, patches
- **Revision (0)**: Hotfixes (usually 0)

### When to Increment

| Change Type | Version Change | Force Update? |
|-------------|----------------|---------------|
| New module | `1.2` ? `1.3` | No |
| Bug fix | `1.2.15` ? `1.2.16` | No |
| Critical security fix | `1.2.15` ? `1.2.16` | **Yes** |
| Database schema change | `1.2` ? `1.3` | **Yes** |
| Breaking change | `1.0` ? `2.0` | **Yes** |

### Force Updates

To require all users to update:

1. Set `minimumRequiredVersion` in `publish-config.json`
2. Example: `"minimumRequiredVersion": "1.3.0.0"`
3. Users below this version **must** update before running

---

## Troubleshooting

### Problem: "Windows protected your PC"

**Cause:** SmartScreen blocking unsigned app from internet zone.

**Solution (IT Admin):**
```powershell
# Run on network share to remove "Mark of the Web"
Get-ChildItem -Path "\\yourserver\apps\EasyAF" -Recurse | Unblock-File
```

**Or add to Group Policy trusted sites** (recommended):
- GPO Path: `Computer Config ? Policies ? Administrative Templates ? Windows Components ? Internet Explorer ? Security Page ? Site to Zone Assignment List`
- Add: `\\yourserver\apps\EasyAF` ? Zone `1` (Local Intranet)

---

### Problem: Users not getting updates

**Possible Causes:**
1. Update URL incorrect
2. Network share permissions
3. User running old cached version

**Diagnosis:**
```powershell
# Check update manifest
Get-Content "\\yourserver\apps\EasyAF\EasyAF.application"
```

**Fix:**
- Verify `codebase` in manifest matches network path
- Ensure users have **Read** access to network share
- Have users uninstall and reinstall from network share

---

### Problem: File associations not working

**Cause:** ClickOnce file associations require first-time consent.

**Solution:**
1. User opens file normally (File ? Open)
2. Windows prompts "How do you want to open this file?"
3. User selects EasyAF and checks "Always use this app"
4. Future files open automatically

**Or manually register** (per user):
```powershell
# Run in user's PowerShell
$exePath = "$env:LOCALAPPDATA\Apps\2.0\[hash]\EasyAF.Shell.exe"
cmd /c assoc .ezmap=EasyAF.MapFile
cmd /c ftype EasyAF.MapFile=`"$exePath`" `"%1`"
```

---

## Network Share Setup

### Required Permissions

**Share Permissions:**
- `Domain Users` ? **Read**
- `Developers` ? **Change** (for publishing)

**NTFS Permissions:**
- `Domain Users` ? **Read & Execute**
- `Developers` ? **Modify**

### Recommended Structure

```
\\yourserver\apps\
??? EasyAF\
    ??? EasyAF.application        (launch file)
    ??? Application Files\         (versioned folders)
    ?   ??? EasyAF_1_2_0_15\
    ?   ??? EasyAF_1_2_0_16\
    ?   ??? EasyAF_1_3_0_0\
    ??? setup.exe                  (optional offline installer)
```

---

## Advanced: Offline Installer

To create a standalone installer (no network required):

1. **Publish as usual**
2. **Generate setup.exe**:
   - Publish wizard ? **Options** ? **Deployment**
   - ? **Create desktop shortcut**
   - ? **Use .deploy file extension**
3. **Distribute `setup.exe`**
   - Users run `setup.exe` locally
   - Installs with full ClickOnce benefits

---

## Rollback Procedure

If a bad update is deployed:

### Option 1: Republish Previous Version

1. Check out previous Git tag/commit
2. Increment version (e.g., `1.3.1` ? `1.3.2`)
3. Publish with hotfix

### Option 2: Restore Previous Deployment

```powershell
# Backup current
Move-Item "\\yourserver\apps\EasyAF\Application Files\EasyAF_1_3_0_0" -Destination "\\backup"

# Restore old manifest
Copy-Item "\\backup\EasyAF.application.v1.2.16" -Destination "\\yourserver\apps\EasyAF\EasyAF.application"
```

?? **Warning:** Users may have cached the bad version. Force uninstall/reinstall if needed.

---

## Monitoring Deployments

### Check Active Versions

```powershell
# Get current published version
[xml]$manifest = Get-Content "\\yourserver\apps\EasyAF\EasyAF.application"
$manifest.assembly.assemblyIdentity.version
```

### User Install Locations

Users' installations are in:
```
%LOCALAPPDATA%\Apps\2.0\[random hash]\EasyAF.Shell.exe
```

To find user's version:
```powershell
# Run on user's machine
Get-ChildItem "$env:LOCALAPPDATA\Apps\2.0\" -Recurse -Filter "EasyAF.Shell.exe" |
    ForEach-Object { [System.Diagnostics.FileVersionInfo]::GetVersionInfo($_.FullName) }
```

---

## Security Considerations

### Antivirus Exclusions

Add to corporate AV whitelist:
- `\\yourserver\apps\EasyAF\**`
- `%LOCALAPPDATA%\Apps\2.0\**\EasyAF*.exe`

### Code Signing (Optional for Internal Apps)

If you later decide to code sign:
1. Obtain code signing certificate
2. Sign `EasyAF.Shell.exe` before publishing
3. ClickOnce auto-detects signature
4. Removes SmartScreen warnings

**Cost:** $75-500/year (not required for internal network deployment)

---

## Support Contacts

**For Deployment Issues:**
- IT Admin: [Your IT Contact]
- Developer: [Your Dev Contact]

**For Application Issues:**
- Submit ticket: [Your Ticketing System]
- Email: [Your Support Email]

---

## Appendix: PowerShell Script Reference

See `Publish-EasyAF.ps1` for automated publishing.

**Usage:**
```powershell
# Default publish (uses publish-config.json)
.\Publish-EasyAF.ps1

# Override publish location
.\Publish-EasyAF.ps1 -PublishLocation "\\newserver\apps\EasyAF"

# Dry run (show what would happen)
.\Publish-EasyAF.ps1 -WhatIf
```

---

## Change Log

| Version | Date | Changes | Published By |
|---------|------|---------|--------------|
| 1.0.0.0 | 2025-01-20 | Initial release | [Your Name] |
| 1.1.0.0 | TBD | Added Project module | |
| 1.2.0.0 | TBD | Bug fixes | |

---

**Last Updated:** 2025-01-20  
**Maintained By:** [Your Team Name]

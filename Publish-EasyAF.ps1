<#
.SYNOPSIS
    Automates ClickOnce publishing for EasyAF application.

.DESCRIPTION
    This script publishes the EasyAF application to a network share using ClickOnce deployment.
    It reads configuration from publish-config.json and handles version incrementing.

.PARAMETER PublishLocation
    Override the publish location from config file.
    Example: "\\yourserver\apps\EasyAF"

.PARAMETER IncrementVersion
    Automatically increment the patch version number.
    Example: 1.2.15.0 -> 1.2.16.0

.PARAMETER MinimumRequiredVersion
    Set minimum required version (forces users to update).
    Example: "1.2.16.0"

.PARAMETER WhatIf
    Show what would happen without actually publishing.

.EXAMPLE
    .\Publish-EasyAF.ps1
    Publishes using settings from publish-config.json

.EXAMPLE
    .\Publish-EasyAF.ps1 -PublishLocation "\\newserver\apps\EasyAF"
    Publishes to a different location

.EXAMPLE
    .\Publish-EasyAF.ps1 -IncrementVersion
    Increments version and publishes

.NOTES
    Author: EasyAF Development Team
    Requires: Visual Studio 2022, .NET 8 SDK, MSBuild
    Last Updated: 2025-01-20
#>

[CmdletBinding(SupportsShouldProcess=$true)]
param(
    [Parameter(Mandatory=$false)]
    [string]$PublishLocation,
    
    [Parameter(Mandatory=$false)]
    [switch]$IncrementVersion,
    
    [Parameter(Mandatory=$false)]
    [string]$MinimumRequiredVersion,
    
    [Parameter(Mandatory=$false)]
    [switch]$WhatIf
)

#region Configuration

# Script constants
$ScriptRoot = $PSScriptRoot
$SolutionFile = Join-Path $ScriptRoot "EasyAFv3.sln"
$ProjectFile = Join-Path $ScriptRoot "app\EasyAF.Shell\EasyAF.Shell.csproj"
$ConfigFile = Join-Path $ScriptRoot "publish-config.json"

# Colors for output
$InfoColor = "Cyan"
$SuccessColor = "Green"
$WarningColor = "Yellow"
$ErrorColor = "Red"

#endregion

#region Helper Functions

function Write-Info {
    param([string]$Message)
    Write-Host "??  $Message" -ForegroundColor $InfoColor
}

function Write-Success {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor $SuccessColor
}

function Write-Warning2 {
    param([string]$Message)
    Write-Host "??  $Message" -ForegroundColor $WarningColor
}

function Write-Error2 {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor $ErrorColor
}

function Test-Prerequisites {
    Write-Info "Checking prerequisites..."
    
    # Check if solution exists
    if (-not (Test-Path $SolutionFile)) {
        Write-Error2 "Solution file not found: $SolutionFile"
        return $false
    }
    
    # Check if project exists
    if (-not (Test-Path $ProjectFile)) {
        Write-Error2 "Project file not found: $ProjectFile"
        return $false
    }
    
    # Check if config exists
    if (-not (Test-Path $ConfigFile)) {
        Write-Error2 "Configuration file not found: $ConfigFile"
        Write-Info "Please create publish-config.json with deployment settings."
        return $false
    }
    
    # Check for MSBuild
    $msbuild = Get-Command msbuild -ErrorAction SilentlyContinue
    if (-not $msbuild) {
        Write-Error2 "MSBuild not found. Please install Visual Studio 2022 or Build Tools."
        return $false
    }
    
    Write-Success "Prerequisites check passed"
    return $true
}

function Get-PublishConfig {
    Write-Info "Loading configuration from $ConfigFile..."
    
    try {
        $config = Get-Content $ConfigFile -Raw | ConvertFrom-Json
        Write-Success "Configuration loaded successfully"
        return $config
    }
    catch {
        Write-Error2 "Failed to load configuration: $_"
        return $null
    }
}

function Get-CurrentVersion {
    Write-Info "Reading current version from project file..."
    
    try {
        [xml]$project = Get-Content $ProjectFile
        $versionNode = $project.Project.PropertyGroup.ApplicationVersion
        
        if (-not $versionNode) {
            $versionNode = $project.Project.PropertyGroup.Version
        }
        
        if (-not $versionNode) {
            Write-Warning2 "Version not found in project file, using 1.0.0.0"
            return [Version]"1.0.0.0"
        }
        
        # Parse version
        $version = [Version]$versionNode
        Write-Info "Current version: $version"
        return $version
    }
    catch {
        Write-Error2 "Failed to read version: $_"
        return $null
    }
}

function Set-ProjectVersion {
    param([Version]$NewVersion)
    
    Write-Info "Updating project version to $NewVersion..."
    
    try {
        [xml]$project = Get-Content $ProjectFile
        
        # Update ApplicationVersion
        $versionNode = $project.SelectSingleNode("//ApplicationVersion")
        if ($versionNode) {
            $versionNode.InnerText = $NewVersion.ToString()
        }
        
        # Update Version
        $versionNode = $project.SelectSingleNode("//Version")
        if ($versionNode) {
            $versionNode.InnerText = $NewVersion.ToString()
        }
        
        # Save
        $project.Save($ProjectFile)
        Write-Success "Version updated to $NewVersion"
        return $true
    }
    catch {
        Write-Error2 "Failed to update version: $_"
        return $false
    }
}

function Invoke-Build {
    Write-Info "Building solution in Release mode..."
    
    $buildArgs = @(
        $SolutionFile,
        "/p:Configuration=Release",
        "/p:Platform=`"Any CPU`"",
        "/t:Rebuild",
        "/v:minimal",
        "/nologo"
    )
    
    if ($WhatIf) {
        Write-Info "Would execute: msbuild $buildArgs"
        return $true
    }
    
    $output = & msbuild $buildArgs 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error2 "Build failed with exit code $LASTEXITCODE"
        Write-Host $output -ForegroundColor Red
        return $false
    }
    
    Write-Success "Build completed successfully"
    return $true
}

function Invoke-Publish {
    param(
        [string]$PublishUrl,
        [string]$InstallUrl,
        [string]$MinVersion
    )
    
    Write-Info "Publishing to $PublishUrl..."
    
    $publishArgs = @(
        $ProjectFile,
        "/p:Configuration=Release",
        "/p:PublishUrl=`"$PublishUrl\`"",
        "/p:InstallUrl=`"$InstallUrl\`"",
        "/p:UpdateEnabled=true",
        "/p:UpdateMode=Foreground",
        "/p:UpdateInterval=1",
        "/p:UpdateIntervalUnits=Days",
        "/p:UpdatePeriodically=false",
        "/p:UpdateRequired=false",
        "/p:ApplicationRevision=0",
        "/p:PublisherName=`"Your Company`"",
        "/p:ProductName=`"EasyAF`"",
        "/t:Publish",
        "/v:minimal",
        "/nologo"
    )
    
    # Add minimum required version if specified
    if ($MinVersion) {
        $publishArgs += "/p:MinimumRequiredVersion=`"$MinVersion`""
        Write-Info "Setting minimum required version: $MinVersion"
    }
    
    if ($WhatIf) {
        Write-Info "Would execute: msbuild $publishArgs"
        return $true
    }
    
    $output = & msbuild $publishArgs 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error2 "Publish failed with exit code $LASTEXITCODE"
        Write-Host $output -ForegroundColor Red
        return $false
    }
    
    Write-Success "Publish completed successfully"
    return $true
}

function Test-NetworkPath {
    param([string]$Path)
    
    Write-Info "Verifying network path: $Path"
    
    if ($Path -notmatch "^\\\\") {
        Write-Warning2 "Path does not appear to be a UNC path: $Path"
    }
    
    if (-not (Test-Path $Path)) {
        Write-Error2 "Path not accessible: $Path"
        Write-Info "Please ensure:"
        Write-Info "  1. Network share exists"
        Write-Info "  2. You have write permissions"
        Write-Info "  3. Path is reachable from this machine"
        return $false
    }
    
    Write-Success "Network path verified"
    return $true
}

function Remove-MarkOfTheWeb {
    param([string]$Path)
    
    Write-Info "Removing 'Mark of the Web' from published files..."
    
    if ($WhatIf) {
        Write-Info "Would unblock files in: $Path"
        return $true
    }
    
    try {
        Get-ChildItem -Path $Path -Recurse -File | Unblock-File -ErrorAction SilentlyContinue
        Write-Success "Files unblocked successfully"
        return $true
    }
    catch {
        Write-Warning2 "Failed to unblock some files: $_"
        return $true # Non-critical
    }
}

function Show-Summary {
    param(
        [Version]$Version,
        [string]$PublishPath,
        [string]$InstallPath
    )
    
    Write-Host ""
    Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host "  EasyAF Published Successfully! ??" -ForegroundColor Green
    Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  Version:       $Version" -ForegroundColor White
    Write-Host "  Published to:  $PublishPath" -ForegroundColor White
    Write-Host "  Install URL:   $InstallPath" -ForegroundColor White
    Write-Host ""
    Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host ""
    Write-Info "Next steps:"
    Write-Host "  1. Verify deployment:" -ForegroundColor Yellow
    Write-Host "     - Navigate to: $InstallPath" -ForegroundColor Gray
    Write-Host "     - Check for EasyAF.application file" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  2. Test installation:" -ForegroundColor Yellow
    Write-Host "     - Double-click EasyAF.application" -ForegroundColor Gray
    Write-Host "     - Verify app launches and updates" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  3. Notify users:" -ForegroundColor Yellow
    Write-Host "     - Send update announcement" -ForegroundColor Gray
    Write-Host "     - Users will auto-update on next launch" -ForegroundColor Gray
    Write-Host ""
}

#endregion

#region Main Script

Write-Host ""
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  EasyAF ClickOnce Publisher" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
if (-not (Test-Prerequisites)) {
    exit 1
}

# Load configuration
$config = Get-PublishConfig
if (-not $config) {
    exit 1
}

# Use command-line override or config value
$targetPublishLocation = if ($PublishLocation) { $PublishLocation } else { $config.publishLocation }
$targetInstallUrl = if ($PublishLocation) { $PublishLocation } else { $config.installUrl }

# Get current version
$currentVersion = Get-CurrentVersion
if (-not $currentVersion) {
    exit 1
}

# Increment version if requested
$newVersion = $currentVersion
if ($IncrementVersion) {
    $newVersion = [Version]::new(
        $currentVersion.Major,
        $currentVersion.Minor,
        $currentVersion.Build + 1,
        0
    )
    
    Write-Info "Incrementing version: $currentVersion ? $newVersion"
    
    if (-not (Set-ProjectVersion -NewVersion $newVersion)) {
        exit 1
    }
}

# Verify network path
if (-not $WhatIf) {
    if (-not (Test-NetworkPath -Path $targetPublishLocation)) {
        exit 1
    }
}

# Build solution
if (-not (Invoke-Build)) {
    exit 1
}

# Publish
$minVersion = if ($MinimumRequiredVersion) { $MinimumRequiredVersion } else { $config.minimumRequiredVersion }
if (-not (Invoke-Publish -PublishUrl $targetPublishLocation -InstallUrl $targetInstallUrl -MinVersion $minVersion)) {
    exit 1
}

# Remove Mark of the Web (helps with SmartScreen)
if (-not $WhatIf) {
    Remove-MarkOfTheWeb -Path $targetPublishLocation | Out-Null
}

# Show summary
Show-Summary -Version $newVersion -PublishPath $targetPublishLocation -InstallPath $targetInstallUrl

exit 0

#endregion

# NUCLEAR OPTION - Force kill everything and rebuild fresh

Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor Red
Write-Host "?     NUCLEAR REBUILD - Kill Everything & Start Fresh      ?" -ForegroundColor Red
Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor Red
Write-Host ""

# Step 1: Kill Visual Studio
Write-Host "[1/5] Killing Visual Studio processes..." -ForegroundColor Yellow
$vsProcesses = Get-Process | Where-Object { $_.ProcessName -like "devenv*" -or $_.ProcessName -like "Microsoft.VisualStudio*" }
if ($vsProcesses) {
    $vsProcesses | Stop-Process -Force
    Start-Sleep -Seconds 2
    Write-Host "   ? Killed $($vsProcesses.Count) Visual Studio process(es)" -ForegroundColor Green
} else {
    Write-Host "   ? No Visual Studio processes running" -ForegroundColor Green
}

# Step 2: Kill EasyAF
Write-Host "`n[2/5] Killing EasyAF processes..." -ForegroundColor Yellow
$easyAFProcesses = Get-Process | Where-Object { $_.ProcessName -like "*EasyAF*" }
if ($easyAFProcesses) {
    $easyAFProcesses | Stop-Process -Force
    Start-Sleep -Seconds 1
    Write-Host "   ? Killed $($easyAFProcesses.Count) EasyAF process(es)" -ForegroundColor Green
} else {
    Write-Host "   ? No EasyAF processes running" -ForegroundColor Green
}

# Step 3: Delete ALL bin/obj folders
Write-Host "`n[3/5] Deleting ALL bin/obj folders..." -ForegroundColor Yellow
$binObjFolders = Get-ChildItem -Path . -Include bin,obj -Recurse -Directory -Force -ErrorAction SilentlyContinue
$deleted = 0
foreach ($folder in $binObjFolders) {
    try {
        Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
        $deleted++
    } catch {
        # Ignore errors
    }
}
Write-Host "   ? Deleted $deleted folders" -ForegroundColor Green

# Step 4: Clean and rebuild
Write-Host "`n[4/5] Running dotnet clean && build..." -ForegroundColor Yellow
dotnet clean --verbosity quiet
dotnet build --no-incremental --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? Build successful!" -ForegroundColor Green
} else {
    Write-Host "   ? Build failed!" -ForegroundColor Red
    Write-Host "`nPress any key to exit..."
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
    exit 1
}

# Step 5: Verify DLL timestamp
Write-Host "`n[5/5] Verifying DLL was rebuilt..." -ForegroundColor Yellow
$dll = Get-Item "app\EasyAF.Shell\bin\Debug\net8.0-windows\EasyAF.Data.dll" -ErrorAction SilentlyContinue
if ($dll) {
    $age = (Get-Date) - $dll.LastWriteTime
    if ($age.TotalMinutes -lt 2) {
        Write-Host "   ? DLL is fresh (built $([Math]::Round($age.TotalSeconds, 0)) seconds ago)" -ForegroundColor Green
    } else {
        Write-Host "   ? DLL might be stale (built $([Math]::Round($age.TotalMinutes, 1)) minutes ago)" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ? DLL not found!" -ForegroundColor Red
}

Write-Host ""
Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?                   ? REBUILD COMPLETE                     ?" -ForegroundColor Green
Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "NOW DO THIS:" -ForegroundColor Cyan
Write-Host "  1. Open Visual Studio" -ForegroundColor White
Write-Host "  2. Open EasyAFv3.sln" -ForegroundColor White
Write-Host "  3. Press F5 to run (DO NOT use existing debugging session)" -ForegroundColor Yellow
Write-Host "  4. Open spec file" -ForegroundColor White
Write-Host "  5. Click 'Add Filter'" -ForegroundColor White
Write-Host "  6. Click 'Select Property Path'" -ForegroundColor White
Write-Host "  7. Toggle 'Show Active Only' OFF (to see ALL properties)" -ForegroundColor Yellow
Write-Host "  8. Look for IsAdjustable - it WILL be there!" -ForegroundColor Green
Write-Host ""
Write-Host "NOTE: With 'Show Active Only' OFF, you'll see ALL 99 properties!" -ForegroundColor Cyan
Write-Host "      Then toggle it back ON to see only your 41 enabled properties." -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')

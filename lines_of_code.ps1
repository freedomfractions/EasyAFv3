param(
  [Parameter(Position = 0, ValueFromRemainingArguments = $true)]
  [string[]]$RootPaths = @('.'),
  [ValidateSet('Tracked','Other')]
  [string[]]$IncludeTables = @('Tracked')
)

$ErrorActionPreference = 'Stop'

if (-not $RootPaths -or $RootPaths.Count -eq 0) {
  $RootPaths = @('.')
}

$resolvedRoots = @()
$rootSet = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

foreach ($rawRoot in $RootPaths) {
  $candidate = if ([string]::IsNullOrWhiteSpace($rawRoot)) { '.' } else { $rawRoot }

  try {
    $resolved = (Resolve-Path -LiteralPath $candidate -ErrorAction Stop).ProviderPath
  } catch {
    Write-Warning "Skipping path not found: $candidate"
    continue
  }

  if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
    Write-Warning "Skipping non-directory path: $resolved"
    continue
  }

  if ($rootSet.Add($resolved)) {
    $resolvedRoots += $resolved
  }
}

if (-not $resolvedRoots) {
  Write-Host 'No valid directories to scan.'
  exit 1
}

$excludeDirectories = @('bin','obj','packages','node_modules','.git','.hg','.svn','.vs')
$trackedExtensions = @('.cs', '.xaml', '.resx', '.config', '.csproj', '.json', '.xml')

$lineCounts = [ordered]@{}
$fileCounts = [ordered]@{}
foreach ($ext in $trackedExtensions) {
  $lineCounts[$ext] = 0
  $fileCounts[$ext] = 0
}

$dirsToProcess = [System.Collections.Generic.Queue[string]]::new()
$visitedDirs = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
$fileSet = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

foreach ($root in $resolvedRoots) {
  $dirsToProcess.Enqueue($root)
}

$allFiles = @()

while ($dirsToProcess.Count -gt 0) {
  $currentDir = $dirsToProcess.Dequeue()

  if (-not $visitedDirs.Add($currentDir)) { continue }

  try {
    $childDirs = Get-ChildItem -LiteralPath $currentDir -Directory -Force -ErrorAction Stop
  } catch {
    Write-Warning "Skipped directory due to error: $currentDir - $($_.Exception.Message)"
    continue
  }

  foreach ($dir in $childDirs) {
    $dirName = $dir.Name.ToLowerInvariant()
    if ($excludeDirectories -contains $dirName) { continue }
    if (-not $visitedDirs.Contains($dir.FullName)) {
      $dirsToProcess.Enqueue($dir.FullName)
    }
  }

  try {
    $childFiles = Get-ChildItem -LiteralPath $currentDir -File -Force -ErrorAction Stop
  } catch {
    Write-Warning "Skipped files in directory due to error: $currentDir - $($_.Exception.Message)"
    continue
  }

  foreach ($file in $childFiles) {
    $parentName = $file.Directory.Name.ToLowerInvariant()
    if ($excludeDirectories -contains $parentName) { continue }
    if ($fileSet.Add($file.FullName)) {
      $allFiles += $file
    }
  }
}

if (-not $allFiles) {
  Write-Host "No source files found."
  exit 0
}

$totalLines = 0

foreach ($file in $allFiles) {
  $lineCount = 0
  try {
    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) { $lineCount++ }
  } catch {
    Write-Warning "Skipped file due to error: $($file.FullName)"
    continue
  }

  $ext = $file.Extension.ToLowerInvariant()
  if ([string]::IsNullOrWhiteSpace($ext)) { $ext = '(noext)' }
  if (-not $lineCounts.Contains($ext)) { $lineCounts[$ext] = 0 }
  if (-not $fileCounts.Contains($ext)) { $fileCounts[$ext] = 0 }
  $lineCounts[$ext] += $lineCount
  $fileCounts[$ext] += 1

  $totalLines += $lineCount
}

function Get-FormattedRows {
  param(
    [System.Collections.IDictionary]$LineCounts,
    [System.Collections.IDictionary]$FileCounts,
    [string[]]$Tracked
  )

  $rows = @()
  foreach ($key in $LineCounts.Keys) {
    $lineValue = [int64]$LineCounts[$key]
    $fileValue = if ($FileCounts.Contains($key)) { [int64]$FileCounts[$key] } else { 0 }
    $rows += [PSCustomObject]@{
      Extension      = $key
      Files          = $fileValue
      FilesFormatted = ('{0:N0}' -f $fileValue)
      Lines          = $lineValue
      LinesFormatted = ('{0:N0}' -f $lineValue)
      IsTracked      = ($Tracked -contains $key)
    }
  }

  return $rows
}

$allRows = Get-FormattedRows -LineCounts $lineCounts -FileCounts $fileCounts -Tracked $trackedExtensions
$trackedRows = @($allRows | Where-Object { $_.IsTracked })
$otherRows = @($allRows | Where-Object { -not $_.IsTracked })

if ($trackedRows.Count -eq 0) {
  $trackedRows = ,([PSCustomObject]@{ Extension = '(none)'; Files = 0; FilesFormatted = '0'; Lines = 0; LinesFormatted = '0'; IsTracked = $true })
}

if ($otherRows.Count -eq 0) {
  $otherRows = ,([PSCustomObject]@{ Extension = '(none)'; Files = 0; FilesFormatted = '0'; Lines = 0; LinesFormatted = '0'; IsTracked = $false })
}

$orderedTracked = foreach ($ext in $trackedExtensions) {
  $trackedRows | Where-Object { $_.Extension -eq $ext }
}
$orderedTracked = @($orderedTracked | Where-Object { $_ })
if (-not $orderedTracked) { $orderedTracked = $trackedRows }

$orderedOther = @($otherRows | Sort-Object Extension)

$showTracked = $IncludeTables -contains 'Tracked'
$showOther = $IncludeTables -contains 'Other'
$showOverall = $showTracked -and $showOther

$includedRows = @()
if ($showTracked) { $includedRows += $orderedTracked }
if ($showOther) { $includedRows += $orderedOther }

if (-not $includedRows) {
  Write-Host 'No tables selected via IncludeTables parameter.'
  exit 0
}

$trackedLinesTotal = ($orderedTracked | Measure-Object -Property Lines -Sum).Sum
if ($null -eq $trackedLinesTotal) { $trackedLinesTotal = 0 }
$trackedLinesFormatted = '{0:N0}' -f $trackedLinesTotal

$trackedFilesTotal = ($orderedTracked | Measure-Object -Property Files -Sum).Sum
if ($null -eq $trackedFilesTotal) { $trackedFilesTotal = 0 }
$trackedFilesFormatted = '{0:N0}' -f $trackedFilesTotal

$otherLinesTotal = ($orderedOther | Measure-Object -Property Lines -Sum).Sum
if ($null -eq $otherLinesTotal) { $otherLinesTotal = 0 }
$otherLinesFormatted = '{0:N0}' -f $otherLinesTotal

$otherFilesTotal = ($orderedOther | Measure-Object -Property Files -Sum).Sum
if ($null -eq $otherFilesTotal) { $otherFilesTotal = 0 }
$otherFilesFormatted = '{0:N0}' -f $otherFilesTotal

$includedFilesTotal = ($includedRows | Measure-Object -Property Files -Sum).Sum
if ($null -eq $includedFilesTotal) { $includedFilesTotal = 0 }
$includedFilesFormatted = '{0:N0}' -f $includedFilesTotal

$includedLinesTotal = ($includedRows | Measure-Object -Property Lines -Sum).Sum
if ($null -eq $includedLinesTotal) { $includedLinesTotal = 0 }
$includedLinesFormatted = '{0:N0}' -f $includedLinesTotal

$extLengths = @('Extension'.Length, 'Total'.Length)
if ($showOverall) { $extLengths += 'Overall'.Length }
$extLengths += ($includedRows | ForEach-Object { $_.Extension.Length })
$extWidth = ($extLengths | Measure-Object -Maximum).Maximum

$fileLengths = @('Files'.Length)
if ($showTracked) { $fileLengths += $trackedFilesFormatted.Length }
if ($showOther) { $fileLengths += $otherFilesFormatted.Length }
$fileLengths += $includedFilesFormatted.Length
$fileLengths += ($includedRows | ForEach-Object { $_.FilesFormatted.Length })
$filesWidth = ($fileLengths | Measure-Object -Maximum).Maximum

$lineLengths = @('Lines'.Length)
if ($showTracked) { $lineLengths += $trackedLinesFormatted.Length }
if ($showOther) { $lineLengths += $otherLinesFormatted.Length }
$lineLengths += $includedLinesFormatted.Length
$lineLengths += ($includedRows | ForEach-Object { $_.LinesFormatted.Length })
$linesWidth = ($lineLengths | Measure-Object -Maximum).Maximum

$trackedTitle = 'Visual Studio / Source Files'
$otherTitle = 'Other Files'
$grandTotalTitle = 'Overall'
$titleCandidates = @()
if ($showTracked) { $titleCandidates += $trackedTitle }
if ($showOther) { $titleCandidates += $otherTitle }
if ($showOverall) { $titleCandidates += $grandTotalTitle }
$maxTitleLength = ($titleCandidates | ForEach-Object { $_.Length } | Measure-Object -Maximum).Maximum

$columnWidths = @($extWidth, $filesWidth, $linesWidth)
$columnCount = $columnWidths.Count
$sumWidths = ($columnWidths | Measure-Object -Sum).Sum
$innerWidth = $sumWidths + (3 * $columnCount) - 3
if ($innerWidth -lt $maxTitleLength) {
  $adjust = $maxTitleLength - $innerWidth
  $linesWidth += $adjust
  $columnWidths[2] = $linesWidth
  $sumWidths = ($columnWidths | Measure-Object -Sum).Sum
  $innerWidth = $sumWidths + (3 * $columnCount) - 3
}

$columnWidths = @($extWidth, $filesWidth, $linesWidth)

$boxChars = [ordered]@{
  TopLeft     = [string][char]0x250C
  TopRight    = [string][char]0x2510
  BottomLeft  = [string][char]0x2514
  BottomRight = [string][char]0x2518
  Horizontal  = [string][char]0x2500
  Vertical    = [string][char]0x2502
  TitleLeft   = [string][char]0x251C
  TitleMid    = [string][char]0x252C
  TitleRight  = [string][char]0x2524
  MidLeft     = [string][char]0x251C
  MidMid      = [string][char]0x253C
  MidRight    = [string][char]0x2524
  BottomMid   = [string][char]0x2534
}

function New-BorderLine {
  param(
    [string]$Left,
    [string]$Middle,
    [string]$Right,
    [int[]]$ColumnWidths,
    [string]$Horizontal
  )

  $line = $Left
  for ($i = 0; $i -lt $ColumnWidths.Count; $i++) {
    $line += $Horizontal * ($ColumnWidths[$i] + 2)
    if ($i -lt $ColumnWidths.Count - 1) {
      $line += $Middle
    } else {
      $line += $Right
    }
  }

  return $line
}

$topBorder = $boxChars.TopLeft + ($boxChars.Horizontal * ($innerWidth + 2)) + $boxChars.TopRight
$titleSeparator = New-BorderLine -Left $boxChars.TitleLeft -Middle $boxChars.TitleMid -Right $boxChars.TitleRight -ColumnWidths $columnWidths -Horizontal $boxChars.Horizontal
$separatorBorder = New-BorderLine -Left $boxChars.MidLeft -Middle $boxChars.MidMid -Right $boxChars.MidRight -ColumnWidths $columnWidths -Horizontal $boxChars.Horizontal
$bottomBorder = New-BorderLine -Left $boxChars.BottomLeft -Middle $boxChars.BottomMid -Right $boxChars.BottomRight -ColumnWidths $columnWidths -Horizontal $boxChars.Horizontal
$overallTopBorder = New-BorderLine -Left $boxChars.TopLeft -Middle $boxChars.TitleMid -Right $boxChars.TopRight -ColumnWidths $columnWidths -Horizontal $boxChars.Horizontal

$rowFormat = "{0} {1,-$extWidth} {0} {2,$filesWidth} {0} {3,$linesWidth} {0}"
$titleFormat = "{0} {1,-$innerWidth} {0}"

function Write-Table {
  param(
    [string]$Title,
    [System.Object[]]$Rows,
    [string]$TopBorder,
    [string]$TitleSeparator,
    [string]$SeparatorBorder,
    [string]$BottomBorder,
    [string]$RowFormat,
    [string]$TitleFormat,
    [string]$TableFilesTotalFormatted,
    [string]$TableLinesTotalFormatted
  )

  Write-Host $TopBorder
  Write-Host ($TitleFormat -f $boxChars.Vertical, $Title)
  Write-Host $TitleSeparator
  Write-Host ($RowFormat -f $boxChars.Vertical, 'Extension', 'Files', 'Lines')
  Write-Host $SeparatorBorder

  foreach ($row in $Rows) {
    Write-Host ($RowFormat -f $boxChars.Vertical, $row.Extension, $row.FilesFormatted, $row.LinesFormatted)
  }

  Write-Host $SeparatorBorder
  Write-Host ($RowFormat -f $boxChars.Vertical, 'Total', $TableFilesTotalFormatted, $TableLinesTotalFormatted)
  Write-Host $BottomBorder
  Write-Host
}

Write-Host
if ($showTracked) {
  Write-Table -Title $trackedTitle -Rows $orderedTracked -TopBorder $topBorder -TitleSeparator $titleSeparator -SeparatorBorder $separatorBorder -BottomBorder $bottomBorder -RowFormat $rowFormat -TitleFormat $titleFormat -TableFilesTotalFormatted $trackedFilesFormatted -TableLinesTotalFormatted $trackedLinesFormatted
}

if ($showOther) {
  Write-Table -Title $otherTitle -Rows $orderedOther -TopBorder $topBorder -TitleSeparator $titleSeparator -SeparatorBorder $separatorBorder -BottomBorder $bottomBorder -RowFormat $rowFormat -TitleFormat $titleFormat -TableFilesTotalFormatted $otherFilesFormatted -TableLinesTotalFormatted $otherLinesFormatted
}

if ($showOverall) {
  Write-Host $overallTopBorder
  Write-Host ($rowFormat -f $boxChars.Vertical, 'Overall', 'Files', 'Lines')
  Write-Host $separatorBorder
  Write-Host ($rowFormat -f $boxChars.Vertical, 'Total', $includedFilesFormatted, $includedLinesFormatted)
  Write-Host $bottomBorder
  Write-Host
}
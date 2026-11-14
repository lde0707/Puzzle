$ErrorActionPreference = 'Stop'
$releaseRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../../Releases/crazygames')).Path
$keepName = '2026-11-04-v1.4.0-sdk'
$keepPath = Join-Path $releaseRoot $keepName
foreach ($mode in @('basic','monetized')) {
    $check = Get-Content -LiteralPath (Join-Path $keepPath "$mode/package-checks.json") -Raw | ConvertFrom-Json
    $zip = Join-Path $keepPath "$mode/$($check.zip)"
    if ((Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant() -ne $check.sha256) { throw 'New ZIP hash mismatch' }
}
if (!(Test-Path -LiteralPath (Join-Path $keepPath 'validation.json'))) { throw 'Validate the new build first' }
$oldNames = @('2026-11-02-v1.1.0','2026-11-02-v1.2.0-sdk','2026-11-03-v1.3.0-sdk','2026-11-03-v1.3.1-sdk','2026-11-03-v1.3.2-sdk')
$targets = @()
foreach ($name in $oldNames) {
    $candidate = Join-Path $releaseRoot $name
    if (!(Test-Path -LiteralPath $candidate)) { continue }
    $resolved = (Resolve-Path -LiteralPath $candidate).Path
    if ([IO.Path]::GetDirectoryName($resolved) -ne $releaseRoot -or $resolved -eq $keepPath) { throw 'Target outside old-release allowlist' }
    $entry = Get-Item -LiteralPath $resolved
    if ($entry.LinkType -or @(Get-ChildItem -LiteralPath $resolved -Directory -Recurse | Where-Object LinkType).Count) { throw 'Unexpected directory link' }
    $targets += $resolved
}
# User requested replacement of the old local CrazyGames submission files.
# All absolute targets were checked above, before any recursive removal.
foreach ($target in $targets) { Remove-Item -LiteralPath $target -Recurse -Force }
@{retained=$keepName;removed=$oldNames;scope='Local CrazyGames release directories only'} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $keepPath 'cleanup-receipt.json') -Encoding utf8
Get-ChildItem -LiteralPath $releaseRoot -Directory | Select-Object Name

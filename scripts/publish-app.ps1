#Requires -Version 5.1
<#
.SYNOPSIS
  Publishes the Triple A Flutter App Bundle (.aab) for Google Play Console with automatic version incrementing.

.DESCRIPTION
  Reads current version from owner-app-flutter/pubspec.yaml, prompts for or applies version increment (patch/minor/major/build),
  updates pubspec.yaml, cleans and builds the release Android App Bundle (.aab) signed with release key.

.PARAMETER Increment
  Version increment type: 'patch', 'minor', 'major', 'build', or 'prompt' (interactive selection). Default: 'prompt'

.PARAMETER CustomVersion
  Explicit version to set, e.g. '1.2.0+5'. Overrides Increment.

.PARAMETER ApiBaseUrl
  Production API URL passed to --dart-define=API_BASE_URL. Default: 'https://mytriplea.co.za'

.PARAMETER NoClean
  Skips 'flutter clean' before building.

.PARAMETER OpenFolder
  Opens the build outputs folder in Windows Explorer after build finishes.

.EXAMPLE
  .\scripts\publish-app.ps1

.EXAMPLE
  .\scripts\publish-app.ps1 -Increment patch

.EXAMPLE
  .\scripts\publish-app.ps1 -Increment minor -ApiBaseUrl https://mytriplea.co.za
#>
[CmdletBinding()]
param(
    [ValidateSet("patch", "minor", "major", "build", "none", "keep", "prompt")]
    [string] $Increment = "prompt",

    [string] $CustomVersion = "",

    [string] $ApiBaseUrl = "https://mytriplea.co.za",

    [switch] $NoClean,

    [switch] $OpenFolder
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$FlutterDir = Join-Path $RepoRoot "owner-app-flutter"
$PubspecPath = Join-Path $FlutterDir "pubspec.yaml"

if (-not (Test-Path $PubspecPath)) {
    Write-Error "Could not find pubspec.yaml at $PubspecPath"
    exit 1
}

# 1. Read current version
$pubspecContent = Get-Content -Path $PubspecPath -Raw
if ($pubspecContent -match '(?m)^version:\s*([0-9]+)\.([0-9]+)\.([0-9]+)\+([0-9]+)') {
    [int]$major = $Matches[1]
    [int]$minor = $Matches[2]
    [int]$patch = $Matches[3]
    [int]$build = $Matches[4]
} else {
    Write-Error "Failed to parse version in $PubspecPath. Expected format 'version: x.y.z+n'"
    exit 1
}

$currentVersionStr = "$major.$minor.$patch+$build"

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   Triple A -- Flutter Google Play Bundle Publisher         " -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "Current App Version: " -NoNewline
Write-Host "$major.$minor.$patch (Build $build)" -ForegroundColor Yellow
Write-Host "Target API Base URL: " -NoNewline
Write-Host "$ApiBaseUrl" -ForegroundColor Green
Write-Host ""

# 2. Determine Next Version
$nextVersion = ""
if ($CustomVersion) {
    if ($CustomVersion -notmatch '^[0-9]+\.[0-9]+\.[0-9]+\+[0-9]+$') {
        Write-Error "Invalid CustomVersion '$CustomVersion'. Must be in format 'x.y.z+n' (e.g. 1.2.0+5)"
        exit 1
    }
    $nextVersion = $CustomVersion
} elseif ($Increment -eq "prompt") {
    $patchOpt = "$major.$minor.$($patch + 1)+$($build + 1)"
    $minorOpt = "$major.$($minor + 1).0+$($build + 1)"
    $majorOpt = "$($major + 1).0.0+$($build + 1)"
    $buildOpt = "$major.$minor.$patch+$($build + 1)"

    Write-Host "Select version increment:" -ForegroundColor White
    Write-Host "  [0] Keep current -> $currentVersionStr  (Build without changing version)" -ForegroundColor Green
    Write-Host "  [1] Patch        -> $patchOpt  (Bug fixes / debugging)" -ForegroundColor Cyan
    Write-Host "  [2] Minor        -> $minorOpt  (New features)" -ForegroundColor Cyan
    Write-Host "  [3] Major        -> $majorOpt  (Breaking / major release)" -ForegroundColor Cyan
    Write-Host "  [4] Build only   -> $buildOpt  (Same version name, increment build)" -ForegroundColor Cyan
    Write-Host "  [5] Custom       -> Enter manually" -ForegroundColor Cyan
    Write-Host "  [q] Cancel" -ForegroundColor Red
    Write-Host ""

    $choice = Read-Host "Choose option [0-5, default 1]"
    if (-not $choice) { $choice = "1" }

    switch ($choice) {
        "0" { $nextVersion = $currentVersionStr }
        "1" { $nextVersion = $patchOpt }
        "2" { $nextVersion = $minorOpt }
        "3" { $nextVersion = $majorOpt }
        "4" { $nextVersion = $buildOpt }
        "5" {
            $nextVersion = Read-Host "Enter custom version (format x.y.z+n, e.g. 1.0.2+3)"
            if ($nextVersion -notmatch '^[0-9]+\.[0-9]+\.[0-9]+\+[0-9]+$') {
                Write-Error "Invalid version format '$nextVersion'."
                exit 1
            }
        }
        "q" {
            Write-Host "Build cancelled." -ForegroundColor Yellow
            exit 0
        }
        default {
            Write-Error "Invalid option '$choice'."
            exit 1
        }
    }
} else {
    switch ($Increment) {
        "none"  { $nextVersion = $currentVersionStr }
        "keep"  { $nextVersion = $currentVersionStr }
        "patch" { $nextVersion = "$major.$minor.$($patch + 1)+$($build + 1)" }
        "minor" { $nextVersion = "$major.$($minor + 1).0+$($build + 1)" }
        "major" { $nextVersion = "$($major + 1).0.0+$($build + 1)" }
        "build" { $nextVersion = "$major.$minor.$patch+$($build + 1)" }
    }
}

Write-Host ""
Write-Host "--> Updating pubspec.yaml version to: " -NoNewline
Write-Host "$nextVersion" -ForegroundColor Green

# 3. Update pubspec.yaml
$updatedPubspec = [regex]::Replace($pubspecContent, '(?m)^version:\s*[^\r\n]+', "version: $nextVersion")
Set-Content -Path $PubspecPath -Value $updatedPubspec -NoNewline

# 4. Build Flutter App Bundle
Push-Location $FlutterDir
try {
    if (-not $NoClean) {
        Write-Host ""
        Write-Host "--> Running 'flutter clean'..." -ForegroundColor Cyan
        try { & flutter clean } catch {}
    }

    Write-Host ""
    Write-Host "--> Running 'flutter pub get'..." -ForegroundColor Cyan
    & flutter pub get

    $packageConfig = Join-Path $FlutterDir ".dart_tool\package_config.json"
    if (-not (Test-Path $packageConfig)) {
        throw "flutter pub get failed to resolve packages."
    }

    Write-Host ""
    Write-Host "--> Building Android App Bundle (.aab) for release..." -ForegroundColor Cyan
    Write-Host "    API Base URL: $ApiBaseUrl" -ForegroundColor DarkGray
    & flutter build appbundle --release --dart-define="API_BASE_URL=$ApiBaseUrl"
    
    # Check if bundle output exists
    $bundlePath = Join-Path $FlutterDir "build\app\outputs\bundle\release\app-release.aab"
    if (-not (Test-Path $bundlePath)) {
        throw "Build completed but bundle file was not found at $bundlePath"
    }

    $bundleItem = Get-Item $bundlePath
    $bundleSizeMb = [math]::Round($bundleItem.Length / 1MB, 2)

    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host "  SUCCESS! Release App Bundle Built Successfully           " -ForegroundColor Green
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host "File:    $($bundleItem.FullName)" -ForegroundColor Yellow
    Write-Host "Version: $nextVersion" -ForegroundColor Yellow
    Write-Host "Size:    $bundleSizeMb MB" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Next steps in Google Play Console:" -ForegroundColor Cyan
    Write-Host "  1. Open: https://play.google.com/console"
    Write-Host "  2. Go to: TripleA -> Test and release -> Testing -> Internal testing"
    Write-Host "  3. Click 'Create new release' and upload app-release.aab"
    Write-Host ""

    if ($OpenFolder) {
        explorer.exe (Split-Path $bundlePath)
    }

} finally {
    Pop-Location
}

#Requires -Version 5.1
<#
.SYNOPSIS
  Deploys a build folder to an IIS application physical path on the local runner host.

.DESCRIPTION
  Optionally mirrors the live folder to a backup, stops the app pool, writes app_offline.htm,
  robocopies /MIR from source to destination, removes app_offline.htm, and recycles the pool.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $SourcePath,

    [Parameter(Mandatory = $true)]
    [string] $DestinationPath,

    [string] $AppPoolName = "",

    [string] $BackupRoot = "",

    [string] $BackupLabel = "deploy",

    [int] $BackupRetentionCount = 1,

    [switch] $SkipBackup,

    [switch] $UseAppOffline,

    [string[]] $ExcludeDirectories = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-FullPath {
    param([string] $Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Path not found: $Path"
    }
    return (Resolve-Path -LiteralPath $Path).Path
}

function Import-IisModule {
    if (-not (Get-Module -ListAvailable -Name WebAdministration)) {
        throw "IIS WebAdministration module is not available on this host."
    }
    Import-Module WebAdministration -ErrorAction Stop
}

function Stop-AppPoolSafe {
    param([string] $Name)
    if ([string]::IsNullOrWhiteSpace($Name)) {
        return
    }

    Import-IisModule
    $pool = Get-WebAppPoolState -Name $Name -ErrorAction SilentlyContinue
    if ($null -eq $pool) {
        Write-Warning "App pool '$Name' was not found; continuing without stop/start."
        return
    }

    if ($pool.Value -ne "Stopped") {
        Write-Host "Stopping app pool '$Name'..."
        Stop-WebAppPool -Name $Name
        Start-Sleep -Seconds 2
    }
}

function Start-AppPoolSafe {
    param([string] $Name)
    if ([string]::IsNullOrWhiteSpace($Name)) {
        return
    }

    Import-IisModule
    $pool = Get-WebAppPoolState -Name $Name -ErrorAction SilentlyContinue
    if ($null -eq $pool) {
        return
    }

    Write-Host "Starting app pool '$Name'..."
    Start-WebAppPool -Name $Name
}

function Remove-OldBackups {
    param(
        [string] $Root,
        [string] $Label,
        [int] $Retention
    )

    if ($Retention -lt 1 -or -not (Test-Path -LiteralPath $Root)) {
        return
    }

    $pattern = "$Label-*"
    $backups = Get-ChildItem -LiteralPath $Root -Directory -Filter $pattern |
        Sort-Object LastWriteTime -Descending

    if ($backups.Count -le $Retention) {
        return
    }

    $backups | Select-Object -Skip $Retention | ForEach-Object {
        Write-Host "Removing old backup: $($_.FullName)"
        Remove-Item -LiteralPath $_.FullName -Recurse -Force
    }
}

$source = Resolve-FullPath $SourcePath
$destination = $DestinationPath
if (-not (Test-Path -LiteralPath $destination)) {
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
}
$destination = (Resolve-Path -LiteralPath $destination).Path

Write-Host "Deploying '$source' -> '$destination'"

if (-not $SkipBackup -and -not [string]::IsNullOrWhiteSpace($BackupRoot)) {
    if (-not (Test-Path -LiteralPath $BackupRoot)) {
        New-Item -ItemType Directory -Path $BackupRoot -Force | Out-Null
    }

  $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
  $backupPath = Join-Path $BackupRoot "$BackupLabel-$timestamp"
  if (Test-Path -LiteralPath $destination) {
    Write-Host "Backing up live folder to '$backupPath'..."
    New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
    & robocopy $destination $backupPath /MIR /NFL /NDL /NJH /NJS /NC /NS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) {
      throw "Backup robocopy failed with exit code $LASTEXITCODE"
    }
    Remove-OldBackups -Root $BackupRoot -Label $BackupLabel -Retention $BackupRetentionCount
  }
}

$offlineFile = Join-Path $destination "app_offline.htm"
$poolStopped = $false

try {
    if ($UseAppOffline) {
        Stop-AppPoolSafe -Name $AppPoolName
        $poolStopped = $true
        Set-Content -LiteralPath $offlineFile -Value @"
<!DOCTYPE html>
<html>
<head><title>Maintenance</title></head>
<body><p>Application is being updated. Please try again shortly.</p></body>
</html>
"@ -Encoding utf8
        Write-Host "Wrote app_offline.htm"
        Start-Sleep -Seconds 2
    }

    Write-Host "Running robocopy /MIR..."
    $robocopyArgs = @($source, $destination, "/MIR", "/NFL", "/NDL", "/NJH", "/NJS", "/NC", "/NS", "/NP")
    if ($ExcludeDirectories.Count -gt 0) {
        $robocopyArgs += "/XD"
        $robocopyArgs += $ExcludeDirectories
        Write-Host "Excluding directories from mirror: $($ExcludeDirectories -join ', ')"
    }
    & robocopy @robocopyArgs
    if ($LASTEXITCODE -ge 8) {
        throw "Deploy robocopy failed with exit code $LASTEXITCODE"
    }
}
finally {
    if (Test-Path -LiteralPath $offlineFile) {
        Remove-Item -LiteralPath $offlineFile -Force -ErrorAction SilentlyContinue
        Write-Host "Removed app_offline.htm"
    }

    if ($poolStopped) {
        Start-AppPoolSafe -Name $AppPoolName
    }
}

Write-Host "Deploy complete: $destination"

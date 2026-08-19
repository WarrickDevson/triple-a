#Requires -Version 5.1
<#
.SYNOPSIS
  Restores the latest API backup from C:\WebApps\TripleA\_backups to the live API folder.
#>
[CmdletBinding()]
param(
    [string] $BackupRoot = "C:\WebApps\TripleA\_backups",
    [string] $DestinationPath = "C:\WebApps\TripleA\api",
    [string] $AppPoolName = "KPW",
    [string] $BackupLabel = "api"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $BackupRoot)) {
    throw "Backup root not found: $BackupRoot"
}

$latest = Get-ChildItem -LiteralPath $BackupRoot -Directory -Filter "$BackupLabel-*" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($null -eq $latest) {
    throw "No backups found matching '$BackupLabel-*' in $BackupRoot"
}

Write-Host "Rolling back API from $($latest.FullName) to $DestinationPath"

$params = @{
    SourcePath = $latest.FullName
    DestinationPath = $DestinationPath
    AppPoolName = $AppPoolName
    SkipBackup = $true
    UseAppOffline = $true
}

& (Join-Path $PSScriptRoot "deploy-iis.ps1") @params

#Requires -Version 5.1
<#
.SYNOPSIS
  Publishes KPW MoveWell for IIS (mytriplea.co.za).

.DESCRIPTION
  Builds all three applications into a single output tree:

    publish/iis/
      index.html, styles.css, ...  -> IIS site root (/)
      api/      -> copy to IIS application /api
      portal/   -> copy to IIS application /portal
      app/      -> copy to IIS application /app

.PARAMETER OutputRoot
  Root folder for publish output. Default: <repo>/publish/iis

.PARAMETER ApiBaseUrl
  Base URL clients use for API calls (no trailing slash).
  Default: https://mytriplea.co.za

.PARAMETER Clean
  Delete OutputRoot before publishing.

.PARAMETER SkipNpmInstall
  Skip 'npm ci' in physio-portal-vue3 (use if node_modules is already current).

.EXAMPLE
  .\scripts\publish-iis.ps1

.EXAMPLE
  .\scripts\publish-iis.ps1 -Clean
#>
[CmdletBinding()]
param(
    [string] $OutputRoot = "",
    [string] $ApiBaseUrl = "https://mytriplea.co.za",
    [switch] $Clean,
    [switch] $SkipNpmInstall
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
if (-not $OutputRoot) {
    $OutputRoot = Join-Path $RepoRoot "publish\iis"
} elseif (-not (Test-Path $OutputRoot)) {
    $OutputRoot = (New-Item -ItemType Directory -Path $OutputRoot -Force).FullName
} else {
    $OutputRoot = (Resolve-Path -LiteralPath $OutputRoot).Path
}

$ApiOut = Join-Path $OutputRoot "api"
$PortalOut = Join-Path $OutputRoot "portal"
$AppOut = Join-Path $OutputRoot "app"

function Require-Command {
    param([string] $Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command not found on PATH: $Name"
    }
}

function Write-Step {
    param([string] $Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Reset-Directory {
    param([string] $Path)
    if (Test-Path $Path) {
        Remove-Item -LiteralPath $Path -Recurse -Force
    }
    New-Item -ItemType Directory -Path $Path -Force | Out-Null
}

Write-Host "KPW MoveWell — IIS publish" -ForegroundColor Green
Write-Host "Repo:       $RepoRoot"
Write-Host "Output:     $OutputRoot"
Write-Host "API URL:    $ApiBaseUrl"

Require-Command "dotnet"
Require-Command "npm"
Require-Command "flutter"

if ($Clean -and (Test-Path $OutputRoot)) {
    Write-Step "Cleaning $OutputRoot"
    Remove-Item -LiteralPath $OutputRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null

# --- API (.NET) ---
Write-Step "Publishing API to api\"
Reset-Directory $ApiOut

$ApiProject = Join-Path $RepoRoot "backend-api-dot-net\KPW.Api\KPW.Api.csproj"
Push-Location (Join-Path $RepoRoot "backend-api-dot-net")
try {
    dotnet publish $ApiProject `
        -c Release `
        -o $ApiOut `
        /p:EnvironmentName=Staging
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }

    $GcpCredentials = Join-Path $RepoRoot "backend-api-dot-net\KPW.Api\devson-development-6d4da133b74e.json"
    if (Test-Path $GcpCredentials) {
        Copy-Item -LiteralPath $GcpCredentials -Destination $ApiOut -Force
        Write-Host "Copied GCP credentials to api\devson-development-6d4da133b74e.json"
    } else {
        Write-Warning "GCP credentials not found: $GcpCredentials"
        Write-Warning "Video upload and AI chat will fail on staging until this file is copied to the api folder."
    }
} finally {
    Pop-Location
}

# --- Physio portal (Vue) ---
Write-Step "Building physio portal to portal\"
Reset-Directory $PortalOut

$PortalDir = Join-Path $RepoRoot "physio-portal-vue3"
Push-Location $PortalDir
try {
    # NODE_ENV=production makes npm omit devDependencies (vue-tsc, vite, etc.).
    $savedNodeEnv = $env:NODE_ENV
    $env:NODE_ENV = "development"

    if (-not $SkipNpmInstall) {
        npm ci --include=dev
        if ($LASTEXITCODE -ne 0) { throw "npm ci failed with exit code $LASTEXITCODE" }
    }
    $env:VITE_API_BASE_URL = $ApiBaseUrl
    npm run build -- --base /
    if ($LASTEXITCODE -ne 0) { throw "npm run build failed with exit code $LASTEXITCODE" }
} finally {
    if ($null -eq $savedNodeEnv) {
        Remove-Item Env:NODE_ENV -ErrorAction SilentlyContinue
    } else {
        $env:NODE_ENV = $savedNodeEnv
    }
    Remove-Item Env:VITE_API_BASE_URL -ErrorAction SilentlyContinue
    Pop-Location
}

$DistDir = Join-Path $PortalDir "dist"
if (-not (Test-Path $DistDir)) {
    throw "Vue build output not found: $DistDir"
}
Copy-Item -Path (Join-Path $DistDir "*") -Destination $PortalOut -Recurse -Force

# --- Owner app (Flutter web) ---
Write-Step "Building owner app to app\"
Reset-Directory $AppOut

$FlutterDir = Join-Path $RepoRoot "owner-app-flutter"
Push-Location $FlutterDir
try {
    flutter pub get
    if ($LASTEXITCODE -ne 0) { throw "flutter pub get failed with exit code $LASTEXITCODE" }

    flutter build web --release --base-href / --dart-define=ENV=staging
    if ($LASTEXITCODE -ne 0) { throw "flutter build web failed with exit code $LASTEXITCODE" }
} finally {
    Pop-Location
}

$WebBuildDir = Join-Path $FlutterDir "build\web"
if (-not (Test-Path $WebBuildDir)) {
    throw "Flutter web build output not found: $WebBuildDir"
}
Copy-Item -Path (Join-Path $WebBuildDir "*") -Destination $AppOut -Recurse -Force

# --- Site root gateway (static landing) ---
Write-Step "Copying site gateway to publish root"
$LandingDir = Join-Path $RepoRoot "site-landing"
if (-not (Test-Path $LandingDir)) {
    throw "Site landing source not found: $LandingDir"
}
Copy-Item -Path (Join-Path $LandingDir "*") -Destination $OutputRoot -Force

# --- Summary ---
Write-Host ""
Write-Host "Publish complete." -ForegroundColor Green
Write-Host ""
Write-Host "Copy to IIS (C:\WebApps\TripleA\):"
Write-Host "  $OutputRoot\index.html (+ styles.css, favicon.svg, web.config, landing.js)  ->  www site root"
Write-Host "  $ApiOut     ->  ...\api"
Write-Host "  $PortalOut  ->  ...\portal  (bind app.mytriplea.co.za)"
Write-Host "  $AppOut     ->  ...\app     (bind owner.mytriplea.co.za)"
Write-Host ""
Write-Host "URLs after deploy:"
Write-Host "  Gateway: https://mytriplea.co.za/"
Write-Host "  API:     https://mytriplea.co.za/api/"
Write-Host "  Portal:  https://app.mytriplea.co.za/"
Write-Host "  App:     https://owner.mytriplea.co.za/"
Write-Host ""
Write-Host "API app pool: set ASPNETCORE_ENVIRONMENT=Staging and GOOGLE_APPLICATION_CREDENTIALS."
Write-Host "See backend-api-dot-net\docs\IIS_STAGING.md for full checklist."

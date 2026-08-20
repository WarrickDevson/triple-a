#Requires -Version 5.1
<#
.SYNOPSIS
  Patches ASP.NET Core environment variables in a published API web.config before IIS deploy.

.DESCRIPTION
  Reads DEPLOY_* environment variables and writes matching aspNetCore environmentVariables entries.
  Only non-empty DEPLOY_* values are applied.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $ApiPhysicalPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Set-XmlEnvironmentVariable {
    param(
        [System.Xml.XmlDocument] $Document,
        [string] $Name,
        [string] $Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return
    }

    $xpath = "/configuration/location/system.webServer/aspNetCore/environmentVariables/environmentVariable[@name='$Name']"
    $node = $Document.SelectSingleNode($xpath)
    if ($null -eq $node) {
        $envVars = $Document.SelectSingleNode("/configuration/location/system.webServer/aspNetCore/environmentVariables")
        if ($null -eq $envVars) {
            $aspNetCore = $Document.SelectSingleNode("/configuration/location/system.webServer/aspNetCore")
            if ($null -eq $aspNetCore) {
                $aspNetCore = $Document.SelectSingleNode("/configuration/system.webServer/aspNetCore")
            }
            if ($null -eq $aspNetCore) {
                throw "web.config is missing aspNetCore section."
            }
            $envVars = $Document.CreateElement("environmentVariables")
            [void]$aspNetCore.AppendChild($envVars)
        }

        $node = $Document.CreateElement("environmentVariable")
        $nameAttr = $Document.CreateAttribute("name")
        $nameAttr.Value = $Name
        [void]$node.Attributes.Append($nameAttr)
        [void]$envVars.AppendChild($node)
    }

    $node.SetAttribute("value", $Value)
    Write-Host "Set $Name in web.config"
}

$apiPath = (Resolve-Path -LiteralPath $ApiPhysicalPath).Path
$configPath = Join-Path $apiPath "web.config"
if (-not (Test-Path -LiteralPath $configPath)) {
    throw "web.config not found at $configPath"
}

$xml = New-Object System.Xml.XmlDocument
$xml.PreserveWhitespace = $true
$xml.Load($configPath)

$mapping = @{
    "DEPLOY_ASPNETCORE_ENVIRONMENT" = "ASPNETCORE_ENVIRONMENT"
    "DEPLOY_DB_CONNECTION_STRING" = "ConnectionStrings__DefaultConnection"
    "DEPLOY_JWT_KEY" = "Jwt__Key"
    "DEPLOY_GCP_CREDENTIALS_PATH" = "GOOGLE_APPLICATION_CREDENTIALS"
    "DEPLOY_APP_PUBLIC_PORTAL_URL" = "App__PublicPortalUrl"
    "DEPLOY_APP_PUBLIC_OWNER_APP_URL" = "App__PublicOwnerAppUrl"
    "DEPLOY_SENDGRID_API_KEY" = "SendGrid__ApiKey"
    "DEPLOY_SENDGRID_PROVIDER" = "SendGrid__Provider"
    "DEPLOY_SENDGRID_FROM_EMAIL" = "SendGrid__FromEmail"
    "DEPLOY_SENDGRID_FROM_NAME" = "SendGrid__FromName"
    "DEPLOY_AI_APIKEY" = "Ai__ApiKey"
    "DEPLOY_AI_PROVIDER" = "Ai__Provider"
    "DEPLOY_AI_MODEL" = "Ai__Model"
}

foreach ($deployName in $mapping.Keys) {
    $configName = $mapping[$deployName]
    $value = [Environment]::GetEnvironmentVariable($deployName)
    Set-XmlEnvironmentVariable -Document $xml -Name $configName -Value $value
}

$xml.Save($configPath)
Write-Host "Updated $configPath"

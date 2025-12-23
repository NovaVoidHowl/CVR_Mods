#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Synchronizes metadata from AssemblyInfo.cs files to CVRMG.json files for all CVR mods.

.DESCRIPTION
    This script scans for CVR mod projects in the repository and updates their CVRMG.json files
    with metadata extracted from their corresponding AssemblyInfo.cs files.

    It updates:
    - modversion: from AssemblyInfoParams.Version
    - author: from AssemblyInfoParams.Author
    - loaderversion: from VerifyLoaderVersion attribute (formatted as v0.x.x)

.PARAMETER DryRun
    If specified, shows what would be changed without actually modifying files.

.EXAMPLE
    .\sync-cvrmg-metadata.ps1

.EXAMPLE
    .\sync-cvrmg-metadata.ps1 -DryRun
#>

param(
    [switch]$DryRun
)

function Get-AssemblyInfoData {
    param(
        [string]$AssemblyInfoPath
    )

    if (-not (Test-Path $AssemblyInfoPath)) {
        Write-Warning "AssemblyInfo.cs not found at: $AssemblyInfoPath"
        return $null
    }

    $content = Get-Content $AssemblyInfoPath -Raw

    # Extract version from AssemblyInfoParams.Version
    $versionMatch = [regex]::Match($content, 'public const string Version = "([^"]+)";')
    if (-not $versionMatch.Success) {
        Write-Warning "Could not find Version in $AssemblyInfoPath"
        return $null
    }
    $version = $versionMatch.Groups[1].Value

    # Extract author from AssemblyInfoParams.Author
    $authorMatch = [regex]::Match($content, 'public const string Author = "([^"]+)";')
    if (-not $authorMatch.Success) {
        Write-Warning "Could not find Author in $AssemblyInfoPath"
        return $null
    }
    $author = $authorMatch.Groups[1].Value

    # Extract loader version from VerifyLoaderVersion attribute
    $loaderVersionMatch = [regex]::Match($content, '\[assembly: VerifyLoaderVersion\((\d+),\s*(\d+),\s*(\d+)')
    if (-not $loaderVersionMatch.Success) {
        Write-Warning "Could not find VerifyLoaderVersion in $AssemblyInfoPath"
        return $null
    }
    $major = $loaderVersionMatch.Groups[1].Value
    $minor = $loaderVersionMatch.Groups[2].Value
    $patch = $loaderVersionMatch.Groups[3].Value
    $loaderVersion = "v$major.$minor.$patch"

    return @{
        Version = $version
        Author = $author
        LoaderVersion = $loaderVersion
    }
}

function Update-CVRMGJson {
    param(
        [string]$CVRMGPath,
        [hashtable]$AssemblyData,
        [switch]$DryRun
    )

    if (-not (Test-Path $CVRMGPath)) {
        Write-Warning "CVRMG.json not found at: $CVRMGPath"
        return
    }

    try {
        $jsonContent = Get-Content $CVRMGPath -Raw | ConvertFrom-Json

        $changed = $false

        # Update modversion
        if ($jsonContent.modversion -ne $AssemblyData.Version) {
            Write-Host "  Updating modversion: $($jsonContent.modversion) -> $($AssemblyData.Version)" -ForegroundColor Yellow
            $jsonContent.modversion = $AssemblyData.Version
            $changed = $true
        }

        # Update author
        if ($jsonContent.author -ne $AssemblyData.Author) {
            Write-Host "  Updating author: $($jsonContent.author) -> $($AssemblyData.Author)" -ForegroundColor Yellow
            $jsonContent.author = $AssemblyData.Author
            $changed = $true
        }

        # Update loaderversion
        if ($jsonContent.loaderversion -ne $AssemblyData.LoaderVersion) {
            Write-Host "  Updating loaderversion: $($jsonContent.loaderversion) -> $($AssemblyData.LoaderVersion)" -ForegroundColor Yellow
            $jsonContent.loaderversion = $AssemblyData.LoaderVersion
            $changed = $true
        }

        if ($changed) {
            if ($DryRun) {
                Write-Host "  [DRY RUN] Would update $CVRMGPath" -ForegroundColor Cyan
            } else {
                $jsonContent | ConvertTo-Json -Depth 10 | Set-Content $CVRMGPath -Encoding UTF8
                Write-Host "  Updated $CVRMGPath" -ForegroundColor Green
            }
        } else {
            Write-Host "  No changes needed for $CVRMGPath" -ForegroundColor DarkGreen
        }

    } catch {
        Write-Error "Failed to process $CVRMGPath`: $($_.Exception.Message)"
    }
}

# Main script execution
Write-Host "=== CVR Mod Metadata Synchronization ===" -ForegroundColor Magenta

if ($DryRun) {
    Write-Host "DRY RUN MODE - No files will be modified" -ForegroundColor Cyan
    Write-Host ""
}

# Find all mod directories (directories containing both AssemblyInfo.cs and CVRMG.json in Properties folder)
$rootPath = $PSScriptRoot
$modDirs = Get-ChildItem -Path $rootPath -Directory | Where-Object {
    $propertiesPath = Join-Path $_.FullName "Properties"
    $assemblyInfoPath = Join-Path $propertiesPath "AssemblyInfo.cs"
    $cvrmgPath = Join-Path $propertiesPath "CVRMG.json"

    (Test-Path $assemblyInfoPath) -and (Test-Path $cvrmgPath)
}

if ($modDirs.Count -eq 0) {
    Write-Warning "No CVR mod directories found in $rootPath"
    exit 1
}

Write-Host "Found $($modDirs.Count) CVR mod(s):" -ForegroundColor Green
$modDirs | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }
Write-Host ""

# Process each mod
foreach ($modDir in $modDirs) {
    Write-Host "Processing mod: $($modDir.Name)" -ForegroundColor Blue

    $propertiesPath = Join-Path $modDir.FullName "Properties"
    $assemblyInfoPath = Join-Path $propertiesPath "AssemblyInfo.cs"
    $cvrmgPath = Join-Path $propertiesPath "CVRMG.json"

    # Extract data from AssemblyInfo.cs
    $assemblyData = Get-AssemblyInfoData -AssemblyInfoPath $assemblyInfoPath

    if ($null -eq $assemblyData) {
        Write-Warning "  Skipping $($modDir.Name) due to assembly info extraction failure"
        continue
    }

    Write-Host "  Found metadata - Version: $($assemblyData.Version), Author: $($assemblyData.Author), LoaderVersion: $($assemblyData.LoaderVersion)" -ForegroundColor Gray

    # Update CVRMG.json
    Update-CVRMGJson -CVRMGPath $cvrmgPath -AssemblyData $assemblyData -DryRun:$DryRun

    Write-Host ""
}

Write-Host "=== Synchronization Complete ===" -ForegroundColor Magenta

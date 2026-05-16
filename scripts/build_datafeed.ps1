param(
    [string]$Configuration = "Release"
)

$scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
& (Join-Path -Path $scriptDir -ChildPath "build_mod.ps1") -ModName "DataFeed" -Configuration $Configuration

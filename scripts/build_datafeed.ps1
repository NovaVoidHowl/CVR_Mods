param(
    [string]$Configuration = "Release"
)

$scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$repoRoot = Split-Path -Parent -Path $scriptDir
$cvrPath = $env:CVRPATH
$cvrExecutable = "ChilloutVR.exe"
$cvrDefaultPath = "C:\Program Files (x86)\Steam\steamapps\common\ChilloutVR"

if ($cvrPath -and (Test-Path (Join-Path -Path $cvrPath -ChildPath $cvrExecutable))) {
    Write-Host "Found the ChilloutVR folder on: $cvrPath"
}
elseif (Test-Path (Join-Path -Path $cvrDefaultPath -ChildPath $cvrExecutable)) {
    $cvrPath = $cvrDefaultPath
    $env:CVRPATH = $cvrPath
    Write-Host "Found ChilloutVR at the default Steam location."
}
else {
    Write-Host "[ERROR] ChilloutVR.exe not found in CVRPATH or the default Steam location." -ForegroundColor Red
    Write-Host "        Please define the CVRPATH environment variable pointing to the ChilloutVR folder."
    exit 1
}

$projectPath = Join-Path -Path $repoRoot -ChildPath "DataFeed\DataFeed.csproj"
$outputPath = Join-Path -Path $cvrPath -ChildPath "Mods"

dotnet build $projectPath `
    -c $Configuration `
    -p:OutputPath="$outputPath\" `
    -p:SolutionDir="$repoRoot\"

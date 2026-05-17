param(
    [Parameter(Mandatory = $true)]
    [string]$ModName,
    [string]$Configuration = "Release"
)

$scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$repoRoot = Split-Path -Parent -Path $scriptDir
$cvrPath = $env:CVRPATH
$cvrExecutable = "ChilloutVR.exe"
$cvrDefaultPath = "C:\Program Files (x86)\Steam\steamapps\common\ChilloutVR"

$projectPath = Join-Path -Path $repoRoot -ChildPath "$ModName\$ModName.csproj"
if (-not (Test-Path $projectPath)) {
    Write-Host "[ERROR] Project not found: $projectPath" -ForegroundColor Red
    exit 1
}

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

$outputPath = Join-Path -Path $cvrPath -ChildPath "Mods"

# MSBuild accepts forward slashes on Windows. Avoid quoted property values ending
# in a backslash because native Windows argument parsing can merge the next arg.
$msbuildOutputPath = ($outputPath -replace "\\", "/").TrimEnd("/") + "/"
$msbuildSolutionDir = ($repoRoot -replace "\\", "/").TrimEnd("/") + "/"

$buildArgs = @(
    "build"
    $projectPath
    "-c"
    $Configuration
    "-p:OutputPath=$msbuildOutputPath"
    "-p:SolutionDir=$msbuildSolutionDir"
)

dotnet @buildArgs
exit $LASTEXITCODE

param([string]$file)

# not in the specified directory, so will need to handle normal csharpier command
Write-Host "Running csharpier on $file"
dotnet csharpier $file

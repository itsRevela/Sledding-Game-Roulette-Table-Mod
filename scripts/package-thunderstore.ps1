<#
.SYNOPSIS
    Builds the Thunderstore package, dist\RouletteRevival-<version>.zip, for upload at thunderstore.io.
.DESCRIPTION
    Thunderstore ships the compiled mod, which can only be built on a PC that has the game (with MelonLoader run once),
    so this runs locally rather than in CI. The version comes from src\RouletteRevival\RouletteRevival.csproj: run it
    after the release workflow has bumped it and you have pulled. thunderstore\CHANGELOG.md (the player-facing
    changelog) must have a "## X.Y.Z" section for that version.
.PARAMETER GameDir
    The game folder, if it isn't in the default Steam location.
#>
param([string]$GameDir)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'src\RouletteRevival\RouletteRevival.csproj'
$gameArgs = @()
if ($GameDir) { $gameArgs += "-p:GameDir=$GameDir" }

Write-Host "Building the mod against your game..." -ForegroundColor Cyan
dotnet build $project -c Release --nologo -v q @gameArgs
if ($LASTEXITCODE -ne 0) { throw "Building the mod failed (see the errors above)." }
$dll = (dotnet msbuild $project -nologo -getProperty:TargetPath -p:Configuration=Release @gameArgs).Trim()
if (-not (Test-Path $dll)) { throw "Built mod not found at '$dll'." }

Write-Host "Packaging..." -ForegroundColor Cyan
$ts = Join-Path $root 'thunderstore'
$toolOutput = dotnet run --project (Join-Path $root 'tools\ReleaseTool') -c Release -- thunderstore `
    --csproj $project `
    --template (Join-Path $ts 'manifest.json') `
    --icon (Join-Path $ts 'icon.png') `
    --readme (Join-Path $ts 'README.md') `
    --changelog (Join-Path $ts 'CHANGELOG.md') `
    --dll $dll `
    --out-dir (Join-Path $root 'dist')
if ($LASTEXITCODE -ne 0) { throw "Packaging failed (see the error above)." }
$zip = @($toolOutput)[-1] # the tool prints the zip path last, after any build output

Write-Host ""
Write-Host "Done: $zip" -ForegroundColor Green
Write-Host "Upload it at https://thunderstore.io/package/create/ and pick the Sledding Game community."

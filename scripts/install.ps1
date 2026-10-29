<#
.SYNOPSIS
    Installs Roulette Revival for Sledding Game: MelonLoader, then builds the mod from this folder against your own copy
    of the game and copies it into <game>\Mods. Safe to re-run (for example after a game update).
.PARAMETER GameDir
    Game install folder. Found automatically through Steam when omitted.
#>
param(
    [string]$GameDir = ""
)

$ErrorActionPreference = "Stop"
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12

$SteamAppId = "3438850"
$GameExe = "Sledding Game.exe"
$MelonLoaderUrl = "https://github.com/LavaGang/MelonLoader/releases/download/v0.7.3/MelonLoader.x64.zip"
$MelonLoaderSha256 = "5B2B2F3D1CD42B59EC886C5BDC2663EDAE87A0097A4F4A8F58C0965A99DDA416"
$DotnetInstallUrl = "https://dot.net/v1/dotnet-install.ps1"
$MinSdkMajor = 8
$RepoRoot = Split-Path -Parent $PSScriptRoot
$ToolsDir = Join-Path $env:LOCALAPPDATA "RouletteRevival"

function Write-Step([string]$Text) { Write-Host ""; Write-Host "==> $Text" -ForegroundColor Cyan }

function Get-SteamLibraries {
    $roots = @()
    foreach ($key in @("HKCU:\Software\Valve\Steam", "HKLM:\SOFTWARE\WOW6432Node\Valve\Steam")) {
        $props = Get-ItemProperty -Path $key -ErrorAction SilentlyContinue
        if ($props.SteamPath) { $roots += $props.SteamPath }
        if ($props.InstallPath) { $roots += $props.InstallPath }
    }
    $roots += "${env:ProgramFiles(x86)}\Steam"
    $libraries = @()
    foreach ($root in ($roots | Select-Object -Unique)) {
        $vdf = Join-Path $root "steamapps\libraryfolders.vdf"
        if (Test-Path $vdf) {
            $libraries += Select-String -Path $vdf -Pattern '"path"\s+"([^"]+)"' |
                ForEach-Object { $_.Matches[0].Groups[1].Value -replace '\\\\', '\' }
        }
        $libraries += $root
    }
    return $libraries | Where-Object { $_ } | Select-Object -Unique
}

function Find-GameDir {
    foreach ($library in Get-SteamLibraries) {
        $manifest = Join-Path $library "steamapps\appmanifest_$SteamAppId.acf"
        if (-not (Test-Path $manifest)) { continue }
        $installDir = (Select-String -Path $manifest -Pattern '"installdir"\s+"([^"]+)"').Matches[0].Groups[1].Value
        $candidate = Join-Path $library "steamapps\common\$installDir"
        if (Test-Path (Join-Path $candidate $GameExe)) { return $candidate }
    }
    throw "Couldn't find Sledding Game through Steam. Re-run with the folder, e.g.: install.bat -GameDir ""D:\Games\Sledding Game"""
}

function Wait-GameClosed {
    while (Get-Process -Name ([IO.Path]::GetFileNameWithoutExtension($GameExe)) -ErrorAction SilentlyContinue) {
        Write-Host "Sledding Game is running. Close it to continue..." -ForegroundColor Yellow
        Start-Sleep -Seconds 3
    }
}

function Install-MelonLoader([string]$Game) {
    if ((Test-Path (Join-Path $Game "version.dll")) -and (Test-Path (Join-Path $Game "MelonLoader\net6\MelonLoader.dll"))) {
        Write-Host "MelonLoader is already installed."
        return
    }
    $zip = Join-Path $env:TEMP "MelonLoader.x64.zip"
    Write-Host "Downloading MelonLoader 0.7.3..."
    Invoke-WebRequest -Uri $MelonLoaderUrl -OutFile $zip -UseBasicParsing
    $hash = (Get-FileHash -Path $zip -Algorithm SHA256).Hash
    if ($hash -ne $MelonLoaderSha256) {
        Remove-Item $zip -Force
        throw "MelonLoader download failed its checksum (got $hash). Nothing was installed."
    }
    Expand-Archive -Path $zip -DestinationPath $Game -Force
    Remove-Item $zip -Force
    Write-Host "MelonLoader installed."
}

function Initialize-GameAssemblies([string]$Game) {
    $marker = Join-Path $Game "MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll"
    if (Test-Path $marker) {
        Write-Host "Game assemblies already generated."
        return
    }
    Write-Host "Launching the game once so MelonLoader can generate its assemblies (takes a minute or two)..."
    $log = Join-Path $Game "MelonLoader\Latest.log"
    Start-Process "steam://rungameid/$SteamAppId"
    $deadline = (Get-Date).AddMinutes(15)
    while ((Get-Date) -lt $deadline) {
        Start-Sleep -Seconds 3
        if ((Test-Path $marker) -and (Test-Path $log) -and (Select-String -Path $log -Pattern "Mods loaded" -Quiet)) { break }
    }
    if (-not (Test-Path $marker)) { throw "MelonLoader didn't generate its assemblies. Check $log." }
    Write-Host "Assemblies ready. Closing the game..."
    Get-Process -Name ([IO.Path]::GetFileNameWithoutExtension($GameExe)) -ErrorAction SilentlyContinue |
        ForEach-Object { [void]$_.CloseMainWindow() }
    Wait-GameClosed
}

function Test-DotnetSdk([string]$Dotnet) {
    try {
        $sdks = & $Dotnet --list-sdks 2>$null
        return [bool]($sdks | Where-Object { [int](($_ -split '\.')[0]) -ge $MinSdkMajor })
    } catch {
        return $false
    }
}

function Get-Dotnet {
    $system = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($system -and (Test-DotnetSdk $system.Source)) { return $system.Source }
    $local = Join-Path $ToolsDir "dotnet\dotnet.exe"
    if ((Test-Path $local) -and (Test-DotnetSdk $local)) { return $local }

    Write-Host "Installing a private .NET SDK into $ToolsDir\dotnet (no admin rights needed, ~250 MB)..."
    New-Item -ItemType Directory -Force -Path $ToolsDir | Out-Null
    $installer = Join-Path $ToolsDir "dotnet-install.ps1"
    Invoke-WebRequest -Uri $DotnetInstallUrl -OutFile $installer -UseBasicParsing
    $signature = Get-AuthenticodeSignature -FilePath $installer
    if ($signature.Status -ne "Valid" -or $signature.SignerCertificate.Subject -notmatch "O=Microsoft Corporation") {
        Remove-Item $installer -Force
        throw "The .NET installer script isn't signed by Microsoft; refusing to run it."
    }
    & $installer -Channel "10.0" -InstallDir (Join-Path $ToolsDir "dotnet") -NoPath
    if (-not (Test-DotnetSdk $local)) { throw ".NET SDK installation failed." }
    return $local
}

function Build-Mod([string]$Dotnet, [string]$Game) {
    $project = Join-Path $RepoRoot "src\RouletteRevival\RouletteRevival.csproj"
    if (-not (Test-Path $project)) { throw "Mod source not found at $project. Run install.bat from the extracted release folder." }
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
    $env:DOTNET_NOLOGO = "1"
    & $Dotnet build $project -c Release -nologo "-p:GameDir=$Game" "-p:Deploy=true"
    if ($LASTEXITCODE -ne 0) { throw "Building the mod failed (see the errors above)." }
    $deployed = Join-Path $Game "Mods\RouletteRevival.dll"
    if (-not (Test-Path $deployed)) { throw "Build succeeded but $deployed is missing." }
    return $deployed
}

try {
    Write-Host "Roulette Revival installer" -ForegroundColor Green
    Write-Step "Finding Sledding Game"
    if (-not $GameDir) { $GameDir = Find-GameDir }
    if (-not (Test-Path (Join-Path $GameDir $GameExe))) { throw "'$GameDir' doesn't contain $GameExe." }
    Write-Host $GameDir
    Wait-GameClosed

    Write-Step "MelonLoader"
    Install-MelonLoader $GameDir
    Initialize-GameAssemblies $GameDir

    Write-Step ".NET SDK"
    $dotnet = Get-Dotnet
    Write-Host "Using $dotnet"

    Write-Step "Building the mod against your game"
    $dll = Build-Mod $dotnet $GameDir

    Write-Host ""
    Write-Host "Done! Installed $dll" -ForegroundColor Green
    Write-Host "Host a lobby to get the roulette table. Everyone who wants to bet needs the mod too."
    exit 0
} catch {
    Write-Host ""
    Write-Host "Install failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

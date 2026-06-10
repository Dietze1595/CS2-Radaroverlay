# Baut eine portable, eigenständige RadarOverlay.exe (kein installiertes .NET nötig).
# Ergebnis liegt in .\publish\ -> einfach RadarOverlay.exe doppelklicken.

$ErrorActionPreference = "Stop"
$out = Join-Path $PSScriptRoot "publish"

dotnet publish (Join-Path $PSScriptRoot "RadarOverlay.csproj") `
    -c Release `
    -r win-x64 `
    --self-contained `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $out

Write-Host ""
Write-Host "Fertig. Starte:  $out\RadarOverlay.exe" -ForegroundColor Green

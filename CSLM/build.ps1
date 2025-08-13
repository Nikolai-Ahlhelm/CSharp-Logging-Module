# build.ps1     v1.1.0

# Eingabe: Produktversion
$version = Read-Host "🏷️ Version (e.g. 1.0.0): "

# Buildnummer erzeugen: yyDDDHH
$now = Get-Date
$year = $now.ToString("yy")
$dayOfYear = $now.DayOfYear.ToString("D3")
$hour = $now.ToString("HH")
$buildNumber = "$year$dayOfYear$hour"
Write-Host "🔢 Build number generated: $buildNumber" -ForegroundColor Yellow

$informationalVersion = "$version+$buildNumber"
Write-Host "🏗️ Generating version: $informationalVersion ..." -ForegroundColor Yellow

# --- Clean bin/obj ---
Write-Host "🧹 Cleaning old build folders..." -ForegroundColor Yellow
$dirsToClean = @("bin", "obj")
foreach ($dir in $dirsToClean) {
    if (Test-Path $dir) {
        Remove-Item $dir -Recurse -Force
    }
}

# --- Output-Verzeichnis eine Ebene höher ---
$projectRoot = Split-Path -Parent $PSCommandPath
$outputRoot  = Join-Path $projectRoot "..\artifacts"
$outputDir   = Join-Path $outputRoot "$informationalVersion"

if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

# --- Build ---
Write-Host "📦 Building project..." -ForegroundColor Yellow
dotnet publish `
    -c Release `
    -o $outputDir `
    -p:InformationalVersion=$informationalVersion `
    -p:Version=$version `
    -p:AssemblyVersion=$version `
    -p:FileVersion=$version
Write-Host "📦 Build completed." -ForegroundColor Yellow

# --- example.ps1 mitkopieren ---
$exampleScript = Join-Path $projectRoot "example.ps1"
if (Test-Path $exampleScript) {
    Copy-Item $exampleScript $outputDir -Force
    Write-Host "📄 example.ps1 copied to output folder." -ForegroundColor Yellow
} else {
    Write-Host "⚠️ example.ps1 not found in project directory." -ForegroundColor Red
}

# --- Version-Datei ---
Set-Content -Path (Join-Path $outputDir "version.txt") -Value $informationalVersion

# --- ZIP erstellen ---
Write-Host "🗃️ Creating archive file..." -ForegroundColor Yellow
$zipPath = "$outputDir.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
Compress-Archive -Path "$outputDir\*" -DestinationPath $zipPath
Write-Host "🗃️ Archive created" -ForegroundColor Yellow

Write-Host "`n✅ Build and packaging completed successfully! 🎉" -ForegroundColor Green
Write-Host "📂 Output directory: $outputDir" -ForegroundColor Green
Write-Host "📦 ZIP file: $zipPath" -ForegroundColor Green
pause
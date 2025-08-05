# build.ps1     v1.0.0

# Input product version
$version = Read-Host "🏷️ Version (e.g. 1.0.0): "

# generate buildnumber: yyDDDHH (z. B. 2513009 = 2025, Tag 130, 09 Uhr)
$now = Get-Date
$year = $now.ToString("yy")
$dayOfYear = $now.DayOfYear.ToString("D3")
$hour = $now.ToString("HH")
$buildNumber = "$year$dayOfYear$hour"
Write-Host "🔢 Build number generated: $buildNumber" -ForegroundColor Yellow


$informationalVersion = "$version+$buildNumber"
Write-Host "🏗️ Generating version: $informationalVersion ..." -ForegroundColor Yellow

# targetfolder
$outputDir = Join-Path -Path "artifacts" -ChildPath "$informationalVersion"
if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

# build command
Write-Host "📦 Building project..." -ForegroundColor Yellow
dotnet publish `
    -c Release `
    -o $outputDir `
    -p:InformationalVersion=$informationalVersion `
    -p:Version=$version `
    -p:AssemblyVersion=$version `
    -p:FileVersion=$version
Write-Host "📦 Build completed." -ForegroundColor Yellow

# create version.txt
Set-Content -Path (Join-Path $outputDir "version.txt") -Value $informationalVersion

# ZIP-Datei erstellen
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
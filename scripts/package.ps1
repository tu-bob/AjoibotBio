$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo = Split-Path -Parent $root
Set-Location $repo

$publishBase = Join-Path $repo "publish"
$zips = @(
    @{ folder = Join-Path $publishBase 'win-x64'; zip = Join-Path $publishBase 'AjoibotBio-win-x64.zip' },
    @{ folder = Join-Path $publishBase 'win-x86'; zip = Join-Path $publishBase 'AjoibotBio-win-x86.zip' }
)

foreach ($z in $zips) {
    if (-not (Test-Path $z.folder)) {
        Write-Warning "Skip packaging: folder not found $($z.folder). Run scripts\\publish.ps1 first."
        continue
    }
    if (Test-Path $z.zip) { Remove-Item $z.zip -Force }
    Write-Host "Creating archive $($z.zip) from $($z.folder)" -ForegroundColor Cyan
    Compress-Archive -Path (Join-Path $z.folder '*') -DestinationPath $z.zip
}

Write-Host "Done. ZIP files are in $publishBase" -ForegroundColor Green

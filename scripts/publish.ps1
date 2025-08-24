param(
    [switch]$SelfContained,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo = Split-Path -Parent $root
Set-Location $repo

$scFlag = if ($SelfContained) { "true" } else { "false" }

$publishBase = Join-Path $repo "publish"
New-Item -ItemType Directory -Force -Path $publishBase | Out-Null

$targets = @(
    @{ rid = 'win-x64'; out = Join-Path $publishBase 'win-x64' },
    @{ rid = 'win-x86'; out = Join-Path $publishBase 'win-x86' }
)

foreach ($t in $targets) {
    Write-Host "Publishing $($t.rid) (SelfContained=$scFlag, Configuration=$Configuration)" -ForegroundColor Cyan
    dotnet publish "AjoibotBio\AjoibotBio.csproj" -c $Configuration -r $t.rid --self-contained $scFlag -o $t.out
}

Write-Host "Done. Artifacts in $publishBase" -ForegroundColor Green

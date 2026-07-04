# build-installers.ps1 - 编译三个架构的安装包
param(
    [string]$Version = "0.0.1"
)

$ErrorActionPreference = "Stop"
$ISCC = "C:\Users\Liham\AppData\Local\Programs\Inno Setup 6\ISCC.exe"

if (-not (Test-Path $ISCC)) {
    Write-Host "ERROR: Inno Setup not found at $ISCC" -ForegroundColor Red
    exit 1
}

$architectures = @(
    @{ Arch = "x64";   Rid = "win-x64" }
    @{ Arch = "x86";   Rid = "win-x86" }
    @{ Arch = "arm64"; Rid = "win-arm64" }
)

foreach ($arch in $architectures) {
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  Building installer: $($arch.Arch)" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan

    # Check if publish output exists
    $publishDir = "publish\$($arch.Rid)"
    if (-not (Test-Path "$publishDir\BookmarkManager.exe")) {
        Write-Host "  ERROR: $publishDir\BookmarkManager.exe not found!" -ForegroundColor Red
        Write-Host "  Run publish-all.ps1 first." -ForegroundColor Yellow
        exit 1
    }

    # Compile installer
    & $ISCC "installer-arch.iss" "/DArch=$($arch.Arch)" "/DRid=$($arch.Rid)" 2>&1 | ForEach-Object {
        if ($_ -match "error") { Write-Host "  $_" -ForegroundColor Red }
        elseif ($_ -match "warning") { Write-Host "  $_" -ForegroundColor Yellow }
        else { Write-Host "  $_" }
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ERROR: Compile failed for $($arch.Arch)!" -ForegroundColor Red
        exit 1
    }

    # Verify output
    $setup = "bin\Release\BookmarkManager-Setup-$($arch.Arch).exe"
    if (Test-Path $setup) {
        $size = [math]::Round((Get-Item $setup).Length / 1MB, 1)
        Write-Host "  OK: $setup ($size MB)" -ForegroundColor Green
    } else {
        Write-Host "  ERROR: Setup file not found!" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Green
Write-Host "  All installers built!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output files:" -ForegroundColor White
foreach ($arch in $architectures) {
    $setup = "bin\Release\BookmarkManager-Setup-$($arch.Arch).exe"
    $size = [math]::Round((Get-Item $setup).Length / 1MB, 1)
    Write-Host "  $setup ($size MB)" -ForegroundColor White
}

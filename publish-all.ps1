# publish-all.ps1 - 发布所有架构版本
param(
    [string]$Version = "0.0.1"
)

$ErrorActionPreference = "Stop"
$rids = @(
    @{ Name = "x64";    Rid = "win-x64";    Suffix = "-x64" }
    @{ Name = "x86";    Rid = "win-x86";    Suffix = "-x86" }
    @{ Name = "ARM64";  Rid = "win-arm64";  Suffix = "-arm64" }
)

foreach ($arch in $rids) {
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  Publishing $($arch.Name) ($($arch.Rid))" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan

    $outDir = "publish\$($arch.Rid)"

    # Clean
    Remove-Item -Recurse -Force $outDir -ErrorAction SilentlyContinue

    # Publish
    Write-Host "  Building..."
    dotnet publish -c Release -r $arch.Rid --self-contained `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -o $outDir 2>&1 | ForEach-Object { Write-Host "  $_" }

    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ERROR: Publish failed for $($arch.Name)!" -ForegroundColor Red
        exit 1
    }

    # Verify
    $exe = Join-Path $outDir "BookmarkManager.exe"
    if (Test-Path $exe) {
        $size = [math]::Round((Get-Item $exe).Length / 1MB, 1)
        Write-Host "  OK: BookmarkManager.exe ($size MB)" -ForegroundColor Green
    } else {
        Write-Host "  ERROR: BookmarkManager.exe not found!" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  All architectures published!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

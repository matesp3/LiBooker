# Robust per-table scaffold tester that finds dotnet.exe and runs per-table scaffolding
param(
    [string] $Schema = "NEUPAUER",
    [string] $OutDir = "Models\Scaffolded\All",
    [string] $ContextName = "AppDbContext"
)

# locate dotnet
function Get-DotNetExe {
    $cmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $candidates = @(
        "$env:ProgramFiles\dotnet\dotnet.exe",
        "$env:ProgramFiles(x86)\dotnet\dotnet.exe",
        "$env:ProgramW6432\dotnet\dotnet.exe"
    )
    foreach ($p in $candidates) { if (Test-Path $p) { return $p } }
    return $null
}

$dotnet = Get-DotNetExe
if (-not $dotnet) {
    Write-Host "ERROR: 'dotnet' not found on PATH. Run from Developer PowerShell or install .NET SDK." -ForegroundColor Red
    exit 2
}

# ensure dotnet-ef tool available
& $dotnet ef --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: 'dotnet ef' not available. Install: dotnet tool install --global dotnet-ef --version 9.0.11" -ForegroundColor Red
    exit 3
}

# prepare output folder
if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }
$log = Join-Path $OutDir "scaffold.log"

# build args for full-schema scaffold (no --table)
$efArgs = @(
    'ef','dbcontext','scaffold',
    $env:DB_CONN,
    'Oracle.EntityFrameworkCore',
    '--context', $ContextName,
    '--context-dir', 'Models',
    '--output-dir', $OutDir,
    '--no-onconfiguring',
    '--force',
    '--use-database-names',
    '--verbose'
)

if ($Schema) { $efArgs += @('--schema', $Schema) }

Write-Host "Running scaffold for all accessible tables (log: $log)" -ForegroundColor Cyan
Write-Host "$dotnet $($efArgs -join ' ')" -ForegroundColor DarkGray

# execute and capture
$output = & $dotnet @efArgs 2>&1
$output | Tee-Object -FilePath $log | ForEach-Object { $_ }

if ($LASTEXITCODE -eq 0) {
    Write-Host "Scaffold completed successfully. Generated files are in $OutDir." -ForegroundColor Green
    exit 0
} else {
    Write-Host "Scaffold failed (exit code $LASTEXITCODE). See tail of log:" -ForegroundColor Red
    Get-Content -Path $log -Tail 80 | ForEach-Object { Write-Host $_ }
    exit $LASTEXITCODE
}
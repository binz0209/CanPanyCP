# ============================================================
#  CanPany – Start All Services (ngrok + API + Worker + FE)
# ============================================================
#  Usage:  .\run.ps1          (run all)
#          .\run.ps1 -NoNgrok (skip ngrok)
# ============================================================

param(
    [switch]$NoNgrok
)

$ErrorActionPreference = "Stop"
$Host.UI.RawUI.WindowTitle = "CanPany – Launcher"

# ── Paths ────────────────────────────────────────────────────
$ROOT      = $PSScriptRoot
$API_DIR   = Join-Path $ROOT "CanPany-BE\CanPany.Api"
$WORKER_DIR= Join-Path $ROOT "CanPany-BE\CanPany.Worker"
$FE_DIR    = Join-Path $ROOT "CanPany-FE"

# ── Colors ───────────────────────────────────────────────────
function Write-Header($text) {
    Write-Host ""
    Write-Host "  ★  $text" -ForegroundColor Cyan
    Write-Host ("  " + "─" * 50) -ForegroundColor DarkGray
}

function Write-Status($service, $detail) {
    Write-Host "  ✓  " -ForegroundColor Green -NoNewline
    Write-Host "$service" -ForegroundColor Yellow -NoNewline
    Write-Host " → $detail" -ForegroundColor Gray
}

# ── Banner ───────────────────────────────────────────────────
Clear-Host
Write-Host ""
Write-Host "  ╔══════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "  ║          🚀  CanPany Dev Launcher  🚀       ║" -ForegroundColor Magenta
Write-Host "  ╚══════════════════════════════════════════════╝" -ForegroundColor Magenta
Write-Host ""

$pids = @()

# ── 1. Ngrok (tunnel API port 5001) ─────────────────────────
if (-not $NoNgrok) {
    Write-Header "Starting ngrok (port 5001)"
    try {
        $ngrokProc = Start-Process -FilePath "ngrok" `
            -ArgumentList "http", "5001" `
            -WindowStyle Minimized `
            -PassThru
        $pids += $ngrokProc.Id
        Start-Sleep -Seconds 2
        
        # Try to fetch the public URL from ngrok API
        try {
            $tunnels = Invoke-RestMethod -Uri "http://127.0.0.1:4040/api/tunnels" -TimeoutSec 5
            $publicUrl = ($tunnels.tunnels | Where-Object { $_.proto -eq "https" } | Select-Object -First 1).public_url
            if ($publicUrl) {
                Write-Status "ngrok" "Public URL: $publicUrl"
            } else {
                Write-Status "ngrok" "Started (check http://127.0.0.1:4040 for URL)"
            }
        } catch {
            Write-Status "ngrok" "Started (inspect at http://127.0.0.1:4040)"
        }
    } catch {
        Write-Host "  ⚠  ngrok not found or failed to start. Skipping..." -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⏭  Skipping ngrok (--NoNgrok flag)" -ForegroundColor DarkGray
}

# ── 2. Backend API (dotnet run) ──────────────────────────────
Write-Header "Starting Backend API"
$apiProc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run", "--project", "`"$API_DIR`"", "--launch-profile", "http" `
    -WorkingDirectory $ROOT `
    -WindowStyle Minimized `
    -PassThru
$pids += $apiProc.Id
Write-Status "API" "http://localhost:5001  (PID: $($apiProc.Id))"

# ── 3. Background Worker (dotnet run) ────────────────────────
Write-Header "Starting Background Worker"
$workerProc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run", "--project", "`"$WORKER_DIR`"" `
    -WorkingDirectory $ROOT `
    -WindowStyle Minimized `
    -PassThru
$pids += $workerProc.Id
Write-Status "Worker" "Background jobs  (PID: $($workerProc.Id))"

# ── 4. Frontend (npm run dev) ────────────────────────────────
Write-Header "Starting Frontend"
$feProc = Start-Process -FilePath "cmd.exe" `
    -ArgumentList "/c", "npm run dev" `
    -WorkingDirectory $FE_DIR `
    -WindowStyle Minimized `
    -PassThru
$pids += $feProc.Id
Write-Status "Frontend" "http://localhost:5173  (PID: $($feProc.Id))"

# ── Summary ──────────────────────────────────────────────────
Write-Host ""
Write-Host "  ╔══════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "  ║       All services started successfully!     ║" -ForegroundColor Green
Write-Host "  ╠══════════════════════════════════════════════╣" -ForegroundColor Green
Write-Host "  ║  Frontend  → http://localhost:5173           ║" -ForegroundColor Green
Write-Host "  ║  API       → http://localhost:5001           ║" -ForegroundColor Green
Write-Host "  ║  Swagger   → http://localhost:5001/swagger   ║" -ForegroundColor Green
if (-not $NoNgrok) {
Write-Host "  ║  ngrok     → http://127.0.0.1:4040          ║" -ForegroundColor Green
}
Write-Host "  ╚══════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "  Press Ctrl+C or close this window to stop all." -ForegroundColor DarkGray
Write-Host ""

# ── Wait & Cleanup ───────────────────────────────────────────
try {
    while ($true) { Start-Sleep -Seconds 30 }
} finally {
    Write-Host ""
    Write-Host "  Shutting down all services..." -ForegroundColor Yellow

    # Kill tracked PIDs
    foreach ($procId in $pids) {
        try {
            Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
        } catch { }
    }

    # Also kill any leftover dotnet/node/ngrok processes from this session
    @("ngrok", "node") | ForEach-Object {
        Get-Process -Name $_ -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    }

    Write-Host "  Done. Goodbye! 👋" -ForegroundColor Cyan
}

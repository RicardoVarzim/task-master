# Script to stop Task Master API
$originalDirectory = Get-Location

try {
    Write-Host "Stopping Task Master API..." -ForegroundColor Yellow

    # Try to find processes using port 5000
    $port = 5000
    $pidsToStop = @()
    
    # Method 1: Get-NetTCPConnection
    $connections = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Where-Object {$_.State -eq "Listen"}
    if ($connections) {
        $pidsToStop += $connections | ForEach-Object { $_.OwningProcess }
    }
    
    # Method 2: netstat (fallback)
    $netstatOutput = netstat -ano | Select-String ":$port " | Select-String "LISTENING"
    if ($netstatOutput) {
        $pids = $netstatOutput | ForEach-Object { ($_ -split '\s+')[-1] } | Select-Object -Unique
        $pidsToStop += $pids
    }
    
    $pidsToStop = $pidsToStop | Select-Object -Unique
    
    if ($pidsToStop) {
        foreach ($pid in $pidsToStop) {
            try {
                $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
                if ($process) {
                    Write-Host "Stopping process $($process.ProcessName) (PID: $pid) using port $port" -ForegroundColor Cyan
                    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
                }
            } catch {
                Write-Host "Could not stop process PID $pid : $($_)" -ForegroundColor Red
            }
        }
    }

    # Also try to stop dotnet processes that might be running the API
    $dotnetProcesses = Get-Process dotnet -ErrorAction SilentlyContinue
    foreach ($proc in $dotnetProcesses) {
        try {
            $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($proc.Id)").CommandLine
            if ($cmdLine -like "*TaskMaster.API*") {
                Write-Host "Stopping dotnet process (PID: $($proc.Id)) running TaskMaster.API" -ForegroundColor Cyan
                Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            }
        } catch {
            # Ignore errors when accessing process information
        }
    }

    Start-Sleep -Seconds 2
    Write-Host "Processes stopped!" -ForegroundColor Green
}
finally {
    # Restore original directory even if script fails
    Set-Location $originalDirectory -ErrorAction SilentlyContinue
}

# Script to stop all Task Master processes
$originalDirectory = Get-Location

try {
    Write-Host "Stopping Task Master processes..." -ForegroundColor Yellow

    # Stop TaskMaster.API processes
    $apiProcesses = Get-Process | Where-Object {$_.ProcessName -like "*TaskMaster.API*" -or $_.MainWindowTitle -like "*TaskMaster*"}
    if ($apiProcesses) {
        Write-Host "Found API processes, stopping..." -ForegroundColor Cyan
        $apiProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
    }

    # Stop dotnet processes related to TaskMaster
    $dotnetProcesses = Get-Process dotnet -ErrorAction SilentlyContinue | Where-Object {
        $_.Path -like "*task-master*"
    }
    if ($dotnetProcesses) {
        Write-Host "Found TaskMaster dotnet processes, stopping..." -ForegroundColor Cyan
        $dotnetProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
    }

    # Stop processes on port 5000 (API)
    $port5000 = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue | Where-Object {$_.State -eq "Listen"}
    if ($port5000) {
        foreach ($conn in $port5000) {
            $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "Stopping process on port 5000 (PID: $($process.Id))" -ForegroundColor Cyan
                Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
            }
        }
    }

    # Stop processes on port 5001 (Blazor)
    $port5001 = Get-NetTCPConnection -LocalPort 5001 -ErrorAction SilentlyContinue | Where-Object {$_.State -eq "Listen"}
    if ($port5001) {
        foreach ($conn in $port5001) {
            $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "Stopping process on port 5001 (PID: $($process.Id))" -ForegroundColor Cyan
                Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
            }
        }
    }

    # Wait a bit to ensure processes are terminated
    Start-Sleep -Seconds 2

    Write-Host "All processes stopped!" -ForegroundColor Green
}
finally {
    # Restore original directory even if script fails
    Set-Location $originalDirectory -ErrorAction SilentlyContinue
}

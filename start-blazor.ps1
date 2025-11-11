# Script to start Task Master Blazor
$originalDirectory = Get-Location

try {
    Write-Host "Starting Task Master Blazor..." -ForegroundColor Green

    # Check if port 5001 is in use
    $port = 5001
    Write-Host "Checking if port $port is in use..." -ForegroundColor Gray
    
    # Try multiple methods to find processes using the port
    $connections = @()
    
    # Method 1: Get-NetTCPConnection
    $connections += Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Where-Object {$_.State -eq "Listen"}
    
    # Method 2: netstat (fallback)
    if (-not $connections) {
        $netstatOutput = netstat -ano | Select-String ":$port " | Select-String "LISTENING"
        if ($netstatOutput) {
            $pids = $netstatOutput | ForEach-Object { ($_ -split '\s+')[-1] } | Select-Object -Unique
            foreach ($pid in $pids) {
                try {
                    $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
                    if ($proc) {
                        $connections += [PSCustomObject]@{OwningProcess = $pid}
                    }
                } catch {}
            }
        }
    }

    if ($connections) {
        Write-Host "Warning: Port $port is in use. Stopping processes..." -ForegroundColor Yellow
        foreach ($conn in $connections) {
            $pid = $conn.OwningProcess
            try {
                $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
                if ($process) {
                    Write-Host "Stopping process $($process.ProcessName) (PID: $pid) using port $port" -ForegroundColor Cyan
                    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
                }
            } catch {
                Write-Host "Could not stop process PID $pid: $_" -ForegroundColor Red
            }
        }
        Start-Sleep -Seconds 3
        
        # Verify port is free
        $stillInUse = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Where-Object {$_.State -eq "Listen"}
        if ($stillInUse) {
            Write-Host "Warning: Port $port is still in use. You may need to stop processes manually." -ForegroundColor Red
            Write-Host "Run: .\stop-blazor.ps1" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Port $port is available." -ForegroundColor Green
    }

    # Check if there are existing processes running
    $existingProcesses = Get-Process | Where-Object {
        ($_.ProcessName -like "*TaskMaster.Blazor*") -or 
        ($_.ProcessName -eq "dotnet" -and $_.Path -like "*task-master*TaskMaster.Blazor*")
    } -ErrorAction SilentlyContinue

    if ($existingProcesses) {
        Write-Host "Warning: Existing processes found. Stopping..." -ForegroundColor Yellow
        $existingProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    }

    Write-Host "Blazor will be available at: http://localhost:5001" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Make sure the API is running on port 5000!" -ForegroundColor Cyan
    Write-Host ""
    
    Set-Location src\TaskMaster.Blazor\TaskMaster.Blazor
    dotnet run
}
finally {
    # Restore original directory even if script fails
    Set-Location $originalDirectory -ErrorAction SilentlyContinue
}

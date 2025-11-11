# Script to start Task Master API
$originalDirectory = Get-Location

try {
    Write-Host "Starting Task Master API..." -ForegroundColor Green

    # Check if port 5000 is in use
    $port = 5000
    Write-Host "Checking if port $port is in use..." -ForegroundColor Gray
    
    # Try multiple methods to find processes using the port
    $connections = @()
    $pidsToStop = @()
    
    # Method 1: Get-NetTCPConnection - check ALL states, not just Listen
    $allConnections = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($allConnections) {
        $pidsToStop += $allConnections | ForEach-Object { $_.OwningProcess } | Select-Object -Unique
        $connections += $allConnections
    }
    
    # Method 2: netstat (fallback) - check all states
    $netstatOutput = netstat -ano | Select-String ":$port "
    if ($netstatOutput) {
        $pids = $netstatOutput | ForEach-Object { 
            $parts = $_ -split '\s+'
            $parts[-1]
        } | Select-Object -Unique
        foreach ($pid in $pids) {
            if ($pid -match '^\d+$') {
                try {
                    $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
                    if ($proc) {
                        $pidsToStop += $pid
                        if (-not ($connections | Where-Object { $_.OwningProcess -eq $pid })) {
                            $connections += [PSCustomObject]@{OwningProcess = $pid}
                        }
                    }
                } catch {}
            }
        }
    }
    
    # Method 3: Check for dotnet processes that might be using the port
    $dotnetProcesses = Get-Process dotnet -ErrorAction SilentlyContinue
    foreach ($proc in $dotnetProcesses) {
        try {
            $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($proc.Id)" -ErrorAction SilentlyContinue).CommandLine
            if ($cmdLine -like "*TaskMaster.API*" -or $cmdLine -like "*5000*") {
                Write-Host "Found dotnet process (PID: $($proc.Id)) that might be using port $port" -ForegroundColor Yellow
                $pidsToStop += $proc.Id
            }
        } catch {
            # Ignore errors when accessing process information
        }
    }
    
    $pidsToStop = $pidsToStop | Select-Object -Unique

    if ($pidsToStop.Count -gt 0) {
        Write-Host "Warning: Port $port is in use or related processes found. Stopping processes..." -ForegroundColor Yellow
        foreach ($pid in $pidsToStop) {
            try {
                $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
                if ($process) {
                    Write-Host "Stopping process $($process.ProcessName) (PID: $pid)" -ForegroundColor Cyan
                    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
                }
            } catch {
                Write-Host "Could not stop process PID $pid : $($_)" -ForegroundColor Red
            }
        }
        Start-Sleep -Seconds 3
        
        # Verify port is free - check all states
        $stillInUse = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
        if ($stillInUse) {
            Write-Host "Warning: Port $port is still in use. You may need to stop processes manually." -ForegroundColor Red
            Write-Host "Run: .\stop-api.ps1" -ForegroundColor Yellow
            exit 1
        } else {
            Write-Host "Port $port is now available." -ForegroundColor Green
        }
    } else {
        Write-Host "Port $port is available." -ForegroundColor Green
    }

    # Check if there are existing processes running (additional check)
    $existingProcesses = Get-Process | Where-Object {
        ($_.ProcessName -like "*TaskMaster.API*") -or 
        ($_.ProcessName -eq "dotnet" -and $_.Path -like "*task-master*TaskMaster.API*")
    } -ErrorAction SilentlyContinue

    if ($existingProcesses) {
        Write-Host "Warning: Additional existing processes found. Stopping..." -ForegroundColor Yellow
        foreach ($proc in $existingProcesses) {
            try {
                Write-Host "Stopping process $($proc.ProcessName) (PID: $($proc.Id))" -ForegroundColor Cyan
                Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            } catch {
                Write-Host "Could not stop process PID $($proc.Id) : $($_)" -ForegroundColor Red
            }
        }
        Start-Sleep -Seconds 2
        
        # Final check - verify port is free before starting
        $finalCheck = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
        if ($finalCheck) {
            Write-Host "Error: Port $port is still in use after cleanup attempts." -ForegroundColor Red
            Write-Host "Please run: .\stop-api.ps1" -ForegroundColor Yellow
            exit 1
        }
    }

    Write-Host "API will be available at: http://localhost:5000" -ForegroundColor Yellow
    Write-Host "Swagger will be available at: http://localhost:5000/swagger" -ForegroundColor Yellow
    Write-Host ""
    
    Set-Location src\TaskMaster.API\TaskMaster.API
    dotnet run
}
finally {
    # Restore original directory even if script fails
    Set-Location $originalDirectory -ErrorAction SilentlyContinue
}

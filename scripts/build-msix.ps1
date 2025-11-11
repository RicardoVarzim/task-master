# Script to create MSIX package for Task Master
# Requires: Windows SDK, .NET 8 SDK

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = ".\dist\msix",
    [switch]$SkipMSIX = $false
)

$ErrorActionPreference = "Stop"

Write-Host "=== Task Master MSIX Build Script ===" -ForegroundColor Cyan

# Check prerequisites
Write-Host "`nChecking prerequisites..." -ForegroundColor Yellow

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: .NET SDK not found!" -ForegroundColor Red
    exit 1
}

# Create directories
$packageDir = Join-Path $OutputPath "package"
$assetsDir = Join-Path $packageDir "Assets"

# Try to close running processes before cleaning
Write-Host "`nChecking for running processes..." -ForegroundColor Yellow

# Close TaskMaster.Host first
$hostProcesses = Get-Process -Name "TaskMaster.Host" -ErrorAction SilentlyContinue
if ($hostProcesses) {
    Write-Host "Closing TaskMaster.Host processes..." -ForegroundColor Yellow
    $hostProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}

# Close dotnet processes that may be running API/Worker/Blazor
# Note: This may close other dotnet processes too, but it's necessary to ensure files are not locked
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    Write-Host "WARNING: Found running dotnet processes. They will be closed to allow the build." -ForegroundColor Yellow
    Write-Host "If you have other .NET projects running, close them manually before continuing." -ForegroundColor Yellow
    $dotnetProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 3
}

# Wait a bit more to ensure processes are closed
Start-Sleep -Seconds 1

# Check if there are still locked files
if (Test-Path $packageDir) {
    $lockedFiles = @()
    Get-ChildItem -Path $packageDir -Filter "*.dll" -ErrorAction SilentlyContinue | ForEach-Object {
        try {
            $fileStream = [System.IO.File]::Open($_.FullName, 'Open', 'ReadWrite', 'None')
            $fileStream.Close()
        } catch {
            $lockedFiles += $_.Name
        }
    }
    
    if ($lockedFiles.Count -gt 0) {
        Write-Host "WARNING: Some files are still locked: $($lockedFiles -join ', ')" -ForegroundColor Yellow
        Write-Host "Attempting to remove directory anyway..." -ForegroundColor Yellow
    }
}

if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
}

New-Item -ItemType Directory -Path $packageDir -Force | Out-Null
New-Item -ItemType Directory -Path $assetsDir -Force | Out-Null

Write-Host "`n1. Publishing Host application..." -ForegroundColor Yellow
$hostProject = "src\TaskMaster.Host\TaskMaster.Host.csproj"
dotnet publish $hostProject -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true -o $packageDir

Write-Host "`n1.1. Publishing API (without single-file)..." -ForegroundColor Yellow
$apiProject = "src\TaskMaster.API\TaskMaster.API\TaskMaster.API.csproj"
dotnet publish $apiProject -c $Configuration -r win-x64 --self-contained true -o $packageDir

Write-Host "`n1.2. Publishing Worker (without single-file)..." -ForegroundColor Yellow
$workerProject = "src\TaskMaster.Worker\TaskMaster.Worker\TaskMaster.Worker.csproj"
dotnet publish $workerProject -c $Configuration -r win-x64 --self-contained true -o $packageDir

Write-Host "`n1.3. Publishing Blazor (without single-file)..." -ForegroundColor Yellow
$blazorProject = "src\TaskMaster.Blazor\TaskMaster.Blazor\TaskMaster.Blazor.csproj"
dotnet publish $blazorProject -c $Configuration -r win-x64 --self-contained true -o $packageDir

# Copy assets
Write-Host "`n2. Copying assets..." -ForegroundColor Yellow
$sourceAssets = "src\TaskMaster.Host\Assets"
if (Test-Path $sourceAssets) {
    Copy-Item "$sourceAssets\*" -Destination $assetsDir -Recurse -Force -Exclude "README-ICONS.md"
    
    # Verify required assets exist
    $requiredAssets = @("StoreLogo.png", "Square150x150Logo.png", "Square44x44Logo.png", "Wide310x150Logo.png", "SplashScreen.png")
    $missingAssets = @()
    foreach ($asset in $requiredAssets) {
        $assetPath = Join-Path $assetsDir $asset
        if (-not (Test-Path $assetPath)) {
            $missingAssets += $asset
        }
    }
    
    if ($missingAssets.Count -gt 0) {
        Write-Host "  ERROR: Missing required assets: $($missingAssets -join ', ')" -ForegroundColor Red
        Write-Host "  Run .\scripts\generate-assets.ps1 to create default assets." -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "  ERROR: Assets directory not found: $sourceAssets" -ForegroundColor Red
    Write-Host "  Run .\scripts\generate-assets.ps1 to create default assets." -ForegroundColor Yellow
    exit 1
}

# Copy manifest
Write-Host "`n3. Copying manifest..." -ForegroundColor Yellow
Copy-Item "src\TaskMaster.Host\Package.appxmanifest" -Destination $packageDir -Force
# Rename to AppxManifest.xml (required by MakeAppx.exe)
Rename-Item -Path (Join-Path $packageDir "Package.appxmanifest") -NewName "AppxManifest.xml" -Force

# Create certificate if it doesn't exist
$certPath = Join-Path $OutputPath "TaskMasterDemo.pfx"
if (-not (Test-Path $certPath)) {
    Write-Host "`n4. Creating self-signed certificate..." -ForegroundColor Yellow
    $cert = New-SelfSignedCertificate `
        -Type Custom `
        -Subject "CN=TaskMaster Demo" `
        -KeyUsage DigitalSignature `
        -FriendlyName "TaskMaster Demo Certificate" `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

    $password = ConvertTo-SecureString -String "TaskMaster123!" -Force -AsPlainText
    Export-PfxCertificate -Cert $cert -FilePath $certPath -Password $password
    
    # Export public certificate as well
    $cerPath = Join-Path $OutputPath "TaskMasterDemo.cer"
    Export-Certificate -Cert $cert -FilePath $cerPath
    
    Write-Host "Certificate created: $certPath" -ForegroundColor Green
    Write-Host "IMPORTANT: Install TaskMasterDemo.cer certificate in trusted root certificate store before installing the package!" -ForegroundColor Yellow
}

# Find MakeAppx.exe
Write-Host "`n5. Searching for MakeAppx.exe..." -ForegroundColor Yellow

# Try multiple locations
$sdkPaths = @(
    "${env:ProgramFiles(x86)}\Windows Kits\10\bin",
    "${env:ProgramFiles}\Windows Kits\10\bin",
    "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v10.0A\bin"
)

$makeAppx = $null
$sdkPath = $null

foreach ($path in $sdkPaths) {
    if (Test-Path $path) {
        $found = Get-ChildItem -Path $path -Filter "MakeAppx.exe" -Recurse -ErrorAction SilentlyContinue | 
            Sort-Object FullName -Descending | 
            Select-Object -First 1
        if ($found) {
            $makeAppx = $found
            $sdkPath = $path
            break
        }
    }
}

if (-not $makeAppx) {
    Write-Host "WARNING: MakeAppx.exe not found!" -ForegroundColor Yellow
    Write-Host "Windows SDK is not installed or not in the expected path." -ForegroundColor Yellow
    Write-Host "`nOptions:" -ForegroundColor Yellow
    Write-Host "  1. Install Windows 10/11 SDK: https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/" -ForegroundColor White
    Write-Host "  2. Run with -SkipMSIX to only publish the application without creating the MSIX package" -ForegroundColor White
    Write-Host "`nApplication published to: $packageDir" -ForegroundColor Green
    
    if (-not $SkipMSIX) {
        Write-Host "`nTo continue without creating the MSIX package, run:" -ForegroundColor Yellow
        Write-Host "  .\scripts\build-msix.ps1 -SkipMSIX" -ForegroundColor White
        exit 1
    } else {
        Write-Host "`nContinuing without creating MSIX package..." -ForegroundColor Yellow
        Write-Host "`n=== Completed ===" -ForegroundColor Cyan
        Write-Host "Application published to: $packageDir" -ForegroundColor Green
        exit 0
    }
}

Write-Host "MakeAppx found: $($makeAppx.FullName)" -ForegroundColor Green

if ($SkipMSIX) {
    Write-Host "`nSkipping MSIX package creation (parameter -SkipMSIX)" -ForegroundColor Yellow
    Write-Host "`n=== Completed ===" -ForegroundColor Cyan
    Write-Host "Application published to: $packageDir" -ForegroundColor Green
    exit 0
}

# Create MSIX package
Write-Host "`n6. Creating MSIX package..." -ForegroundColor Yellow
$msixPath = Join-Path $OutputPath "TaskMaster_1.0.0.0_x64.msix"
& $makeAppx.FullName pack /d $packageDir /p $msixPath /o

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR creating MSIX package!" -ForegroundColor Red
    exit 1
}

# Sign package
Write-Host "`n7. Signing package..." -ForegroundColor Yellow
$signTool = Get-ChildItem -Path $sdkPath -Filter "signtool.exe" -Recurse -ErrorAction SilentlyContinue | 
    Sort-Object FullName -Descending | 
    Select-Object -First 1

if ($signTool) {
    $password = "TaskMaster123!"
    & $signTool.FullName sign /f $certPath /p $password /fd SHA256 $msixPath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Package signed successfully!" -ForegroundColor Green
    } else {
        Write-Host "WARNING: Failed to sign package. Package can still be installed in developer mode." -ForegroundColor Yellow
    }
} else {
    Write-Host "WARNING: SignTool not found. Package not signed." -ForegroundColor Yellow
}

Write-Host "`n=== Completed ===" -ForegroundColor Cyan
Write-Host "MSIX package created: $msixPath" -ForegroundColor Green
Write-Host "`nTo install:" -ForegroundColor Yellow
Write-Host "  1. Install TaskMasterDemo.cer certificate in trusted root certificate store" -ForegroundColor White
Write-Host "  2. Run: Add-AppxPackage -Path `"$msixPath`"" -ForegroundColor White

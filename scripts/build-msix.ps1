# Script para criar pacote MSIX do Task Master
# Requer: Windows SDK, .NET 8 SDK

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = ".\dist\msix"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Task Master MSIX Build Script ===" -ForegroundColor Cyan

# Verificar pré-requisitos
Write-Host "`nVerificando pré-requisitos..." -ForegroundColor Yellow

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "ERRO: .NET SDK não encontrado!" -ForegroundColor Red
    exit 1
}

# Criar diretórios
$packageDir = Join-Path $OutputPath "package"
$assetsDir = Join-Path $packageDir "Assets"

if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}

New-Item -ItemType Directory -Path $packageDir -Force | Out-Null
New-Item -ItemType Directory -Path $assetsDir -Force | Out-Null

Write-Host "`n1. Publicando aplicação..." -ForegroundColor Yellow
$hostProject = "src\TaskMaster.Host\TaskMaster.Host.csproj"
dotnet publish $hostProject -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true -o $packageDir

# Copiar assets
Write-Host "`n2. Copiando recursos..." -ForegroundColor Yellow
$sourceAssets = "src\TaskMaster.Host\Assets"
if (Test-Path $sourceAssets) {
    Copy-Item "$sourceAssets\*" -Destination $assetsDir -Recurse -Force
}

# Copiar manifest
Write-Host "`n3. Copiando manifest..." -ForegroundColor Yellow
Copy-Item "src\TaskMaster.Host\Package.appxmanifest" -Destination $packageDir -Force

# Criar certificado se não existir
$certPath = Join-Path $OutputPath "TaskMasterDemo.pfx"
if (-not (Test-Path $certPath)) {
    Write-Host "`n4. Criando certificado self-signed..." -ForegroundColor Yellow
    $cert = New-SelfSignedCertificate `
        -Type Custom `
        -Subject "CN=TaskMaster Demo" `
        -KeyUsage DigitalSignature `
        -FriendlyName "TaskMaster Demo Certificate" `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

    $password = ConvertTo-SecureString -String "TaskMaster123!" -Force -AsPlainText
    Export-PfxCertificate -Cert $cert -FilePath $certPath -Password $password
    
    # Exportar certificado público também
    $cerPath = Join-Path $OutputPath "TaskMasterDemo.cer"
    Export-Certificate -Cert $cert -FilePath $cerPath
    
    Write-Host "Certificado criado: $certPath" -ForegroundColor Green
    Write-Host "IMPORTANTE: Instale o certificado TaskMasterDemo.cer na raiz de certificados confiáveis antes de instalar o pacote!" -ForegroundColor Yellow
}

# Encontrar MakeAppx.exe
Write-Host "`n5. Procurando MakeAppx.exe..." -ForegroundColor Yellow
$sdkPath = "${env:ProgramFiles(x86)}\Windows Kits\10\bin"
$makeAppx = Get-ChildItem -Path $sdkPath -Filter "MakeAppx.exe" -Recurse -ErrorAction SilentlyContinue | 
    Sort-Object FullName -Descending | 
    Select-Object -First 1

if (-not $makeAppx) {
    Write-Host "ERRO: MakeAppx.exe não encontrado! Instale o Windows SDK." -ForegroundColor Red
    exit 1
}

Write-Host "MakeAppx encontrado: $($makeAppx.FullName)" -ForegroundColor Green

# Criar pacote MSIX
Write-Host "`n6. Criando pacote MSIX..." -ForegroundColor Yellow
$msixPath = Join-Path $OutputPath "TaskMaster_1.0.0.0_x64.msix"
& $makeAppx.FullName pack /d $packageDir /p $msixPath /o

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRO ao criar pacote MSIX!" -ForegroundColor Red
    exit 1
}

# Assinar pacote
Write-Host "`n7. Assinando pacote..." -ForegroundColor Yellow
$signTool = Get-ChildItem -Path $sdkPath -Filter "signtool.exe" -Recurse -ErrorAction SilentlyContinue | 
    Sort-Object FullName -Descending | 
    Select-Object -First 1

if ($signTool) {
    $password = "TaskMaster123!"
    & $signTool.FullName sign /f $certPath /p $password /fd SHA256 $msixPath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Pacote assinado com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "AVISO: Falha ao assinar pacote. O pacote ainda pode ser instalado em modo desenvolvedor." -ForegroundColor Yellow
    }
} else {
    Write-Host "AVISO: SignTool não encontrado. Pacote não assinado." -ForegroundColor Yellow
}

Write-Host "`n=== Concluído ===" -ForegroundColor Cyan
Write-Host "Pacote MSIX criado: $msixPath" -ForegroundColor Green
Write-Host "`nPara instalar:" -ForegroundColor Yellow
Write-Host "  1. Instale o certificado TaskMasterDemo.cer na raiz de certificados confiáveis" -ForegroundColor White
Write-Host "  2. Execute: Add-AppxPackage -Path `"$msixPath`"" -ForegroundColor White


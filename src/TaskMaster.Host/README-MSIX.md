# Task Master - Guia de Empacotamento MSIX

Este guia explica como criar um pacote MSIX para distribuição da aplicação Task Master.

## Pré-requisitos

1. **Windows 10/11 SDK** (versão 10.0.17763.0 ou superior)
2. **MakeAppx.exe** - Incluído no Windows SDK
3. **SignTool.exe** - Incluído no Windows SDK
4. **Certificado de assinatura** (self-signed para demo)

## Passos para Criar o Pacote MSIX

### 1. Criar Certificado Self-Signed (para demo)

```powershell
# Criar certificado
$cert = New-SelfSignedCertificate -Type Custom -Subject "CN=TaskMaster Demo" -KeyUsage DigitalSignature -FriendlyName "TaskMaster Demo Certificate" -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

# Exportar certificado para arquivo
$password = ConvertTo-SecureString -String "TaskMaster123!" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "TaskMasterDemo.pfx" -Password $password
```

### 2. Publicar a Aplicação

```powershell
cd src\TaskMaster.Host
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### 3. Criar Estrutura de Pacote

Crie uma pasta `package` com a seguinte estrutura:

```
package/
├── TaskMaster.Host.exe (e todas as DLLs necessárias)
├── TaskMaster.API.dll (e dependências)
├── TaskMaster.Blazor.dll (e dependências)
├── TaskMaster.Worker.dll (e dependências)
├── TaskMaster.Core.dll (e dependências)
├── Assets/
│   ├── icon.ico
│   ├── Square150x150Logo.png
│   ├── Square44x44Logo.png
│   ├── Wide310x150Logo.png
│   ├── SplashScreen.png
│   └── StoreLogo.png
└── Package.appxmanifest
```

### 4. Criar Pacote MSIX

```powershell
# Usar MakeAppx.exe do Windows SDK
$makeAppx = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\MakeAppx.exe"
& $makeAppx pack /d package /p TaskMaster_1.0.0.0_x64.msix
```

### 5. Assinar o Pacote

```powershell
# Usar SignTool.exe do Windows SDK
$signTool = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe"
& $signTool sign /f TaskMasterDemo.pfx /p TaskMaster123! /fd SHA256 TaskMaster_1.0.0.0_x64.msix
```

### 6. Instalar o Pacote

```powershell
# Instalar certificado (necessário apenas uma vez)
Import-Certificate -FilePath TaskMasterDemo.cer -CertStoreLocation Cert:\CurrentUser\Root

# Instalar pacote
Add-AppxPackage -Path TaskMaster_1.0.0.0_x64.msix
```

## Script Automatizado

Um script PowerShell completo está disponível em `scripts/build-msix.ps1`.

## Notas

- Para produção, use um certificado assinado por uma Autoridade Certificadora confiável
- O certificado self-signed é apenas para demonstração e desenvolvimento
- Usuários precisarão instalar o certificado na raiz de certificados confiáveis antes de instalar o pacote


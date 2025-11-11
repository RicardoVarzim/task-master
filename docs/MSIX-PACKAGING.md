# Task Master - Guia de Empacotamento MSIX

Este documento descreve o processo de empacotamento da aplicação Task Master como pacote MSIX para distribuição.

## Visão Geral

O Task Master é empacotado como uma aplicação MSIX que inclui:
- **TaskMaster.Host**: Aplicação WPF que gerencia os serviços
- **TaskMaster.API**: API REST em background
- **TaskMaster.Worker**: Serviço de monitorização de ficheiros
- **TaskMaster.Blazor**: Interface web Blazor Server

## Estrutura do Pacote

```
TaskMaster.Host/
├── MainWindow.xaml          # Interface principal
├── ServiceManager.cs         # Gerencia API, Worker e Blazor
├── Package.appxmanifest      # Manifest MSIX
├── app.manifest              # Manifest da aplicação
└── Assets/                   # Recursos visuais
    ├── icon.ico
    ├── Square150x150Logo.png
    ├── Square44x44Logo.png
    ├── Wide310x150Logo.png
    ├── SplashScreen.png
    └── StoreLogo.png
```

## Pré-requisitos

1. **Windows 10/11 SDK** (versão 10.0.17763.0 ou superior)
   - Download: https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/
   - Inclui MakeAppx.exe e SignTool.exe

2. **.NET 8 SDK**
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0

3. **PowerShell 5.1+** (já incluído no Windows)

## Processo de Build

### Método 1: Script Automatizado (Recomendado)

```powershell
# Executar script de build
.\scripts\build-msix.ps1

# O script irá:
# 1. Publicar a aplicação
# 2. Criar certificado self-signed (se não existir)
# 3. Criar pacote MSIX
# 4. Assinar o pacote
```

### Método 2: Manual

#### 1. Publicar Aplicação

```powershell
cd src\TaskMaster.Host
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

#### 2. Criar Estrutura de Pacote

Copie os arquivos necessários para uma pasta `package`:
- Todos os arquivos do publish
- Assets (ícones)
- Package.appxmanifest

#### 3. Criar Certificado Self-Signed

```powershell
$cert = New-SelfSignedCertificate `
    -Type Custom `
    -Subject "CN=TaskMaster Demo" `
    -KeyUsage DigitalSignature `
    -FriendlyName "TaskMaster Demo Certificate" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

$password = ConvertTo-SecureString -String "TaskMaster123!" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "TaskMasterDemo.pfx" -Password $password
Export-Certificate -Cert $cert -FilePath "TaskMasterDemo.cer"
```

#### 4. Criar Pacote MSIX

```powershell
$makeAppx = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\MakeAppx.exe"
& $makeAppx pack /d package /p TaskMaster_1.0.0.0_x64.msix
```

#### 5. Assinar Pacote

```powershell
$signTool = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe"
& $signTool sign /f TaskMasterDemo.pfx /p TaskMaster123! /fd SHA256 TaskMaster_1.0.0.0_x64.msix
```

## Instalação

### Para Desenvolvimento/Demo

1. **Instalar Certificado na Raiz de Confiança**:
   ```powershell
   Import-Certificate -FilePath TaskMasterDemo.cer -CertStoreLocation Cert:\CurrentUser\Root
   ```

2. **Instalar Pacote**:
   ```powershell
   Add-AppxPackage -Path TaskMaster_1.0.0.0_x64.msix
   ```

### Para Produção

Para distribuição pública, você precisará:
1. Certificado assinado por uma Autoridade Certificadora confiável
2. Publicar na Microsoft Store (opcional)
3. Ou distribuir com certificado de código confiável

## Execução

Após instalação:
1. Procure "Task Master" no menu Iniciar
2. Execute a aplicação
3. A janela do Host mostrará o status dos serviços
4. Clique em "Iniciar Todos" para iniciar os serviços
5. O navegador abrirá automaticamente com a interface Blazor

## Desinstalação

```powershell
Get-AppxPackage -Name TaskMasterApp | Remove-AppxPackage
```

## Troubleshooting

### Erro: "The package is not digitally signed"

- Certifique-se de que o certificado está instalado na raiz de confiança
- Ou instale em modo desenvolvedor: `Add-AppxPackage -Path ... -AllowUnsigned`

### Erro: "The package failed to launch"

- Verifique se todos os arquivos necessários estão no pacote
- Verifique os logs do Windows Event Viewer

### Serviços não iniciam

- Verifique se as portas 5000 e 5001 estão disponíveis
- Verifique se o .NET Runtime está instalado (se não usar self-contained)

## Próximos Passos

- [ ] Criar ícones profissionais
- [ ] Configurar atualizações automáticas
- [ ] Publicar na Microsoft Store (opcional)
- [ ] Configurar telemetria (opcional)


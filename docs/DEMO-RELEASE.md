# Task Master - Versão Demo

## Status da Versão Demo

A versão demo do Task Master está pronta para distribuição! Esta versão inclui todas as funcionalidades principais empacotadas como aplicação MSIX.

## O que está incluído

### Funcionalidades Principais
- ✅ Monitorização automática de ficheiros Markdown
- ✅ Extração de tarefas de ficheiros `.md`
- ✅ Interface Blazor completa (Dashboard, Tasks, Projects)
- ✅ Sincronização em tempo real via SignalR
- ✅ API REST completa
- ✅ Worker Service para monitorização contínua
- ✅ Gestão de projetos e tarefas
- ✅ Filtros e busca avançada
- ✅ Métricas e estatísticas

### Empacotamento
- ✅ Aplicação WPF Host para gerenciar serviços
- ✅ Pacote MSIX configurado
- ✅ Certificado self-signed para demo
- ✅ Script de build automatizado
- ✅ Documentação completa

## Como Criar o Pacote Demo

### Pré-requisitos
- Windows 10/11
- .NET 8 SDK
- Windows SDK (para MakeAppx e SignTool)

### Passos

1. **Build do Pacote**:
   ```powershell
   .\scripts\build-msix.ps1
   ```

2. **Instalar Certificado** (apenas primeira vez):
   ```powershell
   Import-Certificate -FilePath dist\msix\TaskMasterDemo.cer -CertStoreLocation Cert:\CurrentUser\Root
   ```

3. **Instalar Aplicação**:
   ```powershell
   Add-AppxPackage -Path dist\msix\TaskMaster_1.0.0.0_x64.msix
   ```

## Como Usar

1. Execute "Task Master" do menu Iniciar
2. Na janela do Host, clique em "Iniciar Todos"
3. Aguarde os serviços iniciarem (indicadores ficam verdes)
4. O navegador abrirá automaticamente com a interface
5. Adicione um projeto e comece a usar!

## Limitações da Versão Demo

- Certificado self-signed (usuários precisam instalar o certificado)
- Sem atualizações automáticas
- Sem telemetria
- Ícones são placeholders (substituir por designs profissionais)

## Próximos Passos para Produção

1. **Certificado de Produção**:
   - Obter certificado de uma Autoridade Certificadora confiável
   - Ou configurar certificado de código da empresa

2. **Ícones Profissionais**:
   - Criar designs profissionais para todos os tamanhos
   - Seguir guidelines da Microsoft Store

3. **Atualizações Automáticas**:
   - Implementar sistema de atualização
   - Configurar servidor de distribuição

4. **Microsoft Store** (opcional):
   - Preparar para publicação na Store
   - Configurar metadados e screenshots

5. **Testes Adicionais**:
   - Testes em diferentes versões do Windows
   - Testes de instalação/desinstalação
   - Testes de migração de dados

## Distribuição

### Para Demo Interna
- Distribuir o arquivo `.msix` junto com o certificado `.cer`
- Instruir usuários a instalar o certificado primeiro

### Para Testes Externos
- Considerar usar certificado temporário de uma CA
- Ou usar modo desenvolvedor do Windows

### Para Produção
- Certificado assinado por CA confiável
- Publicação na Microsoft Store (recomendado)
- Ou distribuição com certificado de código empresarial

## Suporte

Para questões sobre a versão demo:
- Consulte `docs/QUICK-START.md` para uso básico
- Consulte `docs/MSIX-PACKAGING.md` para detalhes técnicos
- Verifique logs do Windows Event Viewer em caso de problemas

## Changelog

### Versão 1.0.0.0 (Demo)
- Versão inicial demo
- Todas as funcionalidades principais implementadas
- Empacotamento MSIX funcional
- Documentação completa


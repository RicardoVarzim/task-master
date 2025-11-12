# Guia de Teste - Task Master

## Funcionalidades Implementadas para Teste

### 1. Edição Inline Completa de Tarefas
### 2. Histórico de Sincronizações

## Pré-requisitos

- .NET 8 SDK instalado
- Base de dados SQLite (será criada automaticamente em `%LOCALAPPDATA%\TaskMasterApp\taskmaster.db`)

## Passos para Testar

### Opção 1: Usando Scripts PowerShell (Recomendado)

**Terminal 1 - Start API:**
```powershell
.\scripts\start-api.ps1
```

**Terminal 2 - Start Blazor:**
```powershell
.\scripts\start-blazor.ps1
```

**Note:** Scripts automatically restore the original directory even if they fail.

### Opção 2: Manual

**Terminal 1 - Iniciar API:**
```powershell
cd src\TaskMaster.API\TaskMaster.API
dotnet run
```

A API estará disponível em:
- HTTP: http://localhost:5000
- Swagger: http://localhost:5000/swagger

**Terminal 2 - Iniciar Blazor:**
```powershell
cd src\TaskMaster.Blazor\TaskMaster.Blazor
dotnet run
```

O Blazor estará disponível em:
- HTTP: http://localhost:5001
- HTTPS: https://localhost:5001

**Nota:** A API roda na porta 5000 e o Blazor na porta 5001 para evitar conflitos.
**Importante:** Inicie a API primeiro e aguarde ela estar rodando antes de iniciar o Blazor.

### 3. Testar Edição Inline de Tarefas

1. Acesse a aplicação Blazor
2. Navegue para a página **Tasks** (`/tasks`)
3. Clique em uma tarefa para abrir o modal de detalhes
4. Clique no botão **Edit**
5. Modifique:
   - **Description**: Altere o texto da descrição
   - **Status**: Selecione um novo status (Completed, InProgress, Planned, Blocked, Pending)
   - **Priority**: Selecione uma nova prioridade (Maximum, High, Medium, Low, Strategic, Maintenance, Administrative)
6. Clique em **Save Changes**
7. Verifique:
   - Mensagem de sucesso aparece
   - Tarefa é atualizada na lista
   - Alterações são refletidas no arquivo Markdown (via API)

### 4. Testar Histórico de Sincronizações

1. Navegue para a página **Projects** (`/projects`)
2. Se não houver projetos, adicione um:
   - Clique em **Add Project**
   - Digite o caminho completo de uma pasta que contenha uma subpasta `.tasks` com arquivos `.md`
   - Clique em **Add**
3. Clique no botão **Sync** em um projeto para sincronizar
4. Clique no botão **History** (ícone de relógio) no mesmo projeto
5. Verifique o modal de histórico:
   - Lista de sincronizações ordenadas por data (mais recente primeiro)
   - Informações mostradas:
     - **Date**: Data e hora da sincronização
     - **Type**: Manual ou Automatic
     - **Status**: Success, Failed ou Partial
     - **Files**: Número de arquivos processados
     - **Tasks**: Estatísticas (Found, Added, Removed)
     - **Duration**: Duração em milissegundos
   - Se houver erro, mensagem de erro é exibida

### 5. Testar Múltiplas Sincronizações

1. Faça várias sincronizações do mesmo projeto
2. Abra o histórico novamente
3. Verifique que todas as sincronizações aparecem ordenadas por data
4. Verifique que cada sincronização mostra estatísticas corretas

### 6. Testar Sincronização com Erro

1. Adicione um projeto com caminho inválido ou sem pasta `.tasks`
2. Tente sincronizar
3. Verifique no histórico que aparece com status **Failed** e mensagem de erro

## Checklist de Testes

### Edição de Tarefas
- [ ] Modal abre corretamente ao clicar em uma tarefa
- [ ] Botão Edit entra em modo de edição
- [ ] Campos são editáveis (Description, Status, Priority)
- [ ] Dropdowns de Status e Priority funcionam
- [ ] Botão Save salva as alterações
- [ ] Mensagem de sucesso aparece após salvar
- [ ] Tarefa é atualizada na lista após salvar
- [ ] Botão Cancel cancela a edição sem salvar
- [ ] Erros são exibidos se houver falha

### Histórico de Sincronizações
- [ ] Botão History aparece em cada projeto
- [ ] Modal de histórico abre corretamente
- [ ] Histórico é carregado do servidor
- [ ] Sincronizações aparecem ordenadas por data (mais recente primeiro)
- [ ] Todas as informações são exibidas corretamente
- [ ] Status é exibido com cores corretas (Success=verde, Failed=vermelho, Partial=amarelo)
- [ ] Estatísticas de tarefas são corretas
- [ ] Duração é calculada corretamente
- [ ] Mensagens de erro aparecem quando aplicável
- [ ] Modal fecha corretamente

## Solução de Problemas

### Processo já está rodando

Se você receber um erro dizendo que o arquivo está sendo usado por outro processo:

**Option 1 - Use stop scripts:**
```powershell
.\scripts\stop-api.ps1      # Stop the API
.\scripts\stop-blazor.ps1   # Stop the Blazor
.\scripts\stop-all.ps1      # Stop all processes
```

**Option 2 - Manual:**
```powershell
# Find and stop processes on port 5000 (API)
Get-NetTCPConnection -LocalPort 5000 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }

# Find and stop processes on port 5001 (Blazor)
Get-NetTCPConnection -LocalPort 5001 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
```

### Outros Problemas

- Se a API e o Blazor tentarem usar a mesma porta, ajuste as configurações em `launchSettings.json`
- A primeira sincronização pode demorar mais se houver muitos arquivos
- Se houver problemas de CORS, verifique se as portas estão corretas no `Program.cs` da API

## Próximos Passos

Após testar estas funcionalidades, podemos implementar:
- Modal de configurações de projeto
- Bulk Actions (ações em massa)
- Melhorias adicionais conforme feedback


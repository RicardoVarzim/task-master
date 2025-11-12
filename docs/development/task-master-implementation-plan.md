<!-- eaebffa6-198c-4f8e-9418-b5085be6fc30 145cd60a-57fb-407e-965f-e7a47ed6fc96 -->
# Plano de Implementação: Task Master

## Visão Geral

Aplicação desktop local-first que monitoriza ficheiros Markdown (`.md`) em pastas de projetos, extrai tarefas (formato `- [ ]` e `- [x]`), e apresenta-as numa interface Blazor Server com sincronização em tempo real. Suporta tags, prioridades, histórico de check-ins, relatórios semanais e métricas.

## Estrutura do Projeto

```
task-master/
├── src/
│   ├── TaskMaster.Core/              # Class Library (.NET 8) - Lógica de negócio
│   │   ├── Models/
│   │   │   ├── Project.cs
│   │   │   ├── Task.cs
│   │   │   ├── TaskTag.cs
│   │   │   ├── TaskPriority.cs (enum: Maximum, High, Medium, Low, Strategic, Maintenance, Administrative)
│   │   │   ├── TaskStatus.cs (enum: Completed, InProgress, Planned, Blocked, Pending)
│   │   │   ├── TaskRole.cs (enum: Requester, Analyst, Developer, Reviewer, Tester, etc.)
│   │   │   ├── TaskAssignment.cs (Task + TeamMember + Role)
│   │   │   ├── Team.cs
│   │   │   ├── TeamMember.cs
│   │   │   ├── GitCommit.cs
│   │   │   ├── TaskChangeHistory.cs (rastreamento via Git)
│   │   │   ├── WeeklyReport.cs
│   │   │   ├── CheckInHistory.cs
│   │   │   └── TaskMetrics.cs
│   │   ├── Data/
│   │   │   └── AppDbContext.cs
│   │   ├── Services/
│   │   │   ├── ITaskParsingService.cs
│   │   │   ├── TaskParsingService.cs (usa Markdig - extrai tags, prioridades, status)
│   │   │   ├── IWeeklyReportService.cs
│   │   │   ├── WeeklyReportService.cs
│   │   │   ├── IHistoryService.cs
│   │   │   └── HistoryService.cs
│   │   └── TaskMaster.Core.csproj
│   │
│   ├── TaskMaster.API/               # ASP.NET Core Web API (.NET 8)
│   │   ├── Controllers/
│   │   │   ├── ProjectsController.cs
│   │   │   ├── TasksController.cs (com filtros: projectId, isCompleted, priority, tags)
│   │   │   ├── SyncController.cs
│   │   │   ├── WeeklyReportsController.cs
│   │   │   ├── HistoryController.cs
│   │   │   └── MetricsController.cs
│   │   ├── Hubs/
│   │   │   └── SyncHub.cs (SignalR)
│   │   ├── Program.cs
│   │   └── TaskMaster.API.csproj
│   │
│   ├── TaskMaster.Worker/            # Worker Service (.NET 8)
│   │   ├── Services/
│   │   │   ├── FileWatcherService.cs
│   │   │   └── SyncService.cs
│   │   ├── Program.cs
│   │   └── TaskMaster.Worker.csproj
│   │
│   └── TaskMaster.Blazor/            # Blazor Server App (.NET 8)
│       ├── Pages/
│       │   ├── Index.razor (Dashboard com filtros avançados)
│       │   ├── Projects.razor
│       │   ├── WeeklyReports.razor
│       │   ├── History.razor
│       │   └── Metrics.razor
│       ├── Components/
│       │   ├── DocumentPanel.razor
│       │   ├── TaskCard.razor
│       │   ├── PriorityBadge.razor
│       │   ├── TagBadge.razor
│       │   ├── QuickReference.razor
│       │   └── TaskFilters.razor
│       ├── Services/
│       │   ├── TaskService.cs (cliente HTTP/SignalR)
│       │   ├── WeeklyReportService.cs
│       │   └── HistoryService.cs
│       ├── Program.cs
│       └── TaskMaster.Blazor.csproj
│
├── tests/
│   └── TaskMaster.Core.Tests/        # Testes unitários
│
├── docs/
│   └── CONTEXT.md                    # Diretrizes de documentação
│
├── TaskMaster.sln                    # Solution file
└── README.md
```

## Funcionalidades Identificadas (Baseadas na Estrutura XDSoba/.tasks)

### Funcionalidades Core (MVP)

1. **Extração de Tarefas**: Parsing de Markdown com suporte a:

   - Task lists (`- [ ]` e `- [x]`)
   - Tags/Hashtags (`#TagName`)
   - Prioridades (🔴 Máxima, 🟠 Alta, 🟡 Média, 🔵 Baixa, 🟣 Estratégico, 📝 Manutenção, 📌 Administrativo)
   - Status (✅ Concluído, 🔄 Em Progresso, 🟠 Em Planeamento, etc.)
   - Subtarefas aninhadas
   - Hierarquia de secções (agrupamento por prioridade)

2. **Dashboard Principal**:

   - Lista consolidada de tarefas
   - Filtros: projeto, estado, prioridade, tags
   - Agrupamento por projeto, prioridade ou tags
   - Vista Quick Reference (tarefas prioritárias)
   - Atualização em tempo real via SignalR

3. **Gestão de Projetos**:

   - Adicionar/remover projetos
   - Monitorização automática da pasta `.tasks`
   - Visualização de estatísticas por projeto

### Funcionalidades de Equipa e Git

4. **Gestão de Equipas e Membros**:

   - Criar/gerir equipas por projeto
   - Adicionar membros da equipa (nome, email, username Git)
   - Mapear utilizadores Git para membros da equipa
   - Perfis de utilizador locais
   - Múltiplas equipas por projeto (suporte multi-equipa)

5. **Atribuição de Responsabilidades**:

   - Atribuir múltiplos roles por tarefa (Requester, Analyst, Developer, Reviewer, Tester, etc.)
   - Parsing de atribuições no Markdown (formato: `@username:role` ou `[role:username]`)
   - Atribuição via interface UI
   - Histórico de atribuições e mudanças de responsabilidade
   - Filtros e visualização por responsável/role

6. **Integração Git**:

   - Deteção automática de repositório Git em cada projeto
   - Análise de commits para identificar autor de alterações em ficheiros `.md`
   - Rastreamento de alterações de tarefas via Git (quem alterou, quando, commit hash)
   - Histórico de commits relacionados com tarefas específicas
   - Visualização de timeline de alterações por tarefa
   - Mapeamento de commits Git para alterações de tarefas
   - Suporte para múltiplos branches (detetar branch atual)

### Funcionalidades Avançadas

7. **Weekly Reports**:

   - Gerar relatórios semanais baseados em templates
   - Estatísticas: tarefas concluídas, bugs resolvidos, code reviews
   - Progresso por área/tag/responsável
   - Bloqueios e conquistas
   - Objetivos para próxima semana
   - Exportação para Markdown

8. **History Tracking**:

   - Histórico de check-ins diários
   - Métricas mensais (tarefas concluídas, bugs resolvidos, etc.)
   - Marcos importantes
   - Notas históricas
   - Visualização cronológica
   - Histórico baseado em Git (quem fez o quê e quando)

9. **Métricas e Estatísticas**:

   - Dashboard de métricas
   - Tarefas concluídas por período/responsável
   - Distribuição por prioridade/role
   - Progresso por tag/projeto/equipa
   - Gráficos e visualizações
   - Métricas de contribuição por membro da equipa

10. **Templates**:

    - Suporte para templates de check-in diário
    - Templates de relatório semanal
    - Criação de novos templates personalizados

## Fases de Implementação

### Fase 1: Estrutura Base e Core Logic Expandido

1. Criar solution e projetos base (.NET 8)
2. Implementar entidades expandidas:

   - `Project` e `Task` (base)
   - `TaskTag` (relação many-to-many com Task)
   - Enums: `TaskPriority`, `TaskStatus`
   - `WeeklyReport`, `CheckInHistory`, `TaskMetrics`

3. Configurar Entity Framework Core com SQLite
4. Criar `AppDbContext` e migrations
5. Implementar `TaskParsingService` expandido:

   - Extrair tags (`#TagName`)
   - Detetar prioridades (emojis ou texto)
   - Detetar status (emojis ou texto)
   - Suportar subtarefas aninhadas
   - Preservar hierarquia de secções

6. Configurar localização da base de dados em `%LOCALAPPDATA%\TaskMasterApp\`

### Fase 2: API Layer Expandido

1. Configurar ASP.NET Core Web API
2. Implementar controllers RESTful base:

   - `GET/POST/DELETE /api/projects`
   - `GET /api/tasks` (filtros: `projectId`, `isCompleted`, `priority`, `tags`, `status`)
   - `GET/PUT /api/tasks/{id}/document`
   - `POST /api/sync`

3. Implementar controllers avançados:

   - `GET/POST /api/weekly-reports`
   - `GET/POST /api/history`
   - `GET /api/metrics` (com filtros de período)

4. Configurar SignalR Hub (`SyncHub`) com método `TasksUpdated()`
5. Endpoint interno `POST /api/internal/notify-update` para o Worker Service

### Fase 3: Worker Service

1. Criar Worker Service project
2. Implementar `FileWatcherService`:

   - Carregar projetos da base de dados no arranque
   - Criar `FileSystemWatcher` para cada projeto (monitorizar `.tasks\*.md` recursivamente)
   - Implementar debounce para eventos `Created`, `Changed`, `Deleted`, `Renamed`

3. Implementar `SyncService`:

   - Analisar ficheiros modificados
   - Sincronizar tarefas, tags, prioridades na base de dados
   - Notificar API via HTTP após atualização

4. Configurar como Windows Service (para MVP)

### Fase 4: Frontend Blazor Server - Dashboard e Gestão

1. Criar Blazor Server project
2. Configurar SignalR client connection ao `SyncHub`
3. Implementar Dashboard (`Index.razor`):

   - Lista consolidada de tarefas com filtros avançados
   - Agrupamento por projeto, prioridade ou tags
   - Componente `QuickReference` para tarefas prioritárias
   - Componentes `PriorityBadge` e `TagBadge` para visualização
   - Atualização em tempo real via SignalR

4. Implementar gestão de projetos (`Projects.razor`):

   - Lista de projetos com estatísticas
   - Adicionar projeto (seletor de pastas)
   - Remover projeto

5. Implementar `DocumentPanel` component:

   - Modo visualização (renderizar Markdown como HTML)
   - Modo edição (textarea com conteúdo Markdown)
   - Botão "Abrir no Editor Padrão"
   - Botão "Guardar" (PUT para API)

### Fase 5: Frontend Blazor Server - Funcionalidades Avançadas

1. Implementar página Weekly Reports (`WeeklyReports.razor`):

   - Lista de relatórios semanais
   - Gerar novo relatório a partir de template
   - Visualizar/editar relatório existente
   - Estatísticas e métricas do período
   - Exportação para Markdown

2. Implementar página History (`History.razor`):

   - Histórico cronológico de check-ins
   - Filtros por período
   - Métricas mensais
   - Marcos importantes

3. Implementar página Metrics (`Metrics.razor`):

   - Dashboard de métricas
   - Gráficos de progresso
   - Distribuição por prioridade/tag
   - Estatísticas por projeto

### Fase 6: Integração e Testes

1. Integrar todos os componentes
2. Testar fluxo completo: adicionar projeto → criar ficheiro `.md` → ver tarefa aparecer
3. Testar parsing avançado: tags, prioridades, status
4. Testar edição de tarefa via UI → verificar sincronização
5. Testar atualizações em tempo real via SignalR
6. Testar geração de relatórios semanais
7. Testar histórico e métricas
8. Testes unitários para parsing de Markdown com casos complexos

### Fase 7: Empacotamento (MVP Windows)

1. Configurar MSIX packaging
2. Incluir Worker Service como Windows Service
3. Configurar instalação/desinstalação
4. Criar atalho no Menu Iniciar

## Dependências Principais

- **TaskMaster.Core**: Entity Framework Core, SQLite, Markdig
- **TaskMaster.API**: ASP.NET Core, SignalR
- **TaskMaster.Worker**: .NET Worker Service, Entity Framework Core
- **TaskMaster.Blazor**: Blazor Server, SignalR Client, Chart.js ou similar para gráficos

## Decisões Técnicas

1. **Base de dados**: SQLite em `%LOCALAPPDATA%\TaskMasterApp\taskmaster.db`
2. **Monitorização**: `FileSystemWatcher` na subpasta `.tasks` de cada projeto (recursivo)
3. **Parsing**: Markdig para extrair task lists, tags (regex `#\w+`), prioridades (emojis ou texto), status
4. **Debounce**: 500ms para eventos de ficheiros
5. **Comunicação Worker→API**: HTTP POST para endpoint interno
6. **Comunicação API→Frontend**: SignalR Hub push notifications
7. **Templates**: Armazenados como ficheiros Markdown na pasta do projeto ou em `%LOCALAPPDATA%\TaskMasterApp\Templates\`
8. **Prioridades**: Mapeamento de emojis para enum (🔴→Maximum, 🟠→High, etc.)

## Assumptions

- Cada projeto monitorizado deve ter uma pasta `.tasks` na raiz
- Ficheiros `.md` são procurados recursivamente dentro de `.tasks`
- Formato de tarefas: `- [ ]` e `- [x]` (hífen, não asterisco)
- Tags seguem formato `#TagName` (hashtag seguido de alfanuméricos)
- Prioridades podem ser indicadas por emojis ou texto (ex: "Prioridade Máxima")
- Worker Service e API podem partilhar a mesma base de dados SQLite
- Templates de relatórios seguem estrutura similar ao `WEEKLY_REPORT_TEMPLATE.md`
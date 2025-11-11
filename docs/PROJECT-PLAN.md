# Task Master - Plano de Projeto

## Visão Geral

Task Master é uma aplicação desktop local-first que monitoriza ficheiros Markdown em projetos, extrai tarefas e apresenta-as numa interface Blazor Server com sincronização em tempo real.

**Data de Início**: Novembro 2024  
**Status Atual**: Em Desenvolvimento - Fase 3 e 4 (Frontend e API) - Progresso Significativo

---

## Status do Projeto

### ✅ Fase 1: Core Infrastructure (COMPLETO)

#### 1.1 Estrutura Base
- [x] Solution e projetos (.NET 8) criados
- [x] Estrutura de pastas organizada
- [x] Configuração de dependências

#### 1.2 Core Library (TaskMaster.Core)
- [x] **Modelos de Dados**:
  - [x] `Project` - Representa projetos monitorizados
  - [x] `Task` - Representa tarefas extraídas de Markdown
  - [x] `Team` e `TeamMember` - Gestão de equipas
  - [x] `TaskAssignment` - Atribuições de tarefas
  - [x] `TaskTag` - Tags para categorização
  - [x] `TaskChangeHistory` - Histórico de alterações
  - [x] `GitCommit` - Integração com Git
  - [x] `WeeklyReport` - Relatórios semanais
  - [x] `TaskMetrics` - Métricas de tarefas
  - [x] `CheckInHistory` - Histórico de check-ins

- [x] **Enums**:
  - [x] `TaskPriority` - Prioridades (Maximum, High, Medium, Low, Strategic, Maintenance, Administrative)
  - [x] `TaskStatus` - Estados (Completed, InProgress, Planned, Blocked, Pending)
  - [x] `TaskRole` - Funções (Requester, Analyst, Developer, Reviewer, Tester, Manager, Other)

- [x] **Data Layer**:
  - [x] `AppDbContext` - Contexto do Entity Framework
  - [x] `AppDbContextFactory` - Factory para migrations
  - [x] `DatabaseHelper` - Utilitários de base de dados
  - [x] Migration inicial criada e aplicada
  - [x] SQLite configurado como base de dados

- [x] **Serviços Core**:
  - [x] `TaskParsingService` - Parsing de Markdown e extração de tarefas
  - [x] `TaskUpdateService` - Atualização de tarefas em arquivos Markdown
  - [x] `WeeklyReportService` - Geração de relatórios semanais
  - [x] `HistoryService` - Gestão de histórico de alterações
  - [x] Interfaces correspondentes (`ITaskParsingService`, `ITaskUpdateService`, `IWeeklyReportService`, `IHistoryService`)

#### 1.3 API Layer (TaskMaster.API)
- [x] **Configuração Base**:
  - [x] Program.cs configurado com serviços
  - [x] Entity Framework Core configurado
  - [x] CORS configurado para Blazor
  - [x] Swagger/OpenAPI configurado

- [x] **Controllers**:
  - [x] `ProjectsController` - CRUD de projetos
  - [x] `TasksController` - CRUD de tarefas
  - [x] `SyncController` - Sincronização de projetos

- [x] **SignalR**:
  - [x] `SyncHub` - Hub para sincronização em tempo real
  - [x] Notificações de atualização de tarefas

#### 1.4 Worker Service (TaskMaster.Worker) - Implementado com Melhorias
- [x] Estrutura base do Worker Service
- [x] `FileWatcherService` - Serviço de monitorização de ficheiros
- [x] Integração básica com API para sincronização
- [x] Tratamento robusto de erros
- [x] Retry logic com exponential backoff
- [x] Configuração via appsettings.json
- [x] Validação de projetos e caminhos

#### 1.5 Blazor Frontend (TaskMaster.Blazor) - Implementado
- [x] Estrutura base do Blazor Server
- [x] Página `Projects.razor` - Listagem e gestão de projetos com estatísticas
- [x] Página `Tasks.razor` - Interface completa de tarefas
- [x] Página `Dashboard.razor` - Dashboard com métricas e visualizações
- [x] Layout base e navegação
- [x] Componentes reutilizáveis (StatusBadge, PriorityBadge, TagBadge, TaskCard, TaskFilter, StatCard)

---

### ✅ Fase 2: Worker Service Completo (COMPLETO)

#### 2.1 Melhorias no FileWatcherService
- [x] Tratamento robusto de erros
- [x] Retry logic para falhas de sincronização (com exponential backoff)
- [x] Configuração de debounce ajustável
- [x] Suporte para múltiplos padrões de ficheiros
- [x] Logging detalhado e estruturado
- [x] Limitação de sincronizações concorrentes
- [x] Validação de entrada e recursos

#### 2.2 Integração Git
- [ ] Detecção automática de repositórios Git
- [ ] Rastreamento de commits relacionados a tarefas
- [ ] Associação de commits com alterações de tarefas
- [ ] Histórico de alterações baseado em Git

#### 2.3 Gestão de Projetos no Worker
- [x] Adição/remoção dinâmica de projetos
- [x] Validação de caminhos de projetos
- [ ] Detecção de projetos Git (pendente - Fase 2.2)
- [x] Sincronização inicial ao iniciar

#### 2.4 Configuração
- [x] Configuração via appsettings.json (`WorkerServiceOptions`)
- [x] Configuração de intervalos de verificação
- [x] Configuração de endpoints da API
- [x] Configuração de pastas de tarefas (.tasks)
- [x] Configuração de retry e debounce

---

### ✅ Fase 3: Blazor Frontend Completo (COMPLETO - Parcialmente)

#### 3.1 Páginas Principais
- [x] **Dashboard** (`Dashboard.razor`):
  - [x] Visão geral de tarefas por projeto
  - [x] Estatísticas (total, completas, pendentes, in progress, blocked)
  - [x] Gráficos de atividade (últimos 14 dias)
  - [x] Tarefas recentes
  - [x] Métricas por projeto
  - [x] Tarefas por prioridade

- [x] **Tasks** (`Tasks.razor`):
  - [x] Listagem de tarefas com filtros
  - [x] Filtros por projeto, status, prioridade, completion
  - [x] Busca de tarefas (descrição, projeto, tags)
  - [x] Ordenação (data, prioridade, status, projeto)
  - [x] Visualização de detalhes de tarefa (modal)
  - [x] Atualização de completion status via API
  - [ ] Edição inline completa (descrição, status, prioridade) - Pendente

- [x] **Projects** (`Projects.razor`) - Melhorias:
  - [x] Visualização de estatísticas por projeto
  - [x] Barras de progresso de conclusão
  - [x] Sincronização manual por projeto
  - [x] Botão "Sync All"
  - [x] Link para visualizar tarefas do projeto
  - [ ] Gestão de configurações de projeto - Pendente
  - [ ] Histórico de sincronizações - Pendente

#### 3.2 Componentes Reutilizáveis
- [x] `TaskCard.razor` - Card de tarefa com checkbox e badges
- [ ] `TaskList.razor` - Lista de tarefas (não necessário - TaskCard usado diretamente)
- [x] `TaskFilter.razor` - Filtros de tarefas completos
- [ ] `ProjectCard.razor` - Card de projeto (pendente)
- [x] `StatusBadge.razor` - Badge de status
- [x] `PriorityBadge.razor` - Badge de prioridade
- [x] `TagBadge.razor` - Badge de tag
- [x] `StatCard.razor` - Card de estatísticas para dashboard

#### 3.3 Funcionalidades Avançadas
- [ ] **Drag and Drop**: Reordenar tarefas
- [ ] **Bulk Actions**: Ações em massa (marcar como completa, alterar prioridade)
- [ ] **Keyboard Shortcuts**: Atalhos de teclado
- [ ] **Dark Mode**: Tema escuro
- [ ] **Responsive Design**: Design responsivo para diferentes tamanhos de ecrã

#### 3.4 Sincronização em Tempo Real
- [x] Conexão SignalR funcional
- [x] Atualização automática quando tarefas mudam
- [x] Integração com HubConnection
- [ ] Indicadores de sincronização visuais (pendente)
- [ ] Notificações de atualizações (pendente)

---

### ✅ Fase 4: API Completa (COMPLETO - Parcialmente)

#### 4.1 Controllers Adicionais
- [x] **WeeklyReportsController**:
  - [x] GET `/api/weekly-reports` - Listar relatórios (com paginação)
  - [x] GET `/api/weekly-reports/{id}` - Obter relatório específico
  - [x] POST `/api/weekly-reports/generate` - Gerar relatório
  - [x] DELETE `/api/weekly-reports/{id}` - Eliminar relatório

- [x] **HistoryController**:
  - [x] GET `/api/history/task/{taskId}` - Histórico de uma tarefa
  - [x] GET `/api/history/project/{projectId}` - Histórico de um projeto
  - [x] POST `/api/history/checkin` - Criar check-in
  - [x] POST `/api/history/task/{taskId}/change` - Registrar alteração manual

- [x] **MetricsController**:
  - [x] GET `/api/metrics/project/{projectId}` - Métricas de projeto
  - [x] GET `/api/metrics/overview` - Métricas gerais
  - [x] GET `/api/metrics/team/{teamId}` - Métricas de equipa

- [x] **TeamsController**:
  - [x] CRUD completo de equipas
  - [x] Gestão de membros de equipa
  - [x] Atribuições de tarefas

#### 4.2 Melhorias nos Controllers Existentes
- [x] **ProjectsController**:
  - [x] Validação de caminhos (Data Annotations)
  - [ ] Detecção automática de Git (pendente - Fase 2.2)
  - [x] Endpoint para sincronização manual (via SyncController)

- [x] **TasksController**:
  - [x] Filtros avançados (query parameters: projectId, isCompleted, priority, status, tags)
  - [x] Paginação (com `PagedResult<T>` e extensões `ToPagedResult`)
  - [x] Ordenação (sortBy, sortOrder)
  - [x] Atualização parcial (PATCH `/api/tasks/{id}`)
  - [x] Integração com SignalR para notificações
  - [x] Registro de histórico de alterações

- [ ] **SyncController**:
  - [ ] Sincronização incremental (pendente)
  - [ ] Relatório de sincronização (pendente)
  - [ ] Tratamento de conflitos (pendente)

#### 4.3 Validação e Tratamento de Erros
- [x] Validação de modelos com Data Annotations
- [x] Tratamento centralizado de erros (`GlobalExceptionHandlerMiddleware`)
- [x] Respostas de erro padronizadas
- [x] Logging de erros
- [x] Respostas customizadas para validação de modelos (InvalidModelStateResponseFactory)

---

### 📋 Fase 5: Funcionalidades Avançadas (PENDENTE)

#### 5.1 Gestão de Equipas
- [ ] Interface para criar/editar equipas
- [ ] Gestão de membros de equipa
- [ ] Atribuição de tarefas a membros
- [ ] Visualização de tarefas por equipa

#### 5.2 Relatórios e Analytics
- [ ] Relatórios semanais automáticos
- [ ] Gráficos de progresso
- [ ] Métricas de produtividade
- [ ] Exportação de relatórios (PDF, CSV)

#### 5.3 Integração Git Avançada
- [ ] Visualização de commits relacionados
- [ ] Associação automática de commits com tarefas
- [ ] Histórico de alterações baseado em Git
- [ ] Diferenças entre versões

#### 5.4 Notificações
- [ ] Notificações de tarefas atribuídas
- [ ] Notificações de tarefas completas
- [ ] Notificações de prazos
- [ ] Configuração de preferências de notificação

---

### 🚧 Fase 6: Testes (EM PROGRESSO)

#### 6.1 Testes Unitários
- [ ] Testes para `TaskParsingService`
- [ ] Testes para `TaskUpdateService`
- [ ] Testes para `WeeklyReportService`
- [ ] Testes para `HistoryService`
- [x] Testes para modelos e validações
- [x] Testes para `TasksController` (GetTasks, GetTask com filtros, paginação, ordenação)
- [x] Testes para `ProjectsController` (GetProjects, GetProject, CreateProject, DeleteProject)

#### 6.2 Testes de Integração
- [ ] Testes de API endpoints completos
- [ ] Testes de sincronização
- [ ] Testes de SignalR
- [ ] Testes de base de dados

#### 6.3 Testes End-to-End
- [ ] Testes de fluxo completo
- [ ] Testes de interface Blazor
- [ ] Testes de Worker Service

---

### ✅ Fase 7: Empacotamento e Distribuição (COMPLETO - Demo)

#### 7.1 MSIX Package
- [x] Configuração de empacotamento MSIX
- [x] Manifesto de aplicação (`Package.appxmanifest`)
- [x] Estrutura de recursos (Assets/)
- [x] Certificado de assinatura (self-signed para demo)
- [x] Script de build automatizado (`scripts/build-msix.ps1`)

#### 7.2 Instalação e Atualização
- [x] Instalador MSIX funcional
- [ ] Atualizações automáticas (pendente para produção)
- [ ] Migração de dados entre versões (pendente)

#### 7.3 Documentação de Utilizador
- [x] Guia de início rápido (`docs/QUICK-START.md`)
- [x] Guia de empacotamento MSIX (`docs/MSIX-PACKAGING.md`)
- [x] Documentação de ícones (`src/TaskMaster.Host/Assets/README-ICONS.md`)
- [ ] Manual completo de utilizador (pendente)
- [ ] FAQ (pendente)
- [ ] Vídeos tutoriais (opcional)

---

## Prioridades de Implementação

### Prioridade Alta (Concluído ✅)
1. ✅ Completar Worker Service (Fase 2) - **CONCLUÍDO**
2. ✅ Interface básica de tarefas no Blazor (Fase 3.1) - **CONCLUÍDO**
3. ✅ Controllers adicionais da API (Fase 4.1) - **CONCLUÍDO**
4. ✅ Melhorias nos controllers existentes (Fase 4.2) - **CONCLUÍDO**
5. ✅ Tratamento centralizado de erros (Fase 4.3) - **CONCLUÍDO**
6. ✅ Dashboard completo (Fase 3.1) - **CONCLUÍDO**
7. ✅ Endpoint PATCH para tarefas (Fase 4.2) - **CONCLUÍDO**

### Prioridade Média (Próximas 2-4 semanas)
1. **Edição inline completa de tarefas** - Adicionar edição de descrição, status e prioridade no modal
2. **Melhorias na página Projects** - Histórico de sincronizações e configurações
3. **Bulk Actions** - Ações em massa para tarefas
4. **Componentes adicionais** - TaskList, ProjectCard
5. **Testes adicionais** - TaskUpdateService, TaskParsingService, testes de integração

### Prioridade Baixa (Futuro)
1. Integração Git avançada (Fase 2.2, 5.3)
2. Funcionalidades avançadas (Drag and Drop, Keyboard Shortcuts, Dark Mode)
3. Notificações (Fase 5.4)
4. Empacotamento MSIX (Fase 7)

---

## Decisões Técnicas Importantes

### Base de Dados
- **SQLite**: Escolhido para ser local-first e não requerer servidor separado
- **Localização**: `%LOCALAPPDATA%\TaskMasterApp\taskmaster.db`

### Arquitetura
- **Local-First**: Dados armazenados localmente, sem dependência de servidor remoto
- **Blazor Server**: Escolhido para sincronização em tempo real sem necessidade de WebAssembly
- **Worker Service**: Executa em background para monitorização contínua

### Parsing de Markdown
- **Markdig**: Biblioteca utilizada para parsing robusto
- **Suporte**: Formato padrão `- [ ]` e `- [x]`
- **Extensões**: Suporte para tags (`#tag`), assignments (`@user:role`), prioridades e status

---

## Riscos e Mitigações

### Riscos Identificados
1. **Performance com muitos projetos**: Muitos FileSystemWatchers podem impactar performance
   - **Mitigação**: Implementar debounce e limitar número de watchers ativos

2. **Conflitos de sincronização**: Alterações simultâneas podem causar conflitos
   - **Mitigação**: Implementar estratégia de resolução de conflitos (última escrita vence)

3. **Base de dados corrompida**: SQLite pode corromper em caso de crash
   - **Mitigação**: Implementar backups automáticos e validação de integridade

---

## Métricas de Sucesso

- [ ] Worker Service monitoriza projetos sem erros
- [ ] Interface Blazor responsiva (< 200ms para operações básicas)
- [ ] Sincronização em tempo real funcional (< 1s de latência)
- [ ] Parsing de tarefas com 100% de precisão
- [ ] Zero perda de dados em sincronizações

---

## Próximos Passos Imediatos

1. **Edição Inline Completa de Tarefas**:
   - Adicionar campos editáveis no modal de detalhes
   - Permitir editar descrição, status e prioridade
   - Integrar com endpoint PATCH existente
   - Atualizar arquivo Markdown automaticamente

2. **Melhorias na Página Projects**:
   - Adicionar histórico de sincronizações
   - Criar interface para configurações de projeto
   - Melhorar visualização de estatísticas

3. **Bulk Actions**:
   - Implementar seleção múltipla de tarefas
   - Adicionar ações em massa (completar, alterar prioridade/status)
   - Criar endpoint na API para bulk updates

4. **Testes Adicionais**:
   - Testes para TaskUpdateService
   - Testes para TaskParsingService
   - Testes de integração para endpoints PATCH
   - Testes de SignalR

5. **Integração Git** (Fase 2.2):
   - Detecção automática de repositórios Git
   - Rastreamento de commits relacionados a tarefas

---

## Notas de Desenvolvimento

### Convenções
- Usar async/await para todas operações I/O
- Incluir XML comments em métodos públicos
- Seguir convenções de naming do .NET
- Manter código testável (dependency injection)

### Ferramentas
- Visual Studio 2022 ou VS Code
- .NET 8 SDK
- SQLite Browser (opcional, para debug)

---

**Última Atualização**: Dezembro 2024  
**Próxima Revisão**: Após implementação de edição inline completa

## Resumo do Progresso

### ✅ Concluído
- **Fase 1**: Core Infrastructure (100%)
- **Fase 2**: Worker Service Completo (100%)
- **Fase 3**: Blazor Frontend (85% - falta edição inline completa e algumas melhorias)
- **Fase 4**: API Completa (90% - falta sincronização incremental e relatórios)
- **Fase 6**: Testes (20% - testes básicos de controllers implementados)
- **Fase 7**: Empacotamento MSIX (80% - demo funcional, falta atualizações automáticas)

### 🚧 Em Progresso
- Edição inline completa de tarefas
- Melhorias na página Projects
- Testes adicionais

### 📋 Pendente
- Integração Git avançada
- Funcionalidades avançadas (Drag and Drop, Dark Mode, etc.)
- Bulk Actions
- Empacotamento MSIX


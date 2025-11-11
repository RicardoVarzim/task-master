# Task Master

Aplicação desktop local-first que monitoriza ficheiros Markdown (`.md`) em pastas de projetos, extrai tarefas (formato `- [ ]` e `- [x]`), e apresenta-as numa interface Blazor Server com sincronização em tempo real.

## Status do Projeto

### ✅ Implementado

- **Estrutura Base**: Solution e projetos (.NET 8)
- **Core Library**: 
  - Entidades (Project, Task, Team, TeamMember, TaskAssignment, etc.)
  - Enums (TaskPriority, TaskStatus, TaskRole)
  - AppDbContext com Entity Framework Core e SQLite
  - Serviços (TaskParsingService, WeeklyReportService, HistoryService)
  - Migration inicial criada
- **API Layer**:
  - Controllers (Projects, Tasks, Sync)
  - SignalR Hub configurado
  - Configuração de CORS para Blazor
  - Swagger/OpenAPI configurado

### 🚧 Em Progresso / Pendente

- **Worker Service**: Monitorização de ficheiros implementada, melhorias pendentes
- **Frontend Blazor**: Página de projetos implementada, interface completa de tarefas pendente
- **Controllers Adicionais**: WeeklyReports, History, Metrics pendentes
- **Integração Git**: Rastreamento de alterações via Git pendente
- **Gestão de Equipas**: Interface para gerir equipas e membros pendente

## Estrutura do Projeto

```
task-master/
├── src/
│   ├── TaskMaster.Core/          # Lógica de negócio
│   ├── TaskMaster.API/           # Web API
│   ├── TaskMaster.Worker/        # Worker Service (pendente)
│   └── TaskMaster.Blazor/        # Frontend Blazor (pendente)
├── tests/
│   └── TaskMaster.Core.Tests/   # Testes unitários
└── docs/
    ├── CONTEXT.md               # Diretrizes de documentação
    └── PROJECT-PLAN.md          # Plano detalhado do projeto
```

## Como Executar

### Pré-requisitos

- .NET 8 SDK
- Visual Studio 2022 ou VS Code

### Executar a API

```bash
cd src/TaskMaster.API/TaskMaster.API
dotnet run
```

A API estará disponível em `https://localhost:7000` ou `http://localhost:5000`

### Swagger UI

Aceda a `https://localhost:7000/swagger` para ver a documentação da API.

## Base de Dados

A base de dados SQLite é criada automaticamente em:
`%LOCALAPPDATA%\TaskMasterApp\taskmaster.db`

## Documentação

- **[CONTEXT.md](docs/CONTEXT.md)**: Diretrizes de documentação e contexto do projeto
- **[PROJECT-PLAN.md](docs/PROJECT-PLAN.md)**: Plano detalhado de implementação com fases e tarefas

## Próximos Passos

Consulte o [PROJECT-PLAN.md](docs/PROJECT-PLAN.md) para o plano completo. Prioridades imediatas:

1. Completar melhorias no Worker Service (tratamento de erros, configuração)
2. Criar interface completa de tarefas no Blazor
3. Implementar controllers adicionais (WeeklyReports, History, Metrics)
4. Adicionar testes unitários
5. Configurar empacotamento MSIX

## Licença

[Definir licença]


# Task Master - Contexto e Diretrizes de Documentação

## Visão Geral do Projeto

Task Master é uma aplicação desktop local-first que monitoriza ficheiros Markdown (`.md`) em pastas de projetos, extrai tarefas (formato `- [ ]` e `- [x]`), e apresenta-as numa interface Blazor Server com sincronização em tempo real.

## Estrutura de Organização da Documentação

### Pasta `.docs`

A pasta `docs/` contém toda a documentação do projeto, organizada da seguinte forma:

```
docs/
├── CONTEXT.md              # Este ficheiro - diretrizes de documentação
├── PROJECT-PLAN.md         # Plano detalhado do projeto
├── ARCHITECTURE.md         # Documentação de arquitetura (quando criada)
├── API.md                  # Documentação da API (quando criada)
└── [outros documentos]     # Outros documentos conforme necessário
```

### Regras de Documentação

1. **Prioridade do README.md**: O ficheiro `README.md` na raiz do projeto deve sempre conter:
   - Visão geral do projeto
   - Status atual (implementado vs pendente)
   - Instruções básicas de execução
   - Estrutura do projeto
   - Próximos passos

2. **Documentação por Tarefa**: Quando trabalhar em uma tarefa específica:
   - Criar documentação na pasta `docs/` se necessário
   - Atualizar o `README.md` com o status atualizado
   - Documentar decisões arquiteturais importantes

3. **Checklists e Procedimentos**:
   - Manter o `PROJECT-PLAN.md` atualizado com o progresso
   - Documentar problemas encontrados e soluções
   - Manter histórico de mudanças importantes

## Diretrizes de Manutenção

- **Atualização Contínua**: A documentação deve ser atualizada sempre que houver mudanças significativas
- **Clareza**: Usar linguagem clara e exemplos práticos
- **Organização**: Manter a estrutura de pastas consistente
- **Referências**: Incluir referências cruzadas entre documentos quando relevante

## Tecnologias Principais

- **.NET 8**: Framework principal
- **Blazor Server**: Interface de utilizador
- **Entity Framework Core**: ORM para acesso a dados
- **SQLite**: Base de dados local
- **SignalR**: Sincronização em tempo real
- **Markdig**: Parsing de Markdown

## Estrutura do Projeto

```
task-master/
├── src/
│   ├── TaskMaster.Core/          # Lógica de negócio e modelos
│   ├── TaskMaster.API/           # Web API REST
│   ├── TaskMaster.Worker/        # Worker Service para monitorização
│   └── TaskMaster.Blazor/        # Frontend Blazor Server
├── tests/
│   └── TaskMaster.Core.Tests/    # Testes unitários
└── docs/
    └── CONTEXT.md                # Este ficheiro
```

## Convenções de Código

- **C#**: Seguir convenções padrão do .NET
- **Naming**: Usar PascalCase para classes, camelCase para variáveis locais
- **Documentação**: Incluir XML comments em métodos públicos
- **Async/Await**: Usar async/await para operações I/O

## Próximos Passos

Consulte `PROJECT-PLAN.md` para o plano detalhado de implementação.


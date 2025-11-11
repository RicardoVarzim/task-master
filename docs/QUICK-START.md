# Task Master - Guia de Início Rápido

## Instalação Rápida (Demo)

### Pré-requisitos

- Windows 10/11
- .NET 8 Runtime (se não usar versão self-contained)

### Passos

1. **Criar Pacote MSIX**:
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

4. **Executar**:
   - Procure "Task Master" no menu Iniciar
   - Clique para abrir
   - Clique em "Iniciar Todos" na janela do Host
   - O navegador abrirá automaticamente

## Uso Básico

### 1. Adicionar Projeto

1. Na interface Blazor, vá para "Projects"
2. Clique em "Add Project"
3. Selecione a pasta do seu projeto
4. O sistema procurará automaticamente por ficheiros `.md` na pasta `.tasks`

### 2. Criar Tarefas

Crie ficheiros Markdown na pasta `.tasks` do seu projeto:

```markdown
# Minhas Tarefas

- [ ] Implementar nova funcionalidade
- [x] Revisar código
- [ ] Escrever testes
```

### 3. Visualizar Tarefas

- **Dashboard**: Visão geral de todas as tarefas
- **Tasks**: Lista completa com filtros
- **Projects**: Gestão de projetos

### 4. Atualizar Tarefas

- Marque tarefas como completas usando o checkbox
- As alterações são sincronizadas automaticamente com o ficheiro Markdown

## Estrutura de Pastas

```
meu-projeto/
├── .tasks/              # Pasta de tarefas (opcional)
│   └── tasks.md
├── src/
└── ...
```

## Formato de Tarefas

```markdown
- [ ] Descrição da tarefa
- [x] Tarefa completa
- [ ] Tarefa com prioridade 🔴
- [ ] Tarefa com tag #importante
- [ ] Tarefa atribuída @joao:developer
```

## Recursos

- **Filtros**: Por projeto, status, prioridade, tags
- **Busca**: Procure por descrição, projeto ou tags
- **Ordenação**: Por data, prioridade, status
- **Sincronização**: Automática via Worker Service

## Suporte

Para problemas ou questões:
- Consulte a documentação completa em `docs/`
- Verifique os logs do Windows Event Viewer
- Verifique se os serviços estão rodando na janela do Host


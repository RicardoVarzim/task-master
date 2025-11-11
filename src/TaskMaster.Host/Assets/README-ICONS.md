# Ícones e Recursos Visuais

Para criar os ícones necessários para o pacote MSIX, você precisa dos seguintes arquivos:

## Arquivos Necessários

1. **icon.ico** - Ícone principal da aplicação (256x256 ou múltiplos tamanhos)
2. **Square150x150Logo.png** - Logo quadrado 150x150px
3. **Square44x44Logo.png** - Logo quadrado 44x44px (tile pequeno)
4. **Wide310x150Logo.png** - Logo largo 310x150px (tile largo)
5. **SplashScreen.png** - Tela de splash 620x300px
6. **StoreLogo.png** - Logo da loja 50x50px

## Como Criar os Ícones

### Opção 1: Usar Ferramentas Online
- Use https://www.icoconverter.com/ para criar .ico a partir de PNG
- Use https://www.favicon-generator.org/ para gerar múltiplos tamanhos

### Opção 2: Usar GIMP/Photoshop
1. Crie um design base (sugestão: ícone de lista/tarefas)
2. Exporte em diferentes tamanhos:
   - 256x256 para icon.ico
   - 150x150 para Square150x150Logo.png
   - 44x44 para Square44x44Logo.png
   - 310x150 para Wide310x150Logo.png
   - 620x300 para SplashScreen.png
   - 50x50 para StoreLogo.png

### Opção 3: Placeholders Temporários
Para demo rápida, você pode usar imagens simples coloridas como placeholder.

## Sugestão de Design

- **Cor principal**: Azul (#0078D4) ou Verde (#107C10)
- **Ícone**: Lista de tarefas com checkmark ou clipboard
- **Estilo**: Flat design, moderno

## Exemplo de Criação Rápida (PowerShell)

```powershell
# Criar imagens placeholder usando .NET (requer System.Drawing)
# Ou use uma ferramenta online para converter um logo simples
```

Para uma demo rápida, você pode usar qualquer imagem PNG nos tamanhos corretos e depois substituir por designs profissionais.


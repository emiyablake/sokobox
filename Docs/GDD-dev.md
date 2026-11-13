# GDD Técnico — Sokoban (visão de desenvolvedor)

> Documento interno. Descreve a arquitetura, as mecânicas implementadas, o formato dos dados e os pontos de atenção para manutenção e migração (JavaScript → Unity).

---

## 1. Visão geral

| Item | Descrição |
|---|---|
| **Gênero** | Puzzle (Sokoban) |
| **Plataforma-alvo** | Web (HTML5), publicação via itch.io |
| **Stack** | HTML5 Canvas + CSS3 + JavaScript ES6+ (vanilla, sem frameworks) |
| **Estado** | Protótipo jogável em desenvolvimento ativo |
| **Origem** | Projeto de estudo: ciclo de jogo, estado, colisão em grid, carregamento de assets |

Sem engine, sem build step: o jogo roda servindo a pasta estática. Não há `package.json`, bundler, nem transpiler.

---

## 2. Estrutura de arquivos

```
sokoban/
├── index.html          # Shell: header, HUD, canvas, overlay de vitória
├── main.css            # Estilos, paleta de cores e tipografia
├── game-engine.js      # TODO o jogo em um único arquivo (~300 linhas)
├── levels.json         # Definição das fases (formato texto ASCII)
├── levels.md           # Legenda dos caracteres dos mapas
├── assets/sprites/     # Sprites PNG 40×40 (pixel art)
│   ├── player_idle.png
│   ├── player_push.png
│   ├── box.png
│   ├── goal.png
│   ├── wall.png
│   └── background-320x180.png
├── piloto.aseprite     # Arquivo-fonte de arte (Aseprite)
└── refs.pur            # Referências de arte (PureRef)
```

---

## 3. Arquitetura

Arquitetura simples e linear, sem loop de jogo contínuo — o jogo é **dirigido por eventos**: cada `keydown` dispara uma movimentação, que redesenha o canvas inteiro (renderização por invalidação, não por frame).

### Fluxo de inicialização

```
loadGameData()
  └─ fetch('levels.json') → levels[]
  └─ loadSprites()        → sprites{} (Promise.all paralelo)
  └─ gameInit()
       └─ loadLevel(0)
            └─ parseLevel(map)  → walls[], boxes[], targets[], player
            └─ draw() + updateHUD()
```

### Estado global (variáveis de módulo, sem encapsulamento)

| Variável | Tipo | Papel |
|---|---|---|
| `isGameActive` | bool | Trava de input (vitória / game over) |
| `player` | `{col, row}` | Posição do jogador em células |
| `walls` / `boxes` / `targets` | `[]` de `{col, row}` | Entidades do grid |
| `levels` | `[]` | Fases carregadas do JSON |
| `movesCount` | int | Contador de passos |
| `currentLevelIndex` | int | Índice da fase atual |
| `isPushing` | bool | Troca o sprite do jogador (idle ↔ push) |
| `timerStart` / `timerInterval` / `elapsedSeconds` | — | **Timer declarado mas NÃO funcional** (ver §7) |

### Renderização

- Canvas 2D, tamanho dinâmico por fase: `map[0].length × map.length` células de 40 px.
- Ordem de desenho: `goal → wall → box → player` (alvos por baixo de tudo, jogador por cima).
- `image-rendering: pixelated` no CSS para manter o aspecto pixel art.
- Código morto: `drawTile()` (retângulos coloridos) comentado — era a versão placeholder.

### Input

Listener único de `keydown` nas setas do teclado. Sem WASD, sem suporte a toque, sem buffer/queued input.

---

## 4. Mecânicas

### Movimento e empurrão

`movePlayer(dCol, dRow)` — lógica por célula:

1. Calcula célula de destino; se parede, aborta.
2. Se há caixa na célula: verifica se a célula seguinte (na mesma direção) está livre de parede **e** de outra caixa; se sim, move a caixa e marca `isPushing = true`.
3. Move o jogador, incrementa `movesCount`.
4. `draw()` + `updateHUD()` + `checkWin()`.

Regras clássicas de Sokoban presentes: empurrar **não puxar**, movimento em grid puro (sem interpolação/dt).

### Vitória

`checkWin()`: todas as caixas sobre alvos → `levelComplete()`:

- Trava input, aguarda 400 ms, avança `currentLevelIndex`.
- Se excedeu as fases → `gameComplete()` → overlay "Omedetou!" + botão Restart.

### Timer por fase (decisão de design)

**Decisão (2026-11):** o timer existe para medir **quanto tempo o jogador leva para finalizar cada fase** — não é countdown. É métrica de desempenho, combinada com o contador de movimentos. Pendente: implementar o start no `loadLevel()` e exibir no HUD (o HTML e as variáveis já existem).

### Contador de movimentos (decisão de design)

**Decisão (2026-11):** **não haverá limite de movimentos.** O limite foi removido de propósito para dar "luz" ao contador de movimentos como métrica de desempenho — quanto menos movimentos, melhor o resultado. O `gameOver()` e o código comentado em `movePlayer()` são resíduo da versão antiga e devem ser removidos.

### Caixas travadas (decisão de design)

**Decisão (2026-11):** a intenção de design é que **caixas posicionadas em cima de alvos fiquem travadas** (sem poder ser empurradas para fora). Porém, no código atual **isso não acontece**: `movePlayer()` só verifica parede e caixa adjacente antes de mover a caixa — não checa alvo. A impressão de "travada" em jogo acontece quando a caixa está encostada na parede. Implementação futura: bloquear empurrão de caixa sobre alvo (~3 linhas).

### Relatório de fase (decisão de design)

**Decisão (2026-11):** ao final de cada fase, exibir um pequeno **relatório** com o tempo do jogador e o número de movimentos, perguntando se ele quer ir para a próxima fase ou sair do jogo. Substitui a transição automática de 400 ms atual.

---

## 5. Dados de fase

`levels.json` — array de níveis com `id`, `name` e `map` (array de strings ASCII):

```
#  parede        $  caixa
@  jogador       .  alvo (target)
```

- Fases atuais: `0 "Tutorial"`, `1 "First real level"`, `2 ""` (sem nome — pendência).
- Todos os mapas têm 20×10 células e são validáveis visualmente em texto.
- O formato ASCII é um ponto forte para migração: qualquer engine lê isso (basta um parser de 1 loop).

---

## 6. Assets e direção de arte

- Sprites próprios (pixel art 40×40), IP do autor — **não reutilizar fora do projeto**.
- Fontes Google: `Bytesized` (título), `IBM Plex Mono` (conteúdo), `Pixelify Sans` (carregada no HTML, licença OFL).
- Paleta CSS definida em `:root` (white/orange/red/mint/yellow/green/lightblue sobre fundo escuro).
- Background `background-320x180.png` foi salvo na pasta **por engano** (não pertence ao projeto) — candidato a remoção da pasta de assets.
- Fontes de arte: `piloto.aseprite`, `refs.pur` — fora do build, apenas produção.

---

## 7. Pendências e débitos técnicos

| # | Item | Decisão / Detalhe |
|---|---|---|
| 1 | Timer do HUD | **Implementar** (decisão de design): medir tempo por fase como métrica de desempenho. Start no `loadLevel()`, atualização no HUD |
| 2 | Limite de movimentos | **Removido de propósito** (decisão de design): moves é métrica, não recurso. Limpar `gameOver()` e código comentado residual |
| 3 | Travamento de caixas | **Decidido que deve travar**, mas ausente no código. Implementar bloqueio de empurrão de caixa sobre alvo |
| 4 | Relatório de fase | **Decisão de design**: ao fim da fase, mostrar tempo + moves e perguntar: próxima fase ou sair do jogo |
| 5 | Fase 3 sem nome | `name: ""` no levels.json |
| 6 | Background salvo por engano | `background-320x180.png` não pertence ao projeto — remover da pasta |
| 7 | Input limitado | Só setas; sem WASD, touch, undo/restart por nível, seleção de fase |
| 8 | Estado global sem encapsulamento | Tudo em variáveis de módulo; sem save/load, não preserva progresso |
| 9 | Código morto | `drawTile()` comentado, checagens comentadas em `movePlayer()` |
| 10 | Sem testes | Nenhuma cobertura; `checkWin` e empurrão são fáceis de unit-testar |

---

## 8. Notas para a migração para Unity

Pontos que tornam a migração barata:

1. **O formato de fases ASCII é portável**: manter `levels.json` como `TextAsset` e reescrever `parseLevel()` como um `LevelParser` em C# (mesmo loop de caracteres).
2. **Sprites 40×40 com `image-rendering: pixelated`** mapeiam direto para: `Filter Mode = Point`, `Pixels Per Unit = 40`, Tilemap + SpriteRenderer ou `Image` em Canvas.
3. **Lógica por célula** vira um `GridSystem` desacoplado da Unity (código puro C#, testável com NUnit/EditMode tests) + uma camada de visual que espelha o estado — o oposto do acoplamento atual de estado+render.
4. **Input por evento** vira `Update()` com `Input.GetKeyDown` ou o novo Input System (WASD + setas + gamepad de graça).
5. **Migração de pendências é oportunidade**: implementar na versão Unity desde o início: timer por fase, travamento de caixas sobre alvo, tela de relatório de fase (tempo + moves + "próxima fase / sair"), e undo (pilha de estados — trivial com o estado em structs). Não migrar `gameOver()` por limite de moves — mecânica descartada.
6. Arte: exportar do Aseprite (`piloto.aseprite`) para PNGs; PureRef é só referência.
7. Destino itch.io: build WebGL do Unity funciona, mas o jogo atual roda "de graça" numa página estática — avaliar se o alvo da migração é WebGL ou standalone.

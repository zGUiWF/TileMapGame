# Tilemap Demo Game — Unity 2D

**Colete 5 moedas num mapa tile-based 2D desviando de árvores, água e paredes.**

Projeto demonstrativo desenvolvido para a disciplina de **Computação Gráfica**, com o objetivo de apresentar o funcionamento do sistema de **Tilemaps da Unity** — um dos recursos mais importantes para criação de cenários em jogos 2D.

---

## Integrantes

| Nome | RA |
|------|-----|
| Guilherme Winck Ferrari | 1134330 |
| Guilherme Menezes  | 1134714 |
| Cristiano Salles | 1133511 |

---

## O que são Tilemaps?

Tilemap é um sistema que permite construir cenários 2D posicionando pequenos pedaços de arte reutilizáveis (chamados **tiles**) em uma grade (grid). Em vez de desenhar o mapa inteiro como uma única imagem, o desenvolvedor cria peças individuais — um bloco de grama, um pedaço de parede, água — e monta o cenário encaixando essas peças, como um mosaico.

### Componentes do sistema na Unity

- **Grid:** objeto pai que define o layout da grade (retangular, hexagonal ou isométrico).
- **Tilemap:** camada onde os tiles são posicionados. Um projeto pode ter várias camadas (chão, obstáculos, decoração).
- **Tile:** asset que referencia um sprite e pode conter comportamentos customizados.
- **Tile Palette:** ferramenta visual para organizar e "pintar" tiles diretamente na cena.
- **Tilemap Collider 2D:** gera colisão automaticamente para os tiles marcados como sólidos.
- **Composite Collider 2D:** combina múltiplos colliders de tiles em uma forma otimizada.

### Por que usar Tilemaps?

O principal benefício é **performance**. A Unity agrupa tiles em batches durante a renderização, reduzindo drasticamente os draw calls. Em testes da própria Unity, um cenário com sprites individuais consumia 244ms/frame e 1.1GB de RAM, enquanto o mesmo cenário com Tilemap consumia apenas 13ms/frame e 21MB de RAM. Além da performance, Tilemaps aceleram o fluxo de trabalho do level designer, permitindo iteração rápida no design dos mapas.

---

## Relação com jogos Indie e AAA

O sistema de tilemaps é amplamente utilizado tanto em jogos independentes quanto em títulos de grande escala:

### Jogos Indie

| Jogo | Como usa Tilemaps |
|------|-------------------|
| **Stardew Valley** | Todo o mundo é construído com tiles usando o editor tIDE/Tiled. Camadas de chão, objetos e colisão são separadas. O sistema de modding do jogo permite que jogadores editem os mapas diretamente. |
| **Celeste** | Plataformas, paredes e terrenos são compostos por tiles. O jogo usa tiles com colisão para definir superfícies onde o jogador pode escalar, pular e fazer dash. |
| **Terraria** | Cada bloco do mundo é um tile individual. O jogo usa tilemaps destrutíveis — o jogador pode cavar e construir modificando os tiles em tempo real. |
| **Spelunky** | Usa tilemaps com geração procedural — os níveis são montados combinando "rooms" pré-desenhadas com tiles, garantindo que cada partida seja diferente. |
| **Undertale** | Cenários top-down construídos com tiles simples. As áreas de colisão e interação são definidas por tiles específicos no mapa. |

### Jogos AAA / Grande escala

| Jogo | Como usa Tilemaps |
|------|-------------------|
| **Pokémon** (série principal) | Desde a primeira geração, todos os mapas são grids de tiles. Grama, caminhos, prédios e água são tiles com comportamentos distintos (grama alta = encontros aleatórios). |
| **The Legend of Zelda** (2D) | O mundo de Link to the Past e os títulos originais são inteiramente tile-based, com camadas de colisão e interação. |
| **Fire Emblem** | O grid tático do jogo é um tilemap onde cada tile define tipo de terreno, bônus de defesa e custo de movimento. |
| **Factorio** | O mapa industrial gigante é composto por tiles que representam terreno, máquinas e esteiras. O sistema suporta milhões de tiles simultâneos com alta performance. |
| **RimWorld** | Toda a colônia é um tilemap — pisos, paredes, portas e terreno natural são tiles individuais com propriedades de beleza, limpeza e isolamento térmico. |

---

## Funcionamento do protótipo

### Visão geral

O protótipo é um jogo top-down 2D onde o jogador se move por um mapa de 16×12 tiles, coletando 5 moedas enquanto desvia de obstáculos (árvores, água e paredes). O objetivo é coletar todas as moedas no menor número de passos.

### Tipos de tiles utilizados

| Tile | Cor | Comportamento |
|------|-----|---------------|
| Grama | Verde `#6AB04C` | Caminhável — tile base do mapa |
| Caminho | Bege `#C8A96E` | Caminhável — indica rota principal |
| Água | Azul `#4A90D9` | Bloqueante — colisão impede passagem |
| Árvore | Verde escuro `#2D5A1E` | Bloqueante — colisão impede passagem |
| Parede | Marrom `#8A7060` | Bloqueante — colisão impede passagem |

### Estrutura de camadas

O projeto utiliza 3 camadas (Sorting Layers), demonstrando como Tilemaps permitem separar elementos visuais:

1. **Ground (camada 0):** Grama e Caminho — preenchem todo o mapa como base.
2. **Obstacles (camada 1):** Água, Árvores e Paredes — pintados por cima da camada de chão.
3. **Entities (camada 2):** Jogador e Moedas — GameObjects posicionados sobre o mapa.

### Sistema de colisão

A colisão utiliza o **Tilemap Collider 2D** combinado com o **Composite Collider 2D**. Os tiles de obstáculo são marcados com `Collider Type = Grid`, e o Composite Collider une todos em uma forma otimizada. O jogador possui um `BoxCollider2D` que impede a passagem por tiles sólidos.

### Mapa do jogo

```
T T T T T T T T T T T T T T T T       T = Árvore
T . . . = . . . . T . . . . . T       . = Grama
T . $ . = . T . . . . . $ . . T       = = Caminho
T . . . = . T . . . . T T . . T       ~ = Água
T = = = = . . . ~ ~ . . . . . T       # = Parede
T . . . . . . ~ ~ ~ ~ . . . . T       $ = Moeda (5 total)
T . . # # # . ~ ~ ~ . . # # . T
T . . # $ # . . ~ . . . # $ . T       Spawn do jogador: (1,1)
T . . # . # . . . . . . # # . T
T . . . . . . . . T . . . . . T
T . $ . . . T . . T . . . . . T
T T T T T T T T T T T T T T T T
```

### Scripts principais

- **MapGenerator.cs** — Lê a matriz do mapa e pinta os tiles programaticamente nas camadas Ground e Obstacles. Instancia as moedas e o jogador nas posições corretas.
- **PlayerController.cs** — Movimentação discreta (tile a tile) com WASD/Setas. Verifica colisão antes de mover consultando o Tilemap de obstáculos.
- **CoinCollectible.cs** — Detecta colisão com o jogador via trigger e notifica o GameManager.
- **GameManager.cs** — Singleton que gerencia estado do jogo (moedas coletadas, passos, condição de vitória).
- **UIManager.cs** — Atualiza o HUD (contador de moedas e passos) e exibe a tela de vitória.

### Conceitos de Tilemap demonstrados

| Conceito | Onde é demonstrado |
|----------|-------------------|
| Grid e Tilemap | Mapa 16×12 com tiles de 32×32 pixels |
| Tile Palette | Tiles criados como assets e pintados via script |
| Sorting Layers | 3 camadas separando chão, obstáculos e entidades |
| Tilemap Collider 2D | Obstáculos bloqueiam movimento do jogador |
| Composite Collider 2D | Colliders combinados para performance |
| Geração procedural via script | MapGenerator pinta o mapa a partir de uma matriz |

---

## Como executar

### Requisitos

- Unity 2022.3 LTS ou superior
- Pacotes incluídos: 2D Sprite, 2D Tilemap Editor (já vêm por padrão)

### Passos

1. Clone o repositório:
   ```bash
   git clone https://github.com/SEU_USUARIO/tilemap-demo-game.git
   ```
2. Abra o projeto no Unity Hub.
3. Abra a cena `Assets/Scenes/GameScene.unity`.
4. Pressione **Play** — o mapa é gerado automaticamente via script.

### Controles

- **WASD** ou **Setas** — Mover o jogador
- O objetivo é coletar as 5 moedas no menor número de passos

---

## Estrutura do projeto

```
Assets/
├── Scenes/
│   └── GameScene.unity
├── Scripts/
│   ├── PlayerController.cs      # Movimentação tile-based
│   ├── CoinCollectible.cs       # Coleta de moedas com trigger
│   ├── GameManager.cs           # Estado do jogo (singleton)
│   ├── UIManager.cs             # HUD e tela de vitória
│   └── MapGenerator.cs          # Geração do mapa via código
├── Tiles/                       # Assets de tiles
├── Sprites/                     # Sprites do tileset, player e moeda
├── Prefabs/                     # Prefabs de Player e Coin
├── UI/                          # Canvas e elementos de interface
└── Animations/                  # Animação de bounce da moeda
```

---

## Referências

- [Unity Learn — Introduction to Tilemaps](https://learn.unity.com/tutorial/introduction-to-tilemaps)
- [Unity — Optimize performance of 2D games with Tilemap](https://unity.com/how-to/optimize-performance-2d-games-unity-tilemap)
- [Unity — Isometric 2D environments with Tilemap](https://unity.com/blog/engine-platform/isometric-2d-environments-with-tilemap)
- [Unity — Create art and gameplay with 2D Tilemaps](https://unity.com/how-to/create-art-and-gameplay-2d-tilemaps-unity)

---

## Licença

Este projeto é de uso acadêmico, desenvolvido para fins de avaliação na disciplina de Computação Gráfica.

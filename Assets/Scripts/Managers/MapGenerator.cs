using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Gera o mapa do jogo automaticamente a partir de uma matriz.
/// Cria tiles programaticamente, pinta os dois Tilemaps (Ground e Obstacles),
/// e instancia o jogador e as moedas nas posições corretas.
///
/// Este script deve ser anexado a um GameObject na cena (ex: "MapGenerator").
/// Ele precisa das referências aos Tilemaps configuradas no Inspector.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    [Header("Referências aos Tilemaps")]
    [Tooltip("Tilemap da camada Ground (grama e caminho)")]
    [SerializeField] private Tilemap _groundTilemap;
    [Tooltip("Tilemap da camada Obstacles (água, árvores, paredes)")]
    [SerializeField] private Tilemap _obstacleTilemap;

    [Header("Referências aos Prefabs")]
    [Tooltip("Prefab do jogador (será instanciado no spawn)")]
    [SerializeField] private GameObject _playerPrefab;
    [Tooltip("Prefab da moeda (será instanciado nas posições de moeda)")]
    [SerializeField] private GameObject _coinPrefab;

    // Constantes do mapa
    private const int COLS = 16;
    private const int ROWS = 12;

    /// <summary>
    /// Legenda do mapa:
    /// 0 = Grama, 1 = Caminho, 2 = Água, 3 = Árvore, 4 = Parede, 5 = Moeda (no chão de grama)
    /// </summary>
    private readonly int[,] _mapData = new int[ROWS, COLS]
    {
        { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 },
        { 3, 0, 0, 0, 1, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 3 },
        { 3, 0, 5, 0, 1, 0, 3, 0, 0, 0, 0, 0, 5, 0, 0, 3 },
        { 3, 0, 0, 0, 1, 0, 3, 0, 0, 0, 0, 3, 3, 0, 0, 3 },
        { 3, 1, 1, 1, 1, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 3 },
        { 3, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 3 },
        { 3, 0, 0, 4, 4, 4, 0, 2, 2, 2, 0, 0, 4, 4, 0, 3 },
        { 3, 0, 0, 4, 5, 4, 0, 0, 2, 0, 0, 0, 4, 5, 0, 3 },
        { 3, 0, 0, 4, 0, 4, 0, 0, 0, 0, 0, 0, 4, 4, 0, 3 },
        { 3, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 3 },
        { 3, 0, 5, 0, 0, 0, 3, 0, 0, 3, 0, 0, 0, 0, 0, 3 },
        { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 },
    };

    // Tiles gerados programaticamente (usando a classe Tile da Unity)
    private Tile _grassTile;
    private Tile _pathTile;
    private Tile _waterTile;
    private Tile _treeTile;
    private Tile _wallTile;

    /// <summary>
    /// Awake é chamado antes de Start. Criamos os tiles aqui para que
    /// estejam prontos quando GenerateMap() for chamado.
    /// </summary>
    private void Awake()
    {
        CreateTiles();
    }

    /// <summary>
    /// Start é chamado no primeiro frame. Configura materiais e gera o mapa.
    /// </summary>
    private void Start()
    {
        // No URP 2D, sprites precisam do shader Unlit para renderizar sem depender de luzes.
        // Sprite-Lit-Default (padrão) pode renderizar preto se a 2D Light não estiver configurada.
        SetUnlitMaterial();
        GenerateMap();
    }

    /// <summary>
    /// Configura o material Unlit nos TilemapRenderers para garantir que os tiles
    /// sejam visíveis independente da configuração de luzes 2D.
    /// </summary>
    private void SetUnlitMaterial()
    {
        Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (unlitShader == null)
            unlitShader = Shader.Find("Sprites/Default");

        if (unlitShader == null) return;

        _unlitMaterial = new Material(unlitShader);

        // Aplica nos TilemapRenderers
        var groundRenderer = _groundTilemap.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
        if (groundRenderer != null) groundRenderer.material = _unlitMaterial;

        var obstacleRenderer = _obstacleTilemap.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
        if (obstacleRenderer != null) obstacleRenderer.material = _unlitMaterial;
    }

    // Material compartilhado para sprites Unlit
    private Material _unlitMaterial;

    /// <summary>
    /// Cria os assets de Tile programaticamente usando sprites gerados pelo SpriteGenerator.
    /// Tile: asset da Unity que representa um único quadrado no Tilemap.
    /// ScriptableObject.CreateInstance: cria um asset em memória (sem salvar no disco).
    /// </summary>
    private void CreateTiles()
    {
        _grassTile = ScriptableObject.CreateInstance<Tile>();
        _grassTile.sprite = SpriteGenerator.CreateGrassSprite();

        _pathTile = ScriptableObject.CreateInstance<Tile>();
        _pathTile.sprite = SpriteGenerator.CreatePathSprite();

        _waterTile = ScriptableObject.CreateInstance<Tile>();
        _waterTile.sprite = SpriteGenerator.CreateWaterSprite();
        _waterTile.colliderType = Tile.ColliderType.Grid; // Habilita colisão

        _treeTile = ScriptableObject.CreateInstance<Tile>();
        _treeTile.sprite = SpriteGenerator.CreateTreeSprite();
        _treeTile.colliderType = Tile.ColliderType.Grid;

        _wallTile = ScriptableObject.CreateInstance<Tile>();
        _wallTile.sprite = SpriteGenerator.CreateWallSprite();
        _wallTile.colliderType = Tile.ColliderType.Grid;
    }

    /// <summary>
    /// Itera pela matriz do mapa e pinta os tiles nos Tilemaps.
    /// Também instancia o jogador e as moedas.
    ///
    /// Conversão de coordenadas:
    /// - Na matriz, row 0 = topo do mapa
    /// - Na Unity, Y cresce pra cima
    /// - Fórmula: worldY = (ROWS - 1 - row)
    /// </summary>
    private void GenerateMap()
    {
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLS; col++)
            {
                int tileType = _mapData[row, col];

                // Converte coordenada da matriz para posição no tilemap
                // row 0 = topo do mapa → y mais alto na Unity
                Vector3Int cellPos = new Vector3Int(col, ROWS - 1 - row, 0);

                // Camada Ground: sempre pinta grama (ou caminho se for tipo 1)
                if (tileType == 1)
                    _groundTilemap.SetTile(cellPos, _pathTile);
                else
                    _groundTilemap.SetTile(cellPos, _grassTile);

                // Camada Obstacles: pinta obstáculos por cima
                switch (tileType)
                {
                    case 2: // Água
                        _obstacleTilemap.SetTile(cellPos, _waterTile);
                        break;
                    case 3: // Árvore
                        _obstacleTilemap.SetTile(cellPos, _treeTile);
                        break;
                    case 4: // Parede
                        _obstacleTilemap.SetTile(cellPos, _wallTile);
                        break;
                    case 5: // Moeda — spawna prefab e pinta grama embaixo
                        SpawnCoin(col, row);
                        break;
                }
            }
        }

        // Spawna o jogador na posição (1, 10) da grid (conforme spec)
        SpawnPlayer(1, 10);
    }

    /// <summary>
    /// Instancia uma moeda na posição world correspondente à célula (col, row).
    /// A posição world é o centro da célula no tilemap.
    /// </summary>
    private void SpawnCoin(int col, int row)
    {
        if (_coinPrefab == null) return;

        // Converte para posição world (centro da célula)
        Vector3Int cellPos = new Vector3Int(col, ROWS - 1 - row, 0);
        Vector3 worldPos = _groundTilemap.GetCellCenterWorld(cellPos);

        GameObject coin = Instantiate(_coinPrefab, worldPos, Quaternion.identity);

        // Sprites gerados via Texture2D não persistem em prefabs, então
        // precisamos atribuí-los em runtime após instanciar.
        SetupSpriteRenderer(coin, SpriteGenerator.CreateCoinSprite(), "Entities", 0);
    }

    /// <summary>
    /// Instancia o jogador na posição world correspondente à célula (col, row).
    /// Configura a referência ao obstacleTilemap no PlayerController.
    /// </summary>
    private void SpawnPlayer(int col, int row)
    {
        if (_playerPrefab == null) return;

        Vector3Int cellPos = new Vector3Int(col, ROWS - 1 - row, 0);
        Vector3 worldPos = _groundTilemap.GetCellCenterWorld(cellPos);

        GameObject player = Instantiate(_playerPrefab, worldPos, Quaternion.identity);

        // Configura a referência ao tilemap de obstáculos no PlayerController
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
            controller.ObstacleTilemap = _obstacleTilemap;

        // Atribui sprite e material em runtime
        SetupSpriteRenderer(player, SpriteGenerator.CreatePlayerSprite(), "Entities", 1);
    }

    /// <summary>
    /// Configura o SpriteRenderer de um GameObject instanciado:
    /// atribui o sprite gerado programaticamente, o material Unlit e o sorting layer.
    /// </summary>
    private void SetupSpriteRenderer(GameObject obj, Sprite sprite, string sortingLayer, int sortingOrder)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.sprite = sprite;
        sr.sortingLayerName = sortingLayer;
        sr.sortingOrder = sortingOrder;

        if (_unlitMaterial != null)
            sr.material = _unlitMaterial;
    }
}

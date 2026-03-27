using UnityEngine;

/// <summary>
/// Gera todos os sprites do jogo programaticamente usando Texture2D.
/// Cada sprite é criado pixel a pixel com as cores definidas na spec.
/// Isso torna o projeto auto-contido, sem precisar de assets externos.
///
/// Texture2D: classe da Unity que representa uma textura (imagem) na memória.
/// Sprite.Create: converte uma Texture2D em um Sprite usável por SpriteRenderers.
/// </summary>
public static class SpriteGenerator
{
    private const int TILE_SIZE = 32;
    private const float PPU = 32f; // Pixels Per Unit

    // --- Cores dos tiles (definidas na spec) ---
    private static readonly Color GRASS_1 = HexToColor("6AB04C");
    private static readonly Color GRASS_2 = HexToColor("5A9E40");
    private static readonly Color PATH_1 = HexToColor("C8A96E");
    private static readonly Color PATH_2 = HexToColor("B89A60");
    private static readonly Color WATER_1 = HexToColor("4A90D9");
    private static readonly Color WATER_2 = HexToColor("3D7FC4");
    private static readonly Color WATER_WAVE = Color.white;
    private static readonly Color TREE_TRUNK = HexToColor("4A3520");
    private static readonly Color TREE_CANOPY = HexToColor("3A8A25");
    private static readonly Color WALL_1 = HexToColor("8A7060");
    private static readonly Color WALL_2 = HexToColor("6A5545");

    // --- Cores do jogador ---
    private static readonly Color PLAYER_BODY = HexToColor("E74C3C");
    private static readonly Color PLAYER_HEAD = HexToColor("C0392B");

    // --- Cores da moeda ---
    private static readonly Color COIN_OUTER = HexToColor("F9CA24");
    private static readonly Color COIN_INNER = HexToColor("F0B500");

    /// <summary>
    /// Converte uma string hexadecimal (sem #) para Color da Unity.
    /// </summary>
    private static Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }

    /// <summary>
    /// Cria uma Texture2D e a converte para Sprite.
    /// pivot (0.5, 0.5) centraliza o sprite no tile.
    /// </summary>
    private static Sprite TextureToSprite(Texture2D texture)
    {
        texture.filterMode = FilterMode.Point; // Pixel art sem suavização
        texture.Apply();
        return Sprite.Create(
            texture,
            new Rect(0, 0, TILE_SIZE, TILE_SIZE),
            new Vector2(0.5f, 0.5f),
            PPU
        );
    }

    /// <summary>
    /// Gera o sprite de Grama: padrão checkerboard sutil entre 2 tons de verde.
    /// Checkerboard = quadrados alternados como um tabuleiro de xadrez.
    /// </summary>
    public static Sprite CreateGrassSprite()
    {
        var tex = new Texture2D(TILE_SIZE, TILE_SIZE);
        for (int x = 0; x < TILE_SIZE; x++)
            for (int y = 0; y < TILE_SIZE; y++)
            {
                // Alterna cores a cada bloco de 4x4 pixels
                bool isLight = ((x / 4) + (y / 4)) % 2 == 0;
                tex.SetPixel(x, y, isLight ? GRASS_1 : GRASS_2);
            }
        return TextureToSprite(tex);
    }

    /// <summary>
    /// Gera o sprite de Caminho: padrão checkerboard sutil em tons de bege.
    /// </summary>
    public static Sprite CreatePathSprite()
    {
        var tex = new Texture2D(TILE_SIZE, TILE_SIZE);
        for (int x = 0; x < TILE_SIZE; x++)
            for (int y = 0; y < TILE_SIZE; y++)
            {
                bool isLight = ((x / 4) + (y / 4)) % 2 == 0;
                tex.SetPixel(x, y, isLight ? PATH_1 : PATH_2);
            }
        return TextureToSprite(tex);
    }

    /// <summary>
    /// Gera o sprite de Água: fundo azul com ondas brancas horizontais.
    /// </summary>
    public static Sprite CreateWaterSprite()
    {
        var tex = new Texture2D(TILE_SIZE, TILE_SIZE);
        for (int x = 0; x < TILE_SIZE; x++)
            for (int y = 0; y < TILE_SIZE; y++)
            {
                // Alterna cores base
                bool isLight = ((x / 4) + (y / 4)) % 2 == 0;
                Color baseColor = isLight ? WATER_1 : WATER_2;

                // Ondas brancas a cada 8 pixels verticais (linhas de 1px)
                if (y % 8 == 0 && x % 3 != 0)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.5f));
                else
                    tex.SetPixel(x, y, baseColor);
            }
        return TextureToSprite(tex);
    }

    /// <summary>
    /// Gera o sprite de Árvore: tronco retangular marrom + copa circular verde.
    /// Fundo transparente para que a grama apareça por baixo (na camada Ground).
    /// </summary>
    public static Sprite CreateTreeSprite()
    {
        var tex = new Texture2D(TILE_SIZE, TILE_SIZE);
        Color transparent = new Color(0, 0, 0, 0);

        // Preenche tudo com cor sólida (fundo verde escuro para preencher o tile)
        for (int x = 0; x < TILE_SIZE; x++)
            for (int y = 0; y < TILE_SIZE; y++)
                tex.SetPixel(x, y, GRASS_2);

        // Tronco: retângulo central na parte inferior
        for (int x = 12; x < 20; x++)
            for (int y = 0; y < 14; y++)
                tex.SetPixel(x, y, TREE_TRUNK);

        // Copa: círculo verde na parte superior
        int centerX = 16, centerY = 22;
        int radius = 11;
        for (int x = 0; x < TILE_SIZE; x++)
            for (int y = 10; y < TILE_SIZE; y++)
            {
                float dist = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                if (dist <= radius)
                    tex.SetPixel(x, y, TREE_CANOPY);
            }

        return TextureToSprite(tex);
    }

    /// <summary>
    /// Gera o sprite de Parede: padrão de tijolos com linhas de argamassa.
    /// </summary>
    public static Sprite CreateWallSprite()
    {
        var tex = new Texture2D(TILE_SIZE, TILE_SIZE);
        Color mortar = WALL_2; // Cor da argamassa (linhas entre tijolos)

        for (int x = 0; x < TILE_SIZE; x++)
            for (int y = 0; y < TILE_SIZE; y++)
            {
                // Linhas horizontais de argamassa a cada 8 pixels
                if (y % 8 == 0)
                {
                    tex.SetPixel(x, y, mortar);
                    continue;
                }

                // Linhas verticais de argamassa alternadas por fileira
                int row = y / 8;
                int offset = (row % 2 == 0) ? 0 : 8;
                if ((x + offset) % 16 == 0)
                {
                    tex.SetPixel(x, y, mortar);
                    continue;
                }

                // Corpo do tijolo
                tex.SetPixel(x, y, WALL_1);
            }
        return TextureToSprite(tex);
    }

    /// <summary>
    /// Gera o sprite do jogador: corpo retangular vermelho com cabeça e olhos.
    /// </summary>
    public static Sprite CreatePlayerSprite()
    {
        var tex = new Texture2D(TILE_SIZE, TILE_SIZE);
        Color transparent = new Color(0, 0, 0, 0);

        // Preenche com transparente
        for (int x = 0; x < TILE_SIZE; x++)
            for (int y = 0; y < TILE_SIZE; y++)
                tex.SetPixel(x, y, transparent);

        // Corpo: retângulo vermelho (da base até o meio)
        for (int x = 8; x < 24; x++)
            for (int y = 2; y < 20; y++)
                tex.SetPixel(x, y, PLAYER_BODY);

        // Cabeça: retângulo menor e mais escuro no topo
        for (int x = 10; x < 22; x++)
            for (int y = 20; y < 30; y++)
                tex.SetPixel(x, y, PLAYER_HEAD);

        // Olhos: 2 pixels brancos com pupilas escuras
        tex.SetPixel(13, 25, Color.white);
        tex.SetPixel(14, 25, Color.white);
        tex.SetPixel(18, 25, Color.white);
        tex.SetPixel(19, 25, Color.white);
        // Pupilas
        tex.SetPixel(14, 25, Color.black);
        tex.SetPixel(19, 25, Color.black);

        return TextureToSprite(tex);
    }

    /// <summary>
    /// Gera o sprite da moeda: círculo amarelo externo + círculo interno mais escuro.
    /// </summary>
    public static Sprite CreateCoinSprite()
    {
        var tex = new Texture2D(TILE_SIZE, TILE_SIZE);
        Color transparent = new Color(0, 0, 0, 0);

        // Preenche com transparente
        for (int x = 0; x < TILE_SIZE; x++)
            for (int y = 0; y < TILE_SIZE; y++)
                tex.SetPixel(x, y, transparent);

        int center = TILE_SIZE / 2;

        // Círculo externo (raio 12)
        for (int x = 0; x < TILE_SIZE; x++)
            for (int y = 0; y < TILE_SIZE; y++)
            {
                float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                if (dist <= 12f)
                    tex.SetPixel(x, y, COIN_OUTER);
                if (dist <= 8f)
                    tex.SetPixel(x, y, COIN_INNER);
            }

        return TextureToSprite(tex);
    }
}

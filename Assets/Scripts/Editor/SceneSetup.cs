using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Script de Editor que configura toda a cena do jogo automaticamente.
/// Acesse pelo menu: Tools > Setup Game Scene
///
/// Este script cria: Sorting Layers, Grid + Tilemaps, Prefabs (Player e Coin),
/// Canvas com UI completa, GameManager, MapGenerator e câmera configurada.
///
/// IMPORTANTE: Este script só funciona dentro do Unity Editor (pasta Editor).
/// Ele não é incluído no build final do jogo.
/// </summary>
public class SceneSetup
{
    /// <summary>
    /// Ponto de entrada: aparece no menu Tools do Unity Editor.
    /// MenuItem é um atributo que registra um método como item de menu.
    /// </summary>
    [MenuItem("Tools/Setup Game Scene")]
    public static void SetupScene()
    {
        // 1. Configura Sorting Layers
        SetupSortingLayers();

        // 2. Limpa a cena atual (remove objetos padrão desnecessários)
        ClearDefaultScene();

        // 3. Cria a estrutura do Grid + Tilemaps
        GameObject grid = CreateGridAndTilemaps();

        // 4. Cria os Prefabs (Player e Coin)
        GameObject playerPrefab = CreatePlayerPrefab();
        GameObject coinPrefab = CreateCoinPrefab();

        // 5. Cria o MapGenerator e conecta referências
        CreateMapGenerator(grid, playerPrefab, coinPrefab);

        // 6. Cria o GameManager
        CreateGameManager();

        // 7. Cria o Canvas com toda a UI
        CreateUICanvas();

        // 8. Configura a câmera
        SetupCamera();

        Debug.Log("✅ Cena configurada com sucesso! Aperte Play para testar.");
    }

    /// <summary>
    /// Configura os Sorting Layers necessários: Ground, Obstacles, Entities.
    /// Sorting Layers definem a ordem de renderização dos sprites (quem fica na frente de quem).
    /// </summary>
    private static void SetupSortingLayers()
    {
        // Acessa o TagManager (onde ficam as tags e sorting layers)
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );
        SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");

        // Adiciona layers se não existirem
        string[] layersToAdd = { "Ground", "Obstacles", "Entities" };
        foreach (string layerName in layersToAdd)
        {
            bool exists = false;
            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                if (sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == layerName)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
                var newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
                newLayer.FindPropertyRelative("name").stringValue = layerName;
                // UniqueID é gerado automaticamente pela Unity
                newLayer.FindPropertyRelative("uniqueID").intValue = layerName.GetHashCode();
            }
        }
        tagManager.ApplyModifiedProperties();
        Debug.Log("Sorting Layers configurados: Ground, Obstacles, Entities");
    }

    /// <summary>
    /// Remove a luz direcional padrão que a Unity cria em cenas novas.
    /// </summary>
    private static void ClearDefaultScene()
    {
        // Remove Directional Light (não usamos em 2D)
        var light = GameObject.Find("Directional Light");
        if (light != null) Object.DestroyImmediate(light);

        // Remove Global Volume se existir (URP cria automaticamente)
        var volume = GameObject.Find("Global Volume");
        if (volume != null) Object.DestroyImmediate(volume);
    }

    /// <summary>
    /// Cria a estrutura Grid > GroundTilemap + ObstacleTilemap.
    /// Grid: componente da Unity que organiza Tilemaps em uma grade.
    /// Configura colisão no ObstacleTilemap (Tilemap Collider 2D + Composite Collider 2D).
    /// </summary>
    private static GameObject CreateGridAndTilemaps()
    {
        // Cria o Grid pai
        GameObject gridObj = new GameObject("Grid");
        Grid grid = gridObj.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0); // 1 unidade por célula

        // --- GroundTilemap ---
        GameObject groundObj = new GameObject("GroundTilemap");
        groundObj.transform.SetParent(gridObj.transform);
        Tilemap groundTilemap = groundObj.AddComponent<Tilemap>();
        TilemapRenderer groundRenderer = groundObj.AddComponent<TilemapRenderer>();
        groundRenderer.sortingLayerName = "Ground";
        groundRenderer.sortingOrder = 0;

        // --- ObstacleTilemap ---
        GameObject obstacleObj = new GameObject("ObstacleTilemap");
        obstacleObj.transform.SetParent(gridObj.transform);
        Tilemap obstacleTilemap = obstacleObj.AddComponent<Tilemap>();
        TilemapRenderer obstacleRenderer = obstacleObj.AddComponent<TilemapRenderer>();
        obstacleRenderer.sortingLayerName = "Obstacles";
        obstacleRenderer.sortingOrder = 0;

        // Adiciona Tilemap Collider 2D com "Used By Composite"
        TilemapCollider2D tilemapCollider = obstacleObj.AddComponent<TilemapCollider2D>();
        tilemapCollider.usedByComposite = true;

        // Rigidbody2D estático (necessário para o Composite Collider)
        Rigidbody2D rb = obstacleObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        // Composite Collider 2D (junta todos os colliders de tile em um só — melhor performance)
        obstacleObj.AddComponent<CompositeCollider2D>();

        Debug.Log("Grid e Tilemaps criados com colisão configurada.");
        return gridObj;
    }

    /// <summary>
    /// Cria o Prefab do jogador e salva em Assets/Prefabs/.
    /// Prefab: template reutilizável de um GameObject que pode ser instanciado em runtime.
    /// </summary>
    private static GameObject CreatePlayerPrefab()
    {
        // Cria GameObject temporário na cena
        GameObject player = new GameObject("Player");

        // SpriteRenderer: componente que desenha um sprite na tela
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreatePlayerSprite();
        sr.sortingLayerName = "Entities";
        sr.sortingOrder = 1;

        // Rigidbody2D: componente de física 2D (necessário para colisão e movimento)
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f; // Sem gravidade (jogo top-down)
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Não rotaciona

        // BoxCollider2D: caixa de colisão 2D
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.8f); // Um pouco menor que o tile para facilitar navegação
        col.isTrigger = false;

        // PlayerController: nosso script de movimento
        player.AddComponent<PlayerController>();

        // Salva como Prefab
        string prefabPath = "Assets/Prefabs/Player.prefab";
        EnsureDirectoryExists(prefabPath);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
        Object.DestroyImmediate(player); // Remove da cena (será instanciado pelo MapGenerator)

        Debug.Log("Prefab do Player criado em: " + prefabPath);
        return prefab;
    }

    /// <summary>
    /// Cria o Prefab da moeda e salva em Assets/Prefabs/.
    /// </summary>
    private static GameObject CreateCoinPrefab()
    {
        GameObject coin = new GameObject("Coin");

        SpriteRenderer sr = coin.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCoinSprite();
        sr.sortingLayerName = "Entities";
        sr.sortingOrder = 0;

        // BoxCollider2D como Trigger (detecta sobreposição sem bloquear)
        BoxCollider2D col = coin.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.6f);
        col.isTrigger = true; // Trigger = não bloqueia, apenas detecta

        // CoinCollectible: nosso script de coleta
        coin.AddComponent<CoinCollectible>();

        // Salva como Prefab
        string prefabPath = "Assets/Prefabs/Coin.prefab";
        EnsureDirectoryExists(prefabPath);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(coin, prefabPath);
        Object.DestroyImmediate(coin);

        Debug.Log("Prefab da Coin criado em: " + prefabPath);
        return prefab;
    }

    /// <summary>
    /// Cria o GameObject MapGenerator e conecta todas as referências via SerializedObject.
    /// SerializedObject/SerializedProperty: API do Editor para acessar campos [SerializeField]
    /// de forma segura (funciona com Undo e suporta Prefabs).
    /// </summary>
    private static void CreateMapGenerator(GameObject grid, GameObject playerPrefab, GameObject coinPrefab)
    {
        GameObject mapGenObj = new GameObject("MapGenerator");
        MapGenerator mapGen = mapGenObj.AddComponent<MapGenerator>();

        // Conecta as referências usando SerializedObject (forma correta no Editor)
        SerializedObject so = new SerializedObject(mapGen);

        // Encontra os Tilemaps dentro do Grid
        Tilemap groundTilemap = grid.transform.Find("GroundTilemap").GetComponent<Tilemap>();
        Tilemap obstacleTilemap = grid.transform.Find("ObstacleTilemap").GetComponent<Tilemap>();

        so.FindProperty("_groundTilemap").objectReferenceValue = groundTilemap;
        so.FindProperty("_obstacleTilemap").objectReferenceValue = obstacleTilemap;
        so.FindProperty("_playerPrefab").objectReferenceValue = playerPrefab;
        so.FindProperty("_coinPrefab").objectReferenceValue = coinPrefab;
        so.ApplyModifiedProperties();

        Debug.Log("MapGenerator criado com referências configuradas.");
    }

    /// <summary>
    /// Cria o GameObject GameManager com o script singleton.
    /// </summary>
    private static void CreateGameManager()
    {
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();
        Debug.Log("GameManager criado.");
    }

    /// <summary>
    /// Cria o Canvas com toda a UI do jogo:
    /// - HUD (moedas e passos no topo)
    /// - Painel de vitória (centro, inicialmente desativado)
    /// - Dica de controles (parte inferior)
    ///
    /// Canvas: sistema de UI da Unity que renderiza elementos 2D na tela.
    /// Screen Space - Overlay: UI fica sempre na frente de tudo, em coordenadas de tela.
    /// </summary>
    private static void CreateUICanvas()
    {
        // --- Canvas ---
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // CanvasScaler: escala a UI proporcionalmente ao tamanho da tela
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1024, 768);

        // GraphicRaycaster: necessário para detectar cliques em botões
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- HUD (topo da tela) ---
        GameObject hud = CreateUIPanel("HUD", canvasObj.transform);
        RectTransform hudRect = hud.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0.5f, 1f);
        hudRect.anchorMax = new Vector2(0.5f, 1f);
        hudRect.pivot = new Vector2(0.5f, 1f);
        hudRect.anchoredPosition = new Vector2(0f, -10f);
        hudRect.sizeDelta = new Vector2(400f, 80f);

        // Adiciona layout vertical para organizar os textos
        VerticalLayoutGroup hudLayout = hud.AddComponent<VerticalLayoutGroup>();
        hudLayout.childAlignment = TextAnchor.UpperCenter;
        hudLayout.spacing = 5f;
        hudLayout.childControlWidth = true;
        hudLayout.childControlHeight = true;

        // Texto de moedas
        GameObject coinsTextObj = CreateTextElement("CoinsText", "Moedas: 0 / 5", hud.transform, 28);
        // Texto de passos
        GameObject stepsTextObj = CreateTextElement("StepsText", "Passos: 0", hud.transform, 24);

        // --- Painel de Vitória (centro, inicialmente desativado) ---
        GameObject winPanel = CreateUIPanel("WinPanel", canvasObj.transform);
        RectTransform winRect = winPanel.GetComponent<RectTransform>();
        winRect.anchorMin = Vector2.zero;
        winRect.anchorMax = Vector2.one;
        winRect.sizeDelta = Vector2.zero;

        // Fundo semi-transparente preto
        Image winBg = winPanel.AddComponent<Image>();
        winBg.color = new Color(0f, 0f, 0f, 0.7f);

        // Título de vitória
        CreateTextElement("WinTitle", "Todas as moedas coletadas!", winPanel.transform, 36);

        // Subtítulo com total de passos
        GameObject winSubtitleObj = CreateTextElement("WinSubtitle", "Parabéns! Passos: 0", winPanel.transform, 28);

        // Espaçamento
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(winPanel.transform, false);
        LayoutElement spacerLayout = spacer.AddComponent<LayoutElement>();
        spacerLayout.minHeight = 20f;

        // Botão "Jogar Novamente"
        GameObject restartBtnObj = CreateButton("RestartButton", "Jogar Novamente", winPanel.transform);

        // Layout vertical para o painel de vitória
        VerticalLayoutGroup winLayout = winPanel.AddComponent<VerticalLayoutGroup>();
        winLayout.childAlignment = TextAnchor.MiddleCenter;
        winLayout.spacing = 15f;
        winLayout.childControlWidth = false;
        winLayout.childControlHeight = false;
        winLayout.padding = new RectOffset(200, 200, 250, 200);

        winPanel.SetActive(false); // Começa desativado

        // --- Dica de Controles (parte inferior) ---
        GameObject hintObj = CreateTextElement("HintText", "Setas / WASD para mover", canvasObj.transform, 20);
        RectTransform hintRect = hintObj.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0f);
        hintRect.anchorMax = new Vector2(0.5f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.anchoredPosition = new Vector2(0f, 15f);
        hintRect.sizeDelta = new Vector2(400f, 40f);

        // Cor mais suave para a dica
        hintObj.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.7f, 0.8f);

        // --- UIManager ---
        UIManager uiManager = canvasObj.AddComponent<UIManager>();

        // Conecta as referências de UI usando SerializedObject
        SerializedObject so = new SerializedObject(uiManager);
        so.FindProperty("_coinsText").objectReferenceValue = coinsTextObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_stepsText").objectReferenceValue = stepsTextObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_winPanel").objectReferenceValue = winPanel;
        so.FindProperty("_winSubtitleText").objectReferenceValue = winSubtitleObj.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedProperties();

        // Cria EventSystem se não existir (necessário para botões funcionarem)
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        Debug.Log("Canvas e UI criados com referências configuradas.");
    }

    /// <summary>
    /// Configura a câmera ortográfica centralizada no mapa.
    /// Orthographic: projeção sem perspectiva, ideal para jogos 2D.
    /// Size = 6 mostra 12 unidades verticais (metade acima e metade abaixo do centro).
    /// </summary>
    private static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }

        cam.orthographic = true;
        cam.orthographicSize = 6f;

        // Centraliza no mapa (16 colunas x 12 linhas, cada tile = 1 unidade)
        // Centro X = 16/2 = 8, Centro Y = 12/2 = 6
        cam.transform.position = new Vector3(8f, 6f, -10f);

        // Cor de fundo escura
        cam.backgroundColor = HexToColor("2C3E50");
        cam.clearFlags = CameraClearFlags.SolidColor;

        Debug.Log("Câmera configurada: ortográfica, size=6, posição (8, 6, -10).");
    }

    // --- Helpers ---

    /// <summary>
    /// Cria um painel de UI vazio (container para outros elementos).
    /// </summary>
    private static GameObject CreateUIPanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        panel.AddComponent<RectTransform>();
        return panel;
    }

    /// <summary>
    /// Cria um elemento de texto TextMeshPro na UI.
    /// TextMeshProUGUI: versão do TextMeshPro para Canvas (UI), diferente do TextMeshPro 3D.
    /// </summary>
    private static GameObject CreateTextElement(string name, string text, Transform parent, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400f, 50f);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return textObj;
    }

    /// <summary>
    /// Cria um botão de UI com texto.
    /// Button: componente da Unity que detecta cliques e executa ações.
    /// </summary>
    private static GameObject CreateButton(string name, string label, Transform parent)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(250f, 50f);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.7f, 0.3f, 1f); // Verde

        Button btn = btnObj.AddComponent<Button>();

        // Texto do botão
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        // Conecta o botão ao GameManager.RestartGame()
        // (precisa ser feito em runtime, pois o GameManager é singleton)
        // Vamos adicionar um helper script no botão
        btnObj.AddComponent<RestartButtonHelper>();

        return btnObj;
    }

    /// <summary>
    /// Converte hex para Color (helper).
    /// </summary>
    private static Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }

    /// <summary>
    /// Garante que o diretório do asset existe.
    /// </summary>
    private static void EnsureDirectoryExists(string assetPath)
    {
        string folder = System.IO.Path.GetDirectoryName(assetPath);
        if (!AssetDatabase.IsValidFolder(folder))
        {
            string[] parts = folder.Replace("\\", "/").Split('/');
            string currentPath = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string nextPath = currentPath + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                currentPath = nextPath;
            }
        }
    }
}

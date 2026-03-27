using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/// <summary>
/// Controla o movimento discreto (tile-based) do jogador.
/// O jogador se move 1 tile por vez usando WASD ou setas.
/// Verifica colisão com obstáculos antes de mover.
///
/// Usa o New Input System (pacote InputSystem) em vez do antigo Input.GetKey().
/// O Keyboard.current lê o teclado diretamente pelo novo sistema.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [Tooltip("Tempo mínimo entre movimentos (em segundos)")]
    [SerializeField] private float _moveDelay = 0.15f;

    [Header("Referências")]
    [Tooltip("Referência ao Tilemap de obstáculos para checar colisão")]
    [SerializeField] private Tilemap _obstacleTilemap;

    // Timer interno para cooldown entre movimentos
    private float _moveTimer;

    /// <summary>
    /// Permite que o MapGenerator configure a referência ao tilemap de obstáculos.
    /// </summary>
    public Tilemap ObstacleTilemap
    {
        get => _obstacleTilemap;
        set => _obstacleTilemap = value;
    }

    /// <summary>
    /// Update é chamado todo frame. Aqui processamos o input do jogador.
    /// Usamos um timer de cooldown para evitar movimento contínuo ao segurar a tecla.
    /// </summary>
    private void Update()
    {
        // Se o jogo já foi vencido, não permite mais movimento
        if (GameManager.Instance != null && GameManager.Instance.GameWon)
            return;

        // Decrementa o timer de cooldown
        _moveTimer -= Time.deltaTime;

        // Só processa input se o cooldown já passou
        if (_moveTimer > 0f)
            return;

        // Captura direção do input (WASD ou setas)
        Vector2Int direction = GetInputDirection();

        // Se nenhuma tecla foi pressionada, sai
        if (direction == Vector2Int.zero)
            return;

        // Tenta mover na direção
        TryMove(direction);
    }

    /// <summary>
    /// Lê o input do jogador usando o New Input System.
    /// Keyboard.current acessa o teclado conectado.
    /// isPressed verifica se a tecla está pressionada neste frame.
    /// Prioriza uma direção por vez (sem diagonal).
    /// </summary>
    private Vector2Int GetInputDirection()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return Vector2Int.zero;

        // Vertical tem prioridade sobre horizontal para evitar diagonais
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            return Vector2Int.up;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            return Vector2Int.down;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            return Vector2Int.left;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            return Vector2Int.right;

        return Vector2Int.zero;
    }

    /// <summary>
    /// Tenta mover o jogador 1 tile na direção especificada.
    /// Verifica se a posição destino não tem um tile de obstáculo.
    /// </summary>
    private void TryMove(Vector2Int direction)
    {
        // Calcula a posição destino (1 unidade = 1 tile)
        Vector3 targetPosition = transform.position + new Vector3(direction.x, direction.y, 0);

        // Converte posição world para coordenada de célula no tilemap
        Vector3Int cellPosition = _obstacleTilemap.WorldToCell(targetPosition);

        // Verifica se existe um tile de obstáculo na posição destino
        if (_obstacleTilemap.HasTile(cellPosition))
            return; // Bloqueado! Não move.

        // Move o jogador para a nova posição
        transform.position = targetPosition;

        // Reinicia o cooldown
        _moveTimer = _moveDelay;

        // Notifica o GameManager que um passo foi dado
        if (GameManager.Instance != null)
            GameManager.Instance.AddStep();
    }
}

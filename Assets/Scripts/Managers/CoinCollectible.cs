using UnityEngine;

/// <summary>
/// Script anexado ao prefab da moeda.
/// Detecta colisão com o jogador (trigger) e notifica o GameManager.
/// Também aplica uma animação senoidal de "bounce" (sobe e desce).
/// </summary>
public class CoinCollectible : MonoBehaviour
{
    [Header("Animação de Bounce")]
    [Tooltip("Amplitude do movimento vertical (em unidades)")]
    [SerializeField] private float _bounceAmplitude = 0.1f;
    [Tooltip("Velocidade da animação de bounce")]
    [SerializeField] private float _bounceSpeed = 3f;

    // Posição inicial para calcular o offset do bounce
    private Vector3 _startPosition;

    /// <summary>
    /// Start é chamado no primeiro frame. Salva a posição inicial da moeda.
    /// </summary>
    private void Start()
    {
        _startPosition = transform.position;
    }

    /// <summary>
    /// Update é chamado todo frame. Aplica o efeito de bounce senoidal.
    /// Mathf.Sin gera um valor entre -1 e 1 que oscila com o tempo,
    /// criando o efeito de sobe-desce suave.
    /// </summary>
    private void Update()
    {
        float offsetY = Mathf.Sin(Time.time * _bounceSpeed) * _bounceAmplitude;
        transform.position = _startPosition + new Vector3(0f, offsetY, 0f);
    }

    /// <summary>
    /// OnTriggerEnter2D é chamado quando outro Collider2D entra no trigger desta moeda.
    /// Como o BoxCollider2D da moeda está marcado como "Is Trigger = true",
    /// este método detecta quando o jogador pisa na moeda.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se quem colidiu foi o jogador
        if (other.GetComponent<PlayerController>() != null)
        {
            // Notifica o GameManager
            if (GameManager.Instance != null)
                GameManager.Instance.CollectCoin();

            // Destrói a moeda
            Destroy(gameObject);
        }
    }
}

using UnityEngine;

/// <summary>
/// Ajusta o Viewport Rect da câmera em runtime para deixar espaço
/// para a HUD (topo) e dica de controles (embaixo).
/// Isso garante que a UI não sobreponha a área jogável.
///
/// Viewport Rect: define qual porção da tela a câmera renderiza.
/// Valores de 0 a 1, onde (0,0) é canto inferior esquerdo e (1,1) é superior direito.
/// </summary>
public class CameraSetup : MonoBehaviour
{
    [Header("Proporções da UI")]
    [Tooltip("Porcentagem da tela reservada para o painel lateral direito (0 a 1)")]
    [SerializeField] private float _rightMargin = 0.20f;

    /// <summary>
    /// Awake ajusta o viewport da câmera para ocupar apenas a parte esquerda da tela,
    /// deixando espaço à direita para o painel de UI.
    /// </summary>
    private void Awake()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            float viewportWidth = 1f - _rightMargin;
            cam.rect = new Rect(0f, 0f, viewportWidth, 1f);

            // Compensa o orthographicSize para que o mapa inteiro caiba
            cam.orthographicSize = 6f;
        }
    }
}

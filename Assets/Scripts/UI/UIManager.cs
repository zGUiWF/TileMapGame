using UnityEngine;
using TMPro;

/// <summary>
/// Gerencia todos os elementos de UI: HUD (moedas e passos),
/// painel de vitória e dica de controles.
/// </summary>
public class UIManager : MonoBehaviour
{
    // --- Singleton ---
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private TextMeshProUGUI _stepsText;

    [Header("Painel de Vitória")]
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private TextMeshProUGUI _winSubtitleText;

    /// <summary>
    /// Configura o Singleton e garante que o painel de vitória comece desativado.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Garante que o painel de vitória comece escondido
        if (_winPanel != null)
            _winPanel.SetActive(false);
    }

    /// <summary>
    /// Inicializa os textos da HUD com valores iniciais.
    /// Start é chamado depois de Awake, quando todos os objetos já foram inicializados.
    /// </summary>
    private void Start()
    {
        UpdateCoinsText(0, 5);
        UpdateStepsText(0);
    }

    /// <summary>
    /// Atualiza o texto de moedas na HUD.
    /// </summary>
    public void UpdateCoinsText(int collected, int total)
    {
        if (_coinsText != null)
            _coinsText.text = $"Moedas: {collected} / {total}";
    }

    /// <summary>
    /// Atualiza o texto de passos na HUD.
    /// </summary>
    public void UpdateStepsText(int steps)
    {
        if (_stepsText != null)
            _stepsText.text = $"Passos: {steps}";
    }

    /// <summary>
    /// Mostra o painel de vitória com o total de passos.
    /// </summary>
    public void ShowWinPanel(int totalSteps)
    {
        if (_winPanel != null)
            _winPanel.SetActive(true);

        if (_winSubtitleText != null)
            _winSubtitleText.text = $"Parabéns! Passos: {totalSteps}";
    }
}

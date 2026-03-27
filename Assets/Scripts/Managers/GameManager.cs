using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton que gerencia o estado global do jogo:
/// moedas coletadas, passos dados e condição de vitória.
/// </summary>
public class GameManager : MonoBehaviour
{
    // --- Singleton ---
    public static GameManager Instance { get; private set; }

    [Header("Estado do Jogo")]
    [SerializeField] private int _totalCoins = 5;

    private int _collectedCoins;
    private int _steps;
    private bool _gameWon;

    // Propriedades de leitura para outros scripts
    public int TotalCoins => _totalCoins;
    public int CollectedCoins => _collectedCoins;
    public int Steps => _steps;
    public bool GameWon => _gameWon;

    /// <summary>
    /// Awake é chamado antes de Start. Usado aqui para configurar o Singleton.
    /// Garante que só exista uma instância do GameManager na cena.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Chamado quando o jogador coleta uma moeda.
    /// Incrementa o contador e verifica se todas foram coletadas.
    /// </summary>
    public void CollectCoin()
    {
        _collectedCoins++;
        UIManager.Instance.UpdateCoinsText(_collectedCoins, _totalCoins);

        // Verifica condição de vitória
        if (_collectedCoins >= _totalCoins)
        {
            _gameWon = true;
            UIManager.Instance.ShowWinPanel(_steps);
        }
    }

    /// <summary>
    /// Chamado a cada movimento válido do jogador.
    /// Incrementa o contador de passos e atualiza a UI.
    /// </summary>
    public void AddStep()
    {
        _steps++;
        UIManager.Instance.UpdateStepsText(_steps);
    }

    /// <summary>
    /// Recarrega a cena atual para reiniciar o jogo.
    /// Chamado pelo botão "Jogar Novamente" na tela de vitória.
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

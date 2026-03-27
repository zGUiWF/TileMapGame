using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper que conecta o botão "Jogar Novamente" ao GameManager.RestartGame().
/// Necessário porque o GameManager é um Singleton criado em runtime,
/// então não podemos conectar o evento no Editor diretamente.
///
/// Este script é anexado ao botão e conecta o onClick no Start().
/// </summary>
public class RestartButtonHelper : MonoBehaviour
{
    /// <summary>
    /// Start é chamado no primeiro frame. Conecta o evento de clique
    /// do botão ao método RestartGame do GameManager.
    /// </summary>
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.RestartGame();
            });
        }
    }
}

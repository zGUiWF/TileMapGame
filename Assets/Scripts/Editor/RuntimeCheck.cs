using UnityEngine;
using UnityEditor;

public class RuntimeCheck
{
    [MenuItem("Tools/Runtime Check")]
    public static string Execute()
    {
        string result = "";

        // Verificar GameManager
        if (GameManager.Instance != null)
            result += $"GameManager: OK (coins={GameManager.Instance.CollectedCoins}, steps={GameManager.Instance.Steps})\n";
        else
            result += "GameManager: NULL!\n";

        // Verificar UIManager
        if (UIManager.Instance != null)
            result += "UIManager: OK\n";
        else
            result += "UIManager: NULL!\n";

        // Verificar Player
        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
            result += $"Player: OK (pos={player.transform.position}, tilemap={player.ObstacleTilemap != null})\n";
        else
            result += "Player: NULL!\n";

        // Verificar Coins
        var coins = Object.FindObjectsByType<CoinCollectible>(FindObjectsSortMode.None);
        result += $"Coins: {coins.Length} na cena\n";

        return result;
    }
}

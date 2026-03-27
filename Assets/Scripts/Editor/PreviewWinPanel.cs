using UnityEngine;
using UnityEditor;

public class PreviewWinPanel
{
    [MenuItem("Tools/Preview Win Panel")]
    public static string Execute()
    {
        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager == null) return "UIManager não encontrado!";

        SerializedObject so = new SerializedObject(uiManager);
        GameObject winPanel = so.FindProperty("_winPanel").objectReferenceValue as GameObject;
        if (winPanel == null) return "WinPanel não encontrado!";

        // Toggle ativo/inativo
        winPanel.SetActive(!winPanel.activeSelf);
        return winPanel.activeSelf ? "WinPanel ATIVADO (preview)" : "WinPanel DESATIVADO";
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class FixWinPanel
{
    [MenuItem("Tools/Fix Win Panel")]
    public static string Execute()
    {
        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager == null) return "UIManager não encontrado!";

        SerializedObject so = new SerializedObject(uiManager);
        GameObject winPanel = so.FindProperty("_winPanel").objectReferenceValue as GameObject;
        if (winPanel == null) return "WinPanel não encontrado!";

        bool wasActive = winPanel.activeSelf;
        winPanel.SetActive(true);

        // Remove o VerticalLayoutGroup — ele está causando problemas de posicionamento
        VerticalLayoutGroup vlg = winPanel.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) Object.DestroyImmediate(vlg);

        // Remove LayoutElements dos filhos
        foreach (Transform child in winPanel.transform)
        {
            LayoutElement le = child.GetComponent<LayoutElement>();
            if (le != null) Object.DestroyImmediate(le);
        }

        // Garante que o WinPanel cobre a tela inteira (stretch)
        RectTransform panelRect = winPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Garante que tem Image semi-transparente
        Image bg = winPanel.GetComponent<Image>();
        if (bg == null) bg = winPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);

        // --- Posiciona filhos manualmente com âncoras centrais ---

        // WinTitle — centro, um pouco acima
        SetupText(winPanel.transform.Find("WinTitle"),
            "Todas as moedas coletadas!", 40,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 60f), new Vector2(600f, 60f), Color.white);

        // WinSubtitle — centro
        SetupText(winPanel.transform.Find("WinSubtitle"),
            "Parabéns! Passos: 0", 30,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f), new Vector2(500f, 45f), Color.white);

        // Remove Spacer antigo (não precisamos mais)
        Transform spacer = winPanel.transform.Find("Spacer");
        if (spacer != null) Object.DestroyImmediate(spacer.gameObject);

        // RestartButton — centro, abaixo
        Transform btn = winPanel.transform.Find("RestartButton");
        if (btn != null)
        {
            RectTransform btnRect = btn.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(0f, -80f);
            btnRect.sizeDelta = new Vector2(280f, 55f);

            // Garante que tem texto
            Transform textChild = btn.Find("Text");
            if (textChild != null)
            {
                RectTransform textRect = textChild.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }
        }

        winPanel.SetActive(wasActive);
        EditorUtility.SetDirty(winPanel);
        return "WinPanel reconstruído com layout manual!";
    }

    private static void SetupText(Transform t, string text, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        if (t == null) return;
        RectTransform rect = t.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        TextMeshProUGUI tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }
}

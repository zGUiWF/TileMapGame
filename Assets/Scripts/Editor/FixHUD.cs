using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class FixHUD
{
    private static readonly Color BG_COLOR = HexToColor("2C3E50");

    [MenuItem("Tools/Fix HUD")]
    public static string Execute()
    {
        GameObject canvas = GameObject.Find("GameCanvas");
        if (canvas == null) return "GameCanvas não encontrado!";

        // --- Remove HintPanel antigo se existir ---
        Transform hintPanel = canvas.transform.Find("HintPanel");
        if (hintPanel != null) Object.DestroyImmediate(hintPanel.gameObject);

        // --- Remove HintText solto se existir ---
        Transform hintTextOld = canvas.transform.Find("HintText");
        if (hintTextOld != null) Object.DestroyImmediate(hintTextOld.gameObject);

        // --- HUD: painel lateral à direita (20% da tela) ---
        GameObject hud = canvas.transform.Find("HUD")?.gameObject;
        if (hud == null) return "HUD não encontrado!";

        RectTransform hudRect = hud.GetComponent<RectTransform>();
        // Ancora no lado direito, ocupa 20% da largura e toda a altura
        hudRect.anchorMin = new Vector2(0.80f, 0f);
        hudRect.anchorMax = new Vector2(1f, 1f);
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;

        // Fundo sólido
        Image hudBg = hud.GetComponent<Image>();
        if (hudBg == null) hudBg = hud.AddComponent<Image>();
        hudBg.color = BG_COLOR;

        // Layout vertical centralizado
        VerticalLayoutGroup vlg = hud.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
        {
            vlg.padding = new RectOffset(15, 15, 30, 30);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 15f;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
        }

        // Textos
        SetTextStyle(hud.transform.Find("CoinsText"), 22, FontStyles.Bold, Color.white, 40f);
        SetTextStyle(hud.transform.Find("StepsText"), 20, FontStyles.Normal, new Color(0.85f, 0.85f, 0.85f), 35f);

        // --- Adiciona dica de controles dentro do HUD ---
        Transform existingHint = hud.transform.Find("HintText");
        if (existingHint == null)
        {
            GameObject hintObj = new GameObject("HintText");
            hintObj.transform.SetParent(hud.transform, false);

            RectTransform hintRect = hintObj.AddComponent<RectTransform>();
            hintRect.sizeDelta = new Vector2(0f, 50f);

            TextMeshProUGUI hintTmp = hintObj.AddComponent<TextMeshProUGUI>();
            hintTmp.text = "Setas / WASD\npara mover";
            hintTmp.fontSize = 16;
            hintTmp.color = new Color(0.6f, 0.6f, 0.6f);
            hintTmp.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            var hintTmp = existingHint.GetComponent<TextMeshProUGUI>();
            if (hintTmp != null)
            {
                hintTmp.text = "Setas / WASD\npara mover";
                hintTmp.fontSize = 16;
                hintTmp.color = new Color(0.6f, 0.6f, 0.6f);
                hintTmp.alignment = TextAlignmentOptions.Center;
            }
            RectTransform hintRect = existingHint.GetComponent<RectTransform>();
            hintRect.sizeDelta = new Vector2(0f, 50f);
        }

        EditorUtility.SetDirty(canvas);
        return "HUD movido para painel lateral direito!";
    }

    private static void SetTextStyle(Transform t, int fontSize, FontStyles style, Color color, float height)
    {
        if (t == null) return;
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rect = t.GetComponent<RectTransform>();
        if (rect != null) rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
    }

    private static Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }
}

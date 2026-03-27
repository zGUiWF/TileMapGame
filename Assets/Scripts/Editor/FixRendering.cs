using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class FixRendering
{
    [MenuItem("Tools/Fix Rendering (Unlit)")]
    public static string Execute()
    {
        string result = "";

        // Encontra o shader Unlit para sprites 2D no URP
        Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (unlitShader == null)
        {
            // Fallback
            unlitShader = Shader.Find("Sprites/Default");
        }

        if (unlitShader == null)
        {
            return "ERROR: Nenhum shader unlit encontrado!";
        }

        Material unlitMaterial = new Material(unlitShader);

        // Aplica nos TilemapRenderers da cena
        var tilemapRenderers = Object.FindObjectsByType<TilemapRenderer>(FindObjectsSortMode.None);
        foreach (var r in tilemapRenderers)
        {
            r.material = unlitMaterial;
            result += $"TilemapRenderer '{r.gameObject.name}' -> {unlitShader.name}\n";
        }

        // Aplica nos SpriteRenderers da cena
        var spriteRenderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (var r in spriteRenderers)
        {
            r.material = unlitMaterial;
            result += $"SpriteRenderer '{r.gameObject.name}' -> {unlitShader.name}\n";
        }

        // Também corrige os prefabs
        string[] prefabPaths = { "Assets/Prefabs/Player.prefab", "Assets/Prefabs/Coin.prefab" };
        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                var sr = prefab.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sharedMaterial = unlitMaterial;
                    EditorUtility.SetDirty(prefab);
                    result += $"Prefab '{path}' -> {unlitShader.name}\n";
                }
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(Object.FindObjectsByType<TilemapRenderer>(FindObjectsSortMode.None)[0]);

        result += "Rendering fix applied!";
        return result;
    }
}

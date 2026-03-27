using UnityEngine;
using UnityEditor;

public class DiagnosticScript
{
    [MenuItem("Tools/Run Diagnostic")]
    public static string Execute()
    {
        string result = "";

        // Verificar sorting layers
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );
        SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");

        result += "SORTING_LAYERS: ";
        for (int i = 0; i < sortingLayers.arraySize; i++)
        {
            var layer = sortingLayers.GetArrayElementAtIndex(i);
            string name = layer.FindPropertyRelative("name").stringValue;
            result += $"'{name}', ";
        }

        // Verificar TilemapRenderers
        var renderers = Object.FindObjectsByType<UnityEngine.Tilemaps.TilemapRenderer>(FindObjectsSortMode.None);
        result += $"\nTILEMAP_RENDERERS({renderers.Length}): ";
        foreach (var r in renderers)
        {
            result += $"{r.gameObject.name}(layer='{r.sortingLayerName}' mat='{r.material?.shader?.name}'), ";
        }

        // Verificar câmera
        var cam = Camera.main;
        if (cam != null)
        {
            result += $"\nCAMERA: pos={cam.transform.position} ortho={cam.orthographic} size={cam.orthographicSize} flags={cam.clearFlags}";
        }

        return result;
    }
}

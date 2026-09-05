using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Punto de entrada para validación por línea de comandos:
/// Unity.exe -batchmode -quit -projectPath ... -executeMethod LoteReparacion.Ejecutar
/// Abre la escena, ejecuta las reparaciones y guarda todo.
/// </summary>
public static class LoteReparacion
{
    public static void Ejecutar()
    {
        Debug.Log("[LOTE] Iniciando reparación por lotes...");

        RepararJefeTool.Reparar();

        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
        ArmarEscenaTool.Armar();

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        Debug.Log("JFF_LOTE_COMPLETO");
    }
}

using UnityEditor;
using UnityEngine;

/// <summary>
/// Reconstruye el prefab del Jefe a partir del escarabajo (serialización 100% de Unity,
/// sin YAML manual) y repara las referencias del SpawnConfig.
/// Menú: JuegoFlash → 1 - Reparar Prefab del Jefe
/// </summary>
public static class RepararJefeTool
{
    const string RutaPrefabJefe = "Assets/_Project/Prefabs/Enemy_Jefe.prefab";
    const string RutaPrefabEsc = "Assets/_Project/Enemy_Escarabajo.prefab";
    const string RutaOrbe = "Assets/_Project/Prefabs/ExpOrbe.prefab";
    const string RutaJefeData = "Assets/_Project/Data/Enemies/JefeData.asset";
    const string RutaSpawnConfig = "Assets/_Project/Data/Spawner/SpawnConfig.asset";
    const string RutaPlayer = "Assets/_Project/Prefabs/Player_Roca.prefab";
    const string RutaPlayerData = "Assets/_Project/Data/Player/PlayerData.asset";
    const string RutaEscData = "Assets/_Project/Data/Enemies/EscarabajoData.asset";

    [MenuItem("JuegoFlash/1 - Reparar Prefab del Jefe")]
    public static void Reparar()
    {
        if (Application.isPlaying) return;

        GameObject escarabajo = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefabEsc);
        GameObject orbe = AssetDatabase.LoadAssetAtPath<GameObject>(RutaOrbe);
        EnemyDataSO jefeData = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(RutaJefeData);

        if (escarabajo == null || jefeData == null)
        {
            Debug.LogError("[RepararJefe] Faltan assets base (escarabajo o JefeData).");
            return;
        }

        // 1) Reconstruir el prefab del jefe
        GameObject root = new GameObject("Enemy_Jefe");
        root.layer = escarabajo.layer;

        SpriteRenderer srEsc = escarabajo.GetComponent<SpriteRenderer>();
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = srEsc != null ? srEsc.sprite : null;
        sr.color = new Color(1f, 0.55f, 0.55f, 1f);
        sr.sortingOrder = 1;

        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.mass = 3f;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D col = root.AddComponent<CircleCollider2D>();
        col.radius = 0.45f;

        EnemyBoss boss = root.AddComponent<EnemyBoss>();
        SerializedObject so = new SerializedObject(boss);
        SetObj(so, "datosEnemigo", jefeData);
        SetFloat(so, "velocidadBase", 2.6f);
        SetFloat(so, "variacionVelocidad", 0.3f);
        SetInt(so, "vida", 12);
        SetInt(so, "danio", 1);
        SetFloat(so, "cooldownDanio", 0.6f);
        SetFloat(so, "radioDeteccion", 100f);
        SetFloat(so, "radioSeparacion", 1.4f);
        SetFloat(so, "fuerzaSeparacion", 4f);
        SetFloat(so, "duracionKnockback", 0.18f);
        SetFloat(so, "tiempoFlash", 0.08f);
        SetInt(so, "expTotal", 12);
        SetInt(so, "expPorOrbe", 1);
        SetObj(so, "prefabOrbeExp", orbe);
        SetFloat(so, "intervaloEmbestida", 5f);
        SetFloat(so, "tiempoApuntado", 0.7f);
        SetFloat(so, "velocidadEmbestida", 13f);
        SetFloat(so, "maxDuracionEmbestida", 1.1f);
        SetFloat(so, "descansoTrasEmbestida", 0.9f);
        SetColor(so, "colorApuntado", new Color(1f, 0.35f, 0.3f, 1f));
        SetObj(so, "prefabMini", escarabajo);
        SetInt(so, "cantidadMinis", 3);
        SetFloat(so, "umbralVidaInvocacion", 0.5f);
        so.ApplyModifiedPropertiesWithoutUndo();

        root.transform.localScale = Vector3.one * 1.8f;
        PrefabUtility.SaveAsPrefabAsset(root, RutaPrefabJefe);
        Object.DestroyImmediate(root);
        Debug.Log("[RepararJefe] Prefab del jefe reconstruido correctamente.");

        // 2) Reparar referencias del SpawnConfig
        SpawnConfigSO cfg = AssetDatabase.LoadAssetAtPath<SpawnConfigSO>(RutaSpawnConfig);
        if (cfg != null)
        {
            SerializedObject soCfg = new SerializedObject(cfg);
            SetObj(soCfg, "prefabEscarabajo", escarabajo);
            SetObj(soCfg, "prefabJefe", AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefabJefe));
            SetObj(soCfg, "datosJefe", jefeData);
            soCfg.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets();
            Debug.Log("[RepararJefe] SpawnConfig reparado (prefabs + datosJefe).");
        }

        // 3) Reparar referencias de datos en los prefabs del jugador y escarabajo
        RepararDatosPrefab(RutaPlayer, "datosBase", RutaPlayerData, "Player_Roca");
        RepararDatosPrefab(RutaPrefabEsc, "datosEnemigo", RutaEscData, "Enemy_Escarabajo");
    }

    static void RepararDatosPrefab(string rutaPrefab, string propiedad, string rutaDato, string etiqueta)
    {
        ScriptableObject dato = AssetDatabase.LoadAssetAtPath<ScriptableObject>(rutaDato);
        if (dato == null)
        {
            Debug.LogWarning("[RepararJefe] No se encontró " + rutaDato);
            return;
        }

        GameObject contenido = PrefabUtility.LoadPrefabContents(rutaPrefab);
        try
        {
            Component comp = propiedad == "datosBase"
                ? (Component)contenido.GetComponent<PlayerRuntimeStats>()
                : (Component)contenido.GetComponent<Enemy2D>();
            if (comp == null)
            {
                Debug.LogWarning("[RepararJefe] Sin componente esperado en " + etiqueta);
                return;
            }

            SerializedObject so = new SerializedObject(comp);
            SerializedProperty p = so.FindProperty(propiedad);
            if (p != null) p.objectReferenceValue = dato;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(contenido, rutaPrefab);
            Debug.Log("[RepararJefe] " + etiqueta + ": " + propiedad + " verificado.");
        }
        finally
        {
            Object.DestroyImmediate(contenido);
        }
    }

    static void SetObj(SerializedObject so, string prop, Object valor)
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p != null) p.objectReferenceValue = valor;
        else Debug.LogWarning("[RepararJefe] Propiedad no encontrada: " + prop);
    }

    static void SetFloat(SerializedObject so, string prop, float v)
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p != null) p.floatValue = v;
    }

    static void SetInt(SerializedObject so, string prop, int v)
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p != null) p.intValue = v;
    }

    static void SetColor(SerializedObject so, string prop, Color c)
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p != null) p.colorValue = c;
    }
}

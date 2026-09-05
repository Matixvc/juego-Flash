using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Arma la escena completa de la arena: fondo, bordes con colliders, cámara fija,
/// GameManager (con datos asignados), UpgradeManager (con pool) y la Roca.
/// Menú: JuegoFlash → 2 - Armar Escena. Se ejecuta solo la primera vez tras compilar.
/// </summary>
public static class ArmarEscenaTool
{
    const string RutaPlayer = "Assets/_Project/Prefabs/Player_Roca.prefab";
    const string RutaRunProgress = "Assets/_Project/Data/Run/RunProgress.asset";
    const string RutaSpawnConfig = "Assets/_Project/Data/Spawner/SpawnConfig.asset";
    const string RutaPool = "Assets/_Project/Data/Upgrades/UpgradePool.asset";
    const string RutaBg = "Assets/_Project/Sprite/BackGround.png";
    const string RutaArbusto = "Assets/_Project/Sprite/Arbusto.jpeg";
    const string RutaPlayerData = "Assets/_Project/Data/Player/PlayerData.asset";

    const float AnchoArena = 44f;   // ancho jugable interior (muros en ±23)
    const float AltoArena = 26f;    // alto jugable interior (muros en ±14)

    [MenuItem("JuegoFlash/2 - Armar Escena")]
    public static void Armar()
    {
        if (Application.isPlaying) return;

        UnityEngine.SceneManagement.Scene escena = EditorSceneManager.GetActiveScene();
        if (!escena.IsValid())
        {
            Debug.LogError("[ArmarEscena] No hay escena activa.");
            return;
        }

        // Cámara con seguimiento: más cerca, sigue a la Roca por toda la arena
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.backgroundColor = new Color(0.10f, 0.14f, 0.10f);

            CamaraTremor.InstalarEn(cam);
            CamaraSeguimiento seguimiento = cam.GetComponent<CamaraSeguimiento>();
            if (seguimiento == null) seguimiento = cam.gameObject.AddComponent<CamaraSeguimiento>();
            SerializedObject soCam = new SerializedObject(seguimiento);
            SerializedProperty pMitad = soCam.FindProperty("mitadArena");
            if (pMitad != null) pMitad.vector2Value = new Vector2(AnchoArena * 0.5f, AltoArena * 0.5f);
            soCam.ApplyModifiedPropertiesWithoutUndo();
        }

        // El mapa creció: los arbustos sueltos viejos quedarían flotando adentro
        foreach (string arbustoViejo in new string[] { "ArbustoArriba", "ArbustoAbajo", "ArbustoIzq", "ArbustoDer" })
        {
            GameObject sobrante = GameObject.Find(arbustoViejo);
            if (sobrante != null) Object.DestroyImmediate(sobrante);
        }

        // El suelo decorativo va detrás de todo lo demás
        GameObject suelo = GameObject.Find("Ground");
        if (suelo != null)
        {
            SpriteRenderer srSuelo = suelo.GetComponent<SpriteRenderer>();
            if (srSuelo != null) srSuelo.sortingOrder = -6;
        }

        // Fondo estático
        GameObject fondo = Obtener("Fondo");
        Sprite spriteBg = CargarSprite(RutaBg);
        if (spriteBg != null)
        {
            SpriteRenderer sr = ObtenerComp<SpriteRenderer>(fondo);
            sr.sprite = spriteBg;
            sr.sortingOrder = -10;
            Vector3 ext = spriteBg.bounds.extents;
            fondo.transform.localScale = new Vector3(
                (AnchoArena + 4f) / (ext.x * 2f),
                (AltoArena + 4f) / (ext.y * 2f), 1f);
            fondo.transform.position = Vector3.zero;
        }

        // Bordes con arbusto visual + collider
        // Bordes alineados con los arbustos del usuario (±11 vertical / ±17 horizontal)
        CrearBorde("Borde_Arriba", new Vector3(0f, AltoArena * 0.5f + 1f, 0f), new Vector2(AnchoArena + 4f, 2f));
        CrearBorde("Borde_Abajo", new Vector3(0f, -AltoArena * 0.5f - 1f, 0f), new Vector2(AnchoArena + 4f, 2f));
        CrearBorde("Borde_Izquierda", new Vector3(-AnchoArena * 0.5f - 1f, 0f, 0f), new Vector2(2f, AltoArena + 4f));
        CrearBorde("Borde_Derecha", new Vector3(AnchoArena * 0.5f + 1f, 0f, 0f), new Vector2(2f, AltoArena + 4f));

        // GameManager con sus datos
        GameObject gmGo = Obtener("GameManager");
        GameManager gm = ObtenerComp<GameManager>(gmGo);
        SerializedObject soGm = new SerializedObject(gm);
        SetObj(soGm, "runProgress", AssetDatabase.LoadAssetAtPath<ScriptableObject>(RutaRunProgress));
        SetObj(soGm, "spawnConfig", AssetDatabase.LoadAssetAtPath<ScriptableObject>(RutaSpawnConfig));
        soGm.ApplyModifiedPropertiesWithoutUndo();

        // UpgradeManager con el pool
        GameObject umGo = Obtener("UpgradeManager");
        UpgradeManager um = ObtenerComp<UpgradeManager>(umGo);
        SerializedObject soUm = new SerializedObject(um);
        SetObj(soUm, "pool", AssetDatabase.LoadAssetAtPath<ScriptableObject>(RutaPool));
        soUm.ApplyModifiedPropertiesWithoutUndo();

        // Jugador (instancia del prefab)
        if (Object.FindAnyObjectByType<PlayerController2D>() == null)
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPlayer);
            if (playerPrefab != null)
            {
                PrefabUtility.InstantiatePrefab(playerPrefab, escena);
            }
            else
            {
                Debug.LogError("[ArmarEscena] Falta Player_Roca.prefab.");
            }
        }

        // Asegurar datosBase del jugador que esté en la escena (aunque sea manual)
        PlayerController2D jugadorEscena = Object.FindAnyObjectByType<PlayerController2D>();
        if (jugadorEscena != null)
        {
            PlayerRuntimeStats statsEscena = jugadorEscena.GetComponent<PlayerRuntimeStats>();
            PlayerDataSO datos = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(RutaPlayerData);
            if (statsEscena != null && datos != null)
            {
                SerializedObject soStats = new SerializedObject(statsEscena);
                SerializedProperty pDb = soStats.FindProperty("datosBase");
                if (pDb != null && pDb.objectReferenceValue == null) pDb.objectReferenceValue = datos;
                soStats.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // Arena en el SpawnConfig (para los spawns del spawner)
        SpawnConfigSO cfg = AssetDatabase.LoadAssetAtPath<SpawnConfigSO>(RutaSpawnConfig);
        if (cfg != null)
        {
            SerializedObject soCfg = new SerializedObject(cfg);
            SerializedProperty pCentro = soCfg.FindProperty("centroArena");
            SerializedProperty pTam = soCfg.FindProperty("tamanoArena");
            if (pCentro != null) pCentro.vector2Value = Vector2.zero;
            if (pTam != null) pTam.vector2Value = new Vector2(42f, 24f);
            soCfg.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cfg);
        }

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("[ArmarEscena] Escena armada y guardada: fondo, bordes, GameManager, UpgradeManager y Roca.");
    }

    /// <summary>Se ejecuta una vez por sesión de Unity tras recompilar los scripts.</summary>
    [InitializeOnLoadMethod]
    static void AutoEjecutarUnaVez()
    {
        if (SessionState.GetBool("JFF_ReparacionAuto_v2", false)) return;
        if (Application.isBatchMode) return; // en lote lo maneja LoteReparacion
        SessionState.SetBool("JFF_ReparacionAuto_v2", true);

        EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;
            RepararJefeTool.Reparar();
            Armar();
        };
    }

    static GameObject Obtener(string nombre)
    {
        GameObject go = GameObject.Find(nombre);
        return go != null ? go : new GameObject(nombre);
    }

    static T ObtenerComp<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    static Sprite CargarSprite(string ruta)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(ruta);
        foreach (Object a in assets)
        {
            if (a is Sprite s) return s;
        }
        Debug.LogWarning("[ArmarEscena] Sprite no encontrado: " + ruta);
        return null;
    }

    static void CrearBorde(string nombre, Vector3 pos, Vector2 tamano)
    {
        GameObject go = Obtener(nombre);
        go.transform.position = pos;
        go.isStatic = true;

        BoxCollider2D col = ObtenerComp<BoxCollider2D>(go);
        col.size = tamano;
        col.offset = Vector2.zero;

        Sprite arbusto = CargarSprite(RutaArbusto);
        if (arbusto != null)
        {
            SpriteRenderer sr = ObtenerComp<SpriteRenderer>(go);
            sr.sprite = arbusto;
            sr.sortingOrder = -5;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = tamano;
        }
    }

    static void SetObj(SerializedObject so, string prop, Object valor)
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p != null) p.objectReferenceValue = valor;
        else Debug.LogWarning("[ArmarEscena] Propiedad no encontrada: " + prop);
    }
}


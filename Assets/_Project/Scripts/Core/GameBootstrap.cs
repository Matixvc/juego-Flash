using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bootstrap de runtime: se ejecuta al cargar la escena y asegura que
/// todos los sistemas críticos estén presentes y configurados.
/// No depende de herramientas de editor — funciona al darle Play.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instancia { get; private set; }

    [Header("Datos (se asignan desde Resources si están vacíos)")]
    [SerializeField] private RunProgressSO runProgress;
    [SerializeField] private SpawnConfigSO spawnConfig;
    [SerializeField] private UpgradePoolSO upgradePool;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;

        AsegurarDatos();
        AsegurarManagers();
        AsegurarCanvasYHUD();
        ConfigurarCamara();
    }

    private void AsegurarDatos()
    {
        if (runProgress == null)
        {
            runProgress = Resources.Load<RunProgressSO>("RunProgress");
        }

        if (spawnConfig == null)
        {
            spawnConfig = Resources.Load<SpawnConfigSO>("SpawnConfig");
        }

        if (upgradePool == null)
        {
            upgradePool = Resources.Load<UpgradePoolSO>("UpgradePool");
        }
    }

    private void AsegurarManagers()
    {
        GameManager gm = FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject goGM = new GameObject("_GameManager");
            gm = goGM.AddComponent<GameManager>();
        }

        var dataLink = gm.gameObject.GetComponent<BootstrapDataLink>();
        if (dataLink == null)
        {
            dataLink = gm.gameObject.AddComponent<BootstrapDataLink>();
        }
        dataLink.runProgress = runProgress;
        dataLink.spawnConfig = spawnConfig;
        gm.ConfigurarDatos(runProgress, spawnConfig);

        EnemySpawner2D spawner = FindAnyObjectByType<EnemySpawner2D>();
        if (spawner == null)
        {
            GameObject goSpawner = new GameObject("_EnemySpawner");
            spawner = goSpawner.AddComponent<EnemySpawner2D>();
        }

        UpgradeManager upgradeMgr = FindAnyObjectByType<UpgradeManager>();
        if (upgradeMgr == null)
        {
            GameObject goUpgrade = new GameObject("_UpgradeManager");
            upgradeMgr = goUpgrade.AddComponent<UpgradeManager>();
        }

        var upgradeLink = upgradeMgr.gameObject.GetComponent<BootstrapUpgradeLink>();
        if (upgradeLink == null)
        {
            upgradeLink = upgradeMgr.gameObject.AddComponent<BootstrapUpgradeLink>();
        }
        upgradeLink.pool = upgradePool;
        upgradeMgr.ConfigurarPool(upgradePool);
    }

    private void AsegurarCanvasYHUD()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("_Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        HUDManager hud = FindAnyObjectByType<HUDManager>();
        if (hud == null)
        {
            GameObject hudGO = new GameObject("_HUDManager");
            hudGO.transform.SetParent(canvas.transform, false);
            hud = hudGO.AddComponent<HUDManager>();
        }
    }

    private void ConfigurarCamara()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        var tremors = cam.GetComponents<CamaraTremor>();
        for (int i = 1; i < tremors.Length; i++)
        {
            Destroy(tremors[i]);
        }

        if (cam.GetComponent<CamaraTremor>() == null)
        {
            CamaraTremor.InstalarEn(cam);
        }

        if (cam.GetComponent<CamaraSeguimiento>() == null)
        {
            cam.gameObject.AddComponent<CamaraSeguimiento>();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInicializar()
    {
        if (FindAnyObjectByType<GameBootstrap>() != null) return;

        GameObject go = new GameObject("_GameBootstrap");
        go.AddComponent<GameBootstrap>();
        Object.DontDestroyOnLoad(go);
    }
}

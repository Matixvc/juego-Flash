using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona mejoras del roguelike. Auto-asigna pool y crea panel UI.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private UpgradePoolSO pool;
    [SerializeField] private GameObject panelEleccion;

    private HashSet<UpgradeSO> mejorasTomadas = new HashSet<UpgradeSO>();
    private UpgradeSO[] ofertaActual;
    private bool esperandoEleccion;

    public void ConfigurarPool(UpgradePoolSO poolConfigurado)
    {
        if (poolConfigurado != null) pool = poolConfigurado;
    }

    private void Awake()
    {
        if (pool == null) pool = Resources.Load<UpgradePoolSO>("UpgradePool");
    }

    private bool _suscrito;

    private void Start()
    {
        if (panelEleccion == null) CrearPanelUpgrade();
        IntentarSuscribir();
    }

    private void Update()
    {
        if (!_suscrito) IntentarSuscribir();
    }

    private void IntentarSuscribir()
    {
        if (GameManager.Instancia != null && GameManager.Instancia.RunProgress != null)
        {
            GameManager.Instancia.RunProgress.AlSubirNivel += OnSubirNivel;
            _suscrito = true;
        }
    }

    private void CrearPanelUpgrade()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject cgo = new GameObject("_Canvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
        }

        panelEleccion = new GameObject("_PanelUpgrade");
        panelEleccion.transform.SetParent(canvas.transform, false);
        panelEleccion.SetActive(false);

        Image fondo = panelEleccion.AddComponent<Image>();
        fondo.color = new Color(0.05f, 0.05f, 0.08f, 0.92f);
        fondo.sprite = RecursosUI.SpriteBlanco;
        RectTransform rtF = panelEleccion.GetComponent<RectTransform>();
        rtF.anchorMin = Vector2.zero; rtF.anchorMax = Vector2.one;
        rtF.offsetMin = Vector2.zero; rtF.offsetMax = Vector2.zero;

        // Titulo
        GameObject titGO = new GameObject("Titulo", typeof(RectTransform));
        titGO.transform.SetParent(panelEleccion.transform, false);
        Text tit = titGO.AddComponent<Text>();
        tit.text = "!SUBISTE DE NIVEL!";
        tit.fontSize = 32; tit.fontStyle = FontStyle.Bold;
        tit.font = RecursosUI.FuenteUI;
        tit.color = new Color(1f, 0.85f, 0.3f, 1f);
        tit.alignment = TextAnchor.MiddleCenter;
        RectTransform rtTit = tit.GetComponent<RectTransform>();
        rtTit.anchorMin = new Vector2(0.5f, 1f); rtTit.anchorMax = new Vector2(0.5f, 1f);
        rtTit.pivot = new Vector2(0.5f, 1f);
        rtTit.anchoredPosition = new Vector2(0f, -60f);
        rtTit.sizeDelta = new Vector2(600f, 50f);

        // Contenedor cartas
        GameObject contGO = new GameObject("ContenedorCartas", typeof(RectTransform));
        contGO.transform.SetParent(panelEleccion.transform, false);
        HorizontalLayoutGroup hlg = contGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20f; hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
        RectTransform rtC = contGO.GetComponent<RectTransform>();
        rtC.anchorMin = new Vector2(0.5f, 0.5f); rtC.anchorMax = new Vector2(0.5f, 0.5f);
        rtC.pivot = new Vector2(0.5f, 0.5f); rtC.anchoredPosition = Vector2.zero;
        rtC.sizeDelta = new Vector2(900f, 300f);

        // Instruction text
        GameObject instGO = new GameObject("Instruccion", typeof(RectTransform));
        instGO.transform.SetParent(panelEleccion.transform, false);
        Text inst = instGO.AddComponent<Text>();
        inst.text = "Haz clic en una mejora para seleccionarla";
        inst.fontSize = 14;
        inst.font = RecursosUI.FuenteUI;
        inst.color = new Color(0.6f, 0.6f, 0.7f, 1f);
        inst.alignment = TextAnchor.MiddleCenter;
        RectTransform rtInst = inst.GetComponent<RectTransform>();
        rtInst.anchorMin = new Vector2(0.5f, 0f); rtInst.anchorMax = new Vector2(0.5f, 0f);
        rtInst.pivot = new Vector2(0.5f, 0f);
        rtInst.anchoredPosition = new Vector2(0f, 30f);
        rtInst.sizeDelta = new Vector2(500f, 25f);

        if (panelEleccion.GetComponent<UpgradeUI>() == null)
            panelEleccion.AddComponent<UpgradeUI>();
    }

    private void OnSubirNivel(int nuevoNivel, int expRequerida)
    {
        if (pool == null)
        {
            Debug.LogWarning("[UpgradeManager] Pool nulo.");
            return;
        }

        List<UpgradeSO> elegidas = pool.ElegirMejoras(pool.CartasOfrecidas, mejorasTomadas);
        if (elegidas.Count == 0)
        {
            Debug.LogWarning("[UpgradeManager] Sin mejoras disponibles.");
            return;
        }

        ofertaActual = elegidas.ToArray();
        esperandoEleccion = true;

        if (GameManager.Instancia != null)
            GameManager.Instancia.CambiarEstado(GameManager.EstadoJuego.SeleccionMejora);

        if (panelEleccion != null)
        {
            panelEleccion.SetActive(true);
            var ui = panelEleccion.GetComponent<IUpgradeSelector>();
            if (ui != null) ui.MostrarOpciones(ofertaActual);
        }
    }

    public void ElegirMejora(UpgradeSO mejora)
    {
        if (!esperandoEleccion || mejora == null) return;
        esperandoEleccion = false;

        PlayerController2D jugador = GameManager.Instancia != null ? GameManager.Instancia.Jugador : null;
        if (jugador != null)
        {
            PlayerRuntimeStats stats = jugador.GetComponent<PlayerRuntimeStats>();
            if (stats != null)
            {
                foreach (UpgradeEffect efecto in mejora.Efectos)
                    stats.AplicarModificador(efecto.stat, efecto.valor);
                if (mejora.EsLeech) stats.ActivarLeech();
            }
            jugador.NotificarCambioVida();
        }

        mejorasTomadas.Add(mejora);

        if (panelEleccion != null) panelEleccion.SetActive(false);

        if (GameManager.Instancia != null)
            GameManager.Instancia.CambiarEstado(GameManager.EstadoJuego.Jugando);
    }

    private void OnDestroy()
    {
        if (_suscrito && GameManager.Instancia != null && GameManager.Instancia.RunProgress != null)
            GameManager.Instancia.RunProgress.AlSubirNivel -= OnSubirNivel;
    }
}

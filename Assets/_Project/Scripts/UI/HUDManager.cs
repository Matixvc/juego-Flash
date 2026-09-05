using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// HUD completo creado 100% desde codigo. Muestra barra de vida, XP, esencia,
/// timer de jefe, tier, pantalla de game over y menu de pausa.
/// </summary>
public class HUDManager : MonoBehaviour
{
    private RectTransform raiz;
    private PlayerController2D jugador;
    private RunProgressSO runProgress;

    private Image rellenoVida;
    private Text textoVida;
    private Image rellenoXP;
    private Text textoNivel;
    private Text textoEsencia;
    private GameObject barraJefe;
    private Image rellenoJefe;
    private Text textoTier;
    private GameObject pantallaGameOver;
    private GameObject menuPausa;

    private void Awake()
    {
        raiz = UIHelper.CrearRaiz(gameObject);
        CrearBarraVida();
        CrearBarraXP();
        CrearContadorEsencia();
        CrearBarraJefe();
        CrearIndicadorTier();
        CrearPantallaGameOver();
        CrearMenuPausa();

        pantallaGameOver.SetActive(false);
        menuPausa.SetActive(false);
        barraJefe.SetActive(false);
    }

    private void Start()
    {
        jugador = FindAnyObjectByType<PlayerController2D>();
        runProgress = GameManager.Instancia != null ? GameManager.Instancia.RunProgress : null;

        if (jugador != null)
        {
            jugador.AlCambiarVida += ActualizarVida;
            jugador.AlMorir += MostrarGameOver;
        }

        if (runProgress != null)
        {
            runProgress.AlCambiarExp += ActualizarXP;
            runProgress.AlSubirNivel += OnSubirNivel;
            runProgress.AlSumarEsencia += ActualizarEsencia;
        }

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.AlCambiarEstado += OnCambiarEstado;
            GameManager.Instancia.AlIniciarEventoJefe += OnJefeIniciado;
        }

        if (jugador != null) ActualizarVida(jugador.VidaActual, jugador.VidaMaxima);
        if (runProgress != null)
        {
            ActualizarXP();
            ActualizarEsencia(runProgress.EsenciaDeCristal);
            ActualizarTier(runProgress.TierDificultad);
        }
    }

    private void Update()
    {
        if (barraJefe != null && barraJefe.activeSelf && rellenoJefe != null && GameManager.Instancia != null)
        {
            rellenoJefe.fillAmount = GameManager.Instancia.ProgresoProximoJefe;
        }
    }

    private void CrearBarraVida()
    {
        GameObject barra = UIHelper.CrearPanel(raiz, "BarraVida",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(20f, -20f), new Vector2(220f, 26f),
            new Color(0.15f, 0.15f, 0.15f, 0.85f));

        rellenoVida = UIHelper.CrearRelleno(barra, "Relleno",
            new Color(0.9f, 0.25f, 0.2f, 1f), 1f);

        textoVida = UIHelper.CrearTexto(barra.transform, "TextoHP", "3 / 3", 14, FontStyle.Normal,
            Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 0f), new Vector2(1f, 1f));

        UIHelper.AgregarBorde(barra, new Color(0f, 0f, 0f, 0.5f));
    }

    private void CrearBarraXP()
    {
        GameObject barra = UIHelper.CrearPanel(raiz, "BarraXP",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(20f, -50f), new Vector2(180f, 14f),
            new Color(0.12f, 0.1f, 0.18f, 0.8f));

        rellenoXP = UIHelper.CrearRelleno(barra, "Relleno",
            new Color(1f, 0.85f, 0.25f, 1f), 0f);

        textoNivel = UIHelper.CrearTexto(raiz, "Nivel", "Nivel 1", 16, FontStyle.Bold,
            new Color(1f, 0.85f, 0.25f, 1f), TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(210f, -48f), new Vector2(80f, 20f));
    }

    private void CrearContadorEsencia()
    {
        GameObject contenedor = UIHelper.CrearPanel(raiz, "Esencia",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-120f, -20f), new Vector2(100f, 28f),
            new Color(0.1f, 0.12f, 0.18f, 0.8f));

        UIHelper.AgregarBorde(contenedor, new Color(0.3f, 0.6f, 1f, 0.6f));

        GameObject iconoGO = new GameObject("Icono", typeof(RectTransform));
        iconoGO.transform.SetParent(contenedor.transform, false);
        Image icono = iconoGO.AddComponent<Image>();
        icono.color = new Color(0.35f, 0.75f, 1f, 1f);
        icono.sprite = UIHelper.SpriteBlanco;
        RectTransform rtI = icono.GetComponent<RectTransform>();
        rtI.anchorMin = new Vector2(0f, 0.5f);
        rtI.anchorMax = new Vector2(0f, 0.5f);
        rtI.pivot = new Vector2(0.5f, 0.5f);
        rtI.anchoredPosition = new Vector2(18f, 0f);
        rtI.sizeDelta = new Vector2(18f, 18f);

        textoEsencia = UIHelper.CrearTexto(contenedor.transform, "Texto", "0", 15, FontStyle.Bold,
            new Color(0.35f, 0.75f, 1f, 1f), TextAnchor.MiddleRight,
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(30f, 2f), new Vector2(-8f, -2f));
    }

    private void CrearBarraJefe()
    {
        barraJefe = UIHelper.CrearPanel(raiz, "BarraJefe",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -30f), new Vector2(300f, 10f),
            new Color(0.2f, 0.05f, 0.05f, 0.9f));

        rellenoJefe = UIHelper.CrearRelleno(barraJefe, "Relleno",
            new Color(1f, 0.3f, 0.2f, 1f), 0f);

        UIHelper.CrearTexto(barraJefe.transform, "Texto", "JEFE", 10, FontStyle.Bold,
            Color.white, TextAnchor.MiddleCenter,
            new Vector2(0f, 0f), new Vector2(1f, 1f));
    }

    private void CrearIndicadorTier()
    {
        GameObject tierGO = UIHelper.CrearPanel(raiz, "Tier",
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(15f, 15f), new Vector2(80f, 24f),
            new Color(0.1f, 0.1f, 0.12f, 0.8f));

        UIHelper.AgregarBorde(tierGO, new Color(0.4f, 0.35f, 0.5f, 0.5f));

        textoTier = UIHelper.CrearTexto(tierGO.transform, "TextoTier", "Tier 0", 12, FontStyle.Bold,
            Color.white, TextAnchor.MiddleCenter,
            new Vector2(0f, 0f), new Vector2(1f, 1f));
    }

    private void CrearPantallaGameOver()
    {
        pantallaGameOver = UIHelper.CrearPanel(raiz, "GameOver",
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero,
            new Color(0.05f, 0.02f, 0.02f, 0.9f));

        UIHelper.CrearTexto(pantallaGameOver.transform, "Titulo", "GAME OVER", 48, FontStyle.Bold,
            new Color(1f, 0.3f, 0.25f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 60f), new Vector2(400f, 60f));

        UIHelper.CrearTexto(pantallaGameOver.transform, "Stats", "Nivel: 1\nKills: 0\nTiempo: 0s",
            18, FontStyle.Normal,
            new Color(0.8f, 0.8f, 0.8f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f), new Vector2(350f, 80f));

        UIHelper.CrearBoton(pantallaGameOver, "BtnReiniciar", "REINICIAR",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -80f), new Vector2(180f, 40f),
            new Color(0.2f, 0.5f, 0.3f, 0.9f), new Color(0.4f, 0.8f, 0.5f, 0.6f),
            ReiniciarJuego);
    }

    private void CrearMenuPausa()
    {
        menuPausa = UIHelper.CrearPanel(raiz, "Pausa",
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero,
            new Color(0.08f, 0.08f, 0.12f, 0.85f));

        UIHelper.CrearTexto(menuPausa.transform, "TituloPausa", "PAUSA", 42, FontStyle.Bold,
            new Color(0.85f, 0.85f, 0.9f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 50f), new Vector2(300f, 50f));

        UIHelper.CrearBoton(menuPausa, "BtnContinuar", "CONTINUAR",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -10f), new Vector2(180f, 36f),
            new Color(0.2f, 0.4f, 0.6f, 0.9f), new Color(0.4f, 0.6f, 0.9f, 0.6f),
            ContinuarJuego);

        UIHelper.CrearBoton(menuPausa, "BtnReiniciarP", "REINICIAR",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -55f), new Vector2(180f, 36f),
            new Color(0.5f, 0.25f, 0.25f, 0.9f), new Color(0.9f, 0.5f, 0.5f, 0.6f),
            ReiniciarJuego);
    }

    private void ActualizarVida(int actual, int max)
    {
        if (textoVida != null) textoVida.text = $"{actual} / {max}";
        if (rellenoVida != null && max > 0) rellenoVida.fillAmount = Mathf.Clamp01((float)actual / max);
    }

    private void ActualizarXP()
    {
        if (runProgress == null) return;
        if (rellenoXP != null) rellenoXP.fillAmount = runProgress.ProgresoExpNivel;
        if (textoNivel != null) textoNivel.text = $"Nivel {runProgress.Nivel}";
    }

    private void OnSubirNivel(int nuevoNivel, int expRequerida)
    {
        if (textoNivel != null) textoNivel.text = $"Nivel {nuevoNivel}";
        if (rellenoXP != null) rellenoXP.fillAmount = 0f;
        HitStop.Golpear(0.06f, 0.08f);
        CamaraTremor.Agregar(0.4f);
    }

    private void ActualizarEsencia(int cantidad)
    {
        if (textoEsencia != null) textoEsencia.text = cantidad.ToString();
    }

    private void ActualizarTier(int tier)
    {
        if (textoTier != null) textoTier.text = $"Tier {tier}";
    }

    private void OnJefeIniciado()
    {
        if (barraJefe != null) barraJefe.SetActive(true);
    }

    private void OnCambiarEstado(GameManager.EstadoJuego estado)
    {
        // La barra del jefe se muestra tanto mientras se juega como durante el evento de jefe
        bool mostrarBarra = estado == GameManager.EstadoJuego.Jugando
                         || estado == GameManager.EstadoJuego.EventoJefe;
        if (barraJefe != null) barraJefe.SetActive(mostrarBarra);

        // Mostrar u ocultar menú de pausa
        if (menuPausa != null)
        {
            menuPausa.SetActive(estado == GameManager.EstadoJuego.Pausa);
        }

        // Mostrar pantalla de Game Over si el estado cambia a GameOver
        if (pantallaGameOver != null)
        {
            if (estado == GameManager.EstadoJuego.GameOver)
            {
                MostrarGameOver();
            }
            else
            {
                pantallaGameOver.SetActive(false);
            }
        }
    }

    private void MostrarGameOver()
    {
        if (pantallaGameOver == null) return;
        pantallaGameOver.SetActive(true);

        Text stats = pantallaGameOver.transform.Find("Stats")?.GetComponent<Text>();
        if (stats != null && runProgress != null)
        {
            float tiempo = runProgress.TiempoDeRun;
            int min = Mathf.FloorToInt(tiempo / 60f);
            int seg = Mathf.FloorToInt(tiempo % 60f);
            stats.text = $"Nivel: {runProgress.Nivel}\n" +
                         $"Kills: {runProgress.EnemigosEliminados}\n" +
                         $"Jefes: {runProgress.JefesDerrotados}\n" +
                         $"Tiempo: {min}m {seg}s";
        }
    }

    private void ContinuarJuego()
    {
        if (GameManager.Instancia != null) GameManager.Instancia.AlternarPausa();
    }

    private void ReiniciarJuego()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (jugador != null)
        {
            jugador.AlCambiarVida -= ActualizarVida;
            jugador.AlMorir -= MostrarGameOver;
        }
        if (runProgress != null)
        {
            runProgress.AlCambiarExp -= ActualizarXP;
            runProgress.AlSubirNivel -= OnSubirNivel;
            runProgress.AlSumarEsencia -= ActualizarEsencia;
        }
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.AlCambiarEstado -= OnCambiarEstado;
            GameManager.Instancia.AlIniciarEventoJefe -= OnJefeIniciado;
        }
    }
}

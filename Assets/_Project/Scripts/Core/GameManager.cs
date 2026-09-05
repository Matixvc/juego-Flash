using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador central del juego: estados, temporizador de eventos de jefe,
/// nivel de dificultad (Tier) y pausa. Coloca una instancia en tu escena y
/// asígnale el RunProgressSO y el SpawnConfigSO. Si no existe, se auto-crea.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum EstadoJuego
    {
        Cargando,
        Jugando,
        Pausa,
        SeleccionMejora,
        EventoJefe,
        GameOver
    }

    public static GameManager Instancia { get; private set; }

    [Header("Datos (ScriptableObjects)")]
    [SerializeField] private RunProgressSO runProgress;
    [SerializeField] private SpawnConfigSO spawnConfig;

    [Header("Jugador")]
    [SerializeField] private Transform puntoSpawnJugador;

    private EstadoJuego estado = EstadoJuego.Cargando;
    private bool pausaDuranteEventoJefe;
    private float temporizadorProximoJefe;
    private float maxTemporizadorJefe;
    private PlayerController2D jugador;

    public EstadoJuego Estado => estado;
    public PlayerController2D Jugador => jugador;
    public RunProgressSO RunProgress => runProgress;
    public SpawnConfigSO SpawnConfig => spawnConfig;

    /// <summary>Progreso 0..1 hacia el próximo evento de jefe (para HUD).</summary>
    public float ProgresoProximoJefe =>
        maxTemporizadorJefe > 0f
            ? 1f - Mathf.Clamp01(temporizadorProximoJefe / maxTemporizadorJefe)
            : 0f;

    public event Action<EstadoJuego> AlCambiarEstado;
    public event Action AlIniciarEventoJefe;

    public void ConfigurarDatos(RunProgressSO progreso, SpawnConfigSO configuracion)
    {
        if (progreso != null) runProgress = progreso;
        if (configuracion != null) spawnConfig = configuracion;
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
    }

    private void Start()
    {
        // Si no se asignaron datos en el Inspector, se crean instancias por defecto
        // para que la run funcione igualmente (puedes reemplazarlas luego).
        if (runProgress == null)
        {
            runProgress = ScriptableObject.CreateInstance<RunProgressSO>();
        }

        if (spawnConfig == null)
        {
            spawnConfig = ScriptableObject.CreateInstance<SpawnConfigSO>();
        }

        runProgress.ResetRun();

        maxTemporizadorJefe = spawnConfig != null ? spawnConfig.TiempoEntreJefes : 270f;
        temporizadorProximoJefe = maxTemporizadorJefe;

        BuscarJugador();

        // VFX: tremor de cámara auto-instalado en la cámara principal
        CamaraTremor.InstalarEn(Camera.main);

        // Cámara con seguimiento de la Roca (para el mapa grande)
        if (Camera.main != null && Camera.main.GetComponent<CamaraSeguimiento>() == null)
        {
            Camera.main.gameObject.AddComponent<CamaraSeguimiento>();
        }

        CambiarEstado(EstadoJuego.Jugando);
    }

    private void Update()
    {
        if (jugador == null)
        {
            BuscarJugador();
        }

        // Pausa con Escape (antes del gate de estado, para poder despausar también)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame &&
            (estado == EstadoJuego.Jugando || estado == EstadoJuego.EventoJefe || estado == EstadoJuego.Pausa))
        {
            AlternarPausa();
            return;
        }

        if (estado != EstadoJuego.Jugando)
        {
            return;
        }

        if (runProgress != null)
        {
            runProgress.SumarTiempo(Time.deltaTime);
        }

        // Cuenta atrás hacia el próximo jefe
        temporizadorProximoJefe -= Time.deltaTime;
        if (temporizadorProximoJefe <= 0f)
        {
            IniciarEventoJefe();
            return;
        }
    }

    public void BuscarJugador()
    {
        if (jugador != null)
        {
            return;
        }

        jugador = FindAnyObjectByType<PlayerController2D>();
        if (jugador != null)
        {
            jugador.AlMorir -= OnJugadorMuerte;
            jugador.AlMorir += OnJugadorMuerte;
        }
    }

    public void CambiarEstado(EstadoJuego nuevoEstado)
    {
        estado = nuevoEstado;
        AlCambiarEstado?.Invoke(nuevoEstado);

        switch (nuevoEstado)
        {
            case EstadoJuego.Jugando:
            case EstadoJuego.EventoJefe:
                Time.timeScale = 1f;
                break;
            default:
                Time.timeScale = 0f;
                break;
        }
    }

    private void IniciarEventoJefe()
    {
        CambiarEstado(EstadoJuego.EventoJefe);
        AlIniciarEventoJefe?.Invoke();
        Debug.Log("[GameManager] ¡Evento de Jefe iniciado!");
    }

    /// <summary>
    /// Debe llamarlo el sistema de jefes cuando el jefe es derrotado:
    /// sube el Tier, reinicia el temporizador y vuelve a oleadas normales.
    /// </summary>
    public void NotificarJefeDerrotado()
    {
        if (runProgress != null)
        {
            runProgress.RegistrarJefeDerrotado();
        }

        temporizadorProximoJefe = maxTemporizadorJefe;
        CambiarEstado(EstadoJuego.Jugando);
        Debug.Log($"[GameManager] Jefe derrotado. Tier actual: {(runProgress != null ? runProgress.TierDificultad : 0)}");
    }

    public void AlternarPausa()
    {
        if (estado == EstadoJuego.Pausa)
        {
            CambiarEstado(pausaDuranteEventoJefe ? EstadoJuego.EventoJefe : EstadoJuego.Jugando);
        }
        else if (estado == EstadoJuego.Jugando || estado == EstadoJuego.EventoJefe)
        {
            pausaDuranteEventoJefe = estado == EstadoJuego.EventoJefe;
            CambiarEstado(EstadoJuego.Pausa);
        }
    }

    private void OnJugadorMuerte()
    {
        if (estado == EstadoJuego.GameOver)
        {
            return;
        }

        CambiarEstado(EstadoJuego.GameOver);
        Debug.Log("[GameManager] Fin de la run.");
    }

    /// <summary>
    /// Garantiza que siempre exista un GameManager aunque la escena no lo tenga.
    /// (Auto-creación legacy: GameBootstrap reemplaza esta lógica en runtime,
    /// pero se mantiene como fallback si GameBootstrap no está presente.)
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCrearSiNoExiste()
    {
        if (FindAnyObjectByType<GameManager>() == null)
        {
            GameObject go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }

        // El spawner también se auto-crea: toma sus prefabs del SpawnConfigSO.
        if (FindAnyObjectByType<EnemySpawner2D>() == null)
        {
            new GameObject("EnemySpawner").AddComponent<EnemySpawner2D>();
        }

        // El gestor de mejoras también se auto-crea (avisa si no tiene pool asignado).
        if (FindAnyObjectByType<UpgradeManager>() == null)
        {
            new GameObject("UpgradeManager").AddComponent<UpgradeManager>();
        }
    }
}
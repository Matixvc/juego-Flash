using UnityEngine;

/// <summary>
/// Genera oleadas de enemigos fuera de cámara y gestiona el evento del JEFE.
/// Toma la configuración (tiempos, tope, prefabs, escalado por Tier) del
/// SpawnConfigSO. Si no está en la escena, GameManager lo crea automáticamente.
/// </summary>
public class EnemySpawner2D : MonoBehaviour
{
    [Header("Overrides manuales (opcional)")]
    [Tooltip("Si está vacío usa el SpawnConfigSO del GameManager.")]
    [SerializeField] private SpawnConfigSO config;
    [SerializeField] private GameObject prefabEscarabajo;
    [SerializeField] private GameObject prefabJefe;

    private Camera camaraPrincipal;
    private Transform jugadorTransform;
    private float temporizador;
    private float tiempoEntreSpawnsActual;

    // Valores cargados desde el SpawnConfigSO
    private float distanciaFueraDeCamara = 3f;
    private float tiempoEntreSpawnsInicial = 2f;
    private float tiempoEntreSpawnsMinimo = 0.3f;
    private float reduccionTiempoPorMinuto = 0.25f;
    private int enemigosPorOleadaInicial = 1;
    private int enemigosPorOleadaMaximo = 5;
    private int maxEnemigosActivos = 30;
    private float bonusOleadaPorTier = 0.25f;

    private bool jefeActivo;
    private Vector2 centroArena;
    private Vector2 tamanoArena;

    private void Start()
    {
        camaraPrincipal = Camera.main;
        CargarConfig();
        tiempoEntreSpawnsActual = tiempoEntreSpawnsInicial;
    }

    private void CargarConfig()
    {
        SpawnConfigSO cfg = config;
        if (cfg == null && GameManager.Instancia != null)
        {
            cfg = GameManager.Instancia.SpawnConfig;
        }

        if (cfg != null)
        {
            distanciaFueraDeCamara = cfg.DistanciaFueraDeCamara;
            tiempoEntreSpawnsInicial = cfg.TiempoEntreSpawnsInicial;
            tiempoEntreSpawnsMinimo = cfg.TiempoEntreSpawnsMinimo;
            reduccionTiempoPorMinuto = cfg.ReduccionTiempoPorMinuto;
            enemigosPorOleadaInicial = cfg.EnemigosPorOleadaInicial;
            enemigosPorOleadaMaximo = cfg.EnemigosPorOleadaMaximo;
            maxEnemigosActivos = cfg.MaxEnemigosActivos;
            bonusOleadaPorTier = cfg.BonusOleadaPorTier;
            centroArena = cfg.CentroArena;
            tamanoArena = cfg.TamanoArena;

            if (prefabEscarabajo == null) prefabEscarabajo = cfg.PrefabEscarabajo;
            if (prefabJefe == null) prefabJefe = cfg.PrefabJefe;
        }

        if (prefabEscarabajo == null)
        {
            Debug.LogWarning("EnemySpawner2D: sin prefab de enemigo. Asigna uno en el Inspector o en SpawnConfig.asset.", this);
        }
    }

    private void Update()
    {
        GestionarEventoJefe();

        if (GameManager.Instancia != null && GameManager.Instancia.Estado != GameManager.EstadoJuego.Jugando)
        {
            return; // Pausa, mejora, evento de jefe o fin de partida: sin oleadas
        }

        if (jugadorTransform == null)
        {
            BuscarJugador();
            if (jugadorTransform == null) return;
        }

        if (prefabEscarabajo == null) return;

        // Dificultad progresiva (tiempo de run) + Tier
        float tiempoJugado = GameManager.Instancia != null && GameManager.Instancia.RunProgress != null
            ? GameManager.Instancia.RunProgress.TiempoDeRun
            : 0f;

        float minutos = tiempoJugado / 60f;
        tiempoEntreSpawnsActual = Mathf.Max(
            tiempoEntreSpawnsMinimo,
            tiempoEntreSpawnsInicial - minutos * reduccionTiempoPorMinuto);

        temporizador += Time.deltaTime;
        if (temporizador >= tiempoEntreSpawnsActual)
        {
            GenerarOleada(tiempoJugado);
            temporizador = 0f;
        }
    }

    // ---------- Evento de Jefe ----------

    private void GestionarEventoJefe()
    {
        if (GameManager.Instancia == null) return;

        if (GameManager.Instancia.Estado == GameManager.EstadoJuego.EventoJefe && !jefeActivo)
        {
            jefeActivo = true;
            InvocarJefe();
        }
        else if (GameManager.Instancia.Estado == GameManager.EstadoJuego.Jugando && jefeActivo)
        {
            // El jefe murió: GameManager volvió a Jugando al notificarse la derrota
            jefeActivo = false;
        }
    }

    private void InvocarJefe()
    {
        if (prefabJefe == null)
        {
            Debug.LogWarning("EnemySpawner2D: sin prefabJefe en el SpawnConfig. Se omite el evento para no trabar la partida.", this);
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.NotificarJefeDerrotado();
            }
            return;
        }

        Vector3 posicion = PosicionDeSpawn();
        GameObject jefe = Instantiate(prefabJefe, posicion, Quaternion.identity);
        Debug.Log("[EnemySpawner2D] ¡El JEFE ha llegado!", jefe);
    }

    // ---------- Oleadas normales ----------

    private void GenerarOleada(float tiempoJugado)
    {
        int tier = GameManager.Instancia != null && GameManager.Instancia.RunProgress != null
            ? GameManager.Instancia.RunProgress.TierDificultad
            : 0;

        float progresoEscalado = (tiempoJugado / 45f) * (enemigosPorOleadaMaximo - enemigosPorOleadaInicial);
        int cantidad = enemigosPorOleadaInicial + Mathf.FloorToInt(progresoEscalado);
        cantidad += Mathf.RoundToInt(tier * bonusOleadaPorTier * (enemigosPorOleadaMaximo - enemigosPorOleadaInicial));
        cantidad = Mathf.Clamp(cantidad, enemigosPorOleadaInicial, enemigosPorOleadaMaximo);

        // Tope global de enemigos vivos
        int cupo = maxEnemigosActivos - Enemy2D.EnemigosActivos;
        if (cupo <= 0) return;
        cantidad = Mathf.Min(cantidad, cupo);

        for (int i = 0; i < cantidad; i++)
        {
            Instantiate(prefabEscarabajo, PosicionDeSpawn(), Quaternion.identity);
        }
    }

    private Vector3 PosicionDeSpawn()
    {
        // Arena cerrada: punto aleatorio dentro de los límites,
        // eligiendo de un puñado de candidatos el más lejano al jugador.
        if (tamanoArena.sqrMagnitude > 0.01f)
        {
            Vector2 mitad = tamanoArena * 0.5f;
            Vector2 mejor = centroArena + new Vector2(Random.Range(-mitad.x, mitad.x), Random.Range(-mitad.y, mitad.y));

            if (jugadorTransform != null)
            {
                float mejorDist = (mejor - (Vector2)jugadorTransform.position).sqrMagnitude;
                for (int i = 0; i < 5; i++)
                {
                    Vector2 candidato = centroArena + new Vector2(Random.Range(-mitad.x, mitad.x), Random.Range(-mitad.y, mitad.y));
                    float d = (candidato - (Vector2)jugadorTransform.position).sqrMagnitude;
                    if (d > mejorDist)
                    {
                        mejorDist = d;
                        mejor = candidato;
                    }
                }
            }

            return mejor;
        }

        // Fallback (arena abierta): fuera de cámara
        if (camaraPrincipal == null)
        {
            Vector2 centroJugador = jugadorTransform != null ? (Vector2)jugadorTransform.position : Vector2.zero;
            return centroJugador + Random.insideUnitCircle.normalized * 12f;
        }

        float altoCamara = camaraPrincipal.orthographicSize;
        float anchoCamara = altoCamara * camaraPrincipal.aspect;
        Vector2 centro = camaraPrincipal.transform.position;

        int borde = Random.Range(0, 4);
        switch (borde)
        {
            case 0: // Arriba
                return new Vector2(Random.Range(centro.x - anchoCamara, centro.x + anchoCamara), centro.y + altoCamara + distanciaFueraDeCamara);
            case 1: // Abajo
                return new Vector2(Random.Range(centro.x - anchoCamara, centro.x + anchoCamara), centro.y - altoCamara - distanciaFueraDeCamara);
            case 2: // Izquierda
                return new Vector2(centro.x - anchoCamara - distanciaFueraDeCamara, Random.Range(centro.y - altoCamara, centro.y + altoCamara));
            default: // Derecha
                return new Vector2(centro.x + anchoCamara + distanciaFueraDeCamara, Random.Range(centro.y - altoCamara, centro.y + altoCamara));
        }
    }

    private void BuscarJugador()
    {
        PlayerController2D jugador = FindAnyObjectByType<PlayerController2D>();
        if (jugador != null)
        {
            jugadorTransform = jugador.transform;
        }
    }
}
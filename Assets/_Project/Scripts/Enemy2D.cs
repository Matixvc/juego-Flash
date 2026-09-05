using System.Collections;
using UnityEngine;

/// <summary>
/// Enemigo con máquina de estados simple (Latente -> Persiguiendo).
/// Persigue a la Roca, evita apelmazarse con otros enemigos (separación),
/// recibe knockback/flash al ser golpeado y suelta orbes de experiencia al morir.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy2D : MonoBehaviour
{
    public enum EstadoEnemigo { Latente, Persiguiendo }

    [Header("Datos (opcional: sobreescribe lo serializado)")]
    [SerializeField] private EnemyDataSO datosEnemigo;

    [Header("Stats base (usados si no hay EnemyDataSO)")]
    [SerializeField] private float velocidadBase = 3.5f;
    [SerializeField] private float variacionVelocidad = 0.5f;
    [SerializeField] private int vida = 1;
    [SerializeField] private int danio = 1;
    [SerializeField] private float cooldownDanio = 0.5f;
    [SerializeField] private float radioDeteccion = 100f;

    [Header("Separación (anti-apelmazamiento)")]
    [SerializeField] private float radioSeparacion = 0.9f;
    [SerializeField] private float fuerzaSeparacion = 5f;

    [Header("Knockback")]
    [SerializeField] private float duracionKnockback = 0.18f;
    [SerializeField] private float tiempoFlash = 0.08f;

    [Header("Recompensa (drops)")]
    [SerializeField] private int expTotal = 3;
    [SerializeField] private int expPorOrbe = 1;
    [SerializeField] private GameObject prefabOrbeExp;

    protected PlayerController2D jugador;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    private EstadoEnemigo estado = EstadoEnemigo.Latente;
    protected float velocidadFinal;
    private float tiempoUltimoDanio;
    protected float tiempoKnockback;
    private Vector2 fuerzaKnockback;
    private int vidaActual;
    private bool estaMuerto;

    /// <summary>El jefe (o variantes) lo activa para tomar el control del movimiento.</summary>
    protected bool MovimientoBloqueado;

    /// <summary>Enemigos vivos en escena (lo usa el spawner como tope).</summary>
    public static int EnemigosActivos { get; private set; }

    public int VidaActual => vidaActual;
    public int VidaMaxima => vida;
    public bool EstaVivo => !estaMuerto;
    public EstadoEnemigo Estado => estado;

    /// <summary>Se dispara al morir (lo usa EnemyBoss para reaccionar).</summary>
    public event System.Action AlMorir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        AplicarDatos();
    }

    protected virtual void OnEnable()
    {
        EnemigosActivos++;
    }

    protected virtual void OnDisable()
    {
        EnemigosActivos--;
    }

    private void AplicarDatos()
    {
        if (datosEnemigo != null)
        {
            velocidadBase = datosEnemigo.VelocidadBase;
            variacionVelocidad = datosEnemigo.VariacionVelocidad;
            vida = datosEnemigo.VidaMaxima;
            danio = datosEnemigo.DanioContacto;
            cooldownDanio = datosEnemigo.CooldownDanio;
            radioDeteccion = datosEnemigo.RadioDeteccion;
            radioSeparacion = datosEnemigo.RadioSeparacion;
            fuerzaSeparacion = datosEnemigo.FuerzaSeparacion;
            expTotal = datosEnemigo.ExpOtorgada;
            expPorOrbe = datosEnemigo.ExpPorOrbe;

            if (datosEnemigo.Sprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = datosEnemigo.Sprite;
            }

            // Escalado por Tier de dificultad
            int tier = GameManager.Instancia != null ? GameManager.Instancia.RunProgress.TierDificultad : 0;
            float multVida = datosEnemigo.EsJefe ? datosEnemigo.MultiplicadorVida : 1f;
            float tierVida = GameManager.Instancia != null && GameManager.Instancia.SpawnConfig != null
                ? 1f + tier * GameManager.Instancia.SpawnConfig.MultiplicadorVidaPorTier
                : 1f;
            float tierVel = GameManager.Instancia != null && GameManager.Instancia.SpawnConfig != null
                ? 1f + tier * GameManager.Instancia.SpawnConfig.MultiplicadorVelocidadPorTier
                : 1f;
            float tierDanio = GameManager.Instancia != null && GameManager.Instancia.SpawnConfig != null
                ? 1f + tier * GameManager.Instancia.SpawnConfig.MultiplicadorDanioPorTier
                : 1f;

            vida = Mathf.Max(1, Mathf.RoundToInt(vida * multVida * tierVida));
            velocidadBase *= tierVel;
            danio = Mathf.Max(1, Mathf.RoundToInt(danio * tierDanio));
        }

        vidaActual = vida;
        velocidadFinal = velocidadBase + Random.Range(-variacionVelocidad, variacionVelocidad);
    }

    protected virtual void Start()
    {
        BuscarJugador();
    }

    private void FixedUpdate()
    {
        if (estaMuerto) return;

        if (GameManager.Instancia != null &&
            GameManager.Instancia.Estado != GameManager.EstadoJuego.Jugando &&
            GameManager.Instancia.Estado != GameManager.EstadoJuego.EventoJefe)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (jugador == null)
        {
            BuscarJugador();
            return;
        }

        // Knockback: no controlar el movimiento mientras dura
        if (tiempoKnockback > 0f)
        {
            tiempoKnockback -= Time.fixedDeltaTime;
            rb.linearVelocity = fuerzaKnockback;
            return;
        }

        // Un jefe (o variante) con control propio del movimiento
        if (MovimientoBloqueado) return;

        Vector2 posJugador = jugador.transform.position;
        float distancia = Vector2.Distance(rb.position, posJugador);

        // Máquina de estados: despierta al entrar en el radio de detección
        if (estado == EstadoEnemigo.Latente && distancia <= radioDeteccion)
        {
            estado = EstadoEnemigo.Persiguiendo;
        }

        if (estado == EstadoEnemigo.Latente)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 4f * Time.fixedDeltaTime);
            return;
        }

        Vector2 direccionJugador = (posJugador - rb.position).normalized;
        Vector2 separacion = CalcularSeparacion();

        Vector2 direccionFinal = (direccionJugador + separacion).normalized;
        rb.linearVelocity = direccionFinal * velocidadFinal;

        // Orientar hacia donde camina
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angulo = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            rb.MoveRotation(angulo);
        }
    }

    private Vector2 CalcularSeparacion()
    {
        Collider2D[] cercanos = Physics2D.OverlapCircleAll(rb.position, radioSeparacion);
        Vector2 separacion = Vector2.zero;

        foreach (Collider2D col in cercanos)
        {
            if (col == null || col.transform == transform || col.transform.parent == transform) continue;

            if (col.TryGetComponent(out Enemy2D _))
            {
                Vector2 alejar = (rb.position - (Vector2)col.transform.position);
                float d = alejar.magnitude;
                if (d < 0.0001f)
                {
                    d = 0.0001f;
                    alejar = Random.insideUnitCircle;
                }
                separacion += (alejar / d) * (fuerzaSeparacion / Mathf.Max(0.1f, d));
            }
        }

        return Vector2.ClampMagnitude(separacion, fuerzaSeparacion);
    }

    private void BuscarJugador()
    {
        jugador = FindAnyObjectByType<PlayerController2D>();
    }

    /// <summary>Daño sin knockback (compatibilidad con StoneAttack2D).</summary>
    public void RecibirDanio(int cantidad = 1)
    {
        RecibirDanio(cantidad, Vector2.zero, 0f);
    }

    /// <summary>Daño con knockback y flash blanco. Devuelve true si herido.</summary>
    public bool RecibirDanio(int cantidad, Vector2 direccionGolpe, float fuerzaEmpuje)
    {
        if (estaMuerto || cantidad <= 0) return false;

        vidaActual = Mathf.Max(0, vidaActual - cantidad);

        // Número de daño flotante (feedback exacto de cuánto pegó)
        NumeroDanio.Crear(transform.position + Vector3.up * 0.6f, cantidad, new Color(1f, 0.95f, 0.7f));

        // Knockback (dirección del golpe, o alejarse del atacante)
        Vector2 dir = direccionGolpe == Vector2.zero
            ? ((Vector2)transform.position - (Vector2)(jugador != null ? jugador.transform.position : transform.position)).normalized
            : direccionGolpe;
        if (dir == Vector2.zero) dir = Random.insideUnitCircle.normalized;

        if (fuerzaEmpuje > 0f)
        {
            tiempoKnockback = duracionKnockback;
            fuerzaKnockback = dir * fuerzaEmpuje;
        }

        if (spriteRenderer != null)
        {
            StartCoroutine(FlashBlanco());
        }

        if (vidaActual <= 0)
        {
            Morir();
        }

        return true;
    }

    private bool _flasheando;

    private IEnumerator FlashBlanco()
    {
        if (spriteRenderer == null || _flasheando) yield break;
        _flasheando = true;

        Color original = spriteRenderer.color;
        Vector3 escalaOriginal = transform.localScale;

        // Flash rojo + golpe de escala (muy visible sobre el escarabajo)
        spriteRenderer.color = new Color(1f, 0.25f, 0.25f, 1f);
        transform.localScale = escalaOriginal * 1.15f;
        yield return new WaitForSeconds(Mathf.Max(0.01f, tiempoFlash));
        spriteRenderer.color = original;
        transform.localScale = escalaOriginal;

        _flasheando = false;
    }

    private void Morir()
    {
        if (estaMuerto) return;
        estaMuerto = true;

        // VFX: pop de muerte con el color del enemigo
        Color colorPop = spriteRenderer != null ? spriteRenderer.color : new Color(0.6f, 0.75f, 0.4f);
        VfxUtil.Destello(transform.position, 1.1f, colorPop, 0.25f);
        VfxUtil.Chispas(transform.position, colorPop, 7);
        CamaraTremor.Agregar(0.12f);

        AlMorir?.Invoke();
        SoltarRecompensas();
        NotificarMuerte();
        Destroy(gameObject);
    }

    private void SoltarRecompensas()
    {
        if (prefabOrbeExp == null || expTotal <= 0) return;

        int porOrbe = Mathf.Max(1, expPorOrbe);
        int cantidadOrbes = Mathf.CeilToInt(Mathf.Max(1, expTotal) / (float)porOrbe);

        for (int i = 0; i < cantidadOrbes; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 0.5f;
            GameObject orbe = Instantiate(prefabOrbeExp, (Vector2)transform.position + offset, Quaternion.identity);

            if (orbe != null && orbe.TryGetComponent(out ExpOrbe expOrbe))
            {
                expOrbe.ConfigurarValor(porOrbe);
            }
        }
    }

    private void NotificarMuerte()
    {
        RunProgressSO run = GameManager.Instancia != null ? GameManager.Instancia.RunProgress : null;
        if (run != null)
        {
            // Solo cuenta de kills; la exp viene de los orbes recolectados.
            run.RegistrarEnemigoEliminado(0);
        }

        // Robo de vida (upgrade Leech): curar al jugador al matar
        if (jugador != null && jugador.TryGetComponent(out PlayerRuntimeStats stats) && stats.TieneLeech)
        {
            jugador.CurarVida(1);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (estaMuerto || Time.time < tiempoUltimoDanio + cooldownDanio) return;

        if (collision.collider.TryGetComponent(out PlayerController2D player) && player.RecibirDanio(danio))
        {
            tiempoUltimoDanio = Time.time;
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Control de movimiento de la Roca basado en físicas (Rigidbody2D).
/// Todos los parámetros se leen de PlayerRuntimeStats, por lo que las
/// mejoras (upgrades) pueden modificarlos durante la partida.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(PlayerRuntimeStats))]
[DisallowMultipleComponent]
public class PlayerController2D : MonoBehaviour
{
    private const float UmbralInput = 0.01f;

    private Rigidbody2D rb;
    private PlayerRuntimeStats stats;
    private SpriteRenderer _sprite;
    private Vector2 entradaInput;
    private Vector2 direccionUltimoMovimiento = Vector2.down;
    private bool estaEnDash;
    private bool puedeHacerDash = true;
    private bool bloquearRotacion;
    private int vidaActual;
    private bool estaMuerto;
    private float _invulnerableHasta;
    private Coroutine _parpadeo;

    private const float DuracionInvulnerable = 0.5f;

    public Vector2 VelocidadActual => rb.linearVelocity;
    public bool EstaEnDash => estaEnDash;
    public bool EstaMuerto => estaMuerto;
    public bool BloquearRotacion
    {
        get => bloquearRotacion;
        set => bloquearRotacion = value;
    }

    public int VidaActual => vidaActual;
    public int VidaMaxima => stats != null ? stats.VidaMaxima : 3;

    /// <summary>
    /// Fuerza la notificación del HUD con los valores actuales
    /// (útil tras aplicar mejoras que cambian la vida máxima).
    /// </summary>
    public void NotificarCambioVida()
    {
        AlCambiarVida?.Invoke(VidaActual, VidaMaxima);
    }

    public event System.Action<int, int> AlCambiarVida;
    public event System.Action AlMorir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerRuntimeStats>();
        _sprite = GetComponent<SpriteRenderer>();

        // Configuración crítica de físicas para top-down preciso
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Evita tirones de cámara
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Evita traspasar enemigos al embestir

        // VFX: rastro del dash (se enciende solo durante el dash)
        if (GetComponent<RastroDash>() == null)
        {
            gameObject.AddComponent<RastroDash>();
        }
    }

    private void Start()
    {
        // En Start todas las Awake ya corrieron (incluida la de PlayerRuntimeStats)
        vidaActual = Mathf.Max(1, VidaMaxima);
        AlCambiarVida?.Invoke(vidaActual, VidaMaxima);

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.BuscarJugador();
        }
    }

    private void Update()
    {
        if (estaMuerto || (GameManager.Instancia != null && GameManager.Instancia.Estado != GameManager.EstadoJuego.Jugando && GameManager.Instancia.Estado != GameManager.EstadoJuego.EventoJefe))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Leer Entrada (WASD / Flechas / Gamepad)
        Keyboard teclado = Keyboard.current;
        Gamepad gamepad = Gamepad.current;

        float moveX = 0f;
        float moveY = 0f;

        if (teclado != null)
        {
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) moveX -= 1f;
            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) moveX += 1f;
            if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) moveY -= 1f;
            if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed) moveY += 1f;
        }

        if (gamepad != null)
        {
            Vector2 stick = gamepad.leftStick.ReadValue();
            moveX += stick.x;
            moveY += stick.y;
        }

        entradaInput = new Vector2(moveX, moveY);
        if (entradaInput.sqrMagnitude > 1f)
        {
            entradaInput.Normalize();
        }

        // Registrar última dirección para el Dash
        if (entradaInput.sqrMagnitude > UmbralInput)
        {
            direccionUltimoMovimiento = entradaInput;
        }

        // Activar Embestida / Dash
        bool pedirDash = (teclado != null && teclado.spaceKey.wasPressedThisFrame)
                      || (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);

        if (pedirDash && puedeHacerDash && !estaEnDash)
        {
            StartCoroutine(EjecutarDash());
        }
    }

    private void FixedUpdate()
    {
        if (estaEnDash || estaMuerto)
        {
            return;
        }

        MoverPiedra();
        RotarHaciaDireccion();
    }

    private void MoverPiedra()
    {
        if (stats == null)
        {
            return;
        }

        // Si hay input, acelerar hacia la velocidad máxima; si no, desacelerar a cero
        Vector2 velocidadObjetivo = entradaInput * stats.VelocidadMaxima;
        float tasaCambio = entradaInput.sqrMagnitude > UmbralInput
            ? stats.Aceleracion
            : stats.Desaceleracion;

        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            velocidadObjetivo,
            tasaCambio * Time.fixedDeltaTime);
    }

    private void RotarHaciaDireccion()
    {
        if (bloquearRotacion || rb.linearVelocity.sqrMagnitude < 0.1f)
        {
            return;
        }

        // Girar suavemente la piedra hacia la dirección de la velocidad actual
        float anguloObjetivo = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
        float anguloSuave = Mathf.LerpAngle(rb.rotation, anguloObjetivo, stats.VelocidadRotacion * Time.fixedDeltaTime);
        rb.MoveRotation(anguloSuave);
    }

    private IEnumerator EjecutarDash()
    {
        puedeHacerDash = false;
        estaEnDash = true;

        // Impulso seco e inmediato en la dirección del movimiento actual
        Vector2 direccionDash = entradaInput.sqrMagnitude > UmbralInput
            ? entradaInput
            : direccionUltimoMovimiento;

        if (stats != null)
        {
            rb.linearVelocity = direccionDash * stats.FuerzaDash;

            yield return new WaitForSeconds(stats.DuracionDash);
            estaEnDash = false;

            yield return new WaitForSeconds(stats.CooldownDash);
        }
        else
        {
            estaEnDash = false;
        }

        puedeHacerDash = true;
    }

    /// <summary>
    /// Aplica daño a la Roca. Devuelve true solo si el golpe surtió efecto.
    /// La Roca es invulnerable durante el Dash (embestida).
    /// </summary>
    public bool RecibirDanio(int cantidad = 1)
    {
        if (estaMuerto || estaEnDash || cantidad <= 0)
        {
            return false;
        }

        if (Time.time < _invulnerableHasta)
        {
            return false;
        }

        int maxVida = Mathf.Max(1, VidaMaxima);
        vidaActual = Mathf.Clamp(vidaActual - cantidad, 0, maxVida);
        _invulnerableHasta = Time.time + DuracionInvulnerable;

        // Feedback claro: número rojo + flash de pantalla + shake + hit-stop + parpadeo
        NumeroDanio.Crear(transform.position + Vector3.up * 0.9f, cantidad, new Color(1f, 0.35f, 0.3f));
        FlashPantalla.Golpe(0.65f);
        CamaraTremor.Agregar(0.55f);
        HitStop.Golpear(0.05f, 0.1f);
        if (_sprite != null && _parpadeo == null)
        {
            _parpadeo = StartCoroutine(Parpadear());
        }

        AlCambiarVida?.Invoke(vidaActual, maxVida);

        if (vidaActual <= 0)
        {
            Morir();
        }

        return true;
    }

    /// <summary>Parpadeo rojo durante la invulnerabilidad (en tiempo real, sobrevive al hit-stop).</summary>
    private IEnumerator Parpadear()
    {
        Color original = _sprite.color;
        float hasta = Time.unscaledTime + DuracionInvulnerable;

        while (Time.unscaledTime < hasta && !estaMuerto)
        {
            _sprite.color = new Color(1f, 0.4f, 0.4f, 0.45f);
            yield return new WaitForSecondsRealtime(0.08f);
            _sprite.color = original;
            yield return new WaitForSecondsRealtime(0.08f);
        }

        _sprite.color = original;
        _parpadeo = null;
    }

    public void CurarVida(int cantidad)
    {
        if (estaMuerto || cantidad <= 0)
        {
            return;
        }

        int maxVida = Mathf.Max(1, VidaMaxima);
        vidaActual = Mathf.Clamp(vidaActual + cantidad, 0, maxVida);
        AlCambiarVida?.Invoke(vidaActual, maxVida);
    }

    private void Morir()
    {
        if (estaMuerto)
        {
            return;
        }

        estaMuerto = true;
        estaEnDash = false;
        rb.linearVelocity = Vector2.zero;
        AlMorir?.Invoke();
        Debug.Log("La Roca ha sido destruida.", gameObject);
    }
}
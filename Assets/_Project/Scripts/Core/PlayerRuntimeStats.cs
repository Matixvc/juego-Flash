using UnityEngine;

/// <summary>
/// Copia en tiempo de ejecución de PlayerDataSO con valores modificables.
/// Las mejoras (upgrades) aplican modificadores aquí durante la run.
/// Va en el mismo GameObject que PlayerController2D.
/// </summary>
public class PlayerRuntimeStats : MonoBehaviour
{
    public enum StatJugador
    {
        VelocidadMaxima,
        Aceleracion,
        Desaceleracion,
        FuerzaDash,
        DuracionDash,
        CooldownDash,
        VidaMaxima,
        RadioOndaChoque,
        CooldownPisoton,
        RadioImpactoDash,
        DanioPisoton,
        DanioDash,
        RadioMagnetOrbes,
        MultiplicadorDanio
    }

    [Header("Datos base (ScriptableObject)")]
    [SerializeField] private PlayerDataSO datosBase;

    // Valores copiados en Awake
    private float velocidadMaxima;
    private float aceleracion;
    private float desaceleracion;
    private float velocidadRotacion;
    private float fuerzaDash;
    private float duracionDash;
    private float cooldownDash;
    private int vidaMaxima;
    private float radioOndaChoque;
    private float cooldownPisoton;
    private float radioImpactoDash;
    private int danioPisoton;
    private int danioDash;
    private float radioMagnetOrbes;
    private float multiplicadorDanio = 1f;
    private bool tieneLeech;

    public PlayerDataSO DatosBase => datosBase;
    public float VelocidadMaxima => velocidadMaxima;
    public float Aceleracion => aceleracion;
    public float Desaceleracion => desaceleracion;
    public float VelocidadRotacion => velocidadRotacion;
    public float FuerzaDash => fuerzaDash;
    public float DuracionDash => duracionDash;
    public float CooldownDash => cooldownDash;
    public int VidaMaxima => vidaMaxima;
    public float RadioOndaChoque => radioOndaChoque;
    public float CooldownPisoton => cooldownPisoton;
    public float RadioImpactoDash => radioImpactoDash;
    public int DanioPisoton => danioPisoton;
    public int DanioDash => danioDash;
    public float RadioMagnetOrbes => radioMagnetOrbes;
    public float MultiplicadorDanio => multiplicadorDanio;
    /// <summary>Robo de vida: cura al jugador al eliminar un enemigo.</summary>
    public bool TieneLeech => tieneLeech;

    /// <summary>Activa el robo de vida (la mejora "Robo de Vida" lo llama).</summary>
    public void ActivarLeech()
    {
        tieneLeech = true;
    }

    private void Awake()
    {
        Reiniciar();
    }

    public void Reiniciar()
    {
        if (datosBase == null)
        {
            Debug.LogWarning("PlayerRuntimeStats: asigna el PlayerDataSO en 'Datos base'.", this);
            return;
        }

        velocidadMaxima = datosBase.VelocidadMaxima;
        aceleracion = datosBase.Aceleracion;
        desaceleracion = datosBase.Desaceleracion;
        velocidadRotacion = datosBase.VelocidadRotacion;
        fuerzaDash = datosBase.FuerzaDash;
        duracionDash = datosBase.DuracionDash;
        cooldownDash = datosBase.CooldownDash;
        vidaMaxima = datosBase.VidaMaxima;
        radioOndaChoque = datosBase.RadioOndaChoque;
        cooldownPisoton = datosBase.CooldownPisoton;
        radioImpactoDash = datosBase.RadioImpactoDash;
        danioPisoton = datosBase.DanioPisoton;
        danioDash = datosBase.DanioDash;
        radioMagnetOrbes = datosBase.RadioMagnetOrbes;
        multiplicadorDanio = 1f;
        tieneLeech = false;
    }

    /// <summary>
    /// Aplica un modificador a un stat del jugador (usado por las mejoras).
    /// Para stats porcentuales como MultiplicadorDanio, pasar 0.25 = +25%.
    /// </summary>
    public void AplicarModificador(StatJugador stat, float valor)
    {
        switch (stat)
        {
            case StatJugador.VelocidadMaxima:
                velocidadMaxima = Mathf.Max(0.1f, velocidadMaxima + valor);
                break;
            case StatJugador.Aceleracion:
                aceleracion = Mathf.Max(1f, aceleracion + valor);
                break;
            case StatJugador.Desaceleracion:
                desaceleracion = Mathf.Max(1f, desaceleracion + valor);
                break;
            case StatJugador.FuerzaDash:
                fuerzaDash = Mathf.Max(1f, fuerzaDash + valor);
                break;
            case StatJugador.DuracionDash:
                duracionDash = Mathf.Max(0.02f, duracionDash + valor);
                break;
            case StatJugador.CooldownDash:
                cooldownDash = Mathf.Max(0.05f, cooldownDash + valor);
                break;
            case StatJugador.VidaMaxima:
                vidaMaxima = Mathf.Max(1, vidaMaxima + Mathf.RoundToInt(valor));
                break;
            case StatJugador.RadioOndaChoque:
                radioOndaChoque = Mathf.Max(0.3f, radioOndaChoque + valor);
                break;
            case StatJugador.CooldownPisoton:
                cooldownPisoton = Mathf.Max(0.05f, cooldownPisoton + valor);
                break;
            case StatJugador.RadioImpactoDash:
                radioImpactoDash = Mathf.Max(0.3f, radioImpactoDash + valor);
                break;
            case StatJugador.DanioPisoton:
                danioPisoton = Mathf.Max(1, danioPisoton + Mathf.RoundToInt(valor));
                break;
            case StatJugador.DanioDash:
                danioDash = Mathf.Max(1, danioDash + Mathf.RoundToInt(valor));
                break;
            case StatJugador.RadioMagnetOrbes:
                radioMagnetOrbes = Mathf.Max(0.5f, radioMagnetOrbes + valor);
                break;
            case StatJugador.MultiplicadorDanio:
                multiplicadorDanio = Mathf.Max(0.1f, multiplicadorDanio + valor);
                break;
        }
    }
}
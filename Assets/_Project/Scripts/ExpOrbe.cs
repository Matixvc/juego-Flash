using UnityEngine;

/// <summary>
/// Orbe de experiencia que sueltan los enemigos al morir.
/// Espera un instante y luego es atraído magneticamente hacia la Roca.
/// Al tocarla suma experiencia a la RunProgress actual.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ExpOrbe : MonoBehaviour
{
    [Header("Magnet")]
    [SerializeField] private float retrasoActivacion = 0.25f;
    [SerializeField] private float aceleracionMagnet = 14f;
    [SerializeField] private float velocidadMaxMagnet = 9f;

    [Header("Valor")]
    [SerializeField] private int expOtorgada = 1;

    private Transform jugador;
    private Vector2 velocidadMagnet;
    private float tiempoVivo;
    private bool recogido;

    private void Start()
    {
        PlayerController2D player = FindAnyObjectByType<PlayerController2D>();
        if (player != null)
        {
            jugador = player.transform;
        }
    }

    private void Update()
    {
        if (recogido || jugador == null)
        {
            return;
        }

        tiempoVivo += Time.deltaTime;
        if (tiempoVivo < retrasoActivacion)
        {
            return;
        }

        Vector2 posicion = transform.position;
        Vector2 posicionJugador = jugador.position;
        float radioMagnet = RadioMagnetDelJugador();

        if ((posicionJugador - posicion).sqrMagnitude > radioMagnet * radioMagnet)
        {
            return;
        }

        Vector2 direccion = (posicionJugador - posicion).normalized;
        velocidadMagnet += direccion * (aceleracionMagnet * Time.deltaTime);

        if (velocidadMagnet.magnitude > velocidadMaxMagnet)
        {
            velocidadMagnet = velocidadMagnet.normalized * velocidadMaxMagnet;
        }

        transform.position += (Vector3)(velocidadMagnet * Time.deltaTime);
    }

    private float RadioMagnetDelJugador()
    {
        if (jugador == null)
        {
            return 2.5f;
        }

        PlayerRuntimeStats stats = jugador.GetComponent<PlayerRuntimeStats>();
        return stats != null ? stats.RadioMagnetOrbes : 2.5f;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (recogido || !col.TryGetComponent(out PlayerController2D _))
        {
            return;
        }

        Recoger();
    }

    /// <summary>Configura cuanta exp otorga este orbe (lo usa el sistema de drops).</summary>
    public void ConfigurarValor(int cantidad)
    {
        expOtorgada = Mathf.Max(1, cantidad);
    }

    private void Recoger()
    {
        recogido = true;

        RunProgressSO run = GameManager.Instancia != null ? GameManager.Instancia.RunProgress : null;
        if (run != null)
        {
            run.SumarExp(expOtorgada);
        }

        Destroy(gameObject);
    }
}
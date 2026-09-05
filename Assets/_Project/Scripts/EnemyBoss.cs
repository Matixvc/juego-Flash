using UnityEngine;

/// <summary>
/// Jefe Élite: hereda la persecución de Enemy2D y añade una embestida
/// telegrafiada (apunta → tiembla → embiste → descansa) e invoca minis
/// al cruzar un umbral de vida. Al morir notifica a GameManager, que sube
/// el Tier de dificultad y otorga Esencia de Cristal.
/// </summary>
public class EnemyBoss : Enemy2D
{
    private enum FaseJefe { Normal, Apuntando, Embistiendo, Descansando }

    [Header("Jefe — Embestida")]
    [SerializeField] private float intervaloEmbestida = 5f;
    [SerializeField] private float tiempoApuntado = 0.7f;
    [SerializeField] private float velocidadEmbestida = 13f;
    [SerializeField] private float maxDuracionEmbestida = 1.1f;
    [SerializeField] private float descansoTrasEmbestida = 0.9f;
    [SerializeField] private Color colorApuntado = new Color(1f, 0.35f, 0.3f, 1f);

    [Header("Jefe — Invocación de minis")]
    [SerializeField] private GameObject prefabMini;
    [SerializeField] private int cantidadMinis = 3;
    [SerializeField, Range(0.1f, 0.9f)] private float umbralVidaInvocacion = 0.5f;

    private FaseJefe fase = FaseJefe.Normal;
    private float temporizadorFase;
    private Vector2 direccionEmbestida;
    private Color colorOriginal = Color.white;
    private Vector3 escalaBase = Vector3.one;
    private bool minisInvocadas;

    protected override void OnEnable()
    {
        base.OnEnable();
        AlMorir += NotificarJefeDerrotado;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        AlMorir -= NotificarJefeDerrotado;
    }

    protected override void Start()
    {
        base.Start();

        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;
        escalaBase = transform.localScale;
        temporizadorFase = intervaloEmbestida;
    }

    private void Update()
    {
        if (!EstaVivo || tiempoKnockback > 0f) return;

        // Invoca minis al cruzar el umbral de vida (una sola vez)
        if (!minisInvocadas && VidaMaxima > 0 &&
            VidaActual <= Mathf.CeilToInt(VidaMaxima * umbralVidaInvocacion))
        {
            InvocarMinis();
        }

        switch (fase)
        {
            case FaseJefe.Normal:
                temporizadorFase -= Time.deltaTime;
                if (temporizadorFase <= 0f && jugador != null)
                {
                    fase = FaseJefe.Apuntando;
                    temporizadorFase = tiempoApuntado;
                    direccionEmbestida = ((Vector2)jugador.transform.position - rb.position).normalized;
                    if (direccionEmbestida == Vector2.zero) direccionEmbestida = Vector2.up;
                    MovimientoBloqueado = true;
                    if (spriteRenderer != null) spriteRenderer.color = colorApuntado;
                }
                break;

            case FaseJefe.Apuntando:
                temporizadorFase -= Time.deltaTime;
                // Tiembla mientras apunta (aviso visual)
                transform.localScale = escalaBase * (1f + Mathf.Sin(Time.time * 45f) * 0.06f);
                if (temporizadorFase <= 0f)
                {
                    fase = FaseJefe.Embistiendo;
                    temporizadorFase = maxDuracionEmbestida;
                    transform.localScale = escalaBase;
                    if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
                    CamaraTremor.Agregar(0.3f);
                }
                break;

            case FaseJefe.Embistiendo:
                rb.linearVelocity = direccionEmbestida * velocidadEmbestida;
                OrientarHacia(direccionEmbestida);
                temporizadorFase -= Time.deltaTime;
                if (temporizadorFase <= 0f)
                {
                    fase = FaseJefe.Descansando;
                    temporizadorFase = descansoTrasEmbestida;
                    rb.linearVelocity = Vector2.zero;
                }
                break;

            case FaseJefe.Descansando:
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 10f * Time.deltaTime);
                temporizadorFase -= Time.deltaTime;
                if (temporizadorFase <= 0f)
                {
                    fase = FaseJefe.Normal;
                    temporizadorFase = intervaloEmbestida;
                    MovimientoBloqueado = false; // devuelve el control a Enemy2D
                }
                break;
        }
    }

    private void OrientarHacia(Vector2 direccion)
    {
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg - 90f;
        rb.MoveRotation(angulo);
    }

    private void InvocarMinis()
    {
        minisInvocadas = true;

        if (prefabMini == null || cantidadMinis <= 0) return;

        for (int i = 0; i < cantidadMinis; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * (1.4f + i * 0.25f);
            Instantiate(prefabMini, rb.position + offset, Quaternion.identity);
        }
    }

    private void NotificarJefeDerrotado()
    {
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.NotificarJefeDerrotado();
        }
    }
}
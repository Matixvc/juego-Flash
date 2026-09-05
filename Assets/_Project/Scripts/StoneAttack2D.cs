using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ataques de la Roca usando los stats vivos del PlayerRuntimeStats:
/// 1. Pisotón (onda de choque): Clic izquierdo o tecla J.
/// 2. Daño por impacto al embestir con el Dash.
/// </summary>
[RequireComponent(typeof(PlayerController2D))]
[RequireComponent(typeof(PlayerRuntimeStats))]
public class StoneAttack2D : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private LayerMask capaEnemigos = ~0;

    [Header("Pisotón")]
    [SerializeField] private float fuerzaEmpujePisoton = 8f;

    private const int TamanoBuffer = 128;

    private PlayerController2D playerController;
    private PlayerRuntimeStats stats;
    private Collider2D[] bufferImpactos;
    private ContactFilter2D filtroEnemigos;
    private HashSet<Enemy2D> enemigosGolpeadosEnDash;
    private bool puedeHacerPisoton = true;

    private void Awake()
    {
        playerController = GetComponent<PlayerController2D>();
        stats = GetComponent<PlayerRuntimeStats>();
        bufferImpactos = new Collider2D[TamanoBuffer];
        enemigosGolpeadosEnDash = new HashSet<Enemy2D>();

        filtroEnemigos = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = true,
            layerMask = capaEnemigos
        };
    }

    private void Update()
    {
        // 1. Pisotón con Clic Izquierdo o Tecla J
        if (puedeHacerPisoton && InputPisoton())
        {
            StartCoroutine(EjecutarPisoton());
        }

        // 2. Si la Roca está haciendo el Dash, daña enemigos al pasarles por encima
        if (playerController.EstaEnDash)
        {
            ImpactarEnemigosConDash();
        }
        else if (enemigosGolpeadosEnDash.Count > 0)
        {
            enemigosGolpeadosEnDash.Clear();
        }
    }

    private static bool InputPisoton()
    {
        bool clic = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool teclaJ = Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame;
        return clic || teclaJ;
    }

    private IEnumerator EjecutarPisoton()
    {
        puedeHacerPisoton = false;

        if (stats == null)
        {
            puedeHacerPisoton = true;
            yield break;
        }

        // Pequeño salto/escala cómica antes del golpe
        Vector3 escalaOriginal = transform.localScale;
        transform.localScale = escalaOriginal * 1.3f;

        yield return new WaitForSeconds(0.08f);

        transform.localScale = escalaOriginal;
        GenerarOndaDeChoque();

        yield return new WaitForSeconds(stats.CooldownPisoton);
        puedeHacerPisoton = true;
    }

    private void GenerarOndaDeChoque()
    {
        if (stats == null)
        {
            return;
        }

        // Radio con margen de gracia: golpear un poquito más allá del anillo visual
        // evita la sensación de "a veces pega, a veces no" en enemigos al borde.
        float radioReal = stats.RadioOndaChoque + 0.3f;
        int cantidad = Physics2D.OverlapCircle(
            transform.position, radioReal, filtroEnemigos, bufferImpactos);

        bool golpeoAlgo = false;

        for (int i = 0; i < cantidad; i++)
        {
            Collider2D col = bufferImpactos[i];
            if (col != null && col.TryGetComponent(out Enemy2D enemigo))
            {
                Vector2 dirEmpuje = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;
                if (enemigo.RecibirDanio(stats.DanioPisoton, dirEmpuje, fuerzaEmpujePisoton))
                {
                    golpeoAlgo = true;
                }
            }
        }

        // VFX: anillo expansivo (radio REAL de impacto para feedback visual preciso) + chispas + tremor (+ hit-stop si conectó)
        VfxUtil.AnilloChoque(transform.position, radioReal, new Color(1f, 0.85f, 0.45f), 0.35f);
        VfxUtil.Chispas(transform.position, new Color(0.85f, 0.65f, 0.4f), golpeoAlgo ? 10 : 5);
        CamaraTremor.Agregar(golpeoAlgo ? 0.5f : 0.25f);
        if (golpeoAlgo)
        {
            HitStop.Golpear();
        }
    }

    private void ImpactarEnemigosConDash()
    {
        if (stats == null)
        {
            return;
        }

        int cantidad = Physics2D.OverlapCircle(
            transform.position, stats.RadioImpactoDash, filtroEnemigos, bufferImpactos);

        for (int i = 0; i < cantidad; i++)
        {
            Collider2D col = bufferImpactos[i];
            if (col != null && col.TryGetComponent(out Enemy2D enemigo) && enemigosGolpeadosEnDash.Add(enemigo))
            {
                enemigo.RecibirDanio(stats.DanioDash);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (stats == null)
        {
            stats = GetComponent<PlayerRuntimeStats>();
        }

        if (stats == null)
        {
            return;
        }

        // Visualizar los rangos de ataque en la ventana Scene de Unity
        Gizmos.color = Color.yellow;
        float radioVisual = stats != null ? stats.RadioOndaChoque + 0.3f : 1.1f;
        Gizmos.DrawWireSphere(transform.position, radioVisual);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.RadioImpactoDash);
    }
}
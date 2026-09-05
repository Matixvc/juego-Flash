using System;
using UnityEngine;

/// <summary>
/// Estado "vivo" de la carrera actual: experiencia, nivel, dificultad (Tier),
/// estadísticas de la run y esencia de cristal (moneda meta).
/// Al ser un ScriptableObject puede inspeccionarse en el Editor, pero se reinicia
/// con ResetRun() al comenzar cada partida.
/// </summary>
[CreateAssetMenu(fileName = "RunProgress", menuName = "JuegoFlash/Data/Run Progress")]
public class RunProgressSO : ScriptableObject
{
    [Header("Curva de experiencia")]
    [Tooltip("x = nivel actual, y = exp requerida para subir al siguiente.")]
    [SerializeField] private AnimationCurve curvaExp = AnimationCurve.Linear(1f, 5f, 30f, 150f);

    [Header("Moneda meta inicial")]
    [SerializeField] private int esenciaInicial;

    // Estado vivo (no se persiste en el asset)
    [NonSerialized] private int nivel = 1;
    [NonSerialized] private float expAcumulada;
    [NonSerialized] private int enemigosEliminados;
    [NonSerialized] private int jefesDerrotados;
    [NonSerialized] private float tiempoDeRun;
    [NonSerialized] private int tierDificultad;
    [NonSerialized] private int esenciaDeCristal;

    public event Action<int, int> AlSubirNivel; // (nivelNuevo, expRequeridaDelNuevo)
    public event Action AlCambiarExp;
    public event Action<int> AlSumarEsencia;

    public int Nivel => nivel;
    public float ExpAcumulada => expAcumulada;
    public float ExpRequeridaNivelActual => curvaExp.Evaluate(nivel);
    public int EnemigosEliminados => enemigosEliminados;
    public int JefesDerrotados => jefesDerrotados;
    public float TiempoDeRun => tiempoDeRun;
    public int TierDificultad => tierDificultad;
    public int EsenciaDeCristal => esenciaDeCristal;

    public float ProgresoExpNivel => ExpRequeridaNivelActual > 0f
        ? Mathf.Clamp01(expAcumulada / ExpRequeridaNivelActual)
        : 0f;

    public void ResetRun()
    {
        nivel = 1;
        expAcumulada = 0f;
        enemigosEliminados = 0;
        jefesDerrotados = 0;
        tiempoDeRun = 0f;
        tierDificultad = 0;
        esenciaDeCristal = esenciaInicial;
    }

    public void SumarTiempo(float delta)
    {
        tiempoDeRun += delta;
    }

    public void SumarExp(float cantidad)
    {
        if (cantidad <= 0f)
        {
            return;
        }

        expAcumulada += cantidad;

        int nivelesSubidos = 0;
        while (expAcumulada >= ExpRequeridaNivelActual)
        {
            expAcumulada -= ExpRequeridaNivelActual;
            nivel++;
            nivelesSubidos++;
        }

        if (nivelesSubidos > 0)
        {
            AlSubirNivel?.Invoke(nivel, Mathf.FloorToInt(ExpRequeridaNivelActual));
        }

        AlCambiarExp?.Invoke();
    }

    public void RegistrarEnemigoEliminado(int expOtorgada)
    {
        enemigosEliminados++;
        SumarExp(expOtorgada);
    }

    public void RegistrarJefeDerrotado()
    {
        jefesDerrotados++;
        tierDificultad++;
        esenciaDeCristal++;
        AlSumarEsencia?.Invoke(esenciaDeCristal);
    }

    public bool QuitarEsencia(int cantidad)
    {
        if (cantidad <= 0 || esenciaDeCristal < cantidad)
        {
            return false;
        }

        esenciaDeCristal -= cantidad;
        AlSumarEsencia?.Invoke(esenciaDeCristal);
        return true;
    }
}
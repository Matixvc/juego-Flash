using UnityEngine;

/// <summary>
/// Define un tipo de enemigo (stats, IA, recompensa).
/// Crea una instancia por variante: Escarabajo, Mosca, Hoja, Jefe...
/// </summary>
[CreateAssetMenu(fileName = "EnemyData", menuName = "JuegoFlash/Data/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Identidad")]
    [SerializeField] private string nombreEnemigo = "Escarabajo";
    [SerializeField] private Sprite sprite;

    [Header("Stats")]
    [SerializeField] private float velocidadBase = 3.5f;
    [SerializeField] private float variacionVelocidad = 0.5f;
    [SerializeField] private int vidaMaxima = 1;
    [SerializeField] private int danioContacto = 1;
    [SerializeField] private float cooldownDanio = 0.5f;

    [Header("Persecución")]
    [SerializeField] private float radioDeteccion = 100f;
    [SerializeField] private float radioSeparacion = 0.9f;
    [SerializeField] private float fuerzaSeparacion = 5f;

    [Header("Recompensa")]
    [SerializeField] private int expOtorgada = 3;
    [SerializeField] private int expPorOrbe = 1;

    [Header("Jefe / Élite")]
    [SerializeField] private bool esJefe;
    [SerializeField] private float multiplicadorVida = 1f;
    [SerializeField] private int esenciaOtorgada = 1;
    [SerializeField] private int expOrbesJefe = 12;

    public string NombreEnemigo => nombreEnemigo;
    public Sprite Sprite => sprite;
    public float VelocidadBase => velocidadBase;
    public float VariacionVelocidad => variacionVelocidad;
    public int VidaMaxima => vidaMaxima;
    public int DanioContacto => danioContacto;
    public float CooldownDanio => cooldownDanio;
    public float RadioDeteccion => radioDeteccion;
    public float RadioSeparacion => radioSeparacion;
    public float FuerzaSeparacion => fuerzaSeparacion;
    public int ExpOtorgada => expOtorgada;
    public int ExpPorOrbe => expPorOrbe;
    public bool EsJefe => esJefe;
    public float MultiplicadorVida => multiplicadorVida;
    public int EsenciaOtorgada => esenciaOtorgada;
    public int ExpOrbesJefe => expOrbesJefe;
}
using UnityEngine;

/// <summary>
/// Configuración base de la Roca (jugador). Es el punto de partida de cada run.
/// Las mejoras (upgrades) modifican una copia viva en PlayerRuntimeStats.
/// </summary>
[CreateAssetMenu(fileName = "PlayerData", menuName = "JuegoFlash/Data/Player Data")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadMaxima = 8.5f;
    [SerializeField] private float aceleracion = 60f;
    [SerializeField] private float desaceleracion = 50f;
    [SerializeField] private float velocidadRotacion = 15f;

    [Header("Dash / Embestida")]
    [SerializeField] private float fuerzaDash = 26f;
    [SerializeField] private float duracionDash = 0.18f;
    [SerializeField] private float cooldownDash = 0.6f;

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 3;

    [Header("Pisotón (onda de choque)")]
    [SerializeField] private float radioOndaChoque = 2.5f;
    [SerializeField] private float cooldownPisoton = 0.8f;
    [SerializeField] private int danioPisoton = 1;

    [Header("Impacto por Dash")]
    [SerializeField] private float radioImpactoDash = 1.2f;
    [SerializeField] private int danioDash = 1;

    [Header("Orbes de experiencia")]
    [SerializeField] private float radioMagnetOrbes = 2.5f;

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
    public int DanioPisoton => danioPisoton;
    public float RadioImpactoDash => radioImpactoDash;
    public int DanioDash => danioDash;
    public float RadioMagnetOrbes => radioMagnetOrbes;
}
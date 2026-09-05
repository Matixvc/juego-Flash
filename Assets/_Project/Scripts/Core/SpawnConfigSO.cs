using UnityEngine;

/// <summary>
/// Configuración global del spawner de enemigos y del evento de jefe.
/// </summary>
[CreateAssetMenu(fileName = "SpawnConfig", menuName = "JuegoFlash/Data/Spawn Config")]
public class SpawnConfigSO : ScriptableObject
{
    [Header("Oleadas normales")]
    [SerializeField] private float tiempoEntreSpawnsInicial = 2f;
    [SerializeField] private float tiempoEntreSpawnsMinimo = 0.3f;
    [SerializeField] private float reduccionTiempoPorMinuto = 0.25f;
    [SerializeField] private int enemigosPorOleadaInicial = 1;
    [SerializeField] private int enemigosPorOleadaMaximo = 5;

    [Header("Spawn")]
    [SerializeField] private float distanciaFueraDeCamara = 3f;
    [SerializeField] private int maxEnemigosActivos = 30;

    [Header("Arena cerrada (spawns dentro de los límites; tamaño (0,0) = usar cámara)")]
    [SerializeField] private Vector2 centroArena = Vector2.zero;
    [SerializeField] private Vector2 tamanoArena = new Vector2(29f, 15f);

    [Header("Prefabs (referenciados aquí para auto-configurar el spawner)")]
    [SerializeField] private GameObject prefabEscarabajo;
    [SerializeField] private GameObject prefabJefe;

    [Header("Evento de Jefe")]
    [SerializeField] private float tiempoEntreJefes = 270f; // 4.5 minutos en segundos
    [SerializeField] private EnemyDataSO datosJefe;

    [Header("Escalado por Tier (nivel de dificultad)")]
    [SerializeField, Range(0f, 2f)] private float multiplicadorVidaPorTier = 0.35f;
    [SerializeField, Range(0f, 0.5f)] private float multiplicadorVelocidadPorTier = 0.06f;
    [SerializeField, Range(0f, 0.5f)] private float multiplicadorDanioPorTier = 0.1f;
    [SerializeField, Range(0f, 1f)] private float bonusOleadaPorTier = 0.25f;

    public float TiempoEntreSpawnsInicial => tiempoEntreSpawnsInicial;
    public float TiempoEntreSpawnsMinimo => tiempoEntreSpawnsMinimo;
    public float ReduccionTiempoPorMinuto => reduccionTiempoPorMinuto;
    public int EnemigosPorOleadaInicial => enemigosPorOleadaInicial;
    public int EnemigosPorOleadaMaximo => enemigosPorOleadaMaximo;
    public float DistanciaFueraDeCamara => distanciaFueraDeCamara;
    public int MaxEnemigosActivos => maxEnemigosActivos;
    public GameObject PrefabEscarabajo => prefabEscarabajo;
    public GameObject PrefabJefe => prefabJefe;
    public Vector2 CentroArena => centroArena;
    public Vector2 TamanoArena => tamanoArena;
    public float TiempoEntreJefes => tiempoEntreJefes;
    public EnemyDataSO DatosJefe => datosJefe;
    public float MultiplicadorVidaPorTier => multiplicadorVidaPorTier;
    public float MultiplicadorVelocidadPorTier => multiplicadorVelocidadPorTier;
    public float MultiplicadorDanioPorTier => multiplicadorDanioPorTier;
    public float BonusOleadaPorTier => bonusOleadaPorTier;
}
using UnityEngine;

/// <summary>
/// Efecto de una mejora: modifica un stat del jugador con un multiplicador o suma.
/// Se usa dentro de UpgradeSO. Puedes tener varios efectos por mejora.
/// </summary>
[System.Serializable]
public struct UpgradeEffect
{
    [Tooltip("Stat del jugador que se modifica.")]
    public PlayerRuntimeStats.StatJugador stat;

    [Tooltip("true = suma/resta fija (ej. +0.5 de daño). false = multiplicador (ej. 0.25 = +25%).")]
    public bool esPlano;

    [Tooltip("Valor del efecto. En modo multiplicador, 0.25 = +25%, -0.1 = -10%.")]
    public float valor;
}

/// <summary>
/// Definición de una mejora elegible en el roguelike: nombre, descripción,
/// rareza, ícono y efectos/estadísticas que aplica al jugador.
/// Crea una instancia del asset por cada mejora distinta.
/// </summary>
[CreateAssetMenu(fileName = "Upgrade", menuName = "JuegoFlash/Data/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    public enum Raridad
    {
        Comun,
        Rara,
        Epica
    }

    [Header("Identidad")]
    [SerializeField] private string nombreMejora = "Nueva Mejora";
    [SerializeField, TextArea(2, 4)] private string descripcion;
    [SerializeField] private Sprite icono;

    [Header("Rareza")]
    [SerializeField] private Raridad raridad = Raridad.Comun;

    [Header("Efectos")]
    [SerializeField, Tooltip("Hasta 3 efectos sobre stats del jugador.")]
    private UpgradeEffect[] efectos;

    [Header("Efectos especiales (flags)")]
    [SerializeField, Tooltip("Robo de vida: cura 1 al eliminar un enemigo.")]
    private bool esLeech;

    public string NombreMejora => nombreMejora;
    public string Descripcion => descripcion;
    public Sprite Icono => icono;
    public Raridad RaridadMejora => raridad;
    public UpgradeEffect[] Efectos => efectos != null ? efectos : System.Array.Empty<UpgradeEffect>();
    public bool EsLeech => esLeech;
}
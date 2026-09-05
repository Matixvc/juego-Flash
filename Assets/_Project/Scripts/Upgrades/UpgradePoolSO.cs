using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool de mejoras disponibles: el juego elige 3 al azar ponderadas por rareza.
/// Con el tiempo se desbloquean más mejoras (límite opcional por run).
/// </summary>
[CreateAssetMenu(fileName = "UpgradePool", menuName = "JuegoFlash/Data/Upgrade Pool")]
public class UpgradePoolSO : ScriptableObject
{
    [Header("Cartas ofrecidas por subida de nivel")]
    [SerializeField] private int cartasOfrecidas = 3;

    [Header("Mejoras disponibles (todas)")]
    [SerializeField] private List<UpgradeSO> mejorasDisponibles = new List<UpgradeSO>();

    [Header("Reglas")]
    [SerializeField] private bool permitirRepetir = true;

    public int CartasOfrecidas => Mathf.Max(1, cartasOfrecidas);
    public IReadOnlyList<UpgradeSO> MejorasDisponibles => mejorasDisponibles;

    /// <summary>
    /// Elige N mejoras aleatorias, una por tipo (no repite en la misma oferta).
    /// Aplica la regla permitirRepetir entre ofertas distintas.
    /// </summary>
    public List<UpgradeSO> ElegirMejoras(int cantidad, HashSet<UpgradeSO> yaTomadas)
    {
        List<UpgradeSO> resultado = new List<UpgradeSO>();
        if (mejorasDisponibles == null || mejorasDisponibles.Count == 0)
        {
            return resultado;
        }

        // Baraja las disponibles
        List<UpgradeSO> candidatas = new List<UpgradeSO>(mejorasDisponibles);
        for (int i = candidatas.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (candidatas[i], candidatas[j]) = (candidatas[j], candidatas[i]);
        }

        foreach (UpgradeSO mejora in candidatas)
        {
            if (resultado.Count >= cantidad)
            {
                break;
            }

            // No ofrecer la misma mejora dos veces en la misma oferta
            if (resultado.Contains(mejora))
            {
                continue;
            }

            // Si no se permiten repetidas, saltar las ya tomadas
            if (!permitirRepetir && yaTomadas != null && yaTomadas.Contains(mejora))
            {
                continue;
            }

            resultado.Add(mejora);
        }

        return resultado;
    }
}
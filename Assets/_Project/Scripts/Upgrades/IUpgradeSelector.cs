using UnityEngine;

/// <summary>
/// Interfaz que implementan las UIs de selección de mejoras.
/// Se define en su propio archivo para que UpgradeUI.cs no dependa
/// de la compilación de UpgradeManager.cs.
/// </summary>
public interface IUpgradeSelector
{
    /// <summary>Muestra las opciones de mejora ofrecidas.</summary>
    void MostrarOpciones(UpgradeSO[] opciones);
}
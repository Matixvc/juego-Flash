using UnityEngine;

/// <summary>
/// Componente helper para pasar datos al GameManager tras su creación.
/// Se auto-destruye tras asignar los datos.
/// </summary>
public class BootstrapDataLink : MonoBehaviour
{
    public RunProgressSO runProgress;
    public SpawnConfigSO spawnConfig;

    private void Start()
    {
        var gm = GetComponent<GameManager>();
        if (gm != null)
        {
            var type = gm.GetType();
            var fieldRun = type.GetField("runProgress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fieldSpawn = type.GetField("spawnConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (fieldRun != null && runProgress != null) fieldRun.SetValue(gm, runProgress);
            if (fieldSpawn != null && spawnConfig != null) fieldSpawn.SetValue(gm, spawnConfig);
        }
        Destroy(this);
    }
}

/// <summary>
/// Componente helper para pasar el pool al UpgradeManager.
/// Se auto-destruye tras asignar el pool.
/// </summary>
public class BootstrapUpgradeLink : MonoBehaviour
{
    public UpgradePoolSO pool;

    private void Start()
    {
        var um = GetComponent<UpgradeManager>();
        if (um != null && pool != null)
        {
            var field = um.GetType().GetField("pool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(um, pool);
        }
        Destroy(this);
    }
}

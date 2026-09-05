using UnityEngine;

/// <summary>
/// Tremor de cámara por "trauma": los golpes suman intensidad, decae con el
/// tiempo. Ya no mueve la cámara directamente: expone DesplazamientoActual
/// para que CamaraSeguimiento lo sume después de clampar la posición.
/// </summary>
public class CamaraTremor : MonoBehaviour
{
    private static CamaraTremor _instancia;
    private float _trauma;
    private float _semilla;

    public static void Agregar(float intensidad)
    {
        if (_instancia == null) return;
        _instancia._trauma = Mathf.Clamp01(_instancia._trauma + intensidad);
    }

    /// <summary>Desplazamiento actual (lo consume CamaraSeguimiento).</summary>
    public static Vector2 DesplazamientoActual
    {
        get
        {
            if (_instancia == null || _instancia._trauma <= 0.001f) return Vector2.zero;
            float magnitud = _instancia._trauma * _instancia._trauma * 0.45f;
            float t = Time.time * 35f + _instancia._semilla;
            return new Vector2(
                Mathf.PerlinNoise(t, 0.37f) - 0.5f,
                Mathf.PerlinNoise(0.71f, t) - 0.5f) * 2f * magnitud;
        }
    }

    /// <summary>Lo llama GameManager / la herramienta para instalarlo en la cámara.</summary>
    public static void InstalarEn(Camera camara)
    {
        if (camara == null || _instancia != null) return;
        _instancia = camara.gameObject.AddComponent<CamaraTremor>();
    }

    private void Awake()
    {
        _instancia = this;
        _semilla = Random.value * 100f;
    }

    private void OnDestroy()
    {
        if (_instancia == this) _instancia = null;
    }

    private void Update()
    {
        if (_trauma > 0.001f)
        {
            _trauma = Mathf.Max(0f, _trauma - Time.deltaTime * 1.5f);
        }
    }
}


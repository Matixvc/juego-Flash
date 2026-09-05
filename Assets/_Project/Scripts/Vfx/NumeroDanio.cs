using UnityEngine;

/// <summary>
/// Números de daño flotantes ("−1") que suben y se desvanecen.
/// Usa TextMesh con la fuente built-in de Unity: no requiere Canvas ni UI.
/// </summary>
public static class NumeroDanio
{
    private static Font _fuente;

    private static Font Fuente
    {
        get
        {
            if (_fuente == null)
            {
                try { _fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
                catch { _fuente = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
            }
            return _fuente;
        }
    }

    public static void Crear(Vector3 pos, int cantidad, Color color)
    {
        Font fuente = Fuente;
        if (fuente == null || cantidad <= 0) return;

        GameObject go = new GameObject("NumeroDanio");
        go.transform.position = pos + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.15f, 0.45f), 0f);

        TextMesh tm = go.AddComponent<TextMesh>();
        tm.text = "-" + cantidad;
        tm.font = fuente;
        tm.fontSize = 48;
        tm.characterSize = 0.16f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null && fuente.material != null)
        {
            mr.sharedMaterial = fuente.material;
            mr.sortingOrder = 20;
        }

        go.AddComponent<VfxNumero>().Iniciar(color);
    }
}

/// <summary>Animación del número: sube y se desvanece.</summary>
public class VfxNumero : MonoBehaviour
{
    private const float Duracion = 0.65f;
    private const float Altura = 0.9f;

    private TextMesh _tm;
    private Color _color;
    private float _t;
    private Vector3 _pos;

    public void Iniciar(Color color)
    {
        _tm = GetComponent<TextMesh>();
        _color = color;
        _pos = transform.position;
    }

    private void Update()
    {
        _t += Time.deltaTime;
        float p = Mathf.Clamp01(_t / Duracion);

        _pos += Vector3.up * (Altura * Time.deltaTime / Duracion);
        transform.position = _pos;

        if (_tm != null)
        {
            Color c = _color;
            c.a = 1f - p;
            _tm.color = c;
        }

        if (p >= 1f) Destroy(gameObject);
    }
}

using UnityEngine;

/// <summary>
/// Utilidades de efectos visuales generados 100% por código (sin assets):
/// anillos de choque, destellos y chispas. Usa el shader Sprites/Default,
/// que ya está en el proyecto por los SpriteRenderers normales.
/// </summary>
public static class VfxUtil
{
    private static Material _material;
    public static Material MaterialSprites
    {
        get
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                _material = new Material(shader);
            }
            return _material;
        }
    }

    private static Sprite _anillo;
    public static Sprite SpriteAnillo
    {
        get
        {
            if (_anillo == null) _anillo = GenerarSprite(96, true);
            return _anillo;
        }
    }

    private static Sprite _destello;
    public static Sprite SpriteDestello
    {
        get
        {
            if (_destello == null) _destello = GenerarSprite(64, false);
            return _destello;
        }
    }

    /// <summary>Anillo expansivo (onda de choque del pisotón).</summary>
    public static void AnilloChoque(Vector2 pos, float radioFinal, Color color, float duracion = 0.35f)
    {
        VfxPool.ObtenerAnillo(pos, radioFinal, color, duracion);
    }

    /// <summary>Destello suave (muerte de enemigos, impactos).</summary>
    public static void Destello(Vector2 pos, float tamano, Color color, float duracion = 0.25f)
    {
        VfxPool.ObtenerAnillo(pos, tamano * 0.5f, color, duracion);
    }

    /// <summary>Chispas que saltan y se apagan (escombros).</summary>
    public static void Chispas(Vector2 pos, Color color, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            float angulo = Random.Range(0f, Mathf.PI * 2f);
            float fuerza = Random.Range(2.5f, 6.5f);
            Vector2 vel = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * fuerza;
            float vida = Random.Range(0.25f, 0.5f);
            VfxPool.ObtenerChispa(pos, color, vel, vida);
        }
    }

    private static Sprite GenerarSprite(int tam, bool anillo)
    {
        Texture2D tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Color[] pix = new Color[tam * tam];
        Vector2 centro = new Vector2(tam * 0.5f, tam * 0.5f);
        float radio = tam * 0.5f;

        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), centro) / radio;
                Color c = Color.clear;

                if (anillo)
                {
                    if (d > 0.78f && d <= 1f) c = Color.white;
                    if (d > 0.70f && d <= 0.78f) c = new Color(1f, 1f, 1f, (d - 0.70f) / 0.08f);
                }
                else
                {
                    float a = Mathf.Clamp01(1f - d);
                    c = new Color(1f, 1f, 1f, a * a);
                }

                pix[y * tam + x] = c;
            }
        }

        tex.SetPixels(pix);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tam, tam), new Vector2(0.5f, 0.5f), tam / 2f);
    }
}

/// <summary>Animación de escala + desvanecido para anillos y destellos.</summary>
public class VfxAnimado : MonoBehaviour
{
    private SpriteRenderer _sr;
    private float _escalaFinal;
    private float _duracion;
    private bool _esAnillo;
    private float _t;
    private Color _colorInicial;

    public void Configurar(float escalaFinal, float duracion, bool esAnillo)
    {
        _sr = GetComponent<SpriteRenderer>();
        _escalaFinal = Mathf.Max(0.1f, escalaFinal);
        _duracion = Mathf.Max(0.05f, duracion);
        _esAnillo = esAnillo;
        _colorInicial = _sr != null ? _sr.color : Color.white;
        transform.localScale = Vector3.one * (_esAnillo ? 0.2f : 0.3f);
    }

    private void Update()
    {
        _t += Time.deltaTime;
        float p = Mathf.Clamp01(_t / _duracion);

        float escala = _esAnillo
            ? Mathf.Lerp(0.2f, _escalaFinal, 1f - (1f - p) * (1f - p))
            : Mathf.Lerp(0.3f, _escalaFinal, p);
        transform.localScale = Vector3.one * escala;

        if (_sr != null)
        {
            Color c = _colorInicial;
            c.a = _colorInicial.a * (1f - p);
            _sr.color = c;
        }

        if (p >= 1f) Destroy(gameObject);
    }
}

/// <summary>Chispa individual con velocidad y vida corta.</summary>
public class VfxChispa : MonoBehaviour
{
    public Vector2 Velocidad;
    public float Vida = 0.35f;

    private SpriteRenderer _sr;
    private Vector2 _pos;
    private float _t;
    private Color _colorInicial;

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _pos = transform.position;
        _colorInicial = _sr != null ? _sr.color : Color.white;
    }

    private void Update()
    {
        _t += Time.deltaTime;
        float p = _t / Vida;

        _pos += Velocidad * Time.deltaTime;
        Velocidad *= 1f - 3f * Time.deltaTime;
        transform.position = _pos;
        transform.localScale *= 1f - 2.5f * Time.deltaTime;

        if (_sr != null)
        {
            Color c = _colorInicial;
            c.a = _colorInicial.a * Mathf.Clamp01(1f - p);
            _sr.color = c;
        }

        if (p >= 1f) Destroy(gameObject);
    }
}

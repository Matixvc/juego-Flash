using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

/// <summary>
/// Orbe de experiencia que sueltan los enemigos al morir.
/// Espera un instante y luego es atraído magneticamente hacia la Roca.
/// Al tocarla suma experiencia a la RunProgress actual.
/// Usa object pooling para mejorar rendimiento.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ExpOrbe : MonoBehaviour
{
    [Header("Magnet")]
    [SerializeField] private float retrasoActivacion = 0.25f;
    [SerializeField] private float aceleracionMagnet = 14f;
    [SerializeField] private float velocidadMaxMagnet = 9f;

    [Header("Valor")]
    [SerializeField] private int expOtorgada = 1;

    private Transform jugador;
    private Vector2 velocidadMagnet;
    private float tiempoVivo;
    private bool recogido;
    private ObjectPool<GameObject> _pool;
    private bool _estaEnPool;
    private CircleCollider2D _collider;
    private SpriteRenderer _spriteRenderer;

    private static ObjectPool<GameObject> _poolOrbes;
    private static bool _poolInicializado;
    private static bool _sceneCleanupRegistrado;

    public static void InicializarPool()
    {
        if (_poolInicializado) return;
        _poolInicializado = true;

        _poolOrbes = new ObjectPool<GameObject>(
            createFunc: CrearOrbe,
            actionOnGet: go =>
            {
                go.SetActive(true);
                ExpOrbe orbe = go.GetComponent<ExpOrbe>();
                if (orbe != null) orbe._estaEnPool = true;
            },
            actionOnRelease: go =>
            {
                go.SetActive(false);
                ExpOrbe orbe = go.GetComponent<ExpOrbe>();
                if (orbe != null) orbe.ResetearOrbe();
            },
            actionOnDestroy: go => Object.Destroy(go),
            defaultCapacity: 30,
            maxSize: 100
        );

        // Registrar limpieza al cambiar de escena
        if (!_sceneCleanupRegistrado)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            _sceneCleanupRegistrado = true;
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LimpiarPool();
    }

    private static void LimpiarPool()
    {
        if (_poolOrbes != null)
        {
            _poolOrbes.Clear();
        }
    }

    private static GameObject CrearOrbe()
    {
        GameObject go = new GameObject("ExpOrbe_Pooled");
        go.SetActive(false);
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        // Crear un sprite básico para el orbe si no hay prefab
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateDefaultOrbeSprite();
        sr.sortingOrder = 2;

        go.AddComponent<ExpOrbe>();
        return go;
    }

    private static Sprite CreateDefaultOrbeSprite()
    {
        Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[32 * 32];
        Vector2 center = new Vector2(16f, 16f);
        float radius = 14f;

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                Color c = Color.clear;
                if (dist < radius)
                {
                    float alpha = Mathf.Clamp01(1f - (dist / radius));
                    c = new Color(0.6f, 0.8f, 1f, alpha);
                }
                pixels[y * 32 + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16f);
    }

    public static GameObject ObtenerOrbe(Vector2 posicion, int expValor)
    {
        InicializarPool();
        GameObject go = _poolOrbes.Get();
        go.transform.position = posicion;

        ExpOrbe orbe = go.GetComponent<ExpOrbe>();
        if (orbe != null)
        {
            orbe.ConfigurarValor(expValor);
            orbe._pool = _poolOrbes;
        }

        return go;
    }

    private void ResetearOrbe()
    {
        tiempoVivo = 0f;
        velocidadMagnet = Vector2.zero;
        recogido = false;
        _estaEnPool = false;
        _pool = null;
    }

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        PlayerController2D player = FindAnyObjectByType<PlayerController2D>();
        if (player != null)
        {
            jugador = player.transform;
        }
    }

    private void Update()
    {
        if (recogido || jugador == null)
        {
            return;
        }

        tiempoVivo += Time.deltaTime;
        if (tiempoVivo < retrasoActivacion)
        {
            return;
        }

        Vector2 posicion = transform.position;
        Vector2 posicionJugador = jugador.position;
        float radioMagnet = RadioMagnetDelJugador();

        if ((posicionJugador - posicion).sqrMagnitude > radioMagnet * radioMagnet)
        {
            return;
        }

        Vector2 direccion = (posicionJugador - posicion).normalized;
        velocidadMagnet += direccion * (aceleracionMagnet * Time.deltaTime);

        if (velocidadMagnet.magnitude > velocidadMaxMagnet)
        {
            velocidadMagnet = velocidadMagnet.normalized * velocidadMaxMagnet;
        }

        transform.position += (Vector3)(velocidadMagnet * Time.deltaTime);
    }

    private float RadioMagnetDelJugador()
    {
        if (jugador == null)
        {
            return 2.5f;
        }

        PlayerRuntimeStats stats = jugador.GetComponent<PlayerRuntimeStats>();
        return stats != null ? stats.RadioMagnetOrbes : 2.5f;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (recogido || !col.TryGetComponent(out PlayerController2D _))
        {
            return;
        }

        Recoger();
    }

    /// <summary>Configura cuanta exp otorga este orbe (lo usa el sistema de drops).</summary>
    public void ConfigurarValor(int cantidad)
    {
        expOtorgada = Mathf.Max(1, cantidad);
    }

    private void Recoger()
    {
        recogido = true;

        RunProgressSO run = GameManager.Instancia != null ? GameManager.Instancia.RunProgress : null;
        if (run != null)
        {
            run.SumarExp(expOtorgada);
        }

        // Devolver al pool en lugar de destruir
        if (_estaEnPool && _pool != null)
        {
            _pool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_estaEnPool && _pool != null)
        {
            _pool.Release(gameObject);
        }
    }
}
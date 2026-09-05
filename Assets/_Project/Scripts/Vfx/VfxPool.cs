using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Pool de VFX para evitar Instantiate/Destroy constante.
/// Usa Unity 2021+ ObjectPool para gestion automatica.
/// </summary>
public static class VfxPool
{
    private static ObjectPool<GameObject> _poolAnillos;
    private static ObjectPool<GameObject> _poolChispas;

    private static bool _inicializado;

    public static void Inicializar()
    {
        if (_inicializado) return;
        _inicializado = true;

        _poolAnillos = new ObjectPool<GameObject>(
            createFunc: CrearAnillo,
            actionOnGet: go => go.SetActive(true),
            actionOnRelease: go =>
            {
                go.SetActive(false);
                var anim = go.GetComponent<VfxAnimado>();
                if (anim != null) DetenerAnimacion(anim);
            },
            actionOnDestroy: go => Object.Destroy(go),
            defaultCapacity: 10,
            maxSize: 30
        );

        _poolChispas = new ObjectPool<GameObject>(
            createFunc: CrearChispa,
            actionOnGet: go => go.SetActive(true),
            actionOnRelease: go => go.SetActive(false),
            actionOnDestroy: go => Object.Destroy(go),
            defaultCapacity: 20,
            maxSize: 50
        );
    }

    private static GameObject CrearAnillo()
    {
        GameObject go = new GameObject("VfxAnillo_Pooled");
        go.SetActive(false);
        return go;
    }

    private static GameObject CrearChispa()
    {
        GameObject go = new GameObject("VfxChispa_Pooled");
        go.SetActive(false);
        return go;
    }

    private static void DetenerAnimacion(VfxAnimado anim)
    {
        // Reset scale
        anim.transform.localScale = Vector3.one;
    }

    public static GameObject ObtenerAnillo(Vector2 pos, float radioFinal, Color color, float duracion)
    {
        Inicializar();
        GameObject go = _poolAnillos.Get();
        go.transform.position = pos;

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VfxUtil.SpriteAnillo;
            sr.material = VfxUtil.MaterialSprites;
            sr.sortingOrder = 4;
        }
        sr.color = color;
        sr.enabled = true;

        VfxAnimado anim = go.GetComponent<VfxAnimado>();
        if (anim == null) anim = go.AddComponent<VfxAnimado>();
        anim.Configurar(radioFinal * 2f, duracion, true);

        // Auto-release cuando termine la animacion
        var releaser = go.GetComponent<VfxPoolReleaser>();
        if (releaser == null) releaser = go.AddComponent<VfxPoolReleaser>();
        releaser.Configurar(_poolAnillos, duracion + 0.1f);

        return go;
    }

    public static GameObject ObtenerChispa(Vector2 pos, Color color, Vector2 velocidad, float vida)
    {
        Inicializar();
        GameObject go = _poolChispas.Get();
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VfxUtil.SpriteDestello;
            sr.material = VfxUtil.MaterialSprites;
            sr.sortingOrder = 4;
        }
        sr.color = color;
        sr.enabled = true;

        VfxChispa chispa = go.GetComponent<VfxChispa>();
        if (chispa == null) chispa = go.AddComponent<VfxChispa>();
        chispa.Velocidad = velocidad;
        chispa.Vida = vida;

        var releaser = go.GetComponent<VfxPoolReleaser>();
        if (releaser == null) releaser = go.AddComponent<VfxPoolReleaser>();
        releaser.Configurar(_poolChispas, vida + 0.1f);

        return go;
    }

    public static void Liberar(GameObject go, ObjectPool<GameObject> pool)
    {
        if (go != null && pool != null)
        {
            pool.Release(go);
        }
    }
}

/// <summary>
/// Componente que libera automaticamente el objeto al pool tras un tiempo.
/// </summary>
public class VfxPoolReleaser : MonoBehaviour
{
    private ObjectPool<GameObject> _pool;
    private float _tiempo;
    private float _t;

    public void Configurar(ObjectPool<GameObject> pool, float delay)
    {
        _pool = pool;
        _tiempo = delay;
        _t = 0f;
    }

    private void Update()
    {
        _t += Time.deltaTime;
        if (_t >= _tiempo)
        {
            VfxPool.Liberar(gameObject, _pool);
        }
    }
}

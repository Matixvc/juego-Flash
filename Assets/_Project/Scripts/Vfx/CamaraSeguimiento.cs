using UnityEngine;

/// <summary>
/// Cámara con seguimiento suave de la Roca, limitada a los bordes de la
/// arena y mezclando el tremor de CamaraTremor. Se auto-instala desde
/// GameManager o desde la herramienta de armado de escena.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CamaraSeguimiento : MonoBehaviour
{
    [SerializeField] private float suavizado = 7f;
    [Tooltip("Mitad del área jugable interior de la arena.")]
    [SerializeField] private Vector2 mitadArena = new Vector2(22f, 13f);

    private Camera _cam;
    private Transform _objetivo;
    private float _z;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _z = transform.position.z;
    }

    private void LateUpdate()
    {
        if (_objetivo == null)
        {
            PlayerController2D jugador = FindAnyObjectByType<PlayerController2D>();
            if (jugador == null) return;
            _objetivo = jugador.transform;
        }

        Vector3 deseado = new Vector3(_objetivo.position.x, _objetivo.position.y, _z);
        Vector3 nueva = transform.position;
        float t = 1f - Mathf.Exp(-suavizado * Time.deltaTime);
        nueva = Vector3.Lerp(nueva, deseado, t);

        // Limitar el centro de la cámara a la arena (según el aspecto actual)
        float mitadAlto = _cam.orthographicSize;
        float mitadAncho = mitadAlto * _cam.aspect;
        float minX = -mitadArena.x + mitadAncho;
        float maxX = mitadArena.x - mitadAncho;
        float minY = -mitadArena.y + mitadAlto;
        float maxY = mitadArena.y - mitadAlto;

        nueva.x = minX > maxX ? 0f : Mathf.Clamp(nueva.x, minX, maxX);
        nueva.y = minY > maxY ? 0f : Mathf.Clamp(nueva.y, minY, maxY);

        // Tremor de los golpes, sumado después del clamp
        nueva += (Vector3)CamaraTremor.DesplazamientoActual;
        nueva.z = _z;
        transform.position = nueva;
    }
}

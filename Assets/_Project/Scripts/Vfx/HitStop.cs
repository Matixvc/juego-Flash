using System.Collections;
using UnityEngine;

/// <summary>
/// Hit-stop: congela brevemente el tiempo (40-60 ms) en golpes fuertes
/// para dar sensación de impacto. Restauración en tiempo real, segura
/// con la pausa del GameManager.
/// </summary>
public class HitStop : MonoBehaviour
{
    private static HitStop _instancia;
    private bool _activo;
    private float _timeScalePrevio = 1f;
    private float _timeScaleDurante = 0.05f;

    public static void Golpear(float duracion = 0.045f, float escala = 0.05f)
    {
        if (_instancia == null)
        {
            _instancia = new GameObject("HitStop").AddComponent<HitStop>();
        }

        if (_instancia._activo) return;

        _instancia._activo = true;
        _instancia._timeScalePrevio = Time.timeScale;
        _instancia._timeScaleDurante = escala;
        Time.timeScale = escala;
        _instancia.StartCoroutine(_instancia.Restaurar(duracion));
    }

    private IEnumerator Restaurar(float duracion)
    {
        yield return new WaitForSecondsRealtime(duracion);
        // Si otro sistema pausó el juego durante el hit-stop, respetar esa pausa.
        if (Mathf.Approximately(Time.timeScale, _timeScaleDurante))
        {
            Time.timeScale = _timeScalePrevio;
        }
        _activo = false;
    }
}

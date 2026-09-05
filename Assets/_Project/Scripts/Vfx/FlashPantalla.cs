using UnityEngine;

/// <summary>
/// Flash rojo en los bordes de la pantalla cuando la Roca recibe daño.
/// Se auto-crea y dibuja con OnGUI (resistente a la pausa y al hit-stop).
/// </summary>
public class FlashPantalla : MonoBehaviour
{
    private static FlashPantalla _instancia;
    private float _intensidad;

    public static void Golpe(float intensidad = 0.6f)
    {
        if (_instancia == null)
        {
            _instancia = new GameObject("FlashPantalla").AddComponent<FlashPantalla>();
        }
        _instancia._intensidad = Mathf.Max(_instancia._intensidad, intensidad);
    }

    private void Update()
    {
        if (_intensidad > 0f)
        {
            _intensidad = Mathf.Max(0f, _intensidad - Time.unscaledDeltaTime * 2.2f);
        }
    }

    private void OnGUI()
    {
        if (_intensidad <= 0.01f) return;

        Color previo = GUI.color;
        GUI.color = new Color(1f, 0.12f, 0.12f, _intensidad * 0.4f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
        GUI.color = previo;
    }
}

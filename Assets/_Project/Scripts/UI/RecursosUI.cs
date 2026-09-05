using UnityEngine;

/// <summary>
/// Recursos compartidos para la UI: fuente y sprite blanco.
/// </summary>
public static class RecursosUI
{
    private static Font _fuente;
    public static Font FuenteUI
    {
        get
        {
            if (_fuente == null)
            {
                _fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            return _fuente;
        }
    }

    private static Sprite _spriteBlanco;
    public static Sprite SpriteBlanco
    {
        get
        {
            if (_spriteBlanco == null)
            {
                Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[16];
                for (int i = 0; i < 16; i++) pixels[i] = Color.white;
                tex.SetPixels(pixels);
                tex.Apply();
                _spriteBlanco = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }
            return _spriteBlanco;
        }
    }
}

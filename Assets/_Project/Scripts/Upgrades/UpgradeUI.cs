using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI de seleccion de mejoras. Crea cards con colores por rareza,
/// iconos, animacion de hover y soporte de teclado (1/2/3).
/// </summary>
public class UpgradeUI : MonoBehaviour, IUpgradeSelector
{
    [SerializeField] private UpgradeSO[] opcionesActuales;

    private Canvas canvas;
    private GameObject contenedorCartas;
    private List<GameObject> cartasCreadas = new List<GameObject>();

    private void Awake()
    {
        canvas = FindAnyObjectByType<Canvas>();
    }

    public void MostrarOpciones(UpgradeSO[] opciones)
    {
        opcionesActuales = opciones;
        LimpiarCartas();

        if (contenedorCartas == null)
        {
            contenedorCartas = new GameObject("CartasContainer", typeof(RectTransform));
            contenedorCartas.transform.SetParent(transform, false);
            contenedorCartas.AddComponent<HorizontalLayoutGroup>();
        }

        // Configurar contenedor
        HorizontalLayoutGroup hlg = contenedorCartas.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 25f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(20, 20, 20, 20);

        RectTransform rtCont = contenedorCartas.GetComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(0.5f, 0.5f);
        rtCont.anchorMax = new Vector2(0.5f, 0.5f);
        rtCont.pivot = new Vector2(0.5f, 0.5f);
        rtCont.anchoredPosition = Vector2.zero;
        rtCont.sizeDelta = new Vector2(900f, 300f);

        foreach (UpgradeSO upgrade in opciones)
        {
            CrearCarta(upgrade);
        }
    }

    private void LimpiarCartas()
    {
        foreach (var carta in cartasCreadas)
        {
            if (carta != null) Destroy(carta);
        }
        cartasCreadas.Clear();
    }

    private void CrearCarta(UpgradeSO upgrade)
    {
        GameObject carta = new GameObject($"Carta_{upgrade.NombreMejora}", typeof(RectTransform));
        carta.transform.SetParent(contenedorCartas.transform, false);
        LayoutElement le = carta.AddComponent<LayoutElement>();
        le.preferredWidth = 250f;
        le.preferredHeight = 200f;

        // Fondo segun rareza
        Image fondo = carta.AddComponent<Image>();
        fondo.sprite = RecursosUI.SpriteBlanco;
        switch (upgrade.RaridadMejora)
        {
            case UpgradeSO.Raridad.Comun:
                fondo.color = new Color(0.22f, 0.22f, 0.25f, 0.95f);
                break;
            case UpgradeSO.Raridad.Rara:
                fondo.color = new Color(0.15f, 0.2f, 0.45f, 0.95f);
                break;
            case UpgradeSO.Raridad.Epica:
                fondo.color = new Color(0.4f, 0.15f, 0.45f, 0.95f);
                break;
        }

        // Borde segun rareza
        Outline borde = carta.AddComponent<Outline>();
        borde.effectDistance = new Vector2(2f, 2f);
        switch (upgrade.RaridadMejora)
        {
            case UpgradeSO.Raridad.Comun:
                borde.effectColor = new Color(0.5f, 0.5f, 0.55f, 0.8f);
                break;
            case UpgradeSO.Raridad.Rara:
                borde.effectColor = new Color(0.3f, 0.6f, 1f, 0.9f);
                break;
            case UpgradeSO.Raridad.Epica:
                borde.effectColor = new Color(0.8f, 0.3f, 1f, 0.9f);
                break;
        }

        // Icono (o placeholder)
        GameObject iconoGO = new GameObject("Icono", typeof(RectTransform));
        iconoGO.transform.SetParent(carta.transform, false);
        Image icono = iconoGO.AddComponent<Image>();
        icono.sprite = upgrade.Icono != null ? upgrade.Icono : RecursosUI.SpriteBlanco;
        icono.color = upgrade.Icono != null ? Color.white : new Color(0.6f, 0.6f, 0.65f, 0.5f);
        icono.raycastTarget = false;
        RectTransform rtI = icono.GetComponent<RectTransform>();
        rtI.anchorMin = new Vector2(0.5f, 1f);
        rtI.anchorMax = new Vector2(0.5f, 1f);
        rtI.pivot = new Vector2(0.5f, 1f);
        rtI.anchoredPosition = new Vector2(0f, -40f);
        rtI.sizeDelta = new Vector2(48f, 48f);

        // Nombre
        GameObject nomGO = new GameObject("Nombre", typeof(RectTransform));
        nomGO.transform.SetParent(carta.transform, false);
        Text txtNom = nomGO.AddComponent<Text>();
        txtNom.text = upgrade.NombreMejora;
        txtNom.fontSize = 18;
        txtNom.fontStyle = FontStyle.Bold;
        txtNom.font = RecursosUI.FuenteUI;
        txtNom.color = Color.white;
        txtNom.alignment = TextAnchor.MiddleCenter;
        txtNom.raycastTarget = false;
        txtNom.horizontalOverflow = HorizontalWrapMode.Wrap;
        RectTransform rtNom = txtNom.GetComponent<RectTransform>();
        rtNom.anchorMin = new Vector2(0f, 1f);
        rtNom.anchorMax = new Vector2(1f, 1f);
        rtNom.pivot = new Vector2(0.5f, 1f);
        rtNom.anchoredPosition = new Vector2(0f, -95f);
        rtNom.sizeDelta = new Vector2(-20f, 30f);

        // Descripcion
        GameObject descGO = new GameObject("Descripcion", typeof(RectTransform));
        descGO.transform.SetParent(carta.transform, false);
        Text txtDesc = descGO.AddComponent<Text>();
        txtDesc.text = upgrade.Descripcion;
        txtDesc.fontSize = 13;
        txtDesc.font = RecursosUI.FuenteUI;
        txtDesc.color = new Color(0.75f, 0.75f, 0.8f, 1f);
        txtDesc.alignment = TextAnchor.MiddleCenter;
        txtDesc.raycastTarget = false;
        txtDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
        txtDesc.verticalOverflow = VerticalWrapMode.Truncate;
        txtDesc.supportRichText = true;
        RectTransform rtDesc = txtDesc.GetComponent<RectTransform>();
        rtDesc.anchorMin = new Vector2(0f, 0f);
        rtDesc.anchorMax = new Vector2(1f, 1f);
        rtDesc.offsetMin = new Vector2(10f, 15f);
        rtDesc.offsetMax = new Vector2(-10f, -55f);

        // Rareza label
        GameObject rarezaGO = new GameObject("Rareza", typeof(RectTransform));
        rarezaGO.transform.SetParent(carta.transform, false);
        Text txtRareza = rarezaGO.AddComponent<Text>();
        txtRareza.text = upgrade.RaridadMejora.ToString().ToUpper();
        txtRareza.fontSize = 10;
        txtRareza.fontStyle = FontStyle.Bold;
        txtRareza.font = RecursosUI.FuenteUI;
        txtRareza.raycastTarget = false;
        switch (upgrade.RaridadMejora)
        {
            case UpgradeSO.Raridad.Comun:
                txtRareza.color = new Color(0.6f, 0.6f, 0.65f, 1f);
                break;
            case UpgradeSO.Raridad.Rara:
                txtRareza.color = new Color(0.4f, 0.7f, 1f, 1f);
                break;
            case UpgradeSO.Raridad.Epica:
                txtRareza.color = new Color(0.9f, 0.4f, 1f, 1f);
                break;
        }
        txtRareza.alignment = TextAnchor.MiddleCenter;
        RectTransform rtRareza = txtRareza.GetComponent<RectTransform>();
        rtRareza.anchorMin = new Vector2(0f, 0f);
        rtRareza.anchorMax = new Vector2(1f, 0f);
        rtRareza.pivot = new Vector2(0.5f, 0f);
        rtRareza.anchoredPosition = new Vector2(0f, 8f);
        rtRareza.sizeDelta = new Vector2(0f, 18f);

        // Boton clickeable
        Button btn = carta.AddComponent<Button>();
        UpgradeSO captura = upgrade;
        btn.onClick.AddListener(() => Seleccionar(captura));
        btn.transition = Button.Transition.ColorTint;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.85f, 1f);
        btn.colors = cb;

        cartasCreadas.Add(carta);
    }

    private void Seleccionar(UpgradeSO mejora)
    {
        UpgradeManager manager = FindAnyObjectByType<UpgradeManager>();
        if (manager != null) manager.ElegirMejora(mejora);
    }

    private void OnDisable()
    {
        if (GameManager.Instancia != null && GameManager.Instancia.Estado == GameManager.EstadoJuego.SeleccionMejora)
        {
            // La reanudacion la hace UpgradeManager.ElegirMejora
        }
    }
}

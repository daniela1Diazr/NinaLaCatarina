using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controla el menu de inicio, el marcador de distancia y la pantalla de reinicio.
// La interfaz (Canvas, textos y botones) se crea sola al darle Play: no hay que armarla a mano.
public class GestorJuego : MonoBehaviour
{
    public static GestorJuego Instancia { get; private set; }

    // Sobrevive al recargar la escena: asi, al reiniciar, no vuelve a salir el menu
    private static bool saltarMenuInicial = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void LimpiarEstatico()
    {
        saltarMenuInicial = false;
        Instancia = null;
    }

    private enum Estado { Menu, Jugando, Muerta, FinDelJuego }

    [Header("Texto")]
    public string tituloDelJuego = "Nina la Catarina";
    [Tooltip("Opcional: arrastra aqui una fuente (TTF) para cambiar la letra. Vacio = fuente por defecto.")]
    public Font fuente;

    [Header("Progreso")]
    [Tooltip("Cuantas unidades de Unity equivalen a 1 metro mostrado.")]
    public float unidadesPorMetro = 1f;

    private const string ClaveRecord = "RecordMetros";

    private Estado estado = Estado.Menu;
    private MovimientoNina movimiento;
    private Animator animator;
    private Camera camara;

    private float xInicial;
    private int metros;
    private int record;
    private bool esNuevoRecord;

    private GameObject panelMenu, panelHud, panelFin;
    private Text textoMetros, textoRecordMenu, textoFinMetros, textoFinRecord, textoNuevoRecord;

    void Awake()
    {
        Instancia = this;
        record = PlayerPrefs.GetInt(ClaveRecord, 0);
        ConstruirUI();
    }

    void Start()
    {
        camara = Camera.main;
        movimiento = FindFirstObjectByType<MovimientoNina>();

        if (camara == null || movimiento == null)
        {
            Debug.LogError("GestorJuego: no encuentro la Main Camera o a Nina (MovimientoNina).", this);
            enabled = false;
            return;
        }

        animator = movimiento.GetComponentInChildren<Animator>();

        if (saltarMenuInicial)
        {
            saltarMenuInicial = false;
            Jugar();
        }
        else
        {
            MostrarMenu();
        }
    }

    void Update()
    {
        switch (estado)
        {
            case Estado.Menu:
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    Jugar();
                break;

            case Estado.Jugando:
                float avance = camara.transform.position.x - xInicial;
                metros = Mathf.Max(0, Mathf.FloorToInt(avance / unidadesPorMetro));
                textoMetros.text = metros + " m";
                break;

            case Estado.FinDelJuego:
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.R))
                    Reiniciar();
                break;
        }
    }

    // ---------- Flujo del juego ----------

    void MostrarMenu()
    {
        estado = Estado.Menu;
        panelMenu.SetActive(true);
        panelHud.SetActive(false);
        panelFin.SetActive(false);
        textoRecordMenu.text = "Record: " + record + " m";

        // Nina y la camara quietas hasta pulsar Jugar
        movimiento.enabled = false;
        if (animator != null) animator.speed = 0f;
    }

    public void Jugar()
    {
        if (estado == Estado.Jugando) return;
        StartCoroutine(Comenzar());
    }

    IEnumerator Comenzar()
    {
        panelMenu.SetActive(false);
        panelFin.SetActive(false);
        panelHud.SetActive(true);
        textoMetros.text = "0 m";

        // Esperamos un frame para que el clic del boton no cuente como salto
        yield return null;

        xInicial = camara.transform.position.x;
        movimiento.enabled = true;
        if (animator != null) animator.speed = 1f;
        estado = Estado.Jugando;
    }

    // Lo llama MuerteNina en cuanto Nina muere
    public void AlMorir()
    {
        if (estado != Estado.Jugando) return;
        estado = Estado.Muerta;

        esNuevoRecord = metros > record;
        if (esNuevoRecord)
        {
            record = metros;
            PlayerPrefs.SetInt(ClaveRecord, record);
            PlayerPrefs.Save();
        }
    }

    // Lo llama MuerteNina cuando termina la animacion de muerte
    public void MostrarFinDeJuego()
    {
        if (estado != Estado.Muerta) return;
        estado = Estado.FinDelJuego;

        textoFinMetros.text = "Distancia: " + metros + " m";
        textoFinRecord.text = "Record: " + record + " m";
        textoNuevoRecord.gameObject.SetActive(esNuevoRecord);
        panelFin.SetActive(true);
    }

    public void Reiniciar()
    {
        int indice = SceneManager.GetActiveScene().buildIndex;
        if (indice < 0)
        {
            Debug.LogError("GestorJuego: la escena no esta en Build Profiles. Ve a File > Build Profiles > Scene List > Add Open Scenes.", this);
            return;
        }

        saltarMenuInicial = true;
        SceneManager.LoadScene(indice);
    }

    // ---------- Construccion de la interfaz ----------

    void ConstruirUI()
    {
        Font f = fuente != null ? fuente : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        GameObject canvasGO = new GameObject("Canvas_UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler escalador = canvasGO.GetComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920f, 1080f);
        escalador.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        escalador.matchWidthOrHeight = 0.5f;

        Transform raiz = canvasGO.transform;

        // --- Marcador (arriba a la izquierda) ---
        panelHud = NuevoRect("HUD", raiz).gameObject;
        textoMetros = CrearTexto(panelHud.transform, "0 m", 72, Vector2.zero, Color.white, f);
        RectTransform rm = textoMetros.rectTransform;
        rm.anchorMin = rm.anchorMax = rm.pivot = new Vector2(0f, 1f);
        rm.anchoredPosition = new Vector2(50f, -30f);
        textoMetros.alignment = TextAnchor.UpperLeft;

        // --- Menu de inicio ---
        panelMenu = CrearPanelCompleto("Menu", raiz);
        CrearTexto(panelMenu.transform, tituloDelJuego, 120, new Vector2(0f, 220f), Color.white, f);
        textoRecordMenu = CrearTexto(panelMenu.transform, "Record: 0 m", 52, new Vector2(0f, 80f), new Color(1f, 0.9f, 0.4f), f);
        CrearBoton(panelMenu.transform, "JUGAR", new Vector2(0f, -70f), Jugar, f);
        CrearTexto(panelMenu.transform, "Enter para jugar  |  Espacio, flecha arriba o clic para saltar", 36, new Vector2(0f, -220f), new Color(0.85f, 0.85f, 0.85f), f);

        // --- Pantalla de fin de juego ---
        panelFin = CrearPanelCompleto("FinDeJuego", raiz);
        CrearTexto(panelFin.transform, "Fin del juego", 110, new Vector2(0f, 250f), Color.white, f);
        textoFinMetros = CrearTexto(panelFin.transform, "Distancia: 0 m", 64, new Vector2(0f, 110f), Color.white, f);
        textoFinRecord = CrearTexto(panelFin.transform, "Record: 0 m", 52, new Vector2(0f, 30f), new Color(1f, 0.9f, 0.4f), f);
        textoNuevoRecord = CrearTexto(panelFin.transform, "Nuevo record!", 56, new Vector2(0f, -50f), new Color(0.5f, 1f, 0.5f), f);
        CrearBoton(panelFin.transform, "REINICIAR", new Vector2(0f, -170f), Reiniciar, f);
        CrearTexto(panelFin.transform, "Enter o R para reiniciar", 36, new Vector2(0f, -280f), new Color(0.85f, 0.85f, 0.85f), f);

        panelMenu.SetActive(false);
        panelHud.SetActive(false);
        panelFin.SetActive(false);
    }

    RectTransform NuevoRect(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        return r;
    }

    void Centrar(RectTransform r, Vector2 posicion, Vector2 tamano)
    {
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = posicion;
        r.sizeDelta = tamano;
    }

    GameObject CrearPanelCompleto(string nombre, Transform padre)
    {
        RectTransform r = NuevoRect(nombre, padre);
        Image fondo = r.gameObject.AddComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.6f);
        return r.gameObject;
    }

    Text CrearTexto(Transform padre, string contenido, int tamano, Vector2 posicion, Color color, Font f)
    {
        RectTransform r = NuevoRect("Texto", padre);
        Centrar(r, posicion, new Vector2(1600f, tamano * 1.5f));

        Text t = r.gameObject.AddComponent<Text>();
        t.font = f;
        t.text = contenido;
        t.fontSize = tamano;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;

        Outline borde = r.gameObject.AddComponent<Outline>();
        borde.effectColor = Color.black;
        borde.effectDistance = new Vector2(3f, -3f);

        return t;
    }

    Button CrearBoton(Transform padre, string contenido, Vector2 posicion, UnityAction accion, Font f)
    {
        RectTransform r = NuevoRect("Boton_" + contenido, padre);
        Centrar(r, posicion, new Vector2(480f, 130f));

        Image imagen = r.gameObject.AddComponent<Image>();
        imagen.color = new Color(0.85f, 0.2f, 0.2f, 1f);

        Button boton = r.gameObject.AddComponent<Button>();
        boton.targetGraphic = imagen;
        boton.onClick.AddListener(accion);

        ColorBlock colores = boton.colors;
        colores.highlightedColor = new Color(1f, 0.45f, 0.45f, 1f);
        colores.pressedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
        boton.colors = colores;

        CrearTexto(r, contenido, 56, Vector2.zero, Color.white, f);
        return boton;
    }
}

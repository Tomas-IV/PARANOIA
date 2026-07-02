using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ContadorBajasUI : MonoBehaviour
{
    // Esto crea un acceso directo global para que los zombis lo encuentren siempre
    public static ContadorBajasUI Instancia;

    private GameObject scoreCanvas;
    private Text textoScore;

    private int zombisMuertos = 0;
    private bool estaVisible = false;

    private void Awake()
    {
        // Configuramos el acceso directo al iniciar
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CrearInterfazTopRight();
        ActualizarMarcador();

        // Al arrancar la partida, el panel arranca OCULTO
        scoreCanvas.SetActive(false);
    }

    private void Update()
    {
        // Si tocás la H, se invierte el estado (si estaba oculto aparece, si estaba visible se va)
        if (Input.GetKeyDown(KeyCode.H))
        {
            estaVisible = !estaVisible;
            scoreCanvas.SetActive(estaVisible);
        }
    }

    // Esta función suma y se puede llamar desde cualquier lado
    public void RegistrarBaja()
    {
        zombisMuertos++;
        ActualizarMarcador();
    }

    private void ActualizarMarcador()
    {
        if (textoScore != null)
        {
            string miNombre = PhotonNetwork.LocalPlayer.NickName;
            if (string.IsNullOrEmpty(miNombre)) miNombre = "Jugador";

            // \n sirve para hacer un "Enter" y que quede en dos líneas prolijas
            textoScore.text = $"{miNombre}\nBajas: {zombisMuertos}";
        }
    }

    // --- CONSTRUCCIÓN AUTOMÁTICA DE UI ARRIBA A LA DERECHA ---
    private void CrearInterfazTopRight()
    {
        GameObject canvasObj = new GameObject("ScoreCanvas_Auto");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Lo ponemos bien al frente
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        scoreCanvas = canvasObj;

        GameObject fondoObj = new GameObject("FondoScore");
        fondoObj.transform.SetParent(canvasObj.transform, false);
        Image imagenFondo = fondoObj.AddComponent<Image>();
        imagenFondo.color = new Color(0, 0, 0, 0.85f);

        RectTransform rtFondo = fondoObj.GetComponent<RectTransform>();
        rtFondo.anchorMin = new Vector2(1f, 1f); // 1, 1 es Arriba a la Derecha
        rtFondo.anchorMax = new Vector2(1f, 1f);
        rtFondo.pivot = new Vector2(1f, 1f);

        // Lo hacemos un poco más alto para que entren las dos líneas de texto
        rtFondo.sizeDelta = new Vector2(200, 70);
        rtFondo.anchoredPosition = new Vector2(-20, -20);

        Font fuentePorDefecto = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject textObj = new GameObject("TextoBajas");
        textObj.transform.SetParent(fondoObj.transform, false);

        textoScore = textObj.AddComponent<Text>();
        textoScore.fontSize = 20;
        textoScore.font = fuentePorDefecto;
        textoScore.color = Color.yellow;
        textoScore.alignment = TextAnchor.MiddleCenter;

        RectTransform rtTexto = textObj.GetComponent<RectTransform>();
        rtTexto.anchorMin = new Vector2(0, 0);
        rtTexto.anchorMax = new Vector2(1, 1);
        rtTexto.sizeDelta = Vector2.zero;
        rtTexto.anchoredPosition = Vector2.zero;
    }
}
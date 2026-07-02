using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ContadorBajasUI : MonoBehaviour
{
    public static ContadorBajasUI Instancia;

    private GameObject scoreCanvas;
    private Text textoScore;

    // --- SISTEMA DE ENCRIPTACIÓN XOR (Basado en el PPT) ---
    // Nuestra clave secreta. ¡Nadie fuera del equipo de desarrollo debe saberla!
    private int claveSecreta = 8352;

    // Aquí guardamos el valor cifrado en memoria, NUNCA el valor real
    private int zombisMuertosCifrado;
    // ------------------------------------------------------

    private bool estaVisible = false;

    private void Awake()
    {
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
        // Inicializamos el contador en 0, pero ENCRIPTADO (0 ^ clave)
        zombisMuertosCifrado = 0 ^ claveSecreta;

        CrearInterfazTopRight();
        ActualizarMarcador();

        scoreCanvas.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            estaVisible = !estaVisible;
            scoreCanvas.SetActive(estaVisible);
        }
    }

    // Esta función suma una baja de forma segura
    public void RegistrarBaja()
    {
        // 1. DESENCRIPTAR: Aplicamos el operador XOR (^) para obtener el valor real temporalmente
        int valorReal = zombisMuertosCifrado ^ claveSecreta;

        // 2. SUMAR: Agregamos la nueva kill
        valorReal++;

        // 3. ENCRIPTAR: Volvemos a aplicar XOR y lo guardamos cifrado en memoria
        zombisMuertosCifrado = valorReal ^ claveSecreta;

        ActualizarMarcador();
    }

    private void ActualizarMarcador()
    {
        if (textoScore != null)
        {
            // Para mostrarlo en pantalla, necesitamos desencriptarlo primero
            int valorRealParaMostrar = zombisMuertosCifrado ^ claveSecreta;

            string miNombre = PhotonNetwork.LocalPlayer.NickName;
            if (string.IsNullOrEmpty(miNombre)) miNombre = "Jugador";

            // Mostramos el valor real, pero en la memoria RAM sigue estando el valor cifrado
            textoScore.text = $"{miNombre}\nBajas: {valorRealParaMostrar}";
        }
    }

    // --- CONSTRUCCIÓN AUTOMÁTICA DE UI ARRIBA A LA DERECHA ---
    private void CrearInterfazTopRight()
    {
        GameObject canvasObj = new GameObject("ScoreCanvas_Auto");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        scoreCanvas = canvasObj;

        GameObject fondoObj = new GameObject("FondoScore");
        fondoObj.transform.SetParent(canvasObj.transform, false);
        Image imagenFondo = fondoObj.AddComponent<Image>();
        imagenFondo.color = new Color(0, 0, 0, 0.85f);

        RectTransform rtFondo = fondoObj.GetComponent<RectTransform>();
        rtFondo.anchorMin = new Vector2(1f, 1f);
        rtFondo.anchorMax = new Vector2(1f, 1f);
        rtFondo.pivot = new Vector2(1f, 1f);

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
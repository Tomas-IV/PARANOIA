using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;

public class HighScoreAPI : MonoBehaviour
{
    private GameObject rankingCanvas;
    private Text textoCargando;
    private Text textoPosicion1;
    private Text textoPosicion2;

    [Header("Configuración de Red")]
    private const string URL_API = "https://jsonplaceholder.typicode.com/users";

    private bool estaVisible = false;

    private void Start()
    {
        CrearInterfazPorCodigo();
    }

    private void Update()
    {
        // La tecla H ahora funciona como interruptor (prende y apaga)
        if (Input.GetKeyDown(KeyCode.H))
        {
            estaVisible = !estaVisible;
            rankingCanvas.SetActive(estaVisible);

            // Solo llamamos a la API si el cartel se está mostrando
            if (estaVisible)
            {
                StartCoroutine(ObtenerHighScores());
            }
        }
    }

    private IEnumerator ObtenerHighScores()
    {
        textoCargando.gameObject.SetActive(true);
        textoPosicion1.text = "";
        textoPosicion2.text = "";
        textoCargando.text = "Cargando servidor...";

        using (UnityWebRequest req = UnityWebRequest.Get(URL_API))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al conectar: " + req.error);
                textoCargando.text = "Error de conexión.";
                yield break;
            }

            string json = req.downloadHandler.text;
            ZombiePlayerData[] listaTopJugadores = JsonConvert.DeserializeObject<ZombiePlayerData[]>(json);

            textoCargando.gameObject.SetActive(false);

            if (listaTopJugadores != null && listaTopJugadores.Length >= 2)
            {
                textoPosicion1.text = $"1. {listaTopJugadores[0].name} - {listaTopJugadores[0].id * 143} Bajas";
                textoPosicion2.text = $"2. {listaTopJugadores[1].name} - {listaTopJugadores[1].id * 92} Bajas";
            }
        }
    }

    // --- INTERFAZ ACOMODADA ARRIBA A LA DERECHA ---
    private void CrearInterfazPorCodigo()
    {
        GameObject canvasObj = new GameObject("RankingCanvas_Auto");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99; // Para que quede al mismo nivel que el otro
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        rankingCanvas = canvasObj;

        GameObject fondoObj = new GameObject("FondoOscuroRanking");
        fondoObj.transform.SetParent(canvasObj.transform, false);
        Image imagenFondo = fondoObj.AddComponent<Image>();
        imagenFondo.color = new Color(0, 0, 0, 0.85f);

        RectTransform rtFondo = fondoObj.GetComponent<RectTransform>();

        // ANCLAJE ARRIBA A LA DERECHA
        rtFondo.anchorMin = new Vector2(1f, 1f);
        rtFondo.anchorMax = new Vector2(1f, 1f);
        rtFondo.pivot = new Vector2(1f, 1f);

        // Lo hacemos más compacto para que encaje bien en la esquina
        rtFondo.sizeDelta = new Vector2(320, 90);

        // Lo posicionamos justo debajo del otro cartel (que medía 70 de alto y estaba en -20)
        // Dejamos un margen de 10px, así que lo ponemos en Y: -100
        rtFondo.anchoredPosition = new Vector2(-20, -100);

        Font fuentePorDefecto = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Textos más chicos y ajustados a la nueva cajita
        textoCargando = CrearTextoGenerico("TextoCargando", fondoObj.transform, "Presioná H...", 16, new Vector2(0, 0), fuentePorDefecto);

        textoPosicion1 = CrearTextoGenerico("TextoPosicion1", fondoObj.transform, "", 16, new Vector2(0, 20), fuentePorDefecto);
        textoPosicion1.color = Color.yellow;

        textoPosicion2 = CrearTextoGenerico("TextoPosicion2", fondoObj.transform, "", 16, new Vector2(0, -20), fuentePorDefecto);
        textoPosicion2.color = Color.white;

        // Arranca apagado
        rankingCanvas.SetActive(false);
    }

    private Text CrearTextoGenerico(string nombre, Transform padre, string textoInicial, int tamañoLetra, Vector2 posicion, Font fuente)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);

        Text textoClasico = go.AddComponent<Text>();
        textoClasico.text = textoInicial;
        textoClasico.fontSize = tamañoLetra;
        textoClasico.font = fuente;
        textoClasico.alignment = TextAnchor.MiddleCenter;

        textoClasico.horizontalOverflow = HorizontalWrapMode.Overflow;
        textoClasico.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 40);
        rt.anchoredPosition = posicion;

        return textoClasico;
    }
}

[System.Serializable]
public class ZombiePlayerData
{
    public int id;
    public string name;
}
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; // Usamos el clásico en vez de TMPro
using Newtonsoft.Json;

public class HighScoreAPI : MonoBehaviour
{
    // Variables internas usando el Text clásico de Unity
    private GameObject rankingCanvas;
    private Text textoCargando;
    private Text textoPosicion1;
    private Text textoPosicion2;

    [Header("Configuración de Red")]
    private const string URL_API = "https://jsonplaceholder.typicode.com/users";

    private void Start()
    {
        // LLAMAMOS A LA FUNCIÓN QUE CREA LA UI AUTOMÁTICAMENTE
        CrearInterfazPorCodigo();
    }

    private void Update()
    {
        // Al presionar la tecla H, consultamos el High Score en el servidor web
        if (Input.GetKeyDown(KeyCode.H))
        {
            StartCoroutine(ObtenerHighScores());
        }
    }

    private IEnumerator ObtenerHighScores()
    {
        textoCargando.gameObject.SetActive(true);
        textoCargando.text = "Conectando al servidor...";
        rankingCanvas.SetActive(true);

        using (UnityWebRequest req = UnityWebRequest.Get(URL_API))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al conectar con la API de HighScores: " + req.error);
                textoCargando.text = "Error al conectar.";
                yield break;
            }

            string json = req.downloadHandler.text;

            // Deserializamos el JSON usando el arreglo
            ZombiePlayerData[] listaTopJugadores = JsonConvert.DeserializeObject<ZombiePlayerData[]>(json);

            textoCargando.gameObject.SetActive(false);

            if (listaTopJugadores != null && listaTopJugadores.Length >= 2)
            {
                // Mostramos los datos de los mejores jugadores
                textoPosicion1.text = $"1. {listaTopJugadores[0].name} - {listaTopJugadores[0].id * 143} Zombis Muertos";
                textoPosicion2.text = $"2. {listaTopJugadores[1].name} - {listaTopJugadores[1].id * 92} Zombis Muertos";
            }
        }
    }

    // --- INTERFAZ CREADA 100% POR CÓDIGO CON UI CLÁSICA ---
    private void CrearInterfazPorCodigo()
    {
        // 1. Crear el Canvas Principal
        GameObject canvasObj = new GameObject("RankingCanvas_Auto");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        rankingCanvas = canvasObj;

        // 2. Crear un fondo oscuro para el Ranking
        GameObject fondoObj = new GameObject("FondoOscuro");
        fondoObj.transform.SetParent(canvasObj.transform, false);
        Image imagenFondo = fondoObj.AddComponent<Image>();
        imagenFondo.color = new Color(0, 0, 0, 0.85f); // Negro transparente

        RectTransform rtFondo = fondoObj.GetComponent<RectTransform>();
        rtFondo.anchorMin = new Vector2(0.5f, 0.5f);
        rtFondo.anchorMax = new Vector2(0.5f, 0.5f);
        rtFondo.sizeDelta = new Vector2(600, 400); // Tamaño de la ventana del ranking

        // Buscamos la fuente Arial nativa de Unity para evitar que el texto se rompa al crearlo de cero
        Font fuentePorDefecto = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 3. Crear los textos pasándoles la fuente
        textoCargando = CrearTextoGenerico("TextoCargando", fondoObj.transform, "Presioná H para cargar Ranking", 24, new Vector2(0, 100), fuentePorDefecto);

        textoPosicion1 = CrearTextoGenerico("TextoPosicion1", fondoObj.transform, "", 28, new Vector2(0, 20), fuentePorDefecto);
        textoPosicion1.color = Color.yellow; // Color oro para el primer puesto

        textoPosicion2 = CrearTextoGenerico("TextoPosicion2", fondoObj.transform, "", 24, new Vector2(0, -40), fuentePorDefecto);
        textoPosicion2.color = Color.white;

        // Ocultamos el Canvas hasta que se presione la H
        rankingCanvas.SetActive(false);
    }

    // Función auxiliar ajustada para Text clásico
    private Text CrearTextoGenerico(string nombre, Transform padre, string textoInicial, int tamañoLetra, Vector2 posicion, Font fuente)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);

        Text textoClasico = go.AddComponent<Text>();
        textoClasico.text = textoInicial;
        textoClasico.fontSize = tamañoLetra;
        textoClasico.font = fuente; // Asignamos la fuente vital
        textoClasico.alignment = TextAnchor.MiddleCenter;

        // Evita que las palabras largas desaparezcan si no entran en la caja
        textoClasico.horizontalOverflow = HorizontalWrapMode.Overflow;
        textoClasico.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(550, 50);
        rt.anchoredPosition = posicion;

        return textoClasico;
    }
}

// --- CLASE CONTENEDORA DE DATOS ---
[System.Serializable]
public class ZombiePlayerData
{
    public int id;
    public string name;
}
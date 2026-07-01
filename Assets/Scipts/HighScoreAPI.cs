using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.UI;

public class HighScoreAPI : MonoBehaviour
{
    // Variables internas para la UI generada por código
    private GameObject rankingCanvas;
    private TextMeshProUGUI textoCargando;
    private TextMeshProUGUI textoPosicion1;
    private TextMeshProUGUI textoPosicion2;

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
        textoCargando.text = "Conectando al servidor de puntuaciones...";
        rankingCanvas.SetActive(true);

        using (UnityWebRequest req = UnityWebRequest.Get(URL_API))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al conectar con la API de HighScores: " + req.error);
                textoCargando.text = "Error al cargar el ranking.";
                yield break;
            }

            string json = req.downloadHandler.text;

            // Deserializamos el JSON usando un arreglo (Array) como requiere tu planilla
            ZombiePlayerData[] listaTopJugadores = JsonConvert.DeserializeObject<ZombiePlayerData[]>(json);

            textoCargando.gameObject.SetActive(false);

            if (listaTopJugadores != null && listaTopJugadores.Length >= 2)
            {
                // Mostramos los datos de los mejores jugadores en los textos creados
                textoPosicion1.text = $"1. {listaTopJugadores[0].name} - {listaTopJugadores[0].id * 143} Zombis Muertos";
                textoPosicion2.text = $"2. {listaTopJugadores[1].name} - {listaTopJugadores[1].id * 92} Zombis Muertos";
            }
        }
    }

    // --- ESTA FUNCIÓN HACE TODO EL PASO 2 POR VOS ---
    private void CrearInterfazPorCodigo()
    {
        // 1. Crear el Canvas Principal de Rediseño de UI
        GameObject canvasObj = new GameObject("RankingCanvas_Auto");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        rankingCanvas = canvasObj;

        // 2. Crear un fondo oscuro para el Ranking
        GameObject fondoObj = new GameObject("FondoFoscuro");
        fondoObj.transform.SetParent(canvasObj.transform, false);
        Image imagenFondo = fondoObj.AddComponent<Image>();
        imagenFondo.color = new Color(0, 0, 0, 0.85f); // Negro transparente

        RectTransform rtFondo = fondoObj.GetComponent<RectTransform>();
        rtFondo.anchorMin = new Vector2(0.5f, 0.5f);
        rtFondo.anchorMax = new Vector2(0.5f, 0.5f);
        rtFondo.sizeDelta = new Vector2(500, 400); // Tamaño de la ventana del ranking

        // 3. Crear el Texto de "Cargando..."
        textoCargando = CrearTextoGenerico("TextoCargando", fondoObj.transform, "Presioná H para cargar Ranking", 24, new Vector2(0, 100));

        // 4. Crear el Texto para la Posición 1
        textoPosicion1 = CrearTextoGenerico("TextoPosicion1", fondoObj.transform, "", 28, new Vector2(0, 20));
        textoPosicion1.color = Color.yellow; // Color oro para el primer puesto

        // 5. Crear el Texto para la Posición 2
        textoPosicion2 = CrearTextoGenerico("TextoPosicion2", fondoObj.transform, "", 24, new Vector2(0, -40));

        // Al inicio ocultamos todo el Canvas para esperar la tecla H
        rankingCanvas.SetActive(false);
    }

    // Función auxiliar para ahorrarnos líneas al fabricar textos de TextMeshPro
    private TextMeshProUGUI CrearTextoGenerico(string nombre, Transform padre, string textoInicial, float tamañoLetra, Vector2 posicion)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = textoInicial;
        tmp.fontSize = tamañoLetra;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(450, 50);
        rt.anchoredPosition = posicion;

        return tmp;
    }
}

// --- CLASE CONTENEDORA DE DATOS ---
[System.Serializable]
public class ZombiePlayerData
{
    public int id;
    public string name;
}
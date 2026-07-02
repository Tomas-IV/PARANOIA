using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Newtonsoft.Json;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GestorScoreboard : MonoBehaviourPunCallbacks
{
    public static GestorScoreboard Instancia;

    // --- ENCRIPTACIÓN XOR ANTI-CHEAT ---
    private int claveSecreta = 8352;
    private int zombisMuertosCifrado;

    // --- INTERFAZ ---
    private GameObject panelPrincipal;
    private Text textoMisBajas;
    private Text textoRankingPhoton;
    private Text textoRankingAPI;

    private const string URL_API = "https://jsonplaceholder.typicode.com/users";
    private bool estaVisible = false;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        zombisMuertosCifrado = 0 ^ claveSecreta;

        CrearInterfazUnificadaTopRight();
        ActualizarMiMarcador();

        panelPrincipal.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            estaVisible = !estaVisible;
            panelPrincipal.SetActive(estaVisible);

            if (estaVisible)
            {
                ActualizarRankingReal();
                StartCoroutine(ObtenerHighScoresAPI());
            }
        }
    }

    // --- 1. TUS BAJAS ---
    public void RegistrarBaja()
    {
        int valorReal = zombisMuertosCifrado ^ claveSecreta;
        valorReal++;
        zombisMuertosCifrado = valorReal ^ claveSecreta;

        Hashtable hash = new Hashtable();
        hash.Add("MisKills", valorReal);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        ActualizarMiMarcador();
        if (estaVisible) ActualizarRankingReal();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("MisKills") && estaVisible)
        {
            ActualizarRankingReal();
        }
    }

    private void ActualizarMiMarcador()
    {
        if (textoMisBajas == null) return;

        int misBajasReales = zombisMuertosCifrado ^ claveSecreta;
        string miNombre = PhotonNetwork.LocalPlayer.NickName;
        if (string.IsNullOrEmpty(miNombre)) miNombre = "Jugador Local";

        textoMisBajas.text = $"[TU PUNTAJE]\n{miNombre}: {misBajasReales} bajas";
    }

    // --- 2. EL RANKING REAL (LOS NICKNAMES DE LA SALA) ---
    private void ActualizarRankingReal()
    {
        if (textoRankingPhoton == null) return;

        string ranking = "\n[JUGADORES EN LA SALA]\n";

        var jugadores = PhotonNetwork.PlayerList.OrderByDescending(p =>
        {
            if (p.CustomProperties.ContainsKey("MisKills")) return (int)p.CustomProperties["MisKills"];
            return 0;
        }).ToList();

        foreach (Player p in jugadores)
        {
            int bajas = p.CustomProperties.ContainsKey("MisKills") ? (int)p.CustomProperties["MisKills"] : 0;
            string nombre = string.IsNullOrEmpty(p.NickName) ? "Invitado" : p.NickName;
            ranking += $"- {nombre}: {bajas} bajas\n";
        }

        textoRankingPhoton.text = ranking;
    }

    // --- 3. LA API PARA EL TP ---
    private IEnumerator ObtenerHighScoresAPI()
    {
        textoRankingAPI.text = "\n[RANKING GLOBAL HISTÓRICO - API]\nCargando servidor web...";

        using (UnityWebRequest req = UnityWebRequest.Get(URL_API))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                textoRankingAPI.text = "\n[RANKING GLOBAL]\nError de conexión a la API.";
                yield break;
            }

            string json = req.downloadHandler.text;

            // Si te tira error de que falta ZombiePlayerData, asegurate de tener esa clase 
            // al fondo de alguno de tus scripts (como estaba en el original).
            ZombiePlayerData[] listaTopJugadores = JsonConvert.DeserializeObject<ZombiePlayerData[]>(json);

            if (listaTopJugadores != null && listaTopJugadores.Length > 0)
            {
                string rankingTexto = "\n[RANKING GLOBAL HISTÓRICO - API]\n";

                // Determinamos cuántos mostrar (queremos 4, pero si la API trae menos, nos adaptamos)
                int cantidadAMostrar = Mathf.Min(listaTopJugadores.Length, 4);

                // Multiplicadores falsos variados para simular distintos puntajes altos
                int[] multiplicadores = { 143, 92, 78, 55 };

                // Bucle para crear el top 4 automático
                for (int i = 0; i < cantidadAMostrar; i++)
                {
                    int bajasSimuladas = listaTopJugadores[i].id * multiplicadores[i];
                    rankingTexto += $"{i + 1}. {listaTopJugadores[i].name} - {bajasSimuladas} bajas\n";
                }

                textoRankingAPI.text = rankingTexto;
            }
        }
    }

    // --- CÓDIGO DE INTERFAZ ESTRICTA ARRIBA A LA DERECHA ---
    private void CrearInterfazUnificadaTopRight()
    {
        GameObject canvasObj = new GameObject("ScoreCanvasUnificado");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        panelPrincipal = new GameObject("PanelFondo");
        panelPrincipal.transform.SetParent(canvasObj.transform, false);
        Image imagenFondo = panelPrincipal.AddComponent<Image>();
        imagenFondo.color = new Color(0, 0, 0, 0.90f);

        RectTransform rtFondo = panelPrincipal.GetComponent<RectTransform>();
        rtFondo.anchorMin = new Vector2(1f, 1f);
        rtFondo.anchorMax = new Vector2(1f, 1f);
        rtFondo.pivot = new Vector2(1f, 1f);

        rtFondo.sizeDelta = new Vector2(400, 350);
        rtFondo.anchoredPosition = new Vector2(-20, -20);

        VerticalLayoutGroup layout = panelPrincipal.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.padding = new RectOffset(20, 20, 20, 20);

        Font fuenteDefecto = Resources.GetBuiltinResource<Font>("Arial.ttf");

        textoMisBajas = CrearTextoLayout("TextoMisBajas", panelPrincipal.transform, fuenteDefecto, Color.yellow, 22);
        textoRankingPhoton = CrearTextoLayout("TextoRankingPhoton", panelPrincipal.transform, fuenteDefecto, Color.green, 20);
        textoRankingAPI = CrearTextoLayout("TextoRankingAPI", panelPrincipal.transform, fuenteDefecto, Color.white, 18);
    }

    private Text CrearTextoLayout(string nombre, Transform padre, Font fuente, Color color, int tamaño)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre, false);
        Text t = obj.AddComponent<Text>();
        t.font = fuente;
        t.color = color;
        t.fontSize = tamaño;
        t.alignment = TextAnchor.UpperLeft;

        ContentSizeFitter fitter = obj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return t;
    }
}
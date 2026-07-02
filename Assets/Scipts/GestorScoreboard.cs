using System.Collections;
using System.Collections.Generic;
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

    private List<DatosSegurosAPI> rankingGlobalSeguro = new List<DatosSegurosAPI>();

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

    // --- 1. LÓGICA DE BAJAS SEGURA (XOR LOCAL Y EN RED) ---
    public void RegistrarBaja()
    {
        int valorReal = zombisMuertosCifrado ^ claveSecreta;
        valorReal++;
        zombisMuertosCifrado = valorReal ^ claveSecreta;

        Hashtable hash = new Hashtable();
        hash.Add("MisKills", zombisMuertosCifrado);
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

    // --- 2. RANKING REAL (DESENCRIPTANDO DATOS DE LA RED) ---
    private void ActualizarRankingReal()
    {
        if (textoRankingPhoton == null) return;

        string ranking = "\n[JUGADORES EN LA SALA]\n";

        var jugadores = PhotonNetwork.PlayerList.OrderByDescending(p =>
        {
            if (p.CustomProperties.ContainsKey("MisKills"))
            {
                int valorEncriptadoEnRed = (int)p.CustomProperties["MisKills"];
                return valorEncriptadoEnRed ^ claveSecreta;
            }
            return 0;
        }).ToList();

        foreach (Player p in jugadores)
        {
            int bajasReales = 0;

            if (p.CustomProperties.ContainsKey("MisKills"))
            {
                int valorEncriptadoEnRed = (int)p.CustomProperties["MisKills"];
                bajasReales = valorEncriptadoEnRed ^ claveSecreta;
            }

            string nombre = string.IsNullOrEmpty(p.NickName) ? "Invitado" : p.NickName;
            ranking += $"- {nombre}: {bajasReales} bajas\n";
        }

        textoRankingPhoton.text = ranking;
    }

    // --- 3. RANKING API PROTEGIDO ---
    private IEnumerator ObtenerHighScoresAPI()
    {
        textoRankingAPI.text = "\n[RANKING GLOBAL - API]\nCargando y protegiendo datos...";

        using (UnityWebRequest req = UnityWebRequest.Get(URL_API))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                textoRankingAPI.text = "\n[RANKING GLOBAL]\nError de conexión a la API.";
                yield break;
            }

            ApiZombiePlayerData[] listaTopJugadores = JsonConvert.DeserializeObject<ApiZombiePlayerData[]>(req.downloadHandler.text);

            if (listaTopJugadores != null && listaTopJugadores.Length > 0)
            {
                rankingGlobalSeguro.Clear();
                string rankingTexto = "\n[RANKING GLOBAL - API]\n";
                int cantidad = Mathf.Min(listaTopJugadores.Length, 4);
                int[] multiplicadores = { 143, 92, 78, 55 };

                for (int i = 0; i < cantidad; i++)
                {
                    int bajaReal = listaTopJugadores[i].id * multiplicadores[i];
                    int bajaEncriptada = bajaReal ^ claveSecreta;

                    rankingGlobalSeguro.Add(new DatosSegurosAPI
                    {
                        nombreJugador = listaTopJugadores[i].name,
                        puntajeCifrado = bajaEncriptada
                    });

                    int bajaMostrada = bajaEncriptada ^ claveSecreta;
                    rankingTexto += $"{i + 1}. {listaTopJugadores[i].name} - {bajaMostrada} bajas\n";
                }

                textoRankingAPI.text = rankingTexto;
            }
        }
    }

    // --- INTERFAZ ---
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

// --- CLASES AUXILIARES ---

[System.Serializable]
public class ApiZombiePlayerData
{
    public int id;        // Deben ser public y en minúscula para coincidir con la API y el código
    public string name;   // Deben ser public y en minúscula para coincidir con la API y el código
}

[System.Serializable]
public class DatosSegurosAPI
{
    public string nombreJugador;
    public int puntajeCifrado;
}
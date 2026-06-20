using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameplayChat : MonoBehaviourPun
{
    //  PROPIEDAD ESTÁTICA AGREGADA: Permite a otros scripts (como el LagSimulator) saber si estás escribiendo
    public static bool ChatActivo { get; private set; }

    private GameObject chatCanvasGO;
    private GameObject chatPanelGO;
    private InputField chatInputField;
    private Text chatDisplayData;

    private bool estaEscribiendo = false;

    void Start()
    {
        // Inicializamos la propiedad al arrancar la escena
        ChatActivo = false;

        CrearInterfazDeChat();
    }

    void Update()
    {
        // Al apretar ENTER abrimos o cerramos el chat
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!estaEscribiendo)
            {
                AbrirChat();
            }
            else
            {
                if (!string.IsNullOrEmpty(chatInputField.text))
                {
                    EnviarMensajeDeChat();
                }
                CerrarChat();
            }
        }
    }

    void AbrirChat()
    {
        estaEscribiendo = true;
        ChatActivo = true; //  Cambia a true para que el simulador ignore la L
        chatPanelGO.SetActive(true); // Muestra la caja para escribir
        chatInputField.ActivateInputField(); // Pone el cursor adentro automáticamente
    }

    void CerrarChat()
    {
        estaEscribiendo = false;
        ChatActivo = false; //  Vuelve a false al cerrar, liberando la L
        chatInputField.text = "";
        chatPanelGO.SetActive(false); // Oculta la caja para escribir
    }

    public void EnviarMensajeDeChat()
    {
        string miNombre = PhotonNetwork.NickName;
        if (string.IsNullOrEmpty(miNombre)) miNombre = "Jugador_" + Random.Range(1000, 9999);

        string mensajeFormateado = miNombre + ": " + chatInputField.text;

        // Enviamos el mensaje por red a todos los jugadores
        photonView.RPC("RecibirMensajeGameplay", RpcTarget.All, mensajeFormateado);
    }

    [PunRPC]
    private void RecibirMensajeGameplay(string mensaje)
    {
        if (chatDisplayData != null)
        {
            chatDisplayData.text += mensaje + "\n";
        }
    }

    //  ESTA FUNCIÓN CONSTRUYE TODO EL CANVAS Y LOS COMPONENTES DE TEXTO EN TIEMPO DE EJECUCIÓN
    private void CrearInterfazDeChat()
    {
        // 1. Crear el Canvas Principal
        chatCanvasGO = new GameObject("ChatCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = chatCanvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Asegurar que exista un EventSystem en la escena para que el InputField responda al teclado
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        // ...
        // 2. Crear el Historial de Mensajes (UI Text) - Siempre visible en pantalla
        GameObject textGO = new GameObject("ChatDisplay", typeof(Text));
        textGO.transform.SetParent(chatCanvasGO.transform, false);

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        // Eliminar o comentar la siguiente línea incorrecta:
        // textRect.alignment = TextAnchor.LowerLeft;
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(0f, 0f);
        textRect.pivot = new Vector2(0f, 0f);
        textRect.anchoredPosition = new Vector2(20f, 70f); // Posicionado arriba del input
        textRect.sizeDelta = new Vector2(400f, 200f);

        chatDisplayData = textGO.GetComponent<Text>();
        chatDisplayData.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
        chatDisplayData.fontSize = 16;
        chatDisplayData.color = Color.white;
        chatDisplayData.alignment = TextAnchor.LowerLeft;
        // ...

        // Añadir una sombra negra sutil al texto para que se lea bien en cualquier mapa
        textGO.AddComponent<Shadow>().effectColor = Color.black;


        chatPanelGO = new GameObject("ChatPanel", typeof(Image));
        chatPanelGO.transform.SetParent(chatCanvasGO.transform, false);

        RectTransform panelRect = chatPanelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 0f);
        panelRect.pivot = new Vector2(0f, 0f);
        panelRect.anchoredPosition = new Vector2(20f, 20f);
        panelRect.sizeDelta = new Vector2(350f, 35f);

        chatPanelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f); // Fondo gris transparente

        // 4. Crear el InputField adentro del Panel
        GameObject inputGO = new GameObject("ChatInputField", typeof(InputField), typeof(Image));
        inputGO.transform.SetParent(chatPanelGO.transform, false);

        RectTransform inputRect = inputGO.GetComponent<RectTransform>();
        inputRect.anchoredPosition = Vector2.zero;
        inputRect.sizeDelta = new Vector2(340f, 25f);
        inputGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);

        // Texto que se escribe dentro del InputField
        GameObject textInputGO = new GameObject("Text", typeof(Text));
        textInputGO.transform.SetParent(inputGO.transform, false);
        RectTransform textInputRect = textInputGO.GetComponent<RectTransform>();
        textInputRect.anchoredPosition = Vector2.zero;
        textInputRect.sizeDelta = new Vector2(330f, 20f);

        Text t = textInputGO.GetComponent<Text>();
        t.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        t.fontSize = 14;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleLeft;

        chatInputField = inputGO.GetComponent<InputField>();
        chatInputField.textComponent = t;


        chatPanelGO.SetActive(false);
    }
}
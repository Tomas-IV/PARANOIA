using UnityEngine;
using Photon.Voice.Unity;
using Photon.Pun;

public class PushToTalk : MonoBehaviourPun
{
    [Header("Configuración de Teclado")]
    [SerializeField] private KeyCode hablarKey = KeyCode.V;

    private Recorder voiceRecorder;

    void Start()
    {
        // Buscamos el componente Recorder en la escena (está en el VoiceManager)
        voiceRecorder = FindObjectOfType<Recorder>();

        if (voiceRecorder != null)
        {
            // Aseguramos que el micrófono empiece apagado al entrar al juego
            voiceRecorder.TransmitEnabled = false;
        }
        else
        {
            Debug.LogError("[PushToTalk] No se encontró ningún componente 'Recorder' en la escena.");
        }
    }

    void Update()
    {
        // Si no se encontró el Recorder, no ejecutamos nada para evitar errores
        if (voiceRecorder == null) return;

        // Al presionar la tecla por primera vez
        if (Input.GetKeyDown(hablarKey))
        {
            voiceRecorder.TransmitEnabled = true;
            Debug.Log("Micrófono abierto: Transmitiendo voz...");
        }

        // Al soltar la tecla
        if (Input.GetKeyUp(hablarKey))
        {
            voiceRecorder.TransmitEnabled = false;
            Debug.Log("Micrófono cerrado: Silenciado.");
        }
    }
}
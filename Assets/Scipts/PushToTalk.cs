using UnityEngine;
using Photon.Voice.Unity;

public class PushToTalk : MonoBehaviour
{
    [Header("Configuración de Teclado")]
    [SerializeField] private KeyCode hablarKey = KeyCode.V;

    private Recorder voiceRecorder;

    void Start()
    {
        // En lugar de buscar en toda la escena, buscamos en este MISMO objeto
        voiceRecorder = GetComponent<Recorder>();

        if (voiceRecorder != null)
        {
            voiceRecorder.TransmitEnabled = false;
            Debug.Log("[PushToTalk] ÉXITO: Recorder encontrado y apagado al iniciar.");
        }
        else
        {
            Debug.LogError("[PushToTalk] ERROR: Este script debe estar en el mismo objeto que el componente Recorder.");
        }
    }

    void Update()
    {
        if (voiceRecorder == null) return;

        if (Input.GetKeyDown(hablarKey))
        {
            voiceRecorder.TransmitEnabled = true;
            Debug.Log("[PushToTalk] Transmitiendo...");
        }

        if (Input.GetKeyUp(hablarKey))
        {
            voiceRecorder.TransmitEnabled = false;
            Debug.Log("[PushToTalk] Silenciado.");
        }
    }
}
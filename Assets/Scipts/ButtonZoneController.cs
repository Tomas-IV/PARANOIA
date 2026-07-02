using UnityEngine;
using Photon.Pun;

public class ButtonZoneController : MonoBehaviour
{
    [Header("Conexión con la Puerta")]
    public DoorController puertaObjetivo;
    public int idBoton; // Poné 1 en el primer botón, y 2 en el segundo

    private bool estoyAdentro = false;
    private bool estadoEnviado = false;

    // Cuando cualquier objeto entra físicamente en el trigger del botón
    private void OnTriggerStay2D(Collider2D otro)
    {
        // Buscamos si el objeto que pisó el botón es un jugador
        if (otro.CompareTag("Player") || otro.gameObject.name.Contains("Player"))
        {
            // Buscamos el PhotonView en el objeto, en su padre o en sus hijos (cubre cualquier jerarquía)
            PhotonView pv = otro.GetComponentInParent<PhotonView>();
            if (pv == null) pv = otro.GetComponentInChildren<PhotonView>();

            // Si el personaje que está pisando es EL TUYO (de tu PC local)
            if (pv != null && pv.IsMine)
            {
                estoyAdentro = true;
            }
        }
    }

    // Cuando salís caminando del botón
    private void OnTriggerExit2D(Collider2D otro)
    {
        PhotonView pv = otro.GetComponentInParent<PhotonView>();
        if (pv == null) pv = otro.GetComponentInChildren<PhotonView>();

        if (pv != null && pv.IsMine)
        {
            estoyAdentro = false;
        }
    }

    private void Update()
    {
        // ¿Estás parado adentro del trigger Y manteniendo apretada la Q?
        bool presionandoAhora = (estoyAdentro && Input.GetKey(KeyCode.Q));

        // Solo enviamos señal por red si cambiaste de estado (para no saturar el servidor)
        if (presionandoAhora != estadoEnviado)
        {
            estadoEnviado = presionandoAhora;

            if (puertaObjetivo != null)
            {
                Debug.Log($"[BOTÓN {idBoton}] Enviando estado: {estadoEnviado}");
                puertaObjetivo.EnviarSeñal(idBoton, estadoEnviado);
            }
            else
            {
                Debug.LogError($"[ERROR] Al Botón {idBoton} le falta arrastrar la Puerta Objetivo en el Inspector!");
            }
        }
    }
}
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class DoorController : MonoBehaviourPun, IOnEventCallback
{
    [Header("Segunda Puerta Opcional")]
    [SerializeField] private GameObject otraPuerta;

    private const byte EV_DESVANECER_PUERTA = 88;

    // Control en tiempo real de cada boton
    private bool boton1Activo = false;
    private bool boton2Activo = false;
    private bool yaSeDesvanecio = false;

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // El boton manda true si apretan, o false si sueltan la Q
    public void ActualizarEstadoBoton(int idBoton, bool estaApretado)
    {
        photonView.RPC(nameof(RPC_MasterSincronizarBoton), RpcTarget.MasterClient, idBoton, estaApretado);
    }

    [PunRPC]
    private void RPC_MasterSincronizarBoton(int idBoton, bool estaApretado)
    {
        if (!PhotonNetwork.IsMasterClient || yaSeDesvanecio) return;

        // Actualizamos el estado exacto del boton
        if (idBoton == 1) boton1Activo = estaApretado;
        if (idBoton == 2) boton2Activo = estaApretado;

        // CONDICIÓN ESTRICTA: ¡Ambos tienen que estar presionados al mismo tiempo!
        if (boton1Activo && boton2Activo)
        {
            yaSeDesvanecio = true;
            Debug.Log("[SERVER] ¡AMBOS PRESIONANDO Q A LA VEZ! Desvaneciendo...");

            object[] content = null;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(EV_DESVANECER_PUERTA, content, raiseEventOptions, sendOptions);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == EV_DESVANECER_PUERTA)
        {
            if (otraPuerta != null)
            {
                otraPuerta.SetActive(false);
            }

            gameObject.SetActive(false);
        }
    }
}
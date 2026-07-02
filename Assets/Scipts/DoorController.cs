using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class DoorController : MonoBehaviourPun, IOnEventCallback
{
    private const byte DESVANECER_PUERTA_EVENT_CODE = 42;

    // Guardamos los IDs de los botones activados de manera permanente
    private HashSet<int> botonesActivados = new HashSet<int>();
    private bool yaSeDesvanecio = false;

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void EnviarConfirmacionInput(int idBoton)
    {
        photonView.RPC(nameof(RPC_RegistrarQBoton), RpcTarget.MasterClient, idBoton);
    }

    [PunRPC]
    private void RPC_RegistrarQBoton(int idBoton)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (yaSeDesvanecio) return;

        // Registramos el boton si no estaba en la lista
        if (!botonesActivados.Contains(idBoton))
        {
            botonesActivados.Add(idBoton);
            Debug.Log("Boton " + idBoton + " registrado de manera permanente. Botones listos: " + botonesActivados.Count + "/2");
        }

        // En cuanto se hayan presionado los 2 botones unicos (el 1 y el 2)
        if (botonesActivados.Count >= 2)
        {
            yaSeDesvanecio = true;
            Debug.Log("¡Ambos botones fueron activados! Desvaneciendo puerta mediante RaiseEvent...");
            MandarRaiseEventDesvanecer();
        }
    }

    private void MandarRaiseEventDesvanecer()
    {
        object[] content = null;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(DESVANECER_PUERTA_EVENT_CODE, content, raiseEventOptions, sendOptions);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == DESVANECER_PUERTA_EVENT_CODE)
        {
            gameObject.SetActive(false);
        }
    }
}
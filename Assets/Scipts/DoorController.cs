using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class DoorController : MonoBehaviourPun, IOnEventCallback
{
    [Header("Segunda Puerta Opcional")]
    [SerializeField] private GameObject otraPuerta;

    // Código único para el evento de desvanecer (entre 1 y 199)
    private const byte EV_DESVANECER_PUERTA = 88;

    private HashSet<int> botonesActivados = new HashSet<int>();
    private bool yaSeDesvanecio = false;

    private void OnEnable()
    {
        // Nos registramos para escuchar los eventos puros de Photon
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        // Nos desregistramos al apagarse para evitar errores
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void EnviarConfirmacionInput(int idBoton)
    {
        // CRUCIAL: El cliente solo le avisa al MASTER CLIENT (el server) para evitar duplicados
        photonView.RPC(nameof(RPC_MasterRegistrarBoton), RpcTarget.MasterClient, idBoton);
    }

    [PunRPC]
    private void RPC_MasterRegistrarBoton(int idBoton)
    {
        // SOLO el Master Client ejecuta esto
        if (!PhotonNetwork.IsMasterClient) return;
        if (yaSeDesvanecio) return;

        if (!botonesActivados.Contains(idBoton))
        {
            botonesActivados.Add(idBoton);
            Debug.Log($"[SERVER] Boton {idBoton} registrado de forma segura. Total: {botonesActivados.Count}/2");
        }

        // SOLO si el Master Client confirma que estan los 2 botones unicos (el 1 y el 2)
        if (botonesActivados.Count >= 2)
        {
            yaSeDesvanecio = true;
            Debug.Log("[SERVER] ¡Sincronizacion perfecta lograda! Disparando RaiseEvent global...");

            // Enviamos la orden de desvanecimiento de alta prioridad a todos
            MandarRaiseEventDesvanecer();
        }
    }

    private void MandarRaiseEventDesvanecer()
    {
        object[] content = null; // No necesitamos datos extras, solo el aviso

        // Enviamos a TODO el grupo de receptores de manera confiable (Reliable)
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(EV_DESVANECER_PUERTA, content, raiseEventOptions, sendOptions);
    }

    // ESTO RECIBE EL EVENTO DE BAJO NIVEL EN TODAS LAS COMPUS AL MISMO TIEMPO
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == EV_DESVANECER_PUERTA)
        {
            Debug.Log("[CLIENTE] RaiseEvent recibido. Desvaneciendo puertas en este fotograma.");

            if (otraPuerta != null)
            {
                otraPuerta.SetActive(false);
            }

            gameObject.SetActive(false);
        }
    }
}
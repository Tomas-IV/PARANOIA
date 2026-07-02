using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class DoorController : MonoBehaviourPun, IOnEventCallback
{
    // Definimos un codigo unico para nuestro evento de desvanecer la puerta (entre 1 y 199)
    private const byte DESVANECER_PUERTA_EVENT_CODE = 42;

    private HashSet<int> jugadoresListos = new HashSet<int>();
    private float tiempoPrimerClick;
    private float ventanaTiempo = 2f;
    private bool comenzandoCuenta = false;
    private bool yaSeDesvanecio = false;

    private void OnEnable()
    {
        // Nos registramos para escuchar eventos de Photon
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        // Nos desregistramos al apagarse el objeto para evitar fugas de memoria
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (yaSeDesvanecio) return;

        if (comenzandoCuenta && Time.time > tiempoPrimerClick + ventanaTiempo)
        {
            Debug.Log("Tiempo agotado (pasaron 2 segundos). Coordinen de nuevo.");
            jugadoresListos.Clear();
            comenzandoCuenta = false;
        }
    }

    public void EnviarConfirmacionInput()
    {
        photonView.RPC(nameof(RPC_RegistrarQJugador), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    private void RPC_RegistrarQJugador(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (yaSeDesvanecio) return;

        if (jugadoresListos.Count == 0)
        {
            tiempoPrimerClick = Time.time;
            comenzandoCuenta = true;
            Debug.Log("Primer jugador listo. Tienen 2 segundos...");
        }

        if (!jugadoresListos.Contains(actorNumber))
        {
            jugadoresListos.Add(actorNumber);
            Debug.Log("Jugador " + actorNumber + " listo. Total: " + jugadoresListos.Count + "/" + PhotonNetwork.CurrentRoom.PlayerCount);
        }

        if (jugadoresListos.Count >= PhotonNetwork.CurrentRoom.PlayerCount && PhotonNetwork.CurrentRoom.PlayerCount > 0)
        {
            yaSeDesvanecio = true;

            // LLAMADA AL RAISE EVENT: El Master Client envia la señal pura por red
            MandarRaiseEventDesvanecer();
        }
    }

    private void MandarRaiseEventDesvanecer()
    {
        // No necesitamos mandar datos pesados, solo el aviso, por eso pasamos null
        object[] content = null;

        // Configuramos para que el evento le llegue a TODOS en la sala (incluido el Master Client)
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true }; // True asegura que el paquete llegue si o si

        PhotonNetwork.RaiseEvent(DESVANECER_PUERTA_EVENT_CODE, content, raiseEventOptions, sendOptions);
    }

    // ESTA FUNCIÓN RECIBE EL EVENTO EN TODAS LAS COMPUTADORAS AL MISMO TIEMPO
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == DESVANECER_PUERTA_EVENT_CODE)
        {
            Debug.Log("RaiseEvent recibido con exito. Desvaneciendo puertas en este cliente.");

            // En vez de destruirla de la red, la apagamos localmente. Funciona con cualquier objeto.
            gameObject.SetActive(false);
        }
    }
}
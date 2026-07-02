using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class DoorController : MonoBehaviourPun, IOnEventCallback
{
    private const byte DESVANECER_PUERTA_EVENT_CODE = 42;

    // Guardamos los IDs de los botones activados en lugar de los jugadores
    private HashSet<int> botonesActivados = new HashSet<int>();

    private float tiempoPrimerClick;
    private float ventanaTiempo = 2f;
    private bool comenzandoCuenta = false;
    private bool yaSeDesvanecio = false;

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (yaSeDesvanecio) return;

        if (comenzandoCuenta && Time.time > tiempoPrimerClick + ventanaTiempo)
        {
            Debug.Log("Tiempo agotado. No lograron presionar ambos botones en menos de 2 segundos.");
            botonesActivados.Clear();
            comenzandoCuenta = false;
        }
    }

    public void EnviarConfirmacionInput(int idBoton)
    {
        // Pasamos el id del boton por el RPC
        photonView.RPC(nameof(RPC_RegistrarQBoton), RpcTarget.MasterClient, idBoton);
    }

    [PunRPC]
    private void RPC_RegistrarQBoton(int idBoton)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (yaSeDesvanecio) return;

        // Si es el primer boton que tocan, arranca el contador de 2 segundos
        if (botonesActivados.Count == 0)
        {
            tiempoPrimerClick = Time.time;
            comenzandoCuenta = true;
            Debug.Log("Primer boton presionado. Tienen 2 segundos para activar el otro...");
        }

        if (!botonesActivados.Contains(idBoton))
        {
            botonesActivados.Add(idBoton);
            Debug.Log("Boton " + idBoton + " registrado. Botones listos: " + botonesActivados.Count + "/2");
        }

        // Si ya se presionaron 2 botones diferentes dentro del tiempo límite
        if (botonesActivados.Count >= 2)
        {
            yaSeDesvanecio = true;
            Debug.Log("¡Coordinacion exitosa de a dos! Desvaneciendo puerta...");
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
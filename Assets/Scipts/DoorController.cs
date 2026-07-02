using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    private HashSet<int> jugadoresListos = new HashSet<int>();
    private float tiempoPrimerClick;
    private float ventanaTiempo = 2f; // Los 2 segundos límite
    private bool comenzandoCuenta = false;
    private bool yaSeDestruyo = false;

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (yaSeDestruyo) return;

        // Si ya paso el tiempo limite de 2 segundos y no completaron el objetivo, reiniciamos
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
        if (yaSeDestruyo) return;

        // Si es el primer jugador en tocar la Q, arranca el contador de 2 segundos
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

        // Si ambos jugadores tocaron dentro de la ventana de tiempo
        if (jugadoresListos.Count >= PhotonNetwork.CurrentRoom.PlayerCount && PhotonNetwork.CurrentRoom.PlayerCount > 0)
        {
            yaSeDestruyo = true;
            Debug.Log("Sincronizacion perfecta dentro de los 2 segundos. Desvaneciendo puerta...");

            // El Master Client destruye el objeto de la red para que desaparezca en todas las pantallas
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
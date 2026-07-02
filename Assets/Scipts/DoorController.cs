using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Configuracion de Apertura")]
    [SerializeField] private Vector2 posicionAbiertaOffset = new Vector2(0f, 4f);
    [SerializeField] private float velocidadApertura = 3f;

    private Vector2 posicionInicial;
    private Vector2 posicionObjetivo;
    private bool debeAbrirse = false;

    private HashSet<int> jugadoresListos = new HashSet<int>();

    private void Start()
    {
        posicionInicial = transform.position;
        posicionObjetivo = posicionInicial + posicionAbiertaOffset;
    }

    private void Update()
    {
        if (debeAbrirse)
        {
            transform.position = Vector3.Lerp(transform.position, (Vector3)posicionObjetivo, Time.deltaTime * velocidadApertura);
        }
    }

    // Metodo publico que llama el boton localmente
    public void EnviarConfirmacionInput()
    {
        photonView.RPC(nameof(RPC_RegistrarQJugador), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    private void RPC_RegistrarQJugador(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (debeAbrirse) return;

        if (!jugadoresListos.Contains(actorNumber))
        {
            jugadoresListos.Add(actorNumber);
            Debug.Log("Jugador " + actorNumber + " presiono Q en el boton. Listos: " + jugadoresListos.Count + "/" + PhotonNetwork.CurrentRoom.PlayerCount);
        }

        if (jugadoresListos.Count >= PhotonNetwork.CurrentRoom.PlayerCount && PhotonNetwork.CurrentRoom.PlayerCount > 0)
        {
            photonView.RPC(nameof(RPC_AbrirPuertaGlobal), RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_AbrirPuertaGlobal()
    {
        debeAbrirse = true;
    }
}
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Puerta Abajo")]
    [SerializeField] private GameObject puertaAbajo;

    [Header("Botones")]
    [SerializeField] private int cantidadBotones = 2;

    [Header("Movimiento")]
    [SerializeField] private float casillasAMover = 3f;
    [SerializeField] private float velocidadMovimiento = 2f;

    private Dictionary<int, bool> estadosBotones = new Dictionary<int, bool>();

    private bool abrirPuertas = false;

    private Vector3 posInicialPrincipal;
    private Vector3 posDestinoPrincipal;

    private Vector3 posInicialAbajo;
    private Vector3 posDestinoAbajo;

    private void Start()
    {
        for (int i = 0; i < cantidadBotones; i++)
        {
            estadosBotones.Add(i, false);
        }

        posInicialPrincipal = transform.position;
        posDestinoPrincipal = posInicialPrincipal + Vector3.up * casillasAMover;

        if (puertaAbajo != null)
        {
            posInicialAbajo = puertaAbajo.transform.position;
            posDestinoAbajo = posInicialAbajo + Vector3.down * casillasAMover;
        }
    }

    public void EnviarVoto(int idBoton, bool estado)
    {
        photonView.RPC(nameof(RPC_RecibirVoto), RpcTarget.MasterClient, idBoton, estado);
    }

    [PunRPC]
    private void RPC_RecibirVoto(int idBoton, bool estado)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (abrirPuertas)
            return;

        if (!estadosBotones.ContainsKey(idBoton))
            return;

        estadosBotones[idBoton] = estado;

        foreach (bool activo in estadosBotones.Values)
        {
            if (!activo)
                return;
        }

        photonView.RPC(nameof(RPC_AbrirPuertas), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_AbrirPuertas()
    {
        abrirPuertas = true;
    }

    private void Update()
    {
        if (!abrirPuertas)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            posDestinoPrincipal,
            velocidadMovimiento * Time.deltaTime);

        if (puertaAbajo != null)
        {
            puertaAbajo.transform.position = Vector3.MoveTowards(
                puertaAbajo.transform.position,
                posDestinoAbajo,
                velocidadMovimiento * Time.deltaTime);
        }
    }
}
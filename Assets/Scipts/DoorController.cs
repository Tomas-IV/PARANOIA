using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Puerta Abajo")]
    [SerializeField] private GameObject puertaAbajo;

    [Header("Botones")]
    [SerializeField] private int cantidadBotones = 2;

    private Dictionary<int, bool> estadosBotones = new Dictionary<int, bool>();
    private bool abrirPuertas = false;

    private void Start()
    {
        // Inicializamos los botones en el diccionario
        for (int i = 0; i < cantidadBotones; i++)
        {
            estadosBotones.Add(i, false);
        }
    }

    public void EnviarVoto(int idBoton, bool estado)
    {
        photonView.RPC(nameof(RPC_RecibirVoto), RpcTarget.MasterClient, idBoton, estado);
    }

    [PunRPC]
    private void RPC_RecibirVoto(int idBoton, bool estado)
    {
        // Solo el MasterClient procesa esta lógica
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (abrirPuertas)
            return;

        if (!estadosBotones.ContainsKey(idBoton))
        {
            Debug.LogError($"¡ERROR! Se recibió señal del botón {idBoton}, pero ese ID no existe. Revisá los IDs en el Inspector de los botones.");
            return;
        }

        estadosBotones[idBoton] = estado;
        Debug.Log($"MasterClient: El Botón {idBoton} está presionado: {estado}");

        // Verificamos si todos los botones están activos
        foreach (bool activo in estadosBotones.Values)
        {
            if (!activo)
                return; // Si alguno es falso, cortamos acá y esperamos
        }

        Debug.Log("¡Los dos botones están presionados! Desapareciendo puertas...");
        photonView.RPC(nameof(RPC_AbrirPuertas), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_AbrirPuertas()
    {
        abrirPuertas = true;

        // Apagamos la puerta principal
        gameObject.SetActive(false);

        // Si tenés una puerta asignada abajo, la apagamos también
        if (puertaAbajo != null)
        {
            puertaAbajo.SetActive(false);
        }
    }
}
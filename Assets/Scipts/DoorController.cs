using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Puerta Abajo")]
    [SerializeField] private GameObject puertaAbajo;

    [Header("Botones")]
    [SerializeField] private int cantidadBotones = 2;

    [Header("Configuración del Boss")]
    [Tooltip("El nombre exacto del prefab de tu Boss que pusiste en la carpeta Resources")]
    [SerializeField] private string nombrePrefabBoss = "Boss";
    [SerializeField] private Transform puntoSpawnBoss;

    private Dictionary<int, bool> estadosBotones = new Dictionary<int, bool>();
    private bool abrirPuertas = false;

    private void Start()
    {
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
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (abrirPuertas)
            return;

        if (!estadosBotones.ContainsKey(idBoton))
        {
            return;
        }

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

        // 1. Apagamos las puertas
        gameObject.SetActive(false);

        if (puertaAbajo != null)
        {
            puertaAbajo.SetActive(false);
        }

        // 2. Instanciamos al Boss (Solo el MasterClient lo hace para evitar que aparezcan 2 clones)
        if (PhotonNetwork.IsMasterClient && puntoSpawnBoss != null)
        {
            // PhotonNetwork.Instantiate busca el nombre del prefab dentro de la carpeta Resources
            PhotonNetwork.Instantiate(nombrePrefabBoss, puntoSpawnBoss.position, puntoSpawnBoss.rotation);
        }
    }
}
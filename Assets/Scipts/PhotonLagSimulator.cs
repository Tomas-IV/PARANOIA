using UnityEngine;
using Photon.Pun;

public class PhotonLagSimulator : MonoBehaviour
{
    private bool simuladorActivo = false;

    void Update()
    {
        // El simulador solo funcionará mientras pruebes el juego dentro del Editor de Unity
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleLagSimulation();
        }
#endif
    }

    private void ToggleLagSimulation()
    {
        // 1. Verificamos que el cliente de Photon y su conexión interna existan
        if (PhotonNetwork.NetworkingClient == null || PhotonNetwork.NetworkingClient.LoadBalancingPeer == null)
        {
            Debug.LogWarning("Lag Simulator: No se puede activar porque no estás conectado a Photon todavía.");
            return;
        }

        simuladorActivo = !simuladorActivo;

        // 2. Activamos o desactivamos el switch general de simulación en el Peer de Photon
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.IsSimulationEnabled = simuladorActivo;

        if (simuladorActivo)
        {
            Debug.LogWarning("[LAG SIMULATOR] SIMULACIÓN ACTIVADA. Photon está emulando una conexión con lag por defecto.");
        }
        else
        {
            Debug.Log("[LAG SIMULATOR] SIMULACIÓN DESACTIVADA. Volviendo a red local limpia.");
        }
    }
}
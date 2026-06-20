using UnityEngine;
using Photon.Pun;

public class PhotonLagSimulator : MonoBehaviour
{
    private bool simuladorActivo = false;
    private static PhotonLagSimulator instancia;

    void Awake()
    {
        // Sistema Singleton 
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject); // Hace que sobreviva entre escenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

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
     
        if (PhotonNetwork.NetworkingClient == null || PhotonNetwork.NetworkingClient.LoadBalancingPeer == null)
        {
            Debug.LogWarning("Lag Simulator: No se puede activar porque no estás conectado a Photon todavía.");
            return;
        }

        simuladorActivo = !simuladorActivo;

       
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.IsSimulationEnabled = simuladorActivo;

        if (simuladorActivo)
        {
            Debug.LogWarning("[LAG SIMULATOR] SIMULACIÓN ACTIVADA EN EL MENÚ. El lag afectará el emparejamiento y el gameplay.");
        }
        else
        {
            Debug.Log("[LAG SIMULATOR] SIMULACIÓN DESACTIVADA. Volviendo a red limpia.");
        }
    }
}
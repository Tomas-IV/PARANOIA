using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;

public class PhotonLagSimulator : MonoBehaviour
{
    private bool simuladorActivo = false;
    private static PhotonLagSimulator instancia;
    private PhotonLagSimulationGui ventanaGrafica;

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);

            ventanaGrafica = GetComponent<PhotonLagSimulationGui>();
            if (ventanaGrafica == null)
            {
                ventanaGrafica = gameObject.AddComponent<PhotonLagSimulationGui>();
            }

            if (ventanaGrafica != null)
            {
                ventanaGrafica.enabled = false;
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
#if UNITY_EDITOR

        if (GameplayChat.ChatActivo)
        {
            return;
        }

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
            Debug.LogWarning("Lag Simulator: No conectado a Photon todavía.");
            return;
        }

        simuladorActivo = !simuladorActivo;

        PhotonNetwork.NetworkingClient.LoadBalancingPeer.IsSimulationEnabled = simuladorActivo;

        if (ventanaGrafica != null)
        {
            ventanaGrafica.enabled = simuladorActivo;
        }
    }
}
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

            // Buscamos o agregamos el componente oficial de Photon para la ventana gris
            ventanaGrafica = GetComponent<PhotonLagSimulationGui>();
            if (ventanaGrafica == null)
            {
                ventanaGrafica = gameObject.AddComponent<PhotonLagSimulationGui>();
            }

            // Lo dejamos apagado al inicio usando la propiedad estándar 'enabled' en minúscula
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

        // 1. Activamos el lag interno de Photon
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.IsSimulationEnabled = simuladorActivo;

        // 2. Encendemos o apagamos el cuadro gris usando 'enabled' en minúscula
        if (ventanaGrafica != null)
        {
            ventanaGrafica.enabled = simuladorActivo;
        }
    }
}
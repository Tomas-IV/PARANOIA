//using UnityEngine;
//using Photon.Pun;

//public class NetworkManager : MonoBehaviour
//{
//    void Start()
//    {
//        // SOLO activarlo en el Editor de Unity para hacer pruebas. 
//        // No queremos que los jugadores reales tengan lag simulado en la build final.
//#if UNITY_EDITOR

//        // 1. Activamos la simulación de lag
//        PhotonNetwork.NetworkingClient.LoadBalancingPeer.IsSimulationEnabled = true;

//        // 2. Configuramos los parámetros del simulador
//        var simPeer = PhotonNetwork.NetworkingClient.LoadBalancingPeer.LagSimulationProfile;

//        simPeer.IncomingOutgoingDelay = 150;  // Simula 150ms de Ping (retraso de ida y vuelta)
//        simPeer.IncomingOutgoingJitter = 20;  // Variación del ping (+/- 20ms) para simular una conexión inestable
//        simPeer.IncomingOutgoingLossPercentage = 5; // Simula un 5% de pérdida de paquetes (balas que "no se registran")

//        Debug.LogWarning("¡Photon Lag Simulator ACTIVO! Probando juego con conexión inestable.");

//#endif
//    }
//}
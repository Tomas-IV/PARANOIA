using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Segunda Puerta Opcional")]
    [Tooltip("Se desactiva junto con esta puerta (no necesita PhotonView propio).")]
    [SerializeField] private GameObject otraPuerta;

    [Header("Precision de sincronizacion")]
    [Tooltip("Maxima diferencia de tiempo (en segundos) entre ambas presiones para considerarlas simultaneas.")]
    [SerializeField] private double toleranciaSegundos = 0.5;

    private bool boton1Activo = false;
    private bool boton2Activo = false;
    private double boton1Timestamp = double.NegativeInfinity;
    private double boton2Timestamp = double.NegativeInfinity;
    private bool yaSeDesvanecio = false;

    public void ActualizarEstadoBoton(int idBoton, bool estaApretado)
    {
        if (yaSeDesvanecio) return;

        // PhotonNetwork.Time es un reloj sincronizado entre TODOS los clientes,
        // por eso lo usamos como marca de tiempo precisa en vez de Time.time (que es local a cada PC).
        double tiempoRed = PhotonNetwork.Time;
        photonView.RPC(nameof(RPC_SincronizarBoton), RpcTarget.All, idBoton, estaApretado, tiempoRed);
    }

    [PunRPC]
    private void RPC_SincronizarBoton(int idBoton, bool estaApretado, double timestamp)
    {
        if (yaSeDesvanecio) return;

        if (idBoton == 1)
        {
            boton1Activo = estaApretado;
            boton1Timestamp = estaApretado ? timestamp : double.NegativeInfinity;
        }
        else if (idBoton == 2)
        {
            boton2Activo = estaApretado;
            boton2Timestamp = estaApretado ? timestamp : double.NegativeInfinity;
        }

        VerificarSincronizacion();
    }

    private void VerificarSincronizacion()
    {
        // Los dos tienen que estar apretados AHORA
        if (!boton1Activo || !boton2Activo) return;

        // Y ademas la diferencia entre el instante en que cada uno empezo a apretar
        // tiene que estar dentro de la tolerancia -> esto es lo que hace la sincronizacion "precisa"
        double diferencia = System.Math.Abs(boton1Timestamp - boton2Timestamp);

        if (diferencia <= toleranciaSegundos)
        {
            yaSeDesvanecio = true;
            Debug.Log($"AMBOS BOTONES SINCRONIZADOS (diferencia: {diferencia:F2}s). Desvaneciendo...");

            if (otraPuerta != null)
            {
                otraPuerta.SetActive(false);
            }
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log($"Botones presionados pero NO sincronizados (diferencia: {diferencia:F2}s > {toleranciaSegundos}s). No cuenta.");
        }
    }
}
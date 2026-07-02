using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Asignación de Puerta Gemela")]
    [SerializeField] private GameObject otraPuerta;

    private bool boton1Activo = false;
    private bool boton2Activo = false;
    private bool seDesvanecieron = false;

    public void NotificarEstadoBoton(int idBoton, bool estaPresionado)
    {
        // Sincronizamos la entrada en el búfer global para que impacte al mismo tiempo
        photonView.RPC(nameof(RPC_SincronizarEntradaBoton), RpcTarget.AllBuffered, idBoton, estaPresionado);
    }

    [PunRPC]
    private void RPC_SincronizarEntradaBoton(int idBoton, bool estaPresionado)
    {
        if (seDesvanecieron) return;

        // Guardamos los estados de las llaves en tiempo real
        if (idBoton == 1) boton1Activo = estaPresionado;
        if (idBoton == 2) boton2Activo = estaPresionado;

        // CONDICIÓN DE SINCRONIZACIÓN EXACTA: Las dos tienen que estar apretadas en este frame
        if (boton1Activo && boton2Activo)
        {
            seDesvanecieron = true;
            DesvanecerEstructuras();
        }
    }

    private void DesvanecerEstructuras()
    {
        // Hacemos desaparecer Door (1) mediante la referencia
        if (otraPuerta != null)
        {
            otraPuerta.SetActive(false);
        }

        // Hacemos desaparecer esta puerta (Door)
        gameObject.SetActive(false);
    }
}